import { CommonModule } from '@angular/common';
import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Httpclientservice } from '../../Service/httpclientservice';
import { SignalrService } from '../../Service/SignalrService';

@Component({
  selector: 'app-start-match',
  imports: [FormsModule, CommonModule],
  templateUrl: './start-match.html',
  styleUrl: './start-match.css',
})
export class StartMatch implements OnInit {
  extras = {
    wide: false,
    noBall: false,
    byes: false,
    wicket: false
  };

  wicketTypes = [
    'Bowled',
    'Caught',
    'LBW',
    'Run Out',
    'Stumped',
    'Hit Wicket'
  ];
  showPlayerOutDropdown = false;
  selectedOutBatsman: number | null = null;
  selectedWicketType: string | null = null;
  selectedNewBatsman: number | null = null;
  teamSchedule: any;
  strickerPlayers: any[] = [];
  BowlerStrickerPlayers: any[] = []
  scheduleId!: number;
  matchStep: number = 1; // 1-Setup, 2-Squad Selection, 3-Opening Players, 4-Scoring
  cricketMatch: any;
  // Match setup
  tossWonBy = '';
  optedTo = 'Bat';
  overs = null;
  umpire: string = '';
  venueName: string = '';
  // Players
  striker = '';
  nonStriker = '';
  bowler = '';

  activeTab: string = 'live';
  fullScorecard: any = null;
  squads: any = null; // Used for the Live View tab

  // Squad Selection State
  teamASquad: any[] = [];
  teamBSquad: any[] = [];
  maxPlayingXI = 11;

  constructor(private route: ActivatedRoute, private http: Httpclientservice, private cdr: ChangeDetectorRef, private signalR: SignalrService, private router: Router) {
    this.route.paramMap.subscribe(params => {
      this.scheduleId = Number(params.get('id'));
      this.GetTeamScheduleById();
      this.GetCricketMatchByTeamScheduleId();

    });
  }

  ngOnInit(): void {
  }

  loadBattingAndBowlingTeams() {
    if (!this.teamSchedule || !this.cricketMatch) return;

    const tossWinnerId = Number(this.cricketMatch.TossWinnerTeamID);
    const tossChoice = this.cricketMatch.TossChoice; // "Bat" or "Bowl"
    const currentInnings = this.cricketMatch.CurrentInnings || 1;

    let battingTeamId;
    if (tossChoice === 'Bat') {
      battingTeamId = (currentInnings === 1) ? tossWinnerId : (tossWinnerId === Number(this.teamSchedule.TeamAID) ? this.teamSchedule.TeamBID : this.teamSchedule.TeamAID);
    } else {
      // Toss winner chose to Bowl. So they bowl in 1st innings, bat in 2nd.
      battingTeamId = (currentInnings === 1)
        ? (tossWinnerId === Number(this.teamSchedule.TeamAID) ? this.teamSchedule.TeamBID : this.teamSchedule.TeamAID)
        : tossWinnerId;
    }

    const bowlingTeamId = (battingTeamId === Number(this.teamSchedule.TeamAID)) ? this.teamSchedule.TeamBID : this.teamSchedule.TeamAID;

    this.loadPlayers(battingTeamId, 'striker');
    this.loadPlayers(bowlingTeamId, 'bowler');
  }
  GetTeamScheduleById() {
    this.http.GetData(`TeamSchedule/GetTeamScheduleById?teamScheduleId=${this.scheduleId}`).subscribe((res: any) => {
      this.teamSchedule = res.data;
      this.loadBattingAndBowlingTeams();
      this.cdr.detectChanges();
    });
  }
  GetCricketMatchByTeamScheduleId() {
    this.http.GetData(`CricketMatch/GetByTeamScheduleId?teamScheduleId=${this.scheduleId}`).subscribe((res: any) => {
      if (res.data == null) {
        this.matchStep = 1;
        return;
      } else {
        this.cricketMatch = res.data;
        this.loadBattingAndBowlingTeams();

        // Pre-populate toss setup fields from existing match
        this.tossWonBy = this.cricketMatch.TossWinnerTeamID;
        this.optedTo = this.cricketMatch.TossChoice || 'Bat';
        this.overs = this.cricketMatch.Overs;
        this.umpire = this.cricketMatch.Umpire || '';
        this.venueName = this.cricketMatch.Venue || '';
        
        // Logical flow check
        if (this.cricketMatch.StrikerPlayerID != null) {
          this.matchStep = 4; // Scoring
          this.striker = this.cricketMatch.StrikerPlayerID;
          this.nonStriker = this.cricketMatch.NonStrikerPlayerID;
          this.bowler = this.cricketMatch.BowlerPlayerID;
          this.getLiveScore(this.cricketMatch.CricketMatchID);
        } else {
          // Check if squads are already saved
          this.http.GetData(`LiveMatch/GetSquads?matchId=${this.cricketMatch.CricketMatchID}`).subscribe((res: any) => {
            const sq = res.data;
            const hasSquadA = sq.TeamAPlayers && sq.TeamAPlayers.some((p: any) => p.IsPlaying);
            const hasSquadB = sq.TeamBPlayers && sq.TeamBPlayers.some((p: any) => p.IsPlaying);
            
            if (hasSquadA && hasSquadB) {
              this.matchStep = 3; // Opening Players
              this.squads = sq;
              this.filterPlayingXI(sq);
            } else {
              this.matchStep = 2; // Squad Selection
              this.squads = sq;
              this.teamASquad = sq.TeamAPlayers || [];
              this.teamBSquad = sq.TeamBPlayers || [];
              this.maxPlayingXI = sq.MatchPlayer || 11;
            }
            this.cdr.detectChanges();
          });
        }
        this.cdr.detectChanges();
      }
    });
  }


  filterPlayingXI(sq: any) {
    this.strickerPlayers = sq.TeamAPlayers.filter((p: any) => p.IsPlaying);
    this.BowlerStrickerPlayers = sq.TeamBPlayers.filter((p: any) => p.IsPlaying);
    
    // Reverse if it's 2nd innings or toss choice requires it
    this.loadBattingAndBowlingTeamsForScoring(sq);
  }

  loadBattingAndBowlingTeamsForScoring(sq: any) {
    const tossWinnerId = Number(this.cricketMatch.TossWinnerTeamID);
    const tossChoice = this.cricketMatch.TossChoice;
    const currentInnings = this.cricketMatch.CurrentInnings || 1;
    const teamAId = Number(this.teamSchedule.TeamAID);
    const teamBId = Number(this.teamSchedule.TeamBID);

    let battingTeamId;
    if (tossChoice === 'Bat') {
      battingTeamId = (currentInnings === 1) ? tossWinnerId : (tossWinnerId === teamAId ? teamBId : teamAId);
    } else {
      battingTeamId = (currentInnings === 1) ? (tossWinnerId === teamAId ? teamBId : teamAId) : tossWinnerId;
    }

    if (battingTeamId === teamAId) {
      this.strickerPlayers = sq.TeamAPlayers.filter((p: any) => p.IsPlaying);
      this.BowlerStrickerPlayers = sq.TeamBPlayers.filter((p: any) => p.IsPlaying);
    } else {
      this.strickerPlayers = sq.TeamBPlayers.filter((p: any) => p.IsPlaying);
      this.BowlerStrickerPlayers = sq.TeamAPlayers.filter((p: any) => p.IsPlaying);
    }
  }

  startMatchSetup() {
    if (this.tossWonBy === null || this.tossWonBy === '' || this.tossWonBy === undefined) {
      alert("Please select Toss Won By team.");
      return;
    }
    if (!this.overs || Number(this.overs) <= 0) {
      alert("Please enter a valid number of Overs (must be greater than 0).");
      return;
    }
    if (!this.umpire || !this.venueName) {
      alert("Please fill in Umpire Name and Venue Name.");
      return;
    }

    const payload = {
      TeamScheduleID: this.scheduleId,
      TossWinnerTeamID: this.tossWonBy,
      TossChoice: this.optedTo,
      Overs: Number(this.overs),
      Umpire: this.umpire,
      Venue: this.venueName
    };

    // If match already exists, UPDATE it; otherwise INSERT
    if (this.cricketMatch && this.cricketMatch.CricketMatchID) {
      const updatePayload = { ...payload, CricketMatchID: this.cricketMatch.CricketMatchID };
      this.http.PutData('CricketMatch/Update', updatePayload).subscribe({
        next: (res: any) => {
          this.GetCricketMatchByTeamScheduleId();
        },
        error: (err: any) => {
          alert('Failed to update match: ' + (err?.error?.Message || err?.message || 'Unknown error'));
        }
      });
    } else {
      this.http.PostData('CricketMatch/Insert', payload).subscribe({
        next: (res: any) => {
          this.GetCricketMatchByTeamScheduleId();
        },
        error: (err: any) => {
          alert('Failed to create match: ' + (err?.error?.Message || err?.message || 'Unknown error'));
        }
      });
    }
  }

  togglePlaying(player: any, teamSquad: any[]) {
    // Check if we already have max players
    const currentlyPlaying = teamSquad.filter(p => p.IsPlaying).length;
    if (!player.IsPlaying && currentlyPlaying >= this.maxPlayingXI) {
      alert(`You can only select ${this.maxPlayingXI} players in the Playing XI.`);
      return;
    }
    player.IsPlaying = !player.IsPlaying;
    if (!player.IsPlaying) {
      player.IsCaptain = false;
      player.IsWicketKeeper = false;
    }
  }

  getPlayingCount(teamSquad: any[]): number {
    return teamSquad ? teamSquad.filter(p => p.IsPlaying).length : 0;
  }

  setRole(player: any, teamSquad: any[], role: 'Captain' | 'WicketKeeper') {
    if (!player.IsPlaying) {
      alert("Only playing players can be assigned roles.");
      return;
    }

    if (role === 'Captain') {
      teamSquad.forEach(p => p.IsCaptain = false);
      player.IsCaptain = true;
    } else {
      teamSquad.forEach(p => p.IsWicketKeeper = false);
      player.IsWicketKeeper = true;
    }
  }

  saveSquads() {
    const playA = this.teamASquad.filter(p => p.IsPlaying).length;
    const playB = this.teamBSquad.filter(p => p.IsPlaying).length;

    if (playA < 1 || playB < 1) {
      alert("Please select at least one player in the Playing XI for both teams.");
      return;
    }

    const payloadA = {
      CricketMatchID: this.cricketMatch.CricketMatchID,
      TeamID: this.teamSchedule.TeamAID,
      Players: this.teamASquad
    };

    const payloadB = {
      CricketMatchID: this.cricketMatch.CricketMatchID,
      TeamID: this.teamSchedule.TeamBID,
      Players: this.teamBSquad
    };

    // Save Team A
    this.http.PostData('LiveMatch/SaveSquad', payloadA).subscribe(() => {
      // Save Team B
      this.http.PostData('LiveMatch/SaveSquad', payloadB).subscribe(() => {
        this.GetCricketMatchByTeamScheduleId();
      });
    });
  }

  startInnings() {
    if (!this.striker || !this.nonStriker || !this.bowler) {
      alert("Please select Striker, Non-Striker, and Bowler.");
      return;
    }

    if (this.striker === this.nonStriker) {
      alert("Striker and Non-Striker must be different players.");
      return;
    }

    this.http.PutData('CricketMatch/UpdatePlayersByCricketMatchID', {
      CricketMatchID: this.cricketMatch.CricketMatchID,
      StrikerPlayerID: this.striker,
      NonStrikerPlayerID: this.nonStriker,
      BowlerPlayerID: this.bowler
    }).subscribe((res: any) => {
      this.matchStep = 4;
      this.signalR.StartMatch(this.cricketMatch.CricketMatchID);
      this.cdr.detectChanges();
    });
  }



  loadPlayers(teamId: number, from: string) {
    this.http.GetData(
      `Teams/GetPlayerbyTeamId?id=${teamId}`
    ).subscribe({
      next: (res: any) => {
        if (from === 'striker') {

          this.strickerPlayers = res.data;
        } else {

          this.BowlerStrickerPlayers = res.data;
        }
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Failed to load players', err);
      }
    });
  }

  getNonStriker() {
    var data = this.strickerPlayers.filter(p => p.PlayerID !== Number(this.striker));
    return data;
  }


  onExtraChange(selected: 'wide' | 'noBall' | 'byes' | 'wicket') {

    // Mutual exclusion for Wide, NoBall, Byes
    if (selected !== 'wicket' && this.extras[selected]) {
      if (selected !== 'wide') this.extras.wide = false;
      if (selected !== 'noBall') this.extras.noBall = false;
      if (selected !== 'byes') this.extras.byes = false;
    }

    // Wicket logic
    this.showPlayerOutDropdown = this.extras.wicket;
    if (!this.extras.wicket) {
      this.resetWicketFlow();
    }
  }

  resetWicketFlow() {
    this.selectedOutBatsman = null;
    this.selectedWicketType = null;
    this.selectedNewBatsman = null;
  }

  getNewBatsMan() {
    return this.strickerPlayers.filter(p => {
      const id = Number(p.PlayerID);
      const isCurrentStriker = id === Number(this.striker);
      const isCurrentNonStriker = id === Number(this.nonStriker);
      const isAlreadyOut = (this.currentScore.outPlayerIds || []).includes(id);

      return !isCurrentStriker && !isCurrentNonStriker && !isAlreadyOut;
    });
  }


  // Score state
  currentScore: any = {
    runs: 0,
    wickets: 0,
    overs: '0.0',
    StrikerStats: { Runs: 0, Balls: 0, Fours: 0, Sixes: 0, StrikeRate: 0 },
    NonStrikerStats: { Runs: 0, Balls: 0, Fours: 0, Sixes: 0, StrikeRate: 0 },
    BowlerStats: { Overs: '0.0', Maidens: 0, Runs: 0, Wickets: 0, Economy: 0 },
    outPlayerIds: []
  };

  getLiveScore(matchId: number) {
    this.http.GetData(`LiveMatch/GetLiveScore?matchId=${matchId}`).subscribe((res: any) => {
      if (res && res.data) {
        this.updateCurrentScore(res.data);
      }
    });
  }

  updateCurrentScore(data: any) {
    if (!this.currentScore) this.currentScore = {};

    // Handle both cases (Pascal/Camel)
    this.currentScore.runs = data.TotalRuns ?? data.totalRuns ?? 0;
    this.currentScore.wickets = data.Wickets ?? data.wickets ?? 0;
    this.currentScore.overs = data.Overs ?? data.overs ?? '0.0';
    this.currentScore.target = data.Target ?? data.target ?? null;
    this.currentScore.crr = data.CRR ?? data.crr ?? 0;
    this.currentScore.rrr = data.RRR ?? data.rrr ?? 0;
    this.currentScore.outPlayerIds = data.OutPlayerIds ?? data.outPlayerIds ?? [];

    // Helper map function
    const mapStats = (s: any) => {
      if (!s) return null;
      return {
        Runs: s.Runs ?? s.runs ?? 0,
        Balls: s.Balls ?? s.balls ?? 0,
        Fours: s.Fours ?? s.fours ?? 0,
        Sixes: s.Sixes ?? s.sixes ?? 0,
        StrikeRate: s.StrikeRate ?? s.strikeRate ?? 0,
        Overs: s.Overs ?? s.overs ?? '0.0',
        Maidens: s.Maidens ?? s.maidens ?? 0,
        Wickets: s.Wickets ?? s.wickets ?? 0,
        Economy: s.Economy ?? s.economy ?? 0
      };
    };

    if (data.StrikerStats || data.strikerStats)
      this.currentScore.StrikerStats = mapStats(data.StrikerStats ?? data.strikerStats);

    if (data.NonStrikerStats || data.nonStrikerStats)
      this.currentScore.NonStrikerStats = mapStats(data.NonStrikerStats ?? data.nonStrikerStats);

    if (data.BowlerStats || data.bowlerStats)
      this.currentScore.BowlerStats = mapStats(data.BowlerStats ?? data.bowlerStats);

    if (data.BowlerStats) this.currentScore.BowlerStats = data.BowlerStats;

    // Refresh scorecard if active
    if (this.activeTab === 'scorecard') {
      this.getFullScorecard();
    }

    this.cdr.detectChanges();
  }

  isStriker(playerId: any): boolean {
    return Number(playerId) === Number(this.striker);
  }

  getPlayer(playerId: any) {
    if (!playerId) return null;
    return this.strickerPlayers.find(p => p.PlayerID === Number(playerId));
  }

  getBowler(playerId: any) {
    if (!playerId) return null;
    return this.BowlerStrickerPlayers.find(p => p.PlayerID === Number(playerId));
  }

  // Bowler Selection
  showNewBowlerSelection = false;

  getAvailableBowlers() {
    // Return all bowlers except the current one (to prevent same bowler bowling twice in row)
    return this.BowlerStrickerPlayers.filter(p => p.PlayerID !== Number(this.bowler));
  }

  selectNewBowler(newBowlerId: any) {
    if (!newBowlerId) return;

    const payload = {
      MatchId: this.cricketMatch.CricketMatchID,
      BowlerId: Number(newBowlerId)
    };

    this.http.PostData('LiveMatch/ChangeBowler', payload).subscribe({
      next: (res: any) => {
        this.bowler = newBowlerId;
        this.showNewBowlerSelection = false;
        // Refresh stats for the new bowler (likely 0-0 for new spell)
        this.getLiveScore(this.cricketMatch.CricketMatchID);
        this.cdr.detectChanges();
      },
      error: (err) => alert("Failed to change bowler")
    });
  }

  scoreBall(runs: number) {
    if (this.showNewBowlerSelection) {
      alert("Please select a new bowler for the next over!");
      return;
    }

    if (this.extras.wicket) {
      if (!this.selectedOutBatsman || !this.selectedWicketType || !this.selectedNewBatsman) {
        alert("Please select Player Out, Wicket Type, and New Batsman before scoring!");
        return;
      }
    }

    const isExtra = this.extras.wide || this.extras.noBall;
    const totalRuns = runs + (isExtra ? 1 : 0);

    const payload = {
      CricketMatchID: Number(this.cricketMatch.CricketMatchID),
      StrikerPlayerID: Number(this.striker),
      NonStrikerPlayerID: Number(this.nonStriker),
      BowlerPlayerID: Number(this.bowler),
      Run: totalRuns,
      IsWicket: this.extras.wicket,
      IsBye: this.extras.byes,
      BallType: this.extras.wide ? 'Wide' : (this.extras.noBall ? 'NoBall' : 'Normal'),
      WicketType: this.selectedWicketType,
      PlayerOutID: this.selectedOutBatsman ? Number(this.selectedOutBatsman) : null
    };

    this.http.PostData('LiveMatch/AddBall', payload).subscribe({
      next: (response: any) => {
        const res = response.data;
        console.log('Ball scored', res);

        // Update local score from backend response
        if (res.stats || res.Stats) {
          this.updateCurrentScore(res.stats ?? res.Stats);
        }

        // Handle Innings/Match Over
        if (res.IsInningsOver || res.isInningsOver) {
          const currentInnings = this.cricketMatch.CurrentInnings || 1;
          if (currentInnings === 1) {
            alert("Innings Over! Transitions to 2nd Innings.");
            this.matchStep = 3; // Return to opening players selection (Step 3)
            this.striker = '';
            this.nonStriker = '';
            this.bowler = '';
            this.GetCricketMatchByTeamScheduleId(); // Re-fetch to get new innings/players
          } else {
            alert("Match Finished!");
            this.router.navigate(['/layout/match-summary', this.cricketMatch.CricketMatchID]);
          }
          return;
        }

        // Sync Latest Data via API as requested
        this.getLiveScore(this.cricketMatch.CricketMatchID);

        // Basic frontend update logic (Strike Rotation)
        if (runs % 2 !== 0 && runs !== 4 && runs !== 6) {
          this.swapHikers();
        }

        // End of Over Logic
        if (this.currentScore.overs && this.currentScore.overs.endsWith('.0') && this.currentScore.overs !== '0.0') {
          console.log("End of Over - Swapping Ends");
          this.swapHikers();

          // Show Bowler Selection UI
          this.showNewBowlerSelection = true;
          this.cdr.detectChanges();
        }


        // Handle Wicket
        if (this.extras.wicket && this.selectedNewBatsman) {
          this.updateAfterWicket();
        }

        // Reset extras
        this.extras = { wide: false, noBall: false, byes: false, wicket: false };
        this.resetWicketFlow();
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error(err);
        alert("Error: " + (err.error?.Message || err.message || "Failed to score ball"));
      }
    });
  }

  updateAfterWicket() {
    let newStriker = this.striker;
    let newNonStriker = this.nonStriker;

    // If striker got out, replace striker with new batsman
    if (Number(this.selectedOutBatsman) === Number(this.striker)) {
      newStriker = this.selectedNewBatsman!.toString();
    } else if (Number(this.selectedOutBatsman) === Number(this.nonStriker)) {
      newNonStriker = this.selectedNewBatsman!.toString();
    }

    const payload = {
      MatchId: this.cricketMatch.CricketMatchID,
      StrikerId: Number(newStriker),
      NonStrikerId: Number(newNonStriker),
      BowlerId: Number(this.bowler)
    };

    this.http.PostData('LiveMatch/UpdateMatchPlayers', payload).subscribe({
      next: (res: any) => {
        this.striker = newStriker;
        this.nonStriker = newNonStriker;
        this.selectedNewBatsman = null;
        // Refresh stats
        this.getLiveScore(this.cricketMatch.CricketMatchID);
        this.cdr.detectChanges();
      },
      error: (err) => alert("Failed to update new batsman")
    });
  }

  swapHikers() {
    const temp = this.striker;
    this.striker = this.nonStriker;
    this.nonStriker = temp;
  }

  getBattingTeamName(): string {
    if (!this.cricketMatch || !this.teamSchedule) return 'Batting Team';

    const tossWinnerId = Number(this.cricketMatch.TossWinnerTeamID);
    const tossChoice = this.cricketMatch.TossChoice; // "Bat" or "Bowl"

    let battingTeamId;
    if (tossChoice === 'Bat') {
      battingTeamId = tossWinnerId;
    } else {
      // If winner chose to Bowl, the other team bats
      battingTeamId = (tossWinnerId === Number(this.teamSchedule.TeamAID))
        ? this.teamSchedule.TeamBID
        : this.teamSchedule.TeamAID;
    }

    return battingTeamId === Number(this.teamSchedule.TeamAID)
      ? this.teamSchedule.TeamAName
      : this.teamSchedule.TeamBName;
  }

  getRemainingBalls(): number {
    if (!this.cricketMatch || !this.currentScore || !this.currentScore.overs) return 0;

    const matchOvers = this.cricketMatch.Overs || 20; // Default or from DB
    const totalBalls = matchOvers * 6;

    const overParts = this.currentScore.overs.split('.');
    const completedOvers = parseInt(overParts[0]);
    const ballsInOver = overParts.length > 1 ? parseInt(overParts[1]) : 0;
    const ballsBowled = (completedOvers * 6) + ballsInOver;

    return Math.max(0, totalBalls - ballsBowled);
  }

  switchTab(tab: string) {
    this.activeTab = tab;
    if (tab === 'scorecard' && this.cricketMatch?.CricketMatchID) {
      this.getFullScorecard();
    } else if (tab === 'squads' && this.cricketMatch?.CricketMatchID) {
      this.getSquads();
    }
    this.cdr.detectChanges();
  }

  getFullScorecard() {
    this.http.GetData(`LiveMatch/GetFullScorecard?matchId=${this.cricketMatch.CricketMatchID}`).subscribe((res: any) => {
      this.fullScorecard = res.data;
      this.cdr.detectChanges();
    });
  }

  getSquads() {
    this.http.GetData(`LiveMatch/GetSquads?matchId=${this.cricketMatch.CricketMatchID}`).subscribe((res: any) => {
      this.squads = res.data;
      this.cdr.detectChanges();
    });
  }

  undoLastBall() {
    if (!confirm("Are you sure you want to undo the last ball?")) return;

    this.http.PostData('LiveMatch/UndoLastBall', this.cricketMatch.CricketMatchID).subscribe({
      next: (res: any) => {
        // Re-fetch match data to sync players and steps
        this.GetCricketMatchByTeamScheduleId();
        // Refresh live score
        if (this.cricketMatch?.CricketMatchID) {
          this.getLiveScore(this.cricketMatch.CricketMatchID);
        }
        
        // Reset UI states
        this.extras = { wide: false, noBall: false, byes: false, wicket: false };
        this.showNewBowlerSelection = false;
        this.showPlayerOutDropdown = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        alert(err.error?.Message || "Failed to undo ball.");
      }
    });
  }
}

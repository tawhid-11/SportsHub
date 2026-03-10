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
  matchStep: number = 1; // 1-Setup, 2-Players, 3-Scoring
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
  squads: any = null;

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
        return;
      } else {
        this.cricketMatch = res.data;
        this.loadBattingAndBowlingTeams();
        if (this.cricketMatch.CricketMatchID != null && this.cricketMatch.StrikerPlayerID == null) {
          this.matchStep = 2;
          this.cdr.detectChanges();
          return;
        } else if (this.cricketMatch.CricketMatchID != null && this.cricketMatch.StrikerPlayerID != null) {
          this.matchStep = 3;
          // Pre-fill selection if match is already live
          this.striker = this.cricketMatch.StrikerPlayerID;
          this.nonStriker = this.cricketMatch.NonStrikerPlayerID;
          this.bowler = this.cricketMatch.BowlerPlayerID;

          // Fetch current score
          if (this.cricketMatch.CricketMatchID) {
            this.getLiveScore(this.cricketMatch.CricketMatchID);
          }

          this.cdr.detectChanges();
          return;
        }

      }

    });
  }

  startMatchSetup() {
    if (!this.tossWonBy || !this.overs || this.overs <= 0 || !this.umpire || !this.venueName) {
      alert("Please fill all fields correctly. Overs must be greater than 0.");
      return;
    }

    this.http.PostData('CricketMatch/Insert', {
      TeamScheduleID: this.scheduleId,
      TossWinnerTeamID: this.tossWonBy,
      TossChoice: this.optedTo,
      Overs: this.overs,
      Umpire: this.umpire,
      Venue: this.venueName
    }).subscribe((res: any) => {
      this.matchStep = 2;
      this.loadPlayers(this.teamSchedule.TeamAID, 'striker');
      this.loadPlayers(this.teamSchedule.TeamBID, 'bowler');
      this.GetCricketMatchByTeamScheduleId();
      this.cdr.detectChanges();
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
      this.matchStep = 3;
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
      if (res) {
        this.updateCurrentScore(res);
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
      next: (res: any) => {
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
            this.matchStep = 2; // Return to player selection step
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
      this.fullScorecard = res;
      this.cdr.detectChanges();
    });
  }

  getSquads() {
    this.http.GetData(`LiveMatch/GetSquads?matchId=${this.cricketMatch.CricketMatchID}`).subscribe((res: any) => {
      this.squads = res;
      this.cdr.detectChanges();
    });
  }
}

import { CommonModule } from '@angular/common';
import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { SignalrService } from '../../Service/SignalrService';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Httpclientservice } from '../../Service/httpclientservice';

@Component({
  selector: 'app-user-live-score',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './user-live-score.html',
  styleUrl: './user-live-score.css',
})
export class UserLiveScore implements OnInit {
  matchId: number = 0;
  matchTitle: string = 'Live Match';
  activeTab: string = 'live'; // info, live, scorecard, squad, overs

  matchInfo: any = null;
  oversDetails: any[] = [];

  score: any = {
    runs: 0,
    wickets: 0,
    overs: '0.0',
    crr: 0,
    rrr: 0,
    target: null,
    currentInnings: 1,
    team: 'Live Score',
    matchStatus: 'Live',
    winnerMessage: null
  };

  batting: any[] = [];
  bowling: any[] = [];
  recentBalls: any[] = [];

  fullScorecard: any = null;
  squads: any = null;

  constructor(
    private signalR: SignalrService,
    private http: Httpclientservice,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit() {
    this.matchId = Number(this.route.snapshot.paramMap.get('id'));

    // 1. Fetch Initial Score
    this.getInitialScore();

    // 2. Connect SignalR
    this.signalR.startConnection();
    this.signalR.liveMatch$.subscribe((data: any) => {
      if (data && (data.CricketMatchID === this.matchId || data.cricketMatchID === this.matchId)) {
        this.updateUI(data);
        // If scorecard is active, refresh it too
        if (this.activeTab === 'scorecard') {
          this.getFullScorecard();
        }
      }
    });
  }

  getInitialScore() {
    this.http.GetData(`LiveMatch/GetLiveScore?matchId=${this.matchId}`).subscribe((res: any) => {
      if (res && res.data) {
        this.updateUI(res.data);
      }
    });
  }

  updateUI(data: any) {
    if (!data) return;

    this.score.runs = data.TotalRuns ?? data.totalRuns ?? 0;
    this.score.wickets = data.Wickets ?? data.wickets ?? 0;
    this.score.overs = data.Overs ?? data.overs ?? '0.0';
    this.score.target = data.Target ?? data.target ?? null;
    this.score.crr = data.CRR ?? data.crr ?? 0;
    this.score.rrr = data.RRR ?? data.rrr ?? 0;
    this.score.currentInnings = data.CurrentInnings ?? data.currentInnings ?? 1;
    this.score.matchStatus = data.MatchStatus ?? data.matchStatus ?? 'Live';
    this.score.winnerMessage = data.WinnerMessage ?? data.winnerMessage ?? null;
    this.recentBalls = data.RecentBalls ?? data.recentBalls ?? [];

    // Update Team Names and Title
    const teamA = data.TeamAName ?? data.teamAName ?? 'Team A';
    const teamB = data.TeamBName ?? data.teamBName ?? 'Team B';
    this.matchTitle = `${teamA} vs ${teamB}`;
    this.score.team = data.BattingTeamName ?? data.battingTeamName ?? 'Batting Team';

    // Map Players
    this.batting = [];
    const s1 = data.StrikerStats ?? data.strikerStats;
    const s2 = data.NonStrikerStats ?? data.nonStrikerStats;

    if (s1) this.addBatsman(s1, true);
    if (s2) this.addBatsman(s2, false);

    this.bowling = [];
    const b = data.BowlerStats ?? data.bowlerStats;
    if (b) {
      this.bowling.push({
        PlayerID: b.PlayerID || b.playerId || b.id,
        name: b.PlayerName || b.playerName || 'Bowler',
        image: b.PlayerImage || b.playerImage || '',
        overs: b.Overs ?? b.overs ?? '0.0',
        maidens: b.Maidens ?? b.maidens ?? 0,
        runs: b.Runs ?? b.runs ?? 0,
        wickets: b.Wickets ?? b.wickets ?? 0,
        economy: b.Economy ?? b.economy ?? 0
      });
    }

    this.cdr.detectChanges();
  }

  addBatsman(s: any, isStriker: boolean) {
    this.batting.push({
      PlayerID: s.PlayerID || s.playerId || s.id,
      name: s.PlayerName || s.playerName || 'Batsman',
      image: s.PlayerImage || s.playerImage || '',
      runs: s.Runs ?? s.runs ?? 0,
      balls: s.Balls ?? s.balls ?? 0,
      fours: s.Fours ?? s.fours ?? 0,
      sixes: s.Sixes ?? s.sixes ?? 0,
      strikeRate: s.StrikeRate ?? s.strikeRate ?? 0,
      isStriker: isStriker
    });
  }

  switchTab(tab: string) {
    this.activeTab = tab;
    if (tab === 'scorecard') {
      this.getFullScorecard();
    } else if (tab === 'squad') {
      this.getSquads();
    } else if (tab === 'info') {
      this.getMatchInfo();
    } else if (tab === 'overs') {
      this.getOversDetails();
    }
    this.cdr.detectChanges();
  }

  getMatchInfo() {
    this.http.GetData(`LiveMatch/GetMatchInfo?matchId=${this.matchId}`).subscribe((res: any) => {
      this.matchInfo = res.data;
      this.cdr.detectChanges();
    });
  }

  getOversDetails() {
    this.http.GetData(`LiveMatch/GetOversDetails?matchId=${this.matchId}`).subscribe((res: any) => {
      this.oversDetails = res.data;
      this.cdr.detectChanges();
    });
  }

  getFullScorecard() {
    this.http.GetData(`LiveMatch/GetFullScorecard?matchId=${this.matchId}`).subscribe((res: any) => {
      this.fullScorecard = res.data;
      this.cdr.detectChanges();
    });
  }

  getSquads() {
    this.http.GetData(`LiveMatch/GetSquads?matchId=${this.matchId}`).subscribe((res: any) => {
      this.squads = res.data;
      this.cdr.detectChanges();
    });
  }

  getBallClass(ball: string): string {
    if (ball === 'W') return 'ball-wicket';
    if (ball === '4') return 'ball-four';
    if (ball === '6') return 'ball-six';
    if (ball.includes('wd') || ball.includes('nb') || ball.includes('lb')) return 'ball-extra';
    return '';
  }
}

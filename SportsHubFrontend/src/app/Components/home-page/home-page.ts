import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { SignalrService } from '../../Service/SignalrService';
import { Httpclientservice } from '../../Service/httpclientservice';

import { ContactUsComponent } from '../contact-us/contact-us';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, RouterOutlet, ContactUsComponent],
  templateUrl: './home-page.html',
  styleUrls: ['./home-page.css']
})
export class HomePage implements OnInit {
  year: number = new Date().getFullYear();

  runningMatches: any[] = [];
  gallery: any[] = [];
  isMenuOpen: boolean = false;
  showContactForm: boolean = false;
  activeSupportTab: string = '';

  constructor(private router: Router, private signalRService: SignalrService, private cdr: ChangeDetectorRef, private http: Httpclientservice) {
    this.signalRService.startConnection();
  }

  showSupportInfo(tab: string) {
    this.activeSupportTab = tab;
    // If opening a support tab (rules, terms, privacy), hide the contact form to keep view clean
    if (tab !== 'help' && tab !== '') {
      this.showContactForm = false;
    }

    if (tab === 'help') {
      this.showContact();
    } else {
      this.cdr.detectChanges();
      setTimeout(() => {
        const element = document.getElementById('support-info');
        if (element) {
          element.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
      }, 100);
    }
  }

  showContact() {
    this.showContactForm = true;
    this.activeSupportTab = ''; // Hide other support info when showing contact
    this.cdr.detectChanges();

    setTimeout(() => {
      const contactSection = document.getElementById('contact-area');
      if (contactSection) {
        contactSection.scrollIntoView({ behavior: 'smooth', block: 'start' });
      }
    }, 100);
  }

  scrollToContact() {
    this.showContact();
  }

  toggleMobileMenu() {
    this.isMenuOpen = !this.isMenuOpen;
  }

  ngOnInit(): void {
    this.loadLiveMatches();
    this.loadTournaments();
    this.signalRService.liveMatch$.subscribe(data => {
      if (!data) return;

      const matchId = data.CricketMatchID ?? data.cricketMatchID;
      const status = data.MatchStatus ?? data.matchStatus;

      if (status === 'Finished') {
        var checkFinished = this.runningMatches.find(m => (m.CricketMatchID === matchId || m.cricketMatchID === matchId));
        if (checkFinished) {
          checkFinished.matchStatus = 'Finished';
          checkFinished.winnerMessage = data.WinnerMessage ?? data.winnerMessage;
          checkFinished.totalRun = data.TotalRuns ?? data.totalRuns;
          checkFinished.wicket = data.Wickets ?? data.wickets;
          checkFinished.overs = data.Overs ?? data.overs;
          this.cdr.detectChanges();
        }
        return;
      }

      const totalRuns = data.TotalRuns ?? data.totalRuns ?? data.totalRun;
      const wickets = data.Wickets ?? data.wickets ?? data.wicket;
      const overs = data.Overs ?? data.overs;

      var checkExist = this.runningMatches.find(m => m.CricketMatchID === matchId || m.cricketMatchID === matchId);
      if (checkExist) {
        checkExist.totalRun = totalRuns;
        checkExist.wicket = wickets;
        checkExist.overs = overs;
        checkExist.matchStatus = status;
        checkExist.currentInnings = data.CurrentInnings ?? data.currentInnings;
        checkExist.innings1TotalRuns = data.Innings1TotalRuns ?? data.innings1TotalRuns;
        checkExist.innings1TotalWickets = data.Innings1TotalWickets ?? data.innings1TotalWickets;
        checkExist.innings1TeamName = data.Innings1TeamName ?? data.innings1TeamName;
        checkExist.innings2TeamName = data.Innings2TeamName ?? data.innings2TeamName;
        checkExist.battingTeamName = data.BattingTeamName ?? data.battingTeamName;
        checkExist.target = data.Target ?? data.target;
        checkExist.crr = data.CRR ?? data.crr;
        checkExist.rrr = data.RRR ?? data.rrr;
        checkExist.matchOvers = data.MatchOvers ?? data.matchOvers ?? 20;
        checkExist.strikerStats = data.StrikerStats ?? data.strikerStats;
        checkExist.nonStrikerStats = data.NonStrikerStats ?? data.nonStrikerStats;
        checkExist.tossWinnerName = data.TossWinnerName ?? data.tossWinnerName;
        checkExist.tossChoice = data.TossChoice ?? data.tossChoice;
      } else if (data.TeamAName || data.teamAName) {
        this.runningMatches.push({
          cricketMatchID: matchId,
          teamAName: data.TeamAName ?? data.teamAName,
          teamBName: data.TeamBName ?? data.teamBName,
          teamALogo: data.TeamALogo ?? data.teamALogo,
          teamBLogo: data.TeamBLogo ?? data.teamBLogo,
          totalRun: totalRuns,
          wicket: wickets,
          overs: overs,
          matchStatus: status,
          currentInnings: data.CurrentInnings ?? data.currentInnings,
          innings1TotalRuns: data.Innings1TotalRuns ?? data.innings1TotalRuns,
          innings1TotalWickets: data.Innings1TotalWickets ?? data.innings1TotalWickets,
          innings1TeamName: data.Innings1TeamName ?? data.innings1TeamName,
          innings2TeamName: data.Innings2TeamName ?? data.innings2TeamName,
          battingTeamName: data.BattingTeamName ?? data.battingTeamName,
          target: data.Target ?? data.target,
          crr: data.CRR ?? data.crr,
          rrr: data.RRR ?? data.rrr,
          matchOvers: data.MatchOvers ?? data.matchOvers ?? 20,
          strikerStats: data.StrikerStats ?? data.strikerStats,
          nonStrikerStats: data.NonStrikerStats ?? data.nonStrikerStats,
          tossWinnerName: data.TossWinnerName ?? data.tossWinnerName,
          tossChoice: data.TossChoice ?? data.tossChoice
        });
      }

      this.cdr.detectChanges();
    });
  }

  /* ================= LOAD DATA (STATIC NOW → API LATER) ================= */

  loadLiveMatches() {
    this.http.GetData('CricketMatch/GetAllLiveMatch').subscribe((data: any) => {
      this.runningMatches = data.data.map((m: any) => ({
        ...m,
        cricketMatchID: m.CricketMatchID ?? m.cricketMatchID,
        matchStatus: m.MatchStatus ?? m.matchStatus,
        totalRun: m.TotalRuns ?? m.totalRuns ?? m.totalRun,
        wicket: m.Wickets ?? m.wickets ?? m.wicket,
        overs: m.Overs ?? m.overs,
        currentInnings: m.CurrentInnings ?? m.currentInnings,
        innings1TotalRuns: m.Innings1TotalRuns ?? m.innings1TotalRuns,
        innings1TotalWickets: m.Innings1TotalWickets ?? m.innings1TotalWickets,
        innings1TeamName: m.Innings1TeamName ?? m.innings1TeamName,
        innings2TeamName: m.Innings2TeamName ?? m.innings2TeamName,
        battingTeamName: m.BattingTeamName ?? m.battingTeamName,
        target: m.Target ?? m.target,
        crr: m.CRR ?? m.crr,
        rrr: m.RRR ?? m.rrr,
        matchOvers: m.MatchOvers ?? m.matchOvers ?? 20,
        strikerStats: m.StrikerStats ?? m.strikerStats,
        nonStrikerStats: m.NonStrikerStats ?? m.nonStrikerStats,
        tossWinnerName: m.TossWinnerName ?? m.tossWinnerName,
        tossChoice: m.TossChoice ?? m.tossChoice
      }));
      this.cdr.detectChanges();
    });
  }

  loadTournaments() {
    this.http.GetData('Tournaments').subscribe((res: any) => {
      if (res.success) {
        // Map backend data to gallery format
        this.gallery = res.data.map((t: any) => ({
          id: t.tournamentID ?? t.tournamentId ?? t.TournamentID,
          title: t.tournamentName ?? t.TournamentName ?? t.title,
          image: t.image ?? t.Image ?? 'assets/image/hero-cricket.jpg',
          status: t.status ?? t.Status,
          location: t.location ?? t.Location
        })).filter((t: any) => t.status !== 'Draft');
      }
      this.cdr.detectChanges();
    });
  }

  /* ================= BUTTON ACTIONS ================= */

  goToLiveMatches() {
    this.router.navigate(['/live-matches']);
  }

  goToTournaments() {
    this.router.navigate(['all-tournaments']);
  }

  viewLiveScore(match: any) {
    if (match.matchStatus === 'Finished') {
      this.router.navigate(['/match-summary', match.cricketMatchID]);
    } else {
      this.router.navigate(['/view-live-score', match.cricketMatchID]);
    }
  }

  viewTournament(tournament: any) {
    this.router.navigate(['/all-tournaments']);
  }

  navigateToSchedule(tournamentId: number) {
    this.router.navigate(['/tournament-schedule', tournamentId]);
  }

  navigateToPointsTable(tournamentId: number) {
    this.router.navigate(['/tournament-points', tournamentId]);
  }

  getRemainingBalls(currentOvers: string, match: any): number {
    if (!currentOvers) return 0;

    // Get actual match overs (default to 20 if not available)
    const matchOvers = match.matchOvers || 20;
    const totalBalls = matchOvers * 6;

    // Parse current overs (e.g., "0.4" means 0 overs and 4 balls = 4 balls)
    const parts = currentOvers.split('.');
    if (parts.length === 2) {
      const overs = parseInt(parts[0]) || 0;
      const balls = parseInt(parts[1]) || 0;
      const ballsPlayed = (overs * 6) + balls;
      return Math.max(0, totalBalls - ballsPlayed);
    }

    return totalBalls;
  }

  viewSchedule(tournament: any) {
    this.router.navigate(['/tournament-schedule', tournament.id]);
  }

  viewPointsTable(tournament: any) {
    this.router.navigate(['/tournament-points', tournament.id]);
  }

  getCurrentUrl(): boolean {
    var currentUrl = this.router.url;
    if (currentUrl === '/') {
      return true;
    }
    return false;
  }
}
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Httpclientservice } from '../../Service/httpclientservice';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-schedule',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './schedule.html',
  styleUrl: './schedule.css',
})
export class Schedule implements OnInit {
  teamSchedules: any[] = [];
  groupedSchedules: { [key: string]: any[] } = {};
  from: string = '';
  tournamentId!: number;
  tournamentName: string = '';
  loading: boolean = true;

  constructor(private http: Httpclientservice, private cdr: ChangeDetectorRef, private route: ActivatedRoute, private router: Router) {
    this.route.paramMap.subscribe(params => {
      this.tournamentId = Number(params.get('id'));
      this.from = this.route.snapshot.queryParamMap.get('from') || '';
      this.getTeamSchedules();
      this.getTournamentDetails();
    });
  }

  ngOnInit() { }

  getTeamSchedules() {
    this.loading = true;
    this.http.GetData(`TeamSchedule?tournamentId=${this.tournamentId}`).subscribe((res: any) => {
      if (res.success) {
        this.teamSchedules = res.data;
        this.groupSchedules();
      }
      this.loading = false;
      this.cdr.detectChanges();
    });
  }

  getTournamentDetails() {
    this.http.GetData(`Tournaments/GetTournamentsById?TournamentId=${this.tournamentId}`).subscribe((res: any) => {
      if (res.success) {
        this.tournamentName = res.data.tournamentName || res.data.TournamentName;
        this.cdr.detectChanges();
      }
    });
  }

  groupSchedules() {
    const groups: { [key: string]: any[] } = {};

    this.teamSchedules.forEach(match => {
      const phase = match.Phase || match.phase || 'General';
      if (!groups[phase]) groups[phase] = [];
      groups[phase].push(match);
    });

    this.groupedSchedules = groups;
  }

  getPhaseKeys() {
    const phases = Object.keys(this.groupedSchedules);
    const phasesRank: { [key: string]: number } = {
      'Group Stage': 1,
      'Round Robin': 1,
      'Quarter-Final': 2,
      'Semi-Final': 3,
      'Final': 4,
      'General': 5
    };
    return phases.sort((a, b) => (phasesRank[a] || 99) - (phasesRank[b] || 99));
  }

  viewMatchDetails(teamScheduleId: number, teamAName: string, teamBName: string) {
    if (this.from === 'admin') {
      this.router.navigate(['/layout/matchdetails', teamScheduleId], { queryParams: { teamAName: teamAName, teamBName: teamBName } });
      return;
    } else if (this.from === 'teamowner') {
      this.router.navigate(['/teamownerlayout/matchdetails', teamScheduleId], { queryParams: { teamAName: teamAName, teamBName: teamBName } });
    } else {
      // Public view usually doesn't have details but if it does:
      this.router.navigate(['/matchdetails', teamScheduleId]);
    }
  }
}
import { Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { Httpclientservice } from '../../Service/httpclientservice';
import { ActivatedRoute, RouterLink } from '@angular/router';

@Component({
  selector: 'app-tournament-details',
  standalone: true,
  imports: [CommonModule, DatePipe, RouterLink],
  templateUrl: './tournament-details.html',
  styleUrl: './tournament-details.css',
})
export class TournamentDetails implements OnInit {
  tournamentId!: number;
  tournamentInfo: any = null;
  teams: any[] = [];
  schedules: any[] = [];
  pointsTable: any[] = [];
  topBatters: any[] = [];
  topBowlers: any[] = [];
  loading = true;
  activeTab = 'overview';

  constructor(
    private http: Httpclientservice,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.tournamentId = Number(params.get('id'));
      
      this.route.queryParams.subscribe(qParams => {
        if (qParams['tab']) {
          this.activeTab = qParams['tab'];
        }
      });

      if (this.tournamentId) {
        this.loadAllTournamentData();
      } else {
        this.loading = false;
      }
    });
  }

  loadAllTournamentData() {
    this.loading = true;
    
    // 1. Load Tournament Info
    this.http.GetData(`Tournaments`).subscribe((res: any) => {
      if (res.success) {
        this.tournamentInfo = res.data.find((t: any) => 
          (t.tournamentID || t.tournamentId || t.TournamentID) === this.tournamentId
        );
      }
    });

    // 2. Load Teams
    this.http.GetData(`Teams/GetTeamIdbyTournamentId?id=${this.tournamentId}`).subscribe((res: any) => {
      if (res.success) {
        this.teams = res.data;
        // Load squads for each team
        this.teams.forEach(team => {
          this.http.GetData(`Teams/GetPlayerbyTeamId?id=${team.TeamsID || team.TeamId}`).subscribe((pRes: any) => {
            if (pRes.success) {
              team.squad = pRes.data;
            }
          });
        });
      }
    });

    // 3. Load Schedule
    this.http.GetData(`TeamSchedule?tournamentId=${this.tournamentId}`).subscribe((res: any) => {
      if (res.success) {
        this.schedules = res.data;
      }
    });

    this.http.GetData(`TournamentPoints/GetPointsTable?tournamentId=${this.tournamentId}`).subscribe((res: any) => {
      if (res.success) {
        this.pointsTable = res.data;
      }
    });

    // 5. Load Top Performers (Stats)
    this.http.GetData(`Player/GetTournamentPerformers?tournamentId=${this.tournamentId}`).subscribe((res: any) => {
      if (res.success) {
        this.topBatters = res.data.topBatters || [];
        this.topBowlers = res.data.topBowlers || [];
      }
      this.loading = false;
    });
  }

  switchTab(tab: string) {
    this.activeTab = tab;
  }
}

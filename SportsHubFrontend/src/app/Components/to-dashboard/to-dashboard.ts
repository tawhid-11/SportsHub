import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Httpclientservice } from '../../Service/httpclientservice';

@Component({
  selector: 'app-to-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './to-dashboard.html',
  styleUrl: './to-dashboard.css'
})
export class ToDashboard implements OnInit {
  teamInfo: any = null;
  playersCount: number = 0;
  recentMatches: any[] = [];
  loading = true;

  constructor(private http: Httpclientservice) {}

  ngOnInit(): void {
    const user = JSON.parse(localStorage.getItem('userInfo') || '{}');
    const userId = user.ID || user.UserID || user.Id || user.id;
    
    if (userId) {
      this.loadTeamData(userId);
    } else {
      this.loading = false;
    }
  }

  loadTeamData(userId: number): void {
    this.http.GetData(`Teams/GetTeamIdbyUserId?id=${userId}`).subscribe({
      next: (res: any) => {
        if (res && res.success && res.data) {
          this.teamInfo = res.data;
          this.loadPlayersCount(userId);
          this.loadRecentPerformance(this.teamInfo.TeamsID);
        } else {
          this.loading = false;
        }
      },
      error: () => this.loading = false
    });
  }

  loadPlayersCount(userId: number) {
    this.http.GetData(`Player/GetPlayerByTeamOwnerId?id=${userId}`).subscribe({
      next: (res: any) => {
        if (res && res.success && res.data) {
          this.playersCount = res.data.length;
        }
      },
      error: () => {}
    });
  }

  loadRecentPerformance(teamId: number) {
    this.http.GetData(`Teams/GetTeamRecentPerformance?teamId=${teamId}`).subscribe({
      next: (res: any) => {
        if (res && res.success && res.data) {
          this.recentMatches = res.data;
        }
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }
}

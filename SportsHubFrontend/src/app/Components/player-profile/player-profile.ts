import { Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { Httpclientservice } from '../../Service/httpclientservice';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-player-profile',
  imports: [CommonModule, DatePipe, RouterLink],
  templateUrl: './player-profile.html',
  styleUrl: './player-profile.css',
})
export class playerProfile implements OnInit {
  userInfo: any = null;
  playerDetails: any = null;
  playerStats: any = null;
  matchHistory: any[] = [];
  loading = true;

  constructor(private http: Httpclientservice) {}

  ngOnInit(): void {
    this.loadUserInfo();
  }

  loadUserInfo(): void {
    try {
      const userInfoStr = localStorage.getItem('userInfo');
      if (userInfoStr) {
        this.userInfo = JSON.parse(userInfoStr);
        const userId = this.userInfo.UserID || this.userInfo.ID || this.userInfo.Id || this.userInfo.id;
        
        if (userId && this.userInfo.UserType?.toLowerCase() === 'player') {
          this.fetchPlayerDetails(userId);
        } else {
          this.loading = false;
        }
      } else {
        this.loading = false;
      }
    } catch (error) {
      console.error('Error loading user info:', error);
      this.loading = false;
    }
  }

  fetchPlayerDetails(userId: number) {
    this.http.GetData(`Player/GetPlayerByUserId?userId=${userId}`).subscribe({
      next: (res: any) => {
        if (res.success && res.data) {
          this.playerDetails = res.data;
          this.fetchPlayerStats(this.playerDetails.PlayerID);
          this.fetchMatchHistory(this.playerDetails.PlayerID);
        } else {
          this.loading = false;
        }
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  fetchPlayerStats(playerId: number) {
    this.http.GetData(`Player/GetPlayerStats?playerId=${playerId}`).subscribe({
      next: (res: any) => {
        if (res.success && res.data) {
          this.playerStats = res.data;
        }
      },
      error: () => {}
    });
  }

  fetchMatchHistory(playerId: number) {
    this.http.GetData(`Player/GetPlayerMatchHistory?playerId=${playerId}`).subscribe({
      next: (res: any) => {
        if (res.success && res.data) {
          this.matchHistory = res.data;
        }
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  getUserField(field: string): string {
    if (!this.userInfo) return 'N/A';
    // Handle both uppercase and lowercase field names
    return this.userInfo[field] || 
           this.userInfo[field.charAt(0).toUpperCase() + field.slice(1)] || 
           this.userInfo[field.toLowerCase()] || 
           'N/A';
  }
}

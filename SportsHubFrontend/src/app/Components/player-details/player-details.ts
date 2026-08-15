import { Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { Httpclientservice } from '../../Service/httpclientservice';
import { ActivatedRoute, RouterLink } from '@angular/router';

@Component({
  selector: 'app-player-details',
  standalone: true,
  imports: [CommonModule, DatePipe, RouterLink],
  templateUrl: './player-details.html',
  styleUrl: './player-details.css',
})
export class PlayerDetails implements OnInit {
  playerDetails: any = null;
  playerStats: any = null;
  matchHistory: any[] = [];
  loading = true;

  constructor(
    private http: Httpclientservice,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const playerId = Number(params.get('id'));
      if (playerId) {
        this.fetchPlayerDetails(playerId);
      } else {
        this.loading = false;
      }
    });
  }

  fetchPlayerDetails(playerId: number) {
    this.http.GetData(`Player/GetPlayerById?PlayerID=${playerId}`).subscribe({
      next: (res: any) => {
        if (res.success && res.data) {
          this.playerDetails = res.data;
          this.fetchPlayerStats(playerId);
          this.fetchMatchHistory(playerId);
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
}

import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Httpclientservice } from '../../Service/httpclientservice';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-today-match',
  imports: [CommonModule],
  templateUrl: './today-match.html',
  styleUrl: './today-match.css',
})
export class TodayMatch implements OnInit {
  matches: any[] = [];
  loading: boolean = true;
  selectedMatchId: number | null = null;
  players: any[] = [];
  playerLoading: boolean = false;

  constructor(private http: Httpclientservice,private cdr:ChangeDetectorRef,private rotuer:Router) { }

  ngOnInit(): void {
    this.fetchTodayMatches();
  }

  fetchTodayMatches() {
    this.loading = true;
    this.http.GetData('TeamSchedule/GetTodayMatches').subscribe({
      next: (res: any) => {
        this.matches = res.data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error(err);
        this.loading = false;
      }
    });
  }

  startMatch(matchId: number) {
    this.rotuer.navigate(['/layout/matchplay', matchId]);
}
}
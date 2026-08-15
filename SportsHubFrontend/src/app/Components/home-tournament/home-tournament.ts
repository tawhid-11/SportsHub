import { Component, ChangeDetectorRef, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Httpclientservice } from '../../Service/httpclientservice';

@Component({
  selector: 'app-home-tournament',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './home-tournament.html',
  styleUrls: ['./home-tournament.css']
})
export class HomeTournament implements OnInit {

  tournaments: any[] = [];
  loading: boolean = false;

  constructor(
    private http: Httpclientservice,
    private cdr: ChangeDetectorRef,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.getData();
  }

  getData(): void {
    this.loading = true;

    this.http.GetData('Tournaments/GetAllUpComing').subscribe({
      next: (res: any) => {
        if (res && res.success) {
          this.tournaments = res.data || [];
        } else {
          this.tournaments = [];
        }

        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading tournaments', err);
        this.loading = false;
        this.tournaments = [];
      }
    });
  }

  // 🔥 ONLY NEW METHOD ADDED
  registerNow(tournament: any): void {
    this.router.navigate(['/teams'], { queryParams: { id: tournament.TournamentID } });
  }

}

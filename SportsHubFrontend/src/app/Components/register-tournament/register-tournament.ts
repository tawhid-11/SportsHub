import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Httpclientservice } from '../../Service/httpclientservice';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-register-tournament',
  imports: [CommonModule],
  templateUrl: './register-tournament.html',
  styleUrl: './register-tournament.css',
})
export class RegisterTournament implements OnInit {

  tournaments: any[] = [];
  teams: any = {};


  constructor(private http: Httpclientservice, private router: Router, private cdr: ChangeDetectorRef) { }

  ngOnInit(): void {
    this.loadTournaments();
    this.loadTeam();
  }

  loadTournaments() {
    var user = JSON.parse(localStorage.getItem('userInfo') || '{}'); // or from auth service

    this.http.GetData(`Tournaments/GetUnregisterTournamentByuserId?userId=${user.ID}`)
      .subscribe({
        next: (res: any) => {
          this.tournaments = res.data;
          debugger;
          this.cdr.detectChanges();
        },
        error: err => console.error('Failed to load tournaments', err)
      });
  }

  loadTeam() {
    debugger;
    // Load teams from API
    var user = JSON.parse(localStorage.getItem('userInfo') || '{}');

    this.http.GetData(`Teams/GetTeamIdbyUserId?id=${user.ID}`).subscribe((res: any) => {
      debugger;
      this.teams = res.data;
      this.cdr.detectChanges();
    });


  }
  goToRegister(TournamentId: number) {
    debugger;
    var user = JSON.parse(localStorage.getItem('userInfo') || '{}');
    this.http.PostData('Teams/TournamentTeamMapping', {
      TournamentId: TournamentId,
      TeamId: this.teams.TeamsID,
      userId: user.ID
    }).subscribe({
      next: (res: any) => {
        if (res.paymentUrl) {
          window.location.href = res.paymentUrl;
        } else if (res.success === false) {
          alert('❌ Registration failed: ' + (res.message || 'Unknown error'));
        } else {
          alert('✅ Team successfully registered for the tournament!');
          this.loadTournaments();
        }
      },
      error: (err) => {
        console.error('❌ Team mapping to tournament failed', err);
        const errorMsg = err.error?.message || 'Server error occurred during registration';
        alert('❌ Error: ' + errorMsg);
      }
    });
  }
}



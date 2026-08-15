import { CommonModule } from '@angular/common';
import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { Httpclientservice } from '../../Service/httpclientservice';

@Component({
  selector: 'app-playing-tournament',
  imports: [CommonModule],
  templateUrl: './playing-tournament.html',
  styleUrl: './playing-tournament.css',
})
export class PlayingTournament implements OnInit {

  tournaments: any[] = [];


  constructor(private http: Httpclientservice,private router: Router, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadTournaments();
  }

  loadTournaments() {
     var user =JSON.parse(localStorage.getItem('userInfo') || '{}'); // or from auth service

    this.http.GetData(`Tournaments/GetTournamentsByuserId?userId=${user.ID}`)
      .subscribe({
        next: (res:any) =>{
          this.tournaments = res.data;
          this.cdr.detectChanges();

        } ,
        error: err => console.error('Failed to load tournaments', err)
      });
  }
  goToViewTeams(tournamentId: number) {
    this.router.navigate(['/teamownerlayout/registeredteams', tournamentId]);
  }
  goToSchedule(tournamentId: number) {
    this.router.navigate(['/teamownerlayout/schedules', tournamentId],{queryParams:{from:'teamowner'}});
  }
}


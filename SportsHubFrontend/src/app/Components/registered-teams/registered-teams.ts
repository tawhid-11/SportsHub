import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Httpclientservice } from '../../Service/httpclientservice';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-registered-teams',
  imports: [CommonModule],
  templateUrl: './registered-teams.html',
  styleUrl: './registered-teams.css',
})
export class RegisteredTeams implements OnInit {

  teams: any[] = [];
   TournamentId!: number;

  constructor(private http: Httpclientservice, private router: Router, private cdr: ChangeDetectorRef,  private route: ActivatedRoute) {}

   ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
     
      this.TournamentId = Number(params.get('id'));

      if (this.TournamentId) {
        this.loadRegisteredTeams(this.TournamentId);
      }
    });
  }

  loadRegisteredTeams(TournamentId: number) {

    this.http.GetData(`Teams/GetTeamIdbyTournamentId?id=${TournamentId}`).subscribe({
      next: (res: any) => {
        this.teams = res.data || [];
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Failed to load registered teams', err);
      }
    });
  }
  goToTeamPlayers(id: number) {
    debugger;
    this.router.navigate(['/teamownerlayout/viewplayers', id]);
  }
 
}

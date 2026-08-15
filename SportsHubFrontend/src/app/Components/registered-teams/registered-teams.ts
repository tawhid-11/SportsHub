import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Httpclientservice } from '../../Service/httpclientservice';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-registered-teams',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './registered-teams.html',
  styleUrl: './registered-teams.css',
})
export class RegisteredTeams implements OnInit {

  teams: any[] = [];
  TournamentId!: number;
  tournament: any = null;
  groups: number[] = [];

  constructor(private http: Httpclientservice, private router: Router, private cdr: ChangeDetectorRef, private route: ActivatedRoute) { }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.TournamentId = Number(params.get('id'));
      if (this.TournamentId) {
        this.loadTournament(this.TournamentId);
        this.loadRegisteredTeams(this.TournamentId);
      }
    });
  }

  loadTournament(id: number) {
    this.http.GetData(`Tournaments/GetTournamentsById?TournamentId=${id}`).subscribe({
      next: (res: any) => {
        this.tournament = res.data;
        if (this.tournament?.NumberOfGroups) {
          this.groups = Array.from({ length: this.tournament.NumberOfGroups }, (_, i) => i + 1);
        }
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

  saveAssignments() {
    const assignments = this.teams.map(t => ({
      TournamentId: this.TournamentId,
      TeamId: t.TeamId,
      GroupId: t.GroupId || 0
    })).filter(a => a.GroupId > 0);

    if (assignments.length === 0) return;

    this.http.PostData('Tournaments/UpdateGroupAssignment', assignments).subscribe({
      next: (res: any) => {
        alert('Groups assigned successfully!');
      },
      error: (err) => {
        alert('Failed to save assignments');
      }
    });
  }

  goToTeamPlayers(id: number) {
    this.router.navigate(['/teamownerlayout/viewplayers', id]);
  }
}

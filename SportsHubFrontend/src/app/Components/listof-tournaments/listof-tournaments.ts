import { ChangeDetectorRef, Component } from '@angular/core';
import { Httpclientservice } from '../../Service/httpclientservice';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-listof-tournaments',
  imports: [CommonModule],
  templateUrl: './listof-tournaments.html',
  styleUrl: './listof-tournaments.css',
})
export class ListofTournaments {
 tournaments: any[] = [];

  constructor(
    private http: Httpclientservice,
    private cdr: ChangeDetectorRef,
    private router: Router
  ) {
    this.getData();
  }

  getData() {
    this.http.GetData('Tournaments').subscribe((res: any) => {
     
      if (res && res.success) {
        this.tournaments = res.data;
        this.cdr.detectChanges();
      }
    });
  }
  

  onNew() {
    this.router.navigate(['/layout/tournaments-forms']);
  }

  onEdit(id: number) {
    this.router.navigate(['/layout/tournaments-forms'], {
      queryParams: { id }
    });
  }

  onDelete(TournamentID: number) {
    if (!confirm('Are you sure you want to delete this tournament?')) {
      return;
    }

    this.http.DeleteData('Tournaments', TournamentID).subscribe(() => {
      this.getData();
    });
  }
  goToSchedule(tournamentId: number) {
    this.router.navigate(['/layout/schedules', tournamentId],{queryParams:{from:'admin'}});
  }
}


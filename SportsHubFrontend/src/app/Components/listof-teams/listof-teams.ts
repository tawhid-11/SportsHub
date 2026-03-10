import { ChangeDetectorRef, Component } from '@angular/core';
import { Httpclientservice } from '../../Service/httpclientservice';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-listof-teams',
  imports: [CommonModule],
  templateUrl: './listof-teams.html',
  styleUrl: './listof-teams.css',
})
export class ListofTeams {
  teams: any[] = [];

  constructor(
    private http: Httpclientservice,
    private cdr: ChangeDetectorRef,
    private router: Router
  ) {
    this.getData();
  }

  getData() {
    this.http.GetData('Teams/GetAllWithTournament').subscribe((res: any) => {
      if (res && res.success) {
        this.teams = res.data;
        this.cdr.detectChanges();
      }
    });
  }

  onEdit(id: number) {
    // Navigate to edit page if needed
    // this.router.navigate(['/layout/teams-forms'], {
    //   queryParams: { id }
    // });
  }

  onDelete(TeamsID: number) {
    if (!confirm('Are you sure you want to delete this team?')) {
      return;
    }

    this.http.DeleteData('Teams', TeamsID).subscribe(() => {
      this.getData();
    });
  }
}

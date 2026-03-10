import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { Httpclientservice } from '../../Service/httpclientservice';

@Component({
  selector: 'app-tournament-points',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './tournament-points.html',
  styleUrl: './tournament-points.css'
})
export class TournamentPoints implements OnInit {
  tournamentId!: number;
  pointsTable: any[] = [];
  groupedTable: { [key: string]: any[] } = {};
  tournamentName: string = '';
  isLoading: boolean = true;
  errorMessage: string = '';

  constructor(
    private route: ActivatedRoute,
    private http: Httpclientservice,
    private cdr: ChangeDetectorRef,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.tournamentId = Number(params.get('id'));
      this.getPointsTable();
      this.getTournamentDetails();
    });
  }

  getPointsTable(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.http.GetData(`TournamentPoints/GetPointsTable?tournamentId=${this.tournamentId}`).subscribe({
      next: (res: any) => {
        if (res.success) {
          this.pointsTable = res.data || [];
          this.groupTeams();
        } else {
          this.errorMessage = res.message || 'Failed to load points table';
        }
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.errorMessage = 'An error occurred while fetching data';
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  groupTeams() {
    const groups: { [key: string]: any[] } = {};
    const hasGroups = this.pointsTable.some(t => t.GroupId || t.groupId);

    if (hasGroups) {
      this.pointsTable.forEach(team => {
        const gid = team.GroupId ?? team.groupId ?? 'Unassigned';
        const groupKey = `Group ${gid}`;
        if (!groups[groupKey]) groups[groupKey] = [];
        groups[groupKey].push(team);
      });
      this.groupedTable = groups;
    } else {
      this.groupedTable = { 'Overall Standings': this.pointsTable };
    }
  }

  getGroupKeys() {
    return Object.keys(this.groupedTable);
  }

  getTournamentDetails(): void {
    this.http.GetData(`Tournaments/GetTournamentsById?TournamentId=${this.tournamentId}`).subscribe((res: any) => {
      if (res.success) {
        const data = res.data;
        this.tournamentName = data.tournamentName ?? data.TournamentName ?? '';
        this.cdr.detectChanges();
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/']);
  }
}

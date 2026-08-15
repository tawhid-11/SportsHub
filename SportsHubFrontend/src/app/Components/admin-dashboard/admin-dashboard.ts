import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Httpclientservice } from '../../Service/httpclientservice';

@Component({
  selector: 'app-admin-dashboard',
  imports: [CommonModule, RouterLink],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.css'
})
export class AdminDashboard implements OnInit {
  statistics = {
    totalTeams: 0,
    totalTournaments: 0,
    totalPlayers: 0,
    totalMatches: 0
  };
  loading = true;

  constructor(private http: Httpclientservice,private cdr:ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadStatistics();
  }

  loadStatistics(): void {
    this.loading = true;
    this.http.GetData('Dashboard/GetStatistics').subscribe({
      next: (res: any) => {
        console.log('Dashboard response:', res);
        if (res && res.success && res.data) {

          // Handle both uppercase and lowercase property names
          this.statistics = {
            totalTeams: res.data.totalTeams || res.data.TotalTeams || 0,
            totalTournaments: res.data.totalTournaments || res.data.TotalTournaments || 0,
            totalPlayers: res.data.totalPlayers || res.data.TotalPlayers || 0,
            totalMatches: res.data.totalMatches || res.data.TotalMatches || 0
          };
         
        } else {
          console.warn('Invalid response format:', res);
        }
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading statistics:', err);
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }
}

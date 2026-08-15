import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-sidebar',
  imports: [RouterLink, RouterLinkActive,CommonModule],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css',
})
export class Sidebar {
  @Input() collapsed = false;
  @Output() toggle = new EventEmitter<void>();
  hasLogo = true;

  constructor(private router: Router) {}

  nav: any[] = [
    { label: 'Dashboard', icon: 'bi bi-grid-1x2-fill', route: '/layout' },
    { label: 'User Information', icon: 'bi bi-people-fill', route: '/layout/user-Dashboard' },
    { label: 'Tournament Types', icon: 'bi bi-tags-fill', route: '/layout/tournamentType' },
    { label: 'Tournaments', icon: 'bi bi-trophy-fill', route: '/layout/tournaments' },
    { label: 'Match Schedules', icon: 'bi bi-calendar-event-fill', route: '/layout/matches' },
    { label: 'Teams Management', icon: 'bi bi-shield-shaded', route: '/layout/teams' },
    { label: 'Player Database', icon: 'bi bi-person-lines-fill', route: '/layout/players' },
    { label: 'Player Roles', icon: 'bi bi-person-badge-fill', route: '/layout/playerRoles' },
    { label: 'Financials', icon: 'bi bi-credit-card-fill', route: '/layout/payments' },
  ];

  getUserName(): string {
    const user = JSON.parse(localStorage.getItem('userInfo') || '{}');
    return user.Name || 'Admin User';
  }

  getUserInitials(): string {
    const name = this.getUserName();
    return name.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase();
  }

  logout() {
    if (confirm("Are you sure you want to logout?")) {
      localStorage.removeItem('userInfo');
      localStorage.removeItem('jwtToken');
      this.router.navigate(['login']);
    }
  }
}

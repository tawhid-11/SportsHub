import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-pl-sidebar',
  imports: [CommonModule, RouterLinkActive, RouterLink],
  templateUrl: './pl-sidebar.html',
  styleUrl: './pl-sidebar.css',
})
export class PlSidebar {
  @Input() collapsed = false;
  @Output() toggle = new EventEmitter<void>();
  hasLogo = true;

  constructor(private router: Router) {}

  nav: any[] = [
    { label: 'Player Portal', icon: 'bi bi-grid-1x2-fill', route: '/PlayerDashboard/profile' },
    { label: 'My Performance', icon: 'bi bi-graph-up-arrow', route: '/PlayerDashboard/stats' },
    { label: 'Team Details', icon: 'bi bi-shield-shaded', route: '/PlayerDashboard/team' },
    { label: 'My Tournaments', icon: 'bi bi-trophy-fill', route: '/PlayerDashboard/playingtournament' },
    { label: 'Account Settings', icon: 'bi bi-gear-fill', route: '/PlayerDashboard/settings' },
  ];

  getUserName(): string {
    const user = JSON.parse(localStorage.getItem('userInfo') || '{}');
    return user.Name || 'Player';
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

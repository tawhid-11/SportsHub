import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-to-sidebar',
  imports: [RouterLink, RouterLinkActive, CommonModule],
  templateUrl: './to-sidebar.html',
  styleUrl: './to-sidebar.css',
})
export class ToSidebar {
  @Input() collapsed = false;
  @Output() toggle = new EventEmitter<void>();
  hasLogo = true;

  constructor(private router: Router) {}

  nav: any[] = [
    { label: 'My Dashboard', icon: 'bi bi-grid-1x2-fill', route: '/teamownerlayout' }, 
    { label: 'Tournament Entry', icon: 'bi bi-trophy-fill', route: '/teamownerlayout/tournamentregistration' }, 
    { label: 'Squad Management', icon: 'bi bi-people-fill', route: '/teamownerlayout/player' },
    { label: 'Active Matches', icon: 'bi bi-cricket-fill', route: '/teamownerlayout/playingtournament' },
  ];

  getUserName(): string {
    const user = JSON.parse(localStorage.getItem('userInfo') || '{}');
    return user.Name || 'Team Owner';
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

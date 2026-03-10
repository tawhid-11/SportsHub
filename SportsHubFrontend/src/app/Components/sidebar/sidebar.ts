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
  constructor(private router: Router) {}

  nav: any[] = [
{ label: 'Dashboard',      icon: '📊', route: '/layout' },
{ label: 'UserInfo',       icon: '🧑', route: '/layout/user-Dashboard' },
{ label: 'Tournament Type',icon: '🏆', route: '/layout/tournamentType' },
{ label: 'Tournaments',    icon: '🏆', route: '/layout/tournaments' },
{ label: 'Matches',        icon: '🏏', route: '/layout/matches' },
{ label: 'Teams',          icon: '🧑‍🤝‍🧑', route: '/layout/teams' },   
{ label: 'Players',        icon: '🧑', route: '/layout/players' },
{ label: 'Player Roles',   icon: '🏏', route: '/layout/playerRoles' },
{ label: 'Payments',       icon: '💳', route: '/layout/payments' },
// { label: 'Reports',        icon: '📈', route: '/layout/reports' },
// { label: 'Settings',       icon: '⚙️', route: '/layout/settings' },
// { label: 'Logout',         icon: '🚪', route: '/login' },
];

logout(){
  debugger;
  localStorage.removeItem('userInfo');
  this.router.navigate(['login']);
  // window.location.href = 'login';
}
}

import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-pl-sidebar',
  imports: [CommonModule,RouterLinkActive, RouterLink],
  templateUrl: './pl-sidebar.html',
  styleUrl: './pl-sidebar.css',
})
export class PlSidebar {
@Input() collapsed = false;
  @Output() toggle = new EventEmitter<void>();
  constructor(private router: Router) {}

  nav: any[] = [


{ label: 'Profile',        icon: '🧑', route: '/PlayerDashboard/profile' },
{ label: 'Stats',        icon: '📊', route: '/PlayerDashboard/stats' },
{ label: 'Team',        icon: '🧑', route: '/PlayerDashboard/team' },
{ label: 'Playing Tournament',   icon: '🏏', route: '/PlayerDashboard/playingtournament' },
{ label: 'Settings',       icon: '⚙️', route: '/PlayerDashboard/settings' },
];

logout(){
  debugger;
  localStorage.removeItem('userInfo');
  this.router.navigate(['login']);
}
}


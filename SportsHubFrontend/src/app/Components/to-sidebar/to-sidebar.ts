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
  constructor(private router: Router) {}

  nav: any[] = [
{ label: 'Register Tournament',icon: '🏆', route: '/teamownerlayout/tournamentregistration' }, 
{ label: 'Player',        icon: '🧑', route: '/teamownerlayout/player' },
{ label: 'Playing Tournament',   icon: '🏏', route: '/teamownerlayout/playingtournament' },
// { label: 'Logout',         icon: '🚪', route: '/login' },
];

logout(){
  debugger;
  localStorage.removeItem('userInfo');
  this.router.navigate(['login']);
  // window.location.href = 'login';
}
}


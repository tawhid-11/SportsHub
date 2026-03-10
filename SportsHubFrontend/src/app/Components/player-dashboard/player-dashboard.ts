import { Component, signal } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { PlSidebar } from '../pl-sidebar/pl-sidebar';
import { PlFooter } from '../pl-footer/pl-footer';
import { PlHeader } from '../pl-header/pl-header';

@Component({
  selector: 'app-player-dashboard',
  imports: [RouterOutlet, PlSidebar, PlFooter, PlHeader],
  templateUrl: './player-dashboard.html',
  styleUrl: './player-dashboard.css',
})
export class PlayerDashboard {
collapsed = signal(false);
  toggleSidebar() { this.collapsed.update(v => !v); }
}

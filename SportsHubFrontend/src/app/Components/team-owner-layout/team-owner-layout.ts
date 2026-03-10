import { Component, signal } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { Header } from '../header/header';
import { Footer } from '../footer/footer';
import { Sidebar } from '../sidebar/sidebar';
import { ToHeader } from '../to-header/to-header';
import { ToSidebar } from '../to-sidebar/to-sidebar';
import { ToFooter } from '../to-footer/to-footer';

@Component({
  selector: 'app-team-owner-layout',
  imports: [RouterOutlet,ToHeader,ToSidebar,ToFooter],
  templateUrl: './team-owner-layout.html',
  styleUrl: './team-owner-layout.css',
})
export class TeamOwnerLayout {
  collapsed = signal(false);
  toggleSidebar() { this.collapsed.update(v => !v); }
}

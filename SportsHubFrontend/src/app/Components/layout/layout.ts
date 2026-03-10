import { Component, signal } from '@angular/core';
import { Header } from '../header/header';
import { Sidebar } from '../sidebar/sidebar';
import { Footer } from '../footer/footer';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-layout',
  imports: [RouterOutlet, Header, Sidebar, Footer],
  templateUrl: './layout.html',
  styleUrl: './layout.css',
})
export class Layout {
 collapsed = signal(false);
  toggleSidebar() { this.collapsed.update(v => !v); }
}

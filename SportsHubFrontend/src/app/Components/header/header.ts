import { Component, EventEmitter, Output } from '@angular/core';
import { SignalrService } from '../../Service/SignalrService';

@Component({
  selector: 'app-header',
  imports: [],
  templateUrl: './header.html',
  styleUrl: './header.css',
})
export class Header {
  constructor(private signalR:SignalrService) {
    this.signalR.startConnection();
  }
  @Output() menu = new EventEmitter<void>();
}

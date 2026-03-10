import { Component, EventEmitter, Output } from '@angular/core';

@Component({
  selector: 'app-to-header',
  imports: [],
  templateUrl: './to-header.html',
  styleUrl: './to-header.css',
})
export class ToHeader {
 @Output() menu = new EventEmitter<void>();
}

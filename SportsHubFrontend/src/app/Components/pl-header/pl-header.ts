import { Component, EventEmitter, Output } from '@angular/core';

@Component({
  selector: 'app-pl-header',
  imports: [],
  templateUrl: './pl-header.html',
  styleUrl: './pl-header.css',
})
export class PlHeader {
@Output() menu = new EventEmitter<void>();
}

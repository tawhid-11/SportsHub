import { Component } from '@angular/core';

@Component({
  selector: 'app-to-footer',
  imports: [],
  templateUrl: './to-footer.html',
  styleUrl: './to-footer.css',
})
export class ToFooter {
 year = new Date().getFullYear();
}

import { Component } from '@angular/core';

@Component({
  selector: 'app-pl-footer',
  imports: [],
  templateUrl: './pl-footer.html',
  styleUrl: './pl-footer.css',
})
export class PlFooter {
  
 year = new Date().getFullYear();
}

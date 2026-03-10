import { ChangeDetectorRef, Component } from '@angular/core';
import { Httpclientservice } from '../../Service/httpclientservice';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-user-info-list',
  imports: [CommonModule],
  templateUrl: './user-info-list.html',
  styleUrl: './user-info-list.css',
})
export class UserInfoList {
  users: any[] = [];

  constructor(
    private http: Httpclientservice,
    private cdr: ChangeDetectorRef,
    private router: Router
  ) {
    this.getData();
  }

  getData() {
    this.http.GetData('UserInfo').subscribe((res: any) => {
      if (res && res.success) {
        this.users = res.data;
        this.cdr.detectChanges();
      }
    });
  }
}

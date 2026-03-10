import { Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';

@Component({
  selector: 'app-player-profile',
  imports: [CommonModule, DatePipe],
  templateUrl: './player-profile.html',
  styleUrl: './player-profile.css',
})
export class playerProfile implements OnInit {
  userInfo: any = null;
  loading = true;

  ngOnInit(): void {
    this.loadUserInfo();
  }

  loadUserInfo(): void {
    try {
      const userInfoStr = localStorage.getItem('userInfo');
      if (userInfoStr) {
        this.userInfo = JSON.parse(userInfoStr);
      }
    } catch (error) {
      console.error('Error loading user info:', error);
    } finally {
      this.loading = false;
    }
  }

  getUserField(field: string): string {
    if (!this.userInfo) return 'N/A';
    // Handle both uppercase and lowercase field names
    return this.userInfo[field] || 
           this.userInfo[field.charAt(0).toUpperCase() + field.slice(1)] || 
           this.userInfo[field.toLowerCase()] || 
           'N/A';
  }
}

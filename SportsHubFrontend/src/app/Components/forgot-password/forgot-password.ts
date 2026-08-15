import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Httpclientservice } from '../../Service/httpclientservice';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './forgot-password.html',
  styleUrl: './forgot-password.css'
})
export class ForgotPassword {
  step: number = 1; // 1: Input Target, 2: Verify OTP, 3: New Password
  target: string = '';
  method: string = 'Email';
  otp: string = '';
  newPassword: string = '';
  confirmPassword: string = '';
  loading: boolean = false;
  message: string = '';
  isError: boolean = false;

  constructor(private http: Httpclientservice, private router: Router) {}

  sendOTP() {
    if (!this.target) return;
    this.loading = true;
    this.http.PostData('Auth/SendOTP', { Target: this.target, Type: this.method }).subscribe({
      next: (res: any) => {
        this.loading = false;
        this.step = 2;
        this.showMessage(res.message, false);
      },
      error: (err: any) => {
        this.loading = false;
        this.showMessage(err.error?.message || 'Failed to send OTP', true);
      }
    });
  }

  verifyOTP() {
    if (!this.otp) return;
    this.loading = true;
    this.http.PostData('Auth/VerifyOTP', { Target: this.target, OTP: this.otp }).subscribe({
      next: (res: any) => {
        this.loading = false;
        this.step = 3;
        this.showMessage(res.message, false);
      },
      error: (err: any) => {
        this.loading = false;
        this.showMessage(err.error?.message || 'Invalid OTP', true);
      }
    });
  }

  resetPassword() {
    if (this.newPassword.length < 8) {
      this.showMessage('Password must be at least 8 characters long', true);
      return;
    }
    if (this.newPassword !== this.confirmPassword) {
      this.showMessage('Passwords do not match', true);
      return;
    }
    this.loading = true;
    this.http.PostData('Auth/ResetPassword', { Target: this.target, NewPassword: this.newPassword }).subscribe({
      next: (res: any) => {
        this.loading = false;
        this.showMessage('Password Reset Successful!', false);
        setTimeout(() => this.router.navigate(['/login']), 2000);
      },
      error: (err: any) => {
        this.loading = false;
        this.showMessage(err.error?.message || 'Reset failed', true);
      }
    });
  }

  showMessage(msg: string, isErr: boolean) {
    this.message = msg;
    this.isError = isErr;
    setTimeout(() => this.message = '', 5000);
  }
}

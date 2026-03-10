import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Httpclientservice } from '../../Service/httpclientservice';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-registration',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './registration.html',
  styleUrl: './registration.css',
})
export class Registration {
  registerData = {
    name: '',
    email: '',
    phone: '',
    userType: '',
    password: ''
  };

  confirmPassword = '';
  errorMessage = '';

  constructor(private router: Router, private httpService: Httpclientservice) { }

  onRegister() {
    if (!this.validateRegistration()) {
      return;
    }

    this.httpService.PostData('UserInfo/Register', this.registerData).subscribe({
      next: (response: any) => {
        alert('🎉 Registration Successful! You can now log in.');
        this.router.navigate(['/login']);
      },
      error: (err: any) => {
        console.error('Registration failed:', err);
        this.errorMessage = err.error?.message || 'Registration failed! Please try again.';
      }
    });
  }

  validateRegistration(): boolean {
    this.errorMessage = '';
    const emailPattern = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$/;
    const phonePattern = /^\d{10,14}$/;

    if (!this.registerData.name || this.registerData.name.length < 3) {
      this.errorMessage = 'Full Name must be at least 3 characters long.';
      return false;
    }

    if (!emailPattern.test(this.registerData.email)) {
      this.errorMessage = 'Please enter a valid email address.';
      return false;
    }

    if (!phonePattern.test(this.registerData.phone)) {
      this.errorMessage = 'Please enter a valid phone number (10-14 digits).';
      return false;
    }

    if (!this.registerData.userType) {
      this.errorMessage = 'Please select a user type.';
      return false;
    }

    if (this.registerData.password.length < 6) {
      this.errorMessage = 'Password must be at least 6 characters long.';
      return false;
    }

    if (this.registerData.password !== this.confirmPassword) {
      this.errorMessage = 'Passwords do not match.';
      return false;
    }

    return true;
  }

  onRoute() {
    this.router.navigate(['/login']);
  }
}

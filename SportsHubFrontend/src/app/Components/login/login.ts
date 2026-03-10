import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from "@angular/router";
import { Httpclientservice } from '../../Service/httpclientservice';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-login',
  imports: [FormsModule, RouterLink, NgClass],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {

  loginData = {
    email: '',
    password: ''
  };

  showPassword: boolean = false;

  constructor(
    private router: Router,
    private httpService: Httpclientservice
  ) { }

  togglePassword() {
    this.showPassword = !this.showPassword;
  }

  onLogin() {
    const emailPattern = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$/;
    if (!emailPattern.test(this.loginData.email)) {
      alert("❌ Please enter a valid email address.");
      return;
    }
    this.httpService.PostData('UserInfo/Login', this.loginData).subscribe(
      (response: any) => {
        localStorage.setItem('userInfo', JSON.stringify(response.data));
        if (response.data && (response.data.Success == 1 || response.data.Success === true)) {
          const user = response.data;
          const userType = (user.UserType || user.userType || '').toLowerCase();
          debugger;
          if (userType === 'admin') {
            this.router.navigate(['/layout']);
          } else if (userType === 'teamowner') {
            this.router.navigate(['/teamownerlayout']);
          } else if (userType === 'player') {
            this.router.navigate(['/PlayerDashboard']);
          }
          alert("🎉 Login Successful! Welcome back.");
        } else {
          alert("❌ Login Failed! " + (response.data?.Message || "Invalid credentials"));
        }
      },
      () => {
        alert("❌ Login Failed! Invalid email or password.");
      }
    );
  }
}

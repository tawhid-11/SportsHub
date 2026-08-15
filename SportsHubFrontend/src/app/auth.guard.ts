import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const token = localStorage.getItem('jwtToken');

  if (token) {
    return true; // Access granted if token exists
  } else {
    // Optional: You can also check for user roles here if needed
    router.navigate(['/login']); // Redirect to login if no token
    return false;
  }
};

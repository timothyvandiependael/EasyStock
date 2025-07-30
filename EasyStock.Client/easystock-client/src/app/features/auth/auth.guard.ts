import { CanActivateFn, Router } from '@angular/router';
import { Injectable } from '@angular/core';
import { inject } from '@angular/core';
import { AuthService } from './auth-service';

export const AuthGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  debugger;

  if (authService.isLoggedIn()) {
    return true;
  }


  return router.parseUrl('/login');
};

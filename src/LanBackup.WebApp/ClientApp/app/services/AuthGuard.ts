import { Injectable, Inject } from '@angular/core';
import { CanActivate } from '@angular/router';
import { Router } from '@angular/router';
import { AuthenticationService } from './authentication.service';


@Injectable()
export class AuthGuard implements CanActivate {

  auth: any = {};

  constructor(private authService: AuthenticationService, private router: Router) {

  }

  canActivate() {
    if (this.authService.checkIsLoggedIn()) {
      //this.router.navigate(['/']);
      return true;
    }
    else {
      this.router.navigate(['/login']);
    }
    return false;
  }
}
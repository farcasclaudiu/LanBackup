import { Injectable, Inject } from '@angular/core';
import { Router } from '@angular/router';
import { Location } from '@angular/common';
import { isBrowser } from 'angular2-universal';
import { Observable } from 'rxjs/Observable';
import { RequestOptions, Request, RequestMethod } from '@angular/http';
import { WebApiService } from './webapi.service';

import { LoggerService } from './logger.service';


export class User {

  email: string;
  password: string;
  newpassword: string;
  isAdmin: boolean;
  succeeded: boolean;
  isLockedOut: boolean;
  errors: any;

  constructor(data) {
    Object.assign(this, data);
  }
}


@Injectable()
export class AuthenticationService {

  constructor(
    private _router: Router,
    private location: Location,
    private webApi: WebApiService,
    private log: LoggerService
  ) { }




  logout() {
    localStorage.removeItem("user");
    this._router.navigate(['login']);
  }

  login(user: User, success, error) {

    return this.webApi.loginUser(user)
      .subscribe(data => {
        if (data.succeeded) {
          localStorage.setItem("user", btoa(JSON.stringify(data)));
          this._router.navigate(['/']);//to default or last url
          success(data);
        }
        else {
          //clear 
          localStorage.removeItem("user");
          error(data.errors.map(err=>err.description));
        }
      }, err => {
        error(err);
      });
  }

  registerUser(user: User, success, error) {
    return this.webApi.registerUser(user)
      .subscribe(data => {
        if (data.succeeded) {
          localStorage.setItem("user", btoa(JSON.stringify(data)));
          //created and logged in
          success();
        }
        else {
          //could not create
          localStorage.removeItem("user");
        }
      }, err => {
        error(err);
      })
      ;
  }





  getLoggedUser(): User {
    if (isBrowser) {
      if (localStorage.getItem("user") === null) {
        return null;
      }
      let _user: User = JSON.parse(atob(localStorage.getItem("user")));
      return _user;
    }
    return null;
  }

  checkCredentials() {
    if (isBrowser) {
      if (localStorage.getItem("user") === null) {
        this._router.navigate(['login']);
      }
      return true;
    }
    return false;
  }

  checkIsAdmin() {
    if (isBrowser) {
      if (localStorage.getItem("user") != null) {
        let _user: User = JSON.parse(atob(localStorage.getItem("user")));
        return _user && _user.isAdmin;
      }
      return false;
    }
    return false;
  }

  checkIsLoggedIn() {
    if (isBrowser) {
      try {
        if (localStorage.getItem("user") === null) {
          return false;
        }
        return true;
      }
      catch (e) {
        this.log.error('e: ' + e);
      }
      return false;
    }
    return false;
  }


}
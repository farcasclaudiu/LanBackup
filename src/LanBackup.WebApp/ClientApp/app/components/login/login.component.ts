import { Component, OnInit, Input } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { Router } from '@angular/router';
import { AuthenticationService, User } from '../../services/authentication.service';

import { LoggerService } from '../../services/logger.service';
import { ToastNotification, ToastType } from '../../services/notifications.service';
import { MessageService, Messages } from '../../services/message.service';


@Component({
  selector: 'login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {


  public user = new User({ email: '', password: '', isAdmin: false });
  public errorMsg = '';
  public repassword = '';

  constructor(
    private _service: AuthenticationService,
    private _router: Router,
    private messageService: MessageService,
    private log: LoggerService
  ) {
  }



  showToast(toast: ToastNotification) {
    this.messageService.broadcast(Messages.MESSAGE_NOTIFY, toast);
  }


  ngOnInit(): void {
    if (this._service.checkIsLoggedIn()) {
      this.log.debug('oninit() navigate to root');
      this._router.navigate(['/']);
    }
  }


  doLogin() {
    this.log.debug("doLogin()");
    if (this.user.password.length > 0 &&
      this.user.email.length > 0) {
      //
      this._service.login(this.user, () => {
        //data 
      }, err => {
        this.showToast({
          title: "Login error",
          body: err,
          type: ToastType.error
        });
      });
    }
  }

  doRegister() {
    this.log.debug("doRegister()");
    if (this.repassword.length > 0 && this.repassword == this.user.password &&
      this.user.email.length > 0) {
      this._service.registerUser(this.user, () => {
        //is registered
        this.showToast({
          title: "Register success",
          body: "The user has been created",
          type: ToastType.success
        });
        //navigate default to root
        this._router.navigate(['/']);//to default or last url
      },
        (err) => {
          this.showToast({
            title: "Register error",
            body: err.text(),
            type: ToastType.error
          });
          this.log.error(err);
        });
    }
    else {
      this.showToast({
        title: "Invalid data",
        body: "Some data not OK!",
        type: ToastType.error
      });
    }
  }


}


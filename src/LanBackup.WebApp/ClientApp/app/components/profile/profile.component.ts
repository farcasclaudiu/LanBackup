import { Component, OnInit, Input } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { Router } from '@angular/router';
import { AuthenticationService, User } from '../../services/authentication.service';
import { WebApiService } from '../../services/webapi.service';

import { LoggerService } from '../../services/logger.service';
import { ToastNotification, ToastType } from '../../services/notifications.service';
import { MessageService, Messages } from '../../services/message.service';


@Component({
  selector: 'profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {


  public user : User;
  public errorMsg = '';
  public password: string = '';
  public repassword: string = '';

  constructor(
    private _service: AuthenticationService,
    private _router: Router,
    private _webApi: WebApiService,
    private messageService: MessageService,
    private log: LoggerService
  ) {
  }

  ngOnInit(): void {
    this.user = this._service.getLoggedUser();
  }

  showToast(toast: ToastNotification) {
    this.messageService.broadcast(Messages.MESSAGE_NOTIFY, toast);
  }

  isAdmin() {
    return this._service.checkIsAdmin();
  }

  doChangePassword() {
    this.password = this.password.trim();
    this.repassword = this.repassword.trim();

    let mpass = this.password.trim();
    let mrepass = this.repassword.trim();

    if (this.user && this.user.email &&
      mpass.length > 0 &&
      mpass == mrepass) {
      this.user.newpassword = mpass;
      this._webApi.changePassword(this.user).subscribe((data) => {
        if (data.succeeded) {
          //notify pwd changed
          this.showToast({ title: "Password change success", body: "Password has been changed!", type: ToastType.success });
        }
        else {
          //notify something not OK
          if (!data.isLoggedIn) {
            localStorage.removeItem("user");
            this._router.navigate(['/']);//to default or last url
          }
          this.showToast({ title: "Password change error", body: data.errors.map(err => err.description), type: ToastType.error });
        }
      }, err => {
        this.log.error(err);
        //notify err
        this.showToast({ title: "Password change error", body: err.text(), type: ToastType.error });
      });
    }
    else if (mpass.length == 0) {
      this.errorMsg = 'Password cannot be empty!';
      this.showToast({ title: "Error", body: this.errorMsg, type: ToastType.error });
    }
    else {
      this.errorMsg = 'Passwords does not match!';
      this.showToast({ title: "Error", body: this.errorMsg, type: ToastType.error });
    }
  }



}


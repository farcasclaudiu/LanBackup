import { Component, OnInit, Input, ViewChild } from '@angular/core';
import { BackupConfiguration } from '../../model/BackupConfiguration';
import { PaginatedList } from '../../model/PaginatedList';
import { WebApiService } from '../../services/webapi.service';
import { Observable } from 'rxjs/Observable';
import { AuthenticationService } from '../../services/authentication.service';

import { User } from '../../services/authentication.service';

import { LoggerService } from '../../services/logger.service';
import { ToastNotification, ToastType } from '../../services/notifications.service';
import { MessageService, Messages } from '../../services/message.service';


@Component({
  selector: 'logs',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.css']
})
export class UsersComponent implements OnInit {


  public list: User[] = new Array<User>();
  currentPage: number = 1;
  pageSize: number = 10;
  totalPages: number;
  loading: boolean;


  constructor(
    private webApi: WebApiService,
    private _service: AuthenticationService,
    private messageService: MessageService,
    private log: LoggerService
  ) { }


  ngOnInit(): void {
    this.getPage();
  }


  showToast(toast: ToastNotification) {
    this.messageService.broadcast(Messages.MESSAGE_NOTIFY, toast);
  }


  getPage() {
    //this.log.debug('getting page: ' + page);
    this.loading = true;
    this.webApi.getAllUsers().subscribe(
      (data) => {
        //this.log.debug('data: ' + JSON.stringify(data));
        this.list = data;

        this.totalPages = data.length;// data.tp * this.pageSize;
        this.loading = false;
      },
      err => this.log.error(err)
    );
  }


  toggleAdmin(user: User) {
    let prevValue = user.isAdmin;
    user.isAdmin = user.isAdmin ? false : true;
    this.log.debug('Make user admin - ' + user.isAdmin);
    return this.webApi.changeAdminRole(user).subscribe((data) => {
      //
      if (data.succeeded) {
        this.showToast({ title: "User change success", body: `User ${user.email} has been ${user.isAdmin ? "promoted" : "revoked"}`, type: ToastType.success });
      }
      else {
        user.isAdmin = prevValue;
        this.log.debug(data);
        this.showToast({ title: "User change error", body: `The user ${user.email} cannot be changed!`, type: ToastType.error });
      }
    }, err => {
      this.log.error(err);
      user.isAdmin = prevValue;
      this.showToast({ title: "User change error", body: err, type: ToastType.error });
    });
  }

  isAllowedToChange(user: User) {
    let isAdmin = this._service.checkIsAdmin();
    if (isAdmin) {
      let currentUser = this._service.getLoggedUser();
      if (currentUser) {
        if (currentUser.email == user.email) {
          //not allowed to change your own password
          return false;
        } else {
          return true;//only case to allow editing
        }
      }
    }
    return false;
  }

}

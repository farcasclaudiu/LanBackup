import { Component, OnInit, Input, ViewChild } from '@angular/core';
import { BackupConfiguration } from '../../model/BackupConfiguration';
import { PaginatedList } from '../../model/PaginatedList';
import { WebApiService } from '../../services/webapi.service';
import { Observable } from 'rxjs/Observable';
import { AuthenticationService } from '../../services/authentication.service';
import { ModalComponent } from '../../shared/modal/modal.component';
import { BackupLog } from '../../model/BackupLog';

import { LoggerService } from '../../services/logger.service';
import { ToastNotification, ToastType } from '../../services/notifications.service';
import { MessageService, Messages } from '../../services/message.service';


@Component({
  selector: 'logs',
  templateUrl: './logs.component.html',
  styleUrls: ['./logs.component.css']
})
export class LogsComponent implements OnInit {


  public list: PaginatedList<BackupLog> = new PaginatedList<BackupLog>(null);


  currentPage: number = 1;
  pageSize: number = 10;
  totalPages: number;
  isloading: boolean;


  constructor(
    private webApi: WebApiService,
    private _service: AuthenticationService,
    private messageService: MessageService,
    private log: LoggerService
  ) { }


  ngOnInit(): void {
    this.doRefresh();
  }


  doRefresh() {
    this.getPage(this.currentPage);
  }


  getPage(page: number) {
    //this.log.debug('getting page: ' + page);
    this.isloading = true;
    this.webApi.getLogsPage(page, this.pageSize).subscribe(
      (data) => {
        setTimeout(() => {
          this.list = data;
          this.currentPage = page
          this.totalPages = data.tp * this.pageSize;
          this.isloading = false;
        }, 400);//induced delay
      },
      err => this.log.error(err)
    );
  }


}

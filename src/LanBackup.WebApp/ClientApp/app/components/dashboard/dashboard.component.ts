import { Component, OnInit, Input, ViewChild, ChangeDetectorRef } from '@angular/core';
import { StatusReportInfo } from '../../model/StatusReportInfo';
import { SaveResult } from '../../model/SaveResult';
import { PaginatedList } from '../../model/PaginatedList';
import { WebApiService } from '../../services/webapi.service';
import { Observable } from 'rxjs/Observable';
import { AuthenticationService } from '../../services/authentication.service';
import { ModalComponent } from '../../shared/modal/modal.component';

import { LoggerService } from '../../services/logger.service';
import { ToastNotification, ToastType } from '../../services/notifications.service';
import { MessageService, Messages } from '../../services/message.service';
import { AgentsService } from '../../services/agents.service';
import { Subscription } from 'rxjs/Subscription';


@Component({
  selector: 'dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'
  ]
})
export class DashboardComponent implements OnInit {


  private list: StatusReportInfo[] = []; // PaginatedList<StatusReportInfo> = new PaginatedList<StatusReportInfo>(null);

  type = 'info';//can be success, info, warning, danger
  currentPage: number = 1;
  pageSize: number = 5;
  totalPages: number;
  public isloading = false;

  private subscription: Subscription;


  constructor(
    private webApi: WebApiService,
    private _service: AuthenticationService,
    private messageService: MessageService,
    private log: LoggerService,
    private agentsService: AgentsService,
    private changeDetectionRef: ChangeDetectorRef
  ) {
    this.subscription = this.messageService.subscribe(Messages.MESSAGE_REFRESHAGENTS, (payload) => {
      //this.log.debug(payload);
      this.list = payload as StatusReportInfo[];
      //trigger change detection
      setTimeout(() =>
        //mark for detection change and refresh
        this.changeDetectionRef.detectChanges()
        , 10);
      
      this.isloading = false;
    });
  }


  showToast(toast: ToastNotification) {
    this.messageService.broadcast(Messages.MESSAGE_NOTIFY, toast);
  }


  ngOnInit(): void {
    this.list = this.agentsService.getAgents();
  }


  doRefresh() {
    this.isloading = true;
    //force service refresh
    this.agentsService.refreshAgents();
  }

  getPage(page: number) {
    this.currentPage = page
    this.totalPages = this.list.length;
  }


}

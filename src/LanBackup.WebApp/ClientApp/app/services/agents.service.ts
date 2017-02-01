import { Injectable } from '@angular/core';
import { WebApiService } from '../services/webapi.service';
import { LoggerService } from './logger.service';
import { StatusReportInfo } from '../model/StatusReportInfo';
import { MessageService, Messages } from './message.service';
import { Observable } from 'rxjs/Observable';


@Injectable()
export class AgentsService {

  isLoaded: boolean;
  agents: StatusReportInfo[] = [];
  connectionState$: Observable<string>;


  constructor(
    private webApi: WebApiService,
    private messageService: MessageService,
    private log: LoggerService,

  ) {
    //wiring signalr

    this.log.debug('connecting to signalr');

    let service = this;
    let connection = $["hubConnection"](); // $.hubConnection();
    let contosoChatHubProxy = connection.createHubProxy('backupslan');
    //hook on client methos 'agentsRefresh'
    contosoChatHubProxy.on('agentsRefresh', function (newagents) {
      service.agents = newagents as StatusReportInfo[];
        service.messageService.broadcast(Messages.MESSAGE_REFRESHAGENTS, service.agents);
    });
    connection.start().done(function () {
      //service.log.debug('SignalR connected');
    });

  }



  getAgents() {
    if (!this.isLoaded) {
      this.refreshAgents();
    }
    return this.agents;
  }

  refreshAgents() {
    this.isLoaded = false;
    this.webApi.getAgentsAll()
      .subscribe(data => {
        this.agents = data;
        this.isLoaded = true;
        this.messageService.broadcast(Messages.MESSAGE_REFRESHAGENTS, this.agents);
      });
  }

}
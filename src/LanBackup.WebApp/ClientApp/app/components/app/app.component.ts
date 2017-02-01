import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { AuthenticationService } from '../../services/authentication.service';
import { Observable } from 'rxjs/Observable';

import { MessageService, Messages } from '../../services/message.service';
import { NotificationsService } from '../../services/notifications.service';
import { LoggerService, Level } from '../../services/logger.service';
import { AgentsService } from '../../services/agents.service';
import { AppInsightsService } from 'ng2-appinsights';

import { GlobalRef } from '../../shared/global/global-ref';

@Component({
    selector: 'app',
    templateUrl: './app.component.html',
    styleUrls: [
      "../../../../node_modules/ng2-toasty/style-material.css"
      , '../../bootswatch/css/bootstrap_yeti.min.css'
      ,'./app.component.css' //THIS SHOULD BE LATS TO OVERRIDE OTHER GLOBAL STYLES
    ],
    encapsulation: ViewEncapsulation.None
})
export class AppComponent implements OnInit {


  instrumentationEnabled: boolean;
  instrumentationKey: string;

  constructor(
    private authService: AuthenticationService,
    private _service: NotificationsService,
    private messageService: MessageService,
    private log: LoggerService,
    private agentsService: AgentsService,
    private appinsightsService: AppInsightsService,
    private _global: GlobalRef
  ) {
    this.instrumentationEnabled = _global.nativeGlobal.clientSettings.instrumentationEnabled;
    this.instrumentationKey = _global.nativeGlobal.clientSettings.instrumentationKey;
    log.level = Level.DEBUG;
    if (this.instrumentationEnabled) {
      this.appinsightsService.Init({
        instrumentationKey: this.instrumentationKey // key obtained from clientSettings
      });
    }
  }

  ngOnInit() {
      this.agentsService.refreshAgents();
  }

}

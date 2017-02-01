import { Component, ChangeDetectorRef } from '@angular/core';
import { AuthenticationService, User } from '../../services/authentication.service';
import { MessageService, Messages } from '../../services/message.service';
//import { AgentsService } from '../../services/agents.service';
import { Subscription } from 'rxjs/Subscription';
import { StatusReportInfo } from '../../model/StatusReportInfo';


@Component({
    selector: 'nav-menu',
    templateUrl: './navmenu.component.html',
    styleUrls: ['./navmenu.component.css'],
    //providers: [AuthenticationService]
})
export class NavMenuComponent {

  private subscription: Subscription;
  private agentsCount: number = 0;

  constructor(
    private _authService: AuthenticationService,
    //private agentsService: AgentsService,
    private messageService: MessageService,
    private changeDetectionRef: ChangeDetectorRef
  ) {
    this.subscription = this.messageService.subscribe(Messages.MESSAGE_REFRESHAGENTS, (payload) => {
      let list = payload as StatusReportInfo[];
      this.agentsCount = list.length;

      setTimeout(() =>
        this.changeDetectionRef.detectChanges()
      , 10);
    });
  }


  isLoggedIn() {
    return this._authService.checkIsLoggedIn();
  }

  logOut() {
    this._authService.logout();
  }

}

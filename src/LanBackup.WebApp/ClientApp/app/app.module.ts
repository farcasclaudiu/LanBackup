import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { UniversalModule } from 'angular2-universal';
import { AppComponent } from './components/app/app.component'
import { NavMenuComponent } from './components/navmenu/navmenu.component';
import { BackupsComponent } from './components/backups/backups.component';
import { LoginComponent } from './components/login/login.component';
import { ProfileComponent } from './components/profile/profile.component';
import { LogsComponent } from './components/logs/logs.component';
import { UsersComponent } from './components/users/users.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';


import { WebApiService } from './services/webapi.service';
import { AuthenticationService } from './services/authentication.service';
import { Ng2PaginationModule } from './shared/ng2-pagination/ng2-pagination'; // <-- import the module
import { FormsModule } from '@angular/forms';
import { AuthGuard } from './services/authguard';
import { ModalComponent } from './shared/modal/modal.component';
import { BrowserModule } from "@angular/platform-browser";

import { ToastyModule } from 'ng2-toasty';

import { MessageService } from './services/message.service';
import { NotificationsService } from './services/notifications.service';
import { AgentsService } from './services/agents.service';


import { LoggerService } from './services/logger.service';
import { Ng2SwitchComponent } from './shared/ng2-switch/ng2-switch';
import { Progress } from './shared/progressbar/progress';
import { Bar } from './shared/progressbar/bar';
import { Progressbar } from './shared/progressbar/progressbar';
import { AppInsightsModule } from 'ng2-appinsights';


import { SharedModule } from './shared/global/SharedModule';



@NgModule({
  bootstrap: [AppComponent],
  declarations: [
    AppComponent,
    NavMenuComponent,
    Progress,
    Bar,
    Progressbar,
    BackupsComponent,
    LoginComponent,
    ProfileComponent,
    ModalComponent,
    Ng2SwitchComponent,
    LogsComponent,
    UsersComponent,
    DashboardComponent
  ],
  imports: [
    UniversalModule, // Must be first import. This automatically imports BrowserModule, HttpModule, and JsonpModule too.
    RouterModule.forRoot([
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: DashboardComponent },
      { path: 'backups', component: BackupsComponent },
      { path: 'logs', component: LogsComponent },
      { path: 'users', component: UsersComponent, canActivate: [AuthGuard] },
      { path: 'login', component: LoginComponent },
      { path: 'profile', component: ProfileComponent, canActivate: [AuthGuard] },
      { path: '**', redirectTo: 'dashboard' }
    ]),
    Ng2PaginationModule,
    FormsModule,
    ToastyModule.forRoot(),
    AppInsightsModule,
    SharedModule.forBrowser()
  ],
  providers: [
    LoggerService,
    MessageService,
    WebApiService,
    AuthenticationService,
    AuthGuard,
    NotificationsService,
    AgentsService
  ]
})
export class AppModule {

  public AppModule() {
    console.log('AppModule');
  }

}

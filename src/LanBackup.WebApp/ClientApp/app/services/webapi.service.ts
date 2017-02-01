import { Injectable } from '@angular/core';
import { Http, Headers, RequestOptions, Response } from '@angular/http';
import { Observable } from 'rxjs/Rx';
import 'rxjs/add/operator/toPromise';
//models
import { BackupConfiguration } from '../model/BackupConfiguration';
import { BackupLog } from '../model/BackupLog';
import { StatusReportInfo } from '../model/StatusReportInfo';
import { User } from '../services/authentication.service';
import { SaveResult } from '../model/SaveResult';
import { SignInResult } from '../model/SignInResult';


import { PaginatedList } from '../model/PaginatedList';
import { LoggerService } from './logger.service';



@Injectable()
export class WebApiService {
  public list: BackupConfiguration[];

  constructor(
    private http: Http,
    private log: LoggerService
  ) {
  }


  // ### REGION BACKUPS ///

  getBackupsAll(): Promise<BackupConfiguration[]> {
    return this.http
      .get('/api/backupconfig/')
      .toPromise()
      .then(response => response.json())
      .then(backups => Array.from(backups, b => new BackupConfiguration(b)))
      .catch(error => this.log.error(error));
  }


  getBackupsPage(idx: number, siz: number): Observable<PaginatedList<BackupConfiguration>> {
    let headers = new Headers({
      'Content-Type': 'application/json',
      'idx': idx,
      'siz': siz
    });
    let options = new RequestOptions({ headers: headers });
    return this.http
      .get('/api/backupconfig', options)
      .map((res: Response) => res.json())
      .catch(error => Observable.throw(error.text() || 'Server error'));
  }


  saveBackup(toSave: BackupConfiguration, isNew: boolean): Observable<BackupConfiguration> {
    let headers = new Headers({ 'Content-Type': 'application/json' });
    let options = new RequestOptions({ headers: headers });
    if (isNew)//create
      return this.http
        .post('/api/backupconfig', toSave, options)
        .map((res: Response) => res.json())
        .catch(error => Observable.throw(error.text() || 'Server error'));
    else//update
      return this.http
        .put(`/api/backupconfig/${toSave.id}`, toSave, options)
        .map((res: Response) => res.json())
        .catch(error => Observable.throw(error.text() || 'Server error')
        );
  }


  deleteBackup(toDelete: BackupConfiguration): Observable<BackupConfiguration> {
    let headers = new Headers({ 'Content-Type': 'application/json' });
    let options = new RequestOptions({ headers: headers });
    return this.http
        .delete(`/api/backupconfig/${toDelete.id}`, options)
        .map((res: Response) => res.json())
        .catch(error => Observable.throw(error.text() || 'Server error'));
  }


  // ### END REGION BACKUPS ///






  // ### REGION LOGS ///

  getLogsPage(idx: number, siz: number): Observable<PaginatedList<BackupLog>> {
    let headers = new Headers({
      'Content-Type': 'application/json',
      'idx': idx,
      'siz': siz
    });
    let options = new RequestOptions({ headers: headers });
    return this.http
      .get('/api/logs', options)
      .map((res: Response) => res.json())
      .catch(error => Observable.throw(error.text() || 'Server error'));
  }

  // ### END REGION LOGS ///






  // ### REGION USER ///


  loginUser(user: User): Observable<User> {
    let headers = new Headers({
      'Content-Type': 'application/json'
    });
    let options = new RequestOptions({ headers: headers });
    return this.http
      .post('/api/users/login', user, options)
      .map((res: Response) => res.json())
      .catch(error => {
        this.log.error(error);
        return Observable.throw(error || 'Server error')
      }
      );
  }

  registerUser(user: User): Observable<User> {
    let headers = new Headers({
      'Content-Type': 'application/json'
    });
    let options = new RequestOptions({ headers: headers });
    return this.http
      .post('/api/users/register', user, options)
      .map((res: Response) => res.json())
      .catch(error => {
        this.log.error(error);
        return Observable.throw(error || 'Server error')
      }
      );
  }

  changePassword(user: User): Observable<User> {
    let headers = new Headers({
      'Content-Type': 'application/json'
    });
    let options = new RequestOptions({ headers: headers });
    return this.http
      .post('/api/users/pwchange', user, options)
      .map((res: Response) => res.json())
      .catch(error => Observable.throw(error || 'Server error'));
  }


  getAllUsers(): Observable<User[]> {
    let headers = new Headers({
      'Content-Type': 'application/json'
    });
    let options = new RequestOptions({ headers: headers });
    return this.http
      .get('/api/users/list', options)
      .map((res: Response) => res.json())
      .catch(error => Observable.throw(error.text() || 'Server error'));
  }


  changeAdminRole(user: User): Observable<User> {
    let headers = new Headers({
      'Content-Type': 'application/json'
    });
    let options = new RequestOptions({ headers: headers });
    return this.http
      .post('/api/users/list', user, options)
      .map((res: Response) => res.json())
      .catch(error => {
        this.log.error(error);
        return Observable.throw(error || 'Server error')
      }
      );
  }


  // ### END REGION USER ///



  //  ### REGION SIGNALR ///

  getAgentsAll(): Observable<StatusReportInfo[]> {
    return this.http
      .get('/api/lanagents/')
      .map((res: Response) => res.json())
      .catch(error => {
        this.log.error(error);
        return Observable.throw(error || 'Server error')
      });
  }

  //  ### ENDREGION SIGNALR ///

}


import { Component, OnInit, Input, ViewChild } from '@angular/core';
import { BackupConfiguration } from '../../model/BackupConfiguration';
import { SaveResult } from '../../model/SaveResult';
import { PaginatedList } from '../../model/PaginatedList';
import { WebApiService } from '../../services/webapi.service';
import { Observable } from 'rxjs/Observable';
import { AuthenticationService } from '../../services/authentication.service';
import { ModalComponent } from '../../shared/modal/modal.component';

import { LoggerService } from '../../services/logger.service';
import { ToastNotification, ToastType } from '../../services/notifications.service';
import { MessageService, Messages } from '../../services/message.service';


import { GlobalRef } from '../../shared/global/global-ref';


const EDIT_BACKUP_TITLE = "Edit backup configuration";
const NEW_BACKUP_TITLE = "Create new backup configuration";

@Component({
  selector: 'backups',
  templateUrl: './backups.component.html',
  styleUrls: ['./backups.component.css'
  ]
})
export class BackupsComponent implements OnInit {

  @ViewChild('modalEdit')
  public readonly modalEdit: ModalComponent;

  @ViewChild('modalDelete')
  public readonly modalDelete: ModalComponent;

  public list: PaginatedList<BackupConfiguration> = new PaginatedList<BackupConfiguration>(null);

  public EditTitle = NEW_BACKUP_TITLE;
  public editItem: BackupConfiguration = new BackupConfiguration(null);
  private prevSaveItem: BackupConfiguration;


  currentPage: number = 1;
  pageSize: number = 5;
  totalPages: number;
  public isloading = false;
  issaving = true;
  isNew = false;
  adminEmail = '';


  constructor(
    private webApi: WebApiService,
    private _service: AuthenticationService,
    private messageService: MessageService,
    private log: LoggerService,
    private _global: GlobalRef
  ) {
    this.adminEmail = _global.nativeGlobal.clientSettings.admin_email;
  }


  showToast(toast: ToastNotification) {
    this.messageService.broadcast(Messages.MESSAGE_NOTIFY, toast);
  }


  ngOnInit(): void {
    this.doRefresh();
  }


  seeLogs(item: BackupConfiguration) {
    //TODO - show schedule logs
    this.log.debug('see logs ' + item);
  }

  isAdmin() {
    return this._service.checkIsAdmin();
  }

  doDelete(item: BackupConfiguration) {
    if (this.isAdmin()) {
      //
      this.editItem = item;
      this.log.debug('do delete ' + item);
      this.modalDelete.show();
    }
  }

  doDeleteConfirm() {
    //remove from webApi
    this.issaving = true;
    this.webApi.deleteBackup(this.editItem)
      .subscribe(
      (data) => {
        //update list 
        this.updateListLocal(this.editItem, true);
        this.modalDelete.hide();
        this.showToast({ title: "Delete success", body: "Backup configuration was deleted", type: ToastType.success });
        this.getPage(this.currentPage);
        this.issaving = false;
      },
      err => {
        this.issaving = false;
        this.showToast({ title: "Delete error", body: err, type: ToastType.error });
        this.log.error(err);
      }
    );
  }


  doNew() {
    if (this.isAdmin()) {
      this.EditTitle = NEW_BACKUP_TITLE;
      this.editItem = new BackupConfiguration(null);
      this.isNew = true;
      this.modalEdit.show();
    }
  }

  doEdit(item: BackupConfiguration) {
    if (this.isAdmin()) {
      //
      this.EditTitle = EDIT_BACKUP_TITLE;
      this.prevSaveItem = this.getPrevItem(item.id);
      this.editItem = jQuery.extend(true, {}, item);//create deep copy of original edited item
      this.isNew = false;
      this.log.debug('do edit ' + JSON.stringify(item));
      this.modalEdit.show();
    }
  }

  toggleActive(item: BackupConfiguration) {
    if (this.isAdmin()) {
      //
      this.issaving = true;
      this.prevSaveItem = this.getPrevItem(item.id);
      item.isActive = !item.isActive;
      this.log.debug('do toggle active ' + item.isActive);
      var strAction = item.isActive ? "activated" : "disabled";
      //save into webapi
      this.callWebApiSave(item, false,
        "Update succes", `Backup configuration has been ${strAction}!`,
        "Update error", null
      );
    }
  }

  doSave() {
    // call API to save
    this.issaving = true;
    this.log.debug('saving');
    this.callWebApiSave(this.editItem, this.isNew,
      this.isNew ? "Create success" : "Edit success",
      this.isNew ? "The new backup configuration has been added!" : "The backup has been updated!",
      this.isNew ? "Create error" : "Edit error",
      () => {
        this.modalEdit.hide();
      }
    )
  }


  callWebApiSave(item: BackupConfiguration, isNew: boolean,
    titleSucces: string, msgSucces: string,
    titleErr: string, callback) {
    this.webApi.saveBackup(item, isNew)
      .subscribe(
      (data) => {
        //update list 
        if (!isNew) {
          this.updateListLocal(data, false);
        }
        else {
          this.doRefresh();
        }
        if (callback) callback();
        this.showToast({ title: titleSucces, body: msgSucces, type: ToastType.success });
        this.issaving = false;
      },
      err => {
        this.issaving = false;
        this.updateListLocal(this.prevSaveItem, false);
        this.showToast({ title: titleErr, body: err, type: ToastType.error });
        this.log.error(err);
      }
      );
  }

  updateListLocal(data: BackupConfiguration, todelete: boolean) {
    for (let ix = 0; ix < this.list.recs.length; ix++) {
      if (this.list.recs[ix].id == data.id) {
        if (todelete) {
          this.log.debug('local delete');
          this.list.recs.splice(ix, 1);
        }
        else {
          this.log.debug('local replace');
          this.list.recs.splice(ix, 1, data);
        }
        break;
      }
    }
  }

  getPrevItem(id: string) {
    for (let ix = 0; ix < this.list.recs.length; ix++) {
      if (this.list.recs[ix].id == id) {
        return this.list.recs[ix];
      }
    }
    return null;
  }


  doRefresh() {
    this.getPage(this.currentPage);
  }

  getPage(page: number) {
    this.isloading = true;
    this.webApi.getBackupsPage(page, this.pageSize).subscribe(
      (data) => {

        setTimeout(() => {
          this.list = data;
          this.currentPage = page
          this.totalPages = data.tp * this.pageSize;
          this.isloading = false;
        }, 200);//TODO - remove - induced delay

      },
      err => {
        this.log.error(err);
        this.isloading = false;
      }
    );
  }


}

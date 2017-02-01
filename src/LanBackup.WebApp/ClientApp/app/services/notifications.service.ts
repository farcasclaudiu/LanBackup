import { Injectable, EventEmitter } from "@angular/core"
import { Observable } from "rxjs"
import { MessageService, Messages } from './message.service';
import { Subscription } from 'rxjs/Subscription';
import { LoggerService } from './logger.service';
import {
  ToastyService,
  ToastOptions, ToastData, ToastyConfig
} from 'ng2-toasty';



@Injectable()
export class NotificationsService {

  private subscription: Subscription;

  constructor(
    private messageService: MessageService,
    private toastyService: ToastyService,
    private toastConfig: ToastyConfig,
    private log: LoggerService
  ) {
    toastConfig.theme = 'material';
    this.subscription = this.messageService.subscribe(Messages.MESSAGE_NOTIFY, (payload) => {
      this.addToast(payload as ToastNotification);
    });
  }


  addToast(toastnotification: ToastNotification) {
    var toastOptions: ToastOptions = {
      title: toastnotification.title,
      msg: toastnotification.body,
      showClose: true,
      timeout: 5000,
      onAdd: (toast: ToastData) => {
        this.log.debug('Toast ' + toast.id + ' has been added!');
      },
      onRemove: (toast: ToastData) => {
        this.log.debug('Toast ' + toast.id + ' has been removed!');
      }
    };
    //create notification
    switch (toastnotification.type) {
      case ToastType.success:
        this.toastyService.success(toastOptions);
        break;
      case ToastType.error:
        this.toastyService.error(toastOptions);
        break;
      case ToastType.warning:
        this.toastyService.warning(toastOptions);
        break;
      case ToastType.info:
        this.toastyService.info(toastOptions);
        break;
      default:
        this.toastyService.default(toastOptions);
        break;
    }
  }
}


//notification dto
export interface ToastNotification {
  title: string
  body: string
  type: ToastType
}

//notification type enumeration
export enum ToastType {
  normal,
  success,
  warning,
  error,
  info
}
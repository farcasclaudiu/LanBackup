//inspired from
//https://github.com/code-chunks/angular2-logger/blob/master/app/core/logger.ts
import { Injectable } from '@angular/core';

const CONSOLE_DEBUG_METHOD = console["debug"] ? "debug" : "log";

//logging here
@Injectable()
export class LoggerService {

  constructor() {
    this._level = Level.WARN;
  }


  error(message?: any, ...optionalParams: any[]) {
    this.isErrorEnabled() && console.error.apply(console, arguments);
  }

  warn(message?: any, ...optionalParams: any[]) {
    this.isWarnEnabled() && console.warn.apply(console, arguments);
  }

  info(message?: any, ...optionalParams: any[]) {
    this.isInfoEnabled() && console.info.apply(console, arguments);
  }

  debug(message?: any, ...optionalParams: any[]) {
    this.isDebugEnabled() && (<any>console)[CONSOLE_DEBUG_METHOD].apply(console, arguments);
  }

  log(message?: any, ...optionalParams: any[]) {
    this.isLogEnabled() && console.log.apply(console, arguments);
  }


  private _level: Level;
  public Level: any = Level;

  get level(): Level { return this._level; }

  set level(level: Level) {
    this._level = level;
  }

  isErrorEnabled = (): boolean => this._level >= Level.ERROR;
  isWarnEnabled = (): boolean => this._level >= Level.WARN;
  isInfoEnabled = (): boolean => this._level >= Level.INFO;
  isDebugEnabled = (): boolean => this._level >= Level.DEBUG;
  isLogEnabled = (): boolean => this._level >= Level.LOG;
  
}

export enum Level { OFF = 0, ERROR = 1, WARN = 2, INFO = 3, DEBUG = 4, LOG = 5 }

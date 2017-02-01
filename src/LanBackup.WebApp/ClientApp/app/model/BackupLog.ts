export class BackupLog {
  iD: string;
  clientIP: string;
  configurationID: string;
  description: string;
  logError: string;
  status: string;
  dateTime: Date;

  constructor(data) {
    Object.assign(this, data);
  }
}
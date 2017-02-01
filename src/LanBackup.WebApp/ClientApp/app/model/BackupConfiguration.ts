export class BackupConfiguration {
  id: string;
  clientIP: string;
  srcFolder: string;
  srcUser: string;
  srcPass: string;
  destLanFolder: string;
  destUser: string;
  destPass: string;
  crontab: string;
  isActive: boolean;
  rowVersion: string;

  constructor(data) {
    Object.assign(this, data);
  }
}
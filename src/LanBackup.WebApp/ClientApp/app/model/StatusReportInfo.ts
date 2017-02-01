export class StatusReportInfo {

  iP: string;
  configurationId: string;
  statusType: number;
  statusDescription: string;
  statusPercent: number;
  statusDateTime: Date;

  constructor(data) {
    Object.assign(this, data);
  }
}
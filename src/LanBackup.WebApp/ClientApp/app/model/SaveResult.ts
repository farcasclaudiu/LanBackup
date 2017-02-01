export class SaveResult<T> {
  _body: T;
  status: string;
  ok: string;
  statusText: string;

  constructor(data) {
    Object.assign(this, data);
  }
}
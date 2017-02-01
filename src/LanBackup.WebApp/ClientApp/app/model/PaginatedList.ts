export class PaginatedList<T> {
  pi: number;
  tp: number;
  recs: T[];
  hp: boolean;
  hn: boolean;

  constructor(data) {
    Object.assign(this, data);
  }
}
/**
 * The default template and styles for the pagination links are borrowed directly
 * from Zurb Foundation 6: http://foundation.zurb.com/sites/docs/pagination.html
 */

export const DEFAULT_TEMPLATE = `
    <pagination-template  #p="paginationApi"
                         [id]="id"
                         [maxSize]="maxSize"
                         (pageChange)="pageChange.emit($event)">
    <ul class="pagination" 
        role="navigation" 
        [attr.aria-label]="screenReaderPaginationLabel" 
        *ngIf="!(autoHide && p.pages.length <= 1)">

        <li [class.disabled]="p.isFirstPage()" *ngIf="directionLinks"> 
            <a (click)="p.previous()" [attr.aria-label]="previousLabel + ' ' + screenReaderPageLabel">
                {{ previousLabel }}
            </a>
        </li>

        <li [class.active]="p.getCurrent() === page.value" *ngFor="let page of p.pages">
            <a (click)="p.setCurrent(page.value)">
                <span>{{ page.label }}</span>
            </a>
        </li>

        <li class="pagination-next" [class.disabled]="p.isLastPage()" *ngIf="directionLinks">
            <a (click)="p.next()" [attr.aria-label]="nextLabel + ' ' + screenReaderPageLabel">
                 {{ nextLabel }}
            </a>
        </li>

    </ul>
    </pagination-template>
    `;

export const DEFAULT_STYLES = `
.ng2-pagination {
  margin-left: 0;
  margin-bottom: 1rem; }
  .ng2-pagination::before, .ng2-pagination::after {
    content: ' ';
    display: table; }
  .ng2-pagination::after {
    clear: both; }
  .ng2-pagination li {
    -moz-user-select: none;
    -webkit-user-select: none;
    -ms-user-select: none;
    margin-right: 0.0625rem;
    cursor: pointer;
    border-radius: 0; }
  .ng2-pagination li {
    cursor: pointer;
    display: inline-block; }
  .ng2-pagination a,
  .ng2-pagination button {
    color: #0a0a0a; 
    display: block;
    padding: 0.1875rem 0.625rem;
    border-radius: 0; }
    .ng2-pagination a:hover,
    .ng2-pagination button:hover {
      background: #e6e6e6; }
  .ng2-pagination .current {
    padding: 0.1875rem 0.625rem;
    background: #2199e8;
    color: #fefefe;
    cursor: default; }
  .ng2-pagination .disabled {
    padding: 0.1875rem 0.625rem;
    color: #cacaca;
    cursor: default; } 
    .ng2-pagination .disabled:hover {
      background: transparent; }
  .ng2-pagination .ellipsis::after {
    content: '…';
    padding: 0.1875rem 0.625rem;
    color: #0a0a0a; }

.ng2-pagination .pagination-previous a::before,
.ng2-pagination .pagination-previous.disabled::before { 
  content: '«';
  display: inline-block;
  margin-right: 0.5rem; }

.ng2-pagination .pagination-next a::after,
.ng2-pagination .pagination-next.disabled::after {
  content: '»';
  display: inline-block;
  margin-left: 0.5rem; }

.ng2-pagination .show-for-sr {
  position: absolute !important;
  width: 1px;
  height: 1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0); }`;

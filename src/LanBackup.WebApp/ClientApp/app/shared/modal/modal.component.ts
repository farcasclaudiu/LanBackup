﻿import { Component } from '@angular/core';

@Component({
  selector: 'app-modal',
  template: `
  <div class="modal fade-scale" tabindex="-1" [ngClass]="{'in': visible}"
       [ngStyle]="{'display': visible ? 'block' : 'none', 'opacity': visibleAnimate ? 1 : 0}" role="dialog">
    <div class="vertical-alignment-helper"> 
    <div class="modal-dialog vertical-align-center">
      <div class="modal-content">
        <div class="modal-header">
          <ng-content select=".app-modal-header"></ng-content>
        </div>
        <div class="modal-body">
          <ng-content select=".app-modal-body"></ng-content>
        </div>
        <div class="modal-footer">
          <ng-content select=".app-modal-footer"></ng-content>
        </div>
      </div>
    </div>
    </div>
  </div>
  `,
  styleUrls: ['./modal.component.css']
})
export class ModalComponent {

  public visible = false;
  private visibleAnimate = false;

  public show(): void {
    this.visible = true;
    setTimeout(() => this.visibleAnimate = true);
  }

  public hide(): void {
    this.visibleAnimate = false;
    setTimeout(() => this.visible = false, 300);
  }
}
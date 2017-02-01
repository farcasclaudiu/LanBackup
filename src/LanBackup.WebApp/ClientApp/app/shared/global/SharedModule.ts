
import { NgModule, ModuleWithProviders } from '@angular/core';
import { GlobalRef, BrowserGlobalRef, NodeGlobalRef } from './global-ref';
@NgModule({})
export class SharedModule {

  static forBrowser(): ModuleWithProviders {
    return {
      ngModule: SharedModule,
      providers: [
        { provide: GlobalRef, useClass: BrowserGlobalRef }
      ]
    };
  }

  static forNode(): ModuleWithProviders {
    return {
      ngModule: SharedModule,
      providers: [
        { provide: GlobalRef, useClass: NodeGlobalRef }
      ]
    };
  }
}
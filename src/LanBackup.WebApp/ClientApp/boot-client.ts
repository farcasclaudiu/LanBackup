import 'angular2-universal-polyfills/browser';
import { enableProdMode } from '@angular/core';
import { platformUniversalDynamic } from 'angular2-universal';
import { AppModule } from './app/app.module';

import * as $ from 'jquery';
import 'bootstrap';
//import { AppInsightsModule, AppInsightsService } from 'ng2-appinsights';

// Enable either Hot Module Reloading or production mode
if (module['hot']) {
    module['hot'].accept();
    module['hot'].dispose(() => { platform.destroy(); });
} else {
    enableProdMode();
}

// Boot the application, either now or when the DOM content is loaded
const platform = platformUniversalDynamic();
const bootApplication = () => { platform.bootstrapModule(AppModule); };
if (document.readyState === 'complete') {
    bootApplication();
} else {
    document.addEventListener('DOMContentLoaded', bootApplication);
}

//var appIns: AppInsightsService = new AppInsightsService('');
//appIns.Init({
//  instrumentationKey: '9f667285-ff2c-4626-a8da-1db518593323' //TODO - MOVE INTO A CONFIG FILE THAT SHOULD NOT BE COMMITED
//});
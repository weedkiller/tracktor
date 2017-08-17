import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppModuleShared } from './app.module.shared';
import { IAppSettings } from './app.service';
import { AppComponent } from './components/app/app.component';

@NgModule({
    bootstrap: [ AppComponent ],
    imports: [
        BrowserModule,
        AppModuleShared
    ],
    providers: [
        { provide: 'BASE_URL', useFactory: getBaseUrl },
        { provide: 'APP_SETTINGS', useFactory: getAppSettings },
    ]
})
export class AppModule {
}

export function getBaseUrl() {
    return document.getElementsByTagName('base')[0].href;
}

declare const APP_SETTINGS: IAppSettings;

export function getAppSettings() {
    return APP_SETTINGS;
}

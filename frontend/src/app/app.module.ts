import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { GoogleTagManagerComponent } from './components/google-tag-manager.component';
import { ChartModule } from 'primeng/chart';
import { PanelModule } from 'primeng/panel';
import { HttpClientModule } from '@angular/common/http';

@NgModule({
  declarations: [AppComponent, GoogleTagManagerComponent],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    AppRoutingModule,
    HttpClientModule,
    ChartModule,
    PanelModule,
  ],
  providers: [],
  bootstrap: [AppComponent],
})
export class AppModule {}

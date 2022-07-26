import { ErrorHandler, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { GoogleTagManagerComponent } from './components/google-tag-manager.component';
import { ChartModule } from 'primeng/chart';
import { PanelModule } from 'primeng/panel';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { HttpClientModule } from '@angular/common/http';
import { InsightsService } from './services/insights.service';
import { ApplicationinsightsAngularpluginErrorService } from '@microsoft/applicationinsights-angularplugin-js';

@NgModule({
  declarations: [AppComponent, GoogleTagManagerComponent],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    AppRoutingModule,
    HttpClientModule,
    ChartModule,
    PanelModule,
    ProgressSpinnerModule,
  ],
  providers: [
    InsightsService,
    {
      provide: ErrorHandler,
      useClass: ApplicationinsightsAngularpluginErrorService,
    },
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}

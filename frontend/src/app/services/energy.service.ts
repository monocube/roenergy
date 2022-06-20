import { Injectable, isDevMode } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { IEnergyData } from '../models/energy-data';

@Injectable()
export class EnergyService {
  private readonly apiURLLive = 'https://roenergy.monocube.com/api/energy';
  private readonly apiURLDebug = 'http://localhost:7071/api/energy';
  constructor(private http: HttpClient) {}
  getData() {
    return this.http.get<IEnergyData[]>(isDevMode()? this.apiURLDebug : this.apiURLLive);
  }
}

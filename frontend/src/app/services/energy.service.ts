import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { IEnergyData } from '../models/energy-data';
import { environment } from 'src/environments/environment';

@Injectable()
export class EnergyService {
  constructor(private http: HttpClient) {}
  getData() {
    return this.http.get<IEnergyData[]>(environment.apiURL);
  }
}

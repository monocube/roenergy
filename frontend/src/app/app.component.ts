import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { SeverityLevel } from '@microsoft/applicationinsights-web';
import { IEnergyData } from './models/energy-data';
import { EnergyService } from './services/energy.service';
import { InsightsService } from './services/insights.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  providers: [EnergyService, DatePipe],
})
export class AppComponent implements OnInit {
  title = 'RoEnergy';
  private readonly pallete = [
    '#a6cee3',
    '#1f78b4',
    '#b2df8a',
    '#33a02c',
    '#fb9a99',
    '#e31a1c',
    '#fdbf6f',
    '#ff7f00',
    '#cab2d6',
    '#6a3d9a',
    '#ffff99',
    '#b15928',
  ];

  energyData: any = null;
  options: any;
  totalGrowth: number = 0;

  constructor(
    private energyService: EnergyService,
    private insightsService: InsightsService,
    private datepipe: DatePipe
  ) {
    this.options = {
      plugins: {
        tooltip: {
          callbacks: {
            label: (item: any) =>
              `${item.dataset.label}: ${item.raw.toFixed(3)} MW`,
          },
        },
      },
    };
  }

  ngOnInit() {
    this.energyService.getData().subscribe({
      next: (data) => {
        this.processData(data);
      },
      error: (error: HttpErrorResponse) => {
        this.insightsService.trackTrace({
          message: error.message,
          severityLevel: SeverityLevel.Error,
        });
      },
    });
  }

  private processData(data: IEnergyData[]) {
    let chartLabels: any = [];
    let datasets = new Map<string, number[]>();
    const chartDatasets: any = [
      {
        label: 'Total',
        data: [],
        borderColor: this.pallete[0],
        tension: 0,
      },
    ];
    let initialTotalCapacity = 0;
    data.map((energyData, index, energyDataArray) => {
      let totalCapacity: number = 0;
      chartLabels.push(
        this.datepipe.transform(new Date(energyData.date), 'dd-MMM-yyyy')
      );
      energyData.sources.map((source) => {
        totalCapacity += source.capacity;
        let capacities: any = [];
        if (datasets.has(source.name)) {
          capacities = datasets.get(source.name)!;
        }
        capacities.push(source.capacity);
        datasets.set(source.name, capacities);
      });
      chartDatasets[0].data.push(totalCapacity);
      if (index === 0) {
        initialTotalCapacity = totalCapacity;
      } else if (index === energyDataArray.length - 1) {
        this.totalGrowth =
          (totalCapacity - initialTotalCapacity) / initialTotalCapacity;
      }
    });
    [...datasets.entries()].forEach(([key, value], index) => {
      chartDatasets.push({
        label: key,
        data: value,
        borderColor: this.pallete[index + 1],
        tension: 0,
      });
    });
    this.energyData = {
      labels: chartLabels,
      datasets: chartDatasets,
    };
  }
}

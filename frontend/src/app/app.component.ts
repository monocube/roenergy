import { DatePipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { EnergyService } from './services/energy.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  providers: [EnergyService, DatePipe],
})
export class AppComponent implements OnInit {
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

  energyData: any;
  options: any;
  constructor(private energyService: EnergyService, public datepipe: DatePipe) {
    this.options = {
      plugins: {
        tooltip: {
          callbacks: {
            label: (item: any) => `${item.dataset.label}: ${item.raw.toFixed(3)} MW`,
          },
        },
      },
    };
  }
  ngOnInit() {
    this.energyService.getData().subscribe((data) => {
      let chartLabels: any = [];
      let datasets = new Map<string, number[]>();
      let totalCapacity: number = 0;
      data.map((energyData) => {
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
      });
      const chartDatasets: any = [
        {
          label: 'Total',
          data: [totalCapacity],
          borderColor: this.pallete[0],
          tension: 0.4,
        },
      ];
      [...datasets.entries()].forEach(([key, value], index) => {
        chartDatasets.push({
          label: key,
          data: value,
          borderColor: this.pallete[index + 1],
          tension: 0.4,
        });
      });
      this.energyData = {
        labels: chartLabels,
        datasets: chartDatasets,
      };
    });
  }
}

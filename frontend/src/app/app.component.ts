import { DatePipe } from '@angular/common';
import { Component, OnInit, ViewChild } from '@angular/core';
import { EnergyService } from './services/energy.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  providers: [EnergyService, DatePipe],
})
export class AppComponent implements OnInit {
  @ViewChild('chart') chart: any;

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
  constructor(private energyService: EnergyService, public datepipe: DatePipe) {}
  ngOnInit() {
    this.energyService.getData().subscribe((data) => {
      let chartLabels: any = [];
      let datasets = new Map<string, Number[]>();
      data.map((energyData) => {
        chartLabels.push(this.datepipe.transform(new Date(energyData.date), 'dd-MMM-yyyy'));
        energyData.sources.map((source) => {
          let capacities = [];
          if (datasets.has(source.name)) {
            capacities = datasets.get(source.name)!;
            capacities.push(source.capacity);
            datasets.set(source.name, capacities);
          } else {
            capacities.push(source.capacity);
            datasets.set(source.name, capacities);
          }
        });
      });
      const chartDatasets: any = [];
      [...datasets.entries()].forEach(([key, value], index) => {
        chartDatasets.push({
          label: key,
          data: value,
          fill: false,
          borderColor: this.pallete[index],
          tension: 0.4,
        });
      });
      this.energyData = {
        labels: chartLabels,
        datasets: chartDatasets,
      };
      this.chart.refresh();
    });
  }
}

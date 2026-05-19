import { Component, inject, PLATFORM_ID, signal, afterNextRender } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ChartComponent } from 'ng-apexcharts';
import type {
  ApexAxisChartSeries,
  ApexChart,
  ApexDataLabels,
  ApexFill,
  ApexGrid,
  ApexLegend,
  ApexPlotOptions,
  ApexStroke,
  ApexTooltip,
  ApexXAxis,
  ApexNonAxisChartSeries
} from 'ng-apexcharts';

type ChartTab = 'daily' | 'weekly' | 'monthly';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, ChartComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent {
  private readonly platformId = inject(PLATFORM_ID);
  userName = '';

  /** Apex runs only in the browser (avoids SSR/prerender issues). */
  chartsReady = signal(false);

  chartTab = signal<ChartTab>('daily');

  readonly appointmentDays = [
    { dow: 'Sat', day: 9, active: false },
    { dow: 'Sun', day: 10, active: false },
    { dow: 'Mon', day: 11, active: true },
    { dow: 'Tue', day: 12, active: false },
    { dow: 'Wed', day: 13, active: false },
    { dow: 'Thu', day: 14, active: false },
    { dow: 'Fri', day: 15, active: false }
  ];

  readonly todayAppointments = [
    { name: 'Bob Johnson', type: 'Routine Check-Up', doctor: 'Dr. Sharma', time: '09:30 AM' },
    { name: 'Maria Garcia', type: 'Follow-up', doctor: 'Dr. Lee', time: '10:15 AM' },
    { name: 'James Wilson', type: 'Task', doctor: 'Dr. Sharma', time: '11:00 AM' },
    { name: 'Anna Müller', type: 'Consultation', doctor: 'Dr. Patel', time: '02:00 PM' }
  ];

  readonly calendarWeeks: (number | null)[][] = [
    [null, null, null, 1, 2, 3, 4],
    [5, 6, 7, 8, 9, 10, 11],
    [12, 13, 14, 15, 16, 17, 18],
    [19, 20, 21, 22, 23, 24, 25],
    [26, 27, 28, 29, 30, 31, null]
  ];

  readonly flatCalendarCells = this.calendarWeeks.flatMap((week, wi) =>
    week.map((cell, ci) => ({
      cell,
      id: `${wi}-${ci}`
    }))
  );

  /** Sparklines — Cliniva-style KPI row */
  sparkAppointments: ApexAxisChartSeries = [
    { name: 'a', data: [12, 18, 14, 22, 19, 28, 32, 26, 35, 40, 38, 45] }
  ];
  sparkOperations: ApexAxisChartSeries = [
    { name: 'o', data: [4, 6, 5, 8, 7, 9, 11, 10, 12, 14, 13, 15] }
  ];
  sparkPatients: ApexAxisChartSeries = [
    { name: 'p', data: [8, 10, 14, 11, 16, 18, 22, 20, 24, 28, 26, 30] }
  ];
  sparkEarning: ApexAxisChartSeries = [
    { name: 'e', data: [120, 190, 160, 210, 240, 280, 260, 300, 320, 340, 360, 380] }
  ];

  sparkChartBase: ApexChart = {
    type: 'area',
    height: 64,
    sparkline: { enabled: true },
    toolbar: { show: false },
    animations: { enabled: true }
  };

  sparkStroke: ApexStroke = { curve: 'smooth', width: 2 };
  sparkDataLabels: ApexDataLabels = { enabled: false };
  sparkTooltip: ApexTooltip = {
    fixed: { enabled: false },
    x: { show: false },
    marker: { show: false }
  };

  sparkFillPurple: ApexFill = {
    type: 'gradient',
    gradient: {
      shadeIntensity: 1,
      opacityFrom: 0.45,
      opacityTo: 0.05,
      stops: [0, 90, 100],
      colorStops: [
        { offset: 0, color: '#7c3aed', opacity: 1 },
        { offset: 100, color: '#7c3aed', opacity: 0.1 }
      ]
    }
  };

  sparkFillOrange: ApexFill = {
    type: 'gradient',
    gradient: {
      shadeIntensity: 1,
      opacityFrom: 0.45,
      opacityTo: 0.05,
      stops: [0, 90, 100],
      colorStops: [
        { offset: 0, color: '#ea580c', opacity: 1 },
        { offset: 100, color: '#ea580c', opacity: 0.1 }
      ]
    }
  };

  sparkFillGreen: ApexFill = {
    type: 'gradient',
    gradient: {
      shadeIntensity: 1,
      opacityFrom: 0.45,
      opacityTo: 0.05,
      stops: [0, 90, 100],
      colorStops: [
        { offset: 0, color: '#16a34a', opacity: 1 },
        { offset: 100, color: '#16a34a', opacity: 0.1 }
      ]
    }
  };

  sparkFillBlue: ApexFill = {
    type: 'gradient',
    gradient: {
      shadeIntensity: 1,
      opacityFrom: 0.45,
      opacityTo: 0.05,
      stops: [0, 90, 100],
      colorStops: [
        { offset: 0, color: '#2563eb', opacity: 1 },
        { offset: 100, color: '#2563eb', opacity: 0.1 }
      ]
    }
  };

  sparkColorsPurple = ['#7c3aed'];
  sparkColorsOrange = ['#ea580c'];
  sparkColorsGreen = ['#16a34a'];
  sparkColorsBlue = ['#2563eb'];

  /** Donut — Patient chart */
  patientSeries: ApexNonAxisChartSeries = [22, 15, 20, 18];
  patientLabels = ['Dengue', 'Typhoid', 'Malaria', 'Cold'];
  patientChart: ApexChart = {
    type: 'donut',
    height: 280,
    width: '100%',
    fontFamily: 'Roboto, system-ui, sans-serif',
    toolbar: { show: false }
  };
  patientColors = ['#8b5cf6', '#6366f1', '#f59e0b', '#22c55e'];
  patientPlot: ApexPlotOptions = {
    pie: {
      donut: {
        size: '72%',
        labels: {
          show: true,
          name: { show: true, fontSize: '14px' },
          value: { fontSize: '22px', fontWeight: 600 },
          total: {
            show: true,
            showAlways: true,
            label: 'Total People',
            fontSize: '13px',
            color: '#6b7280',
            formatter: () => '75'
          }
        }
      }
    }
  };
  patientLegend: ApexLegend = {
    position: 'bottom',
    fontSize: '12px',
    markers: { size: 6, strokeWidth: 0 },
    itemMargin: { horizontal: 10, vertical: 4 }
  };
  patientDataLabels: ApexDataLabels = { enabled: false };

  /** Hospital survey — line */
  surveySeries: ApexAxisChartSeries = [
    { name: 'New Patients', data: [28, 35, 42, 38, 48, 52, 45, 58, 62, 55, 60, 68] },
    { name: 'Old Patients', data: [45, 42, 48, 44, 50, 46, 52, 49, 54, 50, 56, 53] }
  ];
  surveyChart: ApexChart = {
    type: 'line',
    height: 300,
    width: '100%',
    fontFamily: 'Roboto, system-ui, sans-serif',
    toolbar: { show: false },
    zoom: { enabled: false }
  };
  surveyStroke: ApexStroke = { curve: 'smooth', width: 3 };
  surveyColors = ['#3b82f6', '#fb923c'];
  surveyXaxis: ApexXAxis = {
    categories: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
    labels: { style: { colors: '#9ca3af', fontSize: '11px' } },
    axisBorder: { show: false },
    axisTicks: { show: false }
  };
  surveyGrid: ApexGrid = {
    borderColor: '#f3f4f6',
    strokeDashArray: 4,
    padding: { top: 8, right: 8 }
  };
  surveyLegend: ApexLegend = {
    position: 'top',
    horizontalAlign: 'right',
    fontSize: '12px',
    markers: { size: 5 }
  };
  surveyTooltip: ApexTooltip = { theme: 'light', x: { show: true } };

  constructor() {
    if (isPlatformBrowser(this.platformId)) {
      this.userName = localStorage.getItem('pms_user_name') || 'User';
    }
    afterNextRender(() => this.chartsReady.set(true));
  }

  setChartTab(tab: ChartTab): void {
    this.chartTab.set(tab);
    const sets: Record<ChartTab, number[]> = {
      daily: [22, 15, 20, 18],
      weekly: [18, 20, 24, 16],
      monthly: [30, 18, 22, 14]
    };
    this.patientSeries = [...sets[tab]];
  }
}

import { Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-broker-dashboard',
  imports: [RouterLink],
  templateUrl: './broker-dashboard.html',
  styleUrl: './broker-dashboard.scss',
})
export class BrokerDashboard {
  stats = signal([
    { label: 'Managed Agencies',  value: '6',       change: 'Across 4 states',       direction: 'neutral' },
    { label: 'Total GWP (YTD)',   value: '$4.2M',   change: '+22% vs last year',      direction: 'up'     },
    { label: 'Commission YTD',    value: '$378K',   change: '+18% vs last year',      direction: 'up'     },
    { label: 'Active Policies',   value: '312',     change: '+28 this quarter',       direction: 'up'     },
  ]);

  topAgencies = signal([
    { name: 'Mid-Atlantic Group', gwp: '$1.42M', policies: 124, commission: '$127.8K' },
    { name: 'Bayview Insurance',  gwp: '$1.18M', policies: 98,  commission: '$106.2K' },
    { name: 'Coastal Partners',   gwp: '$0.92M', policies: 67,  commission: '$82.8K'  },
    { name: 'Northstar Agency',   gwp: '$0.68M', policies: 23,  commission: '$61.2K'  },
  ]);
}

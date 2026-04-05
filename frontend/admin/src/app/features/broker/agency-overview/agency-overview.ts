import { Component, signal } from '@angular/core';

interface Agency {
  id: string;
  name: string;
  state: string;
  agents: number;
  gwp: string;
  policies: number;
  commission: string;
  growth: string;
  status: 'Active' | 'Under Review' | 'Suspended';
}

@Component({
  selector: 'app-agency-overview',
  templateUrl: './agency-overview.html',
  styleUrl: './agency-overview.scss',
})
export class AgencyOverview {
  agencies = signal<Agency[]>([
    { id: '1', name: 'Mid-Atlantic Group',   state: 'PA', agents: 24, gwp: '$1.42M', policies: 124, commission: '$127.8K', growth: '+24%', status: 'Active' },
    { id: '2', name: 'Bayview Insurance',    state: 'MD', agents: 18, gwp: '$1.18M', policies: 98,  commission: '$106.2K', growth: '+18%', status: 'Active' },
    { id: '3', name: 'Coastal Partners',     state: 'VA', agents: 12, gwp: '$0.92M', policies: 67,  commission: '$82.8K',  growth: '+11%', status: 'Active' },
    { id: '4', name: 'Northstar Agency',     state: 'NJ', agents: 7,  gwp: '$0.68M', policies: 23,  commission: '$61.2K',  growth: '+8%',  status: 'Active' },
    { id: '5', name: 'Keystone Assurance',   state: 'PA', agents: 4,  gwp: '$0.08M', policies: 12,  commission: '$7.2K',   growth: '-2%',  status: 'Under Review' },
    { id: '6', name: 'Old Dominion Brokers', state: 'VA', agents: 0,  gwp: '$0.00M', policies: 0,   commission: '$0',      growth: 'N/A',  status: 'Suspended' },
  ]);

  statusBadge(s: Agency['status']): string {
    return s === 'Active' ? 'badge--success' : s === 'Suspended' ? 'badge--danger' : 'badge--warning';
  }
}

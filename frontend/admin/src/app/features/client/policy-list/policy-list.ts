import { Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

interface Policy {
  id: string;
  type: string;
  coverage: string;
  premium: string;
  deductible: string;
  effective: string;
  expiry: string;
  insurer: string;
  status: 'Active' | 'Expired' | 'Cancelled';
}

@Component({
  selector: 'app-policy-list',
  imports: [RouterLink],
  templateUrl: './policy-list.html',
  styleUrl: './policy-list.scss',
})
export class PolicyList {
  policies = signal<Policy[]>([
    {
      id: 'POL-2026-4421', type: 'Fleet Auto Insurance',
      coverage: '$2,500,000 CSL',  premium: '$285,000/yr', deductible: '$5,000',
      effective: 'May 1, 2026', expiry: 'May 1, 2027',
      insurer: 'SampleTech Underwriters', status: 'Active'
    },
    {
      id: 'POL-2026-4308', type: 'Cyber Liability',
      coverage: '$5,000,000',       premium: '$67,000/yr',  deductible: '$25,000',
      effective: 'Jun 15, 2026', expiry: 'Jun 15, 2027',
      insurer: 'SampleTech Underwriters', status: 'Active'
    },
    {
      id: 'POL-2025-3892', type: 'Workers Compensation',
      coverage: 'Statutory',        premium: '$42,000/yr',  deductible: 'N/A',
      effective: 'Apr 1, 2025', expiry: 'Apr 1, 2027',
      insurer: 'SampleTech Underwriters', status: 'Active'
    },
    {
      id: 'POL-2024-3210', type: 'Commercial Property',
      coverage: '$4,500,000',       premium: '$62,000/yr',  deductible: '$10,000',
      effective: 'Mar 1, 2024', expiry: 'Mar 1, 2025',
      insurer: 'SampleTech Underwriters', status: 'Expired'
    },
  ]);

  statusBadge(s: Policy['status']): string {
    return s === 'Active' ? 'badge--success' : s === 'Expired' ? 'badge--neutral' : 'badge--danger';
  }
}

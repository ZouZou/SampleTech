import { Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-client-dashboard',
  imports: [RouterLink],
  templateUrl: './client-dashboard.html',
  styleUrl: './client-dashboard.scss',
})
export class ClientDashboard {
  policies = signal([
    { id: 'POL-2026-4421', type: 'Fleet Auto Insurance',        coverage: '$2,500,000', premium: '$285,000/yr', expiry: 'May 1, 2027',  status: 'Active' },
    { id: 'POL-2026-4308', type: 'Cyber Liability',             coverage: '$5,000,000', premium: '$67,000/yr',  expiry: 'Jun 15, 2027', status: 'Active' },
    { id: 'POL-2025-3892', type: 'Workers Compensation',        coverage: 'Statutory',   premium: '$42,000/yr',  expiry: 'Apr 1, 2027',  status: 'Active' },
  ]);

  upcomingRenewals = signal([
    { id: 'POL-2025-3892', type: 'Workers Compensation', expiry: 'Apr 1, 2027', daysLeft: 362 },
  ]);
}

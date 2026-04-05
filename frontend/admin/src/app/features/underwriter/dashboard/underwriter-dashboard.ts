import { Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-underwriter-dashboard',
  imports: [RouterLink],
  templateUrl: './underwriter-dashboard.html',
  styleUrl: './underwriter-dashboard.scss',
})
export class UnderwriterDashboard {
  stats = signal([
    { label: 'New Submissions',    value: '18',     change: '+4 today',          direction: 'up'     },
    { label: 'Pending Review',     value: '42',     change: '7 due today',       direction: 'neutral' },
    { label: 'Quoted This Month',  value: '127',    change: '+14% vs last month', direction: 'up'     },
    { label: 'Avg. Decision Time', value: '1.8d',   change: '-0.3d improvement', direction: 'down'   },
  ]);

  urgentItems = signal([
    { id: 'SUB-2026-0899', name: 'Westbrook Manufacturing', type: 'Commercial Property', premium: '$84,000', due: 'Today 5:00 PM' },
    { id: 'SUB-2026-0897', name: 'Harbor Logistics LLC',    type: 'Marine Cargo',         premium: '$32,500', due: 'Today 6:00 PM' },
    { id: 'SUB-2026-0895', name: 'Summit Healthcare Group', type: 'Medical Malpractice',   premium: '$210,000',due: 'Tomorrow 9:00 AM' },
  ]);
}

import { Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-agent-dashboard',
  imports: [RouterLink],
  templateUrl: './agent-dashboard.html',
  styleUrl: './agent-dashboard.scss',
})
export class AgentDashboard {
  stats = signal([
    { label: 'Active Clients',       value: '84',    change: '+3 this month',     direction: 'up'     },
    { label: 'Open Quotes',          value: '12',    change: '5 awaiting decision',direction: 'neutral' },
    { label: 'Bound This Month',     value: '7',     change: '+2 vs last month',   direction: 'up'     },
    { label: 'Commission YTD',       value: '$43.2K', change: '+18% vs last year', direction: 'up'     },
  ]);

  recentQuotes = signal([
    { id: 'SUB-2026-0901', client: 'Apex Retail Holdings',       type: 'Commercial Property', status: 'New',         submitted: '2 hours ago' },
    { id: 'SUB-2026-0898', client: 'Pacific Rim Trading Co.',    type: 'Marine Cargo',        status: 'In Review',   submitted: '1 day ago'   },
    { id: 'SUB-2026-0893', client: 'Metro Transit Authority',    type: 'Fleet Auto',          status: 'Quoted',      submitted: '3 days ago'  },
    { id: 'SUB-2026-0884', client: 'Pioneer Tech Solutions',     type: 'Cyber Liability',     status: 'Bound',       submitted: '1 week ago'  },
  ]);

  statusBadge(status: string): string {
    const map: Record<string, string> = {
      New:        'badge--info',
      'In Review':'badge--warning',
      Quoted:     'badge--neutral',
      Bound:      'badge--success',
      Declined:   'badge--danger',
    };
    return map[status] ?? 'badge--neutral';
  }
}

import { Component, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';

interface CommissionRow {
  id: string;
  agency: string;
  policy: string;
  client: string;
  line: string;
  premium: number;
  commissionRate: number;
  earned: number;
  month: string;
  status: 'Paid' | 'Pending' | 'Processing';
}

@Component({
  selector: 'app-commission-summary',
  imports: [FormsModule],
  templateUrl: './commission-summary.html',
  styleUrl: './commission-summary.scss',
})
export class CommissionSummary {
  filterAgency = signal('all');
  filterStatus = signal('all');

  rows = signal<CommissionRow[]>([
    { id: '1',  agency: 'Mid-Atlantic Group',  policy: 'POL-2026-4421', client: 'Metro Transit Authority',    line: 'Fleet Auto',           premium: 285000, commissionRate: 9,  earned: 25650, month: 'Mar 2026', status: 'Paid'       },
    { id: '2',  agency: 'Bayview Insurance',   policy: 'POL-2026-4398', client: 'Summit Healthcare Group',   line: 'Medical Malpractice',  premium: 210000, commissionRate: 9,  earned: 18900, month: 'Mar 2026', status: 'Paid'       },
    { id: '3',  agency: 'Coastal Partners',    policy: 'POL-2026-4375', client: 'Apex Retail Holdings',      line: 'Commercial Property',  premium: 125000, commissionRate: 10, earned: 12500, month: 'Mar 2026', status: 'Processing' },
    { id: '4',  agency: 'Mid-Atlantic Group',  policy: 'POL-2026-4362', client: 'Greenfield Solar Energy',   line: 'Construction AR',      premium: 195000, commissionRate: 8,  earned: 15600, month: 'Mar 2026', status: 'Pending'    },
    { id: '5',  agency: 'Bayview Insurance',   policy: 'POL-2026-4341', client: 'Westbrook Manufacturing',   line: 'Commercial Property',  premium: 84000,  commissionRate: 9,  earned: 7560,  month: 'Mar 2026', status: 'Paid'       },
    { id: '6',  agency: 'Northstar Agency',    policy: 'POL-2026-4320', client: 'Pacific Rim Trading Co.',   line: 'Marine Cargo',         premium: 58000,  commissionRate: 12, earned: 6960,  month: 'Feb 2026', status: 'Paid'       },
    { id: '7',  agency: 'Coastal Partners',    policy: 'POL-2026-4308', client: 'Pioneer Tech Solutions',    line: 'Cyber Liability',      premium: 67000,  commissionRate: 12, earned: 8040,  month: 'Feb 2026', status: 'Paid'       },
    { id: '8',  agency: 'Mid-Atlantic Group',  policy: 'POL-2026-4291', client: 'BrightPath Education Inc.', line: 'General Liability',    premium: 12000,  commissionRate: 10, earned: 1200,  month: 'Feb 2026', status: 'Paid'       },
  ]);

  filtered = computed(() => {
    let list = this.rows();
    if (this.filterAgency() !== 'all') list = list.filter(r => r.agency === this.filterAgency());
    if (this.filterStatus() !== 'all') list = list.filter(r => r.status === this.filterStatus());
    return list;
  });

  totalEarned = computed(() => this.filtered().reduce((s, r) => s + r.earned, 0));

  statusBadge(s: CommissionRow['status']): string {
    return s === 'Paid' ? 'badge--success' : s === 'Pending' ? 'badge--warning' : 'badge--info';
  }

  fmt(n: number): string { return '$' + n.toLocaleString('en-US'); }

  agencies = ['Mid-Atlantic Group', 'Bayview Insurance', 'Coastal Partners', 'Northstar Agency', 'Keystone Assurance'];
}

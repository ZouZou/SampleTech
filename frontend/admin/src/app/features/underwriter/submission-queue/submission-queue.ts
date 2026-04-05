import { Component, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

interface Submission {
  id: string;
  applicant: string;
  type: string;
  broker: string;
  received: string;
  premium: number;
  status: 'New' | 'In Review' | 'Pending Info' | 'Quoted' | 'Declined';
  priority: 'High' | 'Normal' | 'Low';
}

type SortField = 'received' | 'premium' | 'applicant' | 'status';
type SortDir   = 'asc' | 'desc';

@Component({
  selector: 'app-submission-queue',
  imports: [FormsModule, RouterLink],
  templateUrl: './submission-queue.html',
  styleUrl: './submission-queue.scss',
})
export class SubmissionQueue {
  searchQuery  = signal('');
  filterStatus = signal('all');
  sortField    = signal<SortField>('received');
  sortDir      = signal<SortDir>('desc');

  submissions = signal<Submission[]>([
    { id: 'SUB-2026-0901', applicant: 'Apex Retail Holdings',       type: 'Commercial Property',    broker: 'Coastal Partners',  received: '2026-04-04', premium: 125000, status: 'New',         priority: 'High'   },
    { id: 'SUB-2026-0900', applicant: 'Clearwater Pharmaceuticals', type: 'Product Liability',      broker: 'Mid-Atlantic Group', received: '2026-04-04', premium: 340000, status: 'New',         priority: 'High'   },
    { id: 'SUB-2026-0899', applicant: 'Westbrook Manufacturing',    type: 'Commercial Property',    broker: 'Bayview Insurance',  received: '2026-04-03', premium: 84000,  status: 'In Review',   priority: 'High'   },
    { id: 'SUB-2026-0898', applicant: 'Pacific Rim Trading Co.',    type: 'Marine Cargo',           broker: 'Coastal Partners',  received: '2026-04-03', premium: 58000,  status: 'In Review',   priority: 'Normal' },
    { id: 'SUB-2026-0897', applicant: 'Harbor Logistics LLC',       type: 'Marine Cargo',           broker: 'Bayview Insurance',  received: '2026-04-03', premium: 32500,  status: 'Pending Info',priority: 'High'   },
    { id: 'SUB-2026-0896', applicant: 'Greenfield Solar Energy',    type: 'Construction All-Risk',  broker: 'Mid-Atlantic Group', received: '2026-04-02', premium: 195000, status: 'Pending Info',priority: 'Normal' },
    { id: 'SUB-2026-0895', applicant: 'Summit Healthcare Group',    type: 'Medical Malpractice',    broker: 'Coastal Partners',  received: '2026-04-02', premium: 210000, status: 'In Review',   priority: 'High'   },
    { id: 'SUB-2026-0894', applicant: 'BrightPath Education Inc.',  type: 'General Liability',      broker: 'Bayview Insurance',  received: '2026-04-01', premium: 12000,  status: 'Quoted',      priority: 'Normal' },
    { id: 'SUB-2026-0893', applicant: 'Metro Transit Authority',    type: 'Fleet Auto',             broker: 'Mid-Atlantic Group', received: '2026-04-01', premium: 285000, status: 'Quoted',      priority: 'Normal' },
    { id: 'SUB-2026-0892', applicant: 'Ridgeline Contractors',      type: 'Workers Comp',           broker: 'Coastal Partners',  received: '2026-03-31', premium: 45000,  status: 'Declined',    priority: 'Low'    },
  ]);

  filteredAndSorted = computed(() => {
    let list = this.submissions();
    const q = this.searchQuery().toLowerCase();
    if (q) list = list.filter(s =>
      s.applicant.toLowerCase().includes(q) || s.id.toLowerCase().includes(q) || s.type.toLowerCase().includes(q)
    );
    const fs = this.filterStatus();
    if (fs !== 'all') list = list.filter(s => s.status === fs);

    const field = this.sortField();
    const dir   = this.sortDir() === 'asc' ? 1 : -1;
    return [...list].sort((a, b) => {
      if (field === 'premium')   return (a.premium - b.premium) * dir;
      if (field === 'received')  return a.received.localeCompare(b.received) * dir;
      if (field === 'applicant') return a.applicant.localeCompare(b.applicant) * dir;
      if (field === 'status')    return a.status.localeCompare(b.status) * dir;
      return 0;
    });
  });

  setSort(field: SortField) {
    if (this.sortField() === field) {
      this.sortDir.update(d => d === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortField.set(field);
      this.sortDir.set('desc');
    }
  }

  sortIcon(field: SortField): string {
    if (this.sortField() !== field) return '↕';
    return this.sortDir() === 'asc' ? '↑' : '↓';
  }

  statusBadge(status: Submission['status']): string {
    const map: Record<string, string> = {
      'New':          'badge--info',
      'In Review':    'badge--warning',
      'Pending Info': 'badge--neutral',
      'Quoted':       'badge--success',
      'Declined':     'badge--danger',
    };
    return map[status] ?? 'badge--neutral';
  }

  priorityBadge(p: Submission['priority']): string {
    const map: Record<string, string> = {
      High:   'badge--danger',
      Normal: 'badge--neutral',
      Low:    'badge--success',
    };
    return map[p] ?? 'badge--neutral';
  }

  formatPremium(n: number): string {
    return '$' + n.toLocaleString('en-US');
  }
}

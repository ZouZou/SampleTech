import { Component, signal } from '@angular/core';

interface TrackedQuote {
  id: string;
  client: string;
  type: string;
  submitted: string;
  lastUpdated: string;
  status: string;
  milestones: { label: string; date: string; done: boolean }[];
}

@Component({
  selector: 'app-quote-status',
  templateUrl: './quote-status.html',
  styleUrl: './quote-status.scss',
})
export class QuoteStatus {
  selectedId = signal<string | null>(null);

  quotes = signal<TrackedQuote[]>([
    {
      id: 'SUB-2026-0901', client: 'Apex Retail Holdings', type: 'Commercial Property',
      submitted: 'Apr 4, 2026', lastUpdated: '2 hours ago', status: 'New',
      milestones: [
        { label: 'Submitted',            date: 'Apr 4, 2026 10:14 AM', done: true },
        { label: 'Received by UW',       date: 'Apr 4, 2026 10:16 AM', done: true },
        { label: 'Under Review',         date: '',                      done: false },
        { label: 'Quote Issued',         date: '',                      done: false },
        { label: 'Bound',                date: '',                      done: false },
      ]
    },
    {
      id: 'SUB-2026-0898', client: 'Pacific Rim Trading Co.', type: 'Marine Cargo',
      submitted: 'Apr 3, 2026', lastUpdated: '1 day ago', status: 'In Review',
      milestones: [
        { label: 'Submitted',            date: 'Apr 3, 2026 2:30 PM',  done: true },
        { label: 'Received by UW',       date: 'Apr 3, 2026 2:32 PM',  done: true },
        { label: 'Under Review',         date: 'Apr 3, 2026 4:15 PM',  done: true },
        { label: 'Quote Issued',         date: '',                      done: false },
        { label: 'Bound',                date: '',                      done: false },
      ]
    },
    {
      id: 'SUB-2026-0893', client: 'Metro Transit Authority', type: 'Fleet Auto',
      submitted: 'Apr 1, 2026', lastUpdated: '3 days ago', status: 'Quoted',
      milestones: [
        { label: 'Submitted',            date: 'Apr 1, 2026 9:00 AM',  done: true },
        { label: 'Received by UW',       date: 'Apr 1, 2026 9:02 AM',  done: true },
        { label: 'Under Review',         date: 'Apr 1, 2026 11:30 AM', done: true },
        { label: 'Quote Issued',         date: 'Apr 2, 2026 3:45 PM',  done: true },
        { label: 'Bound',                date: '',                      done: false },
      ]
    },
    {
      id: 'SUB-2026-0884', client: 'Pioneer Tech Solutions', type: 'Cyber Liability',
      submitted: 'Mar 28, 2026', lastUpdated: '1 week ago', status: 'Bound',
      milestones: [
        { label: 'Submitted',            date: 'Mar 28, 2026 10:00 AM', done: true },
        { label: 'Received by UW',       date: 'Mar 28, 2026 10:01 AM', done: true },
        { label: 'Under Review',         date: 'Mar 28, 2026 1:00 PM',  done: true },
        { label: 'Quote Issued',         date: 'Mar 29, 2026 10:22 AM', done: true },
        { label: 'Bound',                date: 'Apr 1, 2026 9:15 AM',   done: true },
      ]
    },
  ]);

  selectedQuote = () => this.quotes().find(q => q.id === this.selectedId());

  statusBadge(status: string): string {
    const map: Record<string, string> = {
      New: 'badge--info', 'In Review': 'badge--warning',
      Quoted: 'badge--neutral', Bound: 'badge--success', Declined: 'badge--danger',
    };
    return map[status] ?? 'badge--neutral';
  }

  milestonesDone(q: TrackedQuote): number {
    return q.milestones.filter(m => m.done).length;
  }
}

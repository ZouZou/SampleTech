import { Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

interface QuoteSection {
  label: string;
  fields: { key: string; value: string }[];
}

@Component({
  selector: 'app-quote-detail',
  imports: [RouterLink],
  templateUrl: './quote-detail.html',
  styleUrl: './quote-detail.scss',
})
export class QuoteDetail {
  decision = signal<'pending' | 'approved' | 'declined'>('pending');
  submitting = signal(false);
  declineReason = signal('');

  quote = signal({
    id: 'SUB-2026-0899',
    applicant: 'Westbrook Manufacturing',
    broker: 'Bayview Insurance',
    brokerContact: 'Angela Brooks (a.brooks@bayview.com)',
    received: 'April 3, 2026',
    deadline: 'April 4, 2026 — 5:00 PM',
    estimatedPremium: '$84,000',
    coverageType: 'Commercial Property',
    policyPeriod: 'May 1, 2026 – May 1, 2027',
  });

  sections = signal<QuoteSection[]>([
    {
      label: 'Applicant Information',
      fields: [
        { key: 'Business Name',   value: 'Westbrook Manufacturing, Inc.' },
        { key: 'Industry',        value: 'Metal Fabrication (NAICS 332710)' },
        { key: 'Years in Business', value: '22 years' },
        { key: 'Annual Revenue',  value: '$14.2M' },
        { key: 'Employees',       value: '187' },
        { key: 'Primary Location', value: '4820 Industrial Blvd, Pittsburgh, PA 15220' },
      ]
    },
    {
      label: 'Coverage Details',
      fields: [
        { key: 'Coverage Type',         value: 'Commercial Property — Building & Contents' },
        { key: 'Building Value',         value: '$6,500,000' },
        { key: 'Contents Value',         value: '$2,100,000' },
        { key: 'Business Interruption',  value: '$1,200,000 (12-month indemnity)' },
        { key: 'Deductible',             value: '$25,000 per occurrence' },
        { key: 'Special Conditions',     value: 'Earthquake extension requested' },
      ]
    },
    {
      label: 'Loss History (5 Years)',
      fields: [
        { key: '2022',  value: 'Machinery breakdown — $18,400' },
        { key: '2021',  value: 'Fire (sprinkler malfunction) — $224,000' },
        { key: '2020',  value: 'No losses' },
        { key: '2019',  value: 'Theft — $6,200' },
        { key: '2018',  value: 'No losses' },
      ]
    },
  ]);

  approve() {
    this.submitting.set(true);
    setTimeout(() => {
      this.submitting.set(false);
      this.decision.set('approved');
    }, 800);
  }

  decline() {
    this.submitting.set(true);
    setTimeout(() => {
      this.submitting.set(false);
      this.decision.set('declined');
    }, 800);
  }
}

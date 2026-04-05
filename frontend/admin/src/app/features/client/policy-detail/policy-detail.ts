import { Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

interface PolicyDocument {
  name: string;
  type: string;
  size: string;
  date: string;
}

@Component({
  selector: 'app-policy-detail',
  imports: [RouterLink],
  templateUrl: './policy-detail.html',
  styleUrl: './policy-detail.scss',
})
export class PolicyDetail {
  downloading = signal<string | null>(null);

  policy = signal({
    id: 'POL-2026-4421',
    type: 'Fleet Auto Insurance',
    status: 'Active',
    insurer: 'SampleTech Underwriters',
    policyNumber: 'FAI-2026-4421-00',
    effective: 'May 1, 2026',
    expiry: 'May 1, 2027',
    premium: '$285,000',
    paymentFrequency: 'Annual',
    coverageLimit: '$2,500,000 Combined Single Limit',
    deductible: '$5,000 per occurrence',
    vehicles: '47 commercial vehicles',
    drivers: '52 authorized drivers',
    territory: 'Pennsylvania, Maryland, Virginia, New Jersey',
    agent: 'Tom Chen — tom.chen@bayview.com',
    broker: 'Bayview Insurance',
    lastUpdated: 'April 2, 2026',
  });

  documents = signal<PolicyDocument[]>([
    { name: 'Policy Declaration Page',        type: 'PDF', size: '284 KB',  date: 'May 1, 2026' },
    { name: 'Certificate of Insurance',       type: 'PDF', size: '142 KB',  date: 'May 1, 2026' },
    { name: 'Auto Schedule (Vehicle List)',   type: 'PDF', size: '512 KB',  date: 'May 1, 2026' },
    { name: 'Policy Wording — Fleet Auto',    type: 'PDF', size: '1.2 MB',  date: 'May 1, 2026' },
    { name: 'Additional Insured Endorsement', type: 'PDF', size: '98 KB',   date: 'Jun 15, 2026' },
    { name: 'Claims Reporting Procedures',   type: 'PDF', size: '74 KB',   date: 'May 1, 2026' },
  ]);

  downloadDoc(doc: PolicyDocument) {
    this.downloading.set(doc.name);
    setTimeout(() => this.downloading.set(null), 1500);
  }
}

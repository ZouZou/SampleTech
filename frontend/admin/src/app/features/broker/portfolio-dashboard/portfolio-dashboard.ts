import { Component, signal } from '@angular/core';

interface PolicyLine {
  line: string;
  gwp: string;
  policyCount: number;
  pctOfTotal: number;
  renewalRate: string;
}

@Component({
  selector: 'app-portfolio-dashboard',
  templateUrl: './portfolio-dashboard.html',
  styleUrl: './portfolio-dashboard.scss',
})
export class PortfolioDashboard {
  kpis = signal([
    { label: 'Total GWP (YTD)',       value: '$4.28M', change: '+22%', dir: 'up'     },
    { label: 'Policies In Force',     value: '312',    change: '+28',  dir: 'up'     },
    { label: 'Expiring Next 30d',     value: '41',     change: 'Action required', dir: 'neutral' },
    { label: 'Portfolio Retention',   value: '91.4%',  change: '+2.1pp', dir: 'up'   },
    { label: 'Loss Ratio (Avg)',      value: '54.2%',  change: '-3.8pp improvement', dir: 'down' },
    { label: 'New Business (YTD)',    value: '$1.14M', change: '27% of GWP', dir: 'neutral'  },
  ]);

  linesOfBusiness = signal<PolicyLine[]>([
    { line: 'Commercial Property',    gwp: '$1.42M', policyCount: 88,  pctOfTotal: 33, renewalRate: '94%' },
    { line: 'General Liability',      gwp: '$0.88M', policyCount: 72,  pctOfTotal: 21, renewalRate: '91%' },
    { line: 'Workers Comp',           gwp: '$0.64M', policyCount: 56,  pctOfTotal: 15, renewalRate: '89%' },
    { line: 'Commercial Auto',        gwp: '$0.52M', policyCount: 34,  pctOfTotal: 12, renewalRate: '88%' },
    { line: 'Cyber Liability',        gwp: '$0.38M', policyCount: 28,  pctOfTotal: 9,  renewalRate: '96%' },
    { line: 'Marine Cargo',           gwp: '$0.24M', policyCount: 18,  pctOfTotal: 6,  renewalRate: '82%' },
    { line: 'Other Lines',            gwp: '$0.20M', policyCount: 16,  pctOfTotal: 4,  renewalRate: '85%' },
  ]);
}

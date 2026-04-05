import { Component, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';

interface AuditEntry {
  id: string;
  timestamp: string;
  user: string;
  action: string;
  resource: string;
  ip: string;
  result: 'success' | 'failure' | 'warning';
}

@Component({
  selector: 'app-audit-log',
  imports: [FormsModule],
  templateUrl: './audit-log.html',
  styleUrl: './audit-log.scss',
})
export class AuditLog {
  filterUser   = signal('');
  filterResult = signal('all');

  entries = signal<AuditEntry[]>([
    { id: '1',  timestamp: '2026-04-04 14:32:01', user: 'admin@sampletech.com',        action: 'USER_CREATE',        resource: 'User: sarah.jones@midatlantic.com', ip: '10.0.1.45',   result: 'success' },
    { id: '2',  timestamp: '2026-04-04 14:15:22', user: 'admin@sampletech.com',        action: 'CONFIG_UPDATE',      resource: 'Tenant: MidAtlantic Group',         ip: '10.0.1.45',   result: 'success' },
    { id: '3',  timestamp: '2026-04-04 13:58:47', user: 'tom.chen@bayview.com',         action: 'LOGIN',              resource: 'Auth',                              ip: '203.45.67.89', result: 'success' },
    { id: '4',  timestamp: '2026-04-04 13:45:11', user: 'unknown@external.com',         action: 'LOGIN_FAILED',       resource: 'Auth',                              ip: '185.220.101.5',result: 'failure' },
    { id: '5',  timestamp: '2026-04-04 13:30:00', user: 'admin@sampletech.com',        action: 'ROLE_CHANGE',        resource: 'User: tom.chen → Underwriter',      ip: '10.0.1.45',   result: 'success' },
    { id: '6',  timestamp: '2026-04-04 12:00:33', user: 'broker@coastal-ins.com',      action: 'REPORT_EXPORT',      resource: 'Portfolio Q1-2026',                 ip: '92.168.1.100', result: 'success' },
    { id: '7',  timestamp: '2026-04-04 11:44:19', user: 'priya.p@bayview.com',          action: 'QUOTE_APPROVE',      resource: 'Quote #UW-2026-0441',               ip: '10.0.2.88',   result: 'success' },
    { id: '8',  timestamp: '2026-04-04 11:22:05', user: 'unknown@external.com',         action: 'API_ACCESS',         resource: '/api/v1/policies',                  ip: '45.33.22.11',  result: 'failure' },
    { id: '9',  timestamp: '2026-04-04 10:55:44', user: 'admin@sampletech.com',        action: 'USER_DEACTIVATE',    resource: 'User: old.agent@acme.com',          ip: '10.0.1.45',   result: 'success' },
    { id: '10', timestamp: '2026-04-04 10:30:12', user: 'sarah.j@midatlantic.com',     action: 'SUBMISSION_CREATE',  resource: 'Sub #S-2026-0892',                  ip: '172.16.0.22',  result: 'success' },
    { id: '11', timestamp: '2026-04-04 09:15:00', user: 'admin@sampletech.com',        action: 'AUDIT_EXPORT',       resource: 'Audit log Q1-2026',                 ip: '10.0.1.45',   result: 'warning' },
  ]);

  filteredEntries = computed(() => {
    let list = this.entries();
    const u = this.filterUser().toLowerCase().trim();
    if (u) list = list.filter(e => e.user.toLowerCase().includes(u));
    const r = this.filterResult();
    if (r !== 'all') list = list.filter(e => e.result === r);
    return list;
  });

  resultBadge(result: AuditEntry['result']): string {
    const map: Record<string, string> = {
      success: 'badge--success',
      failure: 'badge--danger',
      warning: 'badge--warning',
    };
    return map[result] ?? 'badge--neutral';
  }
}

import { Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

interface SystemStat {
  label: string;
  value: string;
  change: string;
  direction: 'up' | 'down' | 'neutral';
}

interface RecentActivity {
  id: string;
  action: string;
  user: string;
  timestamp: string;
  type: 'create' | 'update' | 'delete' | 'login';
}

@Component({
  selector: 'app-admin-dashboard',
  imports: [RouterLink],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.scss',
})
export class AdminDashboard {
  stats = signal<SystemStat[]>([
    { label: 'Total Users',     value: '1,284',   change: '+12 this week',  direction: 'up' },
    { label: 'Active Tenants',  value: '47',       change: '+2 this month',  direction: 'up' },
    { label: 'Open Tickets',    value: '23',       change: '-5 from last week', direction: 'down' },
    { label: 'System Uptime',   value: '99.97%',   change: '30-day average',    direction: 'neutral' },
  ]);

  recentActivity = signal<RecentActivity[]>([
    { id: '1', action: 'New user created: sarah.jones@midatlantic.com', user: 'admin@sampletech.com',    timestamp: '2 min ago',    type: 'create' },
    { id: '2', action: 'Tenant config updated: MidAtlantic Group',      user: 'admin@sampletech.com',    timestamp: '18 min ago',   type: 'update' },
    { id: '3', action: 'User role changed: tom.chen → Underwriter',     user: 'admin@sampletech.com',    timestamp: '1 hour ago',   type: 'update' },
    { id: '4', action: 'User login: broker@coastal-ins.com',            user: 'system',                  timestamp: '1 hour ago',   type: 'login'  },
    { id: '5', action: 'Audit export generated (Q1 2026)',              user: 'admin@sampletech.com',    timestamp: '3 hours ago',  type: 'create' },
    { id: '6', action: 'User deactivated: old.agent@acme.com',          user: 'admin@sampletech.com',    timestamp: '5 hours ago',  type: 'delete' },
  ]);

  activityTypeIcon(type: RecentActivity['type']): string {
    const icons: Record<string, string> = {
      create: '✚',
      update: '✎',
      delete: '✕',
      login:  '→',
    };
    return icons[type] ?? '•';
  }

  activityTypeBadge(type: RecentActivity['type']): string {
    const badges: Record<string, string> = {
      create: 'badge--success',
      update: 'badge--info',
      delete: 'badge--danger',
      login:  'badge--neutral',
    };
    return badges[type] ?? 'badge--neutral';
  }
}

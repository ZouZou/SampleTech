import { Component, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';

interface User {
  id: string;
  name: string;
  email: string;
  role: string;
  status: 'Active' | 'Inactive' | 'Pending';
  lastLogin: string;
  tenant: string;
}

@Component({
  selector: 'app-user-management',
  imports: [FormsModule],
  templateUrl: './user-management.html',
  styleUrl: './user-management.scss',
})
export class UserManagement {
  searchQuery = signal('');

  users = signal<User[]>([
    { id: '1', name: 'Sarah Johnson',  email: 'sarah.j@midatlantic.com',     role: 'Underwriter', status: 'Active',   lastLogin: '2 hours ago',  tenant: 'Mid-Atlantic Group' },
    { id: '2', name: 'Tom Chen',       email: 'tom.chen@bayview.com',         role: 'Agent',       status: 'Active',   lastLogin: '30 min ago',   tenant: 'Bayview Insurance' },
    { id: '3', name: 'Maria Rivera',   email: 'm.rivera@coastal-ins.com',     role: 'Broker',      status: 'Active',   lastLogin: '1 day ago',    tenant: 'Coastal Partners' },
    { id: '4', name: 'David Kim',      email: 'd.kim@sampletech.com',         role: 'Admin',       status: 'Active',   lastLogin: '5 min ago',    tenant: 'SampleTech' },
    { id: '5', name: 'Lisa Thompson',  email: 'lisa.t@northstar.com',         role: 'Client',      status: 'Active',   lastLogin: '3 days ago',   tenant: 'Northstar Corp' },
    { id: '6', name: 'James Wilson',   email: 'j.wilson@midatlantic.com',     role: 'Agent',       status: 'Inactive', lastLogin: '14 days ago',  tenant: 'Mid-Atlantic Group' },
    { id: '7', name: 'Priya Patel',    email: 'priya.p@bayview.com',          role: 'Underwriter', status: 'Active',   lastLogin: '4 hours ago',  tenant: 'Bayview Insurance' },
    { id: '8', name: 'Robert Gates',   email: 'r.gates@acme-corp.com',        role: 'Client',      status: 'Pending',  lastLogin: 'Never',        tenant: 'ACME Corp' },
    { id: '9', name: 'Angela Brooks',  email: 'a.brooks@coastal-ins.com',     role: 'Broker',      status: 'Active',   lastLogin: '6 hours ago',  tenant: 'Coastal Partners' },
    { id: '10',name: 'Carlos Mendez',  email: 'c.mendez@northstar.com',       role: 'Agent',       status: 'Active',   lastLogin: '2 days ago',   tenant: 'Northstar Corp' },
  ]);

  filteredUsers = computed(() => {
    const q = this.searchQuery().toLowerCase();
    if (!q) return this.users();
    return this.users().filter(u =>
      u.name.toLowerCase().includes(q) ||
      u.email.toLowerCase().includes(q) ||
      u.role.toLowerCase().includes(q) ||
      u.tenant.toLowerCase().includes(q)
    );
  });

  statusBadge(status: User['status']): string {
    const map: Record<string, string> = {
      Active:   'badge--success',
      Inactive: 'badge--danger',
      Pending:  'badge--warning',
    };
    return map[status] ?? 'badge--neutral';
  }
}

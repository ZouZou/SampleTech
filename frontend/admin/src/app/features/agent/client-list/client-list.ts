import { Component, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';

interface Client {
  id: string;
  name: string;
  email: string;
  phone: string;
  industry: string;
  activePolicies: number;
  revenue: string;
  since: string;
}

@Component({
  selector: 'app-client-list',
  imports: [FormsModule],
  templateUrl: './client-list.html',
  styleUrl: './client-list.scss',
})
export class ClientList {
  searchQuery = signal('');

  clients = signal<Client[]>([
    { id: '1', name: 'Apex Retail Holdings',        email: 'risk@apex-retail.com',     phone: '(412) 555-0142', industry: 'Retail',           activePolicies: 3, revenue: '$42M',  since: '2021' },
    { id: '2', name: 'Pacific Rim Trading Co.',     email: 'ops@pacificrim-trade.com', phone: '(310) 555-0287', industry: 'Import/Export',    activePolicies: 2, revenue: '$28M',  since: '2020' },
    { id: '3', name: 'Metro Transit Authority',     email: 'fleet@metro-transit.gov',  phone: '(202) 555-0331', industry: 'Public Sector',    activePolicies: 5, revenue: '$180M', since: '2019' },
    { id: '4', name: 'Pioneer Tech Solutions',      email: 'legal@pioneertech.io',     phone: '(415) 555-0498', industry: 'Technology',       activePolicies: 4, revenue: '$67M',  since: '2022' },
    { id: '5', name: 'Greenfield Solar Energy',     email: 'insurance@gfsolar.com',    phone: '(512) 555-0516', industry: 'Renewable Energy', activePolicies: 2, revenue: '$95M',  since: '2023' },
    { id: '6', name: 'BrightPath Education Inc.',   email: 'admin@brightpath.edu',     phone: '(617) 555-0623', industry: 'Education',        activePolicies: 3, revenue: '$12M',  since: '2021' },
    { id: '7', name: 'Ridgeline Contractors',       email: 'bids@ridgeline-co.com',    phone: '(303) 555-0714', industry: 'Construction',     activePolicies: 1, revenue: '$31M',  since: '2022' },
    { id: '8', name: 'Summit Healthcare Group',     email: 'risk@summithhg.com',       phone: '(720) 555-0885', industry: 'Healthcare',       activePolicies: 6, revenue: '$220M', since: '2018' },
  ]);

  filteredClients = computed(() => {
    const q = this.searchQuery().toLowerCase();
    if (!q) return this.clients();
    return this.clients().filter(c =>
      c.name.toLowerCase().includes(q) ||
      c.email.toLowerCase().includes(q) ||
      c.industry.toLowerCase().includes(q)
    );
  });
}

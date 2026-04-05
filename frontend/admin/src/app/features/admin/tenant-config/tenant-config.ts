import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-tenant-config',
  imports: [FormsModule],
  templateUrl: './tenant-config.html',
  styleUrl: './tenant-config.scss',
})
export class TenantConfig {
  saving = signal(false);
  saved = signal(false);

  // Flat signals for form binding
  tenantName             = signal('SampleTech Insurance');
  domain                 = signal('sampletech.com');
  maxUsers               = signal(500);
  mfaEnabled             = signal(true);
  sessionTimeoutMinutes  = signal(60);
  allowedIpRanges        = signal('0.0.0.0/0');
  logoUrl                = signal('https://cdn.sampletech.com/logo.png');
  primaryColor           = signal('#0F2B5B');
  supportEmail           = signal('support@sampletech.com');

  save() {
    this.saving.set(true);
    this.saved.set(false);
    setTimeout(() => {
      this.saving.set(false);
      this.saved.set(true);
      setTimeout(() => this.saved.set(false), 3000);
    }, 800);
  }
}

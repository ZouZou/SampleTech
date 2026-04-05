import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-support-contact',
  imports: [FormsModule],
  templateUrl: './support-contact.html',
  styleUrl: './support-contact.scss',
})
export class SupportContact {
  subject    = signal('');
  category   = signal('');
  message    = signal('');
  submitting = signal(false);
  submitted  = signal(false);
  ticketId   = signal('');

  submit() {
    if (!this.subject() || !this.category() || !this.message()) return;
    this.submitting.set(true);
    setTimeout(() => {
      const num = Math.floor(Math.random() * 9000) + 1000;
      this.ticketId.set(`TKT-2026-${num}`);
      this.submitting.set(false);
      this.submitted.set(true);
    }, 900);
  }

  resetForm() {
    this.subject.set('');
    this.category.set('');
    this.message.set('');
    this.submitted.set(false);
  }
}

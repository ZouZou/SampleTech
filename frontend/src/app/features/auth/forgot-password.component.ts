import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [FormsModule, CommonModule],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-50">
      <div class="w-full max-w-md bg-white rounded-2xl shadow-lg p-8">
        <h1 class="text-2xl font-bold text-gray-900 mb-2">Reset password</h1>
        <p class="text-sm text-gray-500 mb-6">Enter your email and we'll send you a reset link.</p>

        @if (!submitted()) {
          <form (ngSubmit)="onSubmit()" novalidate>
            <div class="mb-4">
              <label class="block text-sm font-medium text-gray-700 mb-1">Email</label>
              <input
                type="email"
                required
                [(ngModel)]="email"
                name="email"
                class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                placeholder="you@company.com"
              />
            </div>
            @if (error()) {
              <div class="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700">{{ error() }}</div>
            }
            <button
              type="submit"
              [disabled]="loading()"
              class="w-full py-2.5 px-4 bg-indigo-600 hover:bg-indigo-700 disabled:bg-indigo-400 text-white text-sm font-medium rounded-lg transition-colors"
            >
              {{ loading() ? 'Sending…' : 'Send reset link' }}
            </button>
          </form>
        } @else {
          <div class="p-4 bg-green-50 border border-green-200 rounded-lg text-sm text-green-700">
            If an account with that email exists, a reset link has been sent.
          </div>
        }

        <div class="mt-4 text-center">
          <a routerLink="/auth/login" class="text-sm text-indigo-600 hover:underline">Back to sign in</a>
        </div>
      </div>
    </div>
  `
})
export class ForgotPasswordComponent {
  email = '';
  loading = signal(false);
  error = signal<string | null>(null);
  submitted = signal(false);

  constructor(private http: HttpClient) {}

  onSubmit(): void {
    if (!this.email) return;
    this.loading.set(true);
    this.error.set(null);

    this.http.post(`${environment.apiUrl}/auth/forgot-password`, { email: this.email }).subscribe({
      next: () => { this.submitted.set(true); },
      error: () => { this.error.set('Something went wrong. Please try again.'); this.loading.set(false); }
    });
  }
}

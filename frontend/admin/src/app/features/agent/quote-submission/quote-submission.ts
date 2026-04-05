import { Component, signal, computed } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { inject } from '@angular/core';

@Component({
  selector: 'app-quote-submission',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './quote-submission.html',
  styleUrl: './quote-submission.scss',
})
export class QuoteSubmission {
  private fb = inject(FormBuilder);

  currentStep = signal(1);
  totalSteps = 3;
  submitted = signal(false);
  submitting = signal(false);
  submissionId = signal('');

  steps = [
    { number: 1, label: 'Applicant Info' },
    { number: 2, label: 'Coverage Details' },
    { number: 3, label: 'Review & Submit' },
  ];

  applicantForm = this.fb.group({
    businessName:  ['', Validators.required],
    industry:      ['', Validators.required],
    annualRevenue: ['', Validators.required],
    employees:     ['', [Validators.required, Validators.min(1)]],
    address:       ['', Validators.required],
    contactName:   ['', Validators.required],
    contactEmail:  ['', [Validators.required, Validators.email]],
    contactPhone:  ['', Validators.required],
  });

  coverageForm = this.fb.group({
    coverageType:        ['', Validators.required],
    effectiveDate:       ['', Validators.required],
    expirationDate:      ['', Validators.required],
    coverageLimit:       ['', Validators.required],
    deductible:          ['', Validators.required],
    additionalNotes:     [''],
    priorInsurer:        [''],
    priorPremium:        [''],
  });

  progressPct = computed(() => ((this.currentStep() - 1) / (this.totalSteps - 1)) * 100);

  nextStep() {
    if (this.currentStep() < this.totalSteps) {
      this.currentStep.update(s => s + 1);
    }
  }

  prevStep() {
    if (this.currentStep() > 1) {
      this.currentStep.update(s => s - 1);
    }
  }

  canProceed(): boolean {
    if (this.currentStep() === 1) return this.applicantForm.valid;
    if (this.currentStep() === 2) return this.coverageForm.valid;
    return true;
  }

  submitQuote() {
    this.submitting.set(true);
    setTimeout(() => {
      const num = Math.floor(Math.random() * 900) + 100;
      this.submissionId.set(`SUB-2026-0${num}`);
      this.submitting.set(false);
      this.submitted.set(true);
    }, 1200);
  }
}

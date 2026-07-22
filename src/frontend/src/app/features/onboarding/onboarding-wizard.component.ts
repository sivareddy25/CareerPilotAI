import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { catchError, of, switchMap } from 'rxjs';
import { OnboardingService } from '../../core/services/onboarding.service';
import { ResumeService } from '../../core/services/resume.service';
import { JobService } from '../../core/services/job.service';
import {
  CardComponent,
  ButtonComponent,
  BadgeComponent,
  IconComponent,
  PageHeaderComponent,
  SpinnerComponent,
} from '../../shared/components';

@Component({
  selector: 'app-onboarding-wizard',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    CardComponent,
    ButtonComponent,
    BadgeComponent,
    IconComponent,
    PageHeaderComponent,
    SpinnerComponent,
  ],
  template: `
    <div class="onboarding-container">
      <app-page-header
        title="Welcome to CareerPilot AI"
        subtitle="Complete your candidate profile once so our Playwright automation engine can auto-fill job application forms for you."
      />

      <!-- Progress Stepper Header -->
      <div class="stepper-header">
        <div class="step-item" [class.active]="currentStep() === 1" [class.completed]="currentStep() > 1">
          <div class="step-badge">1</div>
          <span class="step-label">Contact Details</span>
        </div>
        <div class="step-line" [class.active]="currentStep() > 1"></div>
        <div class="step-item" [class.active]="currentStep() === 2" [class.completed]="currentStep() > 2">
          <div class="step-badge">2</div>
          <span class="step-label">Work Authorization</span>
        </div>
        <div class="step-line" [class.active]="currentStep() > 2"></div>
        <div class="step-item" [class.active]="currentStep() === 3" [class.completed]="currentStep() > 3">
          <div class="step-badge">3</div>
          <span class="step-label">Target Preferences</span>
        </div>
        <div class="step-line" [class.active]="currentStep() > 3"></div>
        <div class="step-item" [class.active]="currentStep() === 4" [class.completed]="currentStep() > 4">
          <div class="step-badge">4</div>
          <span class="step-label">Resume & Complete</span>
        </div>
      </div>

      <!-- Step Cards -->
      <app-card class="form-card">
        <form [formGroup]="form" (ngSubmit)="onSubmit()">
          <!-- STEP 1: Contact Details -->
          @if (currentStep() === 1) {
            <div class="step-content">
              <h3>Step 1: Personal Contact & Profiles</h3>
              <p class="step-sub">These details will be populated into ATS form fields automatically.</p>

              <div class="form-grid">
                <div class="form-group">
                  <label>First Name *</label>
                  <input type="text" formControlName="firstName" placeholder="e.g. Alex" class="form-input" />
                </div>
                <div class="form-group">
                  <label>Last Name *</label>
                  <input type="text" formControlName="lastName" placeholder="e.g. Mercer" class="form-input" />
                </div>
              </div>

              <div class="form-grid">
                <div class="form-group">
                  <label>Phone Number *</label>
                  <input type="tel" formControlName="phoneNumber" placeholder="+1 (555) 019-2834" class="form-input" />
                </div>
                <div class="form-group">
                  <label>LinkedIn Profile URL</label>
                  <input type="url" formControlName="linkedInUrl" placeholder="https://linkedin.com/in/alexmercer" class="form-input" />
                </div>
              </div>

              <div class="form-grid">
                <div class="form-group">
                  <label>GitHub Profile URL</label>
                  <input type="url" formControlName="gitHubUrl" placeholder="https://github.com/alexmercer" class="form-input" />
                </div>
                <div class="form-group">
                  <label>Portfolio / Personal Website</label>
                  <input type="url" formControlName="portfolioUrl" placeholder="https://alexmercer.dev" class="form-input" />
                </div>
              </div>
            </div>
          }

          <!-- STEP 2: Work Authorization -->
          @if (currentStep() === 2) {
            <div class="step-content">
              <h3>Step 2: Legal Work Authorization</h3>
              <p class="step-sub">Required by job portals (Greenhouse, Workday, Lever) during application screening.</p>

              <div class="form-group">
                <label>Work Authorization Status *</label>
                <select formControlName="workAuthorization" class="form-input">
                  <option value="US Citizen">US Citizen / Permanent Resident / Green Card (No Sponsorship Required)</option>
                  <option value="EB Spousal EAD">EB-1 / EB-2 / EB-3 Dependent Spousal EAD (No Sponsorship Required)</option>
                  <option value="H-4 EAD">H-4 Dependent Spousal EAD (No Sponsorship Required)</option>
                  <option value="L-2 / E-2 EAD">L-2 / E-2 Spousal EAD (No Sponsorship Required)</option>
                  <option value="H-1B Visa">H-1B Visa (Sponsorship / Transfer Required)</option>
                  <option value="OPT / STEM OPT">F-1 OPT / STEM OPT (Current EAD - Future Sponsorship Required)</option>
                  <option value="O-1 / TN / E-3">O-1 / TN / E-3 Specialty Visa</option>
                  <option value="EU / UK Citizen">EU / UK Citizen or Permanent Resident</option>
                  <option value="Canadian Citizen">Canadian Citizen / Permanent Resident</option>
                  <option value="Other EAD">Other Legal Work Permit (No Sponsorship Required)</option>
                  <option value="Other">Other / Requires Sponsorship</option>
                </select>
              </div>

              <div class="form-group">
                <label>Target Minimum Annual Salary (USD) *</label>
                <input type="text" formControlName="preferredSalary" placeholder="$140,000 / year" class="form-input" />
              </div>
            </div>
          }

          <!-- STEP 3: Target Preferences -->
          @if (currentStep() === 3) {
            <div class="step-content">
              <h3>Step 3: Target Job Titles & Roles</h3>
              <p class="step-sub">Used by the Job Search & AI Matching engine to deliver tailored job recommendations.</p>

              <div class="form-group">
                <label>Target Job Titles (comma separated) *</label>
                <input type="text" formControlName="targetJobTitles" placeholder="Senior Full Stack Engineer, Staff Software Engineer, Tech Lead" class="form-input" />
              </div>
            </div>
          }

          <!-- STEP 4: Resume upload & confirmation -->
          @if (currentStep() === 4) {
            <div class="step-content">
              <h3>Step 4: Upload Your Resume</h3>
              <p class="step-sub">We'll analyze it to sharpen your job matches, then pull in openings from every provider.</p>

              <input
                #fileInput
                type="file"
                accept=".pdf,.docx,.json"
                hidden
                (change)="onFileSelected($event)"
              />

              @if (selectedFile(); as file) {
                <div class="resume-dropzone selected" (click)="fileInput.click()">
                  <app-icon name="file-text" size="lg" />
                  <h4>{{ file.name }}</h4>
                  <p>{{ (file.size / 1024) | number:'1.0-0' }} KB • click to replace</p>
                  <app-badge variant="success">Ready to upload</app-badge>
                </div>
              } @else {
                <div class="resume-dropzone" (click)="fileInput.click()">
                  <app-icon name="upload" size="lg" />
                  <h4>Choose your resume</h4>
                  <p>PDF, DOCX or JSON — optional, but it makes matching much better.</p>
                </div>
              }

              <div class="info-alert">
                <app-icon name="sparkles" size="sm" />
                <span>On completion we'll upload your resume and immediately search all job providers — your ranked matches appear on the dashboard.</span>
              </div>

              @if (setupStatus()) {
                <div class="setup-status">
                  <app-spinner size="sm" />
                  <span>{{ setupStatus() }}</span>
                </div>
              }
            </div>
          }

          <!-- Form Navigation Controls -->
          <div class="form-actions">
            @if (currentStep() > 1) {
              <app-button type="button" variant="outline" (btnClick)="prevStep()">Back</app-button>
            }
            <div class="spacer"></div>
            @if (currentStep() < 4) {
              <app-button type="button" variant="primary" (btnClick)="nextStep()">
                Continue <app-icon name="arrow-right" size="sm" />
              </app-button>
            } @else {
              <app-button type="submit" variant="primary" [loading]="isSubmitting()">
                Complete Setup & Launch Dashboard
              </app-button>
            }
          </div>
        </form>
      </app-card>
    </div>
  `,
  styles: [`
    .onboarding-container {
      max-width: 800px;
      margin: 0 auto;
      padding: var(--space-6);
    }
    .stepper-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      margin: var(--space-6) 0 var(--space-8) 0;
    }
    .step-item {
      display: flex;
      align-items: center;
      gap: var(--space-2);
      opacity: 0.5;
      &.active, &.completed { opacity: 1; }
    }
    .step-badge {
      width: 32px;
      height: 32px;
      border-radius: 50%;
      background-color: var(--bg-tertiary);
      color: var(--text-primary);
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: 700;
      font-size: 14px;
    }
    .step-item.active .step-badge {
      background-color: var(--brand-primary);
      color: #fff;
    }
    .step-item.completed .step-badge {
      background-color: var(--success);
      color: #fff;
    }
    .step-label { font-size: var(--text-body-sm); font-weight: 600; }
    .step-line {
      flex: 1;
      height: 2px;
      background-color: var(--border-color);
      margin: 0 var(--space-3);
      &.active { background-color: var(--brand-primary); }
    }

    .form-card { padding: var(--space-6); }
    .step-content h3 { margin-top: 0; font-size: var(--text-h3); }
    .step-sub { color: var(--text-secondary); margin-bottom: var(--space-6); }

    .form-group {
      display: flex;
      flex-direction: column;
      gap: var(--space-2);
      margin-bottom: var(--space-4);
      label { font-weight: 600; font-size: var(--text-body-sm); }
    }
    .form-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: var(--space-4);
    }
    @media (max-width: 600px) {
      .form-grid { grid-template-columns: 1fr; }
    }
    .form-input {
      padding: var(--space-3);
      border-radius: var(--radius-md);
      border: 1px solid var(--border-color);
      background-color: var(--bg-primary);
      color: var(--text-primary);
      font-size: var(--text-body-sm);

      &:focus {
        outline: none;
        border-color: var(--brand-primary);
      }
    }

    .resume-dropzone {
      border: 2px dashed var(--border-color);
      border-radius: var(--radius-lg);
      padding: var(--space-8);
      text-align: center;
      background-color: var(--bg-secondary);
      margin-bottom: var(--space-4);
      cursor: pointer;
      transition: border-color 0.15s ease, background-color 0.15s ease;

      h4 { margin: var(--space-2) 0; }
      p { color: var(--text-secondary); font-size: var(--text-body-sm); margin-bottom: var(--space-3); }
    }
    .resume-dropzone:hover { border-color: var(--brand-primary); }
    .resume-dropzone.selected {
      border-style: solid;
      border-color: var(--color-success, #16a34a);
      background-color: color-mix(in srgb, var(--color-success, #16a34a) 8%, transparent);
    }
    .setup-status {
      display: flex; align-items: center; gap: var(--space-2);
      margin-top: var(--space-3);
      font-size: var(--text-body-sm); color: var(--text-secondary);
    }

    .info-alert {
      display: flex;
      align-items: center;
      gap: var(--space-3);
      padding: var(--space-3) var(--space-4);
      border-radius: var(--radius-md);
      background-color: var(--brand-primary-alpha);
      color: var(--brand-primary);
      font-size: var(--text-body-sm);
      margin-bottom: var(--space-6);
    }

    .form-actions {
      display: flex;
      align-items: center;
      margin-top: var(--space-6);
      padding-top: var(--space-4);
      border-top: 1px solid var(--border-color);
    }
    .spacer { flex: 1; }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OnboardingWizardComponent {
  private readonly fb = inject(FormBuilder);
  private readonly onboardingService = inject(OnboardingService);
  private readonly resumeService = inject(ResumeService);
  private readonly jobService = inject(JobService);
  private readonly router = inject(Router);

  protected readonly currentStep = signal<number>(1);
  protected readonly isSubmitting = this.onboardingService.isSubmitting;
  protected readonly selectedFile = signal<File | null>(null);
  // Progress text while the post-submit chain runs (upload → job search). Empty when idle.
  protected readonly setupStatus = signal<string>('');

  protected readonly form = this.fb.group({
    firstName: ['Alex', Validators.required],
    lastName: ['Mercer', Validators.required],
    phoneNumber: ['+1 (555) 019-2834', Validators.required],
    linkedInUrl: ['https://linkedin.com/in/alexmercer'],
    gitHubUrl: ['https://github.com/alexmercer'],
    portfolioUrl: ['https://alexmercer.dev'],
    workAuthorization: ['US Citizen', Validators.required],
    preferredSalary: ['$140,000 / year', Validators.required],
    targetJobTitles: ['Senior Full Stack Engineer, Staff Engineer', Validators.required],
  });

  protected nextStep(): void {
    if (this.currentStep() < 4) {
      this.currentStep.update((s) => s + 1);
    }
  }

  protected prevStep(): void {
    if (this.currentStep() > 1) {
      this.currentStep.update((s) => s - 1);
    }
  }

  protected onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFile.set(input.files?.[0] ?? null);
  }

  protected onSubmit(): void {
    if (this.form.invalid) return;

    const val = this.form.value;
    const file = this.selectedFile();

    this.setupStatus.set('Saving your profile…');

    const firstName = val.firstName || '';
    const lastName = val.lastName || '';
    const displayName = `${firstName} ${lastName}`.trim();

    this.onboardingService
      .completeOnboarding({
        firstName,
        lastName,
        displayName,
        phoneNumber: val.phoneNumber || '',
        linkedInUrl: val.linkedInUrl || '',
        gitHubUrl: val.gitHubUrl || '',
        portfolioUrl: val.portfolioUrl || '',
        workAuthorization: val.workAuthorization || '',
        preferredSalary: val.preferredSalary || '',
        targetJobTitles: val.targetJobTitles || '',
      })
      .pipe(
        switchMap(() => {
          if (!file) {
            return of(null);
          }
          this.setupStatus.set('Uploading and analyzing your resume…');
          return this.resumeService.importResumes([file]).pipe(catchError(() => of(null)));
        }),
        switchMap(() => {
          this.setupStatus.set('Searching all job providers for your matches…');
          return this.jobService.synchronize().pipe(catchError(() => of(null)));
        }),
      )
      .subscribe(() => {
        this.setupStatus.set('');
        void this.router.navigate(['/']);
      });
  }
}

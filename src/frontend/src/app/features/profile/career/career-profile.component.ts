import { Component, ChangeDetectionStrategy, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { CareerProfileService } from '../../../core/services/career-profile.service';
import { ProfileSkillDto } from '../../../core/services/career-profile.models';
import { EmploymentType, RemoteType } from '../../../core/models/job.models';
import { ToastService } from '../../../core/services/toast.service';
import {
  PageHeaderComponent,
  CardComponent,
  ButtonComponent,
  InputComponent,
  SelectComponent,
  ChipComponent,
  IconComponent,
  SpinnerComponent,
  SelectOption,
} from '../../../shared/components';

/**
 * Editor for the structured career profile that drives job match scoring.
 *
 * Local signals hold the working copy; skills are the only collection edited in place, so they
 * get add/remove affordances while the scalar fields bind straight through. Everything is sent as
 * a single replace on save, matching the API's full-replacement semantics — there is no partial
 * patch to reason about.
 */
@Component({
  selector: 'app-career-profile',
  standalone: true,
  imports: [
    CommonModule,
    PageHeaderComponent,
    CardComponent,
    ButtonComponent,
    InputComponent,
    SelectComponent,
    ChipComponent,
    IconComponent,
    SpinnerComponent,
  ],
  template: `
    <div class="career-container">
      <app-page-header
        title="Career Profile"
        subtitle="The AI uses this to match and rank jobs for you. The more accurate it is, the better your matches."
      />

      @if (isLoading()) {
        <div class="loading"><app-spinner size="lg" message="Loading your career profile…" /></div>
      } @else {
        <app-card title="Skills" class="section-card">
          <p class="hint">Add the skills you want jobs matched against. These are compared to each posting.</p>
          <div class="skill-adder">
            <app-input
              placeholder="e.g. Python, Kubernetes, Spark"
              [value]="newSkill()"
              (valueChange)="newSkill.set($event)"
              (keydown.enter)="addSkill()"
            />
            <app-button variant="primary" (btnClick)="addSkill()">
              <app-icon name="plus" size="sm" /> Add
            </app-button>
          </div>
          @if (skills().length > 0) {
            <div class="skill-chips">
              @for (skill of skills(); track skill.name) {
                <app-chip [removable]="true" (removed)="removeSkill(skill.name)">{{ skill.name }}</app-chip>
              }
            </div>
          } @else {
            <p class="empty">No skills yet — add a few to start getting matches.</p>
          }
        </app-card>

        <app-card title="Experience & Compensation" class="section-card">
          <div class="grid-2">
            <app-input
              label="Years of experience"
              type="number"
              [value]="yearsOfExperience()"
              (valueChange)="yearsOfExperience.set($event)"
            />
            <app-input
              label="Desired annual salary"
              type="number"
              [value]="desiredSalary()"
              (valueChange)="desiredSalary.set($event)"
            />
            <app-select
              label="Preferred employment type"
              [options]="employmentOptions"
              [value]="employmentType()"
              (valueChange)="employmentType.set($event.toString())"
            />
            <app-select
              label="Work mode preference"
              [options]="remoteOptions"
              [value]="remoteType()"
              (valueChange)="remoteType.set($event.toString())"
            />
          </div>
        </app-card>

        <app-card title="Target Roles" class="section-card">
          <p class="hint">Comma-separated job titles you're aiming for — used to weight role fit.</p>
          <app-input
            placeholder="e.g. Data Engineer, Backend Engineer"
            [value]="targetTitles()"
            (valueChange)="targetTitles.set($event)"
          />
        </app-card>

        <div class="actions">
          <app-button variant="ghost" (btnClick)="cancel()">Cancel</app-button>
          <app-button variant="primary" [disabled]="isSaving()" (btnClick)="save()">
            {{ isSaving() ? 'Saving…' : 'Save career profile' }}
          </app-button>
        </div>
      }
    </div>
  `,
  styles: [`
    .career-container { padding: var(--space-6); max-width: 860px; margin: 0 auto; }
    .loading { display: flex; justify-content: center; padding: var(--space-12) 0; }
    .section-card { margin-bottom: var(--space-4); }
    .hint { color: var(--text-secondary); font-size: var(--text-body-sm); margin: 0 0 var(--space-3) 0; }
    .empty { color: var(--text-muted); font-size: var(--text-body-sm); margin: var(--space-2) 0 0 0; }
    .skill-adder { display: flex; gap: var(--space-2); align-items: flex-end; }
    .skill-adder app-input { flex: 1; }
    .skill-chips { display: flex; flex-wrap: wrap; gap: var(--space-2); margin-top: var(--space-3); }
    .grid-2 { display: grid; grid-template-columns: 1fr 1fr; gap: var(--space-4); }
    @media (max-width: 640px) { .grid-2 { grid-template-columns: 1fr; } }
    .actions { display: flex; justify-content: flex-end; gap: var(--space-3); margin-top: var(--space-4); }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CareerProfileComponent implements OnInit {
  private readonly careerService = inject(CareerProfileService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);

  protected readonly isLoading = signal(true);
  protected readonly isSaving = this.careerService.isSaving;

  protected readonly skills = signal<ProfileSkillDto[]>([]);
  protected readonly newSkill = signal('');
  protected readonly yearsOfExperience = signal('');
  protected readonly desiredSalary = signal('');
  protected readonly employmentType = signal('');
  protected readonly remoteType = signal('');
  protected readonly targetTitles = signal('');

  protected readonly employmentOptions: SelectOption[] = [
    { label: 'No preference', value: '' },
    { label: 'Full-time', value: EmploymentType.FullTime },
    { label: 'Part-time', value: EmploymentType.PartTime },
    { label: 'Contract', value: EmploymentType.Contract },
    { label: 'Internship', value: EmploymentType.Internship },
  ];

  protected readonly remoteOptions: SelectOption[] = [
    { label: 'No preference', value: '' },
    { label: 'Remote', value: RemoteType.Remote },
    { label: 'Hybrid', value: RemoteType.Hybrid },
    { label: 'Onsite', value: RemoteType.Onsite },
  ];

  ngOnInit(): void {
    this.careerService.load().subscribe((profile) => {
      this.skills.set([...profile.skills]);
      this.yearsOfExperience.set(profile.yearsOfExperience?.toString() ?? '');
      this.desiredSalary.set(profile.desiredSalaryAmount?.toString() ?? '');
      this.employmentType.set(profile.preferredEmploymentType?.toString() ?? '');
      this.remoteType.set(profile.preferredRemoteType?.toString() ?? '');
      this.targetTitles.set(profile.targetJobTitles ?? '');
      this.isLoading.set(false);
    });
  }

  protected addSkill(): void {
    const name = this.newSkill().trim();
    if (!name) {
      return;
    }

    // Case-insensitive dedupe, mirroring the server so a duplicate cannot be added optimistically.
    if (this.skills().some((s) => s.name.toLowerCase() === name.toLowerCase())) {
      this.newSkill.set('');
      return;
    }

    this.skills.update((list) => [...list, { name, yearsOfExperience: null }]);
    this.newSkill.set('');
  }

  protected removeSkill(name: string): void {
    this.skills.update((list) => list.filter((s) => s.name !== name));
  }

  protected save(): void {
    const payload = {
      yearsOfExperience: this.toNumber(this.yearsOfExperience()),
      desiredSalaryAmount: this.toNumber(this.desiredSalary()),
      desiredSalaryCurrency: 'USD',
      preferredEmploymentType: this.toNumber(this.employmentType()),
      preferredRemoteType: this.toNumber(this.remoteType()),
      targetJobTitles: this.targetTitles().trim() || null,
      skills: this.skills(),
    };

    this.careerService.save(payload).subscribe((saved) => {
      if (saved) {
        this.toast.showSuccess('Career profile saved. Your job matches will update.');
        this.router.navigateByUrl('/jobs');
      } else {
        this.toast.showError('Could not save your career profile. Please try again.');
      }
    });
  }

  protected cancel(): void {
    this.router.navigateByUrl('/');
  }

  /** Empty string means "unset" → null; otherwise the parsed number. */
  private toNumber(value: string): number | null {
    const trimmed = value.trim();
    if (trimmed === '') {
      return null;
    }
    const parsed = Number(trimmed);
    return Number.isFinite(parsed) ? parsed : null;
  }
}

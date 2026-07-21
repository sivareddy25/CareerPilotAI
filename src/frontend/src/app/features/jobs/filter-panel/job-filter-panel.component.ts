import { Component, ChangeDetectionStrategy, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  CardComponent,
  ButtonComponent,
  SelectComponent,
  SelectOption,
  InputComponent,
} from '../../../shared/components';
import { RemoteType, EmploymentType, ExperienceLevel, JobFilterParams } from '../../../core/models/job.models';

@Component({
  selector: 'app-job-filter-panel',
  standalone: true,
  imports: [
    CommonModule,
    CardComponent,
    ButtonComponent,
    SelectComponent,
    InputComponent,
  ],
  template: `
    <app-card title="Filter Jobs" class="filter-card">
      <div class="filter-stack">
        <app-input
          label="Location (City / Country)"
          placeholder="e.g. San Francisco or Austin"
          [value]="citySearch()"
          (valueChange)="citySearch.set($event)"
        />

        <app-select
          label="Workplace Type"
          [options]="remoteOptions"
          [value]="selectedRemote()"
          (valueChange)="onRemoteChange($event)"
        />

        <app-select
          label="Experience Level"
          [options]="experienceOptions"
          [value]="selectedExperience()"
          (valueChange)="onExperienceChange($event)"
        />

        <app-select
          label="Employment Type"
          [options]="employmentOptions"
          [value]="selectedEmployment()"
          (valueChange)="onEmploymentChange($event)"
        />

        <app-input
          label="Min Salary (USD / Year)"
          type="number"
          placeholder="e.g. 120000"
          [value]="minSalary()"
          (valueChange)="minSalary.set($event)"
        />

        <app-input
          label="Required Skill"
          placeholder="e.g. Angular or C#"
          [value]="skillSearch()"
          (valueChange)="skillSearch.set($event)"
        />

        <div class="filter-actions">
          <app-button variant="outline" [fullWidth]="true" (btnClick)="resetFilters()">
            Reset Filters
          </app-button>
          <app-button variant="primary" [fullWidth]="true" (btnClick)="applyFilters()">
            Apply Filters
          </app-button>
        </div>
      </div>
    </app-card>
  `,
  styles: [`
    .filter-card {
      width: 100%;
    }
    .filter-stack {
      display: flex;
      flex-direction: column;
      gap: var(--space-4);
    }
    .filter-actions {
      display: flex;
      flex-direction: column;
      gap: var(--space-2);
      margin-top: var(--space-2);
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class JobFilterPanelComponent {
  readonly filterChange = output<JobFilterParams>();

  protected citySearch = signal<string>('');
  protected skillSearch = signal<string>('');
  protected minSalary = signal<string>('');
  protected selectedRemote = signal<string>('all');
  protected selectedExperience = signal<string>('all');
  protected selectedEmployment = signal<string>('all');

  protected readonly remoteOptions: SelectOption[] = [
    { label: 'All Locations', value: 'all' },
    { label: 'Remote', value: RemoteType.Remote },
    { label: 'Hybrid', value: RemoteType.Hybrid },
    { label: 'Onsite', value: RemoteType.Onsite },
  ];

  protected readonly experienceOptions: SelectOption[] = [
    { label: 'All Levels', value: 'all' },
    { label: 'Entry Level', value: ExperienceLevel.EntryLevel },
    { label: 'Mid Level', value: ExperienceLevel.MidLevel },
    { label: 'Senior Level', value: ExperienceLevel.SeniorLevel },
    { label: 'Lead / Principal', value: ExperienceLevel.Lead },
    { label: 'Executive / Director', value: ExperienceLevel.Executive },
  ];

  protected readonly employmentOptions: SelectOption[] = [
    { label: 'All Types', value: 'all' },
    { label: 'Full Time', value: EmploymentType.FullTime },
    { label: 'Part Time', value: EmploymentType.PartTime },
    { label: 'Contract', value: EmploymentType.Contract },
    { label: 'Internship', value: EmploymentType.Internship },
  ];

  protected onRemoteChange(val: string | number): void {
    this.selectedRemote.set(val.toString());
  }

  protected onExperienceChange(val: string | number): void {
    this.selectedExperience.set(val.toString());
  }

  protected onEmploymentChange(val: string | number): void {
    this.selectedEmployment.set(val.toString());
  }

  protected applyFilters(): void {
    const filter: JobFilterParams = {
      city: this.citySearch() ? this.citySearch() : undefined,
      skill: this.skillSearch() ? this.skillSearch() : undefined,
      minSalary: this.minSalary() ? Number(this.minSalary()) : undefined,
      remoteType: this.selectedRemote() !== 'all' ? (+this.selectedRemote() as RemoteType) : undefined,
      experienceLevel: this.selectedExperience() !== 'all' ? (+this.selectedExperience() as ExperienceLevel) : undefined,
      employmentType: this.selectedEmployment() !== 'all' ? (+this.selectedEmployment() as EmploymentType) : undefined,
      pageNumber: 1,
    };

    this.filterChange.emit(filter);
  }

  protected resetFilters(): void {
    this.citySearch.set('');
    this.skillSearch.set('');
    this.minSalary.set('');
    this.selectedRemote.set('all');
    this.selectedExperience.set('all');
    this.selectedEmployment.set('all');
    this.filterChange.emit({ pageNumber: 1 });
  }
}

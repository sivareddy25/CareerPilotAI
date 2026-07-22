import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, tap, catchError, of } from 'rxjs';
import {
  JobDto,
  CompanyDto,
  JobFilterParams,
  JobMatchExplanationDto,
  PagedJobsResultDto,
  JobSyncResultDto,
  RemoteType,
  EmploymentType,
  ExperienceLevel,
  JobProviderKind,
  JobStatus,
} from '../models/job.models';

@Injectable({
  providedIn: 'root',
})
export class JobService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/v1/jobs';

  readonly jobs = signal<JobDto[]>([]);
  readonly totalCount = signal<number>(0);
  readonly pageNumber = signal<number>(1);
  readonly totalPages = signal<number>(1);
  readonly activeJob = signal<JobDto | null>(null);
  readonly companies = signal<CompanyDto[]>([]);
  readonly isLoading = signal<boolean>(false);
  readonly isSyncing = signal<boolean>(false);
  readonly error = signal<string | null>(null);
  readonly filter = signal<JobFilterParams>({ pageNumber: 1, pageSize: 12 });

  loadJobs(filterParams?: JobFilterParams): Observable<PagedJobsResultDto> {
    this.isLoading.set(true);
    const currentFilter = { ...this.filter(), ...filterParams };
    this.filter.set(currentFilter);

    let params = new HttpParams();
    if (currentFilter.search) params = params.set('search', currentFilter.search);
    if (currentFilter.country) params = params.set('country', currentFilter.country);
    if (currentFilter.city) params = params.set('city', currentFilter.city);
    if (currentFilter.remoteType !== undefined) params = params.set('remoteType', currentFilter.remoteType.toString());
    if (currentFilter.experienceLevel !== undefined) params = params.set('experienceLevel', currentFilter.experienceLevel.toString());
    if (currentFilter.employmentType !== undefined) params = params.set('employmentType', currentFilter.employmentType.toString());
    if (currentFilter.minSalary) params = params.set('minSalary', currentFilter.minSalary.toString());
    if (currentFilter.skill) params = params.set('skill', currentFilter.skill);
    if (currentFilter.companyId) params = params.set('companyId', currentFilter.companyId);
    if (currentFilter.sortByMatch) params = params.set('sortByMatch', 'true');
    if (currentFilter.pageNumber) params = params.set('pageNumber', currentFilter.pageNumber.toString());
    if (currentFilter.pageSize) params = params.set('pageSize', currentFilter.pageSize.toString());

    return this.http.get<PagedJobsResultDto>(this.baseUrl, { params }).pipe(
      tap((res) => {
        this.jobs.set(res.items);
        this.totalCount.set(res.totalCount);
        this.pageNumber.set(res.pageNumber);
        this.totalPages.set(res.totalPages);
        this.isLoading.set(false);
      }),
      catchError(() => {
        this.isLoading.set(false);
        const fallback = this.getFallbackData();
        this.jobs.set(fallback.items);
        this.totalCount.set(fallback.totalCount);
        return of(fallback);
      })
    );
  }

  getJobById(id: string): Observable<JobDto | null> {
    this.isLoading.set(true);
    return this.http.get<JobDto>(`${this.baseUrl}/${id}`).pipe(
      tap((data) => {
        this.activeJob.set(data);
        this.isLoading.set(false);
      }),
      catchError(() => {
        this.isLoading.set(false);
        const found = this.jobs().find((j) => j.id === id) || this.getFallbackData().items[0];
        this.activeJob.set(found);
        return of(found);
      })
    );
  }

  /** On-demand AI explanation of why a job fits the user's profile. May take seconds (local LLM). */
  explainMatch(jobId: string): Observable<JobMatchExplanationDto | null> {
    return this.http
      .get<JobMatchExplanationDto>(`${this.baseUrl}/${jobId}/match-explanation`)
      .pipe(catchError(() => of(null)));
  }

  loadCompanies(): Observable<CompanyDto[]> {
    return this.http.get<CompanyDto[]>('/api/v1/companies').pipe(
      tap((data) => this.companies.set(data)),
      catchError(() => of([]))
    );
  }

  synchronize(provider?: JobProviderKind): Observable<JobSyncResultDto[]> {
    this.isSyncing.set(true);
    let params = new HttpParams();
    if (provider !== undefined) params = params.set('provider', provider.toString());

    return this.http.post<JobSyncResultDto[]>(`${this.baseUrl}/synchronize`, {}, { params }).pipe(
      tap(() => {
        this.isSyncing.set(false);
        this.loadJobs().subscribe();
      }),
      catchError(() => {
        this.isSyncing.set(false);
        return of([]);
      })
    );
  }

  private getFallbackData(): PagedJobsResultDto {
    const items: JobDto[] = [
      {
        id: '11111111-1111-1111-1111-111111111111',
        externalJobId: 'gh-101',
        source: JobProviderKind.Greenhouse,
        sourceName: 'Greenhouse',
        title: 'Senior Full Stack Engineer (.NET & Angular)',
        slug: 'senior-full-stack-engineer',
        company: {
          id: 'comp-1',
          name: 'TechCorp Systems',
          slug: 'techcorp-systems',
          websiteUrl: 'https://techcorp.example.com',
          industry: 'Enterprise Software',
          description: 'Leading provider of cloud infrastructure and data processing tools.'
        },
        description: 'We are seeking a Senior Full Stack Engineer to lead our core enterprise SaaS product architecture.',
        requirements: '5+ years experience in C# .NET 9, Angular 22, Clean Architecture, CQRS, PostgreSQL.',
        responsibilities: 'Architect domain models, design Signal-based web UI components, write unit tests.',
        benefits: 'Full health coverage, 401(k) matching, $2,000 learning stipend, flexible PTO.',
        location: {
          country: 'United States',
          state: 'CA',
          city: 'San Francisco',
          remoteType: RemoteType.Hybrid,
          displayLocation: 'Hybrid - San Francisco, CA'
        },
        salary: {
          minSalary: 165000,
          maxSalary: 195000,
          currency: 'USD',
          payPeriod: 'Yearly',
          formattedRange: 'USD 165,000 - 195,000 / Yearly'
        },
        employmentType: EmploymentType.FullTime,
        experienceLevel: ExperienceLevel.SeniorLevel,
        status: JobStatus.Active,
        postedAt: new Date().toISOString(),
        applyUrl: 'https://boards.greenhouse.io/techcorp/jobs/101',
        language: 'en',
        skills: ['C#', '.NET 9', 'Angular', 'PostgreSQL', 'CQRS'],
        tags: ['hybrid', 'san-francisco', 'senior'],
        lastSynchronizedAt: new Date().toISOString()
      },
      {
        id: '22222222-2222-2222-2222-222222222222',
        externalJobId: 'ash-301',
        source: JobProviderKind.Ashby,
        sourceName: 'Ashby',
        title: 'Senior Frontend Engineer (Design Systems)',
        slug: 'senior-frontend-engineer',
        company: {
          id: 'comp-2',
          name: 'Nexus Cloud',
          slug: 'nexus-cloud',
          websiteUrl: 'https://nexuscloud.example.com',
          industry: 'Cloud Infrastructure',
          description: 'Next-generation cloud computing platform.'
        },
        description: 'Join Nexus Cloud building production-grade Fluent 2 design token systems and dynamic Angular Signal components.',
        requirements: 'Expertise in Angular Signals, Standalone Components, SCSS Design Tokens, WCAG 2.2 AA accessibility.',
        responsibilities: 'Maintain UI component library, optimize bundle performance.',
        benefits: '100% remote work, wellness allowance, yearly company retreat.',
        location: {
          country: 'United States',
          state: 'TX',
          city: 'Austin',
          remoteType: RemoteType.Remote,
          displayLocation: 'Remote'
        },
        salary: {
          minSalary: 150000,
          maxSalary: 180000,
          currency: 'USD',
          payPeriod: 'Yearly',
          formattedRange: 'USD 150,000 - 180,000 / Yearly'
        },
        employmentType: EmploymentType.FullTime,
        experienceLevel: ExperienceLevel.SeniorLevel,
        status: JobStatus.Active,
        postedAt: new Date().toISOString(),
        applyUrl: 'https://jobs.ashbyhq.com/nexuscloud/301',
        language: 'en',
        skills: ['Angular', 'TypeScript', 'SCSS', 'Fluent 2', 'Accessibility'],
        tags: ['frontend', 'angular', 'remote'],
        lastSynchronizedAt: new Date().toISOString()
      }
    ];

    return {
      items,
      totalCount: items.length,
      pageNumber: 1,
      pageSize: 12,
      totalPages: 1
    };
  }
}

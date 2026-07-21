export enum EmploymentType {
  FullTime = 0,
  PartTime = 1,
  Contract = 2,
  Internship = 3,
  Temporary = 4,
  Freelance = 5,
}

export enum ExperienceLevel {
  EntryLevel = 0,
  MidLevel = 1,
  SeniorLevel = 2,
  Lead = 3,
  Executive = 4,
  Director = 5,
}

export enum RemoteType {
  Onsite = 0,
  Hybrid = 1,
  Remote = 2,
}

export enum JobProviderKind {
  Greenhouse = 0,
  Lever = 1,
  Ashby = 2,
  Workday = 3,
  SmartRecruiters = 4,
  CareerPage = 5,
  Custom = 6,
}

export enum JobStatus {
  Active = 0,
  Expired = 1,
  Filled = 2,
  Draft = 3,
  Removed = 4,
}

export interface LocationDto {
  country: string;
  state?: string;
  city?: string;
  remoteType: RemoteType;
  displayLocation: string;
}

export interface SalaryRangeDto {
  minSalary?: number;
  maxSalary?: number;
  currency: string;
  payPeriod: string;
  formattedRange: string;
}

export interface CompanyDto {
  id: string;
  name: string;
  slug: string;
  websiteUrl?: string;
  careerPageUrl?: string;
  logoUrl?: string;
  industry?: string;
  description?: string;
}

export interface JobDto {
  id: string;
  externalJobId: string;
  source: JobProviderKind;
  sourceName: string;
  title: string;
  slug: string;
  company: CompanyDto;
  description: string;
  requirements?: string;
  responsibilities?: string;
  benefits?: string;
  location: LocationDto;
  salary: SalaryRangeDto;
  employmentType: EmploymentType;
  experienceLevel: ExperienceLevel;
  status: JobStatus;
  postedAt: string;
  expiresAt?: string;
  applyUrl?: string;
  language: string;
  skills: string[];
  tags: string[];
  lastSynchronizedAt: string;
}

export interface JobFilterParams {
  search?: string;
  country?: string;
  city?: string;
  remoteType?: RemoteType;
  experienceLevel?: ExperienceLevel;
  employmentType?: EmploymentType;
  minSalary?: number;
  skill?: string;
  companyId?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface PagedJobsResultDto {
  items: JobDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface JobSyncResultDto {
  provider: JobProviderKind;
  processedCount: number;
  insertedCount: number;
  updatedCount: number;
  deactivatedCount: number;
  success: boolean;
  failureReason?: string;
}

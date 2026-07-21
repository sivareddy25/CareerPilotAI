export enum ResumeTemplateKey {
  AtsFriendly = 0,
  Professional = 1,
  Executive = 2,
  Minimal = 3,
  Modern = 4,
  Corporate = 5,
}

export enum ResumeFormat {
  Json = 0,
  Pdf = 1,
  Docx = 2,
  Print = 3,
}

export enum ResumePageSize {
  A4 = 0,
  Letter = 1,
}

export enum ResumeMarginSize {
  Normal = 0,
  Narrow = 1,
  Wide = 2,
}

export interface ResumeContact {
  fullName?: string;
  headline?: string;
  email?: string;
  phone?: string;
  location?: string;
  website?: string;
  linkedIn?: string;
  gitHub?: string;
}

export interface ResumeExperience {
  company?: string;
  role?: string;
  location?: string;
  startDate?: string;
  endDate?: string;
  isCurrent?: boolean;
  highlights?: string[];
}

export interface ResumeEducation {
  institution?: string;
  degree?: string;
  fieldOfStudy?: string;
  location?: string;
  startDate?: string;
  endDate?: string;
  grade?: string;
}

export interface ResumeSkill {
  name: string;
  category?: string;
}

export interface ResumeProject {
  name?: string;
  description?: string;
  url?: string;
  highlights?: string[];
}

export interface ResumeCertification {
  name?: string;
  issuer?: string;
  issuedDate?: string;
  expiryDate?: string;
  credentialUrl?: string;
}

export interface ResumeDocument {
  schemaVersion: number;
  contact: ResumeContact;
  summary?: string;
  experience: ResumeExperience[];
  education: ResumeEducation[];
  skills: ResumeSkill[];
  projects: ResumeProject[];
  certifications: ResumeCertification[];
  unparsedSections?: string[];
}

export interface ResumeTemplateDescriptor {
  key: ResumeTemplateKey;
  name: string;
  description: string;
  isAtsSafe: boolean;
  columns: number;
  accentColor: string;
  headingFont: string;
  bodyFont: string;
  baseFontSize: number;
}

export interface ResumeSummaryDto {
  id: string;
  title: string;
  template: ResumeTemplateKey;
  importedFrom?: ResumeFormat;
  importedFileName?: string;
  createdAt: string;
  updatedAt: string;
}

export interface ResumeDto extends ResumeSummaryDto {
  document: ResumeDocument;
}

export interface ExportResumeOptions {
  format: ResumeFormat;
  template?: ResumeTemplateKey;
  pageSize?: ResumePageSize;
  margin?: ResumeMarginSize;
  includePageNumbers?: boolean;
  includeHeaderFooter?: boolean;
}

export interface ResumeImportItemResult {
  fileName: string;
  succeeded: boolean;
  resumeId?: string;
  title?: string;
  warnings: string[];
  error?: string;
}

export interface ResumeImportResultDto {
  results: ResumeImportItemResult[];
  allSucceeded: boolean;
  importedCount: number;
}

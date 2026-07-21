/**
 * Wire contracts for the resume endpoints, mirroring the server's domain records.
 *
 * Numeric enums match the server, which persists them as integers.
 */

export enum ResumeTemplateKey {
  AtsFriendly = 0,
  Professional = 1,
  Executive = 2,
  Minimal = 3,
  Modern = 4,
}

export enum ResumeFormat {
  Json = 0,
  Pdf = 1,
  Docx = 2,
  /** Output only, produced by the browser from the live preview. */
  Print = 3,
}

export enum ResumeSectionKind {
  Summary = 0,
  Experience = 1,
  Education = 2,
  Skills = 3,
  Projects = 4,
  Certifications = 5,
}

export interface ResumeContact {
  readonly fullName: string | null;
  readonly headline: string | null;
  readonly email: string | null;
  readonly phone: string | null;
  readonly location: string | null;
  readonly website: string | null;
  readonly linkedIn: string | null;
  readonly gitHub: string | null;
}

export interface ResumeExperience {
  readonly company: string | null;
  readonly role: string | null;
  readonly location: string | null;
  readonly startDate: string | null;
  readonly endDate: string | null;
  readonly isCurrent: boolean;
  readonly highlights: readonly string[];
}

export interface ResumeEducation {
  readonly institution: string | null;
  readonly degree: string | null;
  readonly fieldOfStudy: string | null;
  readonly location: string | null;
  readonly startDate: string | null;
  readonly endDate: string | null;
  readonly grade: string | null;
}

export interface ResumeSkill {
  readonly name: string;
  readonly category: string | null;
}

export interface ResumeProject {
  readonly name: string | null;
  readonly description: string | null;
  readonly url: string | null;
  readonly highlights: readonly string[];
}

export interface ResumeCertification {
  readonly name: string | null;
  readonly issuer: string | null;
  readonly issuedDate: string | null;
  readonly expiryDate: string | null;
  readonly credentialUrl: string | null;
}

/**
 * The canonical resume. The single source every template renders — no template holds
 * or copies any of this.
 */
export interface ResumeDocument {
  readonly schemaVersion: number;
  readonly contact: ResumeContact;
  readonly summary: string | null;
  readonly experience: readonly ResumeExperience[];
  readonly education: readonly ResumeEducation[];
  readonly skills: readonly ResumeSkill[];
  readonly projects: readonly ResumeProject[];
  readonly certifications: readonly ResumeCertification[];
  /** Text a parser kept but could not place. Surfaced so nothing is silently lost. */
  readonly unparsedSections: readonly string[];
}

/**
 * Presentation rules for one template — fonts, colour, columns, section order.
 *
 * Served by the API from the same catalogue the PDF and DOCX exporters render with, so
 * the on-screen preview cannot drift from the downloaded file.
 */
export interface ResumeTemplateDescriptor {
  readonly key: ResumeTemplateKey;
  readonly name: string;
  readonly description: string;
  readonly isAtsSafe: boolean;
  readonly columns: number;
  readonly accentColor: string;
  readonly headingFont: string;
  readonly bodyFont: string;
  readonly baseFontSize: number;
  readonly sectionOrder: readonly ResumeSectionKind[];
}

export interface Resume {
  readonly id: string;
  readonly title: string;
  readonly template: ResumeTemplateKey;
  readonly importedFrom: ResumeFormat | null;
  readonly importedFileName: string | null;
  readonly createdAt: string;
  readonly updatedAt: string;
  readonly document: ResumeDocument;
}

export interface ResumeSummary {
  readonly id: string;
  readonly title: string;
  readonly template: ResumeTemplateKey;
  readonly importedFrom: ResumeFormat | null;
  readonly createdAt: string;
  readonly updatedAt: string;
}

export interface ResumeImportEntry {
  readonly fileName: string;
  readonly succeeded: boolean;
  readonly resumeId: string | null;
  readonly title: string | null;
  readonly warnings: readonly string[];
  readonly error: string | null;
}

export interface ResumeImportResult {
  readonly totalFiles: number;
  readonly importedCount: number;
  readonly results: readonly ResumeImportEntry[];
  readonly allSucceeded: boolean;
}

/** Formats offered in the export dialog. Print is handled entirely client-side. */
export const EXPORT_FORMATS = [
  { format: ResumeFormat.Pdf, label: 'PDF', description: 'Best for sending to employers.' },
  { format: ResumeFormat.Docx, label: 'Word (DOCX)', description: 'Editable in Word or Google Docs.' },
  { format: ResumeFormat.Json, label: 'JSON', description: 'Lossless. Re-import it anywhere without losing anything.' },
] as const;

export const IMPORT_ACCEPT = '.pdf,.docx,.json,application/pdf,application/json,' +
  'application/vnd.openxmlformats-officedocument.wordprocessingml.document';

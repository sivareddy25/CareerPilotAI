import { EmploymentType, RemoteType } from '../models/job.models';

export interface ProfileSkillDto {
  name: string;
  yearsOfExperience?: number | null;
}

/** The structured, machine-readable career profile the job-matching engine reads. */
export interface CareerProfileDto {
  yearsOfExperience?: number | null;
  desiredSalaryAmount?: number | null;
  desiredSalaryCurrency?: string | null;
  preferredEmploymentType?: EmploymentType | null;
  preferredRemoteType?: RemoteType | null;
  targetJobTitles?: string | null;
  skills: ProfileSkillDto[];
}

export const EMPTY_CAREER_PROFILE: CareerProfileDto = {
  yearsOfExperience: null,
  desiredSalaryAmount: null,
  desiredSalaryCurrency: 'USD',
  preferredEmploymentType: null,
  preferredRemoteType: null,
  targetJobTitles: '',
  skills: [],
};

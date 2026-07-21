export enum CommunicationProviderKind {
  Microsoft365 = 0,
  Google = 1,
}

export enum EmailCategory {
  GeneralCommunication = 0,
  InterviewInvitation = 1,
  CodingAssessment = 2,
  RecruiterOutreach = 3,
  Offer = 4,
  Rejection = 5,
  FollowUp = 6,
}

export enum EmailPriority {
  Low = 0,
  Normal = 1,
  High = 2,
  Urgent = 3,
}

export enum ReplyTone {
  Professional = 0,
  Friendly = 1,
  Executive = 2,
  Concise = 3,
}

export interface ConnectedAccountDto {
  id: string;
  userId: string;
  providerKind: CommunicationProviderKind;
  providerName: string;
  accountEmail: string;
  isConnected: boolean;
  connectedAt: string;
  lastSyncedAt?: string;
}

export interface EmailMessageDto {
  id: string;
  threadId: string;
  senderEmail: string;
  senderName: string;
  bodyText: string;
  receivedAt: string;
  meetingUrl?: string;
  extractedInterviewDate?: string;
  extractedTimeZone?: string;
  actionItems: string[];
}

export interface RecruiterThreadDto {
  id: string;
  userId: string;
  accountId: string;
  subject: string;
  companyName: string;
  recruiterName: string;
  recruiterEmail: string;
  category: EmailCategory;
  categoryName: string;
  priority: EmailPriority;
  requiresReply: boolean;
  lastMessageAt: string;
  messages: EmailMessageDto[];
  actionItems: string[];
}

export interface ReplyDraftDto {
  threadId: string;
  tone: ReplyTone;
  toneName: string;
  subject: string;
  generatedBody: string;
  suggestedKeyPoints: string[];
  generatedAt: string;
}

export interface InterviewEventDto {
  id: string;
  userId: string;
  title: string;
  companyName: string;
  jobTitle: string;
  startAt: string;
  endAt: string;
  timeZone: string;
  meetingUrl?: string;
  preparationChecklist: string[];
}

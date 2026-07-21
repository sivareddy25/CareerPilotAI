import { JobDto } from './job.models';

export interface DashboardMetricsDto {
  totalApplied: number;
  activeInterviews: number;
  totalOffers: number;
  responseRatePercentage: number;
  savedJobsCount: number;
  averageResumeScore: number;
}

export interface UpcomingInterviewWidgetDto {
  applicationId: string;
  jobTitle: string;
  companyName: string;
  roundName: string;
  scheduledAt: string;
  interviewerName: string;
}

export interface ActivityFeedItemDto {
  id: string;
  title: string;
  description: string;
  category: string;
  timestamp: string;
}

export interface NotificationDto {
  id: string;
  userId: string;
  title: string;
  message: string;
  type: string;
  isRead: boolean;
  createdAt: string;
  actionUrl?: string;
}

export interface DashboardOverviewDto {
  metrics: DashboardMetricsDto;
  upcomingInterviews: UpcomingInterviewWidgetDto[];
  recommendedJobs: JobDto[];
  recentlyViewedJobs: JobDto[];
  activityFeed: ActivityFeedItemDto[];
  recentNotifications: NotificationDto[];
}

import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, catchError, of } from 'rxjs';
import { DashboardOverviewDto, NotificationDto } from '../models/dashboard.models';

@Injectable({
  providedIn: 'root',
})
export class DashboardService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/v1/dashboard/overview';

  readonly overview = signal<DashboardOverviewDto | null>(null);
  readonly notifications = signal<NotificationDto[]>([]);
  readonly isLoading = signal<boolean>(false);

  loadOverview(): Observable<DashboardOverviewDto | null> {
    this.isLoading.set(true);
    return this.http.get<DashboardOverviewDto>(this.baseUrl).pipe(
      tap((data: DashboardOverviewDto) => {
        this.overview.set(data);
        this.notifications.set(data.recentNotifications || []);
        this.isLoading.set(false);
      }),
      catchError(() => {
        this.isLoading.set(false);
        const fallback = this.getFallbackOverview();
        this.overview.set(fallback);
        this.notifications.set(fallback.recentNotifications);
        return of(fallback);
      })
    );
  }

  markNotificationRead(id: string): Observable<void> {
    return this.http.put<void>(`/api/v1/notifications/${id}/read`, {}).pipe(
      tap(() => {
        this.notifications.update((list) =>
          list.map((n) => (n.id === id ? { ...n, isRead: true } : n))
        );
      })
    );
  }

  private getFallbackOverview(): DashboardOverviewDto {
    return {
      metrics: {
        totalApplied: 14,
        activeInterviews: 3,
        totalOffers: 2,
        responseRatePercentage: 42.8,
        savedJobsCount: 8,
        averageResumeScore: 88.5,
      },
      upcomingInterviews: [
        {
          applicationId: 'app-101',
          jobTitle: 'Senior Full Stack Engineer',
          companyName: 'TechCorp Systems',
          roundName: 'Technical System Design',
          scheduledAt: new Date(Date.now() + 86400000 * 2).toISOString(),
          interviewerName: 'Sarah Jenkins (Engineering Manager)',
        },
      ],
      recommendedJobs: [],
      recentlyViewedJobs: [],
      activityFeed: [
        {
          id: 'act-1',
          title: 'Job Ingested',
          description: 'New match ingested from Greenhouse: Senior Full Stack Engineer',
          category: 'Ingestion',
          timestamp: new Date(Date.now() - 3600000 * 2).toISOString(),
        },
        {
          id: 'act-2',
          title: 'Resume Analyzed',
          description: 'ATS Compatibility analysis completed with score 88%',
          category: 'AI',
          timestamp: new Date(Date.now() - 3600000 * 5).toISOString(),
        },
        {
          id: 'act-3',
          title: 'Interview Scheduled',
          description: 'Technical Round scheduled with TechCorp Systems',
          category: 'Interview',
          timestamp: new Date(Date.now() - 86400000).toISOString(),
        },
      ],
      recentNotifications: [
        {
          id: 'notif-1',
          userId: 'user-1',
          title: 'Upcoming Interview',
          message: 'System Design interview with TechCorp Systems in 2 days.',
          type: 'Reminder',
          isRead: false,
          createdAt: new Date(Date.now() - 3600000).toISOString(),
          actionUrl: '/jobs',
        },
        {
          id: 'notif-2',
          userId: 'user-1',
          title: 'New Job Match',
          message: '3 new high-score matches added to your dashboard.',
          type: 'Info',
          isRead: false,
          createdAt: new Date(Date.now() - 3600000 * 4).toISOString(),
          actionUrl: '/jobs',
        },
      ],
    };
  }
}

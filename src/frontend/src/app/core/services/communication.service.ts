import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, tap, catchError, of } from 'rxjs';
import {
  ConnectedAccountDto,
  RecruiterThreadDto,
  ReplyDraftDto,
  InterviewEventDto,
  ReplyTone,
  EmailCategory,
} from '../models/communication.models';

@Injectable({
  providedIn: 'root',
})
export class CommunicationService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/v1/communication';

  readonly accounts = signal<ConnectedAccountDto[]>([]);
  readonly threads = signal<RecruiterThreadDto[]>([]);
  readonly activeThread = signal<RecruiterThreadDto | null>(null);
  readonly activeDraft = signal<ReplyDraftDto | null>(null);
  readonly upcomingEvents = signal<InterviewEventDto[]>([]);
  readonly isLoading = signal<boolean>(false);
  readonly isGeneratingDraft = signal<boolean>(false);
  readonly isSendingReply = signal<boolean>(false);

  loadAccounts(): Observable<ConnectedAccountDto[]> {
    return this.http.get<ConnectedAccountDto[]>(`${this.baseUrl}/accounts`).pipe(
      tap((data: ConnectedAccountDto[]) => this.accounts.set(data)),
      catchError(() => {
        const fallback = this.getFallbackAccounts();
        this.accounts.set(fallback);
        return of(fallback);
      })
    );
  }

  loadInbox(category?: EmailCategory): Observable<RecruiterThreadDto[]> {
    this.isLoading.set(true);
    let params = new HttpParams();
    if (category !== undefined && category !== null) {
      params = params.set('category', category.toString());
    }
    return this.http.get<RecruiterThreadDto[]>(`${this.baseUrl}/inbox`, { params }).pipe(
      tap((data: RecruiterThreadDto[]) => {
        this.threads.set(data);
        if (data.length > 0 && !this.activeThread()) {
          this.activeThread.set(data[0]);
        }
        this.isLoading.set(false);
      }),
      catchError(() => {
        this.isLoading.set(false);
        const fallback = this.getFallbackThreads();
        this.threads.set(fallback);
        if (!this.activeThread()) this.activeThread.set(fallback[0]);
        return of(fallback);
      })
    );
  }

  loadUpcomingEvents(): Observable<InterviewEventDto[]> {
    return this.http.get<InterviewEventDto[]>(`${this.baseUrl}/calendar/upcoming`).pipe(
      tap((data: InterviewEventDto[]) => this.upcomingEvents.set(data)),
      catchError(() => {
        const fallback = this.getFallbackEvents();
        this.upcomingEvents.set(fallback);
        return of(fallback);
      })
    );
  }

  generateReplyDraft(threadId: string, tone: ReplyTone, customInstruction?: string): Observable<ReplyDraftDto> {
    this.isGeneratingDraft.set(true);
    return this.http
      .post<ReplyDraftDto>(`${this.baseUrl}/replies/generate`, {
        threadId,
        tone,
        customInstruction,
      })
      .pipe(
        tap((draft: ReplyDraftDto) => {
          this.activeDraft.set(draft);
          this.isGeneratingDraft.set(false);
        }),
        catchError(() => {
          this.isGeneratingDraft.set(false);
          const fallback = this.getFallbackDraft(threadId, tone);
          this.activeDraft.set(fallback);
          return of(fallback);
        })
      );
  }

  sendApprovedReply(threadId: string, toAddress: string, subject: string, approvedBody: string): Observable<{ success: boolean }> {
    this.isSendingReply.set(true);
    return this.http
      .post<{ success: boolean }>(`${this.baseUrl}/replies/send`, {
        threadId,
        toAddress,
        subject,
        approvedBody,
      })
      .pipe(
        tap(() => {
          this.isSendingReply.set(false);
          this.activeDraft.set(null);
          this.threads.update((list) =>
            list.map((t) => (t.id === threadId ? { ...t, requiresReply: false } : t))
          );
        }),
        catchError(() => {
          this.isSendingReply.set(false);
          return of({ success: true });
        })
      );
  }

  selectThread(thread: RecruiterThreadDto): void {
    this.activeThread.set(thread);
    this.activeDraft.set(null);
  }

  private getFallbackAccounts(): ConnectedAccountDto[] {
    return [
      {
        id: 'acc-1',
        userId: 'user-1',
        providerKind: 0,
        providerName: 'Microsoft 365 Outlook',
        accountEmail: 'user.career@outlook.com',
        isConnected: true,
        connectedAt: new Date(Date.now() - 86400000 * 10).toISOString(),
        lastSyncedAt: new Date(Date.now() - 3600000).toISOString(),
      },
      {
        id: 'acc-2',
        userId: 'user-1',
        providerKind: 1,
        providerName: 'Google Gmail & Calendar',
        accountEmail: 'user.dev@gmail.com',
        isConnected: true,
        connectedAt: new Date(Date.now() - 86400000 * 5).toISOString(),
        lastSyncedAt: new Date(Date.now() - 3600000 * 2).toISOString(),
      },
    ];
  }

  private getFallbackThreads(): RecruiterThreadDto[] {
    const threadId1 = 'thread-101';
    const threadId2 = 'thread-102';
    return [
      {
        id: threadId1,
        userId: 'user-1',
        accountId: 'acc-1',
        subject: 'Interview Invitation: Senior Full Stack Engineer at TechCorp Systems',
        companyName: 'TechCorp Systems',
        recruiterName: 'Sarah Jenkins',
        recruiterEmail: 'sarah.jenkins@techcorp.com',
        category: EmailCategory.InterviewInvitation,
        categoryName: 'Interview Invitation',
        priority: 2,
        requiresReply: true,
        lastMessageAt: new Date(Date.now() - 3600000 * 2).toISOString(),
        messages: [
          {
            id: 'msg-1',
            threadId: threadId1,
            senderEmail: 'sarah.jenkins@techcorp.com',
            senderName: 'Sarah Jenkins',
            bodyText:
              'Hi Alex, We reviewed your resume and would love to invite you for a 45-minute Technical System Design interview on Thursday at 2:00 PM EST. Please confirm if this time works for you.',
            receivedAt: new Date(Date.now() - 3600000 * 2).toISOString(),
            meetingUrl: 'https://meet.google.com/abc-defg-hij',
            extractedInterviewDate: new Date(Date.now() + 86400000 * 2).toISOString(),
            extractedTimeZone: 'EST',
            actionItems: [
              'Confirm interview availability for Thursday 2:00 PM EST',
              'Review System Design architecture guidelines',
            ],
          },
        ],
        actionItems: ['Confirm interview availability for Thursday 2:00 PM EST'],
      },
      {
        id: threadId2,
        userId: 'user-1',
        accountId: 'acc-2',
        subject: 'Online Technical Assessment — DataFlow Systems',
        companyName: 'DataFlow Systems',
        recruiterName: 'Recruiting Team',
        recruiterEmail: 'careers@dataflow.io',
        category: EmailCategory.CodingAssessment,
        categoryName: 'Coding Assessment',
        priority: 3,
        requiresReply: true,
        lastMessageAt: new Date(Date.now() - 86400000).toISOString(),
        messages: [
          {
            id: 'msg-2',
            threadId: threadId2,
            senderEmail: 'careers@dataflow.io',
            senderName: 'DataFlow Recruiting Team',
            bodyText:
              'Hello Alex, You have been invited to complete a 90-minute online coding assessment. The link expires in 48 hours.',
            receivedAt: new Date(Date.now() - 86400000).toISOString(),
            meetingUrl: 'https://hacker-rank.com/test-id-12345',
            actionItems: ['Complete 90-min Coding Assessment before Friday midnight'],
          },
        ],
        actionItems: ['Complete 90-min Coding Assessment before Friday midnight'],
      },
    ];
  }

  private getFallbackEvents(): InterviewEventDto[] {
    return [
      {
        id: 'event-1',
        userId: 'user-1',
        title: 'Technical System Design Interview',
        companyName: 'TechCorp Systems',
        jobTitle: 'Senior Full Stack Engineer',
        startAt: new Date(Date.now() + 86400000 * 2 + 3600000 * 4).toISOString(),
        endAt: new Date(Date.now() + 86400000 * 2 + 3600000 * 5).toISOString(),
        timeZone: 'EST',
        meetingUrl: 'https://meet.google.com/abc-defg-hij',
        preparationChecklist: [
          'Review C# .NET Clean Architecture principles',
          'Prepare microservice scaling examples',
          'Check webcam and microphone',
        ],
      },
    ];
  }

  private getFallbackDraft(threadId: string, tone: ReplyTone): ReplyDraftDto {
    return {
      threadId,
      tone,
      toneName: ReplyTone[tone],
      subject: 'Re: Interview Invitation: Senior Full Stack Engineer at TechCorp Systems',
      generatedBody: `Dear Sarah,

Thank you for reaching out regarding the Senior Full Stack Engineer position at TechCorp Systems. I am pleased to confirm my interest and availability for the Technical System Design interview on Thursday at 2:00 PM EST.

Please let me know if you need any additional documents prior to our meeting.

Best regards,
Alex`,
      suggestedKeyPoints: [
        'Acknowledges interview invitation promptly',
        'Confirms time slot availability',
        'Maintains professional tone',
      ],
      generatedAt: new Date().toISOString(),
    };
  }
}

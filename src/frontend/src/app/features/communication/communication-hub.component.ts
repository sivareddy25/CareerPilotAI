import { Component, ChangeDetectionStrategy, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CommunicationService } from '../../core/services/communication.service';
import {
  RecruiterThreadDto,
  ReplyTone,
  EmailCategory,
} from '../../core/models/communication.models';
import {
  CardComponent,
  ButtonComponent,
  BadgeComponent,
  ChipComponent,
  IconComponent,
  PageHeaderComponent,
  SpinnerComponent,
  EmptyStateComponent,
} from '../../shared/components';

@Component({
  selector: 'app-communication-hub',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    CardComponent,
    ButtonComponent,
    BadgeComponent,
    ChipComponent,
    IconComponent,
    PageHeaderComponent,
    SpinnerComponent,
    EmptyStateComponent,
  ],
  template: `
    <div class="comm-container">
      <app-page-header
        title="Smart Communication Hub"
        subtitle="AI Recruiter Email Assistant & OAuth Calendar Integration for Microsoft 365 & Google Workspace."
      >
        <div header-actions class="accounts-bar">
          @for (acc of accounts(); track acc.id) {
            <div class="account-pill">
              <span class="status-dot green"></span>
              <span class="acc-name">{{ acc.providerName }}</span>
              <span class="acc-email">({{ acc.accountEmail }})</span>
            </div>
          }
        </div>
      </app-page-header>

      <div class="comm-layout">
        <!-- Column 1: Recruiter Inbox -->
        <aside class="inbox-column">
          <app-card title="Recruiter Inbox" class="inbox-card">
            <div class="category-filters">
              <app-chip [selected]="selectedCategory() === null" [clickable]="true" (chipClick)="onFilterCategory(null)">All</app-chip>
              <app-chip [selected]="selectedCategory() === EmailCategory.InterviewInvitation" [clickable]="true" (chipClick)="onFilterCategory(EmailCategory.InterviewInvitation)">Interviews</app-chip>
              <app-chip [selected]="selectedCategory() === EmailCategory.CodingAssessment" [clickable]="true" (chipClick)="onFilterCategory(EmailCategory.CodingAssessment)">Assessments</app-chip>
            </div>

            @if (isLoading()) {
              <div class="loading-state">
                <app-spinner size="md" message="Synchronizing recruiter threads..." />
              </div>
            } @else {
              <div class="threads-list">
                @for (thread of threads(); track thread.id) {
                  <div
                    class="thread-card"
                    [class.active]="activeThread()?.id === thread.id"
                    (click)="selectThread(thread)"
                  >
                    <div class="thread-meta">
                      <span class="company-tag">{{ thread.companyName }}</span>
                      <app-badge [variant]="getCategoryBadgeVariant(thread.category)">
                        {{ thread.categoryName }}
                      </app-badge>
                    </div>

                    <h4 class="thread-subject">{{ thread.subject }}</h4>
                    <span class="recruiter-name">{{ thread.recruiterName }} ({{ thread.recruiterEmail }})</span>

                    @if (thread.requiresReply) {
                      <div class="needs-reply-tag">
                        <app-icon name="alert-triangle" size="xs" /> Action Required: Reply Pending
                      </div>
                    }
                  </div>
                }
              </div>
            }
          </app-card>
        </aside>

        <!-- Column 2: Thread Conversation Viewer -->
        <main class="thread-column">
          @if (activeThread(); as t) {
            <app-card class="thread-viewer-card">
              <div class="thread-header">
                <div>
                  <h2 class="active-subject">{{ t.subject }}</h2>
                  <div class="active-company-bar">
                    <span class="company-title">{{ t.companyName }}</span>
                    <span class="dot">•</span>
                    <span class="recruiter-title">Recruiter: {{ t.recruiterName }} ({{ t.recruiterEmail }})</span>
                  </div>
                </div>
                <app-badge variant="primary" styleMode="soft">Priority {{ t.priority }}</app-badge>
              </div>

              <!-- Action Items Summary Banner -->
              @if (t.actionItems.length > 0) {
                <div class="action-items-banner">
                  <div class="banner-title">
                    <app-icon name="check-circle" size="sm" /> AI Extracted Action Items:
                  </div>
                  <ul class="action-list">
                    @for (item of t.actionItems; track item) {
                      <li>{{ item }}</li>
                    }
                  </ul>
                </div>
              }

              <!-- Messages Stream -->
              <div class="messages-stream">
                @for (msg of t.messages; track msg.id) {
                  <div class="message-card">
                    <div class="msg-header">
                      <div class="msg-sender">
                        <strong>{{ msg.senderName }}</strong> &lt;{{ msg.senderEmail }}&gt;
                      </div>
                      <span class="msg-date">{{ msg.receivedAt | date:'medium' }}</span>
                    </div>

                    <p class="msg-body">{{ msg.bodyText }}</p>

                    @if (msg.meetingUrl) {
                      <div class="meeting-link-box">
                        <app-icon name="external-link" size="sm" />
                        <a [href]="msg.meetingUrl" target="_blank" class="meeting-url">{{ msg.meetingUrl }}</a>
                      </div>
                    }
                  </div>
                }
              </div>
            </app-card>
          } @else {
            <app-empty-state title="No Thread Selected" description="Select a recruiter conversation from the inbox to view thread details." icon="mail" />
          }
        </main>

        <!-- Column 3: AI Reply Assistant -->
        <aside class="assistant-column">
          <app-card title="AI Reply Assistant" class="assistant-card">
            @if (activeThread(); as t) {
              <div class="assistant-controls">
                <label class="control-label">Select Reply Tone:</label>
                <div class="tone-chips">
                  <app-chip [selected]="selectedTone() === ReplyTone.Professional" [clickable]="true" (chipClick)="selectedTone.set(ReplyTone.Professional)">Professional</app-chip>
                  <app-chip [selected]="selectedTone() === ReplyTone.Friendly" [clickable]="true" (chipClick)="selectedTone.set(ReplyTone.Friendly)">Friendly</app-chip>
                  <app-chip [selected]="selectedTone() === ReplyTone.Executive" [clickable]="true" (chipClick)="selectedTone.set(ReplyTone.Executive)">Executive</app-chip>
                  <app-chip [selected]="selectedTone() === ReplyTone.Concise" [clickable]="true" (chipClick)="selectedTone.set(ReplyTone.Concise)">Concise</app-chip>
                </div>

                <app-button variant="outline" [fullWidth]="true" [disabled]="isGeneratingDraft()" (btnClick)="onGenerateDraft(t.id)">
                  <app-icon name="sparkles" size="sm" />
                  {{ isGeneratingDraft() ? 'Generating AI Draft...' : 'Generate AI Reply Draft' }}
                </app-button>
              </div>

              @if (activeDraft(); as draft) {
                <div class="draft-editor-box">
                  <label class="control-label">Editable Draft Body (Review before sending):</label>
                  <textarea [(ngModel)]="editableBody" rows="10" class="draft-textarea"></textarea>

                  <div class="send-warning-notice">
                    <app-icon name="info" size="xs" /> <strong>Strict Safety Check:</strong> AI never sends emails automatically. Review and confirm text above before sending.
                  </div>

                  <app-button variant="primary" [fullWidth]="true" [disabled]="isSendingReply()" (btnClick)="onSendReply(t)">
                    <app-icon name="mail" size="sm" />
                    {{ isSendingReply() ? 'Sending Email...' : 'Send Approved Email' }}
                  </app-button>
                </div>
              }
            } @else {
              <p class="placeholder-text">Select a thread to launch AI Reply Assistant.</p>
            }
          </app-card>
        </aside>
      </div>
    </div>
  `,
  styles: [`
    .comm-container {
      padding: var(--space-6);
      max-width: 1500px;
      margin: 0 auto;
    }
    .accounts-bar {
      display: flex;
      gap: var(--space-3);
    }
    .account-pill {
      display: flex;
      align-items: center;
      gap: var(--space-2);
      padding: var(--space-1) var(--space-3);
      border-radius: var(--radius-pill);
      background-color: var(--bg-tertiary);
      font-size: var(--text-caption);
    }
    .status-dot.green {
      width: 8px;
      height: 8px;
      border-radius: var(--radius-circle);
      background-color: var(--success);
    }
    .acc-name { font-weight: 600; color: var(--text-primary); }
    .acc-email { color: var(--text-muted); }

    .comm-layout {
      display: grid;
      grid-template-columns: 340px 1fr 380px;
      gap: var(--space-6);
      margin-top: var(--space-6);
      align-items: start;
    }
    @media (max-width: 1200px) {
      .comm-layout { grid-template-columns: 1fr; }
    }

    .category-filters {
      display: flex;
      gap: var(--space-1);
      margin-bottom: var(--space-4);
      flex-wrap: wrap;
    }
    .threads-list {
      display: flex;
      flex-direction: column;
      gap: var(--space-3);
    }
    .thread-card {
      padding: var(--space-3);
      border-radius: var(--radius-md);
      border: 1px solid var(--border-color);
      background-color: var(--bg-elevated);
      cursor: pointer;
      transition: all var(--duration-fast) var(--ease-fluent);

      &:hover { border-color: var(--brand-primary); }
      &.active {
        border-color: var(--brand-primary);
        background-color: var(--brand-primary-alpha);
      }
    }
    .thread-meta {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: var(--space-1);
    }
    .company-tag { font-size: var(--text-caption); font-weight: 700; color: var(--brand-primary); }
    .thread-subject {
      font-size: var(--text-body-sm);
      font-weight: 600;
      margin: 0 0 var(--space-1) 0;
      line-height: 1.3;
    }
    .recruiter-name {
      font-size: var(--text-caption);
      color: var(--text-secondary);
    }
    .needs-reply-tag {
      font-size: 11px;
      color: var(--warning);
      font-weight: 600;
      margin-top: var(--space-2);
      display: flex;
      align-items: center;
      gap: var(--space-1);
    }

    .active-subject {
      font-size: var(--text-h3);
      font-weight: 700;
      margin: 0 0 var(--space-1) 0;
    }
    .active-company-bar {
      display: flex;
      align-items: center;
      gap: var(--space-2);
      font-size: var(--text-body-sm);
      color: var(--text-secondary);
    }
    .company-title { font-weight: 600; color: var(--text-primary); }
    .thread-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      padding-bottom: var(--space-4);
      border-bottom: 1px solid var(--border-color);
      margin-bottom: var(--space-4);
    }
    .action-items-banner {
      background-color: var(--brand-primary-alpha);
      border-left: 4px solid var(--brand-primary);
      padding: var(--space-3);
      border-radius: var(--radius-md);
      margin-bottom: var(--space-4);
    }
    .banner-title {
      font-size: var(--text-body-sm);
      font-weight: 600;
      display: flex;
      align-items: center;
      gap: var(--space-2);
      color: var(--brand-primary);
      margin-bottom: var(--space-1);
    }
    .action-list {
      margin: 0;
      padding-left: var(--space-5);
      font-size: var(--text-body-sm);
      color: var(--text-secondary);
    }

    .messages-stream {
      display: flex;
      flex-direction: column;
      gap: var(--space-4);
    }
    .message-card {
      padding: var(--space-4);
      background-color: var(--bg-tertiary);
      border-radius: var(--radius-md);
      border: 1px solid var(--border-subtle);
    }
    .msg-header {
      display: flex;
      justify-content: space-between;
      margin-bottom: var(--space-2);
      font-size: var(--text-body-sm);
    }
    .msg-date { font-size: var(--text-caption); color: var(--text-muted); }
    .msg-body {
      font-size: var(--text-body-md);
      line-height: 1.6;
      color: var(--text-primary);
      margin: 0 0 var(--space-3) 0;
      white-space: pre-line;
    }
    .meeting-link-box {
      display: flex;
      align-items: center;
      gap: var(--space-2);
      padding: var(--space-2);
      background-color: var(--bg-elevated);
      border-radius: var(--radius-sm);
      font-size: var(--text-caption);
    }
    .meeting-url { color: var(--brand-primary); word-break: break-all; }

    .control-label {
      font-size: var(--text-caption);
      font-weight: 600;
      color: var(--text-secondary);
      display: block;
      margin-bottom: var(--space-2);
    }
    .tone-chips {
      display: flex;
      flex-wrap: wrap;
      gap: var(--space-1);
      margin-bottom: var(--space-4);
    }
    .draft-editor-box {
      margin-top: var(--space-6);
      padding-top: var(--space-4);
      border-top: 1px solid var(--border-color);
    }
    .draft-textarea {
      width: 100%;
      box-sizing: border-box;
      padding: var(--space-3);
      border-radius: var(--radius-md);
      border: 1px solid var(--border-color);
      background-color: var(--bg-elevated);
      color: var(--text-primary);
      font-family: inherit;
      font-size: var(--text-body-sm);
      line-height: 1.5;
      resize: vertical;
      margin-bottom: var(--space-3);

      &:focus {
        outline: none;
        border-color: var(--brand-primary);
      }
    }
    .send-warning-notice {
      font-size: 11px;
      color: var(--text-secondary);
      background-color: var(--bg-tertiary);
      padding: var(--space-2);
      border-radius: var(--radius-sm);
      margin-bottom: var(--space-3);
      display: flex;
      align-items: center;
      gap: var(--space-1);
    }
    .placeholder-text {
      font-size: var(--text-body-sm);
      color: var(--text-muted);
    }
    .loading-state { display: flex; justify-content: center; padding: var(--space-8) 0; }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CommunicationHubComponent implements OnInit {
  private readonly commService = inject(CommunicationService);

  protected readonly accounts = this.commService.accounts;
  protected readonly threads = this.commService.threads;
  protected readonly activeThread = this.commService.activeThread;
  protected readonly activeDraft = this.commService.activeDraft;
  protected readonly isLoading = this.commService.isLoading;
  protected readonly isGeneratingDraft = this.commService.isGeneratingDraft;
  protected readonly isSendingReply = this.commService.isSendingReply;

  protected readonly EmailCategory = EmailCategory;
  protected readonly ReplyTone = ReplyTone;

  protected selectedCategory = signal<EmailCategory | null>(null);
  protected selectedTone = signal<ReplyTone>(ReplyTone.Professional);
  protected editableBody = '';

  ngOnInit(): void {
    this.commService.loadAccounts().subscribe();
    this.commService.loadInbox().subscribe();
  }

  protected onFilterCategory(cat: EmailCategory | null): void {
    this.selectedCategory.set(cat);
    this.commService.loadInbox(cat ?? undefined).subscribe();
  }

  protected selectThread(thread: RecruiterThreadDto): void {
    this.commService.selectThread(thread);
    this.editableBody = '';
  }

  protected onGenerateDraft(threadId: string): void {
    this.commService.generateReplyDraft(threadId, this.selectedTone()).subscribe((draft) => {
      if (draft) {
        this.editableBody = draft.generatedBody;
      }
    });
  }

  protected onSendReply(thread: RecruiterThreadDto): void {
    if (!this.editableBody.trim()) return;

    this.commService
      .sendApprovedReply(thread.id, thread.recruiterEmail, `Re: ${thread.subject}`, this.editableBody)
      .subscribe(() => {
        this.editableBody = '';
      });
  }

  protected getCategoryBadgeVariant(cat: EmailCategory): 'primary' | 'success' | 'warning' | 'danger' | 'info' | 'secondary' {
    switch (cat) {
      case EmailCategory.InterviewInvitation: return 'success';
      case EmailCategory.CodingAssessment: return 'warning';
      case EmailCategory.Offer: return 'primary';
      case EmailCategory.Rejection: return 'danger';
      default: return 'secondary';
    }
  }
}

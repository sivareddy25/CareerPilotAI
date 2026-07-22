import { Component, ChangeDetectionStrategy, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UnansweredQuestionPrompt } from '../../../core/services/automation.service';
import {
  DialogComponent,
  ButtonComponent,
  IconComponent,
} from '../../../shared/components';

@Component({
  selector: 'app-answer-prompt-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, DialogComponent, ButtonComponent, IconComponent],
  template: `
    <app-dialog
      [isOpen]="isOpen()"
      title="Dynamic Question Answer Vault"
      (closed)="closed.emit()"
    >
      <div class="questions-stack">
        <p class="subtitle-text">Playwright encountered custom application questions. Your answers will be saved to PostgreSQL for future applications.</p>
        @for (q of questions(); track q.questionKey; let idx = $index) {
          <div class="question-item">
            <label class="q-label">
              <app-icon name="help-circle" size="xs" /> {{ q.questionText }}
            </label>
            <input
              type="text"
              class="q-input"
              placeholder="Enter your answer..."
              [ngModel]="userAnswers()[q.questionKey] || ''"
              (ngModelChange)="updateAnswer(q.questionKey, $event)"
            />
          </div>
        }
      </div>

      <div dialog-footer class="modal-footer-actions">
        <app-button variant="outline" (btnClick)="closed.emit()">Cancel</app-button>
        <app-button variant="primary" (btnClick)="submitAnswers()">
          Save to Memory Vault & Resume Apply
        </app-button>
      </div>
    </app-dialog>
  `,
  styles: [`
    .questions-stack {
      display: flex;
      flex-direction: column;
      gap: var(--space-4);
      padding: var(--space-2) 0;
    }
    .subtitle-text {
      font-size: var(--text-body-sm);
      color: var(--text-secondary);
      margin: 0 0 var(--space-2) 0;
    }
    .question-item {
      display: flex;
      flex-direction: column;
      gap: var(--space-2);
    }
    .q-label {
      font-size: var(--text-body-sm);
      font-weight: 600;
      color: var(--text-primary);
      display: flex;
      align-items: center;
      gap: var(--space-2);
    }
    .q-input {
      padding: var(--space-3);
      border-radius: var(--radius-md);
      border: 1px solid var(--border-color);
      background-color: var(--bg-primary);
      color: var(--text-primary);
      font-size: var(--text-body-sm);

      &:focus {
        outline: none;
        border-color: var(--brand-primary);
      }
    }
    .modal-footer-actions {
      display: flex;
      justify-content: flex-end;
      gap: var(--space-3);
      width: 100%;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AnswerPromptModalComponent {
  readonly isOpen = input<boolean>(false);
  readonly questions = input<UnansweredQuestionPrompt[]>([]);

  readonly closed = output<void>();
  readonly answersSubmitted = output<Record<string, string>>();

  protected userAnswers = signal<Record<string, string>>({});

  protected updateAnswer(key: string, value: string): void {
    this.userAnswers.update((prev) => ({ ...prev, [key]: value }));
  }

  protected submitAnswers(): void {
    this.answersSubmitted.emit(this.userAnswers());
  }
}

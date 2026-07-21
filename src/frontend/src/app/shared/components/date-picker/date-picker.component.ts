import { Component, ChangeDetectionStrategy, input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IconComponent } from '../icon/icon.component';

@Component({
  selector: 'app-date-picker',
  standalone: true,
  imports: [CommonModule, IconComponent],
  template: `
    <div class="date-picker-wrapper">
      @if (label()) {
        <label class="field-label">{{ label() }}</label>
      }
      <div class="input-container" (click)="toggleOpen()">
        <input
          type="text"
          readonly
          [value]="selectedDate() || placeholder()"
          class="date-input"
          [disabled]="disabled()"
        />
        <app-icon name="calendar" size="sm" class="calendar-icon" />
      </div>

      @if (isOpen()) {
        <div class="calendar-popover">
          <div class="calendar-header">
            <button type="button" class="nav-btn"><app-icon name="chevron-left" size="xs" /></button>
            <span class="month-title">July 2026</span>
            <button type="button" class="nav-btn"><app-icon name="chevron-right" size="xs" /></button>
          </div>
          <div class="calendar-grid">
            <span class="day-head">Su</span><span class="day-head">Mo</span><span class="day-head">Tu</span>
            <span class="day-head">We</span><span class="day-head">Th</span><span class="day-head">Fr</span><span class="day-head">Sa</span>

            @for (day of [1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31]; track day) {
              <button
                type="button"
                class="day-cell"
                [class.selected]="day === 21"
                (click)="selectDay(day)"
              >
                {{ day }}
              </button>
            }
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .date-picker-wrapper {
      position: relative;
      display: flex;
      flex-direction: column;
      gap: var(--space-1);
      width: 100%;
    }
    .field-label {
      font-size: var(--text-body-sm);
      font-weight: 500;
      color: var(--text-primary);
    }
    .input-container {
      position: relative;
      cursor: pointer;
    }
    .date-input {
      width: 100%;
      height: 40px;
      padding: var(--space-2) 36px var(--space-2) var(--space-3);
      font-size: var(--text-body-sm);
      font-family: var(--font-sans);
      color: var(--text-primary);
      background-color: var(--bg-elevated);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      box-sizing: border-box;
      cursor: pointer;
    }
    .calendar-icon {
      position: absolute;
      right: 12px;
      top: 10px;
      color: var(--text-muted);
    }
    .calendar-popover {
      position: absolute;
      top: 100%;
      left: 0;
      margin-top: var(--space-1);
      width: 280px;
      padding: var(--space-3);
      background-color: var(--bg-elevated);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-lg);
      box-shadow: var(--shadow-lg);
      z-index: var(--z-popover);
    }
    .calendar-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      margin-bottom: var(--space-2);
    }
    .month-title { font-weight: 600; font-size: var(--text-body-sm); }
    .nav-btn {
      background: transparent;
      border: none;
      color: var(--text-secondary);
      cursor: pointer;
      padding: var(--space-1);
      border-radius: var(--radius-sm);
      &:hover { background-color: var(--bg-tertiary); }
    }
    .calendar-grid {
      display: grid;
      grid-template-columns: repeat(7, 1fr);
      gap: 2px;
      text-align: center;
    }
    .day-head {
      font-size: var(--text-caption);
      color: var(--text-muted);
      font-weight: 600;
      padding: var(--space-1) 0;
    }
    .day-cell {
      padding: 6px 0;
      font-size: var(--text-body-sm);
      border: none;
      background: transparent;
      border-radius: var(--radius-circle);
      color: var(--text-primary);
      cursor: pointer;

      &:hover { background-color: var(--bg-tertiary); }
      &.selected {
        background-color: var(--brand-primary);
        color: #ffffff;
        font-weight: 600;
      }
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DatePickerComponent {
  readonly label = input<string>();
  readonly placeholder = input<string>('Select date');
  readonly disabled = input<boolean>(false);

  protected isOpen = signal<boolean>(false);
  protected selectedDate = signal<string>('2026-07-21');

  protected toggleOpen(): void {
    if (!this.disabled()) {
      this.isOpen.update((v) => !v);
    }
  }

  protected selectDay(day: number): void {
    this.selectedDate.set(`2026-07-${day < 10 ? '0' + day : day}`);
    this.isOpen.set(false);
  }
}

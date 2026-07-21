import { Component, ChangeDetectionStrategy, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

@Component({
  selector: 'app-print-preview',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="print-container">
      <div class="print-paper" [innerHTML]="safeHtml()"></div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      background: #ffffff;
    }
    .print-container {
      width: 100%;
      background: #ffffff;
    }
    .print-paper {
      max-width: 800px;
      margin: 0 auto;
      background: #ffffff;
    }
    @media print {
      body { background: #ffffff !important; }
      .print-container { padding: 0 !important; }
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PrintPreviewComponent implements OnInit {
  private readonly sanitizer = inject(DomSanitizer);

  protected rawHtml = signal<string>(`
    <div style="padding: 24px; font-family: Arial, sans-serif;">
      <h1 style="color: #0f172a;">Print View Resume</h1>
      <p>Clean printable format.</p>
    </div>
  `);

  protected readonly safeHtml = signal<SafeHtml>('');

  ngOnInit(): void {
    this.safeHtml.set(this.sanitizer.bypassSecurityTrustHtml(this.rawHtml()));
  }
}

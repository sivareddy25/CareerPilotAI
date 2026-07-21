import { Directive, ElementRef, HostListener, input, inject } from '@angular/core';

@Directive({
  selector: '[appTooltip]',
  standalone: true,
})
export class TooltipDirective {
  readonly tooltipText = input.required<string>({ alias: 'appTooltip' });

  private readonly el = inject(ElementRef);
  private tooltipEl: HTMLElement | null = null;

  @HostListener('mouseenter')
  @HostListener('focus')
  show(): void {
    if (this.tooltipEl || !this.tooltipText()) return;

    this.tooltipEl = document.createElement('div');
    this.tooltipEl.className = 'app-tooltip-bubble';
    this.tooltipEl.textContent = this.tooltipText();
    document.body.appendChild(this.tooltipEl);

    const rect = this.el.nativeElement.getBoundingClientRect();
    const tooltipRect = this.tooltipEl.getBoundingClientRect();

    const top = rect.top - tooltipRect.height - 8;
    const left = rect.left + (rect.width - tooltipRect.width) / 2;

    this.tooltipEl.style.top = `${Math.max(8, top)}px`;
    this.tooltipEl.style.left = `${Math.max(8, left)}px`;
  }

  @HostListener('mouseleave')
  @HostListener('blur')
  hide(): void {
    if (this.tooltipEl) {
      this.tooltipEl.remove();
      this.tooltipEl = null;
    }
  }
}

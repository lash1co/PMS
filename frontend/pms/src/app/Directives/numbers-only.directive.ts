import { Directive, HostListener, ElementRef } from '@angular/core';

@Directive({
  selector: 'input[numbersOnly]',
  standalone: true
})
export class NumbersOnlyDirective {

  constructor(private el: ElementRef) {}

  @HostListener('input', ['$event'])
  onInputChange(event: Event): void {
    const initialValue = this.el.nativeElement.value;

    // Replace any character that is not a digit (0-9)
    this.el.nativeElement.value = initialValue.replace(/[^0-9]/g, '');

    // Trigger input event to update Angular form models (ngModel / FormControls)
    if (initialValue !== this.el.nativeElement.value) {
      event.stopPropagation();
    }
  }
}

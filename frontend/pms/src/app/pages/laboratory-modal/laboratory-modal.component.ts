import { Component, Input, Output, EventEmitter, OnChanges, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

@Component({
  standalone: true,
  selector: 'app-laboratory-modal',
  imports: [CommonModule, FormsModule, MatFormFieldModule, MatInputModule],
  templateUrl: './laboratory-modal.component.html',
  styleUrl: './laboratory-modal.component.css',
})
export class LaboratoryModalComponent implements OnChanges {
  @Input() laboratory: LaboratoryInterface = {
    id: 0,
    description: '',
    price: 0,
    timeToCompleteInHours: 0,
    noFoodBeforeExecuted: false,
    liquidIngestionBeforeExecuted: false
  };

  @Output() save = new EventEmitter<LaboratoryInterface>();
  @Output() close = new EventEmitter<void>();

  formData = signal<LaboratoryInterface>({ ...this.laboratory });
  isSaving = signal<boolean>(false);
  errorMessage = signal<string>('');

  ngOnChanges(): void {
    this.formData.set({ ...this.laboratory });
    this.errorMessage.set('');
  }

  onSave(): void {
    if (!this.validateForm()) {
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set('');

    this.save.emit(this.formData());
    this.isSaving.set(false);
  }

  onClose(): void {
    this.close.emit();
  }

  private validateForm(): boolean {
    const formValue = this.formData();

    if (!formValue.description || formValue.description.trim() === '') {
      this.errorMessage.set('Description is required.');
      return false;
    }

    if (formValue.price < 0) {
      this.errorMessage.set('Price cannot be negative.');
      return false;
    }

    if (formValue.timeToCompleteInHours < 0) {
      this.errorMessage.set('Time to complete cannot be negative.');
      return false;
    }

    return true;
  }

  updateFormField(field: keyof LaboratoryInterface, value: any): void {
    const current = this.formData();
    this.formData.set({ ...current, [field]: value });
  }
}

import { Component, Input, Output, EventEmitter, OnChanges, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

@Component({
  standalone: true,
  selector: 'app-doctor-rest-time-modal',
  imports: [CommonModule, FormsModule, MatFormFieldModule, MatInputModule],
  templateUrl: './doctor-rest-time-modal.html',
  styleUrl: './doctor-rest-time-modal.css',
})
export class DoctorRestTimeModal implements OnChanges {
  @Input() restTime: RestTimeInterface = {
    id: 0,
    startTime: '',
    endTime: '',
    reason: ''
  };

  @Output() save = new EventEmitter<RestTimeInterface>();
  @Output() close = new EventEmitter<void>();

  formData = signal<RestTimeInterface>({ ...this.restTime });
  isSaving = signal<boolean>(false);
  errorMessage = signal<string>('');

  ngOnChanges(): void {
    this.formData.set({ ...this.restTime });
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

    if (!formValue.startTime || formValue.startTime.trim() === '') {
      this.errorMessage.set('Start time is required.');
      return false;
    }

    if (!formValue.endTime || formValue.endTime.trim() === '') {
      this.errorMessage.set('End time is required.');
      return false;
    }

    if (formValue.endTime <= formValue.startTime) {
      this.errorMessage.set('End time must be greater than start time.');
      return false;
    }

    if (!formValue.reason || formValue.reason.trim() === '') {
      this.errorMessage.set('Reason is required.');
      return false;
    }

    return true;
  }

  updateFormField(field: keyof RestTimeInterface, value: any): void {
    const current = this.formData();
    this.formData.set({ ...current, [field]: value });
  }

  updateTimeField(field: 'startTime' | 'endTime', value: string): void {
    const current = this.formData();
    this.formData.set({ ...current, [field]: value });
  }
}


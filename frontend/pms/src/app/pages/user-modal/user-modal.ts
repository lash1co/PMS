import { Component, Input, Output, EventEmitter, signal, OnChanges, SimpleChanges, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-user-modal',
  imports: [CommonModule, FormsModule],
  templateUrl: './user-modal.html',
  styleUrl: './user-modal.css',
})
export class UserModal {
  private _userService: UserService;
  constructor(userService: UserService) {
    this._userService = userService;
  }
  @Input() isEditing: boolean = false;

  @Input() user: UserInterface = {
    id: 0,
    userName: '',
    password: '',
    name: '',
    email: '',
    isActive: false,
    role: '',
    creationDate: new Date()
  };
  @Output() save = new EventEmitter<UserInterface>();
  @Output() close = new EventEmitter<void>();

  //formData = signal<UserInterface>({ ...this.user });
  formData = signal<UserInterface>({} as UserInterface);
  isSaving = signal<boolean>(false);
  errorMessage = signal<string>('');

  ngOnInit(): void {
    this.loadForm();
  }

  ngOnChanges(): void {
    this.loadForm();
  }

private loadForm(): void {
  if (this.user) {
    this.formData.set({ ...this.user });
    this.errorMessage.set('');
  }
}
    


  /*onSave(): void {
    if (!this.validateForm()) {
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set('');

    this._userService.createUser(this.formData()).subscribe({
      next: (response) => {
        if (response) {
          this.save.emit(this.formData());
        } else {
          this.errorMessage.set('Failed to save user. Please try again.');
        }
        this.isSaving.set(false);
      },
      error: (error) => {
        alert('Failed to create user: ' + error.message);
        this.errorMessage.set('Failed to save user: ' + error.message);
        this.isSaving.set(false);
      }
    });
    this._userService.updateUser(this.formData()).subscribe({
      next: (response) => {
        if (response) {
          this.save.emit(this.formData());
        } else {
          this.errorMessage.set('Failed to save user. Please try again.');
        }
        this.isSaving.set(false);
      },
      error: (error) => {
        alert('Failed to create user: ' + error.message);
        this.errorMessage.set('Failed to save user: ' + error.message);
        this.isSaving.set(false);
      }
    });

    // Emit the form data to parent component
    setTimeout(() => {
      this.save.emit(this.formData());
      this.isSaving.set(false);
    }, 500);
  }
  */
 onSave(): void {
  const data = {
    ...this.user,        
    ...this.formData() 
  };

  const request = this.isEditing
    ? this._userService.updateUser(data)
    : this._userService.createUser(data);

    request.subscribe({
      next: () => this.save.emit(data),
      error: err => console.error(err)
    });
  }

  onClose(): void {
    this.close.emit();
  }

  private validateForm(): boolean {
    const formValue = this.formData();

    if (!formValue.userName || formValue.userName.trim() === '') {
      this.errorMessage.set('Username is required');
      return false;
    }

    if (!formValue.name || formValue.name.trim() === '') {
      this.errorMessage.set('Name is required');
      return false;
    }

    if (!formValue.email || formValue.email.trim() === '') {
      this.errorMessage.set('Email is required');
      return false;
    }

    if (!this.isValidEmail(formValue.email)) {
      this.errorMessage.set('Please enter a valid email address');
      return false;
    }

    if (!formValue.password || formValue.password.trim() === '') {
      this.errorMessage.set('Password is required');
      return false;
    }

    if (formValue.password.length < 6) {
      this.errorMessage.set('Password must be at least 6 characters');
      return false;
    }

    if (!formValue.role || formValue.role.trim() === '') {
      this.errorMessage.set('Role is required');
      return false;
    }

    return true;
  }

  private isValidEmail(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }

  updateFormField(field: keyof UserInterface, value: any): void {
    const current = this.formData();
    this.formData.set({ ...current, [field]: value });
  }

  updateFormFieldFromEvent(field: keyof UserInterface, event: Event): void {
    const target = event.target as HTMLInputElement | HTMLSelectElement;
    const value = target.type === 'checkbox' ? (target as HTMLInputElement).checked : target.value;
    this.updateFormField(field, value);
  }
}

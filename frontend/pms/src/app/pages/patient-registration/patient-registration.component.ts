import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-patient-registration',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './patient-registration.component.html'
})
export class PatientRegistrationComponent implements OnInit {
  registrationForm!: FormGroup;

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void {
    this.registrationForm = this.fb.group({
      // Patient entity fields
      personalInfo: this.fb.group({
        firstName: ['', Validators.required],
        lastName: ['', Validators.required],
        dateOfBirth: ['', Validators.required],
        phone: ['', [Validators.required, Validators.pattern('^[0-9]{10}$')]],
        email: ['', [Validators.email]]
      }),
      // Insurance entity fields
      insuranceInfo: this.fb.group({
        payerName: ['', Validators.required],
        memberId: ['', Validators.required],
        planType: [''],
        relationshipToSubscriber: ['Self', Validators.required]
      })
    });
  }

  onSubmit() {
    if (this.registrationForm.valid) {
      console.log('Datos enviados:', this.registrationForm.value);
      // HTTP request to backend API would go here //
    } else {
      this.registrationForm.markAllAsTouched();
    }
  }
}
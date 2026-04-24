import { Component, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { PatientService } from '../../services/patient/patient.service';

@Component({
  selector: 'app-patient-registration',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './patient-registration.component.html'
})
export class PatientRegistrationComponent implements OnInit {
  registrationForm!: FormGroup;
  inviteToken: string | null = null;
  insuranceInfoLocked = signal<boolean>(false);
  personalInfoLocked = signal<boolean>(true);
  isLoading = signal<boolean>(true);

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private patientService: PatientService
  ) {}

  ngOnInit(): void {
    this.inviteToken = this.route.snapshot.paramMap.get('token');

    this.registrationForm = this.fb.group({
      personalInfo: this.fb.group({
        firstName: ['', Validators.required],
        lastName: ['', Validators.required],
        dateOfBirth: ['', Validators.required],
        phone: ['', [Validators.required, Validators.pattern('^[0-9]{10}$')]],
        email: ['', [Validators.email]]
      }),
      insuranceInfo: this.fb.group({
        payerName: ['', Validators.required],
        memberId: ['', Validators.required],
        planType: [''],
        relationshipToSubscriber: ['Self', Validators.required]
      })
    });

    if (this.inviteToken) {
      this.loadPatientData(this.inviteToken);
    } else {
      this.personalInfoLocked.set(false);
      this.isLoading.set(false); 
    }
  }

  loadPatientData(token: string) {
    this.patientService.getInviteDetails(token).subscribe({
      next: (data) => {
        
        if (data.personalInfo) {
          if (data.personalInfo.dateOfBirth) {
            data.personalInfo.dateOfBirth = data.personalInfo.dateOfBirth.split('T')[0];
          }
          this.registrationForm.get('personalInfo')?.patchValue(data.personalInfo);
          this.registrationForm.get('personalInfo')?.disable();
          this.personalInfoLocked.set(true);
        }

        // 2. Cargar Insurance Info (Si ya existe)
        if (data.insuranceInfo) {
          this.registrationForm.get('insuranceInfo')?.patchValue(data.insuranceInfo);
          this.registrationForm.get('insuranceInfo')?.disable();
          this.insuranceInfoLocked.set(true);
        } else {
          this.registrationForm.get('insuranceInfo')?.enable();
          this.insuranceInfoLocked.set(false);
        }
        
        this.isLoading.set(false); 
      },
      error: (err) => {
        this.isLoading.set(false); 
        alert('The link is invalid or the server is not responding.');
        this.router.navigate(['/login']);
      }
    });
  }

  toggleLock() {
    this.personalInfoLocked.update(locked => !locked);
    const personalInfoGroup = this.registrationForm.get('personalInfo');
    if (this.personalInfoLocked()) {
      personalInfoGroup?.disable();
    } else {
      personalInfoGroup?.enable();
    }
  }

  toggleInsuranceLock() {
    this.insuranceInfoLocked.update(locked => !locked);
    const insuranceGroup = this.registrationForm.get('insuranceInfo');
    if (this.insuranceInfoLocked()) {
      insuranceGroup?.disable();
    } else {
      insuranceGroup?.enable();
    }
  }

  onSubmit() {
    const isFormValid = this.registrationForm.valid || this.registrationForm.status === 'DISABLED';
    
    if (isFormValid) {
      const payload = {
        personalInfo: this.registrationForm.getRawValue().personalInfo, 
        insuranceInfo: this.registrationForm.getRawValue().insuranceInfo
      };

      if (this.inviteToken) {
        this.patientService.completeRegistration(this.inviteToken, payload).subscribe({
          next: () => {
            alert('Your profile has been updated successfully!');
            this.router.navigate(['/login']);
          },
          error: (err) => {
            alert('Error updating profile: ' + (err.error?.message || err.error?.error || 'Please verify the data.'));
          }
        });
      }
    } else {
      this.registrationForm.markAllAsTouched();
    }
  }
}
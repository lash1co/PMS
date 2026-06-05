import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { AppLayoutComponent } from './layout/app-layout.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { PatientList } from './pages/patient-list/patient-list';
import { authGuard } from './guards/auth.guard';
import { BillingComponent } from './pages/billing.component/billing.component';
import { DoctorRestTimeFormComponent } from './pages/doctor-rest-time-form.component/doctor-rest-time-form.component';
import { EncounterComponent } from './pages/encounter/encounter.component';
import { LaboratoryComponent } from './pages/laboratory/laboratory.component';
import { PatientRegistrationComponent } from './pages/patient-registration/patient-registration.component';
import { PrescriptionsList} from './pages/prescriptions-list/prescriptions-list';
import { UsersList } from './pages/users-list/users-list';
import { ScheduleComponent } from './pages/schedule/schedule.component';
import { ReportsComponent } from './pages/reports/reports.component';

export const routes: Routes = [

  { path: '', redirectTo: 'login', pathMatch: 'full' },


  { path: 'login', component: LoginComponent },

  { path: 'register/:token', component: PatientRegistrationComponent },

  {
    path: '',
    component: AppLayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: 'dashboard', component: DashboardComponent },
      { path: 'patients', component: PatientList },
      { path: 'users', component: UsersList },
      { path: 'laboratories', component: LaboratoryComponent },
      { path: 'schedule', component: ScheduleComponent },
      { path: 'doctor-rest-time', component: DoctorRestTimeFormComponent },
      { path: 'encounters', component: EncounterComponent },
      { path: 'prescriptions', component: PrescriptionsList },
      { path: 'billing', component: BillingComponent },
      { path: 'reports', component: ReportsComponent }
    ]
  },

  { path: '**', redirectTo: 'dashboard' }
]

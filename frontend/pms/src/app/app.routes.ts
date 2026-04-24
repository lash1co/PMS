import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { AppLayoutComponent } from './layout/app-layout.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { PatientList } from './pages/patient-list/patient-list';
import { authGuard } from './guards/auth.guard';
import { PatientRegistrationComponent } from './pages/patient-registration/patient-registration.component';
import { UsersList } from './pages/users-list/users-list';
import { ScheduleComponent } from './pages/schedule/schedule.component';

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
      { path: 'schedule', component: ScheduleComponent },
    ]
  },

  { path: '**', redirectTo: 'dashboard' }
]

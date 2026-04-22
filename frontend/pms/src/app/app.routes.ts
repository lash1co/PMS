import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { AppLayoutComponent } from './layout/app-layout.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { MainMenu } from './pages/main-menu/main-menu';
import { PatientList } from './pages/patient-list/patient-list';
import { authGuard } from './guards/auth.guard';
import { TokenComponent } from './pages/token/token.component';
import { PatientRegistrationComponent } from './pages/patient-registration/patient-registration.component';

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
      { path: 'patients', component: PatientList }
    ]
  },

  { path: '**', redirectTo: 'dashboard' }
]

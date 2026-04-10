import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { TokenComponent } from './pages/token/token.component';
import { PatientList } from './pages/patient-list/patient-list';

import { AppLayoutComponent } from './layout/app-layout.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';

export const routes: Routes = 
[
    { path: 'login', component: LoginComponent },
    { path: 'token', component: TokenComponent},

    {
    path: '',
    component: AppLayoutComponent,
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }, 
      { path: 'dashboard', component: DashboardComponent },
      { path: 'patients', component: PatientList }
    ]
  },
  
  { path: '**', redirectTo: 'dashboard' }
];
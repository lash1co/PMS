import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { AppLayoutComponent } from './layout/app-layout.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { MainMenu } from './pages/main-menu/main-menu';
import { PatientList } from './pages/patient-list/patient-list';
import { authGuard } from './guards/auth.guard'; // Importa el guard
import { TokenComponent } from './pages/token/token.component';

export const routes: Routes = [

  { path: '', redirectTo: 'login', pathMatch: 'full' },


  { path: 'login', component: LoginComponent },


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
export const routes: Routes =
[
    { path: '', component: LoginComponent },
    { path: 'main-menu', component: MainMenu },
    { path: 'patients', component: PatientList },
    { path: 'token', component: TokenComponent},
    { path: '', redirectTo: '/patients', pathMatch: 'full' } // Redirigir al listado por defecto
];

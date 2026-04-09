import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { TokenComponent } from './pages/token/token.component';
import { PatientList } from './pages/patient-list/patient-list';

export const routes: Routes = 
[
    { path: '', component: LoginComponent },
    { path: 'token', component: TokenComponent},
    { path: 'patients', component: PatientList },
    { path: '', redirectTo: '/patients', pathMatch: 'full' } // Redirigir al listado por defecto   
];

import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { MainMenu } from './pages/main-menu/main-menu';
import { PatientList } from './pages/patient-list/patient-list';
import { TokenComponent } from './pages/token/token.component';

export const routes: Routes =
[
    { path: '', component: LoginComponent },
    { path: 'main-menu', component: MainMenu },
    { path: 'patients', component: PatientList },
    { path: 'token', component: TokenComponent},
    { path: '', redirectTo: '/patients', pathMatch: 'full' } // Redirigir al listado por defecto
];

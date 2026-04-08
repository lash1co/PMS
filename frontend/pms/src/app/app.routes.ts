import { Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { TokenComponent } from './token/token.component';

export const routes: Routes = 
[
    { path: '', component: LoginComponent },
    { path: 'token', component: TokenComponent}    
];

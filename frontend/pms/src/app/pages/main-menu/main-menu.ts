import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { menu_config } from './menu-config';
import {MatMenuModule} from '@angular/material/menu';
import {MatButtonModule} from '@angular/material/button';

@Component({
  selector: 'app-main-menu',
  imports: [
    MatButtonModule,
    MatMenuModule
  ],
  templateUrl: './main-menu.html',
  styleUrl: './main-menu.css',
})
export class MainMenu {
  menus = menu_config;

  constructor(private router: Router){}

  navigateTo(routeUrl: string): void {
    this.router.navigate([routeUrl]);
  }
}

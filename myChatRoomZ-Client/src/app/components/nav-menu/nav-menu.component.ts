import { Component } from '@angular/core';
import { LoginService } from '../../services/login.service';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent {
  isExpanded = false;

  constructor(public log_service: LoginService, private http: HttpClient, private router: Router) { }

  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }

  logOut() {
    this.log_service.username = "";
    this.log_service.resetToken();
    this.log_service.loggedIn = false;
    console.log("LogOUt");
    this.http.get("/Account/Logout");
    this.router.navigate(['/']);

  }
}

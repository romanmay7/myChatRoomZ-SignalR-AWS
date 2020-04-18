import { Component, OnInit } from '@angular/core';
import { LoginService } from '../../services/login.service';
import { ChannelService } from '../../services/channel.service';
import { Router } from '@angular/router';
import { AuthService } from "angularx-social-login";
import { FacebookLoginProvider, GoogleLoginProvider } from "angularx-social-login";


@Component({
  selector: 'login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css', "../../../../node_modules/font-awesome/css/font-awesome.css"]
})
export class LoginComponent implements OnInit {

  constructor(private log_service: LoginService, private ch_service: ChannelService, private router: Router, private authService: AuthService) { }

  errorMessage: string = "";
  public creds = {
    username: "",
    password: ""
  }

  public googleLoginRequest = {
    idToken :""
  }

  ngOnInit() {
  }

  onLogin() {

    //Call the LoginService to perform Authentication with Server

    this.log_service.login(this.creds)
      .subscribe(success => {
        if (success) {
          //Save Username  to track logged in user
          this.log_service.username = this.creds.username;
          this.ch_service.chatterName = this.creds.username;
          this.log_service.loggedIn = true;

          this.router.navigate(["/"])

        }

      }, err => this.errorMessage = "Failed to login")
  }

  //Call this method to perform Authentication with Google
  signInWithGoogle(): void {
    //this will return user data from Google. What we need is a user token which we will send it to the Server
    this.authService.signIn(GoogleLoginProvider.PROVIDER_ID).then(
      (userData) => {
        //send Google token to Server within request object
        this.googleLoginRequest.idToken = userData.idToken;
        this.log_service.loginGoogle(this.googleLoginRequest)
          .subscribe(success => {
            if (success) {
              //Save Username  to track logged in user
              this.log_service.username = userData.email;//Google Email
              this.ch_service.chatterName = userData.email;
              this.log_service.loggedIn = true;

              this.router.navigate(["/"])

            }

          }, err => this.errorMessage = "Failed to login with Google")
        //console.log(userData.idToken);
      });
  }

  //Call this method to perform Authentication with Facebook
  signInWithFB(): void {
    this.authService.signIn(FacebookLoginProvider.PROVIDER_ID);
  }

  signOut(): void {
    this.authService.signOut();
  }

}

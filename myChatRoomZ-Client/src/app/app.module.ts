import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule  } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './components/nav-menu/nav-menu.component';
import { HomeComponent } from './components/home/home.component';
import { ChannelComponent } from './components/channel/channel.component';
import { ChannelService } from './services/channel.service';
import { UploadService } from './services/upload.service';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { LoginService } from './services/login.service';
import { LoginComponent } from './components/login/login.component';
import { SignupComponent } from './components/signup/signup.component';
import { SocialLoginModule, AuthServiceConfig } from "angularx-social-login";
import { GoogleLoginProvider, FacebookLoginProvider } from "angularx-social-login";
import { AngularFontAwesomeModule } from 'angular-font-awesome';

let config = new AuthServiceConfig([
  {
    id: GoogleLoginProvider.PROVIDER_ID,
    provider: new GoogleLoginProvider("907069961520-utqs3la12ou4s2ptq592du5uc2psesge.apps.googleusercontent.com")
  },
  {
    id: FacebookLoginProvider.PROVIDER_ID,
    provider: new FacebookLoginProvider("Facebook-App-Id")
  }
]);

export function provideConfig() {
  return config;
}

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    ChannelComponent,
    LoginComponent,
    SignupComponent,
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule ,
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' },
      { path: 'channel', component: ChannelComponent },
      { path: 'login', component: LoginComponent },
      { path: 'signup', component: SignupComponent }
    ]),
    BrowserAnimationsModule,
    SocialLoginModule,
    AngularFontAwesomeModule
  ],
  providers: [ChannelService, UploadService, LoginService, { provide: AuthServiceConfig, useFactory: provideConfig }],
  bootstrap: [AppComponent]
})
export class AppModule { }

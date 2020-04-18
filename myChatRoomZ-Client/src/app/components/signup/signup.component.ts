
import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { FormBuilder, FormGroup, FormControl, Validators } from "@angular/forms";
import { User } from '../../data-models/user.model';
import { Router } from '@angular/router';

// import custom validator to validate that password and confirm password fields match
import { MustMatch } from '../../helpers/must-match.validator';
import { CustomValidators } from '../../helpers/custom-validators';
import { GlobalVariable } from '../../../global';

@Component({
  selector: 'app-signup',
  templateUrl: './signup.component.html',
  styleUrls: ['./signup.component.scss']
})
export class SignupComponent implements OnInit {
  registerForm: FormGroup;
  submitted = false;
  errorMessage="Error";

  constructor(private formBuilder: FormBuilder, private http: HttpClient, private router: Router) { }

  ngOnInit() {
    this.registerForm = this.formBuilder.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      userName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6),
      // 1. check whether the entered password has a number
      CustomValidators.patternValidator(/\d/, { hasNumber: true }),
      // 2. check whether the entered password has upper case letter
      CustomValidators.patternValidator(/[A-Z]/, { hasCapitalCase: true }),
      // 3. check whether the entered password has a lower-case letter
      CustomValidators.patternValidator(/[a-z]/, { hasSmallCase: true }),
      // 4. check whether the entered password has a Special Character
      CustomValidators.patternValidator(/[!@#$%^&*(),.?":{}|<>]/, { hasSpecialCharacters: true })
      ]],
      confirmPassword: ['', Validators.required]
    }, {
      validator: MustMatch('password', 'confirmPassword')
    });
  }

  // convenience getter for easy access to form fields
  get f() { return this.registerForm.controls; }

  onSignUp() {
    this.submitted = true;

    // stop here if form is invalid
    if (this.registerForm.invalid) {
      //alert('NO SUCCESS!! :-)\n\n' + JSON.stringify(this.registerForm.value))
      return;
    }

    //alert('SUCCESS!! :-)\n\n' + JSON.stringify(this.registerForm.value))

    var new_user = new User();

    new_user.firstName = this.registerForm.get('firstName').value;
    new_user.lastName = this.registerForm.get('lastName').value;
    new_user.userName = this.registerForm.get('userName').value;
    new_user.email = this.registerForm.get('email').value;
    new_user.password = this.registerForm.get('password').value;


    return this.http.post(GlobalVariable.BASE_API_URL+"/Account/CreateUser", new_user,
      {
        headers: new HttpHeaders()
          .set('Content-Type', 'application/json; charset=utf-8')
        //.set('application/x-www-form-urlencoded')
      })
      .subscribe(
        //Refreshing Reviews List on Products page,after new review was submited by calling product Service
        (response) => { this.Redirect(); console.log(response); },
        (error) => { alert("Could not create new user"); console.log(error); }
      )
  }


  public Redirect() {
    alert("New User Created");
    this.router.navigate(['/login']);

    //console.log("Implement method to refresh UI");

  }
}

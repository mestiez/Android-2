import { HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { RestService } from './rest.service';

@Injectable({
  providedIn: 'root'
})
export class AuthorisationService {

  public isAuthorised: boolean = null;

  constructor(private rest: RestService) { }

  public tryAuthorise(token: string) {
    console.log(btoa(token));
    const token64 = btoa(token);

    this.rest.authorise(token64).subscribe(
      (secret) => {
        console.log(secret);
        const success = secret !== null;
        if (success) {
          window.localStorage.botToken = token;
          window.localStorage.secret =  secret;
        }
        this.isAuthorised = success;
      }, (error) => {
        console.error(error);
      }
    );
  }
}

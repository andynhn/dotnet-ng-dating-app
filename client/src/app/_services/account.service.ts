import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  baseUrl = environment.apiUrl;
  /*
    create an observable to store our user in.
    ReplaySubject is a buffer object. When a subscriber subscribes, it emits the last value or however many values inside we want to emit
  */
  private currentUserSource = new ReplaySubject<User>(1);
  currentUser$ = this.currentUserSource.asObservable(); // dollar-sign by convention for observables

  constructor(private http: HttpClient) { }

  /**
   * Access the account.login api and pass in the model used to login a new user.
   * @param model the model that contains the user data being passed in to authenticate the logged in user.
   */
  login(model: any) {
    return this.http.post(this.baseUrl + 'account/login', model).pipe(
      map((response: User) => {
        const user = response;
        if (user) {
          /*map applies a function to each value emitted by the source observable.
            map emits the values as an observable
            populate the user inside local storage in the browser
          */
          this.setCurrentUser(user);
        }
      })
    );
  }

  /**
   * Access the account/register api and pass in the model used to register a new user.
   * RxJs pipe and map the observable returned and then set the current user to the authenticated user.
   * @param model the Register model that contains the user data.
   */
  register(model: any) {
    // this front end angular service interacts with our back-end controller and api for registering users.
    return this.http.post(this.baseUrl + 'account/register', model).pipe(
      map((user: User) => {
        if (user) {
          this.setCurrentUser(user);
        }
      })
    )
  }

  /**
   * Saves the user name, jwt, photoUrl, and KnownAs in localStorage as the 'user' item
   * updates the currentUser$ observable to point to the new user.
   * @param user the authenticated user.
   */
  setCurrentUser(user: User) {
    /*
      NOTE (1/28/2021): Saving the 'user' object in localStorage should be done here (not in the above login/register method's pipe/map).
      Otherwise, may cause issue with updating localStorage with the correct item.
      Some app functionality depends on accessing that localStorage item (e.g. PhotoUrl)
    */
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUserSource.next(user);
  }

  /**
   * Logout by deleting the user item in local storage that contains the name, jwt, photoUrl, and KnownAs
   * update the currentUser$ observable to point to null
   */
  logout() {
    localStorage.removeItem('user');
    this.currentUserSource.next(null);
  }
}

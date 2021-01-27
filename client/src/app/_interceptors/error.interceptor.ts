import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { ToastrService } from 'ngx-toastr';
import { catchError } from 'rxjs/operators';
import { NavigationExtras, Router } from '@angular/router';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {

  constructor(private router: Router, private toastr: ToastrService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(
      catchError(error => {
        if (error) {
          switch (error.status) {
            case 400:
                // check if there are multiple errors (errors array)
                if (error.error.errors) {
                  // aspnet validation errors known as modelstate errors
                  const modelStateErrors = [];
                  for (const key in error.error.errors) {
                    if (error.error.errors[key]) {
                      modelStateErrors.push(error.error.errors[key])
                    }
                  }
                  // We will get an array of arrays, so we want to flatten it. Need to go into tsconfig.json and add es2019 to lib in order to use flat() for now.
                  throw modelStateErrors.flat();
                } else {
                  // Potential bug in Angular may be causing statusText to be "OK" for all http responses. Postman validates that these HTTP responses are accurately "400 Bad Reequest, etc.". Instead of displaying error.statusText here (which would be misleading), display the actual hard-coded string in the toastr notifcation for now...
                  this.toastr.error("Bad Request", error.status);
                }
              break;
            case 401:
              this.toastr.error("Unauthorized", error.status);
              break;
            case 404:
              this.router.navigateByUrl('/not-found');
              break;
            case 500:
              // use NavigationExtras to get a hold of that error so that we can do sth on the page after we've proceeded to the /server-error route
              const navigationExtras: NavigationExtras = {state: {error: error.error}};
              this.router.navigateByUrl('/server-error', navigationExtras);
              break;
            default:
              this.toastr.error('Something unexpected went wrong');
              console.log(error);
              break;
          }
        }
        // we catch most errors in the switch. if not, return error to wherever was calling the http request, just in case.
        return throwError(error);
      })
    );
  }
}

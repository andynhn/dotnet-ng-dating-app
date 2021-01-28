import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpResponse
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { LoadingService } from '../_services/loading.service';
import { catchError, delay, map } from 'rxjs/operators';

@Injectable()
export class LoadingInterceptor implements HttpInterceptor {

  constructor(private loadingService: LoadingService) {}

  /**
   * Intercepts HTTP requests for the loading indicator. When a request
   * starts, set loadingSpinner property in the LoadingService to true. When
   * the request is done and sends a response, set the loadingSpinner property
   * to false. If an error occurs, set the loadingSpinner property to false.
   */
  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    this.loadingService.setLoading(true, request.url);

    return next.handle(request).pipe(catchError((err) => {
        this.loadingService.setLoading(false, request.url);
        return err;
      }))
      .pipe(
        delay(400),
        map<HttpEvent<any>, any>((evt: HttpEvent<any>) => {
        if (evt instanceof HttpResponse) {
          this.loadingService.setLoading(false, request.url);
        }
        return evt;
      }));
  }
}

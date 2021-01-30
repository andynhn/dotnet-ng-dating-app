import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class LoadingService {
  loadingSpinner: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  loadingMap: Map<string, boolean> = new Map<string, boolean>();

  loadingRequestCount = 0;

  constructor() { }

  /**
   * If loading is true, add url to the loadingMap with true.
   * Set loadingSpinner value to true.
   * If loading is false, remove laodingMap entry.
   * When map is empty we set loadingSpinner value to false.
   * We must allow other requests to complete before setting loadingSpinner to false.
   */
  // setLoading(loading: boolean, url: string): void {
  //   if (!url) {
  //     throw new Error('The request URL must be provided to the LoadingService.setLoading function');
  //   }
  //   if (loading === true) {
  //     this.loadingMap.set(url, loading);
  //     this.loadingSpinner.next(true);
  //   } else if (loading === false && this.loadingMap.has(url)) {
  //     this.loadingMap.delete(url);
  //   }
  //   if (this.loadingMap.size === 0) {
  //     this.loadingSpinner.next(false);
  //   }
  // }

  setToLoading(): void {
    this.loadingRequestCount++;
    this.loadingSpinner.next(true);
  }

  setToIdle(): void {
    this.loadingRequestCount--;
    if (this.loadingRequestCount <= 0) {
      this.loadingRequestCount = 0;
      this.loadingSpinner.next(false);
    }
  }
}

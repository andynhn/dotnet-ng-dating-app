import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { delay } from 'rxjs/operators';
import { User } from './_models/user';
import { AccountService } from './_services/account.service';
import { LoadingService } from './_services/loading.service';
import { PresenceService } from './_services/presence.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'The Dating App';
  users: any;
  isLoading: boolean;

  constructor(private accountService: AccountService, private loadingService: LoadingService, private presence: PresenceService) {}

  ngOnInit() {
    this.setCurrentUser();
    this.listenToLoading();
  }

  setCurrentUser() {
    const user: User = JSON.parse(localStorage.getItem('user'));
    if (user) {
      this.accountService.setCurrentUser(user);
      this.presence.createHubConnection(user);
    }
    this.accountService.setCurrentUser(user);
  }

  /**
   * Listens to the loadingSubject property, which displays the loading spinner.
   */
  listenToLoading(): void {
    this.loadingService.loadingSpinner
      .pipe(delay(0)) // This prevents a ExpressionChangedAfterItHasBeenCheckedError for subsequent requests
      .subscribe((response) => {
        this.isLoading = response;
      });
  }

}

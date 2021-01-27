import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-server-error',
  templateUrl: './server-error.component.html',
  styleUrls: ['./server-error.component.css']
})
export class ServerErrorComponent implements OnInit {
  error: any;
  // can only access the Router state within the constructor
  constructor(private router: Router) {
    const navigation = this.router.getCurrentNavigation();
    // we don't know if we get any info. On refresh, we lose what's in the navigatino state. 
    // We only get it once when the route is activated and we redirect a user to this page.
    // be safe and use optional chaining operators.
    this.error = navigation?.extras?.state?.error;
   }

  ngOnInit(): void {
  }

}

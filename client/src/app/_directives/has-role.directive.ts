import { Directive, Input, OnInit, TemplateRef, ViewContainerRef } from '@angular/core';
import { take } from 'rxjs/operators';
import { User } from '../_models/user';
import { AccountService } from '../_services/account.service';

@Directive({
  selector: '[appHasRole]'  // *appHasRole='["Admin"]'
})
export class HasRoleDirective implements OnInit {
  @Input() appHasRole: string[];
  user: User;
  // this custom role directive will help us hide/display DOM elements depending on whether the user is in a role
  // so not just hiding or displaying a nav bar link. This will be used all through our app DOM.
  constructor(private viewContainerRef: ViewContainerRef, private templateRef: TemplateRef<any>, private accountService: AccountService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe( user => {
      this.user = user;
    });
  }
  ngOnInit(): void {
    // clear view if not roles
    if (!this.user?.roles || this.user == null) {
      this.viewContainerRef.clear();
      return;
    }

    // Whereever the custom structural directive is used in the HTML DOM,
    // if the user has a role in the list provided (e.g.: *appHasRole='["Admin", "Moderator"]'), they can view that DOM element
    if (this.user?.roles.some(r => this.appHasRole.includes(r))) {
      this.viewContainerRef.createEmbeddedView(this.templateRef);
    } else {
      // otherwise, remove that element (e.g. Admin nav bar link) from the DOM.
      this.viewContainerRef.clear();
    }
  }

}

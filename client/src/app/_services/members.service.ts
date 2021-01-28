import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { of } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';


@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.apiUrl;
  members: Member[] = [];

  constructor(private http: HttpClient) { }

  getMembers() {
    // if it's the first time we visit the app (no members have been loaded)
    // then we make the api call and display the loading spinner via our loading interceptor
    // But if we've already loaded that page of members, then navigate to a different tab and then come back,
    // we don't want to make the api call again just to show the same data (this would also activate the loading spinner again).
    // So we can use the Singleton nature of services and send back the user list as an observable (return "of" sends it as an observable).
    if (this.members.length > 0) {
      return of(this.members);
    }

    return this.http.get<Member[]>(this.baseUrl + 'users').pipe(
      map(members => {
        this.members = members;
        return members;
      })
    );
  }

  getMember(username: string) {
    const member = this.members.find(x => x.username === username);

    // we want to look for undefined, because that's what the find method returns. Avoid the api call if member is undefined.
    if (member !== undefined) {
      return of(member);
    }
    return this.http.get<Member>(this.baseUrl + 'users/' + username);
  }

  updateMember(member: Member) {
    // need to update the members array when we update a member.
    return this.http.put(this.baseUrl + 'users', member).pipe(
      map(() => {
        // find the index of the member we just updated.
        const index = this.members.indexOf(member);
        // then update members array so that it has the new changes to that member.
        this.members[index] = member;
      })
    )
  }
}

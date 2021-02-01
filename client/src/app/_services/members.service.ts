import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { of } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';
import { PaginatedResult } from '../_models/pagination';
import { User } from '../_models/user';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';


@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.apiUrl;
  members: Member[] = [];
  memberCache = new Map();  // implement a cache for pagination, etc.
  user: User;
  userParams: UserParams;

  /**
   * inject account service here instead of in member-list component
   * (can inject services in services, but be careful of circular references)
   * We want some data to persist between components that come from the userParams
   * so we can take advantage of the singleton nature of services.
   * e.g. Maintaining filters in memory when navigating to a user page and back to the user list.
   */
  constructor(private http: HttpClient, private accountService: AccountService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe(user => {
      this.user = user;
      this.userParams = new UserParams(user);
    });
  }

  getUserParams() {
    return this.userParams;
  }

  setUserParams(params: UserParams) {
    this.userParams = params;
  }

  resetUserParams() {
    this.userParams = new UserParams(this.user);
    return this.userParams;
  }

  getMembers(userParams: UserParams) {
    console.log(Object.values(userParams).join('-'));
    /**
     * we've created a "key" that stores the userParams so that we can keep track of user activity for caching
     * the key is just the params separated by a hyphen the idea is to prevent the "loading spinner"
     * from activating on routes that we've already loaded. This response tries to get a value from the memberCache 
     * based on the most recent userParams.
     */
    var response = this.memberCache.get(Object.values(userParams).join('-'));

    /**
     * if that key exists (meaning the user visited this before, we should bypass our loadingSpinner)
     * We can use the Singleton nature of services and send back the user list as an observable (return "of" sends it as an observable).
     */
    if (response) {
      return of(response);
    }
    let params = getPaginationHeaders(userParams.pageNumber, userParams.pageSize);

    params = params.append('minAge', userParams.minAge.toString());
    params = params.append('maxAge', userParams.maxAge.toString());
    params = params.append('gender', userParams.gender);
    params = params.append('orderBy', userParams.orderBy);

    // If it passes the above caching functionality, then go to the api, which hits our loading interceptor.
    // Then upon return, add that key params route to the memberCache map.
    // onto the memberCache map so that we avoid the loading Spinner next time.
    return getPaginatedResult<Member[]>(this.baseUrl + 'users', params, this.http).pipe(
      map(response => {
        this.memberCache.set(Object.values(userParams).join('-'), response);
        return response;
      })
    );
  }

  getMember(username: string) {
    /**
     * combine the paginated results from the memberCache values as an array
     * then we reduce our array into something else. We want the results of each array in a single array that we can search
     * REDUCE: As we call the reduce function on each element in our member array, we get the result (which contains
     * the members in our cache) then we concatenate that into an array that we have, which starts with nothing.
     * then the FIND method finds the first instance of the user we want, based on the username passed in.
     */
    const member = [...this.memberCache.values()]
      .reduce((arr, elem) => arr.concat(elem.result), [])
      .find((member: Member) => member.username === username);
    console.log(member);

    // if we find the member in our cache, bypass the loading spinner in the loading interceptor.
    if (member) {
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
    );
  }

  setMainPhoto(photoId: number) {
    return this.http.put(this.baseUrl + 'users/set-main-photo/' + photoId, {});
  }

  deletePhoto(photoId: number) {
    return this.http.delete(this.baseUrl + 'users/delete-photo/' + photoId);
  }


  addLike(username: string) {
    // this is a post, so we need to add a body here. an empty object here.
    return this.http.post(this.baseUrl + 'likes/' + username, {});
  }

  getLikes(predicate: string, pageNumber, pageSize) {
    /**
     * we've created a "key" that stores the pageNumber, pageSize, and predicate (liked or likedBy) so that we can
     * keep track of user activity for caching. The key is just the params separated by a hyphen. The idea is to prevent the
     * "loading spinner" from activating on routes that we've already loaded. This response tries to get a value from the memberCache
     * based on the most recent params passed in (see getMembers for similar implementation).
     */
    let params = getPaginationHeaders(pageNumber, pageSize);
    params = params.append('predicate', predicate);

    console.log(params.get('pageNumber') + '-' + params.get('pageSize') + '-' + params.get('predicate'));

    var response = this.memberCache.get(params.get('pageNumber') + '-' + params.get('pageSize') + '-' + params.get('predicate'));

    /**
     * if that key exists (meaning the user visited this before, we should bypass our loadingSpinner)
     * We can use the Singleton nature of services and send back the list as an observable (return "of" sends it as an observable).
     */
    if (response) {
      return of(response);
    }
    /**
     * If it passes the above caching functionality, then go to the api, which hits our loading interceptor.
     * Then upon return, add that key params route to the memberCache map.
     * onto the memberCache map so that we avoid the loading Spinner next time.
     */
    return getPaginatedResult<Partial<Member[]>>(this.baseUrl + 'likes', params, this.http).pipe(
      map(response => {
        this.memberCache.set(params.get('pageNumber') + '-' + params.get('pageSize') + '-' + params.get('predicate'), response);
        return response;
      })
    );
  }


  // moved to paginationHelper class
  // /**
  //  * Private method to get results for pagination
  //  * @param url the api url
  //  * @param params include the pagination headers that help determine what to display on the page
  //  */
  // private getPaginatedResult<T>(url, params: HttpParams) {
  //   const paginatedResult: PaginatedResult<T> = new PaginatedResult<T>();
  //   return this.http.get<T>(url, { observe: 'response', params }).pipe(
  //     map(response => {
  //       paginatedResult.result = response.body;
  //       if (response.headers.get('Pagination') !== null) {
  //         paginatedResult.pagination = JSON.parse(response.headers.get('Pagination'));
  //       }
  //       return paginatedResult;
  //     })
  //   );
  // }

  // /**
  //  * Private method that appends pagenumber and pageSize to the Http Params
  //  * @param pageNumber Current page number the user is on for pagination
  //  * @param pageSize total number of pages
  //  */
  // private getPaginationHeaders(pageNumber: number, pageSize: number) {
  //   // this helps serialize our parameters
  //   let params = new HttpParams();

  //   params = params.append('pageNumber', pageNumber.toString());
  //   params = params.append('pageSize', pageSize.toString());

  //   return params;
  // }
}

import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NgxGalleryAnimation, NgxGalleryImage, NgxGalleryOptions } from '@kolkov/ngx-gallery';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { Member } from 'src/app/_models/member';
import { Message } from 'src/app/_models/message';
import { MembersService } from 'src/app/_services/members.service';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit {
  // access the #memberTabs tag that we specified in the tabset in the html.
  // static so that it does not react to component changes
  @ViewChild('memberTabs', {static: true}) memberTabs: TabsetComponent;
  member: Member;
  galleryOptions: NgxGalleryOptions[];
  galleryImages: NgxGalleryImage[];
  activeTab: TabDirective;
  messages: Message[] = [];

  constructor(private memberService: MembersService, private route: ActivatedRoute, private messageService: MessageService) { }

  ngOnInit(): void {
    // by using Router resolvers like this, we don't need to check "*ngIf='member'" exists on component load for the messages tab feature.
    // we get the member data prior to the component being initialized, from the route. (see member-detailed.resolver.ts)
    // make sure to remove that *ngIf conditional when using this, otherwise, will cause errors.
    // need this for feature that directly accesses Message tab on profile load.
    this.route.data.subscribe(data => {
      this.member = data.member; // guaranteed to have the member in the route
    });


    // determines which tab to route us to. if query params for tab is 3, go to messages tab.
    this.route.queryParams.subscribe(params => {
      params.tab ? this.selectTab(params.tab) : this.selectTab(0);
    });

    this.galleryOptions = [
      {
        width: '500px',
        height: '500px',
        imagePercent: 100,
        thumbnailsColumns: 4,
        imageAnimation: NgxGalleryAnimation.Slide,
        preview: false
      }
    ];

    this.galleryImages = this.getImages();
  }

  getImages(): NgxGalleryImage[] {
    const imageUrls = [];
    for (const photo of this.member.photos) {
      imageUrls.push({
        small: photo.url,
        medium: photo.url,
        big: photo.url
      });
    }
    return imageUrls;
  }

  // loadMember() {
  //   // get the parameters from the route snapshot.
  //   this.memberService.getMember(this.route.snapshot.paramMap.get('username')).subscribe(member => {
  //     this.member = member;
  //   });
  // }


  loadMessages() {
    this.messageService.getMessageThread(this.member.username).subscribe(messages => {
      this.messages = messages;
    });
  }

  selectTab(tabId: number) {
    this.memberTabs.tabs[tabId].active = true;
    // in html, we hardcoded 3 to access the message tab. (tabs are like an index in the tabset array.)
  }

  // method that helps get messages only when that tab is activated
  onTabActivated(data: TabDirective) {
    this.activeTab = data;
    if (this.activeTab.heading === 'Messages' && this.messages.length === 0) {
      this.loadMessages();
    }
  }

}

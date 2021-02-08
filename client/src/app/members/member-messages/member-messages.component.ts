import { ChangeDetectionStrategy, Component, Input, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Message } from 'src/app/_models/message';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,  // in order to have a scrolling property in the component, need to set this to OnPush.
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent implements OnInit {
  @ViewChild('messageForm') messageForm: NgForm;
  @Input() messages: Message[];
  // need username for conditional that displays read/unread. Only display if sender is not the logged in user
  @Input() username: string;
  messageContent: string; // make sure the name of the input in the form is the same
  loading = false;

  constructor(public messageService: MessageService) { }

  ngOnInit(): void {}

  sendMessage() {
    // custom loading here because Signal R does not use HTTP, so our loading interceptor does not handle the delay
    // use this in the html to disable send message button if loading is true.
    this.loading = true;
    // here, this.username is the username of the profile the logged in user is on.
    // sendMessage takes the recipient, then the messageContent.
    this.messageService.sendMessage(this.username, this.messageContent).then(() => {
      // don't push the message anymore because we are receiving it from our Signal R hub
      // this.messages.push(message);
      this.messageForm.reset();
    }).finally(() => this.loading = false);
  }
}

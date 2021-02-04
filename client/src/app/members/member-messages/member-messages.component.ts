import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Message } from 'src/app/_models/message';
import { MessageService } from 'src/app/_services/message.service';

@Component({
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

  constructor(public messageService: MessageService) { }

  ngOnInit(): void {}

  sendMessage() {
    // here, this.username is the username of the profile the logged in user is on.
    // sendMessage takes the recipient, then the messageContent.
    this.messageService.sendMessage(this.username, this.messageContent).then(() => {
      // don't push the message anymore because we are receiving it from our Signal R hub
      // this.messages.push(message);
      this.messageForm.reset();
    });
  }
}

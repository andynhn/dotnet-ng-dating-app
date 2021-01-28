import { Component, Input, OnInit } from '@angular/core';
import { Member } from 'src/app/_models/member';

@Component({
  selector: 'app-member-card',
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.css']
})
export class MemberCardComponent implements OnInit {
  // value is passed down from Parent (Member-list) to Child (Member-card). Describe variable accordingly.
  @Input() memberFromMemberCardComponent: Member;

  constructor() { }

  ngOnInit(): void {
  }

}

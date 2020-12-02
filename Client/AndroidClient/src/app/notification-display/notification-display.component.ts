import { Component, OnInit } from '@angular/core';
import { NotificationMessage } from '../models';

const maxNotifications = 64;
export const notifications: NotificationMessage[] = [];
export function notify(message: string, kind: 'info' | 'warn' | 'err') {
  notifications.push({
    kind,
    message
  });

  if (notifications.length > maxNotifications) {
    notifications.splice(0, 1);
  }
}

@Component({
  selector: 'app-notification-display',
  templateUrl: './notification-display.component.html',
  styleUrls: ['./notification-display.component.less']
})
export class NotificationDisplayComponent implements OnInit {

  public notifications = notifications;

  constructor() { }

  ngOnInit(): void {
  }

}

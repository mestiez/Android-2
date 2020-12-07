import { Component, OnInit } from '@angular/core';
import { NotificationMessage } from '../models';

const notificationListeners: ((m: NotificationMessage) => void)[] = [];
const maxNotifications = 64;
const notifications: NotificationMessage[] = [];

export function notify(message: string, kind: 'info' | 'warn' | 'err') {
  const m = {
    kind,
    message
  };
  notifications.push(m);

  if (notifications.length > maxNotifications) {
    notifications.splice(0, 1);
  }

  notificationListeners.forEach(action => {
    action(m);
  });
}

@Component({
  selector: 'app-notification-display',
  templateUrl: './notification-display.component.html',
  styleUrls: ['./notification-display.component.less']
})
export class NotificationDisplayComponent implements OnInit {

  public messages: NotificationMessage[] = [];

  constructor() { }

  ngOnInit(): void {
    notificationListeners.push(this.onNotify);
  }

  private onNotify = (message: NotificationMessage) => {
    this.messages.unshift(message);
  }
}

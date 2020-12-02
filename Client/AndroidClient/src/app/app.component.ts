import { Component } from '@angular/core';
import { GuildManagerService } from './guild-manager.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.less']
})
export class AppComponent {
  constructor(public guildManager: GuildManagerService) { }
}

import { Component, OnInit } from '@angular/core';
import { AuthorisationService } from './authorisation.service';
import { GuildManagerService } from './guild-manager.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.less']
})
export class AppComponent implements OnInit {
  public tokenInput = '';
  constructor(
    public guildManager: GuildManagerService,
    public auth: AuthorisationService) { }

  ngOnInit(): void {
    if (window.localStorage.botToken) {
      this.tokenInput = window.localStorage.botToken;
    }
  }
}

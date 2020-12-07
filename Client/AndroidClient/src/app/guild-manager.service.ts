import { Injectable } from '@angular/core';
import { GuildInfo } from './models';
import { RestService } from './rest.service';

@Injectable({
  providedIn: 'root'
})
export class GuildManagerService {

  public currentGuild: GuildInfo;
  public hasCurrentGuild = false;

  public guilds: GuildInfo[] = [];

  public isBusy = false;
  public isError = false;

  constructor(private rest: RestService) { }

  public setCurrentGuild(guild: GuildInfo) {
    if (!guild) {
      this.currentGuild = null;
      this.hasCurrentGuild = false;
    } else {
      this.currentGuild = guild;
      this.hasCurrentGuild = true;
    }
  }

  public refreshGuilds() {
    this.isBusy = true;
    this.isError = false;

    this.rest.getGuilds().subscribe((guilds: GuildInfo[]) => {
      this.guilds = guilds;
      this.isBusy = false;
    }, (error) => {
      console.error(error);
      this.isError = true;
      this.isBusy = false;
    });
  }
}

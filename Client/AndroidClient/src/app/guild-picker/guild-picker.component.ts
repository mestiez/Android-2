import { Component, OnInit } from '@angular/core';
import { GuildManagerService } from '../guild-manager.service';
import { noImage } from '../models';

@Component({
  selector: 'app-guild-picker',
  templateUrl: './guild-picker.component.html',
  styleUrls: ['./guild-picker.component.less']
})
export class GuildPickerComponent implements OnInit {

  public noImage = noImage;

  constructor(public guildManager: GuildManagerService) { }

  ngOnInit(): void {
    this.guildManager.refreshGuilds();
  }

}

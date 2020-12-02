import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { DialogService } from '../dialog.service';
import { GuildManagerService } from '../guild-manager.service';
import { notify } from '../notification-display/notification-display.component';
import { RestService } from '../rest.service';

@Component({
  selector: 'app-important-controls',
  templateUrl: './important-controls.component.html',
  styleUrls: ['./important-controls.component.less']
})
export class ImportantControlsComponent implements OnInit {

  constructor(private dialogService: DialogService, private rest: RestService, private guildService: GuildManagerService) { }

  ngOnInit(): void {
  }

  public leaveGuild() {
    this.dialogService.currentDialog = {
      question: 'You are about to delete this instance.\nThis action cannot be undone.',
      action: () => {
        this.rest.leaveGuild(this.guildService.currentGuild.id).subscribe(() => {
          this.guildService.setCurrentGuild(null);
        }, (e: HttpErrorResponse) => {
          notify('Could not leave guild: ' + e.message, 'err');
        });
      }
    };
  }

  public saveToDisk() {
    this.rest.saveToDisk(this.guildService.currentGuild.id).subscribe(() => {
      notify('instance saved', 'info');
    }, (e) => {
      notify('failed to save instance', 'err');
    });
  }

  public shutdown() {
    this.dialogService.currentDialog = {
      question: 'You are about to stop the server application.',
      action: () => {
        notify('shutdown requested...', 'warn');
        this.rest.shutdown().subscribe(() => {
          notify('shutdown success. you should close this page', 'info');
        }, (e) => {
          notify('shutdown failed: ' + e.message, 'err');
        });
      }
    };
  }
}

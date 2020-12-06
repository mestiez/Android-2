import { HttpErrorResponse } from '@angular/common/http';
import { ThrowStmt } from '@angular/compiler';
import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { DialogService } from '../dialog.service';
import { GuildManagerService } from '../guild-manager.service';
import {
  ChannelInfo, EmoteInfo, ListenerInfo, ListenerType, ListenerUIInfo,
  noImage, ResponseFilters, RoleInfo, SayInfo, UserInfo, VariableType
} from '../models';
import { notify } from '../notification-display/notification-display.component';
import { RestService } from '../rest.service';

@Component({
  selector: 'app-instance-editor',
  templateUrl: './instance-editor.component.html',
  styleUrls: ['./instance-editor.component.less']
})
export class InstanceEditorComponent implements OnInit {

  public noImage = noImage;

  public channels: ChannelInfo[] = [];
  public textChannels: ChannelInfo[] = [];
  public voiceChannels: ChannelInfo[] = [];

  public roles: RoleInfo[] = [];
  public selectedChannel: ChannelInfo = null;
  public selectedChannelListeners: ListenerInfo[] = null;
  public emotes: EmoteInfo[] = [];

  public listenerTypes: ListenerType[] = [];
  public selectedAddListenerTypeID: string = null;

  public selectedListener: ListenerInfo = null;
  public selectedListenerUI: ListenerUIInfo = null;

  public selectedListenerSettings = {};
  public sendMessageInfo: SayInfo = new SayInfo();

  public selectedResponseFilters: ResponseFilters = null;
  public selectedResponseRole: RoleInfo = null;
  public selectedResponseUserID: string = null;

  public currentInstanceActive = false;

  constructor(public guildService: GuildManagerService, public rest: RestService, public dialogService: DialogService) { }

  ngOnInit(): void {
    this.rest.getChannels(this.guildService.currentGuild.id).subscribe((channels) => {
      this.channels = channels.filter(c => c.channelType !== 'category');
      this.textChannels = channels.filter(c => c.channelType === 'text');
      this.voiceChannels = channels.filter(c => c.channelType === 'voice');
      notify(this.channels.length + ' channels retrieved', 'info');
    }, (e) => {
      notify('failed to get guild channels', 'err');
    });

    this.rest.getRoles(this.guildService.currentGuild.id).subscribe((roles) => {
      this.roles = roles;
      notify(roles.length + ' roles retrieved', 'info');
    }, (e) => {
      notify('failed to get guild roles', 'err');
    });

    this.rest.getListenerTypes().subscribe((types) => {
      this.listenerTypes = types;
      notify(types.length + ' listener types retrieved', 'info');
    }, (e) => {
      notify('failed to get listener types', 'err');
    });

    this.rest.getEmotes(this.guildService.currentGuild.id).subscribe((emotes) => {
      this.emotes = emotes;
      notify(emotes.length + ' emotes retrieved', 'info');
    }, (e) => {
      notify('failed to get listener types', 'err');
    });

    this.getInstanceActiveState();
  }

  private getInstanceActiveState() {
    this.rest.getInstanceActiveState(this.guildService.currentGuild.id).subscribe((b) => {
      this.currentInstanceActive = b;
    }, (e) => {
      notify('failed to get instance active state', 'err');
    });
  }

  public setChannel(channel: ChannelInfo): void {
    this.selectedChannel = channel;
    this.selectedListener = null;
    this.sendMessageInfo = new SayInfo();
    this.selectedResponseFilters = null;
    this.refreshListeners();
  }

  public selectListener(listener: ListenerInfo) {
    if (this.selectedListener === listener) {
      return;
    }

    this.selectedListenerSettings = {};
    this.selectedListener = null;
    this.rest.getListenerUI(listener.listenerType.typeID).subscribe((info: ListenerUIInfo) => {
      this.selectedListenerUI = info;
      this.selectedListener = listener;
      this.populateListenerSettings();
    }, (e) => {
      notify('failed to get listener UI', 'err');
    });

    this.rest.getListenerFilter(listener.id).subscribe((filter: ResponseFilters) => {
      this.selectedResponseFilters = filter;
    }, (e) => {
      notify('failed to get listener response filter', 'err');
    });
  }

  public populateListenerSettings() {
    this.selectedListenerSettings = {};
    for (const variable of this.selectedListenerUI.variables) {
      this.rest.getListenerSetting(this.selectedListener.id, variable.name).subscribe((v: any) => {
        this.selectedListenerSettings[variable.name] = v ? v.data : null;
      }, (e) => {
        notify('failed to get listener setting', 'err');
      });
    }
  }

  public setListenerSetting(variable: { name: string, type: VariableType }, value: any) {
    value = {
      data: value
    };

    this.rest.setListenerSetting(this.selectedListener.id, variable.name, value).subscribe(() => {
      // this.populateListenerSettings();
      notify('listener setting set to ' + value.data, 'info');
      if (variable.name === 'GlobalListener') {
        this.refreshListeners();
      } else if (variable.name === 'DisplayName') {
        this.selectedListener.displayName = value.data;
      }
    }, (e) => {
      notify('invalid listener setting value', 'err');
    });
  }

  public refreshListeners(): void {
    this.selectedChannelListeners = null;
    this.rest.getListeners(this.guildService.currentGuild.id, this.selectedChannel.id).subscribe((listeners: ListenerInfo[]) => {
      console.log(listeners);
      this.selectedChannelListeners = listeners;
      for (const iterator of listeners) {
        iterator.channelName = this.getChannelName(iterator.channelID);
      }
    }, (e) => {
      notify('failed to get channel listeners', 'err');
    });
  }

  public setListenerState(listener: ListenerInfo, state: boolean): void {
    this.rest.setListenerActive(listener.id, state).subscribe(() => {
      listener.active = state;
      this.refreshListeners();
    }, (e) => {
      notify('failed to set listener active state', 'err');
    });
  }

  public addListener(typeID: string) {
    this.rest.addListener(this.guildService.currentGuild.id, this.selectedChannel.id, typeID).subscribe(() => {
      this.refreshListeners();
      notify('listener added', 'info');
    }, (e) => {
      notify('failed to add listener', 'err');
    });
  }

  public removeListener(id: string) {
    this.dialogService.currentDialog = {
      question: 'You are about to delete this listener.\nThis action cannot be undone.',
      action: () => {
        this.rest.removeListener(this.guildService.currentGuild.id, id).subscribe(() => {
          notify(`listener ${id} removed`, 'info');
          this.setChannel(this.selectedChannel);
        }, () => {
          notify(`failed to remove listener ${id}`, 'err');
        });
      }
    };
  }

  public saveResponseFilter() {
    this.rest.setListenerFilter(this.selectedListener.id, this.selectedResponseFilters).subscribe(() => {
      notify('response filter saved', 'info');
    }, (e) => {
      notify('failed set listener response filter', 'err');
    });
  }

  public addResponseRole(role: RoleInfo) {
    if (role) {
      this.selectedResponseFilters.roles.push(role);
      this.saveResponseFilter();
      this.selectedResponseRole = null;
    }
  }

  public setInstanceActive(state: boolean) {
    this.rest.setInstanceActiveState(this.guildService.currentGuild.id, state).subscribe(() => {
      notify('instance active state set to ' + state, 'info');
      this.getInstanceActiveState();
    }, (e) => {
      notify('failed set instance active state', 'err');
    });
  }

  public removeResponseRole(role: RoleInfo) {
    if (role) {
      const index = this.selectedResponseFilters.roles.findIndex((r) => r.id === role.id);
      if (index === -1) {
        return;
      }

      this.selectedResponseFilters.roles.splice(index, 1);
      this.saveResponseFilter();
    }
  }

  public addResponseUser(id: string) {
    this.selectedResponseUserID = null;
    if (id) {
      this.rest.getUser(id, this.guildService.currentGuild.id).subscribe((u) => {
        if (!u) {
          notify(`${id} is an invalid guild user ID`, 'err');
          return;
        }
        this.selectedResponseFilters.users.push(u);
        this.saveResponseFilter();
      }, () => {
        notify(`${id} is an invalid guild user ID`, 'err');
      });
    }
  }

  public removeResponseUser(user: UserInfo) {
    if (user) {
      const index = this.selectedResponseFilters.users.findIndex((r) => r.id === user.id);
      if (index === -1) {
        return;
      }

      this.selectedResponseFilters.users.splice(index, 1);
      this.saveResponseFilter();
    }
  }

  public setMessageFile(event: any) {
    const reader = new FileReader();
    reader.onload = (e: any) => {
      let dataString = e.target.result.toString();
      const i = dataString.indexOf(',') + 1;
      dataString = dataString.substr(i);
      this.sendMessageInfo.fileB64 = dataString;
    };
    reader.readAsDataURL(event.target.files[0]);
  }

  public handleMessageInput(event: KeyboardEvent) {
    if (event.key === 'Enter' && !event.shiftKey) {
      this.say();
      event.preventDefault();
    }
  }

  public handleUserInput(event: KeyboardEvent) {
    if (event.key === 'Enter') {
      this.addResponseUser(this.selectedResponseUserID);
    }
  }

  public say() {
    const info = this.sendMessageInfo;
    this.rest.say(this.selectedChannel.id, info).subscribe(() => {
      if (info.message) {
        notify(`'${info.message}' sent to ${this.selectedChannel.name}`, 'info');
      }
      if (info.fileB64) {
        notify(`'${info.fileName}' sent to ${this.selectedChannel.name}`, 'info');
      }
    }, (e: HttpErrorResponse) => {
      notify('failed to send server message: ' + e.message, 'err');
    });
    this.sendMessageInfo = new SayInfo();
  }

  public getChannelName(id: string) {
    for (const channel of this.channels) {
      if (channel.id === id) {
        return channel.name;
      }
    }
    return id;
  }
}

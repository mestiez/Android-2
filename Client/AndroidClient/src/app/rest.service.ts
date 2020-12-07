import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  ChannelInfo, EmoteInfo, GuildInfo, ListenerInfo, ListenerType,
  ListenerUIInfo, ResponseFilters, restEndpoint, RoleInfo, SayInfo, UserInfo
} from './models';

@Injectable({
  providedIn: 'root'
})
export class RestService {
  constructor(private http: HttpClient) {
  }

  public shutdown() {
    return this.http.post(`${restEndpoint}system/shutdown`, null);
  }

  public getGuilds() {
    return this.http.get<GuildInfo[]>(`${restEndpoint}guild/all`);
  }

  public getInstanceActiveState(id: string) {
    return this.http.get<boolean>(`${restEndpoint}instance/active?guildID=${id}`);
  }

  public setInstanceActiveState(id: string, state: boolean) {
    return this.http.post(`${restEndpoint}instance/active?guildID=${id}&active=${state}`, null);
  }

  public getChannels(id: string) {
    return this.http.get<ChannelInfo[]>(`${restEndpoint}guild/channels?guildID=${id}`);
  }

  public getUser(id: string, guildID: string) {
    return this.http.get<UserInfo>(`${restEndpoint}guild/user?id=${id}&guildID=${guildID}`);
  }

  public getRoles(guildID: string) {
    return this.http.get<RoleInfo[]>(`${restEndpoint}guild/roles?guildID=${guildID}`);
  }

  public getListeners(guildId: string, channelId: string) {
    return this.http.get<ListenerInfo[]>(`${restEndpoint}instance/listeners?guildID=${guildId}&channelID=${channelId}`);
  }

  public getListenerTypes() {
    return this.http.get<ListenerType[]>(`${restEndpoint}listener/types`);
  }

  public getListenerUI(typeID: string) {
    return this.http.get<ListenerUIInfo>(`${restEndpoint}listener/ui?typeID=${typeID}`);
  }

  public getListenerSetting(id: string, propertyName: string) {
    return this.http.get<{ data: any }>(`${restEndpoint}listener/setting?id=${id}&propertyName=${propertyName}`);
  }

  public setListenerSetting(id: string, propertyName: string, value: { data: any }) {
    return this.http.post(`${restEndpoint}listener/setting?id=${id}&propertyName=${propertyName}`, value);
  }

  public setListenerActive(id: string, active: boolean) {
    return this.http.post(`${restEndpoint}listener/active?id=${id}&active=${active}`, null);
  }

  public addListener(guildID: string, channelID: string, listenerTypeID: string) {
    return this.http.post(
      `${restEndpoint}instance/listeners?guildID=${guildID}&channelID=${channelID}&listenerTypeID=${listenerTypeID}`
      , null);
  }

  public removeListener(guildID: string, listenerID: string) {
    return this.http.delete(`${restEndpoint}instance/listeners?guildID=${guildID}&&listenerID=${listenerID}`);
  }

  public say(channelID: string, info: SayInfo) {
    const data = `${info.message || ''}\n${info.fileName || ''}\n${info.fileB64 || ''}`;
    return this.http.post(`${restEndpoint}instance/say?channelID=${channelID}`, data);
  }

  public getListenerFilter(id: string) {
    return this.http.get<ResponseFilters>(`${restEndpoint}listener/filter?id=${id}`);
  }

  public setListenerFilter(id: string, filter: ResponseFilters) {
    return this.http.post(`${restEndpoint}listener/filter?id=${id}`, filter);
  }

  public leaveGuild(id: string) {
    return this.http.delete(`${restEndpoint}instance?guildID=${id}`);
  }

  public saveToDisk(id: string) {
    return this.http.post(`${restEndpoint}instance/save?guildID=${id}`, null);
  }

  public getEmotes(guildId: string) {
    return this.http.get<EmoteInfo[]>(`${restEndpoint}guild/emotes?guildID=${guildId}`);
  }
}

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

  private httpOptions = {
    headers: new HttpHeaders({
      'Content-Type': 'application/json',
      'Access-Control-Allow-Origin': '*',
      'Access-Control-Allow-Headers': '*'
    })
  };

  constructor(private http: HttpClient) {
  }

  public shutdown() {
    return this.http.post(`${restEndpoint}System/shutdown`, null, this.httpOptions);
  }

  public getGuilds() {
    return this.http.get<GuildInfo[]>(`${restEndpoint}Guild/all`);
  }

  public getInstanceActiveState(id: string) {
    return this.http.get<boolean>(`${restEndpoint}AndroidInstance/active?guildID=${id}`);
  }

  public setInstanceActiveState(id: string, state: boolean) {
    return this.http.post(`${restEndpoint}AndroidInstance/active?guildID=${id}&active=${state}`, null, this.httpOptions);
  }

  public getChannels(id: string) {
    return this.http.get<ChannelInfo[]>(`${restEndpoint}Guild/channels?guildID=${id}`);
  }

  public getUser(id: string, guildID: string) {
    return this.http.get<UserInfo>(`${restEndpoint}Guild/user?id=${id}&guildID=${guildID}`);
  }

  public getRoles(guildID: string) {
    return this.http.get<RoleInfo[]>(`${restEndpoint}Guild/roles?guildID=${guildID}`);
  }

  public getListeners(guildId: string, channelId: string) {
    return this.http.get<ListenerInfo[]>(`${restEndpoint}AndroidInstance/listeners?guildID=${guildId}&channelID=${channelId}`);
  }

  public getListenerTypes() {
    return this.http.get<ListenerType[]>(`${restEndpoint}Listener/types`);
  }

  public getListenerUI(typeID: string) {
    return this.http.get<ListenerUIInfo>(`${restEndpoint}Listener/ui?typeID=${typeID}`);
  }

  public getListenerSetting(id: string, propertyName: string) {
    return this.http.get<{ data: any }>(`${restEndpoint}Listener/setting?id=${id}&propertyName=${propertyName}`);
  }

  public setListenerSetting(id: string, propertyName: string, value: { data: any }) {
    return this.http.post(`${restEndpoint}Listener/setting?id=${id}&propertyName=${propertyName}`, value, this.httpOptions);
  }

  public setListenerActive(id: string, active: boolean) {
    return this.http.post(`${restEndpoint}Listener/active?id=${id}&active=${active}`, null);
  }

  public addListener(guildID: string, channelID: string, listenerTypeID: string) {
    return this.http.post(`${restEndpoint}AndroidInstance/listeners?guildID=${guildID}&channelID=${channelID}&listenerTypeID=${listenerTypeID}`, null);
  }

  public removeListener(guildID: string, listenerID: string) {
    return this.http.delete(`${restEndpoint}AndroidInstance/listeners?guildID=${guildID}&&listenerID=${listenerID}`);
  }

  public say(channelID: string, info: SayInfo) {
    return this.http.post(`${restEndpoint}AndroidInstance/say?channelID=${channelID}`, info);
  }

  public getListenerFilter(id: string) {
    return this.http.get<ResponseFilters>(`${restEndpoint}Listener/filter?id=${id}`);
  }

  public setListenerFilter(id: string, filter: ResponseFilters) {
    return this.http.post(`${restEndpoint}Listener/filter?id=${id}`, filter, this.httpOptions);
  }

  public leaveGuild(id: string) {
    return this.http.delete(`${restEndpoint}AndroidInstance?guildID=${id}`);
  }

  public saveToDisk(id: string) {
    return this.http.post(`${restEndpoint}AndroidInstance/save?guildID=${id}`, null);
  }

  public getEmotes(guildId: string) {
    return this.http.get<EmoteInfo[]>(`${restEndpoint}Guild/emotes?guildID=${guildId}`);
  }
}

export const restEndpoint = 'http://REPLACE ME WITH THE SERVER ADDRESS/';
export const noImage = 'https://i.imgur.com/THyjIPy.png';

export class GuildInfo {
    public id = '';
    public name = '';
    public iconURL = '';
}

export class ChannelInfo {
    public id = '';
    public name = '';
    public description: string = null;
    public channelType: 'text' | 'category' | 'voice' = 'text';
    public position = 0;
    public categoryID: string = null;
}

export class ListenerInfo {
    public active = false;
    public name = '';
    public displayName = '';
    public id = '';
    public channelID = '';
    public listenerType: ListenerType = null;
    public globalListener = false;
    public channelName = '';
}

export class ListenerType {
    public typeName = '';
    public typeID = '';
}

export class ListenerUIInfo {
    public variables: { name: string, type: VariableType }[] = [];
}

export class RoleInfo {
    public id = '';
    public name = '';
    public color = '';
}

export class UserInfo {
    public id = '';
    public username = '';
    public discriminator = '';
    public nickname = '';
}

export class ResponseFilters {
    public blacklist = false;
    public users: UserInfo[] = [];
    public roles: RoleInfo[] = [];
}

export enum VariableType {
    Unknown,
    Number,
    String,
    Boolean,
    TextChannel,
    VoiceChannel,
    RoleID,
    TextArea,
    EmoteID
}

export class SayInfo {
    public message: string;
    public fileB64: string;
    public fileName: string;
}

export class NotificationMessage {
    public message: string;
    public kind: 'info' | 'warn' | 'err';
}

export class EmoteInfo{
    public name = '';
    public url = '';
    public id = '';
}

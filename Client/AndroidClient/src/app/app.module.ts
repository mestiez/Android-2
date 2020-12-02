import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

import { AppComponent } from './app.component';
import { GuildPickerComponent } from './guild-picker/guild-picker.component';
import { InstanceEditorComponent } from './instance-editor/instance-editor.component';
import { CheckBoxComponent } from './check-box/check-box.component';
import { NotificationDisplayComponent } from './notification-display/notification-display.component';
import { DialogScreenComponent } from './dialog-screen/dialog-screen.component';
import { ImportantControlsComponent } from './important-controls/important-controls.component';

@NgModule({
  declarations: [
    AppComponent,
    InstanceEditorComponent,
    GuildPickerComponent,
    CheckBoxComponent,
    NotificationDisplayComponent,
    DialogScreenComponent,
    ImportantControlsComponent,
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    FormsModule,
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }

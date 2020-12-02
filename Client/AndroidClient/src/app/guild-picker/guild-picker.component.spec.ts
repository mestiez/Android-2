import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GuildPickerComponent } from './guild-picker.component';

describe('GuildPickerComponent', () => {
  let component: GuildPickerComponent;
  let fixture: ComponentFixture<GuildPickerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ GuildPickerComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(GuildPickerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NotificationDisplayComponent } from './notification-display.component';

describe('NotificationDisplayComponent', () => {
  let component: NotificationDisplayComponent;
  let fixture: ComponentFixture<NotificationDisplayComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ NotificationDisplayComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(NotificationDisplayComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

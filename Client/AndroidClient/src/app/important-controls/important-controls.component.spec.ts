import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ImportantControlsComponent } from './important-controls.component';

describe('ImportantControlsComponent', () => {
  let component: ImportantControlsComponent;
  let fixture: ComponentFixture<ImportantControlsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ImportantControlsComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ImportantControlsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

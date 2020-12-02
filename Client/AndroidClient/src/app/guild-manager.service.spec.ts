import { TestBed } from '@angular/core/testing';

import { GuildManagerService } from './guild-manager.service';

describe('GuildManagerService', () => {
  let service: GuildManagerService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(GuildManagerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});

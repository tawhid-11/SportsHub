import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RegisteredTeams } from './registered-teams';

describe('RegisteredTeams', () => {
  let component: RegisteredTeams;
  let fixture: ComponentFixture<RegisteredTeams>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RegisteredTeams]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RegisteredTeams);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RegisterTournament } from './register-tournament';

describe('RegisterTournament', () => {
  let component: RegisterTournament;
  let fixture: ComponentFixture<RegisterTournament>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RegisterTournament]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RegisterTournament);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

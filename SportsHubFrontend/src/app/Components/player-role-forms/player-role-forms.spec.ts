import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PlayerRoleForms } from './player-role-forms';

describe('PlayerRoleForms', () => {
  let component: PlayerRoleForms;
  let fixture: ComponentFixture<PlayerRoleForms>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlayerRoleForms]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PlayerRoleForms);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

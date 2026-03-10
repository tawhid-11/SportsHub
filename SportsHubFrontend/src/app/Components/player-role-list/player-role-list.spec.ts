import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PlayerRoleList } from './player-role-list';

describe('PlayerRoleList', () => {
  let component: PlayerRoleList;
  let fixture: ComponentFixture<PlayerRoleList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlayerRoleList]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PlayerRoleList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

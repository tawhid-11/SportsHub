import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TeamOwnerLayout } from './team-owner-layout';

describe('TeamOwnerLayout', () => {
  let component: TeamOwnerLayout;
  let fixture: ComponentFixture<TeamOwnerLayout>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TeamOwnerLayout]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TeamOwnerLayout);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

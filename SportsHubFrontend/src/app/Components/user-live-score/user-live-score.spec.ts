import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UserLiveScore } from './user-live-score';

describe('UserLiveScore', () => {
  let component: UserLiveScore;
  let fixture: ComponentFixture<UserLiveScore>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserLiveScore]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UserLiveScore);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

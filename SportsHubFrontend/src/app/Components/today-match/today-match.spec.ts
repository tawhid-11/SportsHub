import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TodayMatch } from './today-match';

describe('TodayMatch', () => {
  let component: TodayMatch;
  let fixture: ComponentFixture<TodayMatch>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TodayMatch]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TodayMatch);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

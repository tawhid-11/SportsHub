import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HomeTournament } from './home-tournament';

describe('HomeTournament', () => {
  let component: HomeTournament;
  let fixture: ComponentFixture<HomeTournament>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HomeTournament]
    })
    .compileComponents();

    fixture = TestBed.createComponent(HomeTournament);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

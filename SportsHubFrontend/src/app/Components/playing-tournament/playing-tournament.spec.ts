import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PlayingTournament } from './playing-tournament';

describe('PlayingTournament', () => {
  let component: PlayingTournament;
  let fixture: ComponentFixture<PlayingTournament>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlayingTournament]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PlayingTournament);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

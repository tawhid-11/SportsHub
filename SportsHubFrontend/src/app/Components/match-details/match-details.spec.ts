import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MatchDetails } from './match-details';

describe('MatchDetails', () => {
  let component: MatchDetails;
  let fixture: ComponentFixture<MatchDetails>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MatchDetails]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MatchDetails);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

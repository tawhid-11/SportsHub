import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ListofTournamentForms } from './listof-tournament-forms';

describe('ListofTournamentForms', () => {
  let component: ListofTournamentForms;
  let fixture: ComponentFixture<ListofTournamentForms>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ListofTournamentForms]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ListofTournamentForms);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

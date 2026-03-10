import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ListofTournaments } from './listof-tournaments';

describe('ListofTournaments', () => {
  let component: ListofTournaments;
  let fixture: ComponentFixture<ListofTournaments>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ListofTournaments]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ListofTournaments);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

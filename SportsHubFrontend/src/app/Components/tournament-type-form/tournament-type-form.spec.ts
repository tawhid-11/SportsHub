import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TournamentTypeForm } from './tournament-type-form';

describe('TournamentTypeForm', () => {
  let component: TournamentTypeForm;
  let fixture: ComponentFixture<TournamentTypeForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TournamentTypeForm]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TournamentTypeForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

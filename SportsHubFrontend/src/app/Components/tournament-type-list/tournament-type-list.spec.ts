import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TournamentTypeList } from './tournament-type-list';

describe('TournamentTypeList', () => {
  let component: TournamentTypeList;
  let fixture: ComponentFixture<TournamentTypeList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TournamentTypeList]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TournamentTypeList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

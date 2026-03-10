import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ListofPlayer } from './listof-player';

describe('ListofPlayer', () => {
  let component: ListofPlayer;
  let fixture: ComponentFixture<ListofPlayer>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ListofPlayer]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ListofPlayer);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ViewPlayer } from './view-player';

describe('ViewPlayer', () => {
  let component: ViewPlayer;
  let fixture: ComponentFixture<ViewPlayer>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ViewPlayer]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ViewPlayer);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

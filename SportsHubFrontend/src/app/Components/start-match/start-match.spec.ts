import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StartMatch } from './start-match';

describe('StartMatch', () => {
  let component: StartMatch;
  let fixture: ComponentFixture<StartMatch>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StartMatch]
    })
    .compileComponents();

    fixture = TestBed.createComponent(StartMatch);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PlFooter } from './pl-footer';

describe('PlFooter', () => {
  let component: PlFooter;
  let fixture: ComponentFixture<PlFooter>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlFooter]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PlFooter);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

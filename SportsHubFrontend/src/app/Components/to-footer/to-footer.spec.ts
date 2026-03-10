import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ToFooter } from './to-footer';

describe('ToFooter', () => {
  let component: ToFooter;
  let fixture: ComponentFixture<ToFooter>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ToFooter]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ToFooter);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ToHeader } from './to-header';

describe('ToHeader', () => {
  let component: ToHeader;
  let fixture: ComponentFixture<ToHeader>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ToHeader]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ToHeader);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

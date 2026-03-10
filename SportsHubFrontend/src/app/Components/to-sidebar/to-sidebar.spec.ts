import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ToSidebar } from './to-sidebar';

describe('ToSidebar', () => {
  let component: ToSidebar;
  let fixture: ComponentFixture<ToSidebar>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ToSidebar]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ToSidebar);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

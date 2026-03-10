import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PlSidebar } from './pl-sidebar';

describe('PlSidebar', () => {
  let component: PlSidebar;
  let fixture: ComponentFixture<PlSidebar>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlSidebar]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PlSidebar);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

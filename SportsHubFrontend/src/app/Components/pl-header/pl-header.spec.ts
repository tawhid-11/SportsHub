import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PlHeader } from './pl-header';

describe('PlHeader', () => {
  let component: PlHeader;
  let fixture: ComponentFixture<PlHeader>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlHeader]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PlHeader);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

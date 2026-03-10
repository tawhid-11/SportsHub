import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PaymentConfirmation } from './payment-confirmation';

describe('PaymentConfirmation', () => {
  let component: PaymentConfirmation;
  let fixture: ComponentFixture<PaymentConfirmation>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PaymentConfirmation]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PaymentConfirmation);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

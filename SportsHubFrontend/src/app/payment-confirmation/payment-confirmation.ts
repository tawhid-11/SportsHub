
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Httpclientservice } from '../Service/httpclientservice';

@Component({
  selector: 'app-payment-confirmation',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './payment-confirmation.html',
  styleUrl: './payment-confirmation.css',
})
export class PaymentConfirmation implements OnInit {

  loading = true;
  errorMessage = '';
  successMessage = '';

  constructor(
    private route: ActivatedRoute,
    private http: Httpclientservice,
    private router: Router
  ) { }

  ngOnInit(): void {
    const paymentId = this.route.snapshot.queryParamMap.get('paymentID');
    const status = this.route.snapshot.queryParamMap.get('status');

    if (paymentId && status === 'success') {
      this.confirmPayment(paymentId);
    } else {
      this.errorMessage = 'Payment failed or cancelled';
      this.loading = false;
    }
  }

  confirmPayment(paymentId: string) {
    this.http.GetData('Teams/Success_URL?paymemtId=' + paymentId)
      .subscribe({
        next: (res) => {
          this.loading = false;
          this.successMessage = 'Tournament registration confirmed successfully!';
          setTimeout(() => {
            this.router.navigate(['/']);
          }, 3000);
        },
        error: () => {
          this.loading = false;
          this.errorMessage = 'Server error while verifying payment';
        }
      });
  }
}
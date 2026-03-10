
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';


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
    private http: HttpClient,
    private router: Router
  ) { }

  ngOnInit(): void {
    debugger;
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
    this.http.get<any>('https://localhost:7142/api/Teams/Success_URL?paymemtId=' + paymentId)
      .subscribe({
        next: (res) => {
          this.loading = false;
  this.successMessage = 'Tournament registration confirmed successfully!';
            setTimeout(() => {
              this.router.navigate(['/']);
            }, 3000);
          // if (res.success || res.statusCode === '0000') {
          //   this.successMessage = 'Tournament registration confirmed successfully!';
          //   setTimeout(() => {
          //     this.router.navigate(['/']);
          //   }, 3000);
          // } else {
          //   this.errorMessage = 'Payment verification failed: ' + (res.statusMessage || 'Unknown error');
          // }
        },
        error: () => {
          this.loading = false;
          this.errorMessage = 'Server error while verifying payment';
        }
      });
  }
}
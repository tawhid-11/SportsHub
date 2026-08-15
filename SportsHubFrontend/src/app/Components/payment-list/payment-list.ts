import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Httpclientservice } from '../../Service/httpclientservice';

interface PaymentRecord {
  Id?: number;
  TournamentId?: number;
  TeamId?: number;
  PaymentStatus?: string;
  bkashPaymentId?: string;
  PaymentDate?: string;
  bkashTransactionId?: string;
  CreatedDate?: string;
  GroupId?: number;
  TournamentName?: string;
  RegistrationFee?: number;
  TeamName?: string;
  TeamOwnerName?: string;
  TeamOwnerEmail?: string;
  TeamOwnerPhoneNumber?: string;
  // Legacy fields for backward compatibility
  teamName?: string;
  tournamentName?: string;
  amount?: number;
  paymentStatus?: string;
  paymentDate?: string;
  phone?: string;
}

@Component({
  selector: 'app-payment-list',
  imports: [CommonModule],
  templateUrl: './payment-list.html',
  styleUrl: './payment-list.css'
})
export class PaymentList implements OnInit {
  payments: PaymentRecord[] = [];
  filteredPayments: PaymentRecord[] = [];
  loading: boolean = true;
  errorMessage: string = '';
  selectedStatus: string = 'all';

  constructor(private http: Httpclientservice) { }

  ngOnInit(): void {
    this.loadPayments();
  }

  loadPayments(): void {
    this.loading = true;
    this.http.GetData('Payment/GetPaymentDetails').subscribe({
      next: (res: any) => {
        if (res.success) {
          const paymentData = res.Data || res.data || [];
          // Map data to include both new and legacy field names
          this.payments = paymentData.map((p: any) => ({
            ...p,
            Id: p.Id || p.ID || p.id, // Ensure Id is captured regardless of casing
            // Map new fields to legacy fields for backward compatibility
            teamName: p.TeamName || p.teamName,
            tournamentName: p.TournamentName || p.tournamentName,
            amount: p.RegistrationFee || p.amount,
            paymentStatus: p.PaymentStatus || p.paymentStatus,
            paymentDate: p.PaymentDate || p.paymentDate,
            phone: p.TeamOwnerPhoneNumber || p.phone,
            teamOwnerName: p.TeamOwnerName || p.teamOwnerName
          }));
          this.filteredPayments = this.payments;
          this.loading = false;
        } else {
          this.errorMessage = res.Message || 'Failed to load payments';
          this.loading = false;
        }
      },
      error: (err) => {
        console.error('Error loading payments:', err);
        this.errorMessage = err.error?.Message || err.error?.message || 'Error loading payment data';
        this.loading = false;
      }
    });
  }

  filterByStatus(status: string): void {
    this.selectedStatus = status;
    if (status === 'all') {
      this.filteredPayments = this.payments;
    } else {
      this.filteredPayments = this.payments.filter(p => {
        const paymentStatus = (p.PaymentStatus || p.paymentStatus || '').toLowerCase();
        // Map status values
        if (status === 'completed') {
          return paymentStatus === 'completed' || paymentStatus === 'success' || paymentStatus === 'confirmed' || paymentStatus === 'paid';
        } else if (status === 'pending') {
          return paymentStatus === 'pending' || paymentStatus === 'initiated';
        } else if (status === 'failed') {
          return paymentStatus === 'failed';
        }
        return paymentStatus === status.toLowerCase();
      });
    }
  }

  getStatusClass(status?: string): string {
    if (!status) return 'badge bg-secondary';

    const statusLower = status.toLowerCase();
    switch (statusLower) {
      case 'completed':
      case 'success':
      case 'confirmed':
      case 'paid':
        return 'badge bg-success';
      case 'pending':
      case 'initiated':
        return 'badge bg-warning text-dark';
      case 'failed':
        return 'badge bg-danger';
      default:
        return 'badge bg-secondary';
    }
  }

  formatDate(dateStr?: string): string {
    if (!dateStr) return 'N/A';
    try {
      const date = new Date(dateStr);
      return date.toLocaleDateString() + ' ' + date.toLocaleTimeString();
    } catch {
      return dateStr;
    }
  }

  formatAmount(amount?: number): string {
    if (!amount) return '0.00';
    return amount.toFixed(2);
  }

 

  getCompletedCount(): number {
    return this.payments.filter(p => {
      const status = (p.PaymentStatus || p.paymentStatus || '').toLowerCase();
      return status === 'completed' || status === 'success' || status === 'confirmed' || status === 'paid';
    }).length;
  }

  getPendingCount(): number {
    return this.payments.filter(p => {
      const status = (p.PaymentStatus || p.paymentStatus || '').toLowerCase();
      return status === 'pending' || status === 'initiated';
    }).length;
  }

  getFailedCount(): number {
    return this.payments.filter(p => {
      const status = (p.PaymentStatus || p.paymentStatus || '').toLowerCase();
      return status === 'failed';
    }).length;
  }

  markAsPaid(id?: number): void {
    if (!id || !confirm('Are you sure you want to mark this payment as PAID? This will allow the team to participate in the tournament.')) return;

    // We use query parameter pattern which is standard in this project's controllers
    this.http.PostData('Payment/UpdateToPaid?id=' + id, {}).subscribe({
      next: (res: any) => {
        if (res.success) {
          alert('Payment reconciled successfully!');
          this.loadPayments();
        } else {
          alert('Failed: ' + (res.message || res.Message || 'Unknown error from server'));
        }
      },
      error: (err) => {
        console.error('Reconciliation error:', err);
        const status = err.status || 'Unknown';
        const detail = err.error?.message || err.error?.Message || err.message || 'No detail available';
        alert(`Server Error (${status}): ${detail}`);
      }
    });
  }
}

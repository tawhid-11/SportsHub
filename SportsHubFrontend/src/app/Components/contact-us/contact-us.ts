import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-contact-us',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './contact-us.html',
  styleUrls: ['./contact-us.css']
})
export class ContactUsComponent {
  contactInfo = {
    email: 'tshakib25@gmail.com',
    phone: '+880 1610 595016',
    address: 'IUBAT, Uttara, Dhaka, Bangladesh',
    website: 'www.sportshub.com'
  };

  formData = {
    name: '',
    email: '',
    subject: '',
    message: ''
  };

  onSubmit() {
    const emailPattern = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$/;

    if (!this.formData.name || this.formData.name.trim().length < 2) {
      alert('Please enter your full name.');
      return;
    }
    if (!emailPattern.test(this.formData.email)) {
      alert('Please enter a valid email address.');
      return;
    }
    if (!this.formData.message || this.formData.message.trim().length < 10) {
      alert('Message must be at least 10 characters long.');
      return;
    }

    console.log('Form Submitted:', this.formData);
    alert('Thank you for contacting us! We will get back to you soon.');
    this.formData = { name: '', email: '', subject: '', message: '' };
  }
}

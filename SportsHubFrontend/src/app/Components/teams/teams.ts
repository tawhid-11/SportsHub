import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Httpclientservice } from '../../Service/httpclientservice';
import { ActivatedRoute, Router } from '@angular/router';
@Component({
  selector: 'app-teams',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './teams.html',
  styleUrl: './teams.css',
})
export class Teams implements OnInit {

  teamForm!: FormGroup;
  selectedFile: File | null = null;
  id: any = 0;
  constructor(
    private fb: FormBuilder,
    private httpService: Httpclientservice,
    private http: HttpClient,
    private router: Router,
    private route: ActivatedRoute

  ) {
    this.route.queryParams.subscribe(params => {
      debugger;
      const id = params['id'];

      if (id) {
        this.id = id;

      }
    });
  }
  ngOnInit(): void {
    this.teamForm = this.fb.group({
      TeamName: ['', Validators.required],
      UserId: [''],
      ShortName: ['', Validators.required],
      TeamLogo: [''],
      TeamOwnerName: ['', Validators.required],
      TeamOwnerEmail: ['', [Validators.required, Validators.email]],
      TeamOwnerPhoneNumber: ['', [Validators.required, Validators.pattern('^[0-9]{11}$')]],
      CoachName: ['', Validators.required],
      FoundedYear: ['', [Validators.min(1800), Validators.max(new Date().getFullYear())]],
      TotalPlayers: ['', Validators.min(1)],
      IsActive: [true]
    });
  }
  onFileChange(event: any): void {
    if (event.target.files && event.target.files.length > 0) {
      this.selectedFile = event.target.files[0];
    }
  }
  onSubmit(): void {

    if (this.teamForm.invalid) {
      alert('Please fill all required fields correctly.');
      this.teamForm.markAllAsTouched();
      return;
    }
    var data = {
      name: this.teamForm.value.TeamOwnerName,
      email: this.teamForm.value.TeamOwnerEmail,
      phone: this.teamForm.value.TeamOwnerPhoneNumber,
      userType: "TeamOwner",
      password: this.teamForm.value.TeamOwnerEmail
    }
    this.httpService.PostData('UserInfo/Register', data).subscribe(
      (response: any) => {
        debugger;
        console.log('User registration successful:', response);
        const userId = response.data.UserId || response.data.ID || response.data.id;

        const formData = new FormData();
        formData.append('TeamName', this.teamForm.value.TeamName);
        formData.append('UserId', userId.toString());
        formData.append('ShortName', this.teamForm.value.ShortName);
        if (this.selectedFile) {
          formData.append('TeamLogo', this.selectedFile);
        }
        formData.append('TeamOwnerName', this.teamForm.value.TeamOwnerName);
        formData.append('TeamOwnerEmail', this.teamForm.value.TeamOwnerEmail);
        formData.append('TeamOwnerPhoneNumber', this.teamForm.value.TeamOwnerPhoneNumber);
        formData.append('CoachName', this.teamForm.value.CoachName);
        formData.append('FoundedYear', this.teamForm.value.FoundedYear?.toString());
        formData.append('TotalPlayers', this.teamForm.value.TotalPlayers?.toString());
        formData.append('IsActive', this.teamForm.value.IsActive.toString());
        this.httpService.PostData('Teams/teams', formData).subscribe({
          next: (res: any) => {
            debugger;
            if (this.id && this.id > 0) {
              this.httpService.PostData('Teams/TournamentTeamMapping', { TournamentId: this.id, TeamId: res.data.TeamsID, userId: userId }).subscribe({
                next: (res: any) => {
                  if (res.paymentUrl && res.paymentUrl != '') {
                    window.location = res.paymentUrl;
                  } else {
                    const msg = res.message || 'Error initializing payment gateway link';
                    alert('❌ ' + msg);
                  }

                },
                error: (err) => {
                  console.error('❌ Team mapping to tournament failed', err);
                  const errorMsg = err.error?.message || 'Server error occurred during tournament registration';
                  alert('❌ ' + errorMsg);
                }
              });
            } else {
              alert('🎉 Team created successfully!');
              this.router.navigate(['']);
            }

            console.log('✅ Team created successfully', res);
            this.teamForm.reset({ IsActive: true });
            this.selectedFile = null;
          },
          error: (err) => {
            console.error('❌ Team creation failed', err);
            alert('❌ Team creation failed: ' + (err.error?.message || 'Unknown error'));
          }
        });
      },
      (error) => {
        console.error('User registration failed:', error);
      }
    );
  }
}

import { Httpclientservice } from './../../Service/httpclientservice';
import { Router } from '@angular/router';
import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-player-form',
  templateUrl: './playerforms.html',
  styleUrls: ['./playerforms.css'],
  imports: [CommonModule, FormsModule]
})
export class PlayerForm implements OnInit {

  isEditMode = false;
  model: any = {
    PlayerID: 0,
    TeamsID: 0,
    PlayerRoleID: 0,
    FullName: '',
    NickName: '',
    Nationality: '',
    DateOfBirth: '',
    BattingStyle: '',
    BowlingStyle: '',
    PlayerImage: '',
    Description: '',
    Phone: '',
    IsActive: true
  };
  selectedImage: File | null = null;
  teams: any = {};
  playerRoles: any[] = [];
  Description: any[] = [];
  playerId: any = 0;
  constructor(private dataService: Httpclientservice, private cdr: ChangeDetectorRef, private router: Router) { }

  ngOnInit(): void {
    // If edit mode → load player data by ID
    this.loadRoles();
    this.loadDescription();
    this.loadTeam();
    this.router.routerState.root.queryParams.subscribe(params => {
      const id = params['id'];
      if (id) {
        this.isEditMode = true;
        this.playerId = id;
        this.loadPlayerData(id);
      }
    });
  }
  loadPlayerData(id: number) {
    this.dataService.GetData(`Player/GetPlayerById?PlayerID=${id}`).subscribe((res: any) => {
      debugger;
      if (res && res.data) {
        this.model = res.data;
        this.cdr.detectChanges();
      }
    });
  }
  loadRoles() {

    // Load player roles from API
    this.dataService.GetData('PlayerRole').subscribe((res: any) => {
      this.playerRoles = res.data;
      this.cdr.detectChanges();
    });
  }
  loadDescription() {
    // Load player roles from API
    this.dataService.GetData('PlayerRole').subscribe((res: any) => {
      this.Description = res.data;
      this.cdr.detectChanges();
    });
  }
  loadTeam() {
    debugger;
    // Load teams from API
    var user = JSON.parse(localStorage.getItem('userInfo') || '{}');

    this.dataService.GetData(`Teams/GetTeamIdbyUserId?id=${user.ID}`).subscribe((res: any) => {
      debugger;
      this.teams = res.data;
      this.cdr.detectChanges();
    });


  }
  onImageSelect(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.selectedImage = file;        // store File object for FormData
      const reader = new FileReader();
      reader.onload = () => {
        this.model.PlayerImageUrl = reader.result as string; // preview only
      };
      reader.readAsDataURL(file);
    }
  }
  buildFormData(userId?: number): FormData {
    const formData = new FormData();
    debugger;
    formData.append('TeamsID', this.teams.TeamsID.toString());
    formData.append('PlayerRoleID', this.model.PlayerRoleID.toString());
    formData.append('FullName', this.model.FullName.trim());
    formData.append('Nationality', this.model.Nationality.trim());
    formData.append('DateOfBirth', this.model.DateOfBirth);
    formData.append('NickName', this.model.NickName || '');
    formData.append('BattingStyle', this.model.BattingStyle || '');
    formData.append('BowlingStyle', this.model.BowlingStyle || '');
    formData.append('Description', this.model.Description || '');

    // Add userId if provided (for new player registration)
    if (userId) {
      formData.append('UserId', userId.toString());
    }

    // formData.append('IsActive', this.model.IsActive.toString());

    if (this.selectedImage) { //  use File object
      formData.append('PlayerImage', this.selectedImage);
    }
    return formData;
  }
  validatePlayerForm(): boolean {
    if (!this.model.FullName || this.model.FullName.trim().length < 3) {
      alert('Full Name must be at least 3 characters long.');
      return false;
    }

    if (!this.model.PlayerRoleID || this.model.PlayerRoleID == 0) {
      alert('Please select a player role.');
      return false;
    }

    if (!this.model.DateOfBirth) {
      alert('Please enter date of birth.');
      return false;
    }

    const dob = new Date(this.model.DateOfBirth);
    const today = new Date();
    let age = today.getFullYear() - dob.getFullYear();
    const monthDiff = today.getMonth() - dob.getMonth();
    if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < dob.getDate())) {
      age--;
    }

    if (age < 15) {
      alert('Player must be at least 15 years old.');
      return false;
    }

    if (!this.model.Phone || !/^\d{10,14}$/.test(this.model.Phone)) {
      alert('Please enter a valid phone number (10-14 digits).');
      return false;
    }

    if (this.model.BattingStyle === '' || this.model.BowlingStyle === '') {
      alert('Please select both batting and bowling styles.');
      return false;
    }

    return true;
  }

  onSubmit(form: NgForm): void {
    if (form.invalid) return;

    if (!this.validatePlayerForm()) {
      return;
    }

    this.isEditMode = this.model.PlayerID != 0;

    if (this.isEditMode) {
      //  UPDATE
      this.dataService.PutData('Player/UpdatePlayer', this.buildFormData(), this.model.PlayerID).subscribe({
        next: () => {
          alert('Player updated successfully!');
          this.router.navigate(['/teamownerlayout/player']);
        },
        error: (err: any) => {
          alert('Update failed: ' + (err.error?.message || 'Server error'));
        }
      });
    } else {
      // CREATE - First register user, then create player
      // Step 1: Register user with UserType = "Player"
      const userData = {
        name: this.model.FullName.trim(),
        email: `${this.model.Phone}@player.sportshub.local`, // Generate email from phone
        phone: this.model.Phone.trim(),
        userType: 'Player',
        password: this.model.Phone.trim() // Use phone as password
      };

      this.dataService.PostData('UserInfo/Register', userData).subscribe({
        next: (userResponse: any) => {
          if (userResponse && userResponse.success && userResponse.data) {
            // Get userId from registration response
            const userId = userResponse.data.UserId || userResponse.data.ID || userResponse.data.id;

            if (!userId) {
              alert('User registration failed: User ID not received');
              return;
            }

            // Step 2: Create player with userId
            this.dataService.PostData('Player/Player', this.buildFormData(userId)).subscribe({
              next: (playerResponse: any) => {
                alert('Player created successfully!');
                this.router.navigate(['/teamownerlayout/player']);
              },
              error: (err: any) => {
                console.error('Error creating player:', err);
                alert('Player creation failed: ' + (err.error?.message || err.message || 'Unknown error'));
              }
            });
          } else {
            alert('User registration failed: ' + (userResponse?.message || 'Unknown error'));
          }
        },
        error: (err: any) => {
          console.error('Error registering user:', err);
          alert('User registration failed: ' + (err.error?.message || err.error?.Message || err.message || 'Unknown error'));
        }
      });
    }
  }
  onCancel(): void {
    // form.resetForm();
    this.router.navigate(['teamownerlayout/player']);
  }
}

import { ChangeDetectorRef, Component } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Httpclientservice } from '../../Service/httpclientservice';

@Component({
  selector: 'app-tournament-form',
  standalone: true,
  imports: [CommonModule, FormsModule,],
  styleUrl: './listof-tournament-forms.css',
  templateUrl: './listof-tournament-forms.html',
  providers: [DatePipe]
})

export class ListofTournamentForms {
  tournamentTypes: any[] = [];
  model: any = {};
  id: any = 0;
  constructor(private http: Httpclientservice, private router: Router, private route: ActivatedRoute, private cdr: ChangeDetectorRef, private datePipe: DatePipe) {
    this.getData();
    this.route.queryParams.subscribe(params => {
      debugger;
      const id = params['id'];

      if (id) {
        this.id = id;
        // Fetch existing data for editing
        this.http.GetData(`Tournaments/GetTournamentsById?TournamentId=${id}`).subscribe((data: any) => {
          debugger;
          if (data && data.data) {
            debugger;
            this.model.TournamentName = data.data.TournamentName;
            this.model.Prize = data.data.Prize;
            this.model.tournamentTypeID = data.data.TournamentTypeID;
            this.model.StartDate = this.datePipe.transform(data.data.StartDate, 'yyyy-MM-dd');
            this.model.EndDate = this.datePipe.transform(data.data.EndDate, 'yyyy-MM-dd');
            this.model.RegistrationDeadLine = this.datePipe.transform(data.data.RegistrationDeadline, 'yyyy-MM-dd');
            this.model.Location = data.data.Location;
            this.model.TotalPlayer = data.data.TotalPlayer;
            this.model.MatchPlayer = data.data.MatchPlayer;
            this.model.ExtraPlayer = data.data.ExtraPlayer;
            this.model.Status = data.data.Status;
            this.model.RegistrationFee = data.data.RegistrationFee;
            this.model.FieldFee = data.data.FieldFee;
            this.model.MaxTeams = data.data.MaxTeams;
            this.model.NumberOfGroups = data.data.NumberOfGroups || 2;
            this.model.TeamsPerGroup = data.data.TeamsPerGroup || 4;
            this.model.ContactNumber = '0' + data.data.ContactNumber;
            this.cdr.detectChanges();
          }
        });
      }
    });
  }

  errorMessage: string = '';

  onSubmit(form: any): void {
    if (form.invalid) return;

    if (!this.validateForm()) {
      return;
    }

    if (this.id && this.id > 0) {
      this.http.PutData('Tournaments/UpdateTournaments', this.model, Number(this.id)).subscribe({
        next: (data: any) => {
          if (data && data.data && data.data[0].Message) {
            alert(data.data[0].Message);
          } else {
            form.resetForm();
            alert('Tournament Updated successfully!');
            this.router.navigate(['/layout/tournaments']);
          }
        },
        error: (err) => {
          console.error('Update error:', err);
          alert('Failed to update tournament.');
        }
      });
    } else {
      this.http.PostData('Tournaments/Tournaments', this.model).subscribe({
        next: (data: any) => {
          if (data && data.data && data.data[0].Message) {
            alert(data.data[0].Message);
          } else {
            form.resetForm();
            alert('Tournament created successfully!');
            this.router.navigate(['/layout/tournaments']);
          }
        },
        error: (err) => {
          alert(err.error?.message || 'Failed to create tournament');
          console.error('Create error:', err);
        }
      });
    }
  }

  validateForm(): boolean {
    this.errorMessage = '';
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const start = new Date(this.model.StartDate);
    const end = new Date(this.model.EndDate);
    const deadline = this.model.RegistrationDeadLine ? new Date(this.model.RegistrationDeadLine) : null;

    if (start < today) {
      this.errorMessage = 'Start date cannot be in the past.';
      return false;
    }
    if (end < start) {
      this.errorMessage = 'End date cannot be earlier than start date.';
      return false;
    }
    if (deadline && deadline > start) {
      this.errorMessage = 'Registration deadline must be before tournament start date.';
      return false;
    }

    if (this.model.MatchPlayer > this.model.TotalPlayer) {
      this.errorMessage = 'Match players per team cannot exceed total permitted players.';
      return false;
    }

    if (this.isGroupStage()) {
      const groups = this.model.NumberOfGroups || 2;
      const perGroup = this.model.TeamsPerGroup || 4;
      if (groups * perGroup > this.model.MaxTeams) {
        this.errorMessage = `Configuration error: ${groups} groups x ${perGroup} teams exceeds Max Teams (${this.model.MaxTeams}).`;
        return false;
      }
    }

    if (this.model.RegistrationFee < 0 || this.model.FieldFee < 0) {
      this.errorMessage = 'Fees cannot be negative.';
      return false;
    }

    if (this.model.ContactNumber && !/^\d{10,13}$/.test(this.model.ContactNumber)) {
      this.errorMessage = 'Contact number must be between 10-13 digits.';
      return false;
    }

    return true;
  }

  onCancel() {
    this.router.navigate(['/layout/tournaments']);
  }

  getData() {
    this.http.GetData('TournamentType').subscribe((data: any) => {
      debugger;
      this.tournamentTypes = data.data;
      this.cdr.detectChanges();
    });
  }

  isGroupStage(): boolean {
    if (!this.model.tournamentTypeID || !this.tournamentTypes) return false;
    const type = this.tournamentTypes.find(t => t.Id == this.model.tournamentTypeID);
    return type && type.Name.toLowerCase().includes('group');
  }
}

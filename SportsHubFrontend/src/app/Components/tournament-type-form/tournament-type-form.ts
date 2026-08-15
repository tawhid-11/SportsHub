import { ChangeDetectorRef, Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Httpclientservice } from '../../Service/httpclientservice';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-tournament-type-form',
  imports: [FormsModule, CommonModule],
  templateUrl: './tournament-type-form.html',
  styleUrl: './tournament-type-form.css',
})
export class TournamentTypeForm {
  model = {
    name: ''
  };
  id: any = 0;
  constructor(private http: Httpclientservice, private router: Router, private route: ActivatedRoute, private cdr: ChangeDetectorRef) {
    this.route.queryParams.subscribe(params => {
      const id = params['id'];


      if (id) {
        this.id = id;
        // Fetch existing data for editing
        this.http.GetData(`TournamentType/GetTournamentTypeById?id=${id}`).subscribe((data: any) => {

          if (data && data.data) {
            this.model.name = data.data.Name;
            this.cdr.detectChanges();
          }
        });
      }
    });
  }

  onSubmit(form: any): void {
    if (form.invalid) return;

    if (!this.model.name || this.model.name.trim().length < 3) {
      alert('Tournament Type name must be at least 3 characters long.');
      return;
    }

    if (this.id && this.id > 0) {
      this.http.PutData('TournamentType/UpdateTournamentType', { id: Number(this.id), name: this.model.name }, Number(this.id)).subscribe({
        next: (data: any) => {
          if (data && data.message) {
            alert(data.message);
          } else {

            form.resetForm();
            alert('Tournament Type Updated successfully!');

          }
          this.router.navigate(['/layout/tournamentType']);
        }
      });

    } else {
      // API payload => { name: "Cricket" }
      this.http.PostData('TournamentType/TournamentType', this.model).subscribe({
        next: (data: any) => {
          if (data && data.message) {
            alert(data.message);
          } else {

            form.resetForm();
            alert('Tournament Type created successfully!');

          }
          this.router.navigate(['/layout/tournamentType']);
        },
        error: (err) => {
          console.error('Error:', err);
        }
      });
    }


  }

  onCancel(form: any): void {
    form.resetForm();
    this.router.navigate(['/layout/tournamentType']);
  }
}

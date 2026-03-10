import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Httpclientservice } from '../../Service/httpclientservice';

@Component({
  selector: 'app-player-role-form',
  imports: [CommonModule, FormsModule],
  templateUrl: './player-role-forms.html',
  styleUrl: './player-role-forms.css'
})
export class PlayerRoleForm implements OnInit {

  id: number = 0;
  isEditMode: boolean = true;

  model: any = {
    playerRoleID: 0,
    roleName: '',
    description: '',
    isActive: true,
    createdAt: new Date()
  };

  constructor(
    private http: Httpclientservice,
    private router: Router,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      const id = Number(params['id']);
      if (id && id > 0) {
        this.id = id;
        this.isEditMode = true;
        this.getById(id);
      } else {
        this.isEditMode = false;
      }
    });
  }

  getById(id: number): void {
    this.http.GetData(`PlayerRole/GetPlayerRoleById?PLayerRoleID=${id}`).subscribe({
      next: (res: any) => {
        if (res && res.data) {
          this.model = {
            playerRoleID: res.data.PlayerRoleID,
            roleName: res.data.RoleName,
            description: res.data.Description,
            isActive: res.data.IsActive,
            createdAt: res.data.CreatedAt
          };
          this.cdr.detectChanges();
        }
      }
    });
  }

  onSubmit(form: NgForm): void {
    if (form.invalid) return;

    if (!this.model.roleName || this.model.roleName.trim().length < 3) {
      alert('Role Name must be at least 3 characters long.');
      return;
    }

    if (this.isEditMode) {
      // UPDATE
      this.http.PutData(
        'PlayerRole/UpdatePlayerRole',
        this.model,
        this.id
      ).subscribe({
        next: () => {
          alert('Player Role updated successfully!');
          this.router.navigate(['/layout/playerRoles']);
        }
      });
    } else {
      // CREATE
      this.http.PostData(
        'PlayerRole/PlayerRole',
        this.model
      ).subscribe({
        next: () => {
          alert('Player Role created successfully!');
          this.router.navigate(['/layout/playerRoles']);
        }
      });
    }
  }

  onCancel(): void {
    this.router.navigate(['/layout/playerRoles']);
  }
}

import { ChangeDetectorRef, Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-player-role-list',
  imports: [CommonModule],
  templateUrl: './player-role-list.html',
  styleUrl: './player-role-list.css',
})
export class PlayerRoleList {
   playerRoles: any[] = [];

  constructor(
    private http: HttpClient, private cdr:ChangeDetectorRef, private router: Router ) {}

  ngOnInit(): void {
    this.getPlayerRoles();
  }

 getPlayerRoles(): void {
  this.http.get<any[]>('https://localhost:7142/api/PlayerRole')
    .subscribe({
      next: (res:any) => {
       debugger;
        this.playerRoles = res.data || [];
        this.cdr.detectChanges(); 
      },
      error: err => console.error(err)
    });
}

  onAddNew(): void {
    this.router.navigate(['/layout/playerRoleForms']);
  }

  onEdit(item: any): void {
     this.router.navigate([`/layout/playerRoleForms`], { queryParams: { id: item.PlayerRoleID } });
  }

  onDelete(id: number): void {
    if (!confirm('Are you sure you want to delete this role?')) return;

    this.http.delete(`https://localhost:7142/api/PlayerRole/${id}`)
      .subscribe({
        next: () => {
          alert('Player role deleted successfully');
          this.getPlayerRoles();
        }
      });
  }
}



import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Httpclientservice } from '../../Service/httpclientservice';

@Component({
  selector: 'app-player-role-list',
  imports: [CommonModule],
  templateUrl: './player-role-list.html',
  styleUrl: './player-role-list.css',
})
export class PlayerRoleList implements OnInit {
   playerRoles: any[] = [];

  constructor(
    private http: Httpclientservice, private cdr:ChangeDetectorRef, private router: Router ) {}

  ngOnInit(): void {
    this.getPlayerRoles();
  }

 getPlayerRoles(): void {
  this.http.GetData('PlayerRole')
    .subscribe({
      next: (res:any) => {
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

    this.http.DeleteData(`PlayerRole`, id)
      .subscribe({
        next: () => {
          alert('Player role deleted successfully');
          this.getPlayerRoles();
        }
      });
  }
}



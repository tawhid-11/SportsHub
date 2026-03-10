import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Httpclientservice } from '../../Service/httpclientservice';
import { Router } from '@angular/router';

@Component({
  selector: 'app-listof-player',
  imports: [CommonModule],
  templateUrl: './listof-player.html',
  styleUrl: './listof-player.css',
})
export class ListofPlayer implements OnInit{
   players: any[] = [];
   constructor( private http: Httpclientservice, private router: Router, private cdr: ChangeDetectorRef) {
   }

  ngOnInit(): void {
    this.getData();
  }
  getData() {
    var user =JSON.parse(localStorage.getItem('userInfo') || '{}');


    this.http.GetData(`Player/GetPlayerByTeamOwnerId?id=${user.ID}`).subscribe((res: any) => {
      debugger;
      if (res && res.success) {
        this.players = res.data.map((player: any) => {
          return {
          ...player,
          PlayerImage: 'https://localhost:7142/' + player.PlayerImage
          };
          });
        this.cdr.detectChanges();
      }
    });
  }

  // 🔹 New Player
  onNew(): void {
    // navigate or open modal
   this.router.navigate(['teamownerlayout/playerforms']);
  }

  // 🔹 Edit Player
  onEdit(id: number) {
    debugger;
    this.router.navigate(['teamownerlayout/playerforms'], {queryParams: { id }});
  }

  // 🔹 Delete Player
  onDelete(id: number) {
    debugger;
    if (!confirm('Are you sure you want to delete this player?')) {
      return;
    }

    this.http.DeleteData(`Player`, id).subscribe(() => {
      this.getData();
   });
  }

}

import { ChangeDetectorRef, Component } from '@angular/core';
import { Httpclientservice } from '../../Service/httpclientservice';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-admin-players-list',
  imports: [CommonModule],
  templateUrl: './admin-players-list.html',
  styleUrl: './admin-players-list.css',
})
export class AdminPlayersList {
  players: any[] = [];

  constructor(
    private http: Httpclientservice,
    private cdr: ChangeDetectorRef,
    private router: Router
  ) {
    this.getData();
  }

  getData() {
    this.http.GetData('Player/GetAllWithTeamName').subscribe((res: any) => {
      if (res && res.success) {
        this.players = res.data.map((player: any) => {
          return {
            ...player,
            PlayerImage: player.PlayerImage ? 'https://localhost:7142' + player.PlayerImage : null
          };
        });
        this.cdr.detectChanges();
      }
    });
  }
}

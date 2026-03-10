import { Teams } from './../teams/teams';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Httpclientservice } from '../../Service/httpclientservice';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-view-player',
  imports: [CommonModule],
  templateUrl: './view-player.html',
  styleUrl: './view-player.css',
})
export class ViewPlayer implements OnInit {

  players: any[] = [];
  teamId!: number;

  constructor(private http: Httpclientservice, private route: ActivatedRoute, private router: Router, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      debugger;
      this.teamId = Number(params.get('id'));
   
    if (this.teamId) {
      this.loadPlayers(this.teamId);
    }
    });
  }

  loadPlayers(teamId: number) {
    this.http.GetData(
      `Teams/GetPlayerbyTeamId?id=${ teamId}`
    ).subscribe({
      next: (res: any) => {
        this.players = res.data;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Failed to load players', err);
      }
    });
  }

  goBack() {
    this.router.navigate(['registeredteams/:id'], { relativeTo: this.route });
  }
}

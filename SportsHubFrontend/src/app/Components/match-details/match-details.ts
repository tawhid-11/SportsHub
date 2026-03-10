import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Httpclientservice } from '../../Service/httpclientservice';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-match-details',
  imports: [CommonModule],
  templateUrl: './match-details.html',
  styleUrl: './match-details.css',
})
export class MatchDetails  implements OnInit {
  matchId!: number;
  matchDetails!: any;

  teamAPlayers: any[] = [];
  teamBPlayers: any[] = [];

  constructor(private route: ActivatedRoute, private http: Httpclientservice,private cdr:ChangeDetectorRef) {}

  ngOnInit() {
    debugger
    // Get the schedule ID from route
    this.matchId = Number(this.route.snapshot.paramMap.get('id'));
    this.matchDetails = {
      TeamAName: this.route.snapshot.queryParamMap.get('teamAName'),
      TeamBName: this.route.snapshot.queryParamMap.get('teamBName')
    };
    this.getMatchDetails();
  }

  getMatchDetails() {
    this.http.GetData(`TeamSchedule/GetPlayerListByTeamScheduleId?teamScheduleId=${this.matchId}`)
      .subscribe((players:any) => {
        debugger
        // Separate players by TeamSide
        this.teamAPlayers = players.data.filter((p:any) => p.TeamSide === 'TeamA');
        this.teamBPlayers = players.data.filter((p:any) => p.TeamSide === 'TeamB');
        this.cdr.detectChanges();
      });
  }
}
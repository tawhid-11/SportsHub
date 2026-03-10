import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { SignalrService } from '../../Service/SignalrService';
import { Httpclientservice } from '../../Service/httpclientservice';

@Component({
    selector: 'app-live-matches',
    standalone: true,
    imports: [CommonModule, RouterLink],
    templateUrl: './live-matches.html',
    styleUrls: ['./live-matches.css']
})
export class LiveMatchesComponent implements OnInit {
    liveMatches: any[] = [];
    loading: boolean = true;

    constructor(
        private http: Httpclientservice,
        private signalR: SignalrService,
        private cdr: ChangeDetectorRef,
        private router: Router
    ) { }

    ngOnInit(): void {
        this.fetchMatches();
        this.signalR.startConnection();
        this.signalR.liveMatch$.subscribe(data => {
            if (!data) return;
            this.updateMatchInList(data);
        });
    }

    fetchMatches() {
        this.loading = true;
        this.http.GetData('CricketMatch/GetAllLiveMatch').subscribe({
            next: (res: any) => {
                this.liveMatches = res.data.map((m: any) => ({
                    ...m,
                    cricketMatchID: m.CricketMatchID ?? m.cricketMatchID,
                    matchStatus: m.MatchStatus ?? m.matchStatus
                }));
                this.loading = false;
                this.cdr.detectChanges();
            },
            error: (err) => {
                console.error(err);
                this.loading = false;
            }
        });
    }

    updateMatchInList(data: any) {
        const matchId = data.CricketMatchID ?? data.cricketMatchID;
        const index = this.liveMatches.findIndex(m => m.cricketMatchID === matchId);

        if (index !== -1) {
            this.liveMatches[index] = {
                ...this.liveMatches[index],
                matchStatus: data.MatchStatus ?? data.matchStatus,
                totalRun: data.TotalRuns ?? data.totalRuns ?? data.totalRun,
                wicket: data.Wickets ?? data.wickets ?? data.wicket,
                overs: data.Overs ?? data.overs,
                battingTeamName: data.BattingTeamName ?? data.battingTeamName,
                winnerMessage: data.WinnerMessage ?? data.winnerMessage
            };
        } else {
            // If it's a new live match not in list, we could re-fetch or push
            this.fetchMatches();
        }
        this.cdr.detectChanges();
    }

    viewDetail(id: number) {
        const match = this.liveMatches.find(m => m.cricketMatchID === id);
        if (match && match.matchStatus === 'Finished') {
            this.router.navigate(['/match-summary', id]);
        } else {
            this.router.navigate(['/view-live-score', id]);
        }
    }
}

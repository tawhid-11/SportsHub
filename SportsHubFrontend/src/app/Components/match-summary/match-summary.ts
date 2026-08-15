import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Httpclientservice } from '../../Service/httpclientservice';

@Component({
  selector: 'app-match-summary',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './match-summary.html',
  styleUrl: './match-summary.css'
})
export class MatchSummary implements OnInit {
  matchId!: number;
  summary: any;

  constructor(
    private route: ActivatedRoute,
    private http: Httpclientservice,
    private router: Router,
    private cdr:ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.matchId = Number(params.get('id'));
      this.getSummary();
    });
  }

  getSummary() {
    this.http.GetData(`LiveMatch/GetMatchSummary?matchId=${this.matchId}`).subscribe((res: any) => {
      this.summary = res.data;
       this.cdr.detectChanges();
    });
  }

  goBack() {
    this.router.navigate(['/']);
  }
}

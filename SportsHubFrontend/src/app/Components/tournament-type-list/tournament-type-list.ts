import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component } from '@angular/core';
import { Httpclientservice } from '../../Service/httpclientservice';
import { Router } from '@angular/router';

@Component({
  selector: 'app-tournament-type-list',
  imports: [CommonModule],
  templateUrl: './tournament-type-list.html',
  styleUrl: './tournament-type-list.css',
})
export class TournamentTypeList {
tournamentTypes:any[]=[];
constructor(private http:Httpclientservice,private cdr:ChangeDetectorRef,private router:Router) {
  this.getData();
}
getData(){
  this.http.GetData('TournamentType').subscribe((data:any)=>{
    debugger;
    this.tournamentTypes=data.data;
    this.cdr.detectChanges();
  });
}
onNew(){
this.router.navigate(['/layout/tournamentType-forms']);
}
onDelete(id: number) {
  if (!confirm('Are you sure you want to delete this Tournament Type?')) {
    return;
  }
  this.http.DeleteData(`TournamentType`,id).subscribe({
    next: (data:any) => {
   
      this.getData();
    }
  });
}

onEdit(id: number) {
  this.router.navigate([`/layout/tournamentType-forms`], { queryParams: { id } });
}
}
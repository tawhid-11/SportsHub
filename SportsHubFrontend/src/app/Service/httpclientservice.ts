import { Injectable } from '@angular/core';
import { Environment } from '../Environment/environment.model';
import { HttpClient, HttpHeaders } from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export class Httpclientservice {
  apiUrl:string ='';
  baseHost:string ='';
  constructor(private http:HttpClient) {
var apis = new Environment();
this.apiUrl = apis.apiUrl;
this.baseHost = apis.baseHost;
  }
  private getHeaders(): { headers: HttpHeaders } {
    const token = localStorage.getItem('jwtToken');
    let headers = new HttpHeaders();
    if (token) {
      headers = headers.set('Authorization', `Bearer ${token}`);
    }
    return { headers };
  }

  public GetData(url:string){
    return this.http.get(this.apiUrl + url, this.getHeaders());
  }
  public PostData(url:string,data:any){
    return this.http.post(this.apiUrl + url,data, this.getHeaders());
  }
  public DeleteData(url:string,id:number){
    return this.http.delete(this.apiUrl + url+`/${id}`, this.getHeaders());
  }
  public PutData(url:string,data:any,id:number=0){
    const finalUrl = id !== 0
      ? `${this.apiUrl}${url}/${id}`
      : `${this.apiUrl}${url}`;

    return this.http.put(finalUrl, data, this.getHeaders());
  }
}

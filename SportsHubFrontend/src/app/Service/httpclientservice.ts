import { Injectable } from '@angular/core';
import { Environment } from '../Environment/environment.model';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export class Httpclientservice {
  apiUrl:string ='';
  constructor(private http:HttpClient) {
var apis = new Environment();
this.apiUrl = apis.apiUrl;
  }
public GetData(url:string){
    return this.http.get(this.apiUrl + url);
  }
public PostData(url:string,data:any){
    return this.http.post(this.apiUrl + url,data);
  }
  public DeleteData(url:string,id:number){
    return this.http.delete(this.apiUrl + url+`/${id}`);
  }
  public PutData(url:string,data:any,id:number=0){
  const finalUrl = id !== 0
    ? `${this.apiUrl}${url}/${id}`
    : `${this.apiUrl}${url}`;

  return this.http.put(finalUrl, data);
}

  
}

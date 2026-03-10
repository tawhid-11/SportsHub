import { StartMatch } from './../Components/start-match/start-match';
import { signal } from '@angular/core';
// src/app/services/signalr.service.ts
import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject, Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SignalrService {
  signalRurl = 'https://localhost:7142/hubs';
  private hubConnection!: signalR.HubConnection;
  private liveMatchSubject = new Subject<any>();
  liveMatch$ = this.liveMatchSubject.asObservable();






  constructor() {

  }

  // Initialize the connection
  public startConnection(): void {

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.signalRurl)
      .withAutomaticReconnect()
      .build();
    this.hubConnection.start()
      .then(() => console.log('SignalR Connected'))
      .catch(err => console.error('Error connecting to SignalR: ', err));

    this.registerOnServerEvents();
  }

  // Listen to server events
  private registerOnServerEvents(): void {
    this.hubConnection.on('ReceiveLiveMatch', (data: any) => {
      
      this.liveMatchSubject.next(data);
    });

    this.hubConnection.on('UpdateLiveScore', (data: any) => {
      debugger;
      this.liveMatchSubject.next(data);
    });
  }


  public StartMatch(cricketMatchId: any): void {
    this.hubConnection.invoke('StartLiveMatch', cricketMatchId)
      .catch(err => console.error('Error invoking StartMatch: ', err));
  }
}

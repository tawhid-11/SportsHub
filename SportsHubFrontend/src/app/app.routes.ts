import { Routes } from '@angular/router';
import { playerProfile } from './Components/player-profile/player-profile';
import { UserLiveScore } from './Components/user-live-score/user-live-score';
import { MatchSummary } from './Components/match-summary/match-summary';
import { TournamentPoints } from './Components/tournament-points/tournament-points';

import { Login } from './Components/login/login';
import { Registration } from './Components/registration/registration';
import { Layout } from './Components/layout/layout';
import { AdminDashboard } from './Components/admin-dashboard/admin-dashboard';
import { ListofTournamentForms } from './Components/listof-tournament-forms/listof-tournament-forms';
import { ListofTournaments } from './Components/listof-tournaments/listof-tournaments';
import { TournamentTypeForm } from './Components/tournament-type-form/tournament-type-form';
import { TournamentTypeList } from './Components/tournament-type-list/tournament-type-list';
import { PlayerRoleList } from './Components/player-role-list/player-role-list';
import { PlayerRoleForm } from './Components/player-role-forms/player-role-forms';
import { HomePage } from './Components/home-page/home-page';
import { HomeTournament } from './Components/home-tournament/home-tournament';
import { Teams } from './Components/teams/teams';
import { TeamOwnerLayout } from './Components/team-owner-layout/team-owner-layout';
import { ListofPlayer } from './Components/listof-player/listof-player';
import { PlayerForm } from './Components/playerforms/playerforms';
import { PlayingTournament } from './Components/playing-tournament/playing-tournament';
import { RegisterTournament } from './Components/register-tournament/register-tournament';
import { RegisteredTeams } from './Components/registered-teams/registered-teams';
import { ViewPlayer } from './Components/view-player/view-player';
import { PlayerDashboard } from './Components/player-dashboard/player-dashboard';
import { Schedule } from './Components/schedule/schedule';
import { MatchDetails } from './Components/match-details/match-details';
import { TodayMatch } from './Components/today-match/today-match';
import { StartMatch } from './Components/start-match/start-match';
import { PaymentConfirmation } from './payment-confirmation/payment-confirmation';
import { PaymentList } from './Components/payment-list/payment-list';
import { UserInfoList } from './Components/user-info-list/user-info-list';
import { ListofTeams } from './Components/listof-teams/listof-teams';
import { AdminPlayersList } from './Components/admin-players-list/admin-players-list';

import { LiveMatchesComponent } from './Components/live-matches/live-matches';

export const routes: Routes = [
  // home page
  {
    path: '', component: HomePage, children: [
      { path: 'all-tournaments', component: HomeTournament },
      { path: 'teams', component: Teams },
      { path: 'view-live-score/:id', component: UserLiveScore },
      { path: 'live-matches', component: LiveMatchesComponent },
      { path: 'match-summary/:id', component: MatchSummary },
      { path: 'tournament-schedule/:id', component: Schedule },
      { path: 'tournament-points/:id', component: TournamentPoints },
      { path: 'matchdetails/:id', component: MatchDetails }
    ]
  }, { path: 'login', component: Login },
  { path: 'register', component: Registration },
  { path: 'payment-confirmation', component: PaymentConfirmation },

  // Admin part
  {
    path: 'layout', component: Layout, children: [
      { path: '', component: AdminDashboard },
      { path: 'user-Dashboard', component: UserInfoList },
      { path: 'tournaments', component: ListofTournaments },
      { path: 'tournaments-forms', component: ListofTournamentForms },
      { path: 'tournamentType', component: TournamentTypeList },
      { path: 'tournamentType-forms', component: TournamentTypeForm },
      { path: 'playerRoles', component: PlayerRoleList },
      { path: 'playerRoleForms', component: PlayerRoleForm },
      { path: 'schedules/:id', component: Schedule },
      {
        path: 'matchdetails/:id', component: MatchDetails
      }, { path: 'matches', component: TodayMatch },
      { path: 'matchplay/:id', component: StartMatch },
      { path: 'match-summary/:id', component: MatchSummary },
      { path: 'payments', component: PaymentList },
      { path: 'teams', component: ListofTeams },
      { path: 'players', component: AdminPlayersList }]
  },
  // Teamowner
  {
    path: 'teamownerlayout', component: TeamOwnerLayout, children: [
      { path: 'player', component: ListofPlayer },
      { path: 'playerforms', component: PlayerForm },
      { path: 'playingtournament', component: PlayingTournament },
      { path: 'tournamentregistration', component: RegisterTournament },
      { path: 'registeredteams/:id', component: RegisteredTeams },
      { path: 'viewplayers/:id', component: ViewPlayer },
      { path: 'schedules/:id', component: Schedule },
      { path: 'matchdetails/:id', component: MatchDetails }]
  },
  // Player Dashboard
  {
    path: 'PlayerDashboard', component: PlayerDashboard, children: [
      { path: 'profile', component: playerProfile }

    ]
  }
];

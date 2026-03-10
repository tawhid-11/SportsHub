# SportsHub - Complete Project Cycle & Flow Explanation

## 📋 Table of Contents
1. [System Overview](#system-overview)
2. [User Registration & Authentication Flow](#1-user-registration--authentication-flow)
3. [Admin Workflow](#2-admin-workflow)
4. [Team Owner Workflow](#3-team-owner-workflow)
5. [Player Workflow](#4-player-workflow)
6. [Tournament Lifecycle](#5-tournament-lifecycle)
7. [Match Management Flow](#6-match-management-flow)
8. [Live Scoring System](#7-live-scoring-system)
9. [Payment Integration Flow](#8-payment-integration-flow)
10. [Points Table & Rankings](#9-points-table--rankings)

---

## System Overview

**SportsHub** is a comprehensive cricket tournament management system that handles:
- Multi-role user management (Admin, Team Owner, Player)
- Tournament creation and management
- Team registration with payment processing
- Automatic match scheduling
- Real-time live scoring
- Points table calculation with NRR
- Payment gateway integration (bKash)

---

## 1. User Registration & Authentication Flow

### 1.1 Registration Process
```
User Registration Page (/register)
    ↓
User fills form:
  - Full Name
  - Email
  - Phone Number
  - User Type (Admin/TeamOwner/Player)
  - Password
    ↓
POST /api/UserInfo/Register
    ↓
Backend creates UserInfo record
    ↓
Returns UserID
    ↓
Redirects to Login Page
```

**Components:**
- `Registration` component
- `UserInfoController.Register` endpoint
- Stores in `UserInfo` table

### 1.2 Login Process
```
Login Page (/login)
    ↓
User enters Email & Password
    ↓
POST /api/UserInfo/Login
    ↓
Backend validates credentials (SP_UserInfo, Flag=2)
    ↓
Returns user data with UserType
    ↓
Stores in localStorage as 'userInfo'
    ↓
Route based on UserType:
  - Admin → /layout (Admin Dashboard)
  - TeamOwner → /teamownerlayout (Team Owner Dashboard)
  - Player → /PlayerDashboard (Player Dashboard)
```

**Components:**
- `Login` component
- `UserInfoController.Login` endpoint
- Role-based routing

---

## 2. Admin Workflow

### 2.1 Admin Dashboard Overview
**Route:** `/layout`

**Main Features:**
1. **Dashboard Statistics** - View totals (Teams, Tournaments, Players, Matches)
2. **User Management** - View all users (`/layout/user-Dashboard`)
3. **Tournament Type Management** - Create tournament types (Round Robin, Knockout, Group Stage)
4. **Tournament Management** - Create and manage tournaments
5. **Team Management** - View all teams
6. **Player Management** - View all players
7. **Player Roles** - Manage player roles (Batsman, Bowler, etc.)
8. **Match Management** - View today's matches, start matches
9. **Payment Management** - View payment records
10. **Schedule Management** - View tournament schedules

### 2.2 Tournament Creation Flow
```
Admin Dashboard → Tournaments → Create Tournament
    ↓
Tournament Form (/layout/tournaments-forms)
    ↓
Fill tournament details:
  - Tournament Name
  - Tournament Type (from dropdown)
  - Start Date, End Date
  - Registration Deadline
  - Location
  - Max Teams
  - Registration Fee
  - Field Fee
  - Total Players, Match Players, Extra Players
  - Contact Number
  - Status (Draft/Ready/Active/Finished)
    ↓
POST /api/Tournaments/Tournaments
    ↓
Backend creates tournament (SP_Tournaments, Flag=2)
    ↓
Tournament Status = "Draft" or "Ready"
    ↓
Tournament appears in tournament list
```

**Components:**
- `ListofTournamentForms` component
- `TournamentsController.Post` endpoint
- `Tournaments` table

### 2.3 Tournament Status Flow
```
Draft → Ready → Active → Finished

Draft: Tournament created but not published
Ready: Tournament published, accepting registrations
Active: Tournament started, matches in progress
Finished: All matches completed
```

### 2.4 Automatic Scheduling
```
Background Service (TournamentSchedulerService)
    ↓
Runs every 1 hour
    ↓
Checks tournaments with Status = "Ready" (SP_CheckTournamentReadyForSchedule)
    ↓
For each ready tournament:
  - Calls SP_GenerateTeamSchedule
  - Generates matches based on Tournament Type:
    * Round Robin: All teams play each other
    * Knockout: Bracket-style elimination
    * Group Stage: Teams divided into groups
  - Creates TeamSchedule records
  - Updates Tournament Status to "Active"
  - Sets CurrentPhase (e.g., "Round Robin", "Semi-Final")
    ↓
Matches appear in Today's Matches
```

**Components:**
- `TournamentSchedulerService` (Background Service)
- `SP_GenerateTeamSchedule` stored procedure
- `TeamSchedule` table

---

## 3. Team Owner Workflow

### 3.1 Team Owner Dashboard
**Route:** `/teamownerlayout`

**Main Features:**
1. **Player Management** - Add/Edit/Delete players for their team
2. **Tournament Registration** - Register team for tournaments
3. **Playing Tournaments** - View tournaments team is registered in
4. **Registered Teams** - View other teams in a tournament
5. **Schedules** - View match schedules
6. **Match Details** - View match information

### 3.2 Team Creation Flow
```
Team Owner Dashboard → Player → Create Team (if not exists)
    OR
Public Page → Teams → Create Team
    ↓
Team Registration Form
    ↓
Fill team details:
  - Team Name, Short Name
  - Team Logo (image upload)
  - Team Owner Name, Email, Phone
  - Coach Name
  - Founded Year
  - Total Players
    ↓
Step 1: Create UserInfo account (if new)
  POST /api/UserInfo/Register
    ↓
Step 2: Create Team record
  POST /api/Teams/teams
    ↓
Backend:
  - Saves team logo to wwwroot/images
  - Creates team record (SP_Teams, Flag=2)
  - Links UserId to team
  - Sends welcome email to team owner
    ↓
Team created successfully
```

**Components:**
- `Teams` component (public) or `PlayerForm` (team owner)
- `TeamsController.Post` endpoint
- `Teams` table

### 3.3 Player Management Flow
```
Team Owner Dashboard → Player → Add Player
    ↓
Player Form (/teamownerlayout/playerforms)
    ↓
Fill player details:
  - Full Name
  - Nationality
  - Date of Birth 
  - Nick Name
  - Batting Style
  - Bowling Style
  - Player Role (from dropdown)
  - Player Image (optional)
    ↓
If new player:
  Step 1: Create UserInfo account
    POST /api/UserInfo/Register
      ↓
  Step 2: Create Player record
    POST /api/Player/Player
      ↓
If editing:
  PUT /api/Player/UpdatePlayer/{id}
    ↓
Backend:
  - Saves player image to wwwroot/images
  - Creates/updates player record (SP_Players)
  - Links UserId to player
    ↓
Player added/updated successfully
```

**Components:**
- `PlayerForm` component
- `PlayerController.Post` / `Put` endpoints
- `Players` table

### 3.4 Tournament Registration Flow
```
Team Owner Dashboard → Playing Tournament → Register Tournament
    ↓
Register Tournament Page (/teamownerlayout/tournamentregistration)
    ↓
Shows unregistered tournaments (GetUnregisterTournamentByuserId)
    ↓
Team Owner clicks "Register"
    ↓
POST /api/Teams/TournamentTeamMapping
  Body: { TournamentId, TeamId, userId }
    ↓
Backend Process:
  1. Creates TournamentTeamMapping record (SP_TournamentTeamMapping, Flag=2)
     - PaymentStatus = "Pending"
  2. Gets tournament registration fee
  3. Initiates bKash payment
     - Calls BKashService.InitiatePaymentAsync
     - Creates payment request
     - Returns paymentUrl
    ↓
Frontend redirects to bKash payment page
    ↓
User completes payment on bKash
    ↓
bKash redirects to: /payment-confirmation?paymentID=xxx&status=success
    ↓
Payment Confirmation Page
    ↓
GET /api/Teams/Success_URL?paymemtId=xxx
    ↓
Backend:
  1. Confirms payment with bKash (BKashService.ConfirmPaymentAsync)
  2. Updates TournamentTeamMapping:
     - PaymentStatus = "Confirmed"
     - PaymentDate = current date
     - bkashTransactionId = transaction ID
    ↓
Success message displayed
    ↓
Redirects to home page
```

**Components:**
- `RegisterTournament` component
- `PaymentConfirmation` component
- `TeamsController.TournamentTeamMapping` endpoint
- `TeamsController.Success_URL` endpoint
- `BKashService` (payment gateway)
- `TournamentTeamMapping` table

---

## 4. Player Workflow

### 4.1 Player Dashboard
**Route:** `/PlayerDashboard`

**Features:**
1. **Player Profile** - View personal information
   - Shows data from UserInfo table
   - Displays name, email, phone, user type
   - Shows account status

**Components:**
- `PlayerDashboard` component
- `playerProfile` component

---

## 5. Tournament Lifecycle

### 5.1 Complete Tournament Flow
```
┌─────────────────────────────────────────────────────────────┐
│                    TOURNAMENT LIFECYCLE                      │
└─────────────────────────────────────────────────────────────┘

1. ADMIN CREATES TOURNAMENT
   ├─ Tournament Type selected (Round Robin/Knockout/Group)
   ├─ Tournament details configured
   ├─ Status: "Draft" or "Ready"
   └─ Tournament published

2. TEAMS REGISTER
   ├─ Team Owner views available tournaments
   ├─ Selects tournament to register
   ├─ Payment processed via bKash
   ├─ PaymentStatus: "Pending" → "Confirmed"
   └─ Team added to TournamentTeamMapping

3. AUTOMATIC SCHEDULING (Background Service)
   ├─ Service checks tournaments with Status = "Ready"
   ├─ Generates matches based on tournament type:
   │   ├─ Round Robin: All vs All
   │   ├─ Knockout: Bracket elimination
   │   └─ Group Stage: Group matches then knockout
   ├─ Creates TeamSchedule records
   ├─ Sets CurrentPhase
   └─ Status: "Ready" → "Active"

4. MATCH EXECUTION
   ├─ Admin/Team Owner views Today's Matches
   ├─ Selects match to start
   ├─ Live scoring begins
   ├─ Ball-by-ball updates
   └─ Match completion

5. POINTS TABLE UPDATES
   ├─ After each match
   ├─ Calculates wins, losses, points
   ├─ Calculates Net Run Rate (NRR)
   └─ Updates TournamentPointTable

6. TOURNAMENT COMPLETION
   ├─ All matches finished
   ├─ Final rankings determined
   └─ Status: "Active" → "Finished"
```

### 5.2 Tournament Types & Scheduling

#### Round Robin Tournament
```
All teams play against each other once
Example: 4 teams = 6 matches (4C2 = 6)
Phase: "Round Robin"
```

#### Knockout Tournament
```
Elimination bracket style
Phases: Quarter-Final → Semi-Final → Final
Example: 8 teams
  - Quarter-Final: 4 matches
  - Semi-Final: 2 matches (winners)
  - Final: 1 match (winners)
```

#### Group Stage + Knockout
```
Teams divided into groups
Phase 1: "Group Stage" - Teams play within groups
Phase 2: "Semi-Final" - Top teams from each group
Phase 3: "Final" - Winners of semi-finals
```

---

## 6. Match Management Flow

### 6.1 Match Scheduling
```
Automatic Scheduling (Background Service)
    ↓
SP_GenerateTeamSchedule generates matches
    ↓
Creates TeamSchedule records:
  - TournamentID
  - TeamAID
  - TeamBID
  - MatchDate
  - Phase (Round Robin/Semi-Final/etc.)
    ↓
Matches appear in:
  - Today's Matches (/layout/matches)
  - Tournament Schedule (/tournament-schedule/:id)
```

**Components:**
- `TodayMatch` component
- `Schedule` component
- `TeamScheduleController` endpoints

### 6.2 Starting a Match
```
Today's Matches → Select Match → Start Match
    ↓
Start Match Page (/layout/matchplay/:id)
    ↓
Admin/Team Owner:
  1. Selects teams (TeamA and TeamB)
  2. Selects players for each team
  3. Sets batting order
  4. Selects initial bowler
    ↓
Clicks "Start Match"
    ↓
POST /api/LiveMatch/StartMatch
    ↓
Backend:
  1. Creates CricketMatch record
  2. Sets initial players (striker, non-striker, bowler)
  3. Initializes match state
  4. Broadcasts via SignalR: "ReceiveLiveMatch"
    ↓
Match status: "Live"
    ↓
Live scoring interface appears
```

**Components:**
- `StartMatch` component
- `LiveMatchController.StartMatch` endpoint
- `CricketMatch` table
- SignalR Hub

---

## 7. Live Scoring System

### 7.1 Live Match Scoring Flow
```
Match Started → Live Scoring Interface
    ↓
Ball-by-Ball Entry:
  - Select Striker
  - Select Non-Striker
  - Select Bowler
  - Enter Runs (0-6)
  - Select Ball Type (Normal/Wide/NoBall)
  - Mark Wicket (if applicable)
  - Select Wicket Type
    ↓
POST /api/LiveMatch/AddBall
    ↓
Backend Process:
  1. Creates Over record (if new over)
  2. Creates MatchBallByBall record
  3. Updates CricketMatch:
     - Total runs
     - Wickets
     - Current over
     - Current innings
  4. Calculates:
     - Current Run Rate (CRR)
     - Required Run Rate (RRR)
  5. Checks match completion:
     - All out
     - Overs complete
     - Target reached
  6. Broadcasts via SignalR: "UpdateLiveScore"
    ↓
All connected clients receive real-time update
    ↓
Live Score Page updates automatically
```

**Components:**
- `StartMatch` component (scoring interface)
- `UserLiveScore` component (public live score view)
- `LiveMatchController.AddBall` endpoint
- `SignalrService` (frontend)
- `SignalRHub` (backend)
- `Overs` table
- `MatchBallByBall` table

### 7.2 Real-Time Updates
```
SignalR Connection Flow:
    ↓
Frontend connects to SignalR Hub (/hubs)
    ↓
Backend broadcasts on events:
  - "ReceiveLiveMatch" - Initial match data
  - "UpdateLiveScore" - Ball-by-ball updates
    ↓
All connected clients receive updates:
  - Live Score Page
  - Match Summary
  - Tournament Points (if match finished)
    ↓
UI updates automatically without refresh
```

**Technology:**
- SignalR for real-time communication
- WebSocket-based updates
- Automatic reconnection on disconnect

### 7.3 Match Completion
```
Match Finishes (all out / overs complete / target reached)
    ↓
Backend detects completion
    ↓
Updates CricketMatch:
  - MatchStatus = "Finished"
  - Winner determined
  - Final scores recorded
    ↓
Updates TournamentPointTable:
  - Win/Loss points
  - Runs scored/conceded
  - Net Run Rate (NRR)
    ↓
Broadcasts final match stats
    ↓
Match Summary available
```

**Components:**
- `MatchSummary` component
- `TournamentPointsController` (updates points table)
- `TournamentPointTable` table

---

## 8. Payment Integration Flow

### 8.1 bKash Payment Process
```
Team Registration for Tournament
    ↓
POST /api/Teams/TournamentTeamMapping
    ↓
Backend:
  1. Creates mapping with PaymentStatus = "Pending"
  2. Calls BKashService.InitiatePaymentAsync
  3. Creates payment request:
     - Amount (Registration Fee)
     - Currency: BDT
     - Merchant Invoice Number
     - Success URL
    ↓
bKash returns:
  - paymentUrl (redirect URL)
  - paymentId
    ↓
Frontend redirects to bKash payment page
    ↓
User completes payment on bKash
    ↓
bKash redirects back:
  /payment-confirmation?paymentID=xxx&status=success
    ↓
Payment Confirmation Page
    ↓
GET /api/Teams/Success_URL?paymemtId=xxx
    ↓
Backend:
  1. Calls BKashService.ConfirmPaymentAsync
  2. Verifies payment status
  3. Updates TournamentTeamMapping:
     - PaymentStatus = "Confirmed"
     - PaymentDate = current date
     - bkashTransactionId = transaction ID
    ↓
Success message → Redirect to home
```

**Components:**
- `RegisterTournament` component
- `PaymentConfirmation` component
- `BKashService` (payment gateway service)
- `TournamentTeamMapping` table

### 8.2 Payment Status Tracking
```
PaymentStatus Values:
  - "Pending" - Registration initiated, payment not completed
  - "Confirmed" - Payment successful, team registered
```

---

## 9. Points Table & Rankings

### 9.1 Points Calculation Flow
```
Match Completed
    ↓
POST /api/LiveMatch/AddBall (final ball)
    ↓
Backend detects match completion
    ↓
Updates TournamentPointTable:
  For Winning Team:
    - MatchesPlayed += 1
    - Wins += 1
    - Points += 2
    - RunsScored += total runs
    - RunsConceded += opponent runs
    - OversBowled += overs
    - OversFaced += overs
  
  For Losing Team:
    - MatchesPlayed += 1
    - Losses += 1
    - Points += 0
    - RunsScored += total runs
    - RunsConceded += opponent runs
    - OversBowled += overs
    - OversFaced += overs
    ↓
Calculates Net Run Rate (NRR):
  NRR = (RunsScored / OversFaced) - (RunsConceded / OversBowled)
    ↓
Updates TournamentPointTable records
    ↓
Points table sorted by:
  1. Points (descending)
  2. NRR (descending)
```

**Components:**
- `TournamentPoints` component (displays points table)
- `TournamentPointsController` (calculates and updates)
- `TournamentPointTable` table
- `SP_UpdateTournamentPointTable_NRR` stored procedure

### 9.2 Viewing Points Table
```
Public/Admin → Tournament → View Points Table
    ↓
GET /api/TournamentPoints/GetTournamentPoints?tournamentId=xxx
    ↓
Backend returns sorted points table:
  - Team Name
  - Matches Played
  - Wins, Losses
  - Points
  - Net Run Rate (NRR)
  - Rank
    ↓
Displayed in table format
```

**Components:**
- `TournamentPoints` component
- `TournamentPointsController.GetTournamentPoints` endpoint

---

## 10. Public/Home Page Features

### 10.1 Public Access
**Route:** `/` (Home Page)

**Features:**
1. **View All Tournaments** - Browse available tournaments
2. **View Teams** - See all registered teams
3. **Live Scores** - Watch live matches in real-time
4. **Tournament Schedule** - View match schedules
5. **Tournament Points** - View points table and rankings
6. **Match Details** - View match information and squads

**Components:**
- `HomePage` component
- `HomeTournament` component
- `UserLiveScore` component
- `Schedule` component
- `TournamentPoints` component
- `MatchDetails` component

---

## 11. Data Flow Architecture

### 11.1 Frontend → Backend Communication
```
Angular Components
    ↓
Httpclientservice (HTTP Service)
    ↓
REST API Endpoints (/api/[controller])
    ↓
ASP.NET Core Controllers
    ↓
Dapper ORM
    ↓
SQL Server Database
    ↓
Stored Procedures (SP_*)
    ↓
Returns Data
    ↓
JSON Response
    ↓
Angular Components
```

### 11.2 Real-Time Communication
```
Angular Components
    ↓
SignalrService (SignalR Client)
    ↓
WebSocket Connection (/hubs)
    ↓
SignalRHub (Backend)
    ↓
Broadcasts to all clients
    ↓
Angular Components receive updates
    ↓
UI updates automatically
```

---

## 12. Database Tables & Relationships

### 12.1 Core Tables
```
UserInfo
  ├─ UserID (PK)
  ├─ Name, Email, Phone
  ├─ UserType (Admin/TeamOwner/Player)
  └─ Password

Teams
  ├─ TeamsID (PK)
  ├─ UserId (FK → UserInfo)
  ├─ TeamName, ShortName
  ├─ TeamLogo
  └─ TeamOwner details

Players
  ├─ PlayerID (PK)
  ├─ TeamsID (FK → Teams)
  ├─ PlayerRoleID (FK → PlayerRole)
  ├─ UserId (FK → UserInfo)
  ├─ FullName, PlayerImage
  └─ Player statistics

Tournaments
  ├─ TournamentID (PK)
  ├─ TournamentTypeID (FK → TournamentType)
  ├─ TournamentName
  ├─ Status (Draft/Ready/Active/Finished)
  ├─ CurrentPhase
  └─ Tournament details

TournamentTeamMapping
  ├─ ID (PK)
  ├─ TournamentId (FK → Tournaments)
  ├─ TeamId (FK → Teams)
  ├─ userId (FK → UserInfo)
  ├─ PaymentStatus
  ├─ PaymentDate
  └─ bKash transaction details

TeamSchedule
  ├─ TeamScheduleID (PK)
  ├─ TournamentID (FK → Tournaments)
  ├─ TeamAID (FK → Teams)
  ├─ TeamBID (FK → Teams)
  ├─ MatchDate
  └─ Phase

CricketMatch
  ├─ CricketMatchID (PK)
  ├─ TeamScheduleID (FK → TeamSchedule)
  ├─ MatchStatus (Live/Finished)
  ├─ Total runs, wickets
  └─ Current players

Overs
  ├─ Id (PK)
  ├─ CricketMatchID (FK → CricketMatch)
  ├─ BowlerId
  ├─ Innings
  └─ OverNumber

MatchBallByBall
  ├─ BallID (PK)
  ├─ OverId (FK → Overs)
  ├─ StrikerPlayerID, NonStrikerPlayerID
  ├─ BowlerPlayerID
  ├─ Run, IsWicket
  └─ Ball details

TournamentPointTable
  ├─ ID (PK)
  ├─ TournamentID (FK → Tournaments)
  ├─ TeamID (FK → Teams)
  ├─ MatchesPlayed, Wins, Losses
  ├─ Points
  ├─ RunsScored, RunsConceded
  └─ NetRunRate (NRR)
```

---

## 13. Key Features Summary

### 13.1 Admin Features
- ✅ Create and manage tournaments
- ✅ Manage tournament types
- ✅ View all teams and players
- ✅ Manage player roles
- ✅ Start and manage matches
- ✅ View payment records
- ✅ View dashboard statistics

### 13.2 Team Owner Features
- ✅ Create and manage team
- ✅ Add/edit/delete players
- ✅ Register for tournaments
- ✅ View registered tournaments
- ✅ View match schedules
- ✅ View match details

### 13.3 Player Features
- ✅ View personal profile
- ✅ View account information

### 13.4 Public Features
- ✅ Browse tournaments
- ✅ View teams
- ✅ Watch live scores (real-time)
- ✅ View tournament schedules
- ✅ View points table
- ✅ View match details

---

## 14. Technology Stack Summary

### Frontend
- **Framework:** Angular 21.0.0
- **UI Library:** Bootstrap 5.3.8
- **Real-Time:** SignalR (@microsoft/signalr)
- **HTTP Client:** Angular HttpClient
- **Routing:** Angular Router

### Backend
- **Framework:** ASP.NET Core 8.0
- **ORM:** Dapper 2.1.66
- **Database:** SQL Server (LocalDB)
- **Real-Time:** SignalR 1.2.8
- **Payment:** bKash API integration
- **Email:** SMTP (Gmail)

### Database
- **RDBMS:** SQL Server
- **Data Access:** Stored Procedures (SP_*)
- **Relationships:** Foreign keys between tables

---

## 15. Complete User Journey Example

### Example: Team Owner Participating in Tournament

```
1. REGISTRATION
   User → Register → Select "TeamOwner" → Account Created

2. LOGIN
   User → Login → Redirected to Team Owner Dashboard

3. CREATE TEAM
   Team Owner → Player → Create Team
   → Team created with logo and details

4. ADD PLAYERS
   Team Owner → Player → Add Player
   → Multiple players added to team

5. REGISTER FOR TOURNAMENT
   Team Owner → Playing Tournament → Register Tournament
   → Select tournament → Payment via bKash → Registration confirmed

6. WAIT FOR SCHEDULING
   Background service automatically generates matches
   → Matches appear in schedule

7. VIEW SCHEDULE
   Team Owner → Schedules → View match schedule
   → See when team plays

8. MATCH DAY
   Admin starts match → Live scoring begins
   → Team Owner can view live score in real-time

9. MATCH COMPLETION
   Match finishes → Points table updates
   → Team Owner views updated rankings

10. TOURNAMENT COMPLETION
    All matches finished → Final rankings
    → Tournament status: "Finished"
```

---

## 16. Background Services

### TournamentSchedulerService
- **Purpose:** Automatic match scheduling
- **Frequency:** Runs every 1 hour
- **Process:**
  1. Checks tournaments ready for scheduling
  2. Generates matches based on tournament type
  3. Creates TeamSchedule records
  4. Updates tournament status and phase

---

## 17. Security & Authentication

### Current Implementation
- **Authentication:** Session-based (localStorage)
- **Authorization:** Role-based routing
- **User Session:** Stored in localStorage as 'userInfo'
- **Password:** Stored in database (should be hashed in production)

### User Types
- **Admin:** Full system access
- **TeamOwner:** Team and tournament management
- **Player:** Profile viewing

---

## 18. API Endpoints Summary

### User Management
- `POST /api/UserInfo/Register` - User registration
- `POST /api/UserInfo/Login` - User login
- `GET /api/UserInfo` - Get all users

### Tournament Management
- `GET /api/Tournaments` - Get all tournaments
- `POST /api/Tournaments/Tournaments` - Create tournament
- `PUT /api/Tournaments/UpdateTournaments` - Update tournament
- `GET /api/Tournaments/GetTournamentsByuserId` - Get user's tournaments

### Team Management
- `GET /api/Teams` - Get all teams
- `POST /api/Teams/teams` - Create team
- `POST /api/Teams/TournamentTeamMapping` - Register for tournament

### Player Management
- `GET /api/Player` - Get all players
- `POST /api/Player/Player` - Create player
- `PUT /api/Player/UpdatePlayer/{id}` - Update player

### Match Management
- `GET /api/TeamSchedule` - Get schedules
- `GET /api/TeamSchedule/GetTodayMatches` - Get today's matches
- `POST /api/LiveMatch/StartMatch` - Start live match
- `POST /api/LiveMatch/AddBall` - Add ball to match

### Points & Rankings
- `GET /api/TournamentPoints/GetTournamentPoints` - Get points table

---

## 19. Real-Time Features

### SignalR Events
- **ReceiveLiveMatch** - Initial match data broadcast
- **UpdateLiveScore** - Real-time score updates
- **StartLiveMatch** - Match start notification

### Live Updates Include
- Current runs and wickets
- Over progress (e.g., "12.3 overs")
- Current Run Rate (CRR)
- Required Run Rate (RRR)
- Batting team players
- Bowling team players
- Recent ball-by-ball commentary

---

## 20. Payment Flow Details

### bKash Integration
1. **Payment Initiation**
   - Amount: Tournament Registration Fee
   - Currency: BDT (Bangladeshi Taka)
   - Invoice: Auto-generated
   - Success URL: `/payment-confirmation`

2. **Payment Processing**
   - User redirected to bKash payment page
   - User completes payment
   - bKash processes transaction

3. **Payment Confirmation**
   - bKash redirects back with payment ID
   - Backend verifies payment with bKash API
   - Updates database with transaction details
   - Team registration confirmed

---

## Summary

Your SportsHub project is a **complete end-to-end cricket tournament management system** with:

✅ **Multi-role user management** (Admin, Team Owner, Player)
✅ **Tournament lifecycle management** (Creation → Registration → Scheduling → Execution → Completion)
✅ **Automatic match scheduling** (Background service)
✅ **Real-time live scoring** (SignalR)
✅ **Payment integration** (bKash)
✅ **Points table with NRR** (Automatic calculation)
✅ **Public viewing** (Live scores, schedules, points)

The system handles the complete flow from user registration to tournament completion with real-time updates and automated processes.

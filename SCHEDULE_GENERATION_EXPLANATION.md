# Schedule Generation - Complete Explanation

## 📋 Overview

This document explains how the automatic schedule generation works in your SportsHub project and how you can edit match dates.

---

## 🔄 How Schedule Generation Works

### Step 1: Background Service Checks for Ready Tournaments

**Service:** `TournamentSchedulerService` (runs every 1 hour)

**Process:**
```
Every 1 hour:
  1. Calls SP_CheckTournamentReadyForSchedule
  2. Finds tournaments that need scheduling:
     - Status = 'Ready' AND CurrentPhase IS NULL (new tournaments)
     - Status = 'Active' AND all matches in current phase are 'Finished' (next phase)
  3. For each tournament found, calls SP_GenerateTeamSchedule
```

**Location:** `SportsHubBackend/BackgroundServices/TournamentSchedulerService.cs`

---

### Step 2: Stored Procedure Generates Matches

**Stored Procedure:** `SP_GenerateTeamSchedule`

**What it does:**
1. Gets tournament information (Type, MaxTeams, CurrentPhase)
2. Checks tournament type (Round Robin/Knockout/Group Stage)
3. Generates matches based on type
4. **Sets MatchDate = GETDATE()** (current date/time)
5. Creates `TeamSchedule` records

**Location:** `SportsHubBackend/SportsHubBackend/SQL/SP_AutoScheduling.sql`

---

## 📅 How MatchDate is Set

### Current Implementation

**In the stored procedure, MatchDate is set using `GETDATE()`:**

```sql
-- Round Robin (Line 76)
INSERT INTO TeamSchedule (TournamentID, TeamAID, TeamBID, MatchDate, Phase)
SELECT @TournamentID, t1.TeamId, t2.TeamId, GETDATE(), 'Round Robin'
...

-- Knockout (Line 101)
INSERT INTO TeamSchedule (TournamentID, TeamAID, TeamBID, MatchDate, Phase)
SELECT @TournamentID, t1.TeamId, t2.TeamId, GETDATE(), @PhaseName
...

-- Group Stage (Line 138)
INSERT INTO TeamSchedule (TournamentID, TeamAID, TeamBID, MatchDate, Phase)
SELECT @TournamentID, t1.TeamId, t2.TeamId, GETDATE(), 'Group Stage'
...
```

**What `GETDATE()` does:**
- Returns the current date and time when the stored procedure runs
- All matches get the **same date** (the date when scheduling happened)
- Format: `YYYY-MM-DD HH:MM:SS` (e.g., `2026-01-23 14:30:00`)

---

## 🎯 Tournament Type Behavior

### 1. Round Robin Tournament

**When:** `CurrentPhase IS NULL` (first time scheduling)

**What happens:**
- Generates all possible match combinations
- Example: 4 teams = 6 matches (Team1 vs Team2, Team1 vs Team3, Team1 vs Team4, Team2 vs Team3, Team2 vs Team4, Team3 vs Team4)
- **All matches get MatchDate = GETDATE()**
- Phase = "Round Robin"
- No phase transitions (single phase tournament)

**SQL Code (Lines 70-83):**
```sql
IF @TypeName LIKE '%Round Robin%'
BEGIN
    IF @CurrentPhase IS NULL
    BEGIN
        INSERT INTO TeamSchedule (TournamentID, TeamAID, TeamBID, MatchDate, Phase)
        SELECT @TournamentID, t1.TeamId, t2.TeamId, GETDATE(), 'Round Robin'
        FROM TournamentTeamMapping t1
        JOIN TournamentTeamMapping t2 ON t1.TournamentId = t2.TournamentId AND t1.TeamId < t2.TeamId
        WHERE t1.TournamentId = @TournamentID 
        AND t1.PaymentStatus = 'Confirmed' 
        AND t2.PaymentStatus = 'Confirmed';
    END
END
```

---

### 2. Knockout Tournament

**When:** `CurrentPhase IS NULL` (first time scheduling)

**What happens:**
- Determines phase based on MaxTeams:
  - If MaxTeams > 4: Phase = "Quarter-Final"
  - If MaxTeams ≤ 4: Phase = "Semi-Final"
- Pairs teams: Team1 vs Team2, Team3 vs Team4, etc.
- **All matches get MatchDate = GETDATE()**
- Phase transitions: Quarter-Final → Semi-Final → Final

**SQL Code (Lines 86-123):**
```sql
ELSE IF @TypeName LIKE '%Knockout%'
BEGIN
    IF @CurrentPhase IS NULL
    BEGIN
        DECLARE @PhaseName NVARCHAR(50) = 'Semi-Final';
        IF @MaxTeams > 4 SET @PhaseName = 'Quarter-Final';
        
        INSERT INTO TeamSchedule (TournamentID, TeamAID, TeamBID, MatchDate, Phase)
        SELECT @TournamentID, t1.TeamId, t2.TeamId, GETDATE(), @PhaseName
        ...
    END
    ELSE IF @CurrentPhase = 'Quarter-Final'
    BEGIN
        -- Generate Semi-Final matches (winners advance)
        INSERT INTO TeamSchedule (TournamentID, TeamAID, TeamBID, MatchDate, Phase)
        SELECT @TournamentID, t1.TeamId, t2.TeamId, GETDATE(), 'Semi-Final'
        ...
    END
    ELSE IF @CurrentPhase = 'Semi-Final'
    BEGIN
        -- Generate Final match
        INSERT INTO TeamSchedule (TournamentID, TeamAID, TeamBID, MatchDate, Phase)
        SELECT @TournamentID, ..., GETDATE(), 'Final'
        ...
    END
END
```

---

### 3. Group Stage + Knockout Tournament

**When:** `CurrentPhase IS NULL` (first time scheduling)

**What happens:**
- Divides teams into groups (Group 1 and Group 2)
- Generates matches within each group
- **All matches get MatchDate = GETDATE()**
- Phase transitions: Group Stage → Semi-Final → Final

**SQL Code (Lines 126-151):**
```sql
ELSE IF @TypeName LIKE '%Group%'
BEGIN
    IF @CurrentPhase IS NULL
    BEGIN
        -- Assign teams to groups
        UPDATE TournamentTeamMapping SET GroupId = ...
        
        -- Generate Group Stage matches
        INSERT INTO TeamSchedule (TournamentID, TeamAID, TeamBID, MatchDate, Phase)
        SELECT @TournamentID, t1.TeamId, t2.TeamId, GETDATE(), 'Group Stage'
        ...
    END
    ELSE IF @CurrentPhase = 'Group Stage'
    BEGIN
        -- Generate Semi-Final matches (top 2 from each group)
        INSERT INTO TeamSchedule (TournamentID, TeamAID, TeamBID, MatchDate, Phase)
        SELECT @TournamentID, ..., GETDATE(), 'Semi-Final'
        ...
    END
    ELSE IF @CurrentPhase = 'Semi-Final'
    BEGIN
        -- Generate Final match
        INSERT INTO TeamSchedule (TournamentID, TeamAID, TeamBID, MatchDate, Phase)
        SELECT @TournamentID, ..., GETDATE(), 'Final'
        ...
    END
END
```

---

## 📝 How to Edit Match Dates

### Option 1: Direct Database Update (Recommended for Quick Edits)

**Step 1:** Connect to your database (SSMS or Azure Data Studio)

**Step 2:** Run SQL queries to update dates

#### Update Single Match Date:
```sql
USE [SportsHubDB];
GO

-- Update a specific match date
UPDATE TeamSchedule
SET MatchDate = '2026-01-25 10:00:00'  -- Your desired date and time
WHERE TeamScheduleID = 1;  -- Replace 1 with your match ID
```

#### Update All Matches for a Tournament:
```sql
USE [SportsHubDB];
GO

-- Update all matches for a specific tournament
UPDATE TeamSchedule
SET MatchDate = DATEADD(DAY, 2, MatchDate)  -- Add 2 days to current date
WHERE TournamentID = 1;  -- Replace 1 with your tournament ID
```

#### Update Matches by Phase:
```sql
USE [SportsHubDB];
GO

-- Update all Quarter-Final matches for a tournament
UPDATE TeamSchedule
SET MatchDate = '2026-01-24 14:00:00'
WHERE TournamentID = 1 
AND Phase = 'Quarter-Final';
```

#### Schedule Matches with Different Dates (Sequential):
```sql
USE [SportsHubDB];
GO

-- Update matches to be scheduled on different days
-- Example: First match on Day 1, second on Day 2, etc.

DECLARE @TournamentID INT = 1;  -- Your tournament ID
DECLARE @StartDate DATETIME = '2026-01-24 10:00:00';  -- First match date
DECLARE @MatchInterval INT = 1;  -- Days between matches

UPDATE ts
SET ts.MatchDate = DATEADD(DAY, (ROW_NUMBER() OVER (ORDER BY ts.TeamScheduleID) - 1) * @MatchInterval, @StartDate)
FROM TeamSchedule ts
WHERE ts.TournamentID = @TournamentID;
```

---

### Option 2: Modify the Stored Procedure

**If you want to change how dates are automatically assigned:**

**Current Code (Line 76, 101, 138):**
```sql
GETDATE()  -- All matches get current date/time
```

**Possible Modifications:**

#### A. Use Tournament Start Date:
```sql
-- Instead of GETDATE(), use tournament's StartDate
SELECT @TournamentID, t1.TeamId, t2.TeamId, 
       (SELECT StartDate FROM Tournaments WHERE TournamentID = @TournamentID), 
       'Round Robin'
```

#### B. Sequential Dates (One Match Per Day):
```sql
-- Add days based on match order
SELECT @TournamentID, t1.TeamId, t2.TeamId, 
       DATEADD(DAY, ROW_NUMBER() OVER (ORDER BY t1.TeamId, t2.TeamId) - 1, 
               (SELECT StartDate FROM Tournaments WHERE TournamentID = @TournamentID)), 
       'Round Robin'
```

#### C. Multiple Matches Per Day:
```sql
-- Schedule matches at different times on same day
-- Morning: 10:00, Afternoon: 14:00, Evening: 18:00
SELECT @TournamentID, t1.TeamId, t2.TeamId, 
       DATEADD(HOUR, 
               (ROW_NUMBER() OVER (ORDER BY t1.TeamId, t2.TeamId) - 1) % 3 * 4 + 10,  -- 10, 14, or 18
               (SELECT StartDate FROM Tournaments WHERE TournamentID = @TournamentID)), 
       'Round Robin'
```

---

### Option 3: Create an Admin Interface (Frontend)

**You could add a feature to edit match dates through the UI:**

1. **Add an endpoint in backend:**
   - `PUT /api/TeamSchedule/UpdateMatchDate/{id}`
   - Accepts: `{ MatchDate: "2026-01-25T10:00:00" }`

2. **Add UI in frontend:**
   - Edit button next to each match in schedule
   - Date picker to select new date
   - Save button to update

**This would require code changes (which you said not to do), so this is just an explanation of possibility.**

---

## 🔍 Understanding the Current Date Assignment

### Problem with Current Implementation

**Issue:** All matches get the **same date** (`GETDATE()`)

**Example:**
- Tournament scheduled on: `2026-01-23 14:30:00`
- All 6 matches get: `2026-01-23 14:30:00`
- **Result:** All matches appear to be on the same day/time

### Why This Happens

The stored procedure uses `GETDATE()` which returns the **exact moment** the procedure runs. Since all INSERT statements execute in the same transaction, they all get the same timestamp.

---

## 💡 Solutions for Better Date Distribution

### Solution 1: Use Tournament StartDate

**Modify the stored procedure to use tournament's StartDate instead of GETDATE():**

```sql
-- Get tournament start date
DECLARE @TournamentStartDate DATETIME;
SELECT @TournamentStartDate = StartDate 
FROM Tournaments 
WHERE TournamentID = @TournamentID;

-- Use it in INSERT
INSERT INTO TeamSchedule (TournamentID, TeamAID, TeamBID, MatchDate, Phase)
SELECT @TournamentID, t1.TeamId, t2.TeamId, @TournamentStartDate, 'Round Robin'
...
```

**Result:** All matches scheduled on tournament start date

---

### Solution 2: Sequential Dates (One Match Per Day)

**Spread matches across multiple days:**

```sql
-- Calculate match number and add days
INSERT INTO TeamSchedule (TournamentID, TeamAID, TeamBID, MatchDate, Phase)
SELECT 
    @TournamentID, 
    t1.TeamId, 
    t2.TeamId, 
    DATEADD(DAY, 
            ROW_NUMBER() OVER (ORDER BY t1.TeamId, t2.TeamId) - 1, 
            (SELECT StartDate FROM Tournaments WHERE TournamentID = @TournamentID)), 
    'Round Robin'
FROM TournamentTeamMapping t1
JOIN TournamentTeamMapping t2 ON t1.TournamentId = t2.TournamentId AND t1.TeamId < t2.TeamId
WHERE t1.TournamentId = @TournamentID;
```

**Result:** 
- Match 1: StartDate + 0 days
- Match 2: StartDate + 1 day
- Match 3: StartDate + 2 days
- etc.

---

### Solution 3: Multiple Matches Per Day (Time Slots)

**Schedule multiple matches on same day at different times:**

```sql
-- Morning (10:00), Afternoon (14:00), Evening (18:00)
INSERT INTO TeamSchedule (TournamentID, TeamAID, TeamBID, MatchDate, Phase)
SELECT 
    @TournamentID, 
    t1.TeamId, 
    t2.TeamId, 
    DATEADD(HOUR, 
            CASE (ROW_NUMBER() OVER (ORDER BY t1.TeamId, t2.TeamId) - 1) % 3
                WHEN 0 THEN 10  -- 10:00 AM
                WHEN 1 THEN 14  -- 2:00 PM
                WHEN 2 THEN 18  -- 6:00 PM
            END,
            DATEADD(DAY, 
                    (ROW_NUMBER() OVER (ORDER BY t1.TeamId, t2.TeamId) - 1) / 3, 
                    (SELECT StartDate FROM Tournaments WHERE TournamentID = @TournamentID))), 
    'Round Robin'
...
```

**Result:**
- Day 1: Match 1 (10:00), Match 2 (14:00), Match 3 (18:00)
- Day 2: Match 4 (10:00), Match 5 (14:00), Match 6 (18:00)
- etc.

---

## 📊 Current Database Structure

### TeamSchedule Table

**Columns:**
- `TeamScheduleID` (Primary Key)
- `TournamentID` (Foreign Key → Tournaments)
- `TeamAID` (Foreign Key → Teams)
- `TeamBID` (Foreign Key → Teams)
- `MatchDate` (DateTime) ← **This is what gets set**
- `Phase` (NVARCHAR) - e.g., "Round Robin", "Quarter-Final", "Semi-Final", "Final"

---

## 🔧 Manual Date Editing Steps

### Step-by-Step: Edit Match Dates via SQL

**1. View Current Schedule:**
```sql
USE [SportsHubDB];
GO

-- See all matches for a tournament
SELECT 
    ts.TeamScheduleID,
    ts.Phase,
    t1.TeamName as TeamA,
    t2.TeamName as TeamB,
    ts.MatchDate,
    t.TournamentName
FROM TeamSchedule ts
INNER JOIN Teams t1 ON ts.TeamAID = t1.TeamsID
INNER JOIN Teams t2 ON ts.TeamBID = t2.TeamsID
INNER JOIN Tournaments t ON ts.TournamentID = t.TournamentID
WHERE ts.TournamentID = 1  -- Replace with your tournament ID
ORDER BY ts.MatchDate, ts.Phase;
```

**2. Update Specific Match:**
```sql
-- Update match ID 5 to be on January 25, 2026 at 2:00 PM
UPDATE TeamSchedule
SET MatchDate = '2026-01-25 14:00:00'
WHERE TeamScheduleID = 5;
```

**3. Update All Matches for Tournament:**
```sql
-- Set all matches to start from tournament's StartDate
UPDATE ts
SET ts.MatchDate = t.StartDate
FROM TeamSchedule ts
INNER JOIN Tournaments t ON ts.TournamentID = t.TournamentID
WHERE ts.TournamentID = 1;  -- Your tournament ID
```

**4. Spread Matches Across Days:**
```sql
-- Schedule matches one per day starting from tournament StartDate
UPDATE ts
SET ts.MatchDate = DATEADD(DAY, 
                           (ROW_NUMBER() OVER (ORDER BY ts.TeamScheduleID) - 1), 
                           (SELECT StartDate FROM Tournaments WHERE TournamentID = ts.TournamentID))
FROM TeamSchedule ts
WHERE ts.TournamentID = 1;  -- Your tournament ID
```

---

## 🎯 Key Points Summary

### How It Currently Works:
1. ✅ Background service runs every 1 hour
2. ✅ Checks for tournaments ready for scheduling
3. ✅ Calls `SP_GenerateTeamSchedule` for each ready tournament
4. ✅ Generates matches based on tournament type
5. ⚠️ **All matches get MatchDate = GETDATE()** (same date/time)

### How to Edit Dates:
1. **Direct SQL Update** - Quick and easy for manual edits
2. **Modify Stored Procedure** - Change automatic date assignment logic
3. **Admin Interface** - Build UI for date editing (requires code changes)

### Current Limitations:
- ❌ All matches get same date (when scheduling happened)
- ❌ No automatic date distribution
- ❌ No consideration of tournament StartDate
- ❌ No time slots for multiple matches per day

---

## 📝 Example: Editing Dates for a Tournament

**Scenario:** You have a tournament with 6 matches, all scheduled on `2026-01-23 14:30:00`, but you want them spread across 3 days.

**Solution:**
```sql
USE [SportsHubDB];
GO

-- Step 1: Check current dates
SELECT TeamScheduleID, MatchDate, Phase
FROM TeamSchedule
WHERE TournamentID = 1
ORDER BY TeamScheduleID;

-- Step 2: Update to spread across 3 days (2 matches per day)
-- Day 1: Match 1 & 2 at 10:00 and 14:00
-- Day 2: Match 3 & 4 at 10:00 and 14:00
-- Day 3: Match 5 & 6 at 10:00 and 14:00

UPDATE ts
SET ts.MatchDate = 
    DATEADD(HOUR, 
            CASE (ROW_NUMBER() OVER (ORDER BY ts.TeamScheduleID) - 1) % 2
                WHEN 0 THEN 10  -- 10:00 AM
                WHEN 1 THEN 14   -- 2:00 PM
            END,
            DATEADD(DAY, 
                    (ROW_NUMBER() OVER (ORDER BY ts.TeamScheduleID) - 1) / 2, 
                    '2026-01-24'))  -- Start date
FROM TeamSchedule ts
WHERE ts.TournamentID = 1;

-- Step 3: Verify updated dates
SELECT TeamScheduleID, MatchDate, Phase
FROM TeamSchedule
WHERE TournamentID = 1
ORDER BY MatchDate;
```

**Result:**
- Match 1: `2026-01-24 10:00:00`
- Match 2: `2026-01-24 14:00:00`
- Match 3: `2026-01-25 10:00:00`
- Match 4: `2026-01-25 14:00:00`
- Match 5: `2026-01-26 10:00:00`
- Match 6: `2026-01-26 14:00:00`

---

## 🔍 Files Involved

### Backend:
- **Stored Procedure:** `SportsHubBackend/SportsHubBackend/SQL/SP_AutoScheduling.sql`
  - `SP_CheckTournamentReadyForSchedule` - Finds tournaments to schedule
  - `SP_GenerateTeamSchedule` - Generates matches (sets MatchDate)

- **Background Service:** `SportsHubBackend/BackgroundServices/TournamentSchedulerService.cs`
  - Runs every 1 hour
  - Calls the stored procedures

### Database Table:
- **TeamSchedule** - Stores match schedule with MatchDate column

---

## ✅ Summary

**Current Behavior:**
- All matches get `MatchDate = GETDATE()` (current date/time when scheduling happens)
- All matches in a phase get the same date

**To Edit Dates:**
1. Use SQL UPDATE queries (quick manual edits)
2. Modify stored procedure to use different date logic (automatic)
3. Build admin interface (requires code changes)

**Recommendation:**
- For immediate needs: Use SQL UPDATE queries
- For long-term: Consider modifying stored procedure to use tournament StartDate and distribute matches across days

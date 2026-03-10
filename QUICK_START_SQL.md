# Quick Start: Run SQL Queries for Tournament Scheduling

## 🚀 Fastest Way (3 Steps)

### Step 1: Open SQL Server Management Studio
1. Open **SSMS** (SQL Server Management Studio)
2. Connect with:
   - **Server:** `(localdb)\MSSQLLocalDB`
   - **Authentication:** Windows Authentication
   - Click **Connect**

### Step 2: Open and Run the Script
1. In SSMS: **File** → **Open** → **File**
2. Navigate to: `E:\D\SportsHub\SportsHubBackend\SportsHubBackend\SQL\SP_AutoScheduling.sql`
3. **IMPORTANT:** At the top, add this line:
   ```sql
   USE [SportsHubDB];
   GO
   ```
4. Click **Execute** (or press `F5`)
5. Wait for: ✅ **"Command(s) completed successfully"**

### Step 3: Test It Works
Run this in a new query window:

```sql
USE [SportsHubDB];
GO

-- Test 1: Check if stored procedure exists
SELECT name FROM sys.procedures 
WHERE name = 'SP_GenerateTeamSchedule';
-- Should return: SP_GenerateTeamSchedule

-- Test 2: Check tournaments ready
EXEC SP_CheckTournamentReadyForSchedule;
-- Should return: List of TournamentIDs (or empty if none ready)

-- Test 3: Manually trigger for tournament ID 1 (replace 1 with your tournament ID)
EXEC SP_GenerateTeamSchedule @TournamentID = 1;
-- Should return: Command completed successfully
```

---

## 📝 Where to Write Test Queries

### In SSMS:
1. Click **New Query** button
2. Make sure **SportsHubDB** is selected in the database dropdown
3. Paste your query
4. Click **Execute** (or press `F5`)

### Example Test Queries:

```sql
USE [SportsHubDB];
GO

-- ============================================
-- COPY AND PASTE THESE ONE BY ONE
-- ============================================

-- 1. See all tournaments
SELECT TournamentID, TournamentName, Status, CurrentPhase
FROM Tournaments;

-- 2. See which tournaments are ready for scheduling
EXEC SP_CheckTournamentReadyForSchedule;

-- 3. See team registrations for a tournament (replace 1 with your tournament ID)
SELECT 
    t.TeamName,
    tm.PaymentStatus,
    tm.PaymentDate
FROM TournamentTeamMapping tm
INNER JOIN Teams t ON tm.TeamId = t.TeamsID
WHERE tm.TournamentId = 1;

-- 4. Manually generate schedule (replace 1 with your tournament ID)
EXEC SP_GenerateTeamSchedule @TournamentID = 1;

-- 5. See generated matches (replace 1 with your tournament ID)
SELECT 
    ts.Phase,
    t1.TeamName as TeamA,
    t2.TeamName as TeamB,
    ts.MatchDate,
    cm.MatchStatus
FROM TeamSchedule ts
LEFT JOIN Teams t1 ON ts.TeamAID = t1.TeamsID
LEFT JOIN Teams t2 ON ts.TeamBID = t2.TeamsID
LEFT JOIN CricketMatch cm ON ts.TeamScheduleID = cm.TeamScheduleID
WHERE ts.TournamentID = 1
ORDER BY ts.Phase, ts.MatchDate;
```

---

## ⚡ Quick Troubleshooting

### Problem: Can't connect to `(localdb)\MSSQLLocalDB`
**Solution:** 
1. Open **Command Prompt** as Administrator
2. Run: `sqllocaldb start MSSQLLocalDB`
3. Try connecting again

### Problem: "Database 'SportsHubDB' does not exist"
**Solution:**
1. Run your backend application first (it creates the database)
2. OR create it manually:
   ```sql
   CREATE DATABASE SportsHubDB;
   ```

### Problem: "Invalid object name"
**Solution:**
1. Make sure you selected `SportsHubDB` in the database dropdown
2. OR add `USE [SportsHubDB];` at the top of your query

---

## 🎯 Your Connection Details (From appsettings.json)

```
Server: (localdb)\MSSQLLocalDB
Database: SportsHubDB
Authentication: Windows Authentication
```

Use these exact values when connecting!

---

## ✅ Success Checklist

After running the script, verify:

- [ ] Script executed without errors
- [ ] Stored procedure `SP_GenerateTeamSchedule` exists
- [ ] Stored procedure `SP_CheckTournamentReadyForSchedule` exists
- [ ] Can run test queries successfully
- [ ] Matches generate when you call `SP_GenerateTeamSchedule`

---

## 📍 File Location

**Main Script:**
```
E:\D\SportsHub\SportsHubBackend\SportsHubBackend\SQL\SP_AutoScheduling.sql
```

**Where to run queries:**
- SQL Server Management Studio (SSMS) - **Recommended**
- Visual Studio SQL Server Object Explorer
- Azure Data Studio
- Command line (sqlcmd)

---

## 💡 Pro Tip

**Create a saved query file:**
1. In SSMS, create a new query
2. Paste all your test queries
3. Save as: `Test_Scheduling.sql`
4. Run it anytime to check the system!

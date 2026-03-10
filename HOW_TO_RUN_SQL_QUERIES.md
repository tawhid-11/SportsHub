# How to Run SQL Queries - Step by Step Guide

## 📋 Overview

This guide explains where and how to execute the SQL queries for fixing the tournament scheduling system.

---

## Method 1: SQL Server Management Studio (SSMS) - Recommended

### Step 1: Open SQL Server Management Studio
1. Open **SQL Server Management Studio (SSMS)**
2. Connect to your database server:
   - **Server name:** `(localdb)\MSSQLLocalDB` (Your project uses LocalDB)
   - **Authentication:** Windows Authentication
   - Click **Connect**
   
   **Note:** If `(localdb)\MSSQLLocalDB` doesn't work, try:
   - `localhost` 
   - `.\SQLEXPRESS`

### Step 2: Open the SQL Script File
1. In SSMS, go to **File** → **Open** → **File**
2. Navigate to: `SportsHubBackend/SportsHubBackend/SQL/SP_AutoScheduling.sql`
3. Click **Open**

### Step 3: Select Your Database
1. In the toolbar, find the database dropdown (usually shows "master")
2. Select your database: **SportsHubDB**
   - Or type: `USE [SportsHubDB];` at the top of the script

### Step 4: Execute the Script
1. Click the **Execute** button (or press `F5`)
2. Wait for the script to complete
3. You should see: **"Command(s) completed successfully"**

### Step 5: Verify the Stored Procedure
Run this query to verify:
```sql
USE [SportsHubDB];
GO

-- Check if stored procedure exists
SELECT 
    name,
    create_date,
    modify_date
FROM sys.procedures
WHERE name = 'SP_GenerateTeamSchedule';
```

---

## Method 2: Visual Studio (SQL Server Object Explorer)

### Step 1: Open Visual Studio
1. Open your **SportsHubBackend** project in Visual Studio

### Step 2: Open SQL Server Object Explorer
1. Go to **View** → **SQL Server Object Explorer**
2. Expand your database connection
3. Expand **SportsHubDB** → **Programmability** → **Stored Procedures**

### Step 3: Create/Update Stored Procedure
1. Right-click on **Stored Procedures** → **Add New Stored Procedure**
2. OR find existing `SP_GenerateTeamSchedule` → Right-click → **Modify**
3. Copy the content from `SP_AutoScheduling.sql` (the `SP_GenerateTeamSchedule` part)
4. Click **Update** or **Save**

---

## Method 3: Azure Data Studio (Cross-platform)

### Step 1: Open Azure Data Studio
1. Open **Azure Data Studio**
2. Connect to your SQL Server instance

### Step 2: Open SQL Script
1. Go to **File** → **Open File**
2. Select `SP_AutoScheduling.sql`

### Step 3: Connect to Database
1. Click **Connect** in the connection panel
2. Select **SportsHubDB** database

### Step 4: Execute
1. Click **Run** button (or press `F5`)

---

## Method 4: Command Line (sqlcmd)

### Step 1: Open Command Prompt or PowerShell
Navigate to your project directory:
```powershell
cd E:\D\SportsHub\SportsHubBackend\SportsHubBackend\SQL
```

### Step 2: Run the Script
```powershell
sqlcmd -S localhost -d SportsHubDB -i SP_AutoScheduling.sql
```

Or for LocalDB:
```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -d SportsHubDB -i SP_AutoScheduling.sql
```

---

## Method 5: Through Your Backend Application (If you have a setup script)

If your backend has a database initialization endpoint, you can use that.

---

## 📝 Testing Queries - Where to Run

### Option A: In SSMS Query Window

1. Open **SSMS**
2. Click **New Query**
3. Select database: **SportsHubDB**
4. Paste and run each query:

```sql
USE [SportsHubDB];
GO

-- 1. Check tournaments ready for scheduling
EXEC SP_CheckTournamentReadyForSchedule;
GO

-- 2. Check tournament status
SELECT TournamentID, TournamentName, Status, CurrentPhase
FROM Tournaments
WHERE Status = 'Ready' OR Status = 'Active';
GO

-- 3. Check team payments
SELECT TournamentId, TeamId, PaymentStatus
FROM TournamentTeamMapping
WHERE TournamentId = 1; -- Replace 1 with your tournament ID
GO

-- 4. Manually trigger scheduling (for testing)
EXEC SP_GenerateTeamSchedule @TournamentID = 1; -- Replace 1 with your tournament ID
GO

-- 5. Check generated matches
SELECT ts.*, t.TournamentName, t.CurrentPhase
FROM TeamSchedule ts
INNER JOIN Tournaments t ON ts.TournamentID = t.TournamentID
WHERE ts.TournamentID = 1 -- Replace 1 with your tournament ID
ORDER BY ts.Phase, ts.MatchDate;
GO
```

### Option B: Create a Test SQL File

Create a new file: `Test_Scheduling.sql`

```sql
USE [SportsHubDB];
GO

-- =============================================
-- TESTING QUERIES FOR TOURNAMENT SCHEDULING
-- =============================================

PRINT '=== Checking Tournaments Ready for Scheduling ===';
EXEC SP_CheckTournamentReadyForSchedule;
GO

PRINT '=== Tournament Status ===';
SELECT TournamentID, TournamentName, Status, CurrentPhase
FROM Tournaments
WHERE Status = 'Ready' OR Status = 'Active';
GO

PRINT '=== Team Payment Status ===';
SELECT 
    t.TournamentName,
    tm.TeamId,
    tm.PaymentStatus,
    tm.PaymentDate
FROM TournamentTeamMapping tm
INNER JOIN Tournaments t ON tm.TournamentId = t.TournamentID
WHERE tm.TournamentId = 1; -- Replace with your tournament ID
GO

PRINT '=== Manually Triggering Scheduling ===';
EXEC SP_GenerateTeamSchedule @TournamentID = 1; -- Replace with your tournament ID
GO

PRINT '=== Generated Matches ===';
SELECT 
    ts.TeamScheduleID,
    ts.Phase,
    ts.MatchDate,
    t1.TeamName as TeamA,
    t2.TeamName as TeamB,
    cm.MatchStatus,
    cm.WinnerTeamID
FROM TeamSchedule ts
INNER JOIN Tournaments t ON ts.TournamentID = t.TournamentID
LEFT JOIN Teams t1 ON ts.TeamAID = t1.TeamsID
LEFT JOIN Teams t2 ON ts.TeamBID = t2.TeamsID
LEFT JOIN CricketMatch cm ON ts.TeamScheduleID = cm.TeamScheduleID
WHERE ts.TournamentID = 1 -- Replace with your tournament ID
ORDER BY ts.Phase, ts.MatchDate;
GO
```

Then run this file in SSMS.

---

## 🔍 Step-by-Step: First Time Setup

### 1. Check Your Database Connection String

Look in your backend `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SportsHubDB;..."
  }
}
```

Note the **Server** and **Database** name.

### 2. Connect to Database

**For LocalDB (YOUR CURRENT SETUP):**
- Server: `(localdb)\MSSQLLocalDB`
- Database: `SportsHubDB`
- **This is what your appsettings.json shows!**

**For SQL Server Express:**
- Server: `localhost\SQLEXPRESS` or `.\SQLEXPRESS`
- Database: `SportsHubDB`

**For Full SQL Server:**
- Server: `localhost` or your server name
- Database: `SportsHubDB`

### 3. Run the Main Script

```sql
-- Step 1: Select your database
USE [SportsHubDB];
GO

-- Step 2: Run the entire SP_AutoScheduling.sql script
-- (Copy and paste the entire content from the file)
```

### 4. Verify Installation

```sql
-- Check if stored procedures exist
SELECT name 
FROM sys.procedures 
WHERE name IN ('SP_CheckTournamentReadyForSchedule', 'SP_GenerateTeamSchedule');
```

You should see both procedures listed.

---

## 🧪 Testing Workflow

### Test 1: Check if System is Ready
```sql
USE [SportsHubDB];
GO

-- Check tournaments ready for scheduling
EXEC SP_CheckTournamentReadyForSchedule;
```

**Expected Result:** List of TournamentIDs that are ready

### Test 2: Check Tournament Details
```sql
-- Replace 1 with your actual tournament ID
SELECT 
    TournamentID,
    TournamentName,
    Status,
    CurrentPhase,
    MaxTeams
FROM Tournaments
WHERE TournamentID = 1;
```

### Test 3: Check Team Registrations
```sql
-- Replace 1 with your actual tournament ID
SELECT 
    tm.TeamId,
    t.TeamName,
    tm.PaymentStatus,
    tm.PaymentDate
FROM TournamentTeamMapping tm
INNER JOIN Teams t ON tm.TeamId = t.TeamsID
WHERE tm.TournamentId = 1;
```

**Expected:** All teams should have `PaymentStatus = 'Confirmed'`

### Test 4: Manually Trigger Scheduling
```sql
-- Replace 1 with your actual tournament ID
EXEC SP_GenerateTeamSchedule @TournamentID = 1;
```

**Expected:** "Command(s) completed successfully"

### Test 5: Verify Matches Generated
```sql
-- Replace 1 with your actual tournament ID
SELECT 
    ts.TeamScheduleID,
    ts.Phase,
    t1.TeamName as TeamA,
    t2.TeamName as TeamB,
    ts.MatchDate
FROM TeamSchedule ts
LEFT JOIN Teams t1 ON ts.TeamAID = t1.TeamsID
LEFT JOIN Teams t2 ON ts.TeamBID = t2.TeamsID
WHERE ts.TournamentID = 1
ORDER BY ts.Phase, ts.MatchDate;
```

**Expected:** List of matches with correct Phase

---

## ⚠️ Common Issues & Solutions

### Issue 1: "Database does not exist"
**Solution:**
```sql
-- Check if database exists
SELECT name FROM sys.databases WHERE name = 'SportsHubDB';

-- If it doesn't exist, create it (or check your connection string)
CREATE DATABASE SportsHubDB;
```

### Issue 2: "Stored procedure already exists"
**Solution:** This is fine! The script uses `CREATE OR ALTER PROCEDURE`, so it will update the existing procedure.

### Issue 3: "Invalid object name 'TournamentTeamMapping'"
**Solution:** The table might not exist. Check if you need to run other setup scripts first.

### Issue 4: "Cannot find server"
**Solution:** 
- Check your connection string in `appsettings.json`
- Try different server names: `localhost`, `(localdb)\MSSQLLocalDB`, `.\SQLEXPRESS`

---

## 📍 Quick Reference: File Locations

```
SportsHub/
├── SportsHubBackend/
│   └── SportsHubBackend/
│       └── SQL/
│           └── SP_AutoScheduling.sql  ← Main script to run
│
└── Test_Scheduling.sql  ← Create this for testing (optional)
```

---

## ✅ Checklist

- [ ] SQL Server Management Studio (SSMS) or Azure Data Studio installed
- [ ] Connected to database server
- [ ] Selected `SportsHubDB` database
- [ ] Opened `SP_AutoScheduling.sql` file
- [ ] Executed the script successfully
- [ ] Verified stored procedures exist
- [ ] Tested with a tournament that has confirmed teams
- [ ] Verified matches are generated

---

## 🎯 Recommended Approach

**For First Time:**
1. Use **SQL Server Management Studio (SSMS)**
2. Open `SP_AutoScheduling.sql`
3. Select `SportsHubDB` database
4. Execute the entire script
5. Run test queries to verify

**For Regular Testing:**
1. Create a `Test_Scheduling.sql` file with all test queries
2. Run it whenever you need to check the system
3. Replace tournament ID as needed

---

## 💡 Pro Tips

1. **Always select the correct database** before running queries
2. **Use `GO` statements** to separate batches
3. **Test with a small tournament first** (2-4 teams)
4. **Check payment status** before expecting matches to generate
5. **Use transaction rollback** for testing:
   ```sql
   BEGIN TRANSACTION;
   EXEC SP_GenerateTeamSchedule @TournamentID = 1;
   -- Check results
   ROLLBACK TRANSACTION; -- Undo if needed
   ```

---

## 📞 Need Help?

If you encounter errors:
1. Copy the exact error message
2. Check which line in the script failed
3. Verify your database schema matches the script
4. Ensure all required tables exist

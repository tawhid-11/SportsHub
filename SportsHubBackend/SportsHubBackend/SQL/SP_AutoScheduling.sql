USE [SportsHubDB]
GO

PRINT 'Updating database for automatic scheduling...'

-- Add Phase column to TeamSchedule if missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TeamSchedule') AND name = 'Phase')
BEGIN
    ALTER TABLE TeamSchedule ADD Phase NVARCHAR(50) NULL;
END

-- Add CurrentPhase to Tournaments if missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Tournaments') AND name = 'CurrentPhase')
BEGIN
    ALTER TABLE Tournaments ADD CurrentPhase NVARCHAR(50) NULL;
END

-- Add GroupId to TournamentTeamMapping if missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TournamentTeamMapping') AND name = 'GroupId')
BEGIN
    ALTER TABLE TournamentTeamMapping ADD GroupId INT NULL;
END

-- Add Configuration columns for groups
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Tournaments') AND name = 'NumberOfGroups')
BEGIN
    ALTER TABLE Tournaments ADD NumberOfGroups INT DEFAULT 2;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Tournaments') AND name = 'TeamsPerGroup')
BEGIN
    ALTER TABLE Tournaments ADD TeamsPerGroup INT DEFAULT 4;
END
GO

-- Stored Procedure to check which tournaments are ready for scheduling/next phase
CREATE OR ALTER PROCEDURE SP_CheckTournamentReadyForSchedule
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. Tournaments with Status 'Ready' and no phase yet (Initial scheduling)
    -- 2. Tournaments with 'Active' status where all matches in current phase are 'Finished'
    -- Note: A match is considered 'Finished' only if a record exists in CricketMatch with MatchStatus = 'Finished'
    SELECT t.TournamentID
    FROM Tournaments t
    WHERE 
        (t.Status IN ('Ready', 'Upcoming') AND t.CurrentPhase IS NULL)
        OR
        (t.Status = 'Active' AND t.CurrentPhase IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM TeamSchedule ts 
            LEFT JOIN CricketMatch cm ON ts.TeamScheduleID = cm.TeamScheduleID
            WHERE ts.TournamentID = t.TournamentID 
            AND ts.Phase = t.CurrentPhase 
            AND (cm.MatchStatus IS NULL OR cm.MatchStatus != 'Finished')
        )
        AND t.CurrentPhase != 'Final' -- Final phase means tournament is over
        AND t.Status != 'Finished')
END
GO

-- Main Procedure to Generate Schedule
CREATE OR ALTER PROCEDURE SP_GenerateTeamSchedule
    @TournamentID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @TypeID INT, @TypeName NVARCHAR(100), @MaxTeams INT, @CurrentPhase NVARCHAR(50), @StartDate DATETIME;
    
    SELECT 
        @TypeID = t.TournamentTypeID, 
        @TypeName = tt.Name, 
        @MaxTeams = t.MaxTeams,
        @CurrentPhase = t.CurrentPhase,
        @StartDate = ISNULL(t.StartDate, GETDATE())
    FROM Tournaments t
    JOIN TournamentType tt ON t.TournamentTypeID = tt.Id
    WHERE t.TournamentID = @TournamentID;

    -- Handle different types
    
    -- 1. Round Robin
    IF @TypeName LIKE '%Round Robin%'
    BEGIN
        IF @CurrentPhase IS NULL
        BEGIN
            -- Generate all pairs (Simple version: everyone plays everyone once)
            ;WITH MatchPairs AS (
                SELECT t1.TeamId as TeamID_A, t2.TeamId as TeamID_B,
                       ROW_NUMBER() OVER (ORDER BY t1.TeamId, t2.TeamId) as rn
                FROM TournamentTeamMapping t1
                JOIN TournamentTeamMapping t2 ON t1.TournamentId = t2.TournamentId AND t1.TeamId < t2.TeamId
                WHERE t1.TournamentId = @TournamentID AND t1.PaymentStatus = 'Paid' AND t2.PaymentStatus = 'Paid'
            )
            INSERT INTO TeamSchedule (TournamentID, TeamAID, TeamBID, MatchDate, Phase)
            SELECT @TournamentID, TeamID_A, TeamID_B, 
                   DATEADD(HOUR, CASE WHEN (rn % 2) = 1 THEN 10 ELSE 14 END, DATEADD(DAY, (rn - 1) / 2, @StartDate)), 
                   'Round Robin'
            FROM MatchPairs;

            UPDATE Tournaments SET CurrentPhase = 'Round Robin', Status = 'Active' WHERE TournamentID = @TournamentID;
        END
        ELSE
        BEGIN
            -- Round Robin has only one phase, so if it's done, finish the tournament
            UPDATE Tournaments SET Status = 'Finished' WHERE TournamentID = @TournamentID AND CurrentPhase = 'Round Robin';
        END
    END
    
    -- 2. Knockout
    ELSE IF @TypeName LIKE '%Knockout%'
    BEGIN
        IF @CurrentPhase IS NULL
        BEGIN
            -- Initial pairings (Quarter-Finals for 8, Semi-Finals for 4)
            DECLARE @PhaseName NVARCHAR(50) = 'Semi-Final';
            IF @MaxTeams > 4 SET @PhaseName = 'Quarter-Final';
            
            ;WITH TeamsRanked AS (
                SELECT TeamId, ROW_NUMBER() OVER (ORDER BY NEWID()) as r -- Randomize initial pairings
                FROM TournamentTeamMapping WHERE TournamentId = @TournamentID AND PaymentStatus = 'Paid'
            )
            INSERT INTO TeamSchedule (TournamentID, TeamAID, TeamBID, MatchDate, Phase)
            SELECT @TournamentID, t1.TeamId, t2.TeamId, 
                   DATEADD(HOUR, CASE WHEN (ROW_NUMBER() OVER (ORDER BY t1.TeamId) % 2) = 1 THEN 10 ELSE 14 END, @StartDate), 
                   @PhaseName
            FROM TeamsRanked t1
            JOIN TeamsRanked t2 ON t1.r + 1 = t2.r
            WHERE t1.r % 2 = 1;

            UPDATE Tournaments SET CurrentPhase = @PhaseName, Status = 'Active' WHERE TournamentID = @TournamentID;
        END
        ELSE IF @CurrentPhase IN ('Quarter-Final', 'Semi-Final')
        BEGIN
            -- Generate next phase from winners
            DECLARE @NextPhase NVARCHAR(50) = CASE WHEN @CurrentPhase = 'Quarter-Final' THEN 'Semi-Final' ELSE 'Final' END;
            DECLARE @PrevMaxDate DATETIME = (SELECT MAX(MatchDate) FROM TeamSchedule WHERE TournamentID = @TournamentID AND Phase = @CurrentPhase);
            
            DECLARE @Winners TABLE (WinnerId INT, rn INT);
            INSERT INTO @Winners (WinnerId, rn)
            SELECT cm.WinnerTeamID, ROW_NUMBER() OVER (ORDER BY ts.TeamScheduleID)
            FROM TeamSchedule ts
            JOIN CricketMatch cm ON ts.TeamScheduleID = cm.TeamScheduleID
            WHERE ts.TournamentID = @TournamentID AND ts.Phase = @CurrentPhase AND cm.WinnerTeamID IS NOT NULL;

            -- Pair winners
            -- Final match is always at 3:00 PM (15:00)
            -- Semi-Finals/Quarter-Finals use 10 AM and 2 PM slots
            INSERT INTO TeamSchedule (TournamentID, TeamAID, TeamBID, MatchDate, Phase)
            SELECT @TournamentID, w1.WinnerId, w2.WinnerId, 
                   CASE 
                      WHEN @NextPhase = 'Final' THEN DATEADD(HOUR, 15, CAST(CAST(DATEADD(DAY, 1, @PrevMaxDate) AS DATE) AS DATETIME))
                      ELSE DATEADD(HOUR, CASE WHEN (w1.rn % 2) = 1 THEN 10 ELSE 14 END, DATEADD(DAY, (w1.rn - 1) / 2 + 1, CAST(CAST(@PrevMaxDate AS DATE) AS DATETIME)))
                   END,
                   @NextPhase
            FROM @Winners w1
            JOIN @Winners w2 ON w1.rn + 1 = w2.rn
            WHERE w1.rn % 2 = 1;

            UPDATE Tournaments SET CurrentPhase = @NextPhase WHERE TournamentID = @TournamentID;
        END
        ELSE IF @CurrentPhase = 'Final'
        BEGIN
             UPDATE Tournaments SET Status = 'Finished' WHERE TournamentID = @TournamentID;
        END
    END
    
    -- 3. Group Stage + Knockout
    ELSE IF @TypeName LIKE '%Group%'
    BEGIN
        IF @CurrentPhase IS NULL
        BEGIN
            -- 1. Assign Teams to Groups based on admin configuration
            DECLARE @NumGroups INT = 2;
            SELECT @NumGroups = ISNULL(NumberOfGroups, 2) FROM Tournaments WHERE TournamentID = @TournamentID;

            ;WITH RandomTeams AS (
                SELECT ID, ROW_NUMBER() OVER (ORDER BY NEWID()) as r 
                FROM TournamentTeamMapping WHERE TournamentId = @TournamentID AND PaymentStatus = 'Paid'
            )
            UPDATE TournamentTeamMapping
            SET GroupId = ((t.r - 1) % @NumGroups) + 1
            FROM RandomTeams t
            WHERE TournamentTeamMapping.ID = t.ID;

            -- 2. Generate Group Stage Matches (Round Robin within groups)
            -- Using 3 slots per day: 10 AM, 2 PM, 6 PM (18:00)
            ;WITH MatchPairs AS (
                SELECT t1.TeamId as TeamID_A, t2.TeamId as TeamID_B, t1.GroupId,
                       ROW_NUMBER() OVER (ORDER BY t1.GroupId, t1.TeamId, t2.TeamId) as rn_total
                FROM TournamentTeamMapping t1
                JOIN TournamentTeamMapping t2 ON t1.TournamentId = t2.TournamentId AND t1.TeamId < t2.TeamId
                WHERE t1.TournamentId = @TournamentID AND t1.GroupId = t2.GroupId
                AND t1.PaymentStatus = 'Paid' AND t2.PaymentStatus = 'Paid'
            )
            INSERT INTO TeamSchedule (TournamentID, TeamAID, TeamBID, MatchDate, Phase)
            SELECT @TournamentID, TeamID_A, TeamID_B, 
                   DATEADD(HOUR, 
                      CASE (rn_total - 1) % 3 
                         WHEN 0 THEN 10 
                         WHEN 1 THEN 14 
                         WHEN 2 THEN 18 
                      END, 
                      DATEADD(DAY, (rn_total - 1) / 3, CAST(CAST(@StartDate AS DATE) AS DATETIME))), 
                   'Group Stage'
            FROM MatchPairs;

            UPDATE Tournaments SET CurrentPhase = 'Group Stage', Status = 'Active' WHERE TournamentID = @TournamentID;
        END
        ELSE IF @CurrentPhase = 'Group Stage'
        BEGIN
            -- Calculate top 2 from each group
            DECLARE @TopTeams TABLE (GroupId INT, TeamId INT, RankInGroup INT);
            DECLARE @GPrevMaxDate DATETIME = (SELECT MAX(MatchDate) FROM TeamSchedule WHERE TournamentID = @TournamentID AND Phase = 'Group Stage');
            DECLARE @TotalGroups INT = (SELECT COUNT(DISTINCT GroupId) FROM TournamentTeamMapping WHERE TournamentId = @TournamentID);
            
            INSERT INTO @TopTeams (GroupId, TeamId, RankInGroup)
            SELECT GroupId, TeamsID,
                   ROW_NUMBER() OVER (PARTITION BY GroupId ORDER BY Points DESC, NRR DESC) as RankInGroup
            FROM TournamentPointTable pt
            JOIN TournamentTeamMapping tm ON pt.TeamsID = tm.TeamId AND pt.TournamentID = tm.TournamentId
            WHERE pt.TournamentID = @TournamentID;

            IF @TotalGroups = 8
            BEGIN
                -- Round of 16: G1#1 vs G2#2, G2#1 vs G1#2, G3#1 vs G4#2, G4#1 vs G3#2, G5#1 vs G6#2, G6#1 vs G5#2, G7#1 vs G8#2, G8#1 vs G7#2
                INSERT INTO TeamSchedule (TournamentID, TeamAID, TeamBID, MatchDate, Phase)
                SELECT @TournamentID, t1.TeamId, t2.TeamId, DATEADD(DAY, 1, @GPrevMaxDate), 'Round of 16'
                FROM (SELECT TeamId, GroupId FROM @TopTeams WHERE RankInGroup = 1) t1
                JOIN (SELECT TeamId, GroupId FROM @TopTeams WHERE RankInGroup = 2) t2 ON 
                    (t1.GroupId = 1 AND t2.GroupId = 2) OR (t1.GroupId = 2 AND t2.GroupId = 1) OR
                    (t1.GroupId = 3 AND t2.GroupId = 4) OR (t1.GroupId = 4 AND t2.GroupId = 3) OR
                    (t1.GroupId = 5 AND t2.GroupId = 6) OR (t1.GroupId = 6 AND t2.GroupId = 5) OR
                    (t1.GroupId = 7 AND t2.GroupId = 8) OR (t1.GroupId = 8 AND t2.GroupId = 7);
                
                UPDATE Tournaments SET CurrentPhase = 'Round of 16' WHERE TournamentID = @TournamentID;
            END
            ELSE IF @TotalGroups = 4
            BEGIN
                -- Quarter-Finals: G1#1 vs G2#2, G2#1 vs G1#2, G3#1 vs G4#2, G4#1 vs G3#2
                INSERT INTO TeamSchedule (TournamentID, TeamAID, TeamBID, MatchDate, Phase)
                SELECT @TournamentID, t1.TeamId, t2.TeamId, DATEADD(DAY, 1, @GPrevMaxDate), 'Quarter-Final'
                FROM (SELECT TeamId, GroupId FROM @TopTeams WHERE RankInGroup = 1) t1
                JOIN (SELECT TeamId, GroupId FROM @TopTeams WHERE RankInGroup = 2) t2 ON 
                    (t1.GroupId = 1 AND t2.GroupId = 2) OR (t1.GroupId = 2 AND t2.GroupId = 1) OR
                    (t1.GroupId = 3 AND t2.GroupId = 4) OR (t1.GroupId = 4 AND t2.GroupId = 3);
                
                UPDATE Tournaments SET CurrentPhase = 'Quarter-Final' WHERE TournamentID = @TournamentID;
            END
            ELSE
            BEGIN
                -- Standard Semi-Final 1: G1#1 vs G2#2, Semi-Final 2: G2#1 vs G1#2
                DECLARE @G1_1 INT, @G1_2 INT, @G2_1 INT, @G2_2 INT;
                SELECT @G1_1 = TeamId FROM @TopTeams WHERE GroupId = 1 AND RankInGroup = 1;
                SELECT @G1_2 = TeamId FROM @TopTeams WHERE GroupId = 1 AND RankInGroup = 2;
                SELECT @G2_1 = TeamId FROM @TopTeams WHERE GroupId = 2 AND RankInGroup = 1;
                SELECT @G2_2 = TeamId FROM @TopTeams WHERE GroupId = 2 AND RankInGroup = 2;

                IF @G1_1 IS NOT NULL AND @G2_2 IS NOT NULL
                    INSERT INTO TeamSchedule (TournamentID, TeamAID, TeamBID, MatchDate, Phase)
                    VALUES (@TournamentID, @G1_1, @G2_2, DATEADD(HOUR, 10, CAST(CAST(DATEADD(DAY, 1, @GPrevMaxDate) AS DATE) AS DATETIME)), 'Semi-Final');
                
                IF @G2_1 IS NOT NULL AND @G1_2 IS NOT NULL
                    INSERT INTO TeamSchedule (TournamentID, TeamAID, TeamBID, MatchDate, Phase)
                    VALUES (@TournamentID, @G2_1, @G1_2, DATEADD(HOUR, 14, CAST(CAST(DATEADD(DAY, 1, @GPrevMaxDate) AS DATE) AS DATETIME)), 'Semi-Final');

                UPDATE Tournaments SET CurrentPhase = 'Semi-Final' WHERE TournamentID = @TournamentID;
            END
        END
        ELSE IF @CurrentPhase = 'Round of 16'
        BEGIN
            -- Round of 16 -> Quarter-Final
            DECLARE @R16PrevMaxDate DATETIME = (SELECT MAX(MatchDate) FROM TeamSchedule WHERE TournamentID = @TournamentID AND Phase = 'Round of 16');
            DECLARE @R16Winners TABLE (WinnerId INT, rn INT);
            
            INSERT INTO @R16Winners (WinnerId, rn)
            SELECT cm.WinnerTeamID, ROW_NUMBER() OVER (ORDER BY ts.TeamScheduleID)
            FROM TeamSchedule ts
            JOIN CricketMatch cm ON ts.TeamScheduleID = cm.TeamScheduleID
            WHERE ts.TournamentID = @TournamentID AND ts.Phase = 'Round of 16' AND cm.WinnerTeamID IS NOT NULL;

            IF (SELECT COUNT(*) FROM @R16Winners) = 8
            BEGIN
                INSERT INTO TeamSchedule (TournamentID, TeamAID, TeamBID, MatchDate, Phase)
                SELECT @TournamentID, w1.WinnerId, w2.WinnerId, DATEADD(DAY, 1, @R16PrevMaxDate), 'Quarter-Final'
                FROM @R16Winners w1 JOIN @R16Winners w2 ON 
                    (w1.rn = 1 AND w2.rn = 2) OR (w1.rn = 3 AND w2.rn = 4) OR
                    (w1.rn = 5 AND w2.rn = 6) OR (w1.rn = 7 AND w2.rn = 8);

                UPDATE Tournaments SET CurrentPhase = 'Quarter-Final' WHERE TournamentID = @TournamentID;
            END
        END
        ELSE IF @CurrentPhase = 'Quarter-Final'
        BEGIN
            -- Quarter-Final -> Semi-Final
            DECLARE @QFPrevMaxDate DATETIME = (SELECT MAX(MatchDate) FROM TeamSchedule WHERE TournamentID = @TournamentID AND Phase = 'Quarter-Final');
            DECLARE @QFWinners TABLE (WinnerId INT, rn INT);
            
            INSERT INTO @QFWinners (WinnerId, rn)
            SELECT cm.WinnerTeamID, ROW_NUMBER() OVER (ORDER BY ts.TeamScheduleID)
            FROM TeamSchedule ts
            JOIN CricketMatch cm ON ts.TeamScheduleID = cm.TeamScheduleID
            WHERE ts.TournamentID = @TournamentID AND ts.Phase = 'Quarter-Final' AND cm.WinnerTeamID IS NOT NULL;

            IF (SELECT COUNT(*) FROM @QFWinners) = 4
            BEGIN
                INSERT INTO TeamSchedule (TournamentID, TeamAID, TeamBID, MatchDate, Phase)
                SELECT @TournamentID, w1.WinnerId, w2.WinnerId, DATEADD(DAY, 1, @QFPrevMaxDate), 'Semi-Final'
                FROM @QFWinners w1 JOIN @QFWinners w2 ON (w1.rn = 1 AND w2.rn = 2) OR (w1.rn = 3 AND w2.rn = 4);

                UPDATE Tournaments SET CurrentPhase = 'Semi-Final' WHERE TournamentID = @TournamentID;
            END
        END
        ELSE IF @CurrentPhase = 'Semi-Final'
        BEGIN
            -- Semi-Final -> Final
            DECLARE @GSFPrevMaxDate DATETIME = (SELECT MAX(MatchDate) FROM TeamSchedule WHERE TournamentID = @TournamentID AND Phase = 'Semi-Final');
            DECLARE @SF_Winners TABLE (WinnerId INT, rn INT);
            
            INSERT INTO @SF_Winners (WinnerId, rn)
            SELECT cm.WinnerTeamID, ROW_NUMBER() OVER (ORDER BY ts.TeamScheduleID)
            FROM TeamSchedule ts
            JOIN CricketMatch cm ON ts.TeamScheduleID = cm.TeamScheduleID
            WHERE ts.TournamentID = @TournamentID AND ts.Phase = 'Semi-Final' AND cm.WinnerTeamID IS NOT NULL;

            IF (SELECT COUNT(*) FROM @SF_Winners) = 2
            BEGIN
                INSERT INTO TeamSchedule (TournamentID, TeamAID, TeamBID, MatchDate, Phase)
                SELECT @TournamentID, w1.WinnerId, w2.WinnerId, 
                       DATEADD(HOUR, 15, CAST(CAST(DATEADD(DAY, 1, @GSFPrevMaxDate) AS DATE) AS DATETIME)), 
                       'Final'
                FROM @SF_Winners w1
                JOIN @SF_Winners w2 ON w1.rn = 1 AND w2.rn = 2;

                UPDATE Tournaments SET CurrentPhase = 'Final' WHERE TournamentID = @TournamentID;
            END
        END
        ELSE IF @CurrentPhase = 'Final'
        BEGIN
             UPDATE Tournaments SET Status = 'Finished' WHERE TournamentID = @TournamentID;
        END
    END
END
GO

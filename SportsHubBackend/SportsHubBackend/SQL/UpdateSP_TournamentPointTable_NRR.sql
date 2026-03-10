-- Drop and recreate SP_TournamentPointTable with NRR calculation
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_TournamentPointTable')
    DROP PROCEDURE SP_TournamentPointTable;
GO

CREATE PROCEDURE [dbo].[SP_TournamentPointTable]
    @Flag INT,
    @TournamentID INT = NULL,
    @TeamsID INT = NULL,
    @WinnerTeamID INT = NULL,
    @IsDraw BIT = 0,
    @RunsScored INT = 0,
    @BallsFaced INT = 0,
    @RunsConceded INT = 0,
    @BallsBowled INT = 0
AS
BEGIN
    IF @Flag = 1 -- Get Points Table by TournamentID (Showing all teams in tournament)
    BEGIN
        SELECT
            ISNULL(pt.PointTableID, 0) as PointTableID,
            @TournamentID as TournamentID,
            tm.TeamId as TeamsID,
            ISNULL(pt.Played, 0) as Played,
            ISNULL(pt.Won, 0) as Won,
            ISNULL(pt.Lost, 0) as Lost,
            ISNULL(pt.Draw, 0) as Draw,
            ISNULL(pt.NR, 0) as NR,
            ISNULL(pt.Points, 0) as Points,
            ISNULL(pt.NRR, 0) as NRR,
            t.TeamName,
            t.TeamLogo,
            tm.GroupId
        FROM TournamentTeamMapping tm
        JOIN Teams t ON tm.TeamId = t.TeamsID
        LEFT JOIN TournamentPointTable pt ON pt.TeamsID = tm.TeamId AND pt.TournamentID = tm.TournamentId
        WHERE tm.TournamentId = @TournamentID
        ORDER BY tm.GroupId, ISNULL(pt.Points, 0) DESC, ISNULL(pt.NRR, 0) DESC, t.TeamName ASC;
    END

    ELSE IF @Flag = 2 -- Update/Initialize Points for teams in a match
    BEGIN
        -- Initialize if not exists
        IF NOT EXISTS (SELECT 1 FROM TournamentPointTable WHERE TournamentID = @TournamentID AND TeamsID = @TeamsID)
        BEGIN
            INSERT INTO TournamentPointTable (TournamentID, TeamsID, Played, Won, Lost, Draw, NR, Points, NRR, TotalRunsScored, TotalBallsFaced, TotalRunsConceded, TotalBallsBowled)
            VALUES (@TournamentID, @TeamsID, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        END

        -- Update cumulative stats
        UPDATE TournamentPointTable
        SET 
            TotalRunsScored = TotalRunsScored + @RunsScored,
            TotalBallsFaced = TotalBallsFaced + @BallsFaced,
            TotalRunsConceded = TotalRunsConceded + @RunsConceded,
            TotalBallsBowled = TotalBallsBowled + @BallsBowled
        WHERE TournamentID = @TournamentID AND TeamsID = @TeamsID;

        -- Update match result stats
        IF @WinnerTeamID IS NOT NULL AND @WinnerTeamID > 0
        BEGIN
            IF @WinnerTeamID = @TeamsID
            BEGIN
                UPDATE TournamentPointTable
                SET Played = Played + 1, Won = Won + 1, Points = Points + 2
                WHERE TournamentID = @TournamentID AND TeamsID = @TeamsID;
            END
            ELSE
            BEGIN
                UPDATE TournamentPointTable
                SET Played = Played + 1, Lost = Lost + 1
                WHERE TournamentID = @TournamentID AND TeamsID = @TeamsID;
            END
        END
        ELSE IF @IsDraw = 1
        BEGIN
            UPDATE TournamentPointTable
            SET Played = Played + 1, Draw = Draw + 1, Points = Points + 1
            WHERE TournamentID = @TournamentID AND TeamsID = @TeamsID;
        END

        -- Calculate NRR
        DECLARE @RunsFor DECIMAL(18, 6), @OversFor DECIMAL(18, 6), @RunsAgainst DECIMAL(18, 6), @OversAgainst DECIMAL(18, 6);
        DECLARE @NewNRR DECIMAL(18, 3);

        SELECT 
            @RunsFor = CAST(TotalRunsScored AS DECIMAL(18, 6)),
            @OversFor = CAST(TotalBallsFaced AS DECIMAL(18, 6)) / 6.0,
            @RunsAgainst = CAST(TotalRunsConceded AS DECIMAL(18, 6)),
            @OversAgainst = CAST(TotalBallsBowled AS DECIMAL(18, 6)) / 6.0
        FROM TournamentPointTable
        WHERE TournamentID = @TournamentID AND TeamsID = @TeamsID;

        -- NRR = (Runs Scored / Overs Faced) - (Runs Conceded / Overs Bowled)
        IF @OversFor > 0 AND @OversAgainst > 0
        BEGIN
            SET @NewNRR = (@RunsFor / @OversFor) - (@RunsAgainst / @OversAgainst);
            
            UPDATE TournamentPointTable
            SET NRR = @NewNRR
            WHERE TournamentID = @TournamentID AND TeamsID = @TeamsID;
        END
    END
END
GO

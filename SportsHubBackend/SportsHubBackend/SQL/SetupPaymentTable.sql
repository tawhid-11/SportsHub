-- =============================================
-- Quick Setup Script for Payment Table Feature
-- Execute this in SQL Server Management Studio
-- =============================================

USE [SportsHubDB]
GO

PRINT '========================================='
PRINT 'Payment Table Feature - Database Setup'
PRINT '========================================='
PRINT ''

-- Step 1: Add Payment Tracking Columns
PRINT 'Step 1: Adding payment tracking columns...'
PRINT ''

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TournamentTeamMapping') AND name = 'PaymentStatus')
BEGIN
    ALTER TABLE TournamentTeamMapping ADD PaymentStatus NVARCHAR(50) NULL DEFAULT 'Pending';
    PRINT '✓ Added PaymentStatus column'
END
ELSE PRINT '  PaymentStatus column already exists'

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TournamentTeamMapping') AND name = 'PaymentDate')
BEGIN
    ALTER TABLE TournamentTeamMapping ADD PaymentDate DATETIME NULL;
    PRINT '✓ Added PaymentDate column'
END
ELSE PRINT '  PaymentDate column already exists'

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TournamentTeamMapping') AND name = 'bkashTransactionId')
BEGIN
    ALTER TABLE TournamentTeamMapping ADD bkashTransactionId NVARCHAR(255) NULL;
    PRINT '✓ Added bkashTransactionId column'
END
ELSE PRINT '  bkashTransactionId column already exists'

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TournamentTeamMapping') AND name = 'CreatedDate')
BEGIN
    ALTER TABLE TournamentTeamMapping ADD CreatedDate DATETIME NULL DEFAULT GETDATE();
    PRINT '✓ Added CreatedDate column'
END
ELSE PRINT '  CreatedDate column already exists'

PRINT ''
PRINT 'Step 1 Complete!'
PRINT ''

-- Step 2: Create Stored Procedure
PRINT 'Step 2: Creating stored procedure SP_TeamPayment...'
PRINT ''

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('SP_TeamPayment') AND type = 'P')
BEGIN
    DROP PROCEDURE SP_TeamPayment
    PRINT '  Dropped existing SP_TeamPayment'
END

EXEC('
CREATE PROCEDURE SP_TeamPayment
    @Flag INT,
    @ID INT = NULL,
    @Phone NVARCHAR(50) = NULL,
    @OTP NVARCHAR(10) = NULL,
    @Amount DECIMAL(10,2) = NULL,
    @userId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Flag = 1
    BEGIN
        SELECT 
            ttm.ID,
            t.TeamName,
            t.TeamOwnerName,
            t.TeamOwnerPhoneNumber AS Phone,
            tour.TournamentName,
            tour.RegistrationFee AS Amount,
            ttm.bkashPaymentId,
            ttm.bkashTransactionId,
            ttm.PaymentStatus,
            ttm.PaymentDate,
            ttm.CreatedDate,
            u.Name AS UserName,
            u.Email AS UserEmail
        FROM TournamentTeamMapping ttm
        INNER JOIN Teams t ON ttm.TeamId = t.TeamsID
        INNER JOIN Tournaments tour ON ttm.TournamentId = tour.TournamentID
        LEFT JOIN Users u ON t.UserId = u.ID
        WHERE ttm.bkashPaymentId IS NOT NULL
        ORDER BY ttm.CreatedDate DESC;
    END
    ELSE IF @Flag = 3
    BEGIN
        UPDATE TournamentTeamMapping
        SET PaymentStatus = ''Completed'',
            PaymentDate = GETDATE()
        WHERE bkashPaymentId = @Phone;
    END
END
')

PRINT '✓ Created SP_TeamPayment stored procedure'
PRINT ''
PRINT 'Step 2 Complete!'
PRINT ''

PRINT '========================================='
PRINT 'Database setup completed successfully!'
PRINT '========================================='
PRINT ''
PRINT 'Next steps:'
PRINT '1. Run the backend: cd SportsHubBackend && dotnet run'
PRINT '2. Run the frontend: cd SportsHubFrontend && npm start'
PRINT '3. Login as admin and navigate to Payments menu'
PRINT ''
GO

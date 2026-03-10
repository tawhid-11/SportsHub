-- =============================================
-- Database Schema Update for Payment Tracking
-- Description: Adds necessary columns to TournamentTeamMapping table for payment tracking
-- =============================================

USE [SportsHubDB]
GO

-- Check if PaymentStatus column exists, if not add it
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID('TournamentTeamMapping') 
               AND name = 'PaymentStatus')
BEGIN
    ALTER TABLE TournamentTeamMapping
    ADD PaymentStatus NVARCHAR(50) NULL DEFAULT 'Pending';
    
    PRINT 'Added PaymentStatus column to TournamentTeamMapping table'
END
ELSE
BEGIN
    PRINT 'PaymentStatus column already exists'
END
GO

-- Check if PaymentDate column exists, if not add it
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID('TournamentTeamMapping') 
               AND name = 'PaymentDate')
BEGIN
    ALTER TABLE TournamentTeamMapping
    ADD PaymentDate DATETIME NULL;
    
    PRINT 'Added PaymentDate column to TournamentTeamMapping table'
END
ELSE
BEGIN
    PRINT 'PaymentDate column already exists'
END
GO

-- Check if bkashTransactionId column exists, if not add it
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID('TournamentTeamMapping') 
               AND name = 'bkashTransactionId')
BEGIN
    ALTER TABLE TournamentTeamMapping
    ADD bkashTransactionId NVARCHAR(255) NULL;
    
    PRINT 'Added bkashTransactionId column to TournamentTeamMapping table'
END
ELSE
BEGIN
    PRINT 'bkashTransactionId column already exists'
END
GO

-- Check if CreatedDate column exists, if not add it
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID('TournamentTeamMapping') 
               AND name = 'CreatedDate')
BEGIN
    ALTER TABLE TournamentTeamMapping
    ADD CreatedDate DATETIME NULL DEFAULT GETDATE();
    
    PRINT 'Added CreatedDate column to TournamentTeamMapping table'
END
ELSE
BEGIN
    PRINT 'CreatedDate column already exists'
END
GO

PRINT 'Database schema update completed successfully!'
GO

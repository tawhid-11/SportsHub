-- =============================================
-- Stored Procedure: SP_TeamPayment
-- Description: Manages team payment operations
-- =============================================

CREATE OR ALTER PROCEDURE SP_TeamPayment
    @Flag INT,
    @ID INT = NULL,
    @Phone NVARCHAR(50) = NULL,
    @OTP NVARCHAR(10) = NULL,
    @Amount DECIMAL(10,2) = NULL,
    @userId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Flag 1: Get all payment details with team and tournament information
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

    -- Flag 2: Insert new payment record (if needed separately)
    ELSE IF @Flag = 2
    BEGIN
        -- This would be for inserting standalone payment records
        -- Currently payments are inserted via TournamentTeamMapping
        SELECT 'Not Implemented' AS Message;
    END

    -- Flag 3: Update payment status
    ELSE IF @Flag = 3
    BEGIN
        UPDATE TournamentTeamMapping
        SET PaymentStatus = 'Completed',
            PaymentDate = GETDATE()
        WHERE bkashPaymentId = @Phone; -- Using @Phone parameter to pass bkashPaymentId
    END
END
GO

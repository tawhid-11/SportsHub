using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SportsHubBackend.DBContext;
using SportsHubBackend.Services;
using System.Data;

namespace SportsHubBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly DapperContext _context;
       

        public PaymentController(DapperContext context)
        {
            _context = context;
        }

        [HttpGet("GetPaymentDetails")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                using (var connection = _context.CreateConnection())
                {
                    var query = @"
                        SELECT 
                            ttm.Id,
                            ttm.TournamentId,
                            ttm.TeamId,
                            ttm.PaymentStatus,
                            ttm.bkashPaymentId,
                            ttm.PaymentDate,
                            ttm.bkashTransactionId,
                            ttm.CreatedDate,
                            ttm.GroupId,
                            tr.TournamentName,
                            tr.RegistrationFee,
                            t.TeamName,
                            t.TeamOwnerName,
                            t.TeamOwnerEmail,
                            t.TeamOwnerPhoneNumber
                        FROM TournamentTeamMapping ttm
                        LEFT JOIN Tournaments tr ON ttm.TournamentId = tr.TournamentID
                        LEFT JOIN Teams t ON ttm.TeamId = t.TeamsID
                        ORDER BY ttm.CreatedDate DESC";

                    var result = await connection.QueryAsync<dynamic>(query);
                    var rdata = new
                    {
                        success = true,
                        Message = "Payment details fetched successfully",
                        Data = result
                    };
                    return Ok(rdata);
                }
            }
            catch (Exception ex)
            {
                var rdata = new
                {
                    success = false,
                    Message = "Error - " + ex.Message,
                };
                return BadRequest(rdata);
            }
        }

        [HttpPost("SetupDatabase")]
        public async Task<IActionResult> SetupDatabase()
        {
            try
            {
                using (var connection = _context.CreateConnection())
                {
                    connection.Open();
                    
                    // Step 1: Add columns if they don't exist
                    var setupScript = @"
                        -- Add PaymentStatus column
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TournamentTeamMapping') AND name = 'PaymentStatus')
                        BEGIN
                            ALTER TABLE TournamentTeamMapping ADD PaymentStatus NVARCHAR(50) NULL DEFAULT 'Pending';
                        END

                        -- Add PaymentDate column
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TournamentTeamMapping') AND name = 'PaymentDate')
                        BEGIN
                            ALTER TABLE TournamentTeamMapping ADD PaymentDate DATETIME NULL;
                        END

                        -- Add bkashTransactionId column
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TournamentTeamMapping') AND name = 'bkashTransactionId')
                        BEGIN
                            ALTER TABLE TournamentTeamMapping ADD bkashTransactionId NVARCHAR(255) NULL;
                        END

                        -- Add CreatedDate column
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TournamentTeamMapping') AND name = 'CreatedDate')
                        BEGIN
                            ALTER TABLE TournamentTeamMapping ADD CreatedDate DATETIME NULL DEFAULT GETDATE();
                        END
                    ";

                    await connection.ExecuteAsync(setupScript);

                    // Step 2: Create or replace stored procedure
                    var spScript = @"
                        IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('SP_TeamPayment') AND type = 'P')
                        BEGIN
                            DROP PROCEDURE SP_TeamPayment
                        END
                    ";
                    await connection.ExecuteAsync(spScript);

                    var createSpScript = @"
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
                                SET PaymentStatus = 'Completed',
                                    PaymentDate = GETDATE()
                                WHERE bkashPaymentId = @Phone;
                            END
                        END
                    ";

                    await connection.ExecuteAsync(createSpScript);

                    // Step 3: Create or replace SP_TournamentTeamMapping
                    var spMappingScript = @"
                        IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('SP_TournamentTeamMapping') AND type = 'P')
                        BEGIN
                            DROP PROCEDURE SP_TournamentTeamMapping
                        END
                    ";
                    await connection.ExecuteAsync(spMappingScript);

                    var createSpMappingScript = @"
                        CREATE PROCEDURE SP_TournamentTeamMapping
                            @Flag INT,
                            @TournamentId INT = NULL,
                            @TeamId INT = NULL,
                            @UserId INT = NULL,
                            @bkashPaymentId NVARCHAR(255) = NULL,
                            @bkashTrans NVARCHAR(255) = NULL
                        AS
                        BEGIN
                            SET NOCOUNT ON;

                            -- Flag 2: Initial Registration (Set to Pending)
                            IF @Flag = 2
                            BEGIN
                                -- Check if already exists
                                IF NOT EXISTS (SELECT 1 FROM TournamentTeamMapping WHERE TournamentId = @TournamentId AND TeamId = @TeamId)
                                BEGIN
                                    INSERT INTO TournamentTeamMapping (TournamentId, TeamId, userId, PaymentStatus, CreatedDate)
                                    VALUES (@TournamentId, @TeamId, @UserId, 'Pending', GETDATE());
                                END
                                ELSE
                                BEGIN
                                    UPDATE TournamentTeamMapping
                                    SET PaymentStatus = 'Pending', userId = ISNULL(@UserId, userId)
                                    WHERE TournamentId = @TournamentId AND TeamId = @TeamId;
                                END

                                -- Return Tournament Details for bKash initiation
                                SELECT 
                                    t.TournamentName,
                                    t.RegistrationFee
                                FROM Tournaments t
                                WHERE t.TournamentID = @TournamentId;
                            END

                            -- Flag 1: Update bkashPaymentId after initiation
                            ELSE IF @Flag = 1
                            BEGIN
                                UPDATE TournamentTeamMapping
                                SET bkashPaymentId = @bkashPaymentId
                                WHERE TournamentId = @TournamentId AND TeamId = @TeamId AND PaymentStatus = 'Pending';
                            END

                            -- Flag 3: Confirm Payment (Set to Confirmed)
                            ELSE IF @Flag = 3
                            BEGIN
                                UPDATE TournamentTeamMapping
                                SET PaymentStatus = 'Confirmed',
                                    PaymentDate = GETDATE(),
                                    bkashTransactionId = @bkashTrans
                                WHERE bkashPaymentId = @bkashPaymentId;
                            END
                        END
                    ";

                    await connection.ExecuteAsync(createSpMappingScript);

                    // Step 4: Create or replace SP_Tournaments (Update Flags for Payment Status)
                    var spTournamentsScript = @"
                        IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('SP_Tournaments') AND type = 'P')
                        BEGIN
                            DROP PROCEDURE SP_Tournaments
                        END
                    ";
                    await connection.ExecuteAsync(spTournamentsScript);

                    var createSpTournamentsScript = @"
                        CREATE PROCEDURE SP_Tournaments
                            @Flag INT,
                            @TournamentId INT = NULL,
                            @TournamentName NVARCHAR(255) = NULL,
                            @Location NVARCHAR(255) = NULL,
                            @StartDate DATETIME = NULL,
                            @EndDate DATETIME = NULL,
                            @TournamentTypeID INT = NULL,
                            @RegistrationDeadline DATETIME = NULL,
                            @RegistrationFee INT = NULL,
                            @FieldFee INT = NULL,
                            @MaxTeams INT = NULL,
                            @TotalPlayer INT = NULL,
                            @MatchPlayer INT = NULL,
                            @ExtraPlayer INT = NULL,
                            @Status NVARCHAR(50) = NULL,
                            @ContactNumber INT = NULL,
                            @userId INT = NULL,
                            @CreatedBy INT = NULL,
                            @CreatedAt DATETIME = NULL,
                            @UpdatedBy INT = NULL,
                            @UpdatedAt DATETIME = NULL,
                            @IsActive BIT = NULL
                        AS
                        BEGIN
                            SET NOCOUNT ON;

                            -- Flag 1: Get All (For Admin)
                            IF @Flag = 1
                            BEGIN
                                SELECT * FROM Tournaments WHERE IsActive = 1 OR @IsActive = 0;
                            END

                            -- Flag 7: Get REGISTERED Tournaments for User (ONLY CONFIRMED)
                            ELSE IF @Flag = 7
                            BEGIN
                                SELECT t.* 
                                FROM Tournaments t
                                INNER JOIN TournamentTeamMapping ttm ON t.TournamentID = ttm.TournamentId
                                WHERE ttm.userId = @userId AND ttm.PaymentStatus = 'Confirmed';
                            END

                            -- Flag 8: Get UNREGISTERED Tournaments for User (Not Confirmed yet)
                            ELSE IF @Flag = 8
                            BEGIN
                                SELECT * FROM Tournaments 
                                WHERE TournamentID NOT IN (
                                    SELECT TournamentId FROM TournamentTeamMapping 
                                    WHERE userId = @userId AND PaymentStatus = 'Confirmed'
                                ) AND IsActive = 1;
                            END

                            -- Flag 6: Get by ID
                            ELSE IF @Flag = 6
                            BEGIN
                                SELECT * FROM Tournaments WHERE TournamentID = @TournamentId;
                            END

                            -- Other flags (simplification for this setup)
                            ELSE
                            BEGIN
                                SELECT * FROM Tournaments WHERE IsActive = 1;
                            END
                        END
                    ";

                    await connection.ExecuteAsync(createSpTournamentsScript);

                    return Ok(new
                    {
                        success = true,
                        Message = "Database setup completed successfully! Payment table columns, SP_TeamPayment, SP_TournamentTeamMapping, and SP_Tournaments updated."
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    Message = "Database setup failed: " + ex.Message
                });
            }
        }
    }
}

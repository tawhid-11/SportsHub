using Dapper;
using SportsHubBackend.DBContext;
using System.Data;

namespace SportsHubBackend.BackgroundServices
{
    public class TournamentSchedulerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public TournamentSchedulerService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<DapperContext>();

                // 1. Get tournaments ready for scheduling
                var tournaments = await db.CreateConnection().QueryAsync<int>(
                    "SP_CheckTournamentReadyForSchedule",
                    commandType: CommandType.StoredProcedure
                );

                // 2. Generate schedules
                foreach (var tournamentId in tournaments)
                {
                    await db.CreateConnection().ExecuteAsync(
                        "SP_GenerateTeamSchedule",
                        new { TournamentID = tournamentId },
                        commandType: CommandType.StoredProcedure
                    );
                }

                // Run more frequently to handle multi-phase transitions
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }

}

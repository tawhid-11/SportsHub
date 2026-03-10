using Dapper;
using Microsoft.AspNetCore.SignalR;
using SportsHubBackend.DBContext;
using SportsHubBackend.Model;
using System.Data;

namespace SportsHubBackend.Hubs
{
    public class SignalRHub : Hub
    {
        private DapperContext _context;
        public SignalRHub(DapperContext context)
        {
            _context = context;
        }
        public override async Task OnConnectedAsync()
        {
            

            await base.OnConnectedAsync();
        }
        public async Task StartLiveMatch(int cricketMatchId)
        {
            try
            {
                var perameter = new DynamicParameters();
                perameter.Add("@Flag", 7);
                perameter.Add("@CricketMatchID", cricketMatchId);
                using (var connetion = _context.CreateConnection())
                {
                    var result = await connetion.QueryFirstOrDefaultAsync<dynamic>("SP_CricketMatch", perameter, commandType: CommandType.StoredProcedure);
                    var data =new
                    {
                        TeamAName = result.TeamAName,
                        TeamALogo = result.TeamALogo,
                        TeamBName = result.TeamBName,
                        TeamBLogo = result.TeamBLogo,
                        Overs = result.Overs,
                        TotalRun = result.TotalRun,
                        Wicket =0,
                        CricketMatchID = result.CricketMatchID

                    };
                    await Clients.All.SendAsync("ReceiveLiveMatch", data);

                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            
           

            await base.OnDisconnectedAsync(exception);
        }
    }
}

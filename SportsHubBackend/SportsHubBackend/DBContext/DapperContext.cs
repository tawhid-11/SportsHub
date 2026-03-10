using Microsoft.Data.SqlClient;
using System.Data;

namespace SportsHubBackend.DBContext
{
    public class DapperContext
    {
        private readonly IConfiguration _configuration;
        private readonly string? _connectionString;
        public DapperContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("SportsHubDatabase") ??"";
        }

        public IEnumerable<object> Tournaments { get; internal set; }

        public IDbConnection CreateConnection()
            => new SqlConnection(_connectionString);
    }
}


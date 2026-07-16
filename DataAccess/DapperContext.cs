using MySqlConnector;
using System.Data;

namespace CapfortuneBE.DataAccess
{
    public class DapperContext
    {
        private readonly string _connectionString;

        public DapperContext(IConfiguration configuration)
        {
            var raw = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

            _connectionString = NormalizeConnectionString(raw);
        }

        public IDbConnection CreateConnection() => new MySqlConnection(_connectionString);

        // Railway's MYSQL_URL is a "mysql://user:pass@host:port/db" URI, not a
        // MySqlConnector-style key=value string, so it needs converting.
        private static string NormalizeConnectionString(string raw)
        {
            if (!raw.StartsWith("mysql://", StringComparison.OrdinalIgnoreCase))
                return raw;

            var uri = new Uri(raw);
            var userInfo = uri.UserInfo.Split(':', 2);

            var builder = new MySqlConnectionStringBuilder
            {
                Server = uri.Host,
                Port = (uint)(uri.Port > 0 ? uri.Port : 3306),
                UserID = Uri.UnescapeDataString(userInfo[0]),
                Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty,
                Database = uri.AbsolutePath.TrimStart('/')
            };

            return builder.ConnectionString;
        }
    }
}

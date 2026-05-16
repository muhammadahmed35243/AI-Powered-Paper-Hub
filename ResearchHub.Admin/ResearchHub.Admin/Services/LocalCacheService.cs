using Microsoft.Data.Sqlite;

namespace ResearchHub.Admin.Services
{
    public class LocalCacheService
    {
        private readonly string _dbPath;

        public LocalCacheService()
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ResearchHubAdmin");
            Directory.CreateDirectory(folder);
            _dbPath = Path.Combine(folder, "cache.db");
            Initialize();
        }

        private void Initialize()
        {
            using var conn = new SqliteConnection($"Data Source={_dbPath}");
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = """
                CREATE TABLE IF NOT EXISTS Cache (
                    Key TEXT PRIMARY KEY,
                    Json TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL
                );
                """;
            cmd.ExecuteNonQuery();
        }

        public void Save(string key, string json)
        {
            using var conn = new SqliteConnection($"Data Source={_dbPath}");
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = """
                INSERT INTO Cache (Key, Json, UpdatedAt) VALUES ($k, $j, $t)
                ON CONFLICT(Key) DO UPDATE SET Json=$j, UpdatedAt=$t;
                """;
            cmd.Parameters.AddWithValue("$k", key);
            cmd.Parameters.AddWithValue("$j", json);
            cmd.Parameters.AddWithValue("$t", DateTime.UtcNow.ToString("O"));
            cmd.ExecuteNonQuery();
        }

        public string? Load(string key)
        {
            using var conn = new SqliteConnection($"Data Source={_dbPath}");
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Json FROM Cache WHERE Key=$k";
            cmd.Parameters.AddWithValue("$k", key);
            return cmd.ExecuteScalar() as string;
        }
    }
}

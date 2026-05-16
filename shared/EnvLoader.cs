namespace ResearchHub.Shared;

/// <summary>
/// Loads a .env file from the repository root (walks up from several start directories).
/// Values use ASP.NET Core env var syntax, e.g. Jwt__Key=secret
/// </summary>
public static class EnvLoader
{
    public static string? LoadedPath { get; private set; }

    public static void Load(string? startDirectory = null)
    {
        if (LoadedPath != null)
            return;

        var starts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrEmpty(startDirectory))
            starts.Add(startDirectory);
        starts.Add(Directory.GetCurrentDirectory());
        starts.Add(AppContext.BaseDirectory);

        foreach (var start in starts)
        {
            var dir = new DirectoryInfo(start);
            while (dir != null)
            {
                var path = Path.Combine(dir.FullName, ".env");
                if (File.Exists(path))
                {
                    DotNetEnv.Env.Load(path);
                    LoadedPath = path;
                    return;
                }
                dir = dir.Parent;
            }
        }
    }
}

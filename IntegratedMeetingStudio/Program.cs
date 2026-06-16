namespace IntegratedMeetingStudio;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // Load .env file
        try { DotNetEnv.Env.Load(); } catch { } // Search in current directory (project root during dotnet run)
        
        var envPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env");
        if (File.Exists(envPath))
        {
            DotNetEnv.Env.Load(envPath);
        }

        // Initialize local storage directories
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        Directory.CreateDirectory(Path.Combine(baseDir, "Data", "meetings"));
        Directory.CreateDirectory(Path.Combine(baseDir, "Data", "tasks"));
        Directory.CreateDirectory(Path.Combine(baseDir, "Data", "cache"));

        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }    
}

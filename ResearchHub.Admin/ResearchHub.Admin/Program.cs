using ResearchHub.Shared;

namespace ResearchHub.Admin
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            EnvLoader.Load();
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}
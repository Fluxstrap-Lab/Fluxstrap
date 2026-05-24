using Fluxstrap.Enums;

namespace Fluxstrap.Integrations
{
    public static class Cleaner
    {
        private const string LOG_IDENT = "Cleaner";

        public static void PerformCleanup()
        {
            var option = App.Settings.Prop.CleanerOptions;
            if (option == CleanerOptions.Never) return;

            try
            {
                long totalCleaned = 0;
                totalCleaned += CleanDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Roblox", "logs"), "*.log");
                totalCleaned += CleanDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Roblox", "downloads"), "*.*");
                totalCleaned += CleanDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp", "Roblox"), "*.*");

                foreach (var dir in App.Settings.Prop.CleanerDirectories)
                {
                    if (Directory.Exists(dir))
                        totalCleaned += CleanDirectory(dir, "*.*");
                }

                if (totalCleaned > 0)
                    App.Logger.WriteLine(LOG_IDENT, $"Cleaned approximately {totalCleaned / 1024 / 1024} MB");
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        private static long CleanDirectory(string path, string pattern)
        {
            long cleaned = 0;
            try
            {
                if (!Directory.Exists(path)) return 0;

                foreach (var file in Directory.GetFiles(path, pattern, SearchOption.AllDirectories))
                {
                    try
                    {
                        var info = new FileInfo(file);
                        cleaned += info.Length;
                        info.Delete();
                    }
                    catch { }
                }
            }
            catch { }
            return cleaned;
        }
    }
}

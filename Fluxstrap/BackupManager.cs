using System.IO.Compression;

namespace Fluxstrap
{
    public static class BackupManager
    {
        private const string LOG_IDENT = "BackupManager";

        public static void CreateBackup(string outputPath)
        {
            var files = new List<string>
            {
                App.Settings.FileLocation,
                App.State.FileLocation,
                App.FastFlags.FileLocation,
                Path.Combine(Paths.Base, "PlayHistory.json"),
                Path.Combine(Paths.Base, "SessionHistory.json"),
                Path.Combine(Paths.Base, "ServerHistory.json")
            };

            using var zipStream = new FileStream(outputPath, FileMode.Create);
            using var archive = new ZipArchive(zipStream, ZipArchiveMode.Create);

            foreach (string file in files.Where(File.Exists))
            {
                string entryName = Path.GetFileName(file);
                archive.CreateEntryFromFile(file, entryName);
            }

            if (Directory.Exists(Paths.CustomThemes))
            {
                foreach (string themeFile in Directory.GetFiles(Paths.CustomThemes, "*", SearchOption.AllDirectories))
                {
                    string relativePath = Path.GetRelativePath(Paths.Base, themeFile);
                    archive.CreateEntryFromFile(themeFile, relativePath);
                }
            }

            if (Directory.Exists(Paths.Modifications))
            {
                foreach (string modFile in Directory.GetFiles(Paths.Modifications, "*", SearchOption.AllDirectories))
                {
                    string relativePath = Path.GetRelativePath(Paths.Base, modFile);
                    archive.CreateEntryFromFile(modFile, relativePath);
                }
            }

            string? processDir = Path.GetDirectoryName(Paths.Process);
            if (processDir is not null && File.Exists(Path.Combine(processDir, "Settings.json")))
            {
                archive.CreateEntryFromFile(Path.Combine(processDir, "Settings.json"), "portable-Settings.json");
            }

            App.Logger.WriteLine(LOG_IDENT, $"Backup created at {outputPath}");
        }

        public static void RestoreBackup(string backupPath)
        {
            using var zipStream = new FileStream(backupPath, FileMode.Open);
            using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);

            foreach (var entry in archive.Entries)
            {
                try
                {
                    string destPath;

                    if (entry.FullName.Equals("portable-Settings.json", StringComparison.OrdinalIgnoreCase))
                    {
                        string? processDir = Path.GetDirectoryName(Paths.Process);
                        if (processDir is null)
                            continue;
                        destPath = Path.Combine(processDir, "Settings.json");
                    }
                    else if (entry.FullName.StartsWith("CustomThemes", StringComparison.OrdinalIgnoreCase) ||
                             entry.FullName.StartsWith("Modifications", StringComparison.OrdinalIgnoreCase))
                    {
                        destPath = Path.Combine(Paths.Base, entry.FullName);
                    }
                    else
                    {
                        destPath = Path.Combine(Paths.Base, entry.FullName);
                    }

                    Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
                    entry.ExtractToFile(destPath, overwrite: true);
                }
                catch (Exception ex)
                {
                    App.Logger.WriteException(LOG_IDENT, ex);
                }
            }

            App.Settings.Load();
            App.State.Load();
            App.FastFlags.Load();

            App.Logger.WriteLine(LOG_IDENT, "Backup restored successfully");
        }
    }
}

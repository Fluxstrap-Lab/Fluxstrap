using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Win32;

namespace Fluxstrap.UI.ViewModels.Settings
{
    public class FluxstrapViewModel : NotifyPropertyChangedViewModel
    {
        public WebEnvironment[] WebEnvironments => Enum.GetValues<WebEnvironment>();

        public bool UpdateCheckingEnabled
        {
            get => App.Settings.Prop.CheckForUpdates;
            set => App.Settings.Prop.CheckForUpdates = value;
        }

        public bool AnalyticsEnabled
        {
            get => App.Settings.Prop.EnableAnalytics;
            set => App.Settings.Prop.EnableAnalytics = value;
        }

        public WebEnvironment WebEnvironment
        {
            get => App.Settings.Prop.WebEnvironment;
            set => App.Settings.Prop.WebEnvironment = value;
        }

        public Visibility WebEnvironmentVisibility => App.Settings.Prop.DeveloperMode ? Visibility.Visible : Visibility.Collapsed;

        public bool ShouldExportConfig { get; set; } = true;

        public bool ShouldExportLogs { get; set; } = true;

        public ICommand ExportDataCommand => new RelayCommand(ExportData);

        private void ExportData()
        {
            string timestamp = DateTime.UtcNow.ToString("yyyyMMdd'T'HHmmss'Z'");

            var dialog = new SaveFileDialog 
            { 
                FileName = $"Fluxstrap-export-{timestamp}.zip",
                Filter = $"{Strings.FileTypes_ZipArchive}|*.zip" 
            };

            if (dialog.ShowDialog() != true)
                return;

            using var memStream = new MemoryStream();
            using var zipStream = new ZipOutputStream(memStream);

            if (ShouldExportConfig)
            {
                var files = new List<string>()
                {
                    App.Settings.FileLocation,
                    App.State.FileLocation,
                    App.FastFlags.FileLocation
                };

                AddFilesToZipStream(zipStream, files, "Config/");
            }

            if (ShouldExportLogs && Directory.Exists(Paths.Logs))
            {
                var files = Directory.GetFiles(Paths.Logs)
                    .Where(x => !x.Equals(App.Logger.FileLocation, StringComparison.OrdinalIgnoreCase));

                AddFilesToZipStream(zipStream, files, "Logs/");
            }

            zipStream.CloseEntry();
            zipStream.Finish();
            memStream.Position = 0;

            using var outputStream = File.OpenWrite(dialog.FileName);
            memStream.CopyTo(outputStream);

            Process.Start("explorer.exe", $"/select,\"{dialog.FileName}\"");
        }

        private void AddFilesToZipStream(ZipOutputStream zipStream, IEnumerable<string> files, string directory)
        {
            const string LOG_IDENT = "FluxstrapViewModel::AddFilesToZipStream";

            foreach (string file in files)
            {
                if (!File.Exists(file))
                    continue;

                try
                {
                    using FileStream fileStream = File.OpenRead(file);

                    var entry = new ZipEntry(directory + Path.GetFileName(file));
                    entry.DateTime = DateTime.Now;

                    zipStream.PutNextEntry(entry);

                    fileStream.CopyTo(zipStream);
                }
                catch (IOException ex)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Failed to open '{file}'");
                    App.Logger.WriteException(LOG_IDENT, ex);
                }
            }
        }

        public ICommand CreateBackupCommand => new RelayCommand(CreateBackup);

        private void CreateBackup()
        {
            string timestamp = DateTime.UtcNow.ToString("yyyyMMdd'T'HHmmss'Z'");

            var dialog = new SaveFileDialog
            {
                FileName = $"Fluxstrap-backup-{timestamp}.zip",
                Filter = $"{Strings.FileTypes_ZipArchive}|*.zip"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                BackupManager.CreateBackup(dialog.FileName);
                Frontend.ShowMessageBox(Strings.Menu_Fluxstrap_Backup_Success, MessageBoxImage.Information);
                Process.Start("explorer.exe", $"/select,\"{dialog.FileName}\"");
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("FluxstrapViewModel::CreateBackup", ex);
                Frontend.ShowMessageBox(Strings.Menu_Fluxstrap_Backup_Failed, MessageBoxImage.Error);
            }
        }

        public ICommand RestoreBackupCommand => new RelayCommand(RestoreBackup);

        private void RestoreBackup()
        {
            var dialog = new OpenFileDialog
            {
                Filter = $"{Strings.FileTypes_ZipArchive}|*.zip"
            };

            if (dialog.ShowDialog() != true)
                return;

            var result = Frontend.ShowMessageBox(Strings.Menu_Fluxstrap_Restore_Confirm, MessageBoxImage.Question, MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                BackupManager.RestoreBackup(dialog.FileName);
                Frontend.ShowMessageBox(Strings.Menu_Fluxstrap_Restore_Success, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("FluxstrapViewModel::RestoreBackup", ex);
                Frontend.ShowMessageBox(Strings.Menu_Fluxstrap_Restore_Failed, MessageBoxImage.Error);
            }
        }

        public ICommand KillRobloxCommand => new RelayCommand(KillRoblox);

        private void KillRoblox()
        {
            const string LOG_IDENT = "FluxstrapViewModel::KillRoblox";

            foreach (string processName in new[] { "RobloxPlayerBeta", "RobloxStudioBeta", "Roblox" })
            {
                try
                {
                    foreach (var process in Process.GetProcessesByName(processName))
                    {
                        process.Kill();
                        process.WaitForExit(5000);
                        App.Logger.WriteLine(LOG_IDENT, $"Killed process {processName} (PID: {process.Id})");
                    }
                }
                catch (Exception ex)
                {
                    App.Logger.WriteException(LOG_IDENT, ex);
                }
            }

            Frontend.ShowMessageBox(Strings.Menu_Fluxstrap_KillRoblox_Success, MessageBoxImage.Information);
        }

        public ICommand ClearRobloxCacheCommand => new RelayCommand(ClearRobloxCache);

        private void ClearRobloxCache()
        {
            const string LOG_IDENT = "FluxstrapViewModel::ClearRobloxCache";

            try
            {
                string[] cacheDirs = { Paths.RobloxHttpCache, Paths.RobloxCache };

                foreach (string dir in cacheDirs)
                {
                    if (!Directory.Exists(dir))
                        continue;

                    foreach (string file in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
                    {
                        try { File.Delete(file); }
                        catch (Exception ex) { App.Logger.WriteException(LOG_IDENT, ex); }
                    }

                    foreach (string subDir in Directory.GetDirectories(dir))
                    {
                        try { Directory.Delete(subDir, true); }
                        catch (Exception ex) { App.Logger.WriteException(LOG_IDENT, ex); }
                    }
                }

                if (Directory.Exists(Paths.RobloxLogs))
                {
                    foreach (string logFile in Directory.GetFiles(Paths.RobloxLogs, "*.log"))
                    {
                        try { File.Delete(logFile); }
                        catch { }
                    }
                }

                Frontend.ShowMessageBox(Strings.Menu_Fluxstrap_ClearCache_Success, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
                Frontend.ShowMessageBox(Strings.Menu_Fluxstrap_ClearCache_Failed, MessageBoxImage.Error);
            }
        }

        public ICommand OpenLogsFolderCommand => new RelayCommand(() => Process.Start("explorer.exe", Paths.Logs));

        public ICommand OpenModsFolderCommand => new RelayCommand(() => Process.Start("explorer.exe", Paths.Modifications));

        public ICommand OpenSettingsFolderCommand => new RelayCommand(() => Process.Start("explorer.exe", Paths.Base));
    }
}

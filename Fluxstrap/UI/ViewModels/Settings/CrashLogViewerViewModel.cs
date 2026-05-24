using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace Fluxstrap.UI.ViewModels.Settings
{
    public class CrashLogEntry
    {
        public string FileName { get; set; } = "";
        public string FullPath { get; set; } = "";
        public string DateFormatted { get; set; } = "";
        public string SizeFormatted { get; set; } = "";
        public long SizeBytes { get; set; }
        public string Preview { get; set; } = "";
        public bool IsSelected { get; set; }
    }

    public class CrashLogViewerViewModel : NotifyPropertyChangedViewModel
    {
        private bool _isLoading = true;
        private string _totalSize = "";
        private CrashLogEntry? _selectedLog;
        private string _logContent = "";
        private bool _showContent;

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(nameof(IsLoading)); OnPropertyChanged(nameof(ShowList)); }
        }

        public string TotalSize
        {
            get => _totalSize;
            set { _totalSize = value; OnPropertyChanged(nameof(TotalSize)); }
        }

        public CrashLogEntry? SelectedLog
        {
            get => _selectedLog;
            set
            {
                _selectedLog = value;
                OnPropertyChanged(nameof(SelectedLog));
                if (value is not null)
                    LoadLogContent(value);
            }
        }

        public string LogContent
        {
            get => _logContent;
            set { _logContent = value; OnPropertyChanged(nameof(LogContent)); }
        }

        public bool ShowContent
        {
            get => _showContent;
            set { _showContent = value; OnPropertyChanged(nameof(ShowContent)); OnPropertyChanged(nameof(ShowLogViewer)); }
        }

        public bool ShowList => !IsLoading;
        public bool ShowLogViewer => ShowContent && SelectedLog is not null;
        public bool HasLogs => LogFiles.Count > 0;

        public ObservableCollection<CrashLogEntry> LogFiles { get; } = new();

        public ICommand RefreshCommand => new RelayCommand(LoadCrashLogs);
        public ICommand DeleteAllCommand => new RelayCommand(DeleteAllLogs);
        public ICommand OpenCrashFolderCommand => new RelayCommand(() => Process.Start("explorer.exe", Paths.RobloxCrashLogs));
        public ICommand CloseLogViewerCommand => new RelayCommand(() => { ShowContent = false; SelectedLog = null; });

        public CrashLogViewerViewModel()
        {
            LoadCrashLogs();
        }

        public void LoadCrashLogs()
        {
            IsLoading = true;
            LogFiles.Clear();
            ShowContent = false;
            SelectedLog = null;

            string crashDir = Paths.RobloxCrashLogs;
            if (!Directory.Exists(crashDir))
            {
                IsLoading = false;
                OnPropertyChanged(nameof(HasLogs));
                return;
            }

            long totalBytes = 0;

            try
            {
                foreach (string file in Directory.GetFiles(crashDir, "*.log", SearchOption.TopDirectoryOnly)
                    .OrderByDescending(f => File.GetLastWriteTime(f)))
                {
                    try
                    {
                        var fi = new FileInfo(file);
                        totalBytes += fi.Length;

                        string preview = "";
                        try
                        {
                            using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                            using var reader = new StreamReader(fs);
                            string? firstLine = reader.ReadLine();
                            if (!string.IsNullOrEmpty(firstLine) && firstLine.Length > 120)
                                firstLine = firstLine[..120] + "...";
                            preview = firstLine ?? "";
                        }
                        catch { }

                        LogFiles.Add(new CrashLogEntry
                        {
                            FileName = Path.GetFileName(file),
                            FullPath = file,
                            DateFormatted = fi.LastWriteTime.ToString("yyyy-MM-dd HH:mm"),
                            SizeBytes = fi.Length,
                            SizeFormatted = FormatBytes(fi.Length),
                            Preview = preview
                        });
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("CrashLogViewerViewModel::LoadCrashLogs", ex);
            }

            TotalSize = FormatBytes(totalBytes);
            OnPropertyChanged(nameof(HasLogs));
            IsLoading = false;
        }

        private void LoadLogContent(CrashLogEntry entry)
        {
            try
            {
                using var fs = new FileStream(entry.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(fs);
                string content = reader.ReadToEnd();
                LogContent = content.Length > 50000 ? content[..50000] + "\n\n" + Strings.Menu_CrashLogs_Truncated : content;
                ShowContent = true;
            }
            catch (Exception ex)
            {
                LogContent = string.Format(Strings.Menu_CrashLogs_ErrorReadingFile, ex.Message);
                ShowContent = true;
            }
        }

        private void DeleteAllLogs()
        {
            var result = Frontend.ShowMessageBox(Strings.Menu_CrashLogs_DeleteAllConfirm, MessageBoxImage.Warning, MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes)
                return;

            foreach (var entry in LogFiles.ToList())
            {
                try
                {
                    File.Delete(entry.FullPath);
                    LogFiles.Remove(entry);
                }
                catch (Exception ex)
                {
                    App.Logger.WriteException("CrashLogViewerViewModel::DeleteAllLogs", ex);
                }
            }

            ShowContent = false;
            SelectedLog = null;
            TotalSize = $"0 {Strings.Common_B}";
            OnPropertyChanged(nameof(HasLogs));
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes >= 1048576)
                return $"{bytes / 1048576.0:F1} {Strings.Common_MB}";
            if (bytes >= 1024)
                return $"{bytes / 1024.0:F0} {Strings.Common_KB}";
            return $"{bytes} {Strings.Common_B}";
        }
    }
}

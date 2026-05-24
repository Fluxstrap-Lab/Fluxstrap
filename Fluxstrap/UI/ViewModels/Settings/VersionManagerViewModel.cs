using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace Fluxstrap.UI.ViewModels.Settings
{
    public class VersionEntry
    {
        public string VersionGuid { get; set; } = "";
        public string BinaryType { get; set; } = "";
        public string InstalledDate { get; set; } = "";
        public string SizeFormatted { get; set; } = "";
        public long SizeBytes { get; set; }
        public bool IsActive { get; set; }
    }

    public class VersionManagerViewModel : NotifyPropertyChangedViewModel
    {
        private bool _isLoading = true;
        private string _totalSize = "";

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(nameof(IsLoading)); OnPropertyChanged(nameof(ShowContent)); }
        }

        public string TotalSize
        {
            get => _totalSize;
            set { _totalSize = value; OnPropertyChanged(nameof(TotalSize)); }
        }

        public bool ShowContent => !IsLoading;
        public bool HasVersions => Versions.Count > 0;

        public ObservableCollection<VersionEntry> Versions { get; } = new();

        public ICommand RefreshCommand => new RelayCommand(LoadVersions);
        public ICommand OpenVersionsFolderCommand => new RelayCommand(() => Process.Start("explorer.exe", Paths.Versions));
        public ICommand DeleteOldVersionsCommand => new RelayCommand(DeleteOldVersions);

        public VersionManagerViewModel()
        {
            LoadVersions();
        }

        public void LoadVersions()
        {
            IsLoading = true;
            Versions.Clear();

            if (!Directory.Exists(Paths.Versions))
            {
                IsLoading = false;
                return;
            }

            var activeGuidPlayer = App.PlayerState.Prop.VersionGuid;
            var activeGuidStudio = App.StudioState.Prop.VersionGuid;

            long totalBytes = 0;

            foreach (string dir in Directory.GetDirectories(Paths.Versions))
            {
                try
                {
                    string guid = Path.GetFileName(dir);
                    long size = CalculateDirectorySize(dir);
                    totalBytes += size;

                    string binaryType = "Unknown";
                    if (File.Exists(Path.Combine(dir, "RobloxPlayerBeta.exe")))
                        binaryType = "Player";
                    if (File.Exists(Path.Combine(dir, "RobloxStudioBeta.exe")))
                        binaryType = binaryType == "Player" ? "Player + Studio" : "Studio";

                    bool isActive = guid.Equals(activeGuidPlayer, StringComparison.OrdinalIgnoreCase) ||
                                    guid.Equals(activeGuidStudio, StringComparison.OrdinalIgnoreCase);

                    Versions.Add(new VersionEntry
                    {
                        VersionGuid = guid,
                        BinaryType = binaryType,
                        InstalledDate = Directory.GetCreationTime(dir).ToString("yyyy-MM-dd"),
                        SizeBytes = size,
                        SizeFormatted = FormatBytes(size),
                        IsActive = isActive
                    });
                }
                catch { }
            }

            TotalSize = FormatBytes(totalBytes);
            OnPropertyChanged(nameof(HasVersions));
            IsLoading = false;
        }

        private void DeleteOldVersions()
        {
            var result = Frontend.ShowMessageBox(Strings.Menu_VersionManager_DeleteConfirm, MessageBoxImage.Warning, MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes)
                return;

            int deleted = 0;
            var toRemove = Versions.Where(v => !v.IsActive).ToList();

            foreach (var version in toRemove)
            {
                try
                {
                    string dir = Path.Combine(Paths.Versions, version.VersionGuid);
                    if (Directory.Exists(dir))
                    {
                        Directory.Delete(dir, true);
                        Versions.Remove(version);
                        deleted++;
                    }
                }
                catch (Exception ex)
                {
                    App.Logger.WriteException("VersionManagerViewModel::DeleteOldVersions", ex);
                }
            }

            TotalSize = FormatBytes(Versions.Sum(v => v.SizeBytes));
            OnPropertyChanged(nameof(HasVersions));

            Frontend.ShowMessageBox($"{deleted} old version(s) deleted.", MessageBoxImage.Information);
        }

        private static long CalculateDirectorySize(string path)
        {
            long size = 0;
            try
            {
                foreach (string file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                {
                    try { size += new FileInfo(file).Length; }
                    catch { }
                }
            }
            catch { }
            return size;
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes >= 1073741824)
                return $"{bytes / 1073741824.0:F1} GB";
            if (bytes >= 1048576)
                return $"{bytes / 1048576.0:F0} MB";
            if (bytes >= 1024)
                return $"{bytes / 1024.0:F0} KB";
            return $"{bytes} B";
        }
    }
}

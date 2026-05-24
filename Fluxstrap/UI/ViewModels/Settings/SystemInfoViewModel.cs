using System.Collections.ObjectModel;
using System.Management;
using System.Runtime.InteropServices;
using Fluxstrap.AppData;
using Fluxstrap.RobloxInterfaces;

namespace Fluxstrap.UI.ViewModels.Settings
{
    public class SystemInfoEntry
    {
        public string Label { get; set; } = "";
        public string Value { get; set; } = "";
    }

    public class SystemInfoViewModel : NotifyPropertyChangedViewModel
    {
        public ObservableCollection<SystemInfoEntry> SystemEntries { get; } = new();
        public ObservableCollection<SystemInfoEntry> RobloxEntries { get; } = new();

        public SystemInfoViewModel()
        {
            LoadSystemInfo();
            LoadRobloxInfo();
        }

        private void LoadSystemInfo()
        {
            SystemEntries.Add(new() { Label = Strings.Menu_SystemInfo_OS, Value = GetOSInfo() });
            SystemEntries.Add(new() { Label = Strings.Menu_SystemInfo_Architecture, Value = Environment.Is64BitOperatingSystem ? Strings.Menu_SystemInfo_Bit64 : Strings.Menu_SystemInfo_Bit32 });
            SystemEntries.Add(new() { Label = Strings.Menu_SystemInfo_CPUCores, Value = $"{Environment.ProcessorCount} {Strings.Menu_SystemInfo_Cores}" });
            SystemEntries.Add(new() { Label = Strings.Menu_SystemInfo_RAM, Value = GetRAMInfo() });
            SystemEntries.Add(new() { Label = Strings.Menu_SystemInfo_GPU, Value = GetGPUInfo() });
            SystemEntries.Add(new() { Label = Strings.Menu_SystemInfo_DotNetRuntime, Value = RuntimeInformation.FrameworkDescription });
            SystemEntries.Add(new() { Label = Strings.Menu_SystemInfo_MachineName, Value = Environment.MachineName });
            SystemEntries.Add(new() { Label = Strings.Menu_SystemInfo_UserName, Value = Environment.UserName });
        }

        private void LoadRobloxInfo()
        {
            bool installed = App.IsPlayerInstalled;
            string version = installed ? Utilities.GetRobloxVersionStr(false) : Strings.Menu_SystemInfo_NotInstalled;
            string channel = Deployment.Channel;
            string versionGuid = installed ? App.PlayerState.Prop.VersionGuid : "-";
            string installDir = installed ? new RobloxPlayerData().Directory : "-";

            RobloxEntries.Add(new() { Label = Strings.Menu_SystemInfo_RobloxVersion, Value = string.IsNullOrEmpty(version) ? versionGuid : version });
            RobloxEntries.Add(new() { Label = Strings.Menu_SystemInfo_Channel, Value = channel });
            RobloxEntries.Add(new() { Label = Strings.Menu_SystemInfo_InstallLocation, Value = installDir });
            RobloxEntries.Add(new() { Label = Strings.Menu_SystemInfo_LogsLocation, Value = Paths.RobloxLogs });
            RobloxEntries.Add(new() { Label = Strings.Menu_SystemInfo_CrashLogsLocation, Value = Paths.RobloxCrashLogs });
            RobloxEntries.Add(new() { Label = Strings.Menu_SystemInfo_VersionsFolder, Value = Paths.Versions });
        }

        private static string GetOSInfo()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Caption, Version FROM Win32_OperatingSystem");
                foreach (ManagementObject os in searcher.Get())
                {
                    string? name = os["Caption"]?.ToString()?.Trim();
                    if (!string.IsNullOrEmpty(name))
                        return name;
                }
            }
            catch { }

            return Environment.OSVersion.ToString();
        }

        private static string GetRAMInfo()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
                foreach (ManagementObject mo in searcher.Get())
                {
                    ulong? bytes = mo["TotalPhysicalMemory"] as ulong?;
                    if (bytes.HasValue)
                    {
                        double gb = bytes.Value / 1073741824.0;
                        return $"{gb:F2} {Strings.Menu_SystemInfo_GB}";
                    }
                }
            }
            catch { }

            return Common_Unknown;
        }

        private static string GetGPUInfo()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Name, AdapterRAM FROM Win32_VideoController");
                foreach (ManagementObject gpu in searcher.Get())
                {
                    string? name = gpu["Name"]?.ToString()?.Trim();
                    if (!string.IsNullOrEmpty(name))
                    {
                        uint? ram = gpu["AdapterRAM"] as uint?;
                        if (ram.HasValue && ram.Value > 0)
                        {
                            double gb = ram.Value / 1073741824.0;
                            return $"{name} ({gb:F2} {Strings.Menu_SystemInfo_GB})";
                        }
                        return name;
                    }
                }
            }
            catch { }

            return Common_Unknown;
        }

        private static string Common_Unknown => Strings.Common_Unknown;
    }
}

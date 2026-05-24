using System.ComponentModel;
using System.IO.Compression;

namespace Fluxstrap
{
    public enum GoodbyeDPIStatus
    {
        Stopped,
        Downloading,
        Starting,
        Running,
        Stopping,
        Error
    }

    public static class GoodbyeDPIManager
    {
        private const string LOG_IDENT = "GoodbyeDPIManager";
        private const string GoodbyeDPIUrl = "https://github.com/cagritaskn/GoodbyeDPI-Turkey/releases/download/release-0.2.3rc3-turkey/goodbyedpi-0.2.3rc3-turkey.zip";

        private static Process? _currentProcess;
        private static GoodbyeDPIStatus _status = GoodbyeDPIStatus.Stopped;
        private static readonly StringBuilder _logBuffer = new();
        private static readonly object _lock = new();

        public static string BasePath => Path.Combine(Paths.Integrations, "GoodbyeDPI");
        private static string ExePath => Path.Combine(BasePath, "goodbyedpi.exe");
        private static string CmdPath => Path.Combine(BasePath, "turkey_dnsredir.cmd");

        public static bool IsDownloaded => File.Exists(ExePath) || File.Exists(CmdPath);

        public static GoodbyeDPIStatus Status
        {
            get => _status;
            private set
            {
                _status = value;
                StatusChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static string Log => _logBuffer.ToString();

        public static event EventHandler? StatusChanged;
        public static event EventHandler<string>? LogReceived;

        public static async Task EnsureDownloaded()
        {
            if (IsDownloaded)
                return;

            Status = GoodbyeDPIStatus.Downloading;
            AppendLog("Downloading GoodbyeDPI...");

            Directory.CreateDirectory(BasePath);
            string zipPath = Path.Combine(BasePath, "goodbyedpi.zip");

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                var response = await App.HttpClient.GetAsync(GoodbyeDPIUrl, cts.Token);
                response.EnsureSuccessStatusCode();

                using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await response.Content.CopyToAsync(fs, cts.Token);
                    await fs.FlushAsync(cts.Token);
                }

                AppendLog("Download complete, extracting...");
                ZipFile.ExtractToDirectory(zipPath, BasePath, overwriteFiles: true);

                try { File.Delete(zipPath); }
                catch { AppendLog("Could not delete zip (will be cleaned up later)"); }

                if (!IsDownloaded)
                {
                    string? exe = Directory.GetFiles(BasePath, "goodbyedpi.exe", SearchOption.AllDirectories).FirstOrDefault();
                    if (exe is not null && exe != ExePath)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(ExePath)!);
                        File.Copy(exe, ExePath, overwrite: true);
                        AppendLog($"Copied goodbyedpi.exe from {exe}");
                    }

                    string? cmd = Directory.GetFiles(BasePath, "turkey_dnsredir.cmd", SearchOption.AllDirectories).FirstOrDefault();
                    if (cmd is not null && cmd != CmdPath)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(CmdPath)!);
                        File.Copy(cmd, CmdPath, overwrite: true);
                        AppendLog($"Copied turkey_dnsredir.cmd from {cmd}");
                    }
                }

                AppendLog($"Extraction done. goodbyedpi.exe exists: {File.Exists(ExePath)}");
                AppendLog($"turkey_dnsredir.cmd exists: {File.Exists(CmdPath)}");

                if (Status == GoodbyeDPIStatus.Downloading)
                    Status = GoodbyeDPIStatus.Stopped;
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
                if (File.Exists(zipPath))
                    File.Delete(zipPath);
                AppendLog($"Download failed: {ex.Message}");
                Status = GoodbyeDPIStatus.Error;
                throw;
            }
        }

        public static void Start()
        {
            if (!IsDownloaded)
            {
                AppendLog("Cannot start: GoodbyeDPI not downloaded");
                return;
            }

            lock (_lock)
            {
                if (_status == GoodbyeDPIStatus.Running || _status == GoodbyeDPIStatus.Starting)
                {
                    AppendLog("Already running");
                    return;
                }
            }

            Status = GoodbyeDPIStatus.Starting;
            AppendLog("Starting GoodbyeDPI...");

            try
            {
                string args = GetLaunchArgs();

                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"{CmdPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = BasePath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    Verb = "runas"
                };

                try
                {
                    _currentProcess = new Process { StartInfo = psi };
                    _currentProcess.OutputDataReceived += (_, e) => { if (e.Data is not null) AppendLog(e.Data); };
                    _currentProcess.ErrorDataReceived += (_, e) => { if (e.Data is not null) AppendLog("[ERR] " + e.Data); };
                    _currentProcess.Exited += (_, _) =>
                    {
                        AppendLog("GoodbyeDPI process exited");
                        _currentProcess?.Dispose();
                        _currentProcess = null;

                        bool wasRunning = false;
                        lock (_lock) { wasRunning = _status == GoodbyeDPIStatus.Running; }

                        if (wasRunning)
                        {
                            Status = GoodbyeDPIStatus.Stopped;
                        }
                    };
                    _currentProcess.EnableRaisingEvents = true;

                    if (!_currentProcess.Start())
                    {
                        AppendLog("Failed to start process (no error)");
                        Status = GoodbyeDPIStatus.Error;
                        return;
                    }

                    _currentProcess.BeginOutputReadLine();
                    _currentProcess.BeginErrorReadLine();

                    AppendLog($"GoodbyeDPI started (PID: {_currentProcess.Id})");
                    Status = GoodbyeDPIStatus.Running;
                }
                catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
                {
                    AppendLog("UAC cancelled by user");
                    Status = GoodbyeDPIStatus.Stopped;
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
                AppendLog($"Failed to start: {ex.Message}");
                Status = GoodbyeDPIStatus.Error;
            }
        }

        public static void Stop()
        {
            lock (_lock)
            {
                if (_status != GoodbyeDPIStatus.Running)
                {
                    AppendLog("Not running, nothing to stop");
                    return;
                }
            }

            Status = GoodbyeDPIStatus.Stopping;
            AppendLog("Stopping GoodbyeDPI...");

            try
            {
                if (_currentProcess is not null && !_currentProcess.HasExited)
                {
                    _currentProcess.Kill(entireProcessTree: true);
                    if (!_currentProcess.WaitForExit(5000))
                    {
                        AppendLog("Process did not exit in time, forcing...");
                        _currentProcess.Kill(entireProcessTree: true);
                    }
                    AppendLog("GoodbyeDPI stopped");
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }

            KillAllGoodbyeDPInstances();
            Status = GoodbyeDPIStatus.Stopped;
        }

        public static void StopAll()
        {
            Stop();
            KillAllGoodbyeDPInstances();
        }

        private static void KillAllGoodbyeDPInstances()
        {
            try
            {
                foreach (var process in Process.GetProcessesByName("goodbyedpi"))
                {
                    try
                    {
                        process.Kill(entireProcessTree: true);
                        process.WaitForExit(3000);
                        AppendLog($"Killed goodbyedpi.exe (PID: {process.Id})");
                    }
                    catch (Exception ex)
                    {
                        App.Logger.WriteException(LOG_IDENT, ex);
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        private static string GetLaunchArgs()
        {
            return $"/c \"{CmdPath}\"";
        }

        private static void AppendLog(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string line = $"[{timestamp}] {message}";

            lock (_lock)
            {
                if (_logBuffer.Length > 10000)
                {
                    string rest = _logBuffer.ToString(_logBuffer.Length - 8000, 8000);
                    _logBuffer.Clear();
                    _logBuffer.Append(rest);
                }
                _logBuffer.AppendLine(line);
            }

            App.Logger.WriteLine(LOG_IDENT, message);
            LogReceived?.Invoke(null, line);
        }
    }
}

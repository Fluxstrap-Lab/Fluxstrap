using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Fluxstrap
{
    public static class RobloxMemoryCleaner
    {
        [DllImport("psapi.dll")]
        private static extern bool EmptyWorkingSet(IntPtr hProcess);

        [DllImport("kernel32.dll")]
        private static extern bool SetProcessWorkingSetSize(
            IntPtr hProcess,
            IntPtr dwMinimumWorkingSetSize,
            IntPtr dwMaximumWorkingSetSize);

        [DllImport("kernel32.dll")]
        private static extern bool SetPriorityClass(
            IntPtr hProcess,
            uint dwPriorityClass);

        private const uint BELOW_NORMAL_PRIORITY_CLASS = 0x00004000;
        private const uint IDLE_PRIORITY_CLASS = 0x00000040;

        private static readonly string[] RobloxProcesses =
        {
            "RobloxPlayerBeta",
            "RobloxPlayer",
            "Roblox",
            "RobloxStudioBeta"
        };

        public static void OptimizeProcessMemory(int processId)
        {
            try
            {
                using var proc = Process.GetProcessById(processId);
                EmptyWorkingSet(proc.Handle);
                SetProcessWorkingSetSize(proc.Handle, (IntPtr)(-1), (IntPtr)(-1));
                SetPriorityClass(proc.Handle, BELOW_NORMAL_PRIORITY_CLASS);

                App.Logger.WriteLine("RobloxMemoryCleaner::OptimizeProcessMemory",
                    $"Optimized memory for PID {processId}");
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine("RobloxMemoryCleaner::OptimizeProcessMemory",
                    $"Failed to optimize PID {processId}: {ex.Message}");
            }
        }

        public static void CleanAllRobloxMemory()
        {
            var processes = Process.GetProcesses()
                .Where(p => RobloxProcesses.Contains(p.ProcessName))
                .ToArray();

            if (processes.Length == 0)
                return;

            foreach (var proc in processes)
            {
                try
                {
                    EmptyWorkingSet(proc.Handle);
                    SetProcessWorkingSetSize(proc.Handle, (IntPtr)(-1), (IntPtr)(-1));
                    SetPriorityClass(proc.Handle, BELOW_NORMAL_PRIORITY_CLASS);
                }
                finally
                {
                    proc.Dispose();
                }
            }

            App.Logger.WriteLine("RobloxMemoryCleaner::CleanAllRobloxMemory",
                $"Cleaned memory for {processes.Length} Roblox process(es)");
        }
    }
}

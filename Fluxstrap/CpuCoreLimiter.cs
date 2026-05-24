using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Fluxstrap
{
    public static class CpuCoreLimiter
    {
        public static void SetProcessCpuCoreLimit(int processId, int coreCount)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return;

            int maxCores = Environment.ProcessorCount;
            if (coreCount < 1)
                coreCount = 1;
            else if (coreCount > maxCores)
                coreCount = maxCores;

            try
            {
                long affinityMask = 0;
                for (int i = 0; i < coreCount; i++)
                    affinityMask |= 1L << i;

                using var process = Process.GetProcessById(processId);
                process.ProcessorAffinity = (IntPtr)affinityMask;

                App.Logger.WriteLine("CpuCoreLimiter::SetProcessCpuCoreLimit",
                    $"CPU affinity for PID {processId} set to {coreCount} core(s)");
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine("CpuCoreLimiter::SetProcessCpuCoreLimit", $"Failed to set CPU affinity for PID {processId}");
                App.Logger.WriteException("CpuCoreLimiter::SetProcessCpuCoreLimit", ex);
            }
        }
    }
}

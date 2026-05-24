using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Fluxstrap.Utility
{
    public static class AggressivePerformanceManager
    {
        public static void SetTimerResolution()
        {
            timeBeginPeriod(1);
        }

        public static void EnableHighPerformance()
        {
            try
            {
                using var process = Process.GetCurrentProcess();
                process.PriorityClass = ProcessPriorityClass.High;
            }
            catch
            {
            }

            try
            {
                using var power = new PowerMode();
            }
            catch
            {
            }
        }

        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
        private static extern void timeBeginPeriod(int period);

        private class PowerMode : IDisposable
        {
            private IntPtr _ptr;

            public PowerMode()
            {
                var ctx = new PowerRequestContext
                {
                    Version = POWER_REQUEST_CONTEXT_VERSION,
                    Flags = POWER_REQUEST_CONTEXT_SIMPLE_STRING,
                    SimpleReasonString = "Fluxstrap performance mode"
                };

                _ptr = PowerCreateRequest(ref ctx);
                if (_ptr != IntPtr.Zero)
                {
                    PowerSetRequest(_ptr, PowerRequestType.PowerRequestExecutionRequired);
                }
            }

            public void Dispose()
            {
                if (_ptr != IntPtr.Zero)
                {
                    PowerClearRequest(_ptr, PowerRequestType.PowerRequestExecutionRequired);
                    CloseHandle(_ptr);
                    _ptr = IntPtr.Zero;
                }
            }

            private const int POWER_REQUEST_CONTEXT_VERSION = 0;
            private const int POWER_REQUEST_CONTEXT_SIMPLE_STRING = 0x1;

            [StructLayout(LayoutKind.Sequential)]
            private struct PowerRequestContext
            {
                public int Version;
                public int Flags;
                [MarshalAs(UnmanagedType.LPWStr)]
                public string SimpleReasonString;
            }

            private enum PowerRequestType
            {
                PowerRequestExecutionRequired = 0
            }

            [DllImport("kernel32.dll")]
            private static extern IntPtr PowerCreateRequest(ref PowerRequestContext Context);

            [DllImport("kernel32.dll")]
            private static extern bool PowerSetRequest(IntPtr PowerRequest, PowerRequestType RequestType);

            [DllImport("kernel32.dll")]
            private static extern bool PowerClearRequest(IntPtr PowerRequest, PowerRequestType RequestType);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool CloseHandle(IntPtr hObject);
        }
    }
}

using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace Fluxstrap
{
    public class HotkeyManager : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_NOREPEAT = 0x4000;

        private const int HOTKEY_REJOIN = 1;
        private const int HOTKEY_KILL = 2;
        private const int HOTKEY_TOGGLE_FPS = 3;

        private readonly IntPtr _hwnd;
        private readonly HwndSource _source;
        private bool _registered;

        public event EventHandler? RejoinLastServerRequested;
        public event EventHandler? KillRobloxRequested;
        public event EventHandler? ToggleFPSUnlockRequested;
        // Overlay hotkey (Shift+Tab) is handled directly by GameOverlayManager on its own HWND

        public HotkeyManager(IntPtr hwnd)
        {
            _hwnd = hwnd;
            _source = HwndSource.FromHwnd(hwnd);
            _source.AddHook(WndProc);
        }

        public void Register()
        {
            if (_registered) return;

            RegisterHotKey(_hwnd, HOTKEY_REJOIN, MOD_CONTROL | MOD_SHIFT, 0x52);      // Ctrl+Shift+R
            RegisterHotKey(_hwnd, HOTKEY_KILL, MOD_CONTROL | MOD_SHIFT, 0x4B);         // Ctrl+Shift+K
            RegisterHotKey(_hwnd, HOTKEY_TOGGLE_FPS, MOD_CONTROL | MOD_SHIFT, 0x46);   // Ctrl+Shift+F

            _registered = true;
        }

        public void Unregister()
        {
            if (!_registered) return;

            UnregisterHotKey(_hwnd, HOTKEY_REJOIN);
            UnregisterHotKey(_hwnd, HOTKEY_KILL);
            UnregisterHotKey(_hwnd, HOTKEY_TOGGLE_FPS);

            _registered = false;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;

            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                switch (id)
                {
                    case HOTKEY_REJOIN:
                        RejoinLastServerRequested?.Invoke(this, EventArgs.Empty);
                        handled = true;
                        break;
                    case HOTKEY_KILL:
                        KillRobloxRequested?.Invoke(this, EventArgs.Empty);
                        handled = true;
                        break;
                    case HOTKEY_TOGGLE_FPS:
                        ToggleFPSUnlockRequested?.Invoke(this, EventArgs.Empty);
                        handled = true;
                        break;
                }
            }

            return IntPtr.Zero;
        }

        public void Dispose()
        {
            Unregister();
            _source.RemoveHook(WndProc);
            GC.SuppressFinalize(this);
        }
    }
}

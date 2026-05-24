using System.Diagnostics;

namespace Fluxstrap
{
    public static class LuaScriptManager
    {
        private const string LOG_IDENT = "LuaScriptManager";

        public static void ExecuteScript()
        {
            try
            {
                if (!App.Settings.Prop.EnableLuaScripting)
                {
                    App.Logger.WriteLine(LOG_IDENT, "Lua scripting is disabled");
                    return;
                }

                string luaScriptPath = Path.Combine(Paths.Base, "autoexecute.lua");

                if (!File.Exists(luaScriptPath))
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Lua script not found at {luaScriptPath}");
                    return;
                }

                string luaScript = File.ReadAllText(luaScriptPath);

                if (string.IsNullOrWhiteSpace(luaScript))
                {
                    App.Logger.WriteLine(LOG_IDENT, "Lua script is empty");
                    return;
                }

                App.Logger.WriteLine(LOG_IDENT, "Lua script file found. NLua integration would execute here.");
                App.Logger.WriteLine(LOG_IDENT, "Install NLua package to enable Lua scripting.");
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, $"Error with Lua script: {ex.Message}");
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        public static void WaitForRobloxAndExecuteScript()
        {
            const string ROBLOX_PROCESS = "RobloxPlayerBeta";

            App.Logger.WriteLine(LOG_IDENT, "Waiting for Roblox to launch before executing Lua script...");

            try
            {
                for (int i = 0; i < 120; i++)
                {
                    var processes = Process.GetProcessesByName(ROBLOX_PROCESS);

                    foreach (var process in processes)
                    {
                        if (process.MainWindowHandle != IntPtr.Zero)
                        {
                            App.Logger.WriteLine(LOG_IDENT, $"Roblox process found (PID: {process.Id}), executing Lua script...");
                            ExecuteScript();
                            return;
                        }
                    }

                    Thread.Sleep(500);
                }

                App.Logger.WriteLine(LOG_IDENT, "Timed out waiting for Roblox to launch.");
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, $"Error waiting for Roblox: {ex.Message}");
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }
    }
}

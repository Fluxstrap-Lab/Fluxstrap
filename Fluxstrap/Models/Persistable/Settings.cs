using System.Collections.ObjectModel;
using Fluxstrap.Enums.FlagPresets;

namespace Fluxstrap.Models.Persistable
{
    public class Settings
    {
        // Fluxstrap configuration
        public BootstrapperStyle BootstrapperStyle { get; set; } = BootstrapperStyle.FluentDialog;
        public BootstrapperIcon BootstrapperIcon { get; set; } = BootstrapperIcon.IconFluxstrap;
        public string BootstrapperTitle { get; set; } = App.ProjectName;
        public string BootstrapperIconCustomLocation { get; set; } = "";
        public Theme Theme { get; set; } = Theme.Default;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool DeveloperMode { get; set; } = false;
        public bool CheckForUpdates { get; set; } = true;
        public bool ConfirmLaunches { get; set; } = false;
        public string Locale { get; set; } = "nil";
        public bool UseFastFlagManager { get; set; } = true;
        public bool WPFSoftwareRender { get; set; } = false;
        public bool EnableAnalytics { get; set; } = true;
        public bool BackgroundUpdatesEnabled { get; set; } = false;
        public bool DebugDisableVersionPackageCleanup { get; set; } = false;
        public string? SelectedCustomTheme { get; set; } = null;
        public WebEnvironment WebEnvironment { get; set; } = WebEnvironment.Production;
        public bool DisableSplashScreen { get; set; } = true;
        public bool ShouldExportConfig { get; set; } = true;
        public bool ShouldExportLogs { get; set; } = true;
        public bool HasLaunchedGame { get; set; } = false;
        public bool NotificationWindowShow { get; set; } = true;
        public bool BackgroundWindow { get; set; } = true;
        public bool LaunchOnStartup { get; set; } = false;

        // cleaner configuration
        public CleanerOptions CleanerOptions { get; set; } = CleanerOptions.Never;
        public ObservableCollection<string> CleanerDirectories { get; set; } = new();
        public int CleanRobloxNumber { get; set; } = 0;

        // crash configuration
        public bool DisableCrash { get; set; } = false;
        public bool AutoRelaunchOnCrash { get; set; } = false;

        // cursor customization
        public string ShiftlockCursorSelectedPath { get; set; } = "";
        public string ArrowCursorSelectedPath { get; set; } = "";
        public string ArrowFarCursorSelectedPath { get; set; } = "";
        public string IBeamCursorSelectedPath { get; set; } = "";
        public CursorType CursorType { get; set; } = CursorType.Default;

        // icon & game name
        public string UseCustomIcon { get; set; } = "";
        public string CustomGameName { get; set; } = "";

        // priority & status
        public string PriorityLimit { get; set; } = "Normal";
        public string SelectedStatus { get; set; } = "Gray";
        public string SelectedCpuPriority { get; set; } = "Automatic";
        public int CpuCoreLimit { get; set; } = Environment.ProcessorCount;

        // export
        public bool SmooothBARRyesirikikthxlucipook { get; set; } = false;

        // place id
        public bool UsePlaceId { get; set; } = false;
        public string PlaceId { get; set; } = "";

        // notification & ui
        public bool VoidNotify { get; set; } = true;
        public bool ServerPingCounter { get; set; } = false;
        public bool ShowServerDetailsUI { get; set; } = false;
        public bool EnableCustomStatusDisplay { get; set; } = true;
        public bool RenameClientToEuroTrucks2 { get; set; } = false;

        // overlay effects
        public bool SnowWOWSOCOOLWpfSnowbtw { get; set; } = false;
        public bool MotionBlurOverlay { get; set; } = false;

        // client & buffer
        public string ClientPath { get; set; } = "";
        public string BufferSizeKbte { get; set; } = "1024";
        public string BufferSizeKbtes { get; set; } = "2048";

        // skybox & font
        public string SkyboxName { get; set; } = "Default";
        public string FontName { get; set; } = "Default";
        public string LastServerSave { get; set; } = "112757576021097";
        public bool SkyBoxDataSending { get; set; } = false;
        public string CustomFontLocation { get; set; } = string.Empty;

        // rpc & hud
        public bool FFlagRPCDisplayer { get; set; } = true;
        public bool FPSCounter { get; set; } = false;
        public bool CurrentTimeDisplay { get; set; } = false;
        public bool ExclusiveFullscreen { get; set; } = false;
        public bool Crosshair { get; set; } = false;
        public bool LockDefault { get; set; } = false;
        public bool GameWIP { get; set; } = false;
        public bool ForceRobloxLanguage { get; set; } = true;
        public bool IngameChatDiscord { get; set; } = false;

        // visual toggles
        public bool OverClockCPU { get; set; } = false;
        public bool OverClockGPU { get; set; } = false;
        public bool GRADmentFR { get; set; } = false;
        public bool exitondissy { get; set; } = false;
        public bool ServerUptimeBetterBLOXcuzitsbetterXD { get; set; } = true;
        public bool Fullbright { get; set; } = false;
        public bool ConnectCloset { get; set; } = false;

        // download string
        public string DownloadingStringFormat { get; set; } = "Downloading {0} - {1}MB / {2}MB";

        // game info display
        public bool GameIconChecked { get; set; } = true;
        public bool ServerLocationGame { get; set; } = false;
        public bool GameNameChecked { get; set; } = true;
        public bool GameCreatorChecked { get; set; } = true;
        public bool GameStatusChecked { get; set; } = true;

        // integration configuration
        public bool EnableActivityTracking { get; set; } = true;
        public bool UseDiscordRichPresence { get; set; } = true;
        public bool HideRPCButtons { get; set; } = true;
        public bool ShowAccountOnRichPresence { get; set; } = false;
        public bool ShowServerDetails { get; set; } = false;
        public bool UseGoodbyeDPI { get; set; } = false;
        public ObservableCollection<CustomIntegration> CustomIntegrations { get; set; } = new();
        public bool MultiAccount { get; set; } = false;
        public bool VoidRPC { get; set; } = true;

        // overlays
        public bool OverlaysEnabled { get; set; } = false;
        public double Brightness { get; set; } = 50;

        // mod preset configuration
        public bool UseDisableAppPatch { get; set; } = false;

        // performance optimization
        public bool OptimizeRoblox { get; set; } = false;
        public int MaxCpuCores { get; set; } = 0;
        public bool MemoryCleaner { get; set; } = false;
        public int TotalLogicalCores { get; set; } = Environment.ProcessorCount;
        public int TotalPhysicalCores { get; set; } = Environment.ProcessorCount;

        // experimental features
        public bool UnlockFPS { get; set; } = false;
        public int FPSCap { get; set; } = 240;

        // graphics
        public Enums.FlagPresets.FastFlagPreset FastFlagPreset { get; set; } = Enums.FlagPresets.FastFlagPreset.Off;
        public bool ForceGraphicsQuality { get; set; } = false;
        public int GraphicsQualityLevel { get; set; } = 10;

        // cache maintenance
        public bool AutoCleanRobloxCache { get; set; } = false;

        // server region blacklist
        public bool EnableServerBlacklist { get; set; } = false;
        public ObservableCollection<string> BlockedServerRegions { get; set; } = new();

        // favorite places / quick launch
        public ObservableCollection<FavoritePlace> FavoritePlaces { get; set; } = new();

        // launch configuration
        public string CustomLaunchArgs { get; set; } = "";

        // roblox deployment
        public string Channel { get; set; } = "";
        public string ChannelHash { get; set; } = "";
        public bool IsChannelEnabled { get; set; } = false;
        public bool UpdateRoblox { get; set; } = true;

        // game launch
        public string LaunchGameID { get; set; } = "";
        public bool IsGameEnabled { get; set; } = false;
        public bool MatchUniverseId { get; set; } = true;
        public long? TargetUniverseId { get; set; }
        public bool IsBetterServersEnabled { get; set; } = false;

        // in-game resolution
        public ResolutionSetting? InGameResolution { get; set; }

        // advanced
        public bool ShowResourceMonitor { get; set; } = false;
        public bool EnableDevConsole { get; set; } = false;

        // anti-afk
        public bool AntiAFK { get; set; } = false;

        // auto-rejoin
        public bool AutoRejoinOnDisconnect { get; set; } = false;

        // per-game profiles
        public ObservableCollection<GameProfile> GameProfiles { get; set; } = new();

        // hotkeys
        public bool EnableGlobalHotkeys { get; set; } = false;

        // dnd
        public bool DNDDuringGameplay { get; set; } = false;

        // session tracking
        public bool TrackPlayTime { get; set; } = false;

        // auto-repair on crash loop
        public bool AutoRepairOnCrashLoop { get; set; } = false;

        // streaming mode
        public bool StreamingMode { get; set; } = false;

        // overlay
        public bool OverlayEnabled { get; set; } = false;
        public bool OverlayShowFps { get; set; } = true;
        public bool OverlayShowPing { get; set; } = true;
        public bool OverlayShowRam { get; set; } = true;
        public bool OverlayShowServer { get; set; } = true;
        public bool OverlayShowFriends { get; set; } = false;
        public bool OverlayShowCpu { get; set; } = true;
        public bool OverlayShowGpu { get; set; } = false;
        public bool OverlayShowVolume { get; set; } = true;
        public bool OverlayShowGraph { get; set; } = true;
        public string OverlayPosition { get; set; } = "TopRight";
        public double OverlayOpacity { get; set; } = 0.8;
        public string OverlayToggleHotkey { get; set; } = "Shift+Tab";
        public string OverlayAccentColor { get; set; } = "#00A2FF";

        // lua scripting
        public bool EnableLuaScripting { get; set; } = false;
    }

    public class ResolutionSetting
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int RefreshRate { get; set; }
    }
}

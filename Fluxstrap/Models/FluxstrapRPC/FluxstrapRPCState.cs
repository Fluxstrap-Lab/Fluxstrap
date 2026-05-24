namespace Fluxstrap.Models.FluxstrapRPC;

public class FluxstrapRPCState
{
    public bool AutoStartRpc { get; set; } = false;
    public string ApplicationId { get; set; } = "";
    public string CustomStatusText { get; set; } = "";
    public string CustomDetailsText { get; set; } = "";
    public string CustomLargeImageKey { get; set; } = "";
    public string CustomLargeImageText { get; set; } = "";
    public string CustomSmallImageKey { get; set; } = "";
    public string CustomSmallImageText { get; set; } = "";
    public bool ShowGameName { get; set; } = true;
    public bool ShowGameCreator { get; set; } = true;
    public bool ShowServerInfo { get; set; } = true;
    public bool ShowElapsedTime { get; set; } = true;
    public bool ShowPlayerCount { get; set; } = false;
    public string SelectedStatusPreset { get; set; } = "default";
}

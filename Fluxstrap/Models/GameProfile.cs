namespace Fluxstrap.Models
{
    public class GameProfile
    {
        public long UniverseId { get; set; }
        public string GameName { get; set; } = "";
        public bool UseCustomFpsCap { get; set; } = false;
        public int FpsCap { get; set; } = 240;
        public bool UseCustomGraphicsQuality { get; set; } = false;
        public int GraphicsQualityLevel { get; set; } = 10;
        public bool? DisableScaling { get; set; } = null;
        public string? MSAAPreset { get; set; } = null;
        public string? TextureQualityPreset { get; set; } = null;
    }
}

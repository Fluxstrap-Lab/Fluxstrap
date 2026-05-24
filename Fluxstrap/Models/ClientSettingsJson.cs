using System.Text.Json.Serialization;

namespace Fluxstrap.Models
{
    public class ClientSettingsJson
    {
        [JsonPropertyName("FFlagDebugGraphicsPriority")]
        public string? FFlagDebugGraphicsPriority { get; set; }

        [JsonPropertyName("FFlagDebugGraphicsDisableGL")]
        public string? FFlagDebugGraphicsDisableGL { get; set; }

        [JsonPropertyName("FFlagDebugGraphicsPreferD3D11")]
        public string? FFlagDebugGraphicsPreferD3D11 { get; set; }

        [JsonPropertyName("FFlagDebugGraphicsDisableD3D11")]
        public string? FFlagDebugGraphicsDisableD3D11 { get; set; }

        [JsonPropertyName("FFlagDebugGraphicsDisableVulkan")]
        public string? FFlagDebugGraphicsDisableVulkan { get; set; }

        [JsonPropertyName("FIntDebugGraphicsMSAASamples")]
        public string? FIntDebugGraphicsMSAASamples { get; set; }

        [JsonPropertyName("FFlagDebugDisableAnalytics")]
        public string? FFlagDebugDisableAnalytics { get; set; }

        [JsonPropertyName("FFlagDebugDisableTelemetry")]
        public string? FFlagDebugDisableTelemetry { get; set; }

        [JsonPropertyName("DFIntTaskSchedulerTargetFps")]
        public string? DFIntTaskSchedulerTargetFps { get; set; }
    }
}

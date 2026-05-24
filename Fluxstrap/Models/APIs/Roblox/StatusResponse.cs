namespace Fluxstrap.Models.APIs.Roblox
{
    public class StatuspageResponse
    {
        [JsonPropertyName("page")]
        public StatuspageInfo Page { get; set; } = new();

        [JsonPropertyName("components")]
        public List<StatuspageComponent> Components { get; set; } = new();
    }

    public class StatuspageInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("url")]
        public string Url { get; set; } = "";

        [JsonPropertyName("updated_at")]
        public string UpdatedAt { get; set; } = "";
    }

    public class StatuspageComponent
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("status")]
        public string Status { get; set; } = "";

        [JsonPropertyName("description")]
        public string Description { get; set; } = "";
    }
}

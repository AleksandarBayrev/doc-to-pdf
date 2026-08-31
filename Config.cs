using System.Text.Json.Serialization;

namespace DocToPdf
{
    public class Config
    {
        [JsonPropertyName("inputFolder")]
        public string InputFolder { get; init; } = "";
        [JsonPropertyName("outputFolder")]
        public string OutputFolder { get; init; } = "";
    }
}
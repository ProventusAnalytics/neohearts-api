using Newtonsoft.Json;

namespace NeoHearts_API.Models
{
    public class OrganizationModel
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("active")]
        public bool Active { get; set; }

        [JsonProperty("meta")]
        public Meta? Meta { get; set; }

        [JsonProperty("resourceType")]
        public string? ResourceType { get; set; }
    }

    public class Meta
    {
        [JsonProperty("lastUpdated")]
        public string LastUpdated { get; set; }

        [JsonProperty("profile")]
        public List<string> Profile { get; set; }

        [JsonProperty("versionId")]
        public string VersionId { get; set; }
    }
}

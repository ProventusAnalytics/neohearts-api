using Newtonsoft.Json;

public class Patient
{
    [JsonProperty("first_name")]
    public required string First_name { get; set; }

    [JsonProperty("last_name")]
    public required string Last_name { get; set; }

    [JsonProperty("gender")]
    public string Gender { get; set; } = "other";
}

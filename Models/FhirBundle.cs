using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

public class FhirBundle<T>
{
    [JsonProperty("entry")]
    public List<FhirEntry<T>> Entry { get; set; }
}

public class FhirEntry<T>
{
    [JsonProperty("resource")]
    public T Resource { get; set; }
}

public class Patient
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public List<HumanName> Name { get; set; }

    [JsonProperty("gender")]
    public string Gender { get; set; }

    [JsonProperty("birthDate")]
    public string BirthDate { get; set; }
}

public class HumanName
{
    [JsonProperty("family")]
    public string Family { get; set; }

    [JsonProperty("given")]
    public List<string> Given { get; set; }
}

public class Observation
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("category")]
    public List<CodeableConcept> Category { get; set; }

    [JsonProperty("code")]
    public CodeableConcept Code { get; set; }

    [JsonProperty("valueQuantity")]
    public Quantity ValueQuantity { get; set; }

    [JsonProperty("subject")]
    public ResourceReference Subject { get; set; }
}

public class CodeableConcept
{
    [JsonProperty("coding")]
    public List<Coding> Coding { get; set; }
}

public class Coding
{
    [JsonProperty("system")]
    public string System { get; set; }

    [JsonProperty("code")]
    public string Code { get; set; }

    [JsonProperty("display")]
    public string Display { get; set; }
}

public class Quantity
{
    [JsonProperty("value")]
    public decimal? Value { get; set; }

    [JsonProperty("unit")]
    public string Unit { get; set; }
}

public class ResourceReference
{
    [JsonProperty("reference")]
    public string Reference { get; set; }
}

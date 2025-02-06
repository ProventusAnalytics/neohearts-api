using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.CloudHealthcare.v1;
using Google.Apis.CloudHealthcare.v1.Data;
using Hl7.Fhir.Model;
using Hl7.Fhir.Model.CdsHooks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using NeoHearts_API.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[ApiController]
[Route("api/[controller]")]
public class FhirController : ControllerBase
{
    private readonly string fhirBaseUrl;
    private readonly HttpClient _httpClient;

    public FhirController()
    {
        _httpClient = new HttpClient();
        fhirBaseUrl =
            "https://healthcare.googleapis.com/v1/projects/neohearts-dev/locations/asia-south1/datasets/neohearts-fhir-dataset/fhirStores/neohearts-fhir-datastore/fhir";
    }

    private async System.Threading.Tasks.Task AuthenticateAsync()
    {
        //fetches credentials from gcloud CLI, service account, or environment variables.
        var credential = await GoogleCredential.GetApplicationDefaultAsync();
        //tells Google we need access to Google Cloud Healthcare API.
        credential = credential.CreateScoped("https://www.googleapis.com/auth/cloud-healthcare");
        //Retrieves an OAuth 2.0 token for authentication
        string token = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
        //This ensures every request sends the token for authentication.
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );
    }

    [HttpGet("patient")]
    public async Task<IActionResult> TestFhirConnection()
    {
        await AuthenticateAsync(); // Ensure authentication before request

        var response = await _httpClient.GetAsync($"{fhirBaseUrl}/Patient");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return Ok(content);
        }
        return StatusCode((int)response.StatusCode, response.ReasonPhrase);
    }

    /// <summary>
    /// Create a new FHIR Patient resource in Google Healthcare API
    /// </summary>
    [HttpPost("patient")]
    public async Task<IActionResult> CreatePatient(NewbornModel patient)
    {
        await AuthenticateAsync(); // Ensure authentication before request

        // ✅ Define the Patient resource (FHIR format)
        var patientData = new
        {
            resourceType = "Patient",
            identifier = new[]
            {
                new
                {
                    use = "official",
                    system = "http://example.com/newborn-id-system",
                    value = $"NB-{patient.Id}",
                },
            },
            name = new[] { new { family = patient.LastName, given = new[] { patient.FirstName } } },
            gender = patient.Sex,
            birthDate = patient.DOB.ToString("yyyy-MM-dd"),
        };

        // Return the formatted FHIR resource (for debugging or sending to FHIR server)
        return Ok(patientData);
    }

    [HttpPut("patient/{id}")]
    public async Task<IActionResult> UpdatePatient(string id)
    {
        await AuthenticateAsync();
        Console.WriteLine("The id is:" + id);
        Console.ReadLine();
        var patientData = new
        {
            resourceType = "Patient",
            id = id,
            identifier = new[]
            {
                new
                {
                    use = "official",
                    system = "http://example.com/newborn-id-system",
                    value = $"NB-{Guid.NewGuid()}",
                },
            },
            name = new[] { new { family = "newnewnew", given = new[] { "newnewnew" } } },
            gender = "female",
            birthDate = "2020-01-01",
        };

        var content = JsonConvert.SerializeObject(patientData);
        var stringcontent = new StringContent(content, Encoding.UTF8, "application/fhir+json");

        var res = await _httpClient.PutAsync($"{fhirBaseUrl}/Patient/{id}", stringcontent);
        return Ok(new { message = "Item updated successfully!", res });
    }

    [HttpGet("patient/{id}")]
    public async Task<IActionResult> FetchSinglePatient(string id)
    {
        await AuthenticateAsync(); // Ensure authentication before request

        var response = await _httpClient.GetAsync($"{fhirBaseUrl}/Patient/{id}");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return Ok(content);
        }
        return StatusCode((int)response.StatusCode, response.ReasonPhrase);
    }

    [HttpDelete("patient/{id}")]
    public async Task<IActionResult> DeletePatient(string id)
    {
        try
        {
            var res = await _httpClient.DeleteAsync($"{fhirBaseUrl}/Patient/{id}");
            return Ok(new { message = "Deleted", res });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // <<Bundle Post example>>

    [HttpPost("bundle")]
    public async Task<IActionResult> CreateBundle(NewbornModel newborn)
    {
        await AuthenticateAsync(); // Ensure authentication before request

        // Generate a consistent UUID for the Patient
        string patientFullUrl = "urn:uuid:" + Guid.NewGuid().ToString();

        // Determine the correct coding based on Mode_of_Delivery
        var deliveryCoding = newborn.Mode_of_Delivery switch
        {
            "SVD" => new[]
            {
                new
                {
                    system = "http://snomed.info/sct",
                    code = "48782003",
                    display = "Spontaneous vaginal delivery",
                },
            },
            "VAD" => new[]
            {
                new
                {
                    system = "http://snomed.info/sct",
                    code = "61586001",
                    display = "Delivery by vacuum extraction",
                },
            },
            "LSCS" => new[]
            {
                new
                {
                    system = "http://snomed.info/sct",
                    code = "89053004",
                    display = "Caesarean section",
                },
            },
            _ => new[]
            {
                new
                {
                    system = "http://snomed.info/sct",
                    code = "00000000",
                    display = "Other delivery type",
                },
            },
        };

        // ✅ Define the Bundle resource
        var bundleData = new
        {
            resourceType = "Bundle",
            id = "bundle-transaction",
            type = "transaction",
            entry = new List<FhirEntryModel>
            {
                new FhirEntryModel
                {
                    fullUrl = patientFullUrl,
                    resource = new
                    {
                        resourceType = "Patient",
                        identifier = new[]
                        {
                            new
                            {
                                use = "official",
                                system = "http://example.com/newborn-id-system",
                                value = $"NB-{Guid.NewGuid()}",
                            },
                        },
                        name = new[]
                        {
                            new { family = newborn.LastName, given = new[] { newborn.FirstName } },
                        },
                        gender = newborn.Sex,
                        birthDate = newborn.DOB,
                    },
                    request = new NeoHearts_API.Models.Request { method = "POST", url = "Patient" },
                },
                new FhirEntryModel
                {
                    fullUrl = "urn:uuid:" + Guid.NewGuid().ToString(),
                    resource = new
                    {
                        resourceType = "Observation",
                        status = "final",
                        category = new[]
                        {
                            new
                            {
                                coding = new[]
                                {
                                    new
                                    {
                                        system = "http://terminology.hl7.org/CodeSystem/observation-category",
                                        code = "vital-signs",
                                        display = "Vital Signs",
                                    },
                                },
                            },
                        },
                        code = new
                        {
                            coding = new[]
                            {
                                new
                                {
                                    system = "http://loinc.org",
                                    code = "8310-5",
                                    display = "Body Temperature",
                                }, // Corrected LOINC code
                            },
                        },
                        subject = new { reference = patientFullUrl }, // Ensures the Patient reference exists
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
                        valueQuantity = new
                        {
                            value = newborn.T,
                            unit = "degC", // Corrected UCUM unit
                            system = "http://unitsofmeasure.org",
                            code = "Cel",
                        },
                    },
                    request = new NeoHearts_API.Models.Request
                    {
                        method = "POST",
                        url = "Observation",
                    },
                },
                new FhirEntryModel
                {
                    fullUrl = "urn:uuid:" + Guid.NewGuid().ToString(),
                    resource = new
                    {
                        resourceType = "Observation",
                        status = "final",
                        category = new[]
                        {
                            new
                            {
                                coding = new[]
                                {
                                    new
                                    {
                                        system = "http://terminology.hl7.org/CodeSystem/observation-category",
                                        code = "vital-signs",
                                        display = "Vital Signs",
                                    },
                                },
                            },
                        },
                        code = new
                        {
                            coding = new[]
                            {
                                new
                                {
                                    system = "http://loinc.org",
                                    code = "11884-4",
                                    display = "Gestational age",
                                },
                            },
                        },
                        subject = new { reference = patientFullUrl }, // Replace with actual patient reference
                        valueQuantity = new
                        {
                            value = newborn.Gestational_Age, // Use the relevant field from the newborn model
                            unit = "weeks", // Modify unit if necessary
                            system = "http://unitsofmeasure.org",
                            code = "wk",
                        },
                    },
                    request = new NeoHearts_API.Models.Request
                    {
                        method = "POST",
                        url = "Observation",
                    },
                },
                new FhirEntryModel
                {
                    fullUrl = "urn:uuid:" + Guid.NewGuid().ToString(),
                    resource = new
                    {
                        resourceType = "Observation",
                        status = "final",
                        category = new[]
                        {
                            new
                            {
                                coding = new[]
                                {
                                    new
                                    {
                                        system = "http://terminology.hl7.org/CodeSystem/observation-category",
                                        code = "vital-signs",
                                        display = "Vital Signs",
                                    },
                                },
                            },
                        },
                        code = new
                        {
                            coding = new[]
                            {
                                new
                                {
                                    system = "http://snomed.info/sct",
                                    code = "236973005",
                                    display = "Delivery Procedure",
                                },
                            },
                            text = "Mode of Delivery",
                        },
                        subject = new { reference = patientFullUrl }, // Replace with actual patient reference
                        valueCodeableConcept = new { coding = deliveryCoding },
                    },
                    request = new NeoHearts_API.Models.Request
                    {
                        method = "POST",
                        url = "Observation",
                    },
                },
                new FhirEntryModel
                {
                    fullUrl = "urn:uuid:" + Guid.NewGuid().ToString(),
                    resource = new
                    {
                        resourceType = "Observation",
                        status = "final",
                        category = new[]
                        {
                            new
                            {
                                coding = new[]
                                {
                                    new
                                    {
                                        system = "http://terminology.hl7.org/CodeSystem/observation-category",
                                        code = "vital-signs",
                                        display = "Vital Signs",
                                    },
                                },
                            },
                        },
                        code = new
                        {
                            coding = new[]
                            {
                                new
                                {
                                    system = "http://loinc.org",
                                    code = "29463-7",
                                    display = "Body weight",
                                },
                            },
                            text = "Birth weight",
                        },
                        subject = new { reference = patientFullUrl }, // Use patientFullUrl for reference
                        valueQuantity = new
                        {
                            value = newborn.Birth_Weight, // Set the value to null, replace with actual birth weight
                            unit = "kg",
                            system = "http://unitsofmeasure.org",
                            code = "kg",
                        },
                        //effectiveDateTime = "" // Set to the appropriate observation time
                    },
                    request = new NeoHearts_API.Models.Request
                    {
                        method = "POST",
                        url = "Observation",
                    },
                },
                new FhirEntryModel
                {
                    fullUrl = "urn:uuid:" + Guid.NewGuid().ToString(),
                    resource = new
                    {
                        resourceType = "Observation",
                        status = "final",
                        category = new[]
                        {
                            new
                            {
                                coding = new[]
                                {
                                    new
                                    {
                                        system = "http://terminology.hl7.org/CodeSystem/observation-category",
                                        code = "vital-signs",
                                        display = "Vital Signs",
                                    },
                                },
                            },
                        },
                        code = new { text = "Apgar scores" },
                        subject = new { reference = patientFullUrl }, // Use patientFullUrl for reference
                        component = new[]
                        {
                            new
                            {
                                code = new
                                {
                                    coding = new[]
                                    {
                                        new
                                        {
                                            system = "http://loinc.org",
                                            code = "9272-6",
                                            display = "1 minute Apgar score",
                                        },
                                    },
                                    text = "1 minute Apgar score",
                                },
                                valueInteger = newborn.Apgar_Scores_1min,
                            },
                            new
                            {
                                code = new
                                {
                                    coding = new[]
                                    {
                                        new
                                        {
                                            system = "http://loinc.org",
                                            code = "9274-2",
                                            display = "5 minute Apgar score",
                                        },
                                    },
                                    text = "5 minute Apgar score",
                                },
                                valueInteger = newborn.Apgar_Scores_5min,
                            },
                        },
                    },
                    request = new NeoHearts_API.Models.Request
                    {
                        method = "POST",
                        url = "Observation",
                    },
                },
                new FhirEntryModel
                {
                    fullUrl = "urn:uuid:" + Guid.NewGuid().ToString(),
                    resource = new
                    {
                        resourceType = "Observation",
                        status = "final",
                        category = new[]
                        {
                            new
                            {
                                coding = new[]
                                {
                                    new
                                    {
                                        system = "http://terminology.hl7.org/CodeSystem/observation-category",
                                        code = "vital-signs",
                                        display = "Vital Signs",
                                    },
                                },
                            },
                        },
                        code = new { text = "Resuscitation Procedure" },
                        subject = new { reference = patientFullUrl }, // Use patientFullUrl for reference
                        valueString = newborn.Resuscitation, // Set based on NR/BMV/CPR/Adr from form input
                    },
                    request = new NeoHearts_API.Models.Request
                    {
                        method = "POST",
                        url = "Observation",
                    },
                },
            },
        };

        // ✅ Convert C# object to JSON string
        var jsonContent = JsonConvert.SerializeObject(bundleData);
        Console.WriteLine(jsonContent);
        Console.WriteLine();
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/fhir+json");

        // ✅ Send POST request to FHIR API
        var res = await _httpClient.PostAsync($"{fhirBaseUrl}", content);
        var resContent = await res.Content.ReadAsStringAsync();

        if (res.IsSuccessStatusCode)
        {
            return Ok(new { message = "Bundle created successfully", resContent });
        }

        return StatusCode((int)res.StatusCode, resContent);
    }
}

using System.Net.Http.Headers;
using System.Text;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Mvc;
using NeoHearts_API.Models;
using NeoHearts_API.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


[ApiController]
[Route("api/[controller]")]
public class FhirController : ControllerBase
{
    private readonly string fhirBaseUrl;
    private readonly HttpClient _httpClient;

    private readonly IFhirBundleService _fhirBundleService;
    public FhirController(IFhirBundleService fhirBundleService)
    {
        _fhirBundleService = fhirBundleService;
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



    // Method to post the newborn information through a bundle in FHIR store in Google Healthcare API

    [HttpPost("bundle")]
    public async Task<IActionResult> CreateBundle(NewbornModel newborn)
    {
        Console.WriteLine("The newborn is:" + JsonConvert.SerializeObject(newborn));
        await AuthenticateAsync(); // Ensure authentication before request

        var bundleData = _fhirBundleService.CreateFhirBundle(newborn);

        // Convert C# object to JSON string
        var jsonContent = JsonConvert.SerializeObject(bundleData);
        Console.WriteLine();
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/fhir+json");

        // Send POST request to FHIR API
        var res = await _httpClient.PostAsync($"{fhirBaseUrl}", content);
        var resContent = await res.Content.ReadAsStringAsync();

        if (res.IsSuccessStatusCode)
        {
            Console.WriteLine("Bundle successful");
            return Ok(new { message = "Bundle created successfully", resContent });
        }
        else
        {
            Console.WriteLine("Bundle failed" + JsonConvert.SerializeObject(res));
            return StatusCode((int)res.StatusCode, resContent);
        }

    }

    [HttpGet("patients")]
    public async Task<IActionResult> GetAllPatients()
    {
        await AuthenticateAsync(); // Ensure authentication before request

        var response = await _httpClient.GetAsync($"{fhirBaseUrl}/Patient");
        var resContent = await response.Content.ReadAsStringAsync();
        //Console.WriteLine("The response is:" + resContent);
        var bundle = JsonConvert.DeserializeObject<FhirBundle<Patient>>(resContent);
        if (bundle?.Entry == null || bundle.Entry.Count == 0)
        {
            return NotFound("No patients found.");
        }
        var patientData = new List<object>(); // List to hold patient details

        foreach (var entry in bundle.Entry)
        {
            var patient = entry.Resource; // Extract patient resource
            if (patient != null)
            {
                var patientInfo = new
                {
                    patient.Id,
                    Name = string.Join(" ", patient.Name?[0].Given) + " " + patient.Name?[0].Family,
                    patient.BirthDate,
                    patient.Gender

                };
                patientData.Add(patientInfo); // Add the patient info to the list
            }

        }

        if (response.IsSuccessStatusCode)
        {
            return Ok(patientData); // Return patient data
        }
        else
        {
            return StatusCode((int)response.StatusCode, "Not found");
        }
    }

}

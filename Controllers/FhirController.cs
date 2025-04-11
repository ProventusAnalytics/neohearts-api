using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Google.Apis.Auth.OAuth2;
using Google.Apis.CloudHealthcare.v1.Data;
using Hl7.Fhir.Model;
using Hl7.Fhir.Model.CdsHooks;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeoHearts_API.Models;
using NeoHearts_API.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Task = System.Threading.Tasks.Task;


[ApiController]
[Route("api/fhir")]
[Authorize]
public class FhirController : ControllerBase
{
    private readonly string fhirBaseUrl;
    private readonly HttpClient _httpClient;
    private readonly HttpClient _newhttpClient;

    private readonly IFhirBundleService _fhirBundleService;
    private readonly IFhirDataMappingService _fhirDataMappingService;
    private readonly IFhirUpdateService _fhirUpdateService;

    public FhirController(IFhirBundleService fhirBundleService, IFhirDataMappingService fhirDataMappingService, IFhirUpdateService fhirUpdateService)
    {
        _fhirBundleService = fhirBundleService;
        _fhirDataMappingService = fhirDataMappingService;
        _fhirUpdateService = fhirUpdateService;
        _httpClient = new HttpClient();
        _newhttpClient = new HttpClient();
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

    [HttpPut("patient/{patientId}")]
    public async Task<IActionResult> UpdatePatient([FromRoute] string patientId, [FromBody] NewbornModel newborn)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        await AuthenticateAsync(); // Ensure authentication before request

        // Step 1: Retrieve the existing Bundle
        var searchUrl = $"{fhirBaseUrl}/Patient/?_id={patientId}&_revinclude=Observation:patient";
        var searchResponse = await _httpClient.GetAsync(searchUrl);
        var bundleContent = await searchResponse.Content.ReadAsStringAsync();

        if (!searchResponse.IsSuccessStatusCode)
        {
            return StatusCode((int)searchResponse.StatusCode, "Failed to retrieve bundle: " + bundleContent);
        }

        var parser = new FhirJsonParser();
        var bundle = parser.Parse<Bundle>(bundleContent);

        // Step 2: Generate an updated Bundle
        var updatedBundle = _fhirUpdateService.UpdateFhirBundleFromNewbornModel(bundle, newborn);

        var tasks = updatedBundle.Entry
    .Where(entry => entry.Resource?.Id != null)
    .Select(async entry =>
    {
        var resourceType = entry.Resource.GetType().Name;
        var resourceId = entry.Resource.Id;
        var jsonContent = new FhirJsonSerializer().SerializeToString(entry.Resource);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/fhir+json");
        var updateUrl = $"{fhirBaseUrl}/{resourceType}/{resourceId}";

        var updateResponse = await _httpClient.PutAsync(updateUrl, content);

        //Console.WriteLine(updateResponse.Content.ReadAsStringAsync());
        if (!updateResponse.IsSuccessStatusCode)
        {
            var errorContent = await updateResponse.Content.ReadAsStringAsync();
            throw new Exception($"Failed to update {resourceType}/{resourceId}: {errorContent}");
        }
    });

await Task.WhenAll(tasks);


        return Ok(new { message = "All resources updated successfully", data = tasks});
    }


    [HttpGet("patient/{id}")]
    public async Task<IActionResult> FetchSinglePatient([FromRoute]string id)
    {

        await AuthenticateAsync();
        var searchUrl = $"{fhirBaseUrl}/Patient/?_id={id}&_revinclude=Observation:patient";
        //var searchUrl = $"{fhirBaseUrl}/Bundle/3bd37958-8785-4f53-9ba1-132b5aecc0d2";

        var response = await _httpClient.GetAsync(searchUrl);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();

            try
            {
                // Use FHIR's built-in parser instead of Newtonsoft.Json
                var parser = new FhirJsonParser();
                var bundle = parser.Parse<Bundle>(content);

                // Ensure the parsed bundle is valid
                if (bundle == null)
                {
                    return BadRequest("Failed to parse FHIR Bundle.");
                }

                // Map the bundle to NewbornModel
                var mappedData = _fhirDataMappingService.MapFhirToNewbornModel(bundle);

                return Ok(mappedData); // Return the mapped patient data
            }
            catch (Exception ex)
            {
                return BadRequest($"Error parsing FHIR Bundle: {ex.Message}");
            }
        }
        Console.WriteLine("Google Healthcare API Request Failed: " + response);
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

        if (!ModelState.IsValid)
        {
            Console.WriteLine("hereis the model state: " + JsonConvert.SerializeObject(ModelState));
            return BadRequest(ModelState);
        }

        await AuthenticateAsync(); // Ensure authentication before request

        var bundleData = _fhirBundleService.CreateFhirBundle(newborn);

        // Convert C# object to JSON string
        var jsonContent = JsonConvert.SerializeObject(bundleData);
        Console.WriteLine();
        Console.WriteLine(jsonContent);
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

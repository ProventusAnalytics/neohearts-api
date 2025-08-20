using System.CodeDom;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.AccessControl;
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
    private readonly ILogger<FhirController> _logger;
    public FhirController(IFhirBundleService fhirBundleService, IFhirDataMappingService fhirDataMappingService, IFhirUpdateService fhirUpdateService, ILogger<FhirController> logger)
    {
        _logger = logger;
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
        try
        {

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

            if (!updateResponse.IsSuccessStatusCode)
            {
                var errorContent = await updateResponse.Content.ReadAsStringAsync();
                throw new Exception($"Failed to update {resourceType}/{resourceId}: {errorContent}");
            }
        });
            await Task.WhenAll(tasks);
            return Ok(new { message = "All resources updated successfully", data = tasks });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred");
            return BadRequest(ex.Message);
        }
    }


    [HttpGet("patient/{id}")]
    public async Task<IActionResult> FetchSinglePatient([FromRoute] string id)
    {
        await AuthenticateAsync();
        var searchUrl = $"{fhirBaseUrl}/Patient/?_id={id}&_revinclude=Observation:patient";
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
                _logger.LogError(ex, $"Failed to retrieve patient {id}");
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
            await AuthenticateAsync(); // Ensure authentication before request

            // Step 1: Retrieve the existing Bundle
            var searchUrl = $"{fhirBaseUrl}/Patient/?_id={id}";
            var searchResponse = await _httpClient.GetAsync(searchUrl);
            var bundleContent = await searchResponse.Content.ReadAsStringAsync();

            if (!searchResponse.IsSuccessStatusCode)
            {
                return StatusCode((int)searchResponse.StatusCode, "Failed to find patient for deletion!");
            }

            var parser = new FhirJsonParser();
            var bundle = parser.Parse<Bundle>(bundleContent);

            var patient = bundle.Entry[0].Resource as Hl7.Fhir.Model.Patient;
            if (patient == null)
            {
                return NotFound("Patient not found.");
            }
            patient.Active = false;

            var serializer = new FhirJsonSerializer();
            var updatedPatientJson = serializer.SerializeToString(patient);
            var content = new StringContent(updatedPatientJson, Encoding.UTF8, "application/fhir+json");

            var updateUrl = $"{fhirBaseUrl}/Patient/{id}";
            var updateResponse = await _httpClient.PutAsync(updateUrl, content);

            if (!updateResponse.IsSuccessStatusCode)
            {
                return StatusCode((int)updateResponse.StatusCode, "Failed to delete patient by updating status!");
            }

            return Ok("Patient deactivated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete patient");
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
        try
        {
            await AuthenticateAsync(); // Ensure authentication before request

            var bundleData = _fhirBundleService.CreateFhirBundle(newborn);

            // Convert C# object to JSON string
            var jsonContent = JsonConvert.SerializeObject(bundleData);
            Console.WriteLine();
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/fhir+json");

            // Send POST request to FHIR API
            var res = await _httpClient.PostAsync($"{fhirBaseUrl}", content);
            var resContent = await res.Content.ReadAsStringAsync();
            Console.WriteLine("Bundle successful");
            return Ok(new { message = "Bundle created successfully", resContent });

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create patient");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("patients")]
    public async Task<IActionResult> GetAllPatients()
    {
        try
        {
            await AuthenticateAsync(); // Ensure authentication before request

            var response = await _httpClient.GetAsync($"{fhirBaseUrl}/Patient?active=true");
            var resContent = await response.Content.ReadAsStringAsync();
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
                        Name = patient.Name != null && patient.Name.Count > 0
                        ? string.Join(" ", patient.Name[0].Given) + " " + patient.Name[0].Family
                        : string.Empty,
                        patient.BirthDate,
                        patient.Gender

                    };
                    patientData.Add(patientInfo); // Add the patient info to the list
                }

            }
            return Ok(patientData); // Return patient data
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get patients");
            return BadRequest(ex.Message);
        }
    }
}

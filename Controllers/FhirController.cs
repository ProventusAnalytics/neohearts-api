using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Google.Apis.Auth.OAuth2;
using Google.Apis.CloudHealthcare.v1.Data;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc;
using NeoHearts_API.Models;
using NeoHearts_API.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


[ApiController]
[Route("api/fhir")]
public class FhirController : ControllerBase
{
    private readonly string fhirBaseUrl;
    private readonly HttpClient _httpClient;

    private readonly IFhirBundleService _fhirBundleService;
    private readonly IFhirDataMappingService _fhirDataMappingService;
    public FhirController(IFhirBundleService fhirBundleService, IFhirDataMappingService fhirDataMappingService)
    {
        _fhirBundleService = fhirBundleService;
        _fhirDataMappingService = fhirDataMappingService;
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


    [HttpPut("patient/{patientId}")]
    public async Task<IActionResult> UpdatePatient([FromRoute]string patientId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await AuthenticateAsync(); // Ensure authentication before request

        // Step 1: Find the Bundle associated with the Patient
        var searchUrl = $"{fhirBaseUrl}/Patient/?_id={patientId}&_revinclude=Observation:patient";
        //var searchUrl = $"{fhirBaseUrl}/Patient/{patientId}?_revinclude=Encounter:patient&_revinclude=Condition:patient";
        var observationUrl = $"{fhirBaseUrl}/Observation?patient=Patient/{patientId}";
        var searchResponse = await _httpClient.GetAsync(searchUrl);
        var searchContent = await searchResponse.Content.ReadAsStringAsync();

        if (!searchResponse.IsSuccessStatusCode)
        {
            return StatusCode((int)searchResponse.StatusCode, "Failed to retrieve bundle: " + searchContent);
        }

        return Ok(searchContent);
        // Parse the JSON response to get the Bundle ID
        //var searchResult = JsonConvert.DeserializeObject<dynamic>(searchContent);
        //var bundleId = (string)searchResult?.entry?[0]?.resource?.id;

        //if (string.IsNullOrEmpty(bundleId))
        //{
        //    return NotFound("No Bundle found for the given Patient ID.");
        //}

        //// Step 2: Generate a new Bundle with updated data
        //var updatedBundle = _fhirBundleService.CreateFhirBundle(newborn);
        //var jsonContent = JsonConvert.SerializeObject(updatedBundle);
        //var content = new StringContent(jsonContent, Encoding.UTF8, "application/fhir+json");

        // Step 3: Replace the existing Bundle using PUT request
        //var updateResponse = await _httpClient.PutAsync($"{fhirBaseUrl}/Bundle/{bundleId}", content);
        //var updateContent = await updateResponse.Content.ReadAsStringAsync();

        //if (updateResponse.IsSuccessStatusCode)
        //{
        //    return Ok(new { message = "Bundle updated successfully", updateContent });
        //}
        //else
        //{
        //    return StatusCode((int)updateResponse.StatusCode, updateContent);
        //}
    }

    [HttpGet("patient/{id}")]
    public async Task<IActionResult> FetchSinglePatient(string id)
    {
        await AuthenticateAsync(); // Ensure authentication before request
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
        //Console.WriteLine("The newborn is:" + JsonConvert.SerializeObject(newborn));

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

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


// -------> EXAMPLE OF FETCHING A OBSERVATION RESOURCE AND PATIENT THEN COMBINING THEM INTO A SINGLE RESPONSE <-------

//[HttpPut("patient/{patientId}")]
//public async Task<IActionResult> UpdatePatient([FromRoute] string patientId)
//{
//    if (!ModelState.IsValid)
//    {
//        return BadRequest(ModelState);
//    }

//    await AuthenticateAsync(); // Ensure authentication before request

//    try
//    {
//        // Fetch Patient resource
//        var patientUrl = $"{fhirBaseUrl}/Patient/{patientId}";
//        var patientResponse = await _httpClient.GetAsync(patientUrl);

//        if (!patientResponse.IsSuccessStatusCode)
//        {
//            var errorContent = await patientResponse.Content.ReadAsStringAsync();
//            return StatusCode((int)patientResponse.StatusCode, $"Failed to retrieve patient: {errorContent}");
//        }

//        var patientContent = await patientResponse.Content.ReadAsStringAsync();
//        var patientResource = JsonConvert.DeserializeObject(patientContent);

//        // Fetch Observation resources
//        var observationUrl = $"{fhirBaseUrl}/Observation?patient=Patient/{patientId}";
//        var observationResponse = await _httpClient.GetAsync(observationUrl);

//        if (!observationResponse.IsSuccessStatusCode)
//        {
//            var errorContent = await observationResponse.Content.ReadAsStringAsync();
//            return StatusCode((int)observationResponse.StatusCode, $"Failed to retrieve observations: {errorContent}");
//        }

//        var observationContent = await observationResponse.Content.ReadAsStringAsync();
//        var observationBundle = JsonConvert.DeserializeObject(observationContent);

//        // Combine results
//        var result = new
//        {
//            Patient = patientResource,
//            Observations = observationBundle
//        };

//        return Ok(result);
//    }
//    catch (Exception ex)
//    {
//        // Log the exception for debugging
//        Console.WriteLine($"An error occurred: {ex.Message}");
//        return StatusCode(500, "An internal error occurred while processing your request.");
//    }
//}
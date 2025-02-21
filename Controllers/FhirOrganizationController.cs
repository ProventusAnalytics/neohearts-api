using Google.Apis.Auth.OAuth2;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using NeoHearts_API.Services;
using NeoHearts_API.Models;
using Newtonsoft.Json;
using System.Text;
using NeoHearts_API.Models.DTOs;

namespace NeoHearts_API.Controllers
{
    [ApiController]
    [Route("api/fhir/organization")]
    public class FhirOrganizationController : Controller
    {
        private readonly string fhirBaseUrl;
        private readonly HttpClient _httpClient;

        private readonly IFhirBundleService _fhirBundleService;
        public FhirOrganizationController(IFhirBundleService fhirBundleService)
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

        [HttpPost]
        public async Task<IActionResult> CreateOrganization(OrganizationModel newOrg)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await AuthenticateAsync(); // Ensure authentication before request

            var organizationData = new
            {
                resourceType = "Organization",
                //id = newOrg.Id,
                name = newOrg.Name,
                active = true
            };

            // Convert C# object to JSON string
            var jsonContent = JsonConvert.SerializeObject(organizationData);
            Console.WriteLine();
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/fhir+json");

            // Send POST request to FHIR API
            var res = await _httpClient.PostAsync($"{fhirBaseUrl}/Organization", content);
            var resContent = await res.Content.ReadAsStringAsync();

            if (res.IsSuccessStatusCode)
            {
                Console.WriteLine("Organization creaed successful");
                return Ok(new { message = "Organization created successfully", resContent });
            }
            else
            {
                Console.WriteLine("Organization failed" + JsonConvert.SerializeObject(res));
                return StatusCode((int)res.StatusCode, resContent);
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrganizations()
        {
            await AuthenticateAsync(); // Ensure authentication before request

            var response = await _httpClient.GetAsync($"{fhirBaseUrl}/Organization");
            var resContent = await response.Content.ReadAsStringAsync();
            //Console.WriteLine("The response is:" + resContent);
            //var bundle = JsonConvert.DeserializeObject<FhirBundle<Patient>>(resContent);
            //if (bundle?.Entry == null || bundle.Entry.Count == 0)
            //{
            //    return NotFound("No patients found.");
            //}
            //var patientData = new List<object>(); // List to hold patient details

            //foreach (var entry in bundle.Entry)
            //{
            //    var patient = entry.Resource; // Extract patient resource
            //    if (patient != null)
            //    {
            //        var patientInfo = new
            //        {
            //            patient.Id,
            //            Name = string.Join(" ", patient.Name?[0].Given) + " " + patient.Name?[0].Family,
            //            patient.BirthDate,
            //            patient.Gender

            //        };
            //        patientData.Add(patientInfo); // Add the patient info to the list
            //    }
            //}

            if (response.IsSuccessStatusCode)
            {
                return Ok(resContent); // Return org data
            }
            else
            {
                return StatusCode((int)response.StatusCode, "Not found");
            }
        }


        [HttpGet("{orgId}")]
        public async Task<IActionResult> GetSpecifiOrganization([FromRoute] string orgId)
        {
            await AuthenticateAsync(); // Ensure authentication before request

            var response = await _httpClient.GetAsync($"{fhirBaseUrl}/Organization/{orgId}");
            var resContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return Ok(resContent); // Return org data
            }
            else
            {
                return StatusCode((int)response.StatusCode, "Not found");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrganization([FromRoute] string id)
        {
            await AuthenticateAsync(); // Ensure authentication before request
            var response = await _httpClient.DeleteAsync($"{fhirBaseUrl}/Organization/{id}");
            var resContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return Ok(new { message = "Organization deleted successfully", resContent });
            }
            else
            {
                return StatusCode((int)response.StatusCode, resContent);
            }
        }
        [HttpPut("{organizationId}")]
        public async Task<IActionResult> UpdateOrganizationName([FromRoute] string organizationId, [FromBody] UpdateOrganizationDto updateOrg)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await AuthenticateAsync(); // Ensure authentication before request

            // Fetch existing organization data
            var response = await _httpClient.GetAsync($"{fhirBaseUrl}/Organization/{organizationId}");
            var searchContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Failed to retrieve organization: " + searchContent);
            }

            // Deserialize existing data
            var organization = JsonConvert.DeserializeObject<OrganizationModel>(searchContent);

            if (organization == null)
            {
                return NotFound("Organization not found.");
            }

            // Update organization name
            organization.Name = updateOrg.newName;

            Console.WriteLine("Updated organization: " + JsonConvert.SerializeObject(organization));

            // Serialize updated organization
            var jsonContent = JsonConvert.SerializeObject(organization);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/fhir+json");

            // Send PUT request to update the organization
            var updateResponse = await _httpClient.PutAsync($"{fhirBaseUrl}/Organization/{organizationId}", content);
            var updateContent = await updateResponse.Content.ReadAsStringAsync();

            if (updateResponse.IsSuccessStatusCode)
            {
                return Ok(new { message = "Organization updated successfully", updateContent });
            }
            else
            {
                return StatusCode((int)updateResponse.StatusCode, updateContent);
            }
        }

    }
}


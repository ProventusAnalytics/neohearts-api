using Newtonsoft.Json.Linq;
using System.Text;
using NeoHearts_API.Models;
using Newtonsoft.Json;
using Google.Apis.Auth.OAuth2;

namespace NeoHearts_API.Services
{
    public class FhirService
    {
        private readonly string _projectId = "your-project-id";
        private readonly string _location = "your-location";
        private readonly string _datasetId = "your-dataset-id";
        private readonly string _fhirStoreId = "your-fhirstore-id";

        public static JObject MapToFhirObservation(NewbornModel exam)
        {
            var observation = new
            {
                resourceType = "Observation",
                status = "final",
                category = new[]
                {
            new { coding = new[] { new { system = "http://terminology.hl7.org/CodeSystem/observation-category", code = "vital-signs" } } }
        },
                code = new { text = "Physical Examination" },
                subject = new { reference = $"Patient/{exam.Id}" },  // Replace with actual patient reference
                effectiveDateTime = DateTime.UtcNow.ToString("o"),
                component = new List<object>
        {
            new { code = new { text = "Heart Rate" }, valueQuantity = new { value = exam.HR, unit = "bpm" } },
            new { code = new { text = "Respiratory Rate" }, valueQuantity = new { value = exam.RR, unit = "breaths/min" } },
            new { code = new { text = "Temperature" }, valueQuantity = new { value = exam.T, unit = "°C" } },
            new { code = new { text = "Head Circumference" }, valueQuantity = new { value = exam.HC, unit = "cm" } },
            new { code = new { text = "Breath Sounds" }, valueString = exam.Breath_sounds },
            new { code = new { text = "Heart Sounds" }, valueString = exam.Heart_sounds },
            new { code = new { text = "Cyanosis" }, valueBoolean = exam.Cyanosis },
            new { code = new { text = "Increased Work of Breathing" }, valueBoolean = exam.Increased_work_of_breathing },
            new { code = new { text = "Anterior Fontanelle" }, valueString = exam.Anterior },
            new { code = new { text = "Posterior Fontanelle" }, valueString = exam.Posterior },
            new { code = new { text = "Choanal Atresia" }, valueBoolean = exam.Choanal_artesia },
            new { code = new { text = "Palpable Masses" }, valueString = exam.Palpable_masses },
            new { code = new { text = "Umbilical Cord" }, valueString = exam.Umbilical_cord },
            new { code = new { text = "Position and Patency" }, valueString = exam.Position_patency },
            new { code = new { text = "Spine Alignment" }, valueString = exam.Spine_alignment },
            new { code = new { text = "Sacral Dimple" }, valueString = exam.Sacral_dimple },
            new { code = new { text = "Femoral Pulses Right" }, valueBoolean = exam.Femoral_pulses_right },
            new { code = new { text = "Femoral Pulses Left" }, valueBoolean = exam.Femoral_pulses_left },
            new { code = new { text = "Hip Dysplasia" }, valueBoolean = exam.Hip_dysplasia },
            new { code = new { text = "Moro Reflex" }, valueBoolean = exam.Moro_reflex },
            new { code = new { text = "Rooting Reflex" }, valueBoolean = exam.Rooting_reflex },
            new { code = new { text = "Sucking Reflex" }, valueBoolean = exam.Sucking_reflex },
            new { code = new { text = "Tone" }, valueBoolean = exam.Tone },
            new { code = new { text = "Activity" }, valueBoolean = exam.Activity },
            new { code = new { text = "Right Upper Arm" }, valueQuantity = new { value = exam.Right_upper_arm, unit = "cm" } },
            new { code = new { text = "Right Leg" }, valueQuantity = new { value = exam.Right_leg, unit = "cm" } },
            new { code = new { text = "Congenital Heart Disease" }, valueBoolean = exam.Congenital_heart_disease },
            new { code = new { text = "Open Heart Surgery" }, valueBoolean = exam.Open_heart_surgery },
            new { code = new { text = "Sudden Cardiac Death" }, valueBoolean = exam.Sudden_cardiac_death },
            new { code = new { text = "Pacemaker" }, valueBoolean = exam.Pacemaker },
            new { code = new { text = "Dysmorphism" }, valueString = exam.Dysmorphism },
            new { code = new { text = "Others" }, valueString = exam.Others },
            new { code = new { text = "Discharge" }, valueBoolean = exam.Discharge },
            new { code = new { text = "Repeat CCHD" }, valueBoolean = exam.Repeat_CCHD },
            new { code = new { text = "Echocardiogram" }, valueBoolean = exam.Echocardiogram },
            new { code = new { text = "CXR" }, valueBoolean = exam.CXR },
            new { code = new { text = "ECG" }, valueBoolean = exam.ECG },
            new { code = new { text = "Ultrasound" }, valueBoolean = exam.Ultrasound },
            new { code = new { text = "Follow Up" }, valueString = exam.Follow_up },
            new { code = new { text = "Checked By" }, valueString = exam.Checked_by }
        }
            };
            return JObject.Parse(JsonConvert.SerializeObject(observation));
        }

        public static JObject MapToFhirPatient(NewbornModel newborn)
        {
            var fhirPatient = new
            {
                resourceType = "Patient",
                id = newborn.Id, // Unique identifier for the patient
                name = new[]
                {
                new
                {
                    use = "official",
                    given = new[] { newborn.FirstName },
                    family = newborn.LastName
                }
            },
                gender = newborn.Sex?.ToLower() ?? "other",  // Defaults to "other" if Sex is null
                birthDate = newborn.DOB.ToString("yyyy-MM-dd"), // FHIR expects date in YYYY-MM-DD format
            };

            return JObject.Parse(JsonConvert.SerializeObject(fhirPatient));
        }

        public async Task<string> SendFhirResourceAsync(JObject fhirResource, string accessToken)
        {

            var fhirStoreName = $"projects/{_projectId}/locations/{_location}/datasets/{_datasetId}/fhirStores/{_fhirStoreId}";
            var resourceType = fhirResource["resourceType"]?.ToString();
            var requestUrl = $"https://healthcare.googleapis.com/v1/{fhirStoreName}/fhir/{resourceType}";

            using (var httpClient = new HttpClient()) //sending an HTTP request to the Google Healthcare API to create a FHIR resource.
            {
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var content = new StringContent(fhirResource.ToString(), Encoding.UTF8, "application/fhir+json");
                var response = await httpClient.PostAsync(requestUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to create FHIR resource: {errorResponse}");
                }

                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}


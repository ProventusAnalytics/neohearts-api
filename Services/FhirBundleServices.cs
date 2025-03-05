using NeoHearts_API.Models;

namespace NeoHearts_API.Services
{
    public class FhirBundleServices : IFhirBundleService
    {
        public object NormalOrDecreased(string tone)
        {
            return tone switch
            {
                "normal" => new
                {
                    coding = new[]
                    {
                    new
                    {
                        system = "http://terminology.hl7.org/CodeSystem/v3-ObservationInterpretation",
                        code = "N", // Code for Normal
                        display = "Normal",
                    },
                },
                    text = "Normal tone",
                },
                "decreased" => new
                {
                    coding = new[]
                    {
                    new
                    {
                        system = "http://terminology.hl7.org/CodeSystem/v3-ObservationInterpretation",
                        code = "D", // Code for Decreased
                        display = "Significantly change down",
                    },
                },
                    text = "Decreased tone",
                },
                _ => new
                {
                    coding = new[]
                    {
                    new
                    {
                        system = "http://terminology.hl7.org/CodeSystem/v3-ObservationInterpretation",
                        code = "U", // Code for Unknown
                        display = "Unknown",
                    },
                },
                    text = "Unknown tone",
                },
            };
        }
        public object CreateFhirBundle(NewbornModel newborn)
        {
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

            // Define the Bundle resource
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
                            text = "Body temperature",
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
                            text = "Gestational age",
                        },
                        subject = new { reference = patientFullUrl }, // Replace with actual patient reference
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
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
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
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
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
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
                        code = new {  coding = new[]
                                    {
                                        new
                                        {
                                            system = "http://loinc.org",
                                            code = "9272-6",
                                            display = "1 minute Apgar score",
                                        },
                                    },text = "Apgar scores" },
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
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
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
                        valueString = newborn.Resuscitation, // Set based on NR/BMV/CPR/Adr from form input
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
                                    code = "11977-6",
                                    display = "Number of previous pregnancies (parity)",
                                },
                            },
                            text = "Parity",
                        },
                        subject = new { reference = patientFullUrl },
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
                        valueInteger = newborn.Parity,
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
                                    code = "416413003",
                                    display = "Maternal age",
                                },
                            },
                            text = "Maternal age",
                        },
                        subject = new { reference = patientFullUrl },
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
                        valueInteger = newborn.Maternal_age,
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
                                    code = "13213009",
                                    display = "Congenital heart disease",
                                },
                            },
                            text = "CHD/CCHD",
                        },
                        subject = new { reference = patientFullUrl },
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
                        valueString = newborn.CHD_CCHD,
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
                                    code = "16310003", // SNOMED CT code for diagnostic ultrasound
                                    display = "Ultrasound", // Display for ultrasound
                                },
                            },
                            text = "Prenatal ultrasound",
                        },
                        subject = new { reference = patientFullUrl },
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
                        valueString = newborn.Prenatal_ultrasound,
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
                                    code = "78648007", // SNOMED CT code for "At risk for infection"
                                    display = "At risk for infection",
                                },
                            },
                            text = "Risk Factor for Sepsis",
                        },
                        subject = new { reference = patientFullUrl },
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
                        valueString = newborn.Sepsis,
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
                                    code = "29308-4", // LOINC code for "Diagnosis"
                                    display = "Diagnosis",
                                },
                            },
                            text = "Extracardiac diagnosis",
                        },
                        subject = new
                        {
                            reference = patientFullUrl, // Use the actual patient reference
                        },
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
                        valueString = $"{newborn.Extracardiac_Diagnosis}" // Replace with actual diagnosis text
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
                                    code = "40443-4", // LOINC code for "Heart rate"
                                    display = "Heart rate",
                                },
                            },
                            text = "Heart rate",
                        },
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
                        subject = new { reference = patientFullUrl }, // Replace with actual patient reference
                        valueQuantity = new
                        {
                            value = newborn.HR, // Set the HR value here
                            unit = "beats/min",
                            system = "http://unitsofmeasure.org",
                            code = "/min",
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
                                    code = "9303-9", // LOINC code for "Respiratory rate"
                                    display = "Respiratory rate",
                                },
                            },
                            text = "Respiratory rate",
                        },
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
                        subject = new { reference = patientFullUrl }, // Replace with actual patient reference
                        valueQuantity = new
                        {
                            value = newborn.RR, // Set the RR value here
                            unit = "breaths/min",
                            system = "http://unitsofmeasure.org",
                            code = "/min",
                        },
                    },
                    request = new NeoHearts_API.Models.Request
                    {
                        method = "POST",
                        url = "Observation",
                    },
                },
                //new FhirEntryModel
                //{
                //    fullUrl = "urn:uuid:" + Guid.NewGuid().ToString(),
                //    resource = new
                //    {
                //        resourceType = "Observation",
                //        status = "final",
                //        category = new[]
                //        {
                //            new
                //            {
                //                coding = new[]
                //                {
                //                    new
                //                    {
                //                        system = "http://terminology.hl7.org/CodeSystem/observation-category",
                //                        code = "vital-signs",
                //                        display = "Vital Signs",
                //                    },
                //                },
                //            },
                //        },
                //        code = new
                //        {
                //            coding = new[]
                //            {
                //                new
                //                {
                //                    system = "http://loinc.org",
                //                    code = "8310-5", // LOINC code for "Body temperature"
                //                    display = "Body temperature",
                //                },
                //            },
                //            text = "Body Temperature",
                //        },
                //        subject = new { reference = patientFullUrl }, // Replace with actual patient reference
                //        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
                //        valueQuantity = new
                //        {
                //            value = newborn.T, // Set the T value here
                //            unit = "°C",
                //            system = "http://unitsofmeasure.org",
                //            code = "Cel",
                //        },
                //    },
                //    request = new NeoHearts_API.Models.Request
                //    {
                //        method = "POST",
                //        url = "Observation",
                //    },
                //},
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
                                    code = "95837007", // SNOMED CT code for "Cyanosis (Central)"
                                    display = "Cyanosis (Central)",
                                },
                            },
                            text = "Central Cyanosis",
                        },
                        subject = new { reference = patientFullUrl }, // Replace with actual patient reference
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
                        valueString = newborn.Cyanosis,
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
                                    code = "230145002", // SNOMED CT code for "Increased Work of Breathing"
                                    display = "Increased Work of Breathing",
                                },
                            },
                            text = "Work of Breathing",
                        },
                        subject = new { reference = patientFullUrl }, // Replace with actual patient reference
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
                        valueString = newborn.Increased_work_of_breathing,
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
                                    code = "363812007", // SNOMED CT code for "Head circumference"
                                    display = "Head circumference",
                                },
                            },
                            text = "Head Circumference",
                        },
                        subject = new { reference = patientFullUrl }, // Replace with actual patient reference
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
                        valueQuantity = new
                        {
                            value = newborn.HC, // Set this value from the newborn model
                            unit = "cm",
                            system = "http://unitsofmeasure.org",
                            code = "cm",
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
                                    code = "404684003", // SNOMED CT code for "Fontanelle Status"
                                    display = "Clinical Finding",
                                },
                            },
                            text = "Clinical Finding",
                        },
                        subject = new { reference = patientFullUrl }, // Replace with actual patient reference
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
                        //valueString = "Ant: " + newborn.Anterior + " Post: " + newborn.Posterior, // Use newborn model values
                        component = new List<object>
                        {
                            new
                            {
                                code = new
                                {
                                   text = "Anterior Fontanelle",
                                },
                                valueString = newborn.Anterior,
                            },
                            new
                            {
                                  code = new
                                {
                                   text = "Posterior Fontanelle",
                                },
                                valueString = newborn.Posterior,
                            },
                        }
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
                                    code = "79815-7", // LOINC code for "Pupillary Response"
                                    display = "Pupillary Response",
                                },
                            },
                            text = "Pupillary Response",
                        },
                        subject = new { reference = patientFullUrl }, // Replace with actual patient reference
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
                        valueString = newborn.Pupillary_response, // Use newborn model value
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
                                    code = "783819001", // SNOMED code for "Red Reflex"
                                    display = "Red Reflex",
                                },
                            },
                            text = "Red Reflex",
                        },
                        subject = new { reference = patientFullUrl }, // Replace with actual patient reference
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
                        valueString = newborn.Red_reflex, // Use newborn model value
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
                                    code = "1303493008", // LOINC code for "Atresia"
                                    display = "Atresia",
                                },
                            },
                            text = "Choanal atresia", // Descriptive text for the observation
                        },
                        subject = new { reference = patientFullUrl }, // Replace with actual patient reference
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
                        valueString = newborn.Choanal_artesia, // Use newborn model value
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
                                    code = "72072-2", // LOINC code for "Breath Sounds"
                                    display = "Breath Sounds",
                                },
                            },
                            text = "Breath Sounds",
                        },
                        subject = new { reference = patientFullUrl }, // Replace with actual patient reference
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
                        valueString = newborn.Breath_sounds, // Use newborn model value
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
                                    code = "80277-7", // LOINC code for "Heart Sounds/Murmurs"
                                    display = "Heart Sounds/Murmurs",
                                },
                            },
                            text = "Heart Sounds",
                        },
                        subject = new { reference = patientFullUrl }, // Replace with actual patient reference
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
                        valueString = newborn.Heart_sounds, // Use newborn model value
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
                                        code = "physical-exam",
                                        display = "Physical Examination",
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
                                    code = "271860004", // LOINC code for "Abdominal mass"
                                    display = "Abdominal mass",
                                },
                            },
                            text = "Abdominal mass",
                        },
                        subject = new { reference = patientFullUrl }, // Replace with actual patient reference
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
                        valueString = newborn.Palpable_masses, // Use newborn model value
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
                                        code = "physical-exam",
                                        display = "Physical Examination",
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
                                    code = "29870000", // SNOMED CT code for "Umbilical Cord"
                                    display = "Umbilical Cord",
                                },
                            },
                            text = "Umbilical Cord",
                        },
                        subject = new { reference = patientFullUrl },
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
                        valueString = newborn.Umbilical_cord, // Use newborn model value
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
                                        code = "physical-exam",
                                        display = "Physical Examination",
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
                                    code = "28028-9", // LOINC code for "Anus Position/Patency"
                                    display = "Anus Position/Patency",
                                },
                            },
                            text = "Anus Position",
                        },
                        subject = new { reference = patientFullUrl }, // Replace with actual patient reference
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
                        valueString = newborn.Position_patency, // Use newborn model value
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
                                        code = "physical-exam",
                                        display = "Physical Examination",
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
                                    code = "410730009", // LOINC code for "Spine Alignment"
                                    display = "Spine",
                                },
                            },
                            text = "Spine Alignment",
                        },
                        subject = new { reference = patientFullUrl }, // Replace with actual patient reference
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
                        valueString = newborn.Spine_alignment, // Use newborn model value
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
                                    code = "311897005", // LOINC code for "Sacral Dimple"
                                    display = "Sacral Dimple/Hair Tufts",
                                },
                            },
                            text = "Sacral Dimple",
                        },
                        subject = new { reference = patientFullUrl }, // Replace with actual patient reference
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
                        valueString = newborn.Sacral_dimple, // Use newborn model value
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
                                    code = "7657000", // LOINC code for "Femoral Artery"
                                    display = "Femoral Artery",
                                },
                            },
                            text = "Femoral Artery",
                        },
                        subject = new { reference = patientFullUrl }, // Replace with actual patient reference
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
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
                                            system = "http://snomed.info/sct",
                                            code = "7657000", // Right femoral pulse (example)
                                            display = "Right femoral pulse",
                                        },
                                    },
                            text = "Right Femoral Pulse",
                                },
                                valueString = newborn.Femoral_pulses_right, // Replace with actual value
                            },
                            new
                            {
                                code = new
                                {
                                    coding = new[]
                                    {
                                        new
                                        {
                                            system = "http://snomed.info/sct",
                                            code = "7657000", // Left femoral pulse (example)
                                            display = "Left femoral pulse",
                                        },
                                    },
                            text = "Left Femoral Pulse",
                                },
                                valueString = newborn.Femoral_pulses_left, // Replace with actual value
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
                        code = new
                        {
                            coding = new[]
                            {
                                new
                                {
                                    system = "http://snomed.info/sct",
                                    code = "299233007", // Hip dysplasia code (or used an alternative for "Deformity of hip joint")
                                    display = "Deformity of hip joint",
                                },
                            },
                            text = "Hip Dysplasia",
                        },
                        subject = new { reference = patientFullUrl }, // Reference to the newborn patient
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
                        valueString = newborn.Hip_dysplasia, // Replace with actual value from the newborn model
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
                        id = "moro-reflex",
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
                                    code = "87572000", // SNOMED code for Reflex
                                    display = "Reflex",
                                },
                            },
                            text = "Moro Reflex",
                        },
                        subject = new { reference = patientFullUrl }, // Reference to the patient
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
                        valueString = newborn.Moro_reflex, // Replace with the actual value from the newborn model
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
                        id = "rooting-reflex",
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
                                    code = "56876-6", // LOINC code for Rooting Reflex
                                    display = "Rooting reflex",
                                },
                            },
                            text = "Rooting Reflex",
                        },
                        subject = new { reference = patientFullUrl }, // Reference to the patient
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
                        valueString = newborn.Rooting_reflex, // Replace with the actual value from the newborn model
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
                        id = "sucking-reflex",
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
                                    code = "56876-6", // LOINC code for Rooting Reflex (shared with Sucking Reflex)
                                    display = "Rooting reflex", // Note that this code is used for both Sucking and Rooting Reflex
                                },
                            },
                            text = "Sucking Reflex",
                        },
                        subject = new { reference = patientFullUrl }, // Reference to the patient
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
                        valueString = newborn.Sucking_reflex, // Replace with the actual value from the newborn model
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
                        id = "tone",
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
                                    code = "6918002", // SNOMED code for Muscle tone
                                    display = "Muscle tone",
                                },
                            },
                            text = "Tone",
                        },
                        subject = new { reference = patientFullUrl }, // Reference to the patient
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
                        valueCodeableConcept = NormalOrDecreased(newborn.Tone),
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
                        id = "activity",
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
                                    code = "257733005", // SNOMED code for Activity observed
                                    display = "Activity observed",
                                },
                            },
                            text = "Activity",
                        },
                        subject = new { reference = patientFullUrl }, // Reference to the patient
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
                        valueCodeableConcept = NormalOrDecreased(newborn.Activity),
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
                        id = "oxygen-saturation",
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
                                    code = "20564-1", // LOINC code for Oxygen saturation in Arterial blood
                                    display = "Oxygen saturation",
                                },
                            },
                            text = "Oxygen saturation measurements",
                        },
                        subject = new { reference = patientFullUrl }, // Reference to the patient
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
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
                                            system = "http://snomed.info/sct",
                                            code = "40983000", // SNOMED CT code for Right upper arm
                                            display = "Upper arm",
                                        },
                                    },
                                    text = "Oxygen saturation at Right Upper Arm",
                                },
                                valueQuantity = new
                                {
                                    value = newborn.Right_upper_arm,
                                    unit = "%",
                                    system = "http://unitsofmeasure.org",
                                    code = "%",
                                },
                            },
                            new
                            {
                                code = new
                                {
                                    coding = new[]
                                    {
                                        new
                                        {
                                            system = "http://snomed.info/sct",
                                            code = "62175007", // SNOMED CT code for Right leg
                                            display = "Right leg",
                                        },
                                    },
                                    text = "Oxygen saturation at Right Leg",
                                },
                                valueQuantity = new
                                {
                                    value = newborn.Right_leg,
                                    unit = "%",
                                    system = "http://unitsofmeasure.org",
                                    code = "%",
                                },
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
                        id = "cchd-screening-1",
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
                                    code = "73805-4", // LOINC code for CCHD newborn screening panel
                                    display = "CCHD newborn screening panel",
                                },
                            },
                            text = "CCHD Screening (1st)",
                        },
                        subject = new { reference = patientFullUrl }, // Reference to the patient
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
                        valueString = newborn.CCHD_first,
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
                        id = "family-history",
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
                        subject = new { reference = patientFullUrl, display = "Example Patient" }, // Updated reference to patientFullUrl
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
                        code = new
                        {
                            coding = new[]
                            {
                                new
                                {
                                    system = "http://loinc.org",
                                    code = "39155-7", // LOINC code for Family history of disease or condition
                                    display = "Family history of disease or condition",
                                },
                            },
                            text = "Family History of Cardiac Conditions",
                        },
                        component = new List<object>
                        {
                            new
                            {
                                code = new
                                {
                                    coding = new[]
                                    {
                                        new
                                        {
                                            system = "http://snomed.info/sct",
                                            code = "13213009", // SNOMED CT code for Congenital heart disease
                                            display = "Congenital heart disease",
                                        },
                                    },
                            text = "Congenital Heart Disease",
                                },
                                valueString = newborn.Congenital_heart_disease,
                            },
                            new
                            {
                                code = new
                                {
                                    coding = new[]
                                    {
                                        new
                                        {
                                            system = "http://snomed.info/sct",
                                            code = "2598006", // SNOMED CT code for Open-heart surgery
                                            display = "Open-heart surgery",
                                        },
                                    },
                            text = "Open Heart Surgery",
                                },
                                valueString = newborn.Open_heart_surgery,
                            },
                            new
                            {
                                code = new
                                {
                                    coding = new[]
                                    {
                                        new
                                        {
                                            system = "http://snomed.info/sct",
                                            code = "26636000", // SNOMED CT code for Sudden death
                                            display = "Sudden death",
                                        },
                                    },
                                    text = "Sudden cardiac death",
                                },
                                valueString = newborn.Sudden_cardiac_death,
                            },
                            new
                            {
                                code = new
                                {
                                    coding = new[]
                                    {
                                        new
                                        {
                                            system = "http://snomed.info/sct",
                                            code = "118378005", // SNOMED CT code for Pacemaker pulse generator
                                            display = "Pacemaker pulse generator",
                                        },
                                    },
                            text = "Pacemaker",
                                },
                                valueString = newborn.Pacemaker,
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
                        id = "dysmorphism-obs",
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
                                    code = "276720006", // SNOMED CT code for Dysmorphism
                                    display = "Dysmorphism",
                                },
                            },
                            text = "Dysmorphism",
                        },
                        subject = new { reference = patientFullUrl }, // Updated reference to patientFullUrl
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
                        valueString = newborn.Dysmorphism,
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
                        id = "others-observation",
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
                                    code = "75321-0", // LOINC code for Clinical findings
                                    display = "Clinical findings",
                                },
                            },
                            text = "Others",
                        },
                        subject = new { reference = patientFullUrl }, // Updated reference to patientFullUrl
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
                        valueString = newborn.Others,
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
                        id = "plan-and-follow-up",
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
                                    code = "22026-9", // LOINC code for physical follow up
                                    display = "Plan and Follow-Up",
                                },
                            },
                            text = "Plan & Follow-Up",
                        },
                        subject = new { reference = patientFullUrl }, // Updated reference to patientFullUrl
                        effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Required field
                        component = new List<object>
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
                                            code = "52523-8",
                                            display = "Discharge Status",
                                        },
                                    },
                                    text = "Discharge",
                                },
                                valueString = newborn.Discharge,
                            },
                            new
                            {
                                code = new
                                {
                                    coding = new[]
                                    {
                                        new
                                        {
                                            system = "http://snomed.info/sct",
                                            code = "13213009",
                                            display = "Congenital heart disease",
                                        },
                                    },
                                    text = "Repeat CCHD",
                                },
                                valueString = newborn.Repeat_CCHD,
                            },
                            new
                            {
                                code = new
                                {
                                    coding = new[]
                                    {
                                        new
                                        {
                                            system = "http://snomed.info/sct",
                                            code = "40701008",
                                            display = "Echocardiogram",
                                        },
                                    },
                            text = "Echocardiogram",
                                },
                                valueBoolean = newborn.Echocardiogram,
                            },
                            new
                            {
                                code = new
                                {
                                    coding = new[]
                                    {
                                        new
                                        {
                                            system = "http://snomed.info/sct",
                                            code = "399208008",
                                            display = "Chest X-Ray (CXR)",
                                        },
                                    },
                            text = "CXR",
                                },
                                valueBoolean = newborn.CXR,
                            },
                            new
                            {
                                code = new
                                {
                                    coding = new[]
                                    {
                                        new
                                        {
                                            system = "http://snomed.info/sct",
                                            code = "29303009",
                                            display = "ECG",
                                        },
                                    },
                            text = "ECG",
                                },
                                valueBoolean = newborn.ECG,
                            },
                            new
                            {
                                code = new
                                {
                                    coding = new[]
                                    {
                                        new
                                        {
                                            system = "http://snomed.info/sct",
                                            code = "16310003",
                                            display = "Ultrasound",
                                        },
                                    },
                            text = "Ultrasound",
                                },
                                valueBoolean = newborn.Ultrasound,
                            },
                            new
                            {
                                code = new
                                {
                                    coding = new[]
                                    {
                                        new
                                        {
                                            system = "http://snomed.info/sct",
                                            code = "308273005",
                                            display = "Follow-up",
                                        },
                                    },
                                    text = "Follow-up instructions",
                                },
                                valueString = newborn.Follow_up,
                            },
                            new
                            {
                                code = new
                                {
                                    coding = new[]
                                    {
                                        new
                                        {
                                            system = "http://snomed.info/sct",
                                            code = "360160009",
                                            display = "Checking",
                                        },
                                    },
                                    text = "Checked by",
                                },
                                valueString = newborn.Checked_by,
                            },
                        },
                    },
                    request = new NeoHearts_API.Models.Request
                    {
                        method = "POST",
                        url = "Observation",
                    },
                },
            },
            };
            return bundleData;
        }
    }
}

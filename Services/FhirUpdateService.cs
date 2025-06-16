using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using NeoHearts_API.Models;
using Newtonsoft.Json;

namespace NeoHearts_API.Services
{
    public class FhirUpdateService : IFhirUpdateService
    {
        public Bundle UpdateFhirBundleFromNewbornModel(Bundle bundle, NewbornModel newborn)
        {
            // Update Patient information
            var patientEntry = bundle.Entry.FirstOrDefault(e => e.Resource is Hl7.Fhir.Model.Patient);
            if (patientEntry?.Resource is Hl7.Fhir.Model.Patient patientResource)
            {
                var name = new Hl7.Fhir.Model.HumanName
                {
                    Given = new List<string> { newborn.FirstName },
                    Family = newborn.LastName
                };
                patientResource.Name = new List<Hl7.Fhir.Model.HumanName> { name };
                patientResource.Gender = (AdministrativeGender)Enum.Parse(typeof(AdministrativeGender), newborn.Sex);
                patientResource.BirthDate = newborn.DOB.ToString("yyyy-MM-dd");
                patientResource.Active = newborn.Active;
                // Update age extension if it exists
                var ageExtension = patientResource.Extension.FirstOrDefault(ext => ext.Url == "http://hl7.org/fhir/StructureDefinition/patient-age");
                if (ageExtension != null)
                {
                    ageExtension.Value = new Hl7.Fhir.Model.Quantity((decimal)newborn.Age, "hours");
                }
                else
                {
                    patientResource.Extension.Add(new Extension
                    {
                        Url = "http://hl7.org/fhir/StructureDefinition/patient-age",
                        Value = new Hl7.Fhir.Model.Quantity((decimal)newborn.Age, "hours")
                    });
                }
                // Update or add the managing organization reference
                var organizationReference = new Hl7.Fhir.Model.ResourceReference($"Organization/{newborn.OrganizationId}");
                if (patientResource.ManagingOrganization == null)
                {
                    patientResource.ManagingOrganization = organizationReference;
                }
                else
                {
                    patientResource.ManagingOrganization.Reference = organizationReference.Reference;
                }
            }

            // Update Observations
            foreach (var entry in bundle.Entry.Where(e => e.Resource is Hl7.Fhir.Model.Observation))
            {
                if (entry.Resource is Hl7.Fhir.Model.Observation observationResource)
                {
                    var code = observationResource.Code.Coding.FirstOrDefault()?.Code;
                    switch (code)
                    {
                        case "40443-4": // Heart rate
                            observationResource.Value = new Hl7.Fhir.Model.Quantity(newborn.HR, "beats/min");
                            break;

                        case "9303-9": // Respiratory rate
                            observationResource.Value = new Hl7.Fhir.Model.Quantity(newborn.RR, "breaths/min");
                            break;

                        case "8310-5": // Body temperature
                            observationResource.Value = new Hl7.Fhir.Model.Quantity((decimal)newborn.T, "C");
                            break;

                        case "29463-7": // Birth weight
                            observationResource.Value = new Hl7.Fhir.Model.Quantity((decimal)newborn.Birth_Weight, "kg");
                            break;

                        case "9272-6": // Apgar scores
                            var apgarScores = observationResource.Component;
                            foreach (var component in apgarScores)
                            {
                                if (component.Code.Text == "1 minute Apgar score")
                                {
                                    component.Value = new Hl7.Fhir.Model.Quantity((decimal)newborn.Apgar_Scores_1min , "score");
                                }
                                else if (component.Code.Text == "5 minute Apgar score")
                                {
                                    component.Value = new Hl7.Fhir.Model.Quantity((decimal)newborn.Apgar_Scores_5min, "score");
                                }
                            }
                            break;

                        case "11884-4": // Gestational age
                            observationResource.Value = new Hl7.Fhir.Model.Quantity((decimal)newborn.Gestational_Age, "weeks");
                            break;

                        case "236973005": // Mode of delivery
                            observationResource.Value = new Hl7.Fhir.Model.CodeableConcept
                            {
                                Coding = new List<Hl7.Fhir.Model.Coding>
                                {
                                    new Hl7.Fhir.Model.Coding
                                    {
                                        Display = newborn.Mode_of_Delivery
                                    }
                                }
                            };
                            break;

                        case "416413003": // Maternal age
                            observationResource.Value = new Hl7.Fhir.Model.Quantity((decimal)newborn.Maternal_age, "years");
                            break;

                        case "56876-6": // Reflexes
                            if (observationResource.Code.Text == "Rooting Reflex")
                            {
                                observationResource.Value = new FhirString(newborn.Rooting_reflex);
                            }
                            else if (observationResource.Code.Text == "Sucking Reflex")
                            {
                                observationResource.Value = new FhirString(newborn.Sucking_reflex);
                            }
                            break;

                        case "404684003": // Fontanelles
                            var fontanelles = observationResource.Component;
                            foreach (var component in fontanelles)
                            {
                                if (component.Code.Text == "Anterior Fontanelle")
                                {
                                    component.Value = new FhirString(newborn.Anterior);
                                }
                                else if (component.Code.Text == "Posterior Fontanelle")
                                {
                                    component.Value = new FhirString(newborn.Posterior);
                                }
                            }
                            break;

                        case "13213009": // Congenital heart disease
                            observationResource.Value = new FhirString(newborn.CHD_CCHD);
                            break;

                        case "232717009": // Resuscitation
                            observationResource.Value = new FhirString(newborn.Resuscitation);
                            break;

                        case "16310003": // Prenatal ultrasound
                            observationResource.Value = new FhirString(newborn.Prenatal_ultrasound);
                            break;

                        case "11977-6": // Parity
                            observationResource.Value = new Hl7.Fhir.Model.Quantity((decimal)newborn.Parity, "count");
                            break;

                        case "29308-4": // Extracardiac Diagnosis
                            observationResource.Value = new FhirString(newborn.Extracardiac_Diagnosis);
                            break;

                        case "78648007": // Sepsis
                            observationResource.Value = new FhirString(newborn.Sepsis);
                            break;

                        case "95837007": // Central cyanosis
                            observationResource.Value = new FhirString(newborn.Cyanosis);
                            break;

                        case "230145002": // Increased work of breathing
                            observationResource.Value = new FhirString(newborn.Increased_work_of_breathing);
                            break;

                        case "363812007": // Head circumference
                            observationResource.Value = new Hl7.Fhir.Model.Quantity((decimal)newborn.HC, "cm");
                            break;

                        case "783819001": // Red reflex
                            observationResource.Value = new FhirString(newborn.Red_reflex);
                            break;

                        case "1303493008": // Choanal atresia
                            observationResource.Value = new FhirString(newborn.Choanal_artesia);
                            break;

                        case "72072-2": // Breath sounds
                            observationResource.Value = new FhirString(newborn.Breath_sounds);
                            break;

                        case "80277-7": // Heart sounds
                            observationResource.Value = new FhirString(newborn.Heart_sounds);
                            break;

                        case "271860004": // Abdominal mass
                            observationResource.Value = new FhirString(newborn.Palpable_masses);
                            break;

                        case "29870000": // Umbilical cord
                            observationResource.Value = new FhirString(newborn.Umbilical_cord);
                            break;

                        case "28028-9": // Anus position/patency
                            observationResource.Value = new FhirString(newborn.Position_patency);
                            break;

                        case "410730009": // Spine alignment
                            observationResource.Value = new FhirString(newborn.Spine_alignment);
                            break;

                        case "311897005": // Sacral dimple
                            observationResource.Value = new FhirString(newborn.Sacral_dimple);
                            break;

                        case "7657000": // Femoral pulses
                            var components = observationResource.Component;
                            foreach (var component in components)
                            {
                                if (component.Code.Text == "Right Femoral Pulse")
                                {
                                    component.Value = new FhirString(newborn.Femoral_pulses_right);
                                }
                                else if (component.Code.Text == "Left Femoral Pulse")
                                {
                                    component.Value = new FhirString(newborn.Femoral_pulses_left);
                                }
                            }
                            break;

                        case "79815-7": // Pupillary response
                            observationResource.Value = new FhirString(newborn.Pupillary_response);
                            break;

                        case "299233007": // Hip dysplasia
                            observationResource.Value = new FhirString(newborn.Hip_dysplasia);
                            break;

                        case "87572000": // Moro reflex
                            observationResource.Value = new FhirString(newborn.Moro_reflex);
                            break;

                        case "e963d516-331e-4e71-b4c3-fdfb245ded25": // Sucking reflex
                            observationResource.Value = new FhirString(newborn.Sucking_reflex);
                            break;

                        case "6918002": // Tone
                            observationResource.Value = new Hl7.Fhir.Model.CodeableConcept
                            {
                                Coding = new List<Hl7.Fhir.Model.Coding>
                                {
                                    new Hl7.Fhir.Model.Coding
                                    {
                                        Display = newborn.Tone
                                    }
                                }
                            };
                            break;

                        case "257733005": // Activity
                            observationResource.Value = new Hl7.Fhir.Model.CodeableConcept
                            {
                                Coding = new List<Hl7.Fhir.Model.Coding>
                                {
                                    new Hl7.Fhir.Model.Coding
                                    {
                                        Display = newborn.Activity
                                    }
                                }
                            };
                            break;

                        case "20564-1": // Oxygen saturation
                            var oxygenComponents = observationResource.Component;
                            foreach (var component in oxygenComponents)
                            {
                                if (component.Code.Text == "Oxygen saturation at Right Upper Arm")
                                {
                                    component.Value = new Hl7.Fhir.Model.Quantity(newborn.Right_upper_arm, "%");
                                }
                                else if (component.Code.Text == "Oxygen saturation at Right Leg")
                                {
                                    component.Value = new Hl7.Fhir.Model.Quantity(newborn.Right_leg, "%");
                                }
                            }
                            break;

                        case "73805-4": // CCHD screening
                            observationResource.Value = new FhirString(newborn.CCHD_first);
                            break;

                        case "39155-7": // Family history
                            var familyHistoryComponents = observationResource.Component;
                            foreach (var component in familyHistoryComponents)
                            {
                                switch (component.Code.Coding.First().Code)
                                {
                                    case "13213009": // Congenital heart disease
                                        component.Value = new FhirString(newborn.Congenital_heart_disease);
                                        break;

                                    case "2598006": // Open heart surgery
                                        component.Value = new FhirString(newborn.Open_heart_surgery);
                                        break;

                                    case "26636000": // Sudden cardiac death
                                        component.Value = new FhirString(newborn.Sudden_cardiac_death);
                                        break;

                                    case "118378005": // Pacemaker
                                        component.Value = new FhirString(newborn.Pacemaker);
                                        break;
                                }
                            }
                            break;

                        case "276720006": // Dysmorphism
                            observationResource.Value = new FhirString(newborn.Dysmorphism);
                            break;

                        case "75321-0": // Others
                            observationResource.Value = new FhirString(newborn.Others);
                            break;

                        case "22026-9": // Plan and follow-up
                            var followUpComponents = observationResource.Component;
                            foreach (var component in followUpComponents)
                            {
                                switch (component.Code.Text)
                                {
                                    case "Discharge":
                                        component.Value = new FhirString(newborn.Discharge);
                                        break;
                                    case "Repeat CCHD":
                                        component.Value = new FhirString(newborn.Repeat_CCHD);
                                        break;
                                    case "Echocardiogram":
                                        component.Value = new FhirBoolean(newborn.Echocardiogram);
                                        break;
                                    case "CXR":
                                        component.Value = new FhirBoolean(newborn.CXR);
                                        break;
                                    case "ECG":
                                        component.Value = new FhirBoolean(newborn.ECG);
                                        break;
                                    case "Ultrasound":
                                        component.Value = new FhirBoolean(newborn.Ultrasound);
                                        break;
                                    case "Follow-up instructions":
                                        component.Value = new FhirString(newborn.Follow_up);
                                        break;
                                    case "Checked by":
                                        component.Value = new FhirString(newborn.Checked_by);
                                        break;
                                }
                            }
                            break;
                    }
                }
            }

            return bundle;
        }
    }
}
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using NeoHearts_API.Models;
using Newtonsoft.Json;

namespace NeoHearts_API.Services
{
    public class FhirDataMappingService : IFhirDataMappingService
    {
        public NewbornModel MapFhirToNewbornModel(Bundle bundle)
        {
            var newborn = new NewbornModel();

            // Extract Patient information
            var patientEntry = bundle.Entry.FirstOrDefault(e => e.Resource is Hl7.Fhir.Model.Patient);
            if (patientEntry?.Resource is Hl7.Fhir.Model.Patient patientResource)
            {
                newborn.FirstName = patientResource.Name.FirstOrDefault()?.Given.FirstOrDefault();
                newborn.LastName = patientResource.Name.FirstOrDefault()?.Family;
                newborn.Sex = patientResource.Gender?.ToString();
                newborn.DOB = DateOnly.FromDateTime(DateTime.Parse(patientResource.BirthDate));
                var ageExtension = patientResource.Extension.FirstOrDefault(ext => ext.Url == "http://hl7.org/fhir/StructureDefinition/patient-age");
                if (ageExtension?.Value is Hl7.Fhir.Model.Integer ageValue)
                {
                    newborn.Age = (int)ageValue.Value;
                }
                // Extract Organization Reference (managingOrganization)
                if (patientResource.ManagingOrganization?.Reference != null)
                {
                    var reference = patientResource.ManagingOrganization.Reference; // Example: "Organization/12345"
                    newborn.OrganizationId = reference.Split('/').Last(); // Extract only the OrganizationId
                }
            }

            // Extract Observations
            foreach (var entry in bundle.Entry.Where(e => e.Resource is Hl7.Fhir.Model.Observation))
            {
                if (entry.Resource is Hl7.Fhir.Model.Observation observationResource)
                {
                    var code = observationResource.Code.Coding.FirstOrDefault()?.Code;
                    switch (code)
                    {
                        case "40443-4": // Heart rate
                            if (observationResource.Value is Hl7.Fhir.Model.Quantity heartRateQuantity)
                                newborn.HR = (int)heartRateQuantity.Value;
                            break;

                        case "9303-9": // Respiratory rate
                            if (observationResource.Value is Hl7.Fhir.Model.Quantity respiratoryRateQuantity)
                                newborn.RR = (int)respiratoryRateQuantity.Value;
                            break;

                        case "8310-5": // Body temperature
                            if (observationResource.Value is Hl7.Fhir.Model.Quantity bodyTemperatureQuantity)
                                newborn.T = (float)bodyTemperatureQuantity.Value;
                            break;

                        case "29463-7": // Birth weight
                            if (observationResource.Value is Hl7.Fhir.Model.Quantity birthWeightQuantity)
                                newborn.Birth_Weight = (float)birthWeightQuantity.Value;
                            break;

                        case "9272-6": // 1-minute Apgar score
                            var apgarScores = observationResource.Component;
                            foreach (var component in apgarScores)
                            {
                                if (component.Code.Text == "1 minute Apgar score")
                                {
                                    if (component.Value is Hl7.Fhir.Model.Quantity apgar1MinQuantity)
                                        newborn.Apgar_Scores_1min = (decimal)apgar1MinQuantity.Value;
                                }
                                else if (component.Code.Text == "5 minute Apgar score")
                                {
                                    if (component.Value is Hl7.Fhir.Model.Quantity apgar5MinQuantity)
                                        newborn.Apgar_Scores_5min = (decimal)apgar5MinQuantity.Value;
                                }
                            }

                            break;

                        case "11884-4": // Gestational age
                            if (observationResource.Value is Hl7.Fhir.Model.Quantity gestationalAgeQuantity)
                                newborn.Gestational_Age = (int)gestationalAgeQuantity.Value;
                            break;

                        case "236973005": // Mode of delivery
                            if (observationResource.Value is Hl7.Fhir.Model.CodeableConcept modeOfDeliveryCodeableConcept)
                                newborn.Mode_of_Delivery = modeOfDeliveryCodeableConcept.Coding.First().Display;
                            break;

                        case "416413003": // Maternal age
                            if (observationResource.Value is Hl7.Fhir.Model.Quantity maternalAgeQuantity)
                                newborn.Maternal_age = (decimal)maternalAgeQuantity.Value;
                            break;

                        case "56876-6":
                            if (observationResource.Code.Text == "Rooting Reflex")
                            {
                                if (observationResource.Value is Hl7.Fhir.Model.FhirString rootingReflexValue)
                                    newborn.Sucking_reflex = rootingReflexValue.Value;
                            }
                            else if (observationResource.Code.Text == "Sucking Reflex")
                            {
                                if (observationResource.Value is Hl7.Fhir.Model.FhirString suckingReflexValue)
                                    newborn.Rooting_reflex = suckingReflexValue.Value;
                            }
                            break;

                        case "404684003": // LOINC code for Anterior Fontanelle
                            var fontanelles = observationResource.Component;
                            foreach (var component in fontanelles)
                            {
                                if (component.Code.Text == "Anterior Fontanelle" && component.Value is Hl7.Fhir.Model.FhirString rightFemoralPulseCodeableConcept)
                                    newborn.Anterior = rightFemoralPulseCodeableConcept.Value;
                                else if (component.Code.Text == "Posterior Fontanelle" && component.Value is Hl7.Fhir.Model.FhirString leftFemoralPulseCodeableConcept)
                                    newborn.Posterior = leftFemoralPulseCodeableConcept.Value;
                            }
                            break;

                        case "13213009": // Congenital heart disease
                            if (observationResource.Value is Hl7.Fhir.Model.FhirString Congenital)
                                newborn.CHD_CCHD = Congenital.Value;
                            break;

                        case "232717009": // Congenital heart disease
                            if (observationResource.Value is Hl7.Fhir.Model.FhirString Resuscitation)
                                newborn.Resuscitation = Resuscitation.Value;
                            break;

                        case "16310003": // Prenatal ultrasound
                            if (observationResource.Value is Hl7.Fhir.Model.FhirString prenatalUltrasound)
                                newborn.Prenatal_ultrasound = prenatalUltrasound.Value;
                            break;


                        case "11977-6": // Congenital heart disease 
                            if (observationResource.Value is Hl7.Fhir.Model.Quantity parityQuantity)
                                newborn.Parity = (decimal)parityQuantity.Value;
                            break;

                        case "29308-4": // Extracardiac Diagnosis
                            if (observationResource.Value is Hl7.Fhir.Model.FhirString extracardiac)
                                newborn.Extracardiac_Diagnosis = extracardiac.Value;
                            break;

                        case "78648007": // Sepsis
                            if (observationResource.Value is Hl7.Fhir.Model.FhirString sepsis)
                                newborn.Sepsis = sepsis.Value;
                            break;

                        case "95837007": // Central cyanosis
                            if (observationResource.Value is Hl7.Fhir.Model.FhirString cyanosisCodeableConcept)
                                newborn.Cyanosis = cyanosisCodeableConcept.Value;
                            break;

                        case "230145002": // Increased work of breathing
                            if (observationResource.Value is Hl7.Fhir.Model.FhirString workOfBreathingCodeableConcept)
                                newborn.Increased_work_of_breathing = workOfBreathingCodeableConcept.Value;
                            break;

                        case "363812007": // Head circumference
                            if (observationResource.Value is Hl7.Fhir.Model.Quantity headCircumferenceQuantity)
                                newborn.HC = (float)headCircumferenceQuantity.Value;
                            break;

                        case "783819001": // Red reflex
                            if (observationResource.Value is Hl7.Fhir.Model.FhirString redReflexCodeableConcept)
                                newborn.Red_reflex = redReflexCodeableConcept.Value;
                            break;

                        case "1303493008": // Choanal atresia
                            if (observationResource.Value is Hl7.Fhir.Model.FhirString choanalAtresiaCodeableConcept)
                                newborn.Choanal_artesia = choanalAtresiaCodeableConcept.Value;
                            break;

                        case "72072-2": // Breath sounds
                            if (observationResource.Value is Hl7.Fhir.Model.FhirString breathSoundsCodeableConcept)
                                newborn.Breath_sounds = breathSoundsCodeableConcept.Value;
                            break;

                        case "80277-7": // Heart sounds
                            if (observationResource.Value is Hl7.Fhir.Model.FhirString heartSoundsCodeableConcept)
                                newborn.Heart_sounds = heartSoundsCodeableConcept.Value;
                            break;

                        case "271860004": // Abdominal mass
                            if (observationResource.Value is Hl7.Fhir.Model.FhirString abdominalMassCodeableConcept)
                                newborn.Palpable_masses = abdominalMassCodeableConcept.Value;
                            break;

                        case "29870000": // Umbilical cord
                            if (observationResource.Value is Hl7.Fhir.Model.FhirString umbilicalCordCodeableConcept)
                                newborn.Umbilical_cord = umbilicalCordCodeableConcept.Value;
                            break;

                        case "28028-9": // Anus position/patency
                            if (observationResource.Value is Hl7.Fhir.Model.FhirString anusPositionCodeableConcept)
                                newborn.Position_patency = anusPositionCodeableConcept.Value;
                            break;

                        case "410730009": // Spine alignment
                            if (observationResource.Value is Hl7.Fhir.Model.FhirString spineAlignmentCodeableConcept)
                                newborn.Spine_alignment = spineAlignmentCodeableConcept.Value;
                            break;

                        case "311897005": // Sacral dimple
                            if (observationResource.Value is Hl7.Fhir.Model.FhirString sacralDimpleCodeableConcept)
                                newborn.Sacral_dimple = sacralDimpleCodeableConcept.Value;
                            break;

                        case "7657000": // Femoral pulses
                            var components = observationResource.Component;
                            foreach (var component in components)
                            {
                                if (component.Code.Text == "Right Femoral Pulse" && component.Value is Hl7.Fhir.Model.FhirString rightFemoralPulseCodeableConcept)
                                    newborn.Femoral_pulses_right = rightFemoralPulseCodeableConcept.Value;
                                else if (component.Code.Text == "Left Femoral Pulse" && component.Value is Hl7.Fhir.Model.FhirString leftFemoralPulseCodeableConcept)
                                    newborn.Femoral_pulses_left = leftFemoralPulseCodeableConcept.Value;
                            }
                            break;

                        case "79815-7":
                            if (observationResource.Value is Hl7.Fhir.Model.FhirString pupillaryResponseValue)
                                newborn.Pupillary_response = pupillaryResponseValue.Value;
                            break;

                        case "299233007": // Hip dysplasia
                            if (observationResource.Value is Hl7.Fhir.Model.FhirString hipDysplasiaCodeableConcept)
                                newborn.Hip_dysplasia = hipDysplasiaCodeableConcept.Value;
                            break;

                        case "87572000": // Moro reflex
                            if (observationResource.Value is Hl7.Fhir.Model.FhirString moroReflexCodeableConcept)
                                newborn.Moro_reflex = moroReflexCodeableConcept.Value;
                            break;
                        case "e963d516-331e-4e71-b4c3-fdfb245ded25": // Sucking reflex
                            if (observationResource.Value is Hl7.Fhir.Model.FhirString suckingReflexCodeableConcept)
                                newborn.Sucking_reflex = suckingReflexCodeableConcept.Value;
                            break;

                        case "6918002": // Tone
                            if (observationResource.Value is Hl7.Fhir.Model.CodeableConcept toneCodeableConcept)
                                newborn.Tone = toneCodeableConcept.Coding.First().Display;
                            break;

                        case "257733005": // Activity
                            if (observationResource.Value is Hl7.Fhir.Model.CodeableConcept activityCodeableConcept)
                                newborn.Activity = activityCodeableConcept.Coding.First().Display;
                            break;

                        case "20564-1": // Oxygen saturation
                            var oxygenComponents = observationResource.Component;
                            foreach (var component in oxygenComponents)
                            {
                                if (component.Code.Text == "Oxygen saturation at Right Upper Arm" && component.Value is Hl7.Fhir.Model.Quantity rightUpperArmQuantity)
                                    newborn.Right_upper_arm = (decimal)rightUpperArmQuantity.Value;
                                else if (component.Code.Text == "Oxygen saturation at Right Leg" && component.Value is Hl7.Fhir.Model.Quantity rightLegQuantity)
                                    newborn.Right_leg = (decimal)rightLegQuantity.Value;
                            }
                            break;

                        case "73805-4": // CCHD screening
                            if (observationResource.Value is Hl7.Fhir.Model.FhirString cchdScreeningCodeableConcept)
                                newborn.CCHD_first = cchdScreeningCodeableConcept.Value;
                            break;

                        case "39155-7":
                            var familyHistoryComponents = observationResource.Component;
                            foreach (var component in familyHistoryComponents)
                            {
                                switch (component.Code.Coding.First().Code)
                                {
                                    case "13213009": // Congenital heart disease
                                        if (component.Value is Hl7.Fhir.Model.FhirString congenitalHeartDiseaseCodeableConcept)
                                            newborn.Congenital_heart_disease = congenitalHeartDiseaseCodeableConcept.Value;
                                        break;

                                    case "2598006": // Open heart surgery
                                        if (component.Value is Hl7.Fhir.Model.FhirString openHeartSurgeryCodeableConcept)
                                            newborn.Open_heart_surgery = openHeartSurgeryCodeableConcept.Value;
                                        break;

                                    case "26636000": // Sudden cardiac death
                                        if (component.Value is Hl7.Fhir.Model.FhirString suddenCardiacDeathCodeableConcept)
                                            newborn.Sudden_cardiac_death = suddenCardiacDeathCodeableConcept.Value;
                                        break;

                                    case "118378005": // Pacemaker
                                        if (component.Value is Hl7.Fhir.Model.FhirString pacemakerCodeableConcept)
                                            newborn.Pacemaker = pacemakerCodeableConcept.Value;
                                        break;

                                }
                            }
                            break;

                        case "276720006": // Dysmorphism
                            if (observationResource.Value is Hl7.Fhir.Model.FhirString dysmorphismCodeableConcept)
                                newborn.Dysmorphism = dysmorphismCodeableConcept.Value;
                            break;

                        case "75321-0": // Others
                            if (observationResource.Value is Hl7.Fhir.Model.FhirString othersCodeableConcept)
                                newborn.Others = othersCodeableConcept.Value;
                            break;

                        case "22026-9": // Plan and follow-up
                            var followUpComponents = observationResource.Component;
                            foreach (var component in followUpComponents)
                            {
                                switch (component.Code.Text)
                                {
                                    case "Discharge":
                                        if (component.Value is Hl7.Fhir.Model.FhirString dischargeCodeableConcept)
                                            newborn.Discharge = dischargeCodeableConcept.Value;
                                        break;
                                    case "Repeat CCHD":
                                        if (component.Value is Hl7.Fhir.Model.FhirString repeatCCHDCodeableConcept)
                                            newborn.Repeat_CCHD = repeatCCHDCodeableConcept.Value;
                                        break;
                                    case "Echocardiogram":
                                        if (component.Value is FhirBoolean echocardiogramBoolean)
                                            newborn.Echocardiogram = echocardiogramBoolean.Value ?? false;
                                        break;
                                    case "CXR":
                                        if (component.Value is FhirBoolean cxrBoolean)
                                            newborn.CXR = cxrBoolean.Value ?? false;
                                        break;
                                    case "ECG":
                                        if (component.Value is FhirBoolean ecgBoolean)
                                            newborn.ECG = ecgBoolean.Value ?? false;
                                        break;
                                    case "Ultrasound":
                                        if (component.Value is FhirBoolean ultrasoundBoolean)
                                            newborn.Ultrasound = ultrasoundBoolean.Value ?? false;
                                        break;
                                    case "Follow-up instructions":
                                        if (component.Value is Hl7.Fhir.Model.FhirString followUpCodeableConcept)
                                            newborn.Follow_up = followUpCodeableConcept.Value;
                                        break;
                                    case "Checked by":
                                        if (component.Value is Hl7.Fhir.Model.FhirString checkedByCodeableConcept)
                                            newborn.Checked_by = checkedByCodeableConcept.Value;
                                        break;
                                }
                            }
                            break;
                    }
                }
            }

            return newborn;
        }
    }
}
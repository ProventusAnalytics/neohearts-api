using System;
using System.ComponentModel.DataAnnotations;

namespace NeoHearts_API.Models
{
    public class NewbornModel
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public required string FirstName { get; set; }

        [ StringLength(50)]
        public string? LastName { get; set; }

        [ RegularExpression("^(Male|Female|Other)$", ErrorMessage = "Sex must be Male, Female, or Other.")]
        public string? Sex { get; set; }

        [Required]
        public DateOnly DOB { get; set; }

        [Range(0, 10, ErrorMessage = "Age must be between 0 and 10.")]
        public int? Age { get; set; } = null;

        [Range(20, 42, ErrorMessage = "Gestational Age must be between 20 and 42 weeks.")]
        public int? Gestational_Age { get; set; }

        [Required, RegularExpression("^(SVD|VAD|LSCS|Other)$", ErrorMessage = "Mode of Delivery must be SVD, VAD, LSCS, or Other.")]
        public string? Mode_of_Delivery { get; set; }

        [Range(0.5, 5.0, ErrorMessage = "Birth Weight must be between 0.5 and 5.0 kg.")]
        public float? Birth_Weight { get; set; }

        [Range(0, 10, ErrorMessage = "Apgar Scores must be between 0 and 10.")]
        public int? Apgar_Scores_1min { get; set; }

        [Range(0, 10, ErrorMessage = "Apgar Scores must be between 0 and 10.")]
        public int? Apgar_Scores_5min { get; set; }

        public string? Resuscitation { get; set; }

        [Range(12, 50, ErrorMessage = "Maternal Age must be between 12 and 50 years.")]
        public int? Maternal_age { get; set; }

        [Range(0, 10, ErrorMessage = "Parity must be between 0 and 10.")]
        public int? Parity { get; set; }

        public string? Prenatal_ultrasound { get; set; }
        public string? CHD_CCHD { get; set; }
        public string? Extracardiac_Diagnosis { get; set; }
        public string? Sepsis { get; set; }

        [Range(30, 200, ErrorMessage = "Heart Rate must be between 30 and 200 bpm.")]
        public int HR { get; set; }

        [Range(10, 100, ErrorMessage = "Respiratory Rate must be between 10 and 100 breaths per minute.")]
        public int RR { get; set; }

        [Range(30, 42, ErrorMessage = "Temperature must be between 30°C and 42°C.")]
        public float T { get; set; }

        public string? Cyanosis { get; set; }
        public string? Increased_work_of_breathing { get; set; }

        [Range(20, 50, ErrorMessage = "Head Circumference must be between 20 and 50 cm.")]
        public float HC { get; set; }

        public string? Anterior { get; set; }
        public string? Posterior { get; set; }
        public string? Pupillary_response { get; set; }
        public string? Red_reflex { get; set; }
        public string? Choanal_artesia { get; set; }
        public string? Breath_sounds { get; set; }
        public string? Heart_sounds { get; set; }
        public string? Palpable_masses { get; set; }
        public string? Umbilical_cord { get; set; }
        public string? Position_patency { get; set; }
        public string? Spine_alignment { get; set; }
        public string? Sacral_dimple { get; set; }
        public string? Femoral_pulses_right { get; set; }
        public string? Femoral_pulses_left { get; set; }
        public string? Hip_dysplasia { get; set; }
        public string? Moro_reflex { get; set; }
        public string? Rooting_reflex { get; set; }
        public string? Sucking_reflex { get; set; }

        [Required]
        public string? Tone { get; set; } 

        public string? Activity { get; set; }

        [Range(0, 100, ErrorMessage = "Right Upper Arm reading must be between 0 and 100.")]
        public int Right_upper_arm { get; set; }

        [Range(0, 100, ErrorMessage = "Right Leg reading must be between 0 and 100.")]
        public int Right_leg { get; set; }

        public string? CCHD_first { get; set; }
        public string? Congenital_heart_disease { get; set; }
        public string? Open_heart_surgery { get; set; }
        public string? Sudden_cardiac_death { get; set; }
        public string? Pacemaker { get; set; }
        public string? Dysmorphism { get; set; }
        public string? Others { get; set; }
        public string? Discharge { get; set; }
        public string? Repeat_CCHD { get; set; }

        public bool Echocardiogram { get; set; } = false;
        public bool CXR { get; set; } = false;
        public bool ECG { get; set; } = false;
        public bool Ultrasound { get; set; } = false;

        public string? Follow_up { get; set; }

        [ StringLength(100)]
        public string? Checked_by { get; set; }
    }
}

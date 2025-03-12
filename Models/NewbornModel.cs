using System;
using System.ComponentModel.DataAnnotations;

namespace NeoHearts_API.Models
{
    public class NewbornModel
    {
        [StringLength(50)]
        public string? FirstName { get; set; }
        [StringLength(50)]
        public string? LastName { get; set; }
        public int Age { get; set; }
        [RegularExpression("^(Male|Female|Other)$", ErrorMessage = "Sex must be Male, Female, or Other.")]
        public string? Sex { get; set; }
        [Required]
        public DateOnly DOB { get; set; }
        public int? Gestational_Age { get; set; }
        //[Required, RegularExpression("^(SVD|VAD|LSCS|Other)$", ErrorMessage = "Mode of Delivery must be SVD, VAD, LSCS, or Other.")]
        public string? Mode_of_Delivery { get; set; }
        public float? Birth_Weight { get; set; }
        public int? Apgar_Scores_1min { get; set; }
        public int? Apgar_Scores_5min { get; set; }
        public string? Resuscitation { get; set; }
        public int? Maternal_age { get; set; }
        public int? Parity { get; set; }
        public string? Prenatal_ultrasound { get; set; }
        public string? CHD_CCHD { get; set; }
        public string? Extracardiac_Diagnosis { get; set; }
        public string? Sepsis { get; set; }
        public int HR { get; set; }
        public int RR { get; set; }
        public float? T { get; set; }
        public string? Cyanosis { get; set; }
        public string? Increased_work_of_breathing { get; set; }
        public float? HC { get; set; }

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
        public string? Tone { get; set; }
        public string? Activity { get; set; }
        public int Right_upper_arm { get; set; }
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

        [Required, StringLength(100)]
        public string? Checked_by { get; set; }
        public string OrganizationId { get; set; }
        public string? OrganizationName { get; set; }
    }
}

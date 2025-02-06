using System.ComponentModel.DataAnnotations;

namespace NeoHearts_API.Models
{
    public class NewbornModel
    {
        [Key] public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Sex { get; set; }
        public DateOnly DOB { get; set; }
        public int? Age { get; set; }
        public int? Gestational_Age { get; set; }
        public string? Mode_of_Delivery { get; set; }
        public float? Birth_Weight { get; set; }
        public int? Apgar_Scores_1min { get; set; }
        public int? Apgar_Scores_5min { get; set; }
        public string? Resuscitation { get; set; }
        public int? Maternal_age { get; set; }
        public int? Parity { get; set; }
        public bool Prenatal_ultrasound { get; set; }
        public bool CHD_CCHD { get; set; }
        public string? Extracardiac_Diagnosis { get; set; }
        public bool Sepsis { get; set; }
        public int HR { get; set; } //heart rate
        public int RR { get; set; }  //respiratory rate
        public float T { get; set; } // temperature
        public bool Cyanosis { get; set; }
        public bool Increased_work_of_breathing { get; set; }
        public float HC { get; set; } // head circumference
        public string? Anterior { get; set; }
        public string? Posterior { get; set; }
        public bool Choanal_artesia { get; set; }
        public string? Breath_sounds { get; set; }
        public string? Heart_sounds { get; set; }
        public string? Palpable_masses { get; set; }
        public string? Umbilical_cord { get; set; }
        public string? Position_patency { get; set; }
        public string? Spine_alignment { get; set; }
        public string? Sacral_dimple { get; set; }
        public bool Femoral_pulses_right { get; set; }
        public bool Femoral_pulses_left { get; set; }
        public bool Hip_dysplasia { get; set; }
        public bool Moro_reflex { get; set; }
        public bool Rooting_reflex { get; set; }
        public bool Sucking_reflex { get; set; }
        public bool Tone { get; set; }
        public bool Activity { get; set; }
        public float Right_upper_arm { get; set; }
        public float Right_leg { get; set; }
        public string? CCHD_first { get; set; }
        public bool Congenital_heart_disease { get; set; }
        public bool Open_heart_surgery { get; set; }
        public bool Sudden_cardiac_death { get; set; }
        public bool Pacemaker { get; set; }
        public string? Dysmorphism { get; set; }
        public string? Others { get; set; }
        public bool Discharge { get; set; }
        public bool Repeat_CCHD { get; set; }
        public bool Echocardiogram { get; set; }
        public bool CXR { get; set; }
        public bool ECG { get; set; }
        public bool Ultrasound { get; set; }
        public string? Follow_up { get; set; }
        public string? Checked_by { get; set; }
    }
}

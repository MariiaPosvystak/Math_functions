using System.ComponentModel.DataAnnotations;

namespace Math_Functions.Models
{
    public class FunktsiooniUurimine
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Valem on kohustuslik")]
        [StringLength(200, ErrorMessage = "Valem on liiga pikk (max 200 märki)")]
        [Display(Name = "Valem f(x)")]
        public string Valem { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Tuletis on liiga pikk (max 200 märki)")]
        [Display(Name = "Tuletis f'(x) (pole kohustuslik)")]
        public string Tuletis { get; set; } = string.Empty;

        [Display(Name = "Määramispiirkond")]
        public string Maaramispiirkond { get; set; } = string.Empty;

        [Display(Name = "Nullkohad")]
        public string Nullkohad { get; set; } = string.Empty;

        [Display(Name = "Kriitilised punktid")]
        public string KriitilisedPunktid { get; set; } = string.Empty;

        [Display(Name = "Ekstreemumid")]
        public string Ekstreemumid { get; set; } = string.Empty;
        public DateTime LuodudAeg { get; set; } = DateTime.UtcNow;
    }
}

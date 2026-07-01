using System.ComponentModel.DataAnnotations;

namespace ExamBuilder.Models;

public class McAntwort
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Bitte geben Sie einen Antworttext ein.")]
    [Display(Name = "Antwort")]
    public string Antworttext { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bitte geben Sie an, ob die Antwort korrekt ist.")]
    [Display(Name = "Korrekte Antwort")]
    public bool Korrekt { get; set; }

    public int McFrageId { get; set; }
    public McFrage McFrage { get; set; } = null!;
}

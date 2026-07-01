using System.ComponentModel.DataAnnotations;

namespace ExamBuilder.Models;

public class Pruefung
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Bitte geben Sie einen Titel ein.")]
    [Display(Name = "Titel")]
    public string Titel { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bitte geben Sie ein Prüfungsdatum ein.")]
    [Display(Name = "Prüfungsdatum")]
    public DateTime Datum { get; set; }

    [Display(Name = "Beschreibung")]
    public string? Beschreibung { get; set; }

    public int LehrveranstaltungId { get; set; }
    public Lehrveranstaltung Lehrveranstaltung { get; set; } = null!;
    public ICollection<McFrage> McFragen { get; set; } = new List<McFrage>();
}

using System.ComponentModel.DataAnnotations;

namespace ExamBuilder.Models;

public class Kapitel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Bitte geben Sie einen Titel ein.")]
    [Display(Name = "Titel")]
    public string Titel { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bitte geben Sie eine Kapitelnummer ein.")]
    [Display(Name = "Kapitel-Nr.")]
    public int Kapitelnummer { get; set; }

    [Display(Name = "Vorlesungsfolien")]
    public string? Vorlesungsfolien { get; set; }

    public int LehrveranstaltungId { get; set; }
    public Lehrveranstaltung Lehrveranstaltung { get; set; } = null!;
    public ICollection<McFrage> McFragen { get; set; } = new List<McFrage>();
}

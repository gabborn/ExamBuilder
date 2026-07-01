using System.ComponentModel.DataAnnotations;

namespace ExamBuilder.Models;

public class Lehrveranstaltung
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Bitte geben Sie einen Titel ein.")]
    [Display(Name = "Titel")]
    public string Titel { get; set; } = string.Empty;

    [Display(Name = "Dozent")]
    public string? Dozentenname { get; set; }

    [Required(ErrorMessage = "Bitte wählen Sie eine Abschlussart aus.")]
    [Display(Name = "Abschluss")]
    public Abschlussart Abschlussart { get; set; }

    public ICollection<Kapitel> Kapitel { get; set; } = new List<Kapitel>();
    public ICollection<Pruefung> Pruefungen { get; set; } = new List<Pruefung>();
}

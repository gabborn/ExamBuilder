using System.ComponentModel.DataAnnotations;

namespace ExamBuilder.Models;

public class McFrage
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Bitte geben Sie einen Fragetext ein.")]
    [Display(Name = "Frage")]
    public string Fragentext { get; set; } = string.Empty;

    public int KapitelId { get; set; }
    public Kapitel Kapitel { get; set; } = null!;
    public ICollection<McAntwort> McAntworten { get; set; } = new List<McAntwort>();
    public ICollection<Pruefung> Pruefungen { get; set; } = new List<Pruefung>();
}

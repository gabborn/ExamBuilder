using Microsoft.AspNetCore.Identity;

namespace ExamBuilder.Models;

public class ExamBuilderUser : IdentityUser
{
    public string? Vorname { get; set; }
    public string? Nachname { get; set; }
}

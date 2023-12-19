using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Sbt.Models;

public class Division
{
    // note: this domain object is also used directly as a view model,
    // so display-centric attributes are included.

    [RegularExpression(@"^[a-zA-Z0-9]+[ a-zA-Z0-9-_]*$")]
    public string Organization { get; set; } = string.Empty;

    [Required]
    //[Comment("short string version used in URLs - must be unique within an Organization")]
    [RegularExpression(@"^[a-zA-Z]+[a-zA-Z0-9-_]*$", ErrorMessage = "Allowed: digits, letters, dash, and underline.")]
    [StringLength(50, MinimumLength = 2)]
    [Remote("IsDivisionAbbreviationAvailable", "Divisions", ErrorMessage = "Abbreviation in use.",
        AdditionalFields = nameof(Organization))]
    [Key]
    public string Abbreviation { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^[a-zA-Z0-9]+[ a-zA-Z0-9-_]*$", ErrorMessage = "Allowed: digits, letters, dash, underline, and spaces.")]
    public string League { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Name")]
    [RegularExpression(@"^[a-zA-Z0-9]+[ a-zA-Z0-9-_]*$", ErrorMessage = "Allowed: digits, letters, dash, underline, and spaces.")]
    public string NameOrNumber { get; set; } = string.Empty;

    [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy h:mm tt}", ApplyFormatInEditMode = false)]
    public DateTime Updated { get; set; }

    //[Comment("Locked means that scores can no longer be reported")]
    public bool Locked { get; set; }

    public List<Standings> Standings { get; set; } = new List<Standings>();

    public List<Schedule> Schedule { get; set; } = new List<Schedule>();
}

using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Sbt.Models.ViewModels;

public class LoadScheduleViewModel
{

    [Required]
    [RegularExpression(@"^[a-zA-Z]+[a-zA-Z0-9-_]*$", ErrorMessage = "Allowed: digits, letters, dash, and underline.")]
    [StringLength(50, MinimumLength = 2)]
    public string DivisionID { get; set; } = string.Empty;

    [Required]
    [DisplayName("Schedule File")]
    public IFormFile ScheduleFile { get; set; } = default!;

    public bool UsesDoubleHeaders { get; set; } = false;

    public bool ResultSuccess { get; set; } = false;

    public string ResultMessage { get; set; } = string.Empty;
}

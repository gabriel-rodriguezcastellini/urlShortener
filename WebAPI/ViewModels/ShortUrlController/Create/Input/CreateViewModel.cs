using System.ComponentModel.DataAnnotations;

namespace WebAPI.ViewModels.ShortUrlController.Create.Input;

public class CreateViewModel
{
    [Required]
    public string Destination { get; set; } = null!;

    [StringLength(10)]
    [RegularExpression("^[a-zA-Z0-9_-]*$")]
    public string? Path { get; set; }
}

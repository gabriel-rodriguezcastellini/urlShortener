using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models;

public class ShortUrlDto
{
    [Required]
    public string Destination { get; set; } = string.Empty;
    
    [StringLength(10)]
    [RegularExpression("^[a-zA-Z0-9_-]*$")]
    public string? Path { get; set; }
}
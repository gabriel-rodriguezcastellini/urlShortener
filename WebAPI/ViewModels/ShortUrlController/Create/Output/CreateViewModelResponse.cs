using System.ComponentModel.DataAnnotations;

namespace WebAPI.ViewModels.ShortUrlController.Create.Output;

public class CreateViewModelResponse
{    
    public string Destination { get; set; } = null!;
    
    public string Path { get; set; } = null!;
}

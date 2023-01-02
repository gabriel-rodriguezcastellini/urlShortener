using Microsoft.Build.Framework;

namespace WebAPI.ViewModels.ShortUrlController.Get.Input;

public class GetViewModel
{
    [Required]
    public string Path { get; set; } = null!;
}

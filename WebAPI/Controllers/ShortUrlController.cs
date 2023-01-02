using Data;
using Microsoft.AspNetCore.Mvc;
using WebAPI.ViewModels.ShortUrlController.Create.Input;
using WebAPI.ViewModels.ShortUrlController.Create.Output;
using WebAPI.ViewModels.ShortUrlController.Get.Input;
using WebAPI.ViewModels.ShortUrlController.Get.Output;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class ShortUrlController : ControllerBase
{
    private readonly ShortUrlRepository ShortUrlRepository;
    private readonly RandomUrlGenerator ShortUrlGenerator;

    public ShortUrlController(ShortUrlRepository shortUrlRepository, RandomUrlGenerator shortUrlGenerator)
    {
        ShortUrlRepository = shortUrlRepository;
        ShortUrlGenerator = shortUrlGenerator;
    }

    // GET api/<ShortUrlController>/5
    [HttpPost("/[controller]/get-path")]
    [ActionName(nameof(GetAsync))]
    public async Task<ActionResult<GetViewModelResponse>> GetAsync([FromBody] GetViewModel model)
    {
        var shortUrl = await ShortUrlRepository.GetAsync(model.Path);
        if (shortUrl == null)
        {
            return NotFound();
        }
#pragma warning disable CS8601 // Possible null reference assignment.
        var response = new GetViewModelResponse()
        {
            Path = shortUrl?.Path,
            Destination = shortUrl?.Destination
        };
#pragma warning restore CS8601 // Possible null reference assignment.
        return  Ok(response);
    }

    // POST api/<ShortUrlController>
    [HttpPost]
    public async Task<ActionResult<CreateViewModelResponse>> CreateAsync([FromBody] CreateViewModel model)
    {
        var shortUrl = new ShortUrl(model.Destination, model.Path ?? ShortUrlGenerator.Generate());
        await ShortUrlRepository.CreateAsync(shortUrl);
        var response = new CreateViewModelResponse
        {
            Destination = shortUrl.Destination,
            Path = shortUrl.Path
        };
        return CreatedAtAction(nameof(GetAsync), new { path = shortUrl.Path }, response);
    }

    // DELETE api/<ShortUrlController>/5
    [HttpDelete("{path}")]
    public async Task<IActionResult> DeleteAsync(string path)
    {
        var shortUrl = await ShortUrlRepository.GetAsync(path);
        if (shortUrl == null)
        {
            return NotFound();
        }
        await ShortUrlRepository.DeleteAsync(path);
        return NoContent();
    }    
}

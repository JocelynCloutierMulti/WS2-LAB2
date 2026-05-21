using FR_WS2_BaseLab.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
 
namespace FR_WS2_BaseLab.Controllers;
[Route("categories/{categoryId:int}/images")]
 
public class CategoryImagesController : Controller
{
    private readonly ICategoryImageService _imageService;
    public CategoryImagesController(ICategoryImageService imageService)
    {
        _imageService = imageService;
    }

    
    // POST  cat/ img /upload
    [HttpPost("upload")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(int categoryId, IFormFile file)
    {   

        var result = await _imageService.UploadAsync(categoryId, file);
        if (!result.Succeeded)
            return BadRequest(new { success = false, message = result.Error });

        return Ok(new { success = true, data = result.Value });
    }

    //  delete / cat /image
    [HttpDelete("{imageId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int categoryId, int imageId)
    {
        var result = await _imageService.DeleteAsync(imageId);
        if (!result.Succeeded)
            return NotFound(new { success = false, message = result.Error });
 
        return Ok(new { success = true });
    }
}


 
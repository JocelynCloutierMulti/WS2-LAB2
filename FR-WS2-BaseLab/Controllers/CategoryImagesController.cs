using FR_WS2_BaseLab.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[Route("categories/{categoryId:int}/images")]
public class CategoryImagesController : Controller
{
    private readonly ICategoryImageService _imageService;

    public CategoryImagesController(ICategoryImageService imageService)
    {
        _imageService = imageService;
    }

    [HttpGet]
    public async Task<IActionResult> List(int categoryId)
    {
        var images = await _imageService.GetForCategoryAsync(categoryId);
        return Json(images);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(
        int categoryId,
        IFormFile file,
        string? altText)
    {
        var result = await _imageService.UploadAsync(
            categoryId,
            file,
            altText);

        if (!result.Success)
            return BadRequest(new { error = result.ErrorMessage });

        return Json(result.Value);
    }

    [HttpDelete("{imageId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int categoryId, int imageId)
    {
        var result = await _imageService.DeleteAsync(imageId);

        if (!result.Success)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(new { success = true });
    }
}

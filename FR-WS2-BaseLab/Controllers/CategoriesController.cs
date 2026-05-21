using FR_WS2_BaseLab.Models;
using FR_WS2_BaseLab.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FR_WS2_BaseLab.Controllers;

[Authorize(Roles = "Admin")]

public class CategoriesController(ICategoryService categoryService, ICategoryImageService categoryImageService) : Controller
{
    private readonly ICategoryService _categoryService = categoryService;
    private readonly ICategoryImageService _categoryImageService = categoryImageService;


    // GET: Categories
    /// <summary>Affiche tableaux des catégories.</summary>
    /// <returns>Vue contenant liste des catégories.</returns>
    public async Task<IActionResult> Index()
    {
        var result = await _categoryService.GetAllAsync();
        if (!result.Succeeded){
            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToAction(nameof(Index), "Home");}
        return View(result.Value);
    }

    // GET: Categories/Details/5
    /// <summary>Affiche détails d'une catégorie spécifique.</summary>
    /// <param name="id">Id catégorie.</param>
    /// <returns>Vue contenant détails de la catégorie.</returns>
    public async Task<IActionResult> Details(int ? id)
    {
       if (id is null) return NotFound();
        var result = await _categoryService.GetByIdAsync(id.Value);
        if (!result.Succeeded){
            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToAction(nameof(Index), "Home");}
        return View(result.Value);
    }

    // GET: Categories/CreateCategorie
    /// <summary>Affiche formulaire qui créer catégorie.</summary>
    /// <returns>Vue contenant formulaire de création.</returns>
    public IActionResult CreateCategorie()
    {
        return View();
    }

    /// POST: Categories/CreateCategorie
    /// <summary>Traite soumission du formulaire de création d'une catégorie.</summary>
    /// <param name="category">Catégorie à créer.</param>
    /// <returns>Objet JSON indiquant succès ou échec de la création.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategorie([Bind("Name,Description")] Category category)
    {
        if (!ModelState.IsValid){return Json(new{success = false,message = "Formulaire invalide."});}
        var result = await _categoryService.CreateAsync(category);
        if (!result.Succeeded){return Json(new{success = false,message = result.ErrorMessage});}
        return Json(new{success = true,categoryId = result.Value!.Id});
    }

    // GET: Categories/Edit/5
    /// <summary>Affiche formulaire pour modifier catégorie et gérer images associées.</summary>
    /// <param name="id">Id catégorie à modifier.</param>
    /// <returns>Vue contenant formulaire de modification.</returns>
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var result = await _categoryService.GetByIdAsync(id.Value);
        if (!result.Succeeded){
            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToAction(nameof(Index));}
        var imagesResult = await _categoryImageService.GetImagesByCategoryAsync(id.Value);
        ViewBag.CategoryImages = imagesResult.Succeeded
        ? new SelectList(imagesResult.Value, "Id", "OriginalFileName")
        : new SelectList(new List<CategoryImage>(), "Id", "OriginalFileName");
        return View(result.Value);
    }

    // POST: Categories/Edit/5
    /// <summary>Traite soumission du formulaire de modification d'une catégorie.</summary>
    /// <param name="id">Id catégorie à modifier.</param>
    /// <param name="category">Catégorie modifiée.</param>
    /// <param name="imagePrincipaleId">Id image principale à définir.</param>
    /// <param name="imageASupprimerId">Id image à supprimer.</param>
    /// <returns>Redirige vers index catégories ou retourne vue avec erreurs.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Inactive,Name,Description")]
        Category category, int? imagePrincipaleId, int? imageASupprimerId)
    {
        if (id != category.Id) return NotFound();
        if (!ModelState.IsValid) return View(category);
        var result = await _categoryService.UpdateAsync(id, category);
        if (!result.Succeeded){
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            return View(category);}
        if (imagePrincipaleId.HasValue){
            var resultMainImage = await _categoryImageService.SetMainImageAsync(id, imagePrincipaleId.Value);
            if (!resultMainImage.Succeeded){
                ModelState.AddModelError(string.Empty, resultMainImage.ErrorMessage!);
                return View(category);}}
        if (imageASupprimerId.HasValue){
            var resultDeleteImage = await _categoryImageService.DeleteAsync(imageASupprimerId.Value);
            if (!resultDeleteImage.Succeeded){
                ModelState.AddModelError(string.Empty, resultDeleteImage.ErrorMessage!);
                return View(category);}}
        return RedirectToAction(nameof(Index));
    }

    // GET: Categories/Delete/5
    /// <summary>Affiche confirmation de suppression d'une catégorie.</summary>
    /// <param name="id">Id catégorie à supprimer.</param>
    /// <returns>Vue contenant confirmation de suppression.</returns>
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var result = await _categoryService.GetByIdAsync(id.Value);
        if (!result.Succeeded){
            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToAction(nameof(Index));}
        return View(result.Value);
    }

    // POST: Categories/Delete/5
    /// <summary>Traite confirmation de suppression d'une catégorie.</summary>
    /// <param name="id">Id catégorie à supprimer.</param>
    /// <returns>Redirige vers index catégories ou retourne vue avec erreurs.</returns>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
         var imagesResult = await _categoryImageService.DeleteAllAsync(id);
        if (!imagesResult.Succeeded){
            TempData["ErrorMessage"] = imagesResult.ErrorMessage;
            return RedirectToAction(nameof(Delete), new { id });}
        var result = await _categoryService.DeleteAsync(id);
        if (!result.Succeeded){
            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToAction(nameof(Delete), new { id });}
       return RedirectToAction(nameof(Index));
    }

    /// POST: Categories/UploadImage
    /// <summary>Traite le formulaire d'upload d'image pour une catégorie.</summary>
    /// <param name="categoryId">Id catégorie de l'image uploadée.</param>
    /// <param name="imageFile">Fichier image à uploader.</param>
    /// <returns>Objet JSON indiquant succès ou échec de l'upload.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadImage(int categoryId, IFormFile imageFile)
    {
        if (imageFile == null || imageFile.Length == 0){
            return Json(new {success = false, message = "Aucune image reçue."});}
        var category = new Category { Id = categoryId };
        var result = await _categoryImageService.CreateAsync(category, imageFile);
        if (!result.Succeeded){ return Json(new {success = false, message = result.ErrorMessage});}
        return Json(new {
            success = true,
            fileName = result.Value!.FileName,
            imageId = result.Value.Id,
            url = Url.Content("~/images/img_categories/" + result.Value.FileName)});
    }
}
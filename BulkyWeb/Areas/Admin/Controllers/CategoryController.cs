using Bulky.DataAccess.Repositories.Interfaces;
using Bulky.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Admin.Controllers;

[Area("Admin")]
public class CategoryController : Controller
{
	private readonly IUnitOfWork _unitOfWork;

	public CategoryController(IUnitOfWork unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	public IActionResult Index()
	{
		var categories = _unitOfWork.CategoryRepository.GetList();
		return View(categories);
	}

	public IActionResult Create()
	{
		return View();
	}

	[HttpPost]
	public IActionResult Create(Category category)
	{
		if (string.Equals(category.Name, category.DisplayOrder.ToString()))
		{
			ModelState.AddModelError("Name", "The Display Order cannot exactly match the Name");
		}

		if (ModelState.IsValid)
		{
			_unitOfWork.CategoryRepository.Add(category);
			_unitOfWork.Save();
			TempData["success"] = "Category created successfully";
			return RedirectToAction("Index");
		}

		return View();
	}

	public IActionResult Edit(int id)
	{
		if (id <= 0) return NotFound();

		var category = _unitOfWork.CategoryRepository.Get(c => c.Id == id);
		if (category == null) return NotFound();

		return View(category);
	}

	[HttpPost]
	public IActionResult Edit(Category category)
	{
		if (ModelState.IsValid)
		{
			_unitOfWork.CategoryRepository.Update(category);
			_unitOfWork.Save();
			TempData["success"] = "Category updated successfully";
			return RedirectToAction("Index");
		}

		return View();
	}

	public IActionResult Delete(int id)
	{
		if (id <= 0) return NotFound();

		var category = _unitOfWork.CategoryRepository.Get(c => c.Id == id);
		if (category == null) return NotFound();

		return View(category);
	}

	[HttpPost]
	[ActionName("Delete")]
	public IActionResult DeleteCategory(int id)
	{
		if (id <= 0) return NotFound();

		var category = _unitOfWork.CategoryRepository.Get(c => c.Id == id);
		if (category == null) return NotFound();

		_unitOfWork.CategoryRepository.Remove(category);
		_unitOfWork.Save();
		TempData["success"] = "Category deleted successfully";
		return RedirectToAction("Index");
	}
}

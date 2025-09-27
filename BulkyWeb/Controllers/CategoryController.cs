using Bulky.DataAccess.Data;
using Bulky.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace BulkyWeb.Controllers;

public class CategoryController : Controller
{
	private readonly ILogger<CategoryController> _logger;
	private readonly ApplicationDbContext _dbContext;

	public CategoryController(ILogger<CategoryController> logger, ApplicationDbContext dbContext)
	{
		_logger = logger;
		_dbContext = dbContext;
	}

	public IActionResult Index()
	{
		var categories = _dbContext.Categories.AsNoTracking().ToList();
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
			_dbContext.Categories.Add(category);
			_dbContext.SaveChanges();
			TempData["success"] = "Category created successfully";
			return RedirectToAction("Index");
		}

		return View();
	}

	public IActionResult Edit(int id)
	{
		if (id <= 0) return NotFound();

		var category = _dbContext.Categories.FirstOrDefault(c => c.Id == id);
		if (category == null) return NotFound();

		return View(category);
	}

	[HttpPost]
	public IActionResult Edit(Category category)
	{
		if (ModelState.IsValid)
		{
			_dbContext.Categories.Update(category);
			_dbContext.SaveChanges();
			TempData["success"] = "Category updated successfully";
			return RedirectToAction("Index");
		}

		return View();
	}

	public IActionResult Delete(int id)
	{
		if (id <= 0) return NotFound();

		var category = _dbContext.Categories.FirstOrDefault(c => c.Id == id);
		if (category == null) return NotFound();

		return View(category);
	}

	[HttpPost]
	[ActionName("Delete")]
	public IActionResult DeleteCategory(int id)
	{
		if (id <= 0) return NotFound();

		var category = _dbContext.Categories.FirstOrDefault(c => c.Id == id);
		if (category == null) return NotFound();

		_dbContext.Categories.Remove(category);
		_dbContext.SaveChanges();
		TempData["success"] = "Category deleted successfully";
		return RedirectToAction("Index");
	}
}

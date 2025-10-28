using Bulky.DataAccess.Repositories.Interfaces;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.IO;
using System.Linq;

namespace BulkyWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = SD.Role_Admin)]
public class ProductController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _rootPath;

    public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
    {
        _unitOfWork = unitOfWork;
        _rootPath = webHostEnvironment.WebRootPath;
    }

    public IActionResult Index()
    {
        var products = _unitOfWork.ProductRepository.GetList("Category");

        return View(products);
    }

    [HttpGet]
    public IActionResult Upsert(int? id)
    {
        var productVM = new ProductVM
        {
            CategoryList = _unitOfWork.CategoryRepository.GetList().Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString()
            }),
            Product = new Product()
        };

        if (id == null || id == 0)
        {
            return View(productVM);
        }

        var product = _unitOfWork.ProductRepository.Get(c => c.Id == id, "Category");
        if (product == null) return NotFound();

        productVM.Product = product;
        return View(productVM);
    }

    [HttpPost]
    public IActionResult Upsert(ProductVM productVM, IFormFile file)
    {
        if (ModelState.IsValid)
        {
            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string productPath = Path.Combine(_rootPath, @"img\product");

                using var stream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create);
                file.CopyTo(stream);

                if (string.IsNullOrWhiteSpace(productVM.Product.ImageUrl))
                {
                    productVM.Product.ImageUrl = $"\\img\\product\\{fileName}";
                }
                else
                {
                    var oldImagePath = Path.Combine(_rootPath, productVM.Product.ImageUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }

                    productVM.Product.ImageUrl = $"\\img\\product\\{fileName}";
                }
            }

            if (productVM.Product.Id == 0)
            {
                _unitOfWork.ProductRepository.Add(productVM.Product);
                TempData["success"] = "Product created successfully";
            }
            else
            {
                _unitOfWork.ProductRepository.Update(productVM.Product);
                TempData["success"] = "Product updated successfully";
            }

            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        productVM.CategoryList = _unitOfWork.CategoryRepository.GetList().Select(c => new SelectListItem
        {
            Text = c.Name,
            Value = c.Id.ToString()
        });

        return View(productVM);
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        if (id <= 0) return NotFound();

        var product = _unitOfWork.ProductRepository.Get(c => c.Id == id);
        if (product == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(product.ImageUrl))
        {
            var imagePath = Path.Combine(_rootPath, product.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
        }

        _unitOfWork.ProductRepository.Remove(product);
        _unitOfWork.Save();
        TempData["success"] = "Product deleted successfully";
        return RedirectToAction("Index");
    }
}

using Bulky.DataAccess.Repositories.Interfaces;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = SD.Role_Admin)]
public class CompanyController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _rootPath;

    public CompanyController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
    {
        _unitOfWork = unitOfWork;
        _rootPath = webHostEnvironment.WebRootPath;
    }

    public IActionResult Index()
    {
        var companies = _unitOfWork.CompanyRepository.GetList();

        return View(companies);
    }

    [HttpGet]
    public IActionResult Upsert(int? id)
    {
        if (id == null || id == 0)
        {
            return View(new Company());
        }

        var company = _unitOfWork.CompanyRepository.Get(c => c.Id == id);
        if (company == null) return NotFound();

        return View(company);
    }

    [HttpPost]
    public IActionResult Upsert(Company company)
    {
        if (ModelState.IsValid)
        {

            if (company.Id == 0)
            {
                _unitOfWork.CompanyRepository.Add(company);
                TempData["success"] = "Company created successfully";
            }
            else
            {
                _unitOfWork.CompanyRepository.Update(company);
                TempData["success"] = "Company updated successfully";
            }

            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        return View(company);
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        if (id <= 0) return NotFound();

        var company = _unitOfWork.CompanyRepository.Get(c => c.Id == id);
        if (company == null) return NotFound();

        _unitOfWork.CompanyRepository.Remove(company);
        _unitOfWork.Save();
        TempData["success"] = "Company deleted successfully";
        return RedirectToAction("Index");
    }
}

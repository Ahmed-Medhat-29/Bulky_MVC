using Bulky.DataAccess.Repositories.Interfaces;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyWeb.Areas.Customer.Controllers;

[Authorize]
[Area("Customer")]
public class CartController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public CartController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        var model = new ShoppingCartVM
        {
            ShoppingCartList = _unitOfWork.ShoppingCart.GetList(c => c.ApplicationUserId == userId, "Product")
        };

        foreach (var cart in model.ShoppingCartList)
        {
            cart.Price = GetPriceBasedOnQuantity(cart);
            model.OrderTotal += (cart.Price * cart.Count);
        }

        return View(model);
    }

    public IActionResult Summary()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        

        return View();
    }

    public IActionResult Plus(int cartId)
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        var cartFromDb = _unitOfWork.ShoppingCart.Get(c => c.Id == cartId && c.ApplicationUserId == userId);
        cartFromDb.Count += 1;

        _unitOfWork.ShoppingCart.Update(cartFromDb);
        _unitOfWork.Save();

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Minus(int cartId)
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        var cartFromDb = _unitOfWork.ShoppingCart.Get(c => c.Id == cartId && c.ApplicationUserId == userId);
        cartFromDb.Count -= 1;

        if (cartFromDb.Count == 0)
        {
            _unitOfWork.ShoppingCart.Remove(cartFromDb);
        }
        else
        {
            _unitOfWork.ShoppingCart.Update(cartFromDb);
        }

        _unitOfWork.Save();

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Remove(int cartId)
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        var cartFromDb = _unitOfWork.ShoppingCart.Get(c => c.Id == cartId && c.ApplicationUserId == userId);

        _unitOfWork.ShoppingCart.Remove(cartFromDb);
        _unitOfWork.Save();

        return RedirectToAction(nameof(Index));
    }

    private static double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
    {
        if (shoppingCart.Count <= 50)
        {
            return shoppingCart.Product.Price;
        }
        else if (shoppingCart.Count <= 100)
        {
            return shoppingCart.Product.Price50;
        }
        else
        {
            return shoppingCart.Product.Price100;
        }
    }
}

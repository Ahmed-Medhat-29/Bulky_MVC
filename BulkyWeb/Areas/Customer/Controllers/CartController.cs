using Bulky.DataAccess.Repositories.Interfaces;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
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
            ShoppingCartList = _unitOfWork.ShoppingCart.GetList(c => c.ApplicationUserId == userId, "Product"),
            OrderHeader = new()
        };

        foreach (var cart in model.ShoppingCartList)
        {
            cart.Price = GetPriceBasedOnQuantity(cart);
            model.OrderHeader.OrderTotal += (cart.Price * cart.Count);
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Summary()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        var model = new ShoppingCartVM
        {
            ShoppingCartList = _unitOfWork.ShoppingCart.GetList(c => c.ApplicationUserId == userId, "Product"),
            OrderHeader = new()
        };

        model.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(U => U.Id == userId);

        model.OrderHeader.Name = model.OrderHeader.ApplicationUser.Name;
        model.OrderHeader.PhoneNumber = model.OrderHeader.ApplicationUser.PhoneNumber;
        model.OrderHeader.StreetAddress = model.OrderHeader.ApplicationUser.StreetAddress;
        model.OrderHeader.City = model.OrderHeader.ApplicationUser.City;
        model.OrderHeader.State = model.OrderHeader.ApplicationUser.State;
        model.OrderHeader.PostalCode = model.OrderHeader.ApplicationUser.PostalCode;

        foreach (var cart in model.ShoppingCartList)
        {
            cart.Price = GetPriceBasedOnQuantity(cart);
            model.OrderHeader.OrderTotal += (cart.Price * cart.Count);
        }

        return View(model);
    }

    [HttpPost]
    [ActionName("Summary")]
    public IActionResult SummaryPOST(ShoppingCartVM model)
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        model.ShoppingCartList = _unitOfWork.ShoppingCart.GetList(c => c.ApplicationUserId == userId, "Product");

        model.OrderHeader.OrderDate = DateTime.Now;
        model.OrderHeader.ApplicationUserId = userId;
        var applicationUser = _unitOfWork.ApplicationUser.Get(U => U.Id == userId);

        foreach (var cart in model.ShoppingCartList)
        {
            cart.Price = GetPriceBasedOnQuantity(cart);
            model.OrderHeader.OrderTotal += (cart.Price * cart.Count);
        }

        if (applicationUser.CompanyId.GetValueOrDefault() == 0)
        {
            model.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            model.OrderHeader.OrderStatus = SD.StatusPending;
        }
        else
        {
            model.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
            model.OrderHeader.OrderStatus = SD.StatusApproved;
        }

        _unitOfWork.OrderHeader.Add(model.OrderHeader);
        _unitOfWork.Save();

        foreach (var cart in model.ShoppingCartList)
        {
            var orderDetails = new OrderDetail
            {
                ProductId = cart.ProductId,
                OrderHeaderId = model.OrderHeader.Id,
                Count = cart.Count,
                Price = cart.Price
            };

            _unitOfWork.OrderDetail.Add(orderDetails);
        }

        _unitOfWork.Save();

        if (applicationUser.CompanyId.GetValueOrDefault() == 0)
        {
            var options = new SessionCreateOptions
            {
                SuccessUrl = Url.Action("OrderConfirmation", "Cart", new { area = "customer", id = model.OrderHeader.Id }, Request.Scheme),
                CancelUrl = Url.Action("Index", "Cart", new { area = "customer" }, Request.Scheme),
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in model.ShoppingCartList)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count
                };

                options.LineItems.Add(sessionLineItem);
            }

            var service = new SessionService();
            var session = service.Create(options);
            _unitOfWork.OrderHeader.UpdateStripePaymentId(model.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();

            Response.Headers.Append("Location", session.Url);
            return new StatusCodeResult(303);
        }

        return RedirectToAction(nameof(OrderConfirmation), new { model.OrderHeader.Id });
    }

    public IActionResult OrderConfirmation(int id)
    {
        var orderHeader = _unitOfWork.OrderHeader.Get(o => o.Id == id, includeProperties: "ApplicationUser");
        
        if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
        {
            var service = new SessionService();
            var session = service.Get(orderHeader.SessionId);

            if (session.PaymentStatus.Equals("paid", StringComparison.CurrentCultureIgnoreCase))
            {
                _unitOfWork.OrderHeader.UpdateStripePaymentId(orderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusApproved, SD.PaymentStatusApproved);
                _unitOfWork.Save();
            }
        }

        var shoppingCarts = _unitOfWork.ShoppingCart.GetList(u => u.ApplicationUserId == orderHeader.ApplicationUserId);
        _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
        _unitOfWork.Save();

        return View(id);
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

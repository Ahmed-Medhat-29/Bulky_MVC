using Bulky.DataAccess.Repositories.Interfaces;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Claims;

namespace BulkyWeb.Areas.Admin.Controllers;

[Authorize]
[Area("Admin")]
public class OrderController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public IActionResult Index([FromQuery] string status)
    {
        Expression<Func<OrderHeader, bool>> statusFilter = o => true;
        Expression<Func<OrderHeader, bool>> userIdFilter = o => true;

        if (User.IsInRole(SD.Role_Company) || User.IsInRole(SD.Role_Customer))
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            userIdFilter = o => o.ApplicationUserId == userId;
        }

        statusFilter = status switch
        {
            "pending" => o => o.PaymentStatus == SD.PaymentStatusPending,
            "inprocess" => o => o.OrderStatus == SD.StatusInProcess,
            "completed" => o => o.OrderStatus == SD.StatusShipped,
            "approved" => o => o.OrderStatus == SD.StatusApproved,
            _ => o => true,
        };

        var parameter = Expression.Parameter(typeof(OrderHeader));

        var body = Expression.AndAlso(
            Expression.Invoke(statusFilter, parameter),
            Expression.Invoke(userIdFilter, parameter));

        var filters = Expression.Lambda<Func<OrderHeader, bool>>(body, parameter);

        var orderHeaders = _unitOfWork.OrderHeader.GetList(filters, "ApplicationUser");

        return View(orderHeaders);
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var model = new OrderVM
        {
            OrderHeader = _unitOfWork.OrderHeader.Get(o => o.Id == id, "ApplicationUser"),
            OrderDetails = _unitOfWork.OrderDetail.GetList(o => o.OrderHeaderId == id, "Product")
        };

        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public IActionResult UpdateOrderDetails(OrderVM model)
    {
        var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(o => o.Id == model.OrderHeader.Id);

        orderHeaderFromDb.Name = model.OrderHeader.Name;
        orderHeaderFromDb.PhoneNumber = model.OrderHeader.PhoneNumber;
        orderHeaderFromDb.StreetAddress = model.OrderHeader.StreetAddress;
        orderHeaderFromDb.City = model.OrderHeader.City;
        orderHeaderFromDb.State = model.OrderHeader.State;
        orderHeaderFromDb.PostalCode = model.OrderHeader.PostalCode;

        if (!string.IsNullOrEmpty(model.OrderHeader.Carrier))
        {
            orderHeaderFromDb.Carrier = model.OrderHeader.Carrier;
        }

        if (!string.IsNullOrEmpty(model.OrderHeader.TrackingNumber))
        {
            orderHeaderFromDb.TrackingNumber = model.OrderHeader.TrackingNumber;
        }

        _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
        _unitOfWork.Save();

        TempData["success"] = "Order details updated successfully";

        return RedirectToAction(nameof(Details), new { model.OrderHeader.Id });
    }

    [HttpPost]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public IActionResult StartProcessing(OrderVM model)
    {
        _unitOfWork.OrderHeader.UpdateStatus(model.OrderHeader.Id, SD.StatusInProcess);
        _unitOfWork.Save();

        TempData["success"] = "Order details updated successfully";

        return RedirectToAction(nameof(Details), new { model.OrderHeader.Id });
    }

    [HttpPost]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public IActionResult ShipOrder(OrderVM model)
    {
        var orderHeader = _unitOfWork.OrderHeader.Get(o => o.Id == model.OrderHeader.Id);
        orderHeader.TrackingNumber = model.OrderHeader.TrackingNumber;
        orderHeader.Carrier = model.OrderHeader.Carrier;
        orderHeader.OrderStatus = SD.StatusShipped;
        orderHeader.ShippingDate = DateTime.Now;

        if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
        {
            orderHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
        }

        _unitOfWork.OrderHeader.Update(orderHeader);

        _unitOfWork.Save();

        TempData["success"] = "Order shipped successfully";

        return RedirectToAction(nameof(Details), new { model.OrderHeader.Id });
    }

    [HttpPost]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public IActionResult CancelOrder(OrderVM model)
    {
        var orderHeader = _unitOfWork.OrderHeader.Get(o => o.Id == model.OrderHeader.Id);
        if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
        {
            var options = new RefundCreateOptions
            {
                Reason = RefundReasons.RequestedByCustomer,
                PaymentIntent = orderHeader.PaymentIntentId
            };

            var service = new RefundService();
            var refund = service.Create(options);

            _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
        }
        else
        {
            _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
        }

        _unitOfWork.Save();

        TempData["success"] = "Order cancelled successfully";

        return RedirectToAction(nameof(Details), new { model.OrderHeader.Id });
    }

    [HttpPost]
    public IActionResult PayDelayedOrderPayment(OrderVM model)
    {
        var orderHeader = _unitOfWork.OrderHeader.Get(o => o.Id == model.OrderHeader.Id);
        var orderDetails = _unitOfWork.OrderDetail.GetList(o => o.OrderHeaderId == model.OrderHeader.Id, "Product");

        var options = new SessionCreateOptions
        {
            SuccessUrl = Url.Action("PaymentConfirmation", "Order", new { area = "admin", id = orderHeader.Id }, Request.Scheme),
            CancelUrl = Url.Action("Details", "Order", new { area = "admin", id = orderHeader.Id }, Request.Scheme),
            LineItems = new List<SessionLineItemOptions>(),
            Mode = "payment",
        };

        foreach (var item in orderDetails)
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
        _unitOfWork.OrderHeader.UpdateStripePaymentId(orderHeader.Id, session.Id, session.PaymentIntentId);
        _unitOfWork.Save();

        Response.Headers.Append("Location", session.Url);
        return new StatusCodeResult(303);
    }

    public IActionResult PaymentConfirmation(int id)
    {
        var orderHeader = _unitOfWork.OrderHeader.Get(o => o.Id == id);

        if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
        {
            var service = new SessionService();
            var session = service.Get(orderHeader.SessionId);

            if (session.PaymentStatus.Equals("paid", StringComparison.CurrentCultureIgnoreCase))
            {
                _unitOfWork.OrderHeader.UpdateStripePaymentId(orderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                _unitOfWork.Save();
            }
        }

        return View(id);
    }
}

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.ComponentModel.DataAnnotations;

namespace Bulky.Models;

public class OrderHeader
{
    public int Id { get; set; }

    public DateTime OrderDate { get; set; }

    public DateTime ShippingDate { get; set; }

    public double OrderTotal { get; set; }

    [MaxLength(4000)]
    public string OrderStatus { get; set; }

    [MaxLength(4000)]
    public string PaymentStatus { get; set; }

    [MaxLength(4000)]
    public string TrackingNumber { get; set; }

    [MaxLength(4000)]
    public string Carrier { get; set; }

    public DateTime PaymentDate { get; set; }

    public DateOnly PaymentDueDate { get; set; }

    [MaxLength(4000)]
    public string SessionId { get; set; }

    [MaxLength(4000)]
    public string PaymentIntentId { get; set; }

    [Required, MaxLength(4000)]
    public string PhoneNumber { get; set; }

    [Required, MaxLength(4000)]
    public string StreetAddress { get; set; }

    [Required, MaxLength(4000)]
    public string City { get; set; }

    [Required, MaxLength(4000)]
    public string State { get; set; }

    [Required, MaxLength(4000)]
    public string PostalCode { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; }

    [Required]
    public string ApplicationUserId { get; set; }

    [ValidateNever]
    public ApplicationUser ApplicationUser { get; set; }
}

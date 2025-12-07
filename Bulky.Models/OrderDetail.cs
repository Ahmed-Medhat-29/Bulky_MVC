using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Bulky.Models;

public class OrderDetail
{
    public int Id { get; set; }

    public int OrderHeaderId { get; set; }

    public int ProductId { get; set; }

    public int Count { get; set; }

    public double Price { get; set; }

    [ValidateNever]
    public Product Product { get; set; }

    [ValidateNever]
    public OrderHeader OrderHeader { get; set; }
}

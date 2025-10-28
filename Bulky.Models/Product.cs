using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bulky.Models;

public class Product
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(4000)]
    public string Title { get; set; }

    [MaxLength(4000)]
    public string Description { get; set; }

    [Required, MaxLength(4000)]
    public string ISBN { get; set; }

    [Required, MaxLength(4000)]
    public string Author { get; set; }

    [Range(1, 1000)]
    [DisplayName("List Price")]
    public double ListPrice { get; set; }

    [Range(1, 1000)]
    [DisplayName("Price for 1-50")]
    public double Price { get; set; }

    [Range(1, 1000)]
    [DisplayName("Price for 50+")]
    public double Price50 { get; set; }

    [Range(1, 1000)]
    [DisplayName("Price for 100+")]
    public double Price100 { get; set; }

    public int CategoryId { get; set; }

    [ValidateNever]
    public Category Category { get; set; }

    public string ImageUrl { get; set; }
}

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

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
	public string Auther { get; set; }

	[Range(1, 1000)]
	[DisplayName("Price for 1-50")]
	public double Price { get; set; }

	[Range(1, 1000)]
	[DisplayName("Price for 50+")]
	public double Price50 { get; set; }

	[Range(1, 1000)]
	[DisplayName("Price for 100+")]
	public double Price100 { get; set; }
}

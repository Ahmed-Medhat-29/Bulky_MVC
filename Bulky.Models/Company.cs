using System.ComponentModel.DataAnnotations;

namespace Bulky.Models;

public class Company
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(4000)]
    public string Name { get; set; }

    [MaxLength(4000)]
    public string StreetAddress { get; set; }

    [MaxLength(4000)]
    public string City { get; set; }

    [MaxLength(4000)]
    public string State { get; set; }

    [MaxLength(50)]
    public string PostalCode { get; set; }

    [MaxLength(4000)]
    public string PhoneNumber { get; set; }
}

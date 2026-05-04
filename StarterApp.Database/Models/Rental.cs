using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentalApp.Database.Models;

[Table("rental")]
[PrimaryKey(nameof(Id))]
public class Rental
{
    public int Id { get; set; }
    
    [Required]
    public int ItemId { get; set; }
    
    [Required]
    public int RenterId { get; set; }
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal TotalPrice { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Pending";
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedDate { get; set; }

    // Properties from API response (for display purposes when navigation properties not loaded)
    [NotMapped]
    public string? ItemTitle { get; set; }

    [NotMapped]
    public string? RenterName { get; set; }

    [NotMapped]
    public int? OwnerId { get; set; }

    [NotMapped]
    public string? OwnerName { get; set; }

    // Navigation properties
    [ForeignKey(nameof(ItemId))]
    public Item Item { get; set; } = null!;

    [ForeignKey(nameof(RenterId))]
    public User Renter { get; set; } = null!;

    public List<Rating> Ratings { get; set; } = new List<Rating>();
}

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentalApp.Database.Models;

[Table("item")]
[PrimaryKey(nameof(Id))]
public class Item
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal DailyPrice { get; set; }
    
    public bool IsAvailable { get; set; } = true;
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedDate { get; set; }
    
    [Required]
    public int OwnerId { get; set; }
    
    [Required]
    public int CategoryId { get; set; }

    // Navigation properties
    [ForeignKey(nameof(OwnerId))]
    public User Owner { get; set; } = null!;
    
    [ForeignKey(nameof(CategoryId))]
    public Category Category { get; set; } = null!;
    
    public List<ItemImage> ItemImages { get; set; } = new List<ItemImage>();
    public List<Rental> Rentals { get; set; } = new List<Rental>();
}

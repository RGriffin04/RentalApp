using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentalApp.Database.Models;

[Table("rating")]
[PrimaryKey(nameof(Id))]
public class Rating
{
    public int Id { get; set; }
    
    [Required]
    public int RentalId { get; set; }
    
    [Required]
    public int RaterId { get; set; }
    
    [Required]
    public int RatedUserId { get; set; }
    
    [Required]
    [Range(1, 5)]
    public int Stars { get; set; }
    
    [MaxLength(500)]
    public string? Comment { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(RentalId))]
    public Rental Rental { get; set; } = null!;
    
    [ForeignKey(nameof(RaterId))]
    public User Rater { get; set; } = null!;
    
    [ForeignKey(nameof(RatedUserId))]
    public User RatedUser { get; set; } = null!;
}

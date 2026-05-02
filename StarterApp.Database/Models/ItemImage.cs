using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentalApp.Database.Models;

[Table("item_image")]
[PrimaryKey(nameof(Id))]
public class ItemImage
{
    public int Id { get; set; }
    
    [Required]
    public int ItemId { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;
    
    public bool IsPrimary { get; set; } = false;
    
    public DateTime UploadedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(ItemId))]
    public Item Item { get; set; } = null!;
}

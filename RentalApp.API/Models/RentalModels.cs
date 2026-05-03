namespace RentalApp.Api.Models;

// ============================================
// Item DTOs
// ============================================

public class CreateItemRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal DailyPrice { get; set; }
    public int CategoryId { get; set; }
    public bool IsAvailable { get; set; } = true;

    // Location fields (optional)
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Address { get; set; }
}

public class UpdateItemRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal? DailyPrice { get; set; }
    public int? CategoryId { get; set; }
    public bool? IsAvailable { get; set; }

    // Location fields (optional)
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Address { get; set; }
}

public class ItemResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal DailyPrice { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    // Location fields
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Address { get; set; }

    // Rating information (optional)
    public double? AverageRating { get; set; }
    public int? RatingCount { get; set; }
}

public class ItemWithDistanceResponse : ItemResponse
{
    /// <summary>
    /// Distance in kilometers from search point
    /// </summary>
    public double DistanceKm { get; set; }
}

// ============================================
// Category DTOs
// ============================================

public class CategoryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

// ============================================
// Rental DTOs
// ============================================

public class CreateRentalRequest
{
    public int ItemId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalPrice { get; set; }
}

public class UpdateRentalStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public class RentalResponse
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public string ItemTitle { get; set; } = string.Empty;
    public int RenterId { get; set; }
    public string RenterName { get; set; } = string.Empty;
    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
}

// ============================================
// Rating DTOs
// ============================================

public class CreateRatingRequest
{
    public int RentalId { get; set; }
    public int RatedUserId { get; set; }
    public int Stars { get; set; } // 1-5
    public string? Comment { get; set; }
}

public class RatingResponse
{
    public int Id { get; set; }
    public int RentalId { get; set; }
    public int RaterId { get; set; }
    public string RaterName { get; set; } = string.Empty;
    public int RatedUserId { get; set; }
    public string RatedUserName { get; set; } = string.Empty;
    public int Stars { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class RatingStatsResponse
{
    public double AverageRating { get; set; }
    public int TotalCount { get; set; }
}


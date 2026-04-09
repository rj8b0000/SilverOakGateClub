using System.ComponentModel.DataAnnotations;

namespace SilverOakGateClub.Models;

public class Lecture
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required, MaxLength(500)]
    public string VideoUrl { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ThumbnailUrl { get; set; }

    public int? BranchId { get; set; }
    public Branch? Branch { get; set; }

    [MaxLength(100)]
    public string? Subject { get; set; }

    public int DurationMinutes { get; set; }

    public int OrderIndex { get; set; }

    public bool IsActive { get; set; } = true;

    public int ViewCount { get; set; } = 0;

    public int? CreatedByUserId { get; set; }
    public User? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

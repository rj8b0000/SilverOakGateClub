using System.ComponentModel.DataAnnotations;

namespace SilverOakGateClub.Models;

public class Announcement
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent

    public bool IsActive { get; set; } = true;

    public int? BranchId { get; set; } // null = all branches
    public Branch? Branch { get; set; }

    public int CreatedByUserId { get; set; }
    public User CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
}

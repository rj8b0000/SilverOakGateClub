using System.ComponentModel.DataAnnotations;

namespace SilverOakGateClub.Models;

public class Notes
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required, MaxLength(500)]
    public string FileUrl { get; set; } = string.Empty;

    [MaxLength(50)]
    public string FileType { get; set; } = "PDF"; // PDF, DOCX, etc.

    public long FileSizeBytes { get; set; }

    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;

    [MaxLength(100)]
    public string? Subject { get; set; }

    public int UploadedByUserId { get; set; }
    public User UploadedBy { get; set; } = null!;

    public double AverageRating { get; set; } = 0;
    public int RatingCount { get; set; } = 0;

    public int DownloadCount { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

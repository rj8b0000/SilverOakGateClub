using System.ComponentModel.DataAnnotations;

namespace SilverOakGateClub.Models;

public class Branch
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<MockTest> MockTests { get; set; } = new List<MockTest>();
    public ICollection<Lecture> Lectures { get; set; } = new List<Lecture>();
    public ICollection<Notes> Notes { get; set; } = new List<Notes>();
}

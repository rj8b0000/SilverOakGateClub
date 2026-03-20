using System.ComponentModel.DataAnnotations;

namespace SilverOakGateClub.Models;

public class Question
{
    [Key]
    public int Id { get; set; }

    public int MockTestId { get; set; }
    public MockTest MockTest { get; set; } = null!;

    [Required]
    public string QuestionText { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    [Required, MaxLength(500)]
    public string OptionA { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string OptionB { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string OptionC { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string OptionD { get; set; } = string.Empty;

    [Required, MaxLength(1)]
    public string CorrectAnswer { get; set; } = string.Empty; // A, B, C, D

    public int Marks { get; set; } = 1;

    public int NegativeMarks { get; set; } = 0;

    [MaxLength(2000)]
    public string? Explanation { get; set; }

    public int OrderIndex { get; set; }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace petlife.Models;

public class HealthRecord
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public TimeSpan Time { get; set; }

    [Required]
    [MaxLength(200)]
    public string HealthIssue { get; set; } = string.Empty;

    public string? SymptomDescription { get; set; }

    [Range(1, 5)]
    public byte Severity { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

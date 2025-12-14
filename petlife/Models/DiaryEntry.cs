using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace petlife.Models;

public class DiaryEntry
{
 public int Id { get; set; }

 [Required]
 public int PetId { get; set; }

 [Required]
 public DateTime Date { get; set; }

 [Required, MaxLength(100)]
 public string Title { get; set; } = string.Empty;

 [MaxLength(50)]
 public string? Mood { get; set; }

 public string? Content { get; set; }

 // Deprecated: use Photos collection in DiaryPhotos instead
 [NotMapped]
 public string? PhotoUrl { get; set; }

 [MaxLength(200)]
 public string? Tags { get; set; }

 public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

 public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

 // Navigation-like collection for multiple photos
 public List<DiaryPhoto> Photos { get; set; } = new();
}

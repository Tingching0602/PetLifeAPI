using System;
using System.ComponentModel.DataAnnotations;

namespace petlife.Models;

public class DiaryPhoto
{
 public int Id { get; set; }
 public int DiaryEntryId { get; set; }
 [Required]
 [MaxLength(300)]
 public string PhotoUrl { get; set; } = string.Empty;
 public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

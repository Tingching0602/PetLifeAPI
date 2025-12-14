using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace petlife.Models;

public class Pet
{
 [Key]
 public int Id { get; set; }

 [Required]
 [MaxLength(200)]
 public string Name { get; set; } = string.Empty;

 [MaxLength(100)]
 public string Species { get; set; } = string.Empty; // Cat, Dog...

 [MaxLength(200)]
 public string? Breed { get; set; }

 [MaxLength(20)]
 public string? Gender { get; set; } // Male/Female

 public DateTime? BirthDate { get; set; }
 public DateTime? AdoptDate { get; set; }

 [Column(TypeName = "decimal(10,2)")]
 public decimal? WeightKg { get; set; }

 public bool Neutered { get; set; }

 [MaxLength(100)]
 public string? MicrochipNumber { get; set; }

 [MaxLength(100)]
 public string? Color { get; set; }

 [MaxLength(500)]
 public string? AvatarUrl { get; set; }

 public string? Notes { get; set; }

 public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
 public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

 // Navigation
 public ICollection<MedicalRecord>? MedicalRecords { get; set; }
 public ICollection<FeedingRecord>? FeedingRecords { get; set; }
 public ICollection<PetStatusRecord>? PetStatusRecords { get; set; }
}

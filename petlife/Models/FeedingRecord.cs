using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace petlife.Models;

public class FeedingRecord
{
 [Key]
 public int Id { get; set; }

 [ForeignKey(nameof(Pet))]
 public int PetId { get; set; }
 public Pet? Pet { get; set; }

 public DateTime RecordTime { get; set; }

 [Required]
 [MaxLength(200)]
 public string FoodName { get; set; } = string.Empty;

 [MaxLength(200)]
 public string? Brand { get; set; }

 [Column(TypeName = "decimal(10,2)")]
 public decimal? AmountGrams { get; set; }

 [MaxLength(50)]
 public string? MealType { get; set; } // Breakfast, Dinner, Snack

 [Column(TypeName = "decimal(10,2)")]
 public decimal? WaterMl { get; set; }

 public string? Notes { get; set; }
 public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace petlife.Models;

public class InventoryItem
{
 public int Id { get; set; }

 [MaxLength(50)]
 public string? ItemType { get; set; }

 [Required]
 [MaxLength(100)]
 public string Name { get; set; } = string.Empty;

 [Column(TypeName = "decimal(18,2)")]
 public decimal Quantity { get; set; }

 [MaxLength(50)]
 public string? Unit { get; set; }

 [Column(TypeName = "decimal(18,2)")]
 public decimal TotalPrice { get; set; }

 public DateTime PurchaseDate { get; set; }

 public DateTime? ExpirationDate { get; set; }

 public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

 public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

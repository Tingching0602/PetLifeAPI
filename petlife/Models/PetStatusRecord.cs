using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace petlife.Models;

public class PetStatusRecord
{
 [Key]
 public int Id { get; set; }

 [ForeignKey(nameof(Pet))]
 public int PetId { get; set; }
 public Pet? Pet { get; set; }

 public DateTime RecordTime { get; set; }

 public byte EnergyLevel { get; set; } //1-5
 public byte Mood { get; set; } //1-5
 public byte Appetite { get; set; } //1-5

 public string? StoolStatus { get; set; }
 public string? UrineStatus { get; set; }
 public bool Vomiting { get; set; }
 public bool Coughing { get; set; }
 public byte? ItchinessLevel { get; set; }
 public bool MedicationGiven { get; set; }
 public string? Notes { get; set; }

 public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

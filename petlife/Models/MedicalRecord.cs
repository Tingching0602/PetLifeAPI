using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace petlife.Models;

public class MedicalRecord
{
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(Pet))]
    public int PetId { get; set; }
    public Pet? Pet { get; set; }

    public DateTime VisitDate { get; set; }

    [Required]
    [MaxLength(200)]
    public string ClinicName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? VetName { get; set; }

    public string? Reason { get; set; }
    public string? Diagnosis { get; set; }
    public string? Treatment { get; set; }
    public string? Medications { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? WeightKg { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? TemperatureC { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? Cost { get; set; }

    public DateTime? NextVisitDate { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

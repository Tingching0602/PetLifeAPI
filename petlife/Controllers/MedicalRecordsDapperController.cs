using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using petlife.Models;

namespace petlife.Controllers;

[ApiController]
[Route("api/MedicalRecords")] // Fix explicit route to keep /api/MedicalRecords regardless of controller class name
public class MedicalRecordsDapperController : ControllerBase // Ensure correct class name and inheritance
{
 private readonly IDbConnection _db;
 public MedicalRecordsDapperController(IDbConnection db) => _db = db;

 [HttpGet]
 public async Task<ActionResult<IEnumerable<MedicalRecord>>> GetAll()
 {
 var sql = "SELECT * FROM MedicalRecords";
 var result = await _db.QueryAsync<MedicalRecord>(sql);
 return Ok(result);
 }

 [HttpGet("{id:int}")]
 public async Task<ActionResult<MedicalRecord?>> GetById(int id)
 {
 var sql = "SELECT * FROM MedicalRecords WHERE Id = @Id";
 var entity = await _db.QuerySingleOrDefaultAsync<MedicalRecord>(sql, new { Id = id });
 return entity is null ? NotFound() : Ok(entity);
 }

 [HttpPost]
 public async Task<ActionResult<int>> Create(MedicalRecord entity)
 {
 entity.CreatedAt = DateTime.UtcNow;
 var sql = @"INSERT INTO MedicalRecords(PetId, VisitDate, ClinicName, VetName, Reason, Diagnosis, Treatment, Medications, WeightKg, TemperatureC, Cost, NextVisitDate, Notes, CreatedAt)
 VALUES(@PetId, @VisitDate, @ClinicName, @VetName, @Reason, @Diagnosis, @Treatment, @Medications, @WeightKg, @TemperatureC, @Cost, @NextVisitDate, @Notes, @CreatedAt);
 SELECT CAST(SCOPE_IDENTITY() as int);";
 var newId = await _db.ExecuteScalarAsync<int>(sql, entity);
 return CreatedAtAction(nameof(GetById), new { id = newId }, newId);
 }

 [HttpPut("{id:int}")]
 public async Task<IActionResult> Update(int id, MedicalRecord entity)
 {
 if (id != entity.Id) return BadRequest();
 var sql = @"UPDATE MedicalRecords SET PetId=@PetId, VisitDate=@VisitDate, ClinicName=@ClinicName, VetName=@VetName, Reason=@Reason, Diagnosis=@Diagnosis, Treatment=@Treatment, Medications=@Medications, WeightKg=@WeightKg, TemperatureC=@TemperatureC, Cost=@Cost, NextVisitDate=@NextVisitDate, Notes=@Notes WHERE Id=@Id";
 var rows = await _db.ExecuteAsync(sql, entity);
 return rows ==0 ? NotFound() : NoContent();
 }

 [HttpDelete("{id:int}")]
 public async Task<IActionResult> Delete(int id)
 {
 var sql = "DELETE FROM MedicalRecords WHERE Id=@Id";
 var rows = await _db.ExecuteAsync(sql, new { Id = id });
 return rows ==0 ? NotFound() : NoContent();
 }

 // GET /api/MedicalRecords/expense-summary
 [HttpGet("expense-summary")] // Ensure correct route attribute
 public async Task<IActionResult> GetExpenseSummary([FromQuery] string? period = null)
 {
 DateTime? from = period switch
 {
 "one-week" => DateTime.UtcNow.Date.AddDays(-7),
 "one-month" => DateTime.UtcNow.Date.AddMonths(-1),
 "three-months" => DateTime.UtcNow.Date.AddMonths(-3),
 "six-months" => DateTime.UtcNow.Date.AddMonths(-6),
 "one-year" => DateTime.UtcNow.Date.AddYears(-1),
 _ => null
 };
 string where = from is null ? string.Empty : " WHERE VisitDate >= @From";

 var rows = await _db.QueryAsync($"SELECT Id, PetId, VisitDate, Reason, Diagnosis, Treatment, TotalCost, Cost, Amount, Price, Fee FROM MedicalRecords{where}", new { From = from });

 string GetCategory(dynamic r)
 {
 string text = ($"{r.Reason} {r.Diagnosis} {r.Treatment}").ToLowerInvariant();
 if (text.Contains("vacc") || text.Contains("vaccine") || text.Contains("vaccination")) return "Vaccination";
 if (text.Contains("surgery") || text.Contains("operation")) return "Surgery";
 if (text.Contains("check") || text.Contains("exam") || text.Contains("inspection") || text.Contains("routine")) return "Checkup";
 if (text.Contains("medication") || text.Contains("drug") || text.Contains("medicine")) return "Medication";
 if (text.Contains("dental") || text.Contains("teeth")) return "Dental";
 if (text.Contains("groom")) return "Grooming";
 return "Uncategorized";
 }
 decimal GetAmount(dynamic r)
 {
 if (HasColumn(r, "TotalCost") && r.TotalCost != null) return (decimal)r.TotalCost;
 if (HasColumn(r, "Cost") && r.Cost != null) return (decimal)r.Cost;
 if (HasColumn(r, "Amount") && r.Amount != null) return (decimal)r.Amount;
 if (HasColumn(r, "Price") && r.Price != null) return (decimal)r.Price;
 if (HasColumn(r, "Fee") && r.Fee != null) return (decimal)r.Fee;
 return 0m;
 }

 var result = rows
 .Select(r => new
 {
 visitDate = (DateTime)(r.VisitDate ?? DateTime.MinValue),
 category = GetCategory(r),
 amount = GetAmount(r)
 })
 .GroupBy(x => new { Year = x.visitDate.Year, Month = x.visitDate.Month, x.category })
 .Select(g => new ExpenseSummaryByCategoryDto
 {
 Year = g.Key.Year,
 Month = g.Key.Month,
 Category = g.Key.category,
 Total = g.Sum(x => x.amount)
 })
 .OrderBy(x => x.Year).ThenBy(x => x.Month).ThenBy(x => x.Category)
 .ToList();

 return Ok(result);
 }

 // GET /api/MedicalRecords/expense-by-pet
 [HttpGet("expense-by-pet")] // Ensure correct route attribute
 public async Task<IActionResult> GetExpenseByPet(
 [FromQuery] string? period = null // one-week, one-month, three-months, six-months, one-year
 )
 {
 DateTime? from = period switch
 {
 "one-week" => DateTime.UtcNow.Date.AddDays(-7),
 "one-month" => DateTime.UtcNow.Date.AddMonths(-1),
 "three-months" => DateTime.UtcNow.Date.AddMonths(-3),
 "six-months" => DateTime.UtcNow.Date.AddMonths(-6),
 "one-year" => DateTime.UtcNow.Date.AddYears(-1),
 _ => null
 };

 string where = from is null ? string.Empty : " WHERE VisitDate >= @From";

 var records = await _db.QueryAsync($"SELECT Id, PetId, VisitDate, Reason, Diagnosis, Treatment, TotalCost, Cost, Amount, Price, Fee FROM MedicalRecords{where}", new { From = from });
 var pets = await _db.QueryAsync<Pet>("SELECT Id, Name FROM Pets");
 var petNames = pets.ToDictionary(p => p.Id, p => p.Name);

 string GetCategory(dynamic r)
 {
 string text = ($"{r.Reason} {r.Diagnosis} {r.Treatment}").ToLowerInvariant();
 if (text.Contains("vacc") || text.Contains("vaccine") || text.Contains("vaccination")) return "Vaccination";
 if (text.Contains("surgery") || text.Contains("operation")) return "Surgery";
 if (text.Contains("check") || text.Contains("exam") || text.Contains("inspection") || text.Contains("routine")) return "Checkup";
 if (text.Contains("medication") || text.Contains("drug") || text.Contains("medicine")) return "Medication";
 if (text.Contains("dental") || text.Contains("teeth")) return "Dental";
 if (text.Contains("groom")) return "Grooming";
 return "Uncategorized";
 }

 decimal GetAmount(dynamic r)
 {
 if (HasColumn(r, "TotalCost") && r.TotalCost != null) return (decimal)r.TotalCost;
 if (HasColumn(r, "Cost") && r.Cost != null) return (decimal)r.Cost;
 if (HasColumn(r, "Amount") && r.Amount != null) return (decimal)r.Amount;
 if (HasColumn(r, "Price") && r.Price != null) return (decimal)r.Price;
 if (HasColumn(r, "Fee") && r.Fee != null) return (decimal)r.Fee;
 return 0m;
 }

 var result = records
 .Select(r => new
 {
 petId = (int)r.PetId,
 petName = petNames.TryGetValue((int)r.PetId, out var name) ? name : string.Empty,
 category = GetCategory(r),
 amount = GetAmount(r)
 })
 .GroupBy(x => new { x.petId, x.petName, x.category })
 .Select(g => new ExpenseByPetCategoryDto
 {
 PetId = g.Key.petId,
 PetName = g.Key.petName,
 Category = g.Key.category,
 Total = g.Sum(x => x.amount)
 })
 .OrderBy(x => x.PetId)
 .ThenBy(x => x.Category)
 .ToList();

 return Ok(result);
 }

 private static bool HasColumn(dynamic row, string name)
 {
 var dict = row as IDictionary<string, object>;
 return dict != null && dict.ContainsKey(name);
 }
}

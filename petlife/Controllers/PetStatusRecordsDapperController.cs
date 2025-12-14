using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using petlife.Models;

namespace petlife.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PetStatusRecordsController : ControllerBase
{
 private readonly IDbConnection _db;
 public PetStatusRecordsController(IDbConnection db) => _db = db;

 [HttpGet]
 public async Task<ActionResult<IEnumerable<PetStatusRecord>>> GetAll()
 {
 var sql = "SELECT * FROM PetStatusRecords";
 var result = await _db.QueryAsync<PetStatusRecord>(sql);
 return Ok(result);
 }

 [HttpGet("{id:int}")]
 public async Task<ActionResult<PetStatusRecord?>> GetById(int id)
 {
 var sql = "SELECT * FROM PetStatusRecords WHERE Id = @Id";
 var entity = await _db.QuerySingleOrDefaultAsync<PetStatusRecord>(sql, new { Id = id });
 return entity is null ? NotFound() : Ok(entity);
 }

 [HttpPost]
 public async Task<ActionResult<int>> Create(PetStatusRecord entity)
 {
 entity.CreatedAt = DateTime.UtcNow;
 var sql = @"INSERT INTO PetStatusRecords(PetId, RecordTime, EnergyLevel, Mood, Appetite, StoolStatus, UrineStatus, Vomiting, Coughing, ItchinessLevel, MedicationGiven, Notes, CreatedAt)
 VALUES(@PetId, @RecordTime, @EnergyLevel, @Mood, @Appetite, @StoolStatus, @UrineStatus, @Vomiting, @Coughing, @ItchinessLevel, @MedicationGiven, @Notes, @CreatedAt);
 SELECT CAST(SCOPE_IDENTITY() as int);";
 var newId = await _db.ExecuteScalarAsync<int>(sql, entity);
 return CreatedAtAction(nameof(GetById), new { id = newId }, newId);
 }

 [HttpPut("{id:int}")]
 public async Task<IActionResult> Update(int id, PetStatusRecord entity)
 {
 if (id != entity.Id) return BadRequest();
 var sql = @"UPDATE PetStatusRecords SET PetId=@PetId, RecordTime=@RecordTime, EnergyLevel=@EnergyLevel, Mood=@Mood, Appetite=@Appetite, StoolStatus=@StoolStatus, UrineStatus=@UrineStatus, Vomiting=@Vomiting, Coughing=@Coughing, ItchinessLevel=@ItchinessLevel, MedicationGiven=@MedicationGiven, Notes=@Notes WHERE Id=@Id";
 var rows = await _db.ExecuteAsync(sql, entity);
 return rows ==0 ? NotFound() : NoContent();
 }

 [HttpDelete("{id:int}")]
 public async Task<IActionResult> Delete(int id)
 {
 var sql = "DELETE FROM PetStatusRecords WHERE Id=@Id";
 var rows = await _db.ExecuteAsync(sql, new { Id = id });
 return rows ==0 ? NotFound() : NoContent();
 }
}

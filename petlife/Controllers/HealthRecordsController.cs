using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using petlife.Models;

namespace petlife.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthRecordsController : ControllerBase
{
 private readonly IDbConnection _db;
 public HealthRecordsController(IDbConnection db) => _db = db;

 [HttpGet]
 public async Task<ActionResult<IEnumerable<HealthRecord>>> GetAll()
 {
 var sql = "SELECT * FROM HealthRecords";
 var result = await _db.QueryAsync<HealthRecord>(sql);
 return Ok(result);
 }

 [HttpGet("{id:int}")]
 public async Task<ActionResult<HealthRecord?>> GetById(int id)
 {
 var sql = "SELECT * FROM HealthRecords WHERE Id=@Id";
 var entity = await _db.QuerySingleOrDefaultAsync<HealthRecord>(sql, new { Id = id });
 return entity is null ? NotFound() : Ok(entity);
 }

 [HttpPost]
 public async Task<ActionResult<int>> Create([FromBody] HealthRecord entity)
 {
 if (!ModelState.IsValid) return BadRequest(ModelState);
 var sql = @"INSERT INTO HealthRecords(Date, Time, HealthIssue, SymptomDescription, Severity, CreatedAt)
 VALUES(@Date, @Time, @HealthIssue, @SymptomDescription, @Severity, @CreatedAt);
 SELECT CAST(SCOPE_IDENTITY() as int);";
 var newId = await _db.ExecuteScalarAsync<int>(sql, entity);
 return CreatedAtAction(nameof(GetById), new { id = newId }, newId);
 }

 [HttpPut("{id:int}")]
 public async Task<IActionResult> Update(int id, [FromBody] HealthRecord entity)
 {
 if (id != entity.Id) return BadRequest();
 if (!ModelState.IsValid) return BadRequest(ModelState);
 var sql = @"UPDATE HealthRecords SET Date=@Date, Time=@Time, HealthIssue=@HealthIssue, SymptomDescription=@SymptomDescription, Severity=@Severity WHERE Id=@Id";
 var rows = await _db.ExecuteAsync(sql, entity);
 return rows ==0 ? NotFound() : NoContent();
 }

 [HttpDelete("{id:int}")]
 public async Task<IActionResult> Delete(int id)
 {
 var sql = "DELETE FROM HealthRecords WHERE Id=@Id";
 var rows = await _db.ExecuteAsync(sql, new { Id = id });
 return rows ==0 ? NotFound() : NoContent();
 }
}

using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using petlife.Models;

namespace petlife.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeedingRecordsController : ControllerBase
{
 private readonly IDbConnection _db;
 public FeedingRecordsController(IDbConnection db) => _db = db;

 [HttpGet]
 public async Task<ActionResult<IEnumerable<FeedingRecord>>> GetAll()
 {
 var sql = "SELECT * FROM FeedingRecords";
 var result = await _db.QueryAsync<FeedingRecord>(sql);
 return Ok(result);
 }

 [HttpGet("{id:int}")]
 public async Task<ActionResult<FeedingRecord?>> GetById(int id)
 {
 var sql = "SELECT * FROM FeedingRecords WHERE Id = @Id";
 var entity = await _db.QuerySingleOrDefaultAsync<FeedingRecord>(sql, new { Id = id });
 return entity is null ? NotFound() : Ok(entity);
 }

 [HttpPost]
 public async Task<ActionResult<int>> Create(FeedingRecord entity)
 {
 entity.CreatedAt = DateTime.UtcNow;
 var sql = @"INSERT INTO FeedingRecords(PetId, RecordTime, FoodName, Brand, AmountGrams, MealType, WaterMl, Notes, CreatedAt)
 VALUES(@PetId, @RecordTime, @FoodName, @Brand, @AmountGrams, @MealType, @WaterMl, @Notes, @CreatedAt);
 SELECT CAST(SCOPE_IDENTITY() as int);";
 var newId = await _db.ExecuteScalarAsync<int>(sql, entity);
 return CreatedAtAction(nameof(GetById), new { id = newId }, newId);
 }

 [HttpPut("{id:int}")]
 public async Task<IActionResult> Update(int id, FeedingRecord entity)
 {
 if (id != entity.Id) return BadRequest();
 var sql = @"UPDATE FeedingRecords SET PetId=@PetId, RecordTime=@RecordTime, FoodName=@FoodName, Brand=@Brand, AmountGrams=@AmountGrams, MealType=@MealType, WaterMl=@WaterMl, Notes=@Notes WHERE Id=@Id";
 var rows = await _db.ExecuteAsync(sql, entity);
 return rows ==0 ? NotFound() : NoContent();
 }

 [HttpDelete("{id:int}")]
 public async Task<IActionResult> Delete(int id)
 {
 var sql = "DELETE FROM FeedingRecords WHERE Id=@Id";
 var rows = await _db.ExecuteAsync(sql, new { Id = id });
 return rows ==0 ? NotFound() : NoContent();
 }
}

using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using petlife.Models;

namespace petlife.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryItemsController : ControllerBase
{
 private readonly IDbConnection _db;
 public InventoryItemsController(IDbConnection db) => _db = db;

 [HttpGet]
 public async Task<ActionResult<IEnumerable<InventoryItem>>> GetAll()
 {
 var sql = "SELECT * FROM InventoryItems ORDER BY PurchaseDate DESC";
 var items = await _db.QueryAsync<InventoryItem>(sql);
 return Ok(items);
 }

 [HttpGet("{id:int}")]
 public async Task<ActionResult<InventoryItem>> GetById(int id)
 {
 var sql = "SELECT * FROM InventoryItems WHERE Id=@Id";
 var item = await _db.QuerySingleOrDefaultAsync<InventoryItem>(sql, new { Id = id });
 return item is null ? NotFound() : Ok(item);
 }

 [HttpPost]
 public async Task<ActionResult<int>> Create([FromBody] InventoryItem item)
 {
 if (!ModelState.IsValid) return BadRequest(ModelState);
 item.CreatedAt = DateTime.UtcNow;
 item.UpdatedAt = DateTime.UtcNow;
 var sql = @"INSERT INTO InventoryItems(ItemType, Name, Quantity, Unit, TotalPrice, PurchaseDate, ExpirationDate, CreatedAt, UpdatedAt)
 VALUES(@ItemType, @Name, @Quantity, @Unit, @TotalPrice, @PurchaseDate, @ExpirationDate, @CreatedAt, @UpdatedAt);
 SELECT CAST(SCOPE_IDENTITY() as int);";
 var newId = await _db.ExecuteScalarAsync<int>(sql, item);
 return CreatedAtAction(nameof(GetById), new { id = newId }, newId);
 }

 [HttpPut("{id:int}")]
 public async Task<IActionResult> Update(int id, [FromBody] InventoryItem update)
 {
 if (id != update.Id) return BadRequest();
 var exists = await _db.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM InventoryItems WHERE Id=@Id", new { Id = id });
 if (exists ==0) return NotFound();
 update.UpdatedAt = DateTime.UtcNow;
 var sql = @"UPDATE InventoryItems SET ItemType=@ItemType, Name=@Name, Quantity=@Quantity, Unit=@Unit, TotalPrice=@TotalPrice, PurchaseDate=@PurchaseDate, ExpirationDate=@ExpirationDate, UpdatedAt=@UpdatedAt WHERE Id=@Id";
 var rows = await _db.ExecuteAsync(sql, update);
 return rows ==0 ? NotFound() : NoContent();
 }

 [HttpDelete("{id:int}")]
 public async Task<IActionResult> Delete(int id)
 {
 var sql = "DELETE FROM InventoryItems WHERE Id=@Id";
 var rows = await _db.ExecuteAsync(sql, new { Id = id });
 return rows ==0 ? NotFound() : NoContent();
 }
}

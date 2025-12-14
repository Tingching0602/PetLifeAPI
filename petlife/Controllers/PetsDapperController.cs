using System.Data;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using petlife.Models;

namespace petlife.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PetsController : ControllerBase
{
 private readonly IDbConnection _db;
 public PetsController(IDbConnection db) => _db = db;

 [HttpGet]
 public async Task<ActionResult<IEnumerable<Pet>>> GetAll()
 {
 var sql = "SELECT * FROM Pets";
 var result = await _db.QueryAsync<Pet>(sql);
 return Ok(result);
 }

 [HttpGet("{id:int}")]
 public async Task<ActionResult<Pet?>> GetById(int id)
 {
 var sql = "SELECT * FROM Pets WHERE Id = @Id";
 var entity = await _db.QuerySingleOrDefaultAsync<Pet>(sql, new { Id = id });
 return entity is null ? NotFound() : Ok(entity);
 }

 [HttpPost]
 public async Task<ActionResult<int>> Create(Pet pet)
 {
 pet.CreatedAt = DateTime.UtcNow;
 pet.UpdatedAt = DateTime.UtcNow;
 var sql = @"INSERT INTO Pets(Name, Species, Breed, Gender, BirthDate, AdoptDate, WeightKg, Neutered, MicrochipNumber, Color, AvatarUrl, Notes, CreatedAt, UpdatedAt)
 VALUES(@Name, @Species, @Breed, @Gender, @BirthDate, @AdoptDate, @WeightKg, @Neutered, @MicrochipNumber, @Color, @AvatarUrl, @Notes, @CreatedAt, @UpdatedAt);
 SELECT CAST(SCOPE_IDENTITY() as int);";
 var newId = await _db.ExecuteScalarAsync<int>(sql, pet);
 return CreatedAtAction(nameof(GetById), new { id = newId }, newId);
 }

 [HttpPut("{id:int}")]
 public async Task<IActionResult> Update(int id, Pet pet)
 {
 if (id != pet.Id) return BadRequest();
 pet.UpdatedAt = DateTime.UtcNow;
 var sql = @"UPDATE Pets SET Name=@Name, Species=@Species, Breed=@Breed, Gender=@Gender, BirthDate=@BirthDate, AdoptDate=@AdoptDate, WeightKg=@WeightKg, Neutered=@Neutered, MicrochipNumber=@MicrochipNumber, Color=@Color, AvatarUrl=@AvatarUrl, Notes=@Notes, UpdatedAt=@UpdatedAt WHERE Id=@Id";
 var rows = await _db.ExecuteAsync(sql, pet);
 return rows ==0 ? NotFound() : NoContent();
 }

 [HttpDelete("{id:int}")]
 public async Task<IActionResult> Delete(int id)
 {
 var sql = "DELETE FROM Pets WHERE Id=@Id";
 var rows = await _db.ExecuteAsync(sql, new { Id = id });
 return rows ==0 ? NotFound() : NoContent();
 }
}

using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using petlife.Models;

namespace petlife.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiaryEntriesController : ControllerBase
{
 private readonly IDbConnection _db;
 public DiaryEntriesController(IDbConnection db) => _db = db;

 // DTOs
 public class DiaryEntryCreateDto
 {
 public int PetId { get; set; }
 public DateTime Date { get; set; }
 public string Title { get; set; } = string.Empty;
 public string? Mood { get; set; }
 public string? Content { get; set; }
 public string? Tags { get; set; }
 public List<string> PhotoUrls { get; set; } = new();
 }

 // GET /api/DiaryEntries
 [HttpGet]
 public async Task<ActionResult<IEnumerable<DiaryEntry>>> GetAll()
 {
 var sqlEntries = "SELECT * FROM DiaryEntries ORDER BY [Date] DESC, Id DESC";
 var entries = (await _db.QueryAsync<DiaryEntry>(sqlEntries)).ToList();
 if (entries.Count ==0) return Ok(entries);
 var ids = entries.Select(e => e.Id).ToArray();
 var sqlPhotos = "SELECT * FROM DiaryPhotos WHERE DiaryEntryId IN @Ids";
 var photos = await _db.QueryAsync<DiaryPhoto>(sqlPhotos, new { Ids = ids });
 var grouped = photos.GroupBy(p => p.DiaryEntryId).ToDictionary(g => g.Key, g => g.ToList());
 foreach (var e in entries)
 {
 if (grouped.TryGetValue(e.Id, out var list)) e.Photos = list;
 }
 return Ok(entries);
 }

 // GET /api/DiaryEntries/{id}
 [HttpGet("{id:int}")]
 public async Task<ActionResult<DiaryEntry>> GetById(int id)
 {
 var sql = "SELECT * FROM DiaryEntries WHERE Id=@Id";
 var entity = await _db.QuerySingleOrDefaultAsync<DiaryEntry>(sql, new { Id = id });
 if (entity is null) return NotFound();
 var photos = await _db.QueryAsync<DiaryPhoto>("SELECT * FROM DiaryPhotos WHERE DiaryEntryId=@Id", new { Id = id });
 entity.Photos = photos.ToList();
 return Ok(entity);
 }

 // POST /api/DiaryEntries
 [HttpPost]
 public async Task<ActionResult<DiaryEntry>> Create([FromBody] DiaryEntryCreateDto dto)
 {
 if (!ModelState.IsValid) return BadRequest(ModelState);
 var now = DateTime.UtcNow;
 var insertEntrySql = @"INSERT INTO DiaryEntries(PetId, [Date], Title, Mood, Content, Tags, CreatedAt, UpdatedAt)
 VALUES(@PetId, @Date, @Title, @Mood, @Content, @Tags, @CreatedAt, @UpdatedAt);
 SELECT CAST(SCOPE_IDENTITY() as int);";
 var newId = await _db.ExecuteScalarAsync<int>(insertEntrySql, new
 {
 PetId = dto.PetId,
 Date = dto.Date,
 Title = dto.Title,
 Mood = dto.Mood,
 Content = dto.Content,
 Tags = dto.Tags,
 CreatedAt = now,
 UpdatedAt = now
 });

 if (dto.PhotoUrls?.Count >0)
 {
 var insertPhotoSql = "INSERT INTO DiaryPhotos(DiaryEntryId, PhotoUrl, CreatedAt) VALUES(@DiaryEntryId, @PhotoUrl, @CreatedAt)";
 foreach (var url in dto.PhotoUrls)
 {
 await _db.ExecuteAsync(insertPhotoSql, new { DiaryEntryId = newId, PhotoUrl = url, CreatedAt = now });
 }
 }

 // return composed entity with photos
 var created = await _db.QuerySingleAsync<DiaryEntry>("SELECT * FROM DiaryEntries WHERE Id=@Id", new { Id = newId });
 var createdPhotos = await _db.QueryAsync<DiaryPhoto>("SELECT * FROM DiaryPhotos WHERE DiaryEntryId=@Id", new { Id = newId });
 created.Photos = createdPhotos.ToList();
 return CreatedAtAction(nameof(GetById), new { id = newId }, created);
 }

 // PUT /api/DiaryEntries/{id}
 [HttpPut("{id:int}")]
 public async Task<IActionResult> Update(int id, [FromBody] DiaryEntryCreateDto dto)
 {
 if (id <=0) return BadRequest();
 if (!ModelState.IsValid) return BadRequest(ModelState);
 var exists = await _db.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM DiaryEntries WHERE Id=@Id", new { Id = id });
 if (exists ==0) return NotFound();
 var now = DateTime.UtcNow;
 var updateSql = @"UPDATE DiaryEntries
 SET PetId=@PetId, [Date]=@Date, Title=@Title, Mood=@Mood, Content=@Content, Tags=@Tags, UpdatedAt=@UpdatedAt
 WHERE Id=@Id";
 var rows = await _db.ExecuteAsync(updateSql, new
 {
 Id = id,
 PetId = dto.PetId,
 Date = dto.Date,
 Title = dto.Title,
 Mood = dto.Mood,
 Content = dto.Content,
 Tags = dto.Tags,
 UpdatedAt = now
 });
 if (rows ==0) return NotFound();

 // Replace photos: delete existing then insert provided
 await _db.ExecuteAsync("DELETE FROM DiaryPhotos WHERE DiaryEntryId=@Id", new { Id = id });
 if (dto.PhotoUrls?.Count >0)
 {
 var insertPhotoSql = "INSERT INTO DiaryPhotos(DiaryEntryId, PhotoUrl, CreatedAt) VALUES(@DiaryEntryId, @PhotoUrl, @CreatedAt)";
 foreach (var url in dto.PhotoUrls)
 {
 await _db.ExecuteAsync(insertPhotoSql, new { DiaryEntryId = id, PhotoUrl = url, CreatedAt = now });
 }
 }
 return NoContent();
 }

 // DELETE /api/DiaryEntries/{id}
 [HttpDelete("{id:int}")]
 public async Task<IActionResult> Delete(int id)
 {
 var sql = "DELETE FROM DiaryEntries WHERE Id=@Id";
 var rows = await _db.ExecuteAsync(sql, new { Id = id });
 return rows ==0 ? NotFound() : NoContent();
 }

 // Optional photo management endpoints
 [HttpGet("{id:int}/photos")]
 public async Task<ActionResult<IEnumerable<DiaryPhoto>>> GetPhotos(int id)
 {
 var exists = await _db.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM DiaryEntries WHERE Id=@Id", new { Id = id });
 if (exists ==0) return NotFound();
 var photos = await _db.QueryAsync<DiaryPhoto>("SELECT * FROM DiaryPhotos WHERE DiaryEntryId=@Id ORDER BY Id", new { Id = id });
 return Ok(photos);
 }

 [HttpPost("{id:int}/photos")]
 public async Task<IActionResult> AddPhoto(int id, [FromBody] string photoUrl)
 {
 var exists = await _db.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM DiaryEntries WHERE Id=@Id", new { Id = id });
 if (exists ==0) return NotFound();
 var now = DateTime.UtcNow;
 var sql = "INSERT INTO DiaryPhotos(DiaryEntryId, PhotoUrl, CreatedAt) VALUES(@DiaryEntryId, @PhotoUrl, @CreatedAt)";
 await _db.ExecuteAsync(sql, new { DiaryEntryId = id, PhotoUrl = photoUrl, CreatedAt = now });
 return NoContent();
 }

 [HttpDelete("{id:int}/photos/{photoId:int}")]
 public async Task<IActionResult> DeletePhoto(int id, int photoId)
 {
 var sql = "DELETE FROM DiaryPhotos WHERE Id=@PhotoId AND DiaryEntryId=@DiaryEntryId";
 var rows = await _db.ExecuteAsync(sql, new { PhotoId = photoId, DiaryEntryId = id });
 return rows ==0 ? NotFound() : NoContent();
 }
}

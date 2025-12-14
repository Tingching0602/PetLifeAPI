using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace petlife.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
 private readonly IDbConnection _db;
 private readonly IWebHostEnvironment _env;
 public UploadController(IDbConnection db, IWebHostEnvironment env)
 {
 _db = db;
 _env = env;
 }

 [HttpPost("pet-avatar/{petId:int}")]
 public async Task<IActionResult> UploadPetAvatar(int petId, IFormFile file)
 {
 if (file == null || file.Length ==0)
 {
 return BadRequest("No file uploaded.");
 }

 var uploadsRoot = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "uploads", "pets");
 Directory.CreateDirectory(uploadsRoot);
 var ext = Path.GetExtension(file.FileName);
 var fileName = $"pet_{petId}_{Guid.NewGuid():N}{ext}";
 var physicalPath = Path.Combine(uploadsRoot, fileName);

 using (var stream = new FileStream(physicalPath, FileMode.Create))
 {
 await file.CopyToAsync(stream);
 }

 // Build URL path for client (assuming static files served from wwwroot)
 var relativePath = $"/uploads/pets/{fileName}";

 // Update database AvatarUrl
 var sql = "UPDATE Pets SET AvatarUrl=@AvatarUrl, UpdatedAt=SYSUTCDATETIME() WHERE Id=@Id";
 var rows = await _db.ExecuteAsync(sql, new { AvatarUrl = relativePath, Id = petId });
 if (rows ==0)
 {
 // delete file if pet not found
 System.IO.File.Delete(physicalPath);
 return NotFound("Pet not found.");
 }

 return Ok(new { url = relativePath });
 }

 // GET /api/Upload/image?path=/uploads/pets/filename.jpg
 [AllowAnonymous]
 [HttpGet("image")]
 public IActionResult GetImage([FromQuery] string path)
 {
 if (string.IsNullOrWhiteSpace(path)) return BadRequest("Path is required.");
 // Normalize and prevent path traversal
 if (!path.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
 {
 return BadRequest("Invalid path. Must start with /uploads/");
 }
 var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
 var fullPath = Path.GetFullPath(Path.Combine(webRoot, path.TrimStart('/')));
 var uploadsRoot = Path.GetFullPath(Path.Combine(webRoot, "uploads"));
 if (!fullPath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase))
 {
 return BadRequest("Invalid path.");
 }
 if (!System.IO.File.Exists(fullPath)) return NotFound();

 var contentType = GetContentType(fullPath);
 var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
 return File(stream, contentType);
 }

 private static string GetContentType(string filePath)
 {
 var ext = Path.GetExtension(filePath).ToLowerInvariant();
 return ext switch
 {
 ".jpg" or ".jpeg" => "image/jpeg",
 ".png" => "image/png",
 ".gif" => "image/gif",
 ".webp" => "image/webp",
 ".bmp" => "image/bmp",
 ".svg" => "image/svg+xml",
 _ => "application/octet-stream"
 };
 }
}

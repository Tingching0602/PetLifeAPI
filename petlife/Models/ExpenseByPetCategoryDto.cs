namespace petlife.Models;

public class ExpenseByPetCategoryDto
{
 public int PetId { get; set; }
 public string PetName { get; set; } = string.Empty;
 public string Category { get; set; } = string.Empty;
 public decimal Total { get; set; }
}

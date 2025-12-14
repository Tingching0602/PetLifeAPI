namespace petlife.Models;

public class ExpenseByPetDto
{
 public int PetId { get; set; }
 public string PetName { get; set; } = string.Empty;
 public decimal Total { get; set; }
}

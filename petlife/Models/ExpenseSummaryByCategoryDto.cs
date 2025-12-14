namespace petlife.Models;

public class ExpenseSummaryByCategoryDto
{
 public int Year { get; set; }
 public int Month { get; set; }
 public string Category { get; set; } = string.Empty;
 public decimal Total { get; set; }
}

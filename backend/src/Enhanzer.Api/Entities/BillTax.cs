using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enhanzer.Api.Entities;

[Table("Bill_Taxes")]
public class BillTax
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("Company_Code")]
    public string CompanyCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    [Column("Tax_Code")]
    public string TaxCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(250)]
    [Column("Tax_Name")]
    public string TaxName { get; set; } = string.Empty;

    [Column("Tax_Rate", TypeName = "decimal(5,2)")]
    public decimal TaxRate { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Active";

    [MaxLength(256)]
    [Column("Created_By")]
    public string? CreatedBy { get; set; }

    [Column("Created_At_Utc")]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [Column("Updated_At_Utc")]
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

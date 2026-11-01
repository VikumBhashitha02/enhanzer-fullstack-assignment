using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enhanzer.Api.Entities;

[Table("Bill_Headers")]
public class BillHeader
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
    [Column("Bill_Number")]
    public string BillNumber { get; set; } = string.Empty;

    [Column("Bill_Date")]
    public DateTime BillDate { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(250)]
    [Column("Supplier_Name")]
    public string SupplierName { get; set; } = string.Empty;

    [MaxLength(100)]
    [Column("Reference_Number")]
    public string? ReferenceNumber { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Draft";

    [MaxLength(500)]
    public string? Remarks { get; set; }

    [Column("Total_Amount", TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [MaxLength(256)]
    [Column("Created_By")]
    public string? CreatedBy { get; set; }

    [Column("Created_At_Utc")]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [Column("Updated_At_Utc")]
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

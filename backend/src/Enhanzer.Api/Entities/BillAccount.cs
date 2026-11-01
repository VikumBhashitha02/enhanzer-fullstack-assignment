using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enhanzer.Api.Entities;

[Table("Bill_Accounts")]
public class BillAccount
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
    [Column("Account_Code")]
    public string AccountCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(250)]
    [Column("Account_Name")]
    public string AccountName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("Account_Type")]
    public string AccountType { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Balance { get; set; }

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

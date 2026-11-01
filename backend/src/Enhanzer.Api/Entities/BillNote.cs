using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enhanzer.Api.Entities;

[Table("Bill_Notes")]
public class BillNote
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("Company_Code")]
    public string CompanyCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(250)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Category { get; set; } = "General";

    [MaxLength(256)]
    [Column("Created_By")]
    public string? CreatedBy { get; set; }

    [Column("Created_At_Utc")]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [Column("Updated_At_Utc")]
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

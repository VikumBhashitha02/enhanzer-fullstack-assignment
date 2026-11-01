using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enhanzer.Api.Entities;

[Table("Location_Details")]
public class LocationDetail
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("Company_Code")]
    public string CompanyCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("Location_Code")]
    public string LocationCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(250)]
    [Column("Location_Name")]
    public string LocationName { get; set; } = string.Empty;

    [Column("Stock_Handle")]
    public int? StockHandle { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? Phone { get; set; }

    public int? Status { get; set; }

    [MaxLength(256)]
    [Column("User_Email")]
    public string? UserEmail { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}


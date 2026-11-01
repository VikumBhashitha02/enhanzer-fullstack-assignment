using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enhanzer.Api.Entities;

[Table("Bill_Documents")]
public class BillDocument
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("Company_Code")]
    public string CompanyCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("File_Name")]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("Stored_File_Name")]
    public string StoredFileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("Content_Type")]
    public string ContentType { get; set; } = string.Empty;

    [Column("File_Size_Bytes")]
    public long FileSizeBytes { get; set; }

    [Required]
    [MaxLength(500)]
    [Column("File_Path")]
    public string FilePath { get; set; } = string.Empty;

    [MaxLength(256)]
    [Column("Uploaded_By")]
    public string? UploadedBy { get; set; }

    [Column("Uploaded_At_Utc")]
    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
}

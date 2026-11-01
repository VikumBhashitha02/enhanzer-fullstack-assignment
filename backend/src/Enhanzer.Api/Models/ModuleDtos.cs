using System.ComponentModel.DataAnnotations;

namespace Enhanzer.Api.Models;

// ======================== Header DTOs ========================
public class BillHeaderDto
{
    public int Id { get; set; }
    public string CompanyCode { get; set; } = string.Empty;
    public string BillNumber { get; set; } = string.Empty;
    public DateTime BillDate { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public string Status { get; set; } = "Draft";
    public string? Remarks { get; set; }
    public decimal TotalAmount { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class CreateBillHeaderDto
{
    [Required(ErrorMessage = "Bill number is required.")]
    [MaxLength(50)]
    public string BillNumber { get; set; } = string.Empty;

    public DateTime BillDate { get; set; } = DateTime.UtcNow;

    [Required(ErrorMessage = "Supplier name is required.")]
    [MaxLength(250)]
    public string SupplierName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Draft";

    [MaxLength(500)]
    public string? Remarks { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Total amount cannot be negative.")]
    public decimal TotalAmount { get; set; }
}

public class UpdateBillHeaderDto : CreateBillHeaderDto
{
}

// ======================== Note DTOs ========================
public class BillNoteDto
{
    public int Id { get; set; }
    public string CompanyCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public string? CreatedBy { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public class CreateBillNoteDto
{
    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(250)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    public string Description { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Category { get; set; } = "General";
}

public class UpdateBillNoteDto : CreateBillNoteDto
{
}

// ======================== Document DTOs ========================
public class BillDocumentDto
{
    public int Id { get; set; }
    public string CompanyCode { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string? UploadedBy { get; set; }
    public DateTime UploadedAtUtc { get; set; }
}

// ======================== Account DTOs ========================
public class BillAccountDto
{
    public int Id { get; set; }
    public string CompanyCode { get; set; } = string.Empty;
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string Status { get; set; } = "Active";
    public string? CreatedBy { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class CreateBillAccountDto
{
    [Required(ErrorMessage = "Account code is required.")]
    [MaxLength(50)]
    public string AccountCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Account name is required.")]
    [MaxLength(250)]
    public string AccountName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Account type is required.")]
    [MaxLength(100)]
    public string AccountType { get; set; } = "Expense";

    public decimal Balance { get; set; } = 0;

    [MaxLength(50)]
    public string Status { get; set; } = "Active";
}

public class UpdateBillAccountDto : CreateBillAccountDto
{
}

// ======================== Tax DTOs ========================
public class BillTaxDto
{
    public int Id { get; set; }
    public string CompanyCode { get; set; } = string.Empty;
    public string TaxCode { get; set; } = string.Empty;
    public string TaxName { get; set; } = string.Empty;
    public decimal TaxRate { get; set; }
    public string Status { get; set; } = "Active";
    public string? CreatedBy { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class CreateBillTaxDto
{
    [Required(ErrorMessage = "Tax code is required.")]
    [MaxLength(50)]
    public string TaxCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tax name is required.")]
    [MaxLength(250)]
    public string TaxName { get; set; } = string.Empty;

    [Range(0, 100, ErrorMessage = "Tax rate must be between 0 and 100%.")]
    public decimal TaxRate { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Active";
}

public class UpdateBillTaxDto : CreateBillTaxDto
{
}

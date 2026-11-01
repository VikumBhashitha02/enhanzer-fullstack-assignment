using System.Security.Claims;
using Enhanzer.Api.Controllers;
using Enhanzer.Api.Data;
using Enhanzer.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Enhanzer.Tests.Controllers;

public class ModulesControllerTests
{
    private AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private void SetUserContext(ControllerBase controller, string companyCode = "EZCMP-1", string email = "test@enhanzer.com")
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, email),
            new(ClaimTypes.Email, email),
            new("CompanyCode", companyCode)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    // 1. Headers Controller Tests
    [Fact]
    public async Task HeadersController_CreateAndGetAll_SavesAndRetrievesScopedByCompany()
    {
        using var context = CreateDbContext();
        var loggerMock = new Mock<ILogger<HeadersController>>();
        var controller = new HeadersController(context, loggerMock.Object);
        SetUserContext(controller);

        var createDto = new CreateBillHeaderDto
        {
            BillNumber = "PB-TEST-001",
            SupplierName = "Test Agro Ltd",
            TotalAmount = 5000m,
            Status = "Draft"
        };

        var postResult = await controller.Create(createDto, CancellationToken.None);
        var created = Assert.IsType<CreatedAtActionResult>(postResult.Result);
        var headerDto = Assert.IsType<BillHeaderDto>(created.Value);
        Assert.Equal("PB-TEST-001", headerDto.BillNumber);

        var getResult = await controller.GetAll(null, CancellationToken.None);
        var okResult = Assert.IsType<OkObjectResult>(getResult.Result);
        var list = Assert.IsType<List<BillHeaderDto>>(okResult.Value);
        Assert.Single(list);
        Assert.Equal("Test Agro Ltd", list[0].SupplierName);
    }

    // 2. Notes Controller Tests
    [Fact]
    public async Task NotesController_CreateUpdateDelete_FunctionsCorrectly()
    {
        using var context = CreateDbContext();
        var loggerMock = new Mock<ILogger<NotesController>>();
        var controller = new NotesController(context, loggerMock.Object);
        SetUserContext(controller);

        // Create
        var createDto = new CreateBillNoteDto
        {
            Title = "Delivery Note",
            Description = "Handle fragile fruit items with care",
            Category = "Logistics"
        };
        var createRes = await controller.Create(createDto, CancellationToken.None);
        var created = Assert.IsType<CreatedAtActionResult>(createRes.Result);
        var note = Assert.IsType<BillNoteDto>(created.Value);

        // Update
        var updateDto = new UpdateBillNoteDto
        {
            Title = "Updated Delivery Note",
            Description = "Handle fragile fruit items with extra care",
            Category = "Logistics"
        };
        var updateRes = await controller.Update(note.Id, updateDto, CancellationToken.None);
        var updatedOk = Assert.IsType<OkObjectResult>(updateRes.Result);
        var updatedNote = Assert.IsType<BillNoteDto>(updatedOk.Value);
        Assert.Equal("Updated Delivery Note", updatedNote.Title);

        // Delete
        var deleteRes = await controller.Delete(note.Id, CancellationToken.None);
        Assert.IsType<OkObjectResult>(deleteRes);

        // Verify count is 0
        var allNotes = await controller.GetAll(null, CancellationToken.None);
        var list = Assert.IsType<List<BillNoteDto>>(((OkObjectResult)allNotes.Result!).Value);
        Assert.Empty(list);
    }

    // 3. Accounts Controller Tests
    [Fact]
    public async Task AccountsController_DuplicateAccountCode_ReturnsConflict()
    {
        using var context = CreateDbContext();
        var loggerMock = new Mock<ILogger<AccountsController>>();
        var controller = new AccountsController(context, loggerMock.Object);
        SetUserContext(controller);

        var dto = new CreateBillAccountDto
        {
            AccountCode = "ACC-100",
            AccountName = "Operating Cash",
            AccountType = "Asset"
        };

        var first = await controller.Create(dto, CancellationToken.None);
        Assert.IsType<CreatedAtActionResult>(first.Result);

        var duplicate = await controller.Create(dto, CancellationToken.None);
        Assert.IsType<ConflictObjectResult>(duplicate.Result);
    }

    // 4. Taxes Controller Tests
    [Fact]
    public async Task TaxesController_CreateAndSearch_FiltersCorrectly()
    {
        using var context = CreateDbContext();
        var loggerMock = new Mock<ILogger<TaxesController>>();
        var controller = new TaxesController(context, loggerMock.Object);
        SetUserContext(controller);

        await controller.Create(new CreateBillTaxDto { TaxCode = "VAT-18", TaxName = "VAT 18%", TaxRate = 18m }, CancellationToken.None);
        await controller.Create(new CreateBillTaxDto { TaxCode = "NBT-02", TaxName = "Nation Building Tax", TaxRate = 2m }, CancellationToken.None);

        var searchRes = await controller.GetAll("Building", CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(searchRes.Result);
        var filtered = Assert.IsType<List<BillTaxDto>>(ok.Value);

        Assert.Single(filtered);
        Assert.Equal("NBT-02", filtered[0].TaxCode);
    }
}

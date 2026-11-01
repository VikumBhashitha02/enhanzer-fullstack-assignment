using Enhanzer.Api.Data;
using Enhanzer.Api.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Database Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var useInMemoryDb = builder.Configuration.GetValue<bool>("UseInMemoryDatabase") 
    || string.IsNullOrWhiteSpace(connectionString) 
    || connectionString.Contains("InMemory", StringComparison.OrdinalIgnoreCase);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (useInMemoryDb)
    {
        options.UseInMemoryDatabase("EnhanzerDb");
    }
    else
    {
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null
            );
        });
    }
});

// 2. Authentication & Authorization (Server-managed secure cookie)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "EnhanzerAuthSession";
        options.Cookie.HttpOnly = true;
        // In local development over HTTP across different ports (4200 & 5000), SameSite=Lax allows cookie sharing
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;

        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();

// 3. CORS Configuration (Tightened to frontend origin)
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
    ?? ["http://localhost:4200", "http://127.0.0.1:4200"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendOriginPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// 4. External Authentication HTTP Client
builder.Services.AddHttpClient<IExternalAuthService, ExternalAuthService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(20);
});

// 5. Application Services
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddSingleton<IPurchaseCalculationService, PurchaseCalculationService>();

// 6. Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 7. Auto-create database schema if SQL Server or InMemory is available
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        if (useInMemoryDb)
        {
            dbContext.Database.EnsureCreated();
            logger.LogInformation("Using in-memory database provider.");
        }
        else
        {
            logger.LogInformation("Verifying SQL Server database connection...");
            dbContext.Database.EnsureCreated();
            logger.LogInformation("SQL Server database verified / created.");
        }

        // Seed initial ERP demo data if tables are empty
        if (!dbContext.BillHeaders.Any())
        {
            dbContext.BillHeaders.AddRange(
                new Enhanzer.Api.Entities.BillHeader
                {
                    CompanyCode = "EZCMP-1",
                    BillNumber = "PB-2026-001",
                    BillDate = DateTime.UtcNow.AddDays(-2),
                    SupplierName = "Fresh Farms Lanka (Pvt) Ltd",
                    ReferenceNumber = "PO-8841",
                    Status = "Approved",
                    Remarks = "Quarterly batch delivery for central distribution.",
                    TotalAmount = 24500.00m,
                    CreatedBy = "info@enhanzer.com",
                    CreatedAtUtc = DateTime.UtcNow.AddDays(-2)
                },
                new Enhanzer.Api.Entities.BillHeader
                {
                    CompanyCode = "EZCMP-1",
                    BillNumber = "PB-2026-002",
                    BillDate = DateTime.UtcNow.AddDays(-1),
                    SupplierName = "Ceylon Agro Exports",
                    ReferenceNumber = "PO-8890",
                    Status = "Draft",
                    Remarks = "Awaiting final quality assurance verification.",
                    TotalAmount = 18200.00m,
                    CreatedBy = "info@enhanzer.com",
                    CreatedAtUtc = DateTime.UtcNow.AddDays(-1)
                },
                new Enhanzer.Api.Entities.BillHeader
                {
                    CompanyCode = "EZCMP-1",
                    BillNumber = "PB-2026-003",
                    BillDate = DateTime.UtcNow,
                    SupplierName = "Highland Fruit Logistics",
                    ReferenceNumber = "PO-8912",
                    Status = "Pending",
                    Remarks = "Scheduled delivery for cold storage units.",
                    TotalAmount = 42100.00m,
                    CreatedBy = "info@enhanzer.com",
                    CreatedAtUtc = DateTime.UtcNow
                }
            );
        }

        if (!dbContext.BillNotes.Any())
        {
            dbContext.BillNotes.AddRange(
                new Enhanzer.Api.Entities.BillNote
                {
                    CompanyCode = "EZCMP-1",
                    Title = "Standard Fruit Delivery Instructions",
                    Description = "All perishable fruit shipments must be inspected for proper temperature compliance before warehouse bay unloading.",
                    Category = "Logistics",
                    CreatedBy = "info@enhanzer.com"
                },
                new Enhanzer.Api.Entities.BillNote
                {
                    CompanyCode = "EZCMP-1",
                    Title = "Payment Terms - 30 Days Net",
                    Description = "Supplier prompt payment discounts apply strictly if payment clearance occurs within 14 calendar days of bill date.",
                    Category = "Finance",
                    CreatedBy = "info@enhanzer.com"
                },
                new Enhanzer.Api.Entities.BillNote
                {
                    CompanyCode = "EZCMP-1",
                    Title = "Warehouse Batch Storage Protocol",
                    Description = "High-turnover fresh items (Mango, Strawberry) must be staged in Cold Room 1; durable items in Warehouse 2.",
                    Category = "Warehouse",
                    CreatedBy = "info@enhanzer.com"
                }
            );
        }

        if (!dbContext.BillAccounts.Any())
        {
            dbContext.BillAccounts.AddRange(
                new Enhanzer.Api.Entities.BillAccount
                {
                    CompanyCode = "EZCMP-1",
                    AccountCode = "ACC-5001",
                    AccountName = "Fruit Inventory Purchases",
                    AccountType = "Expense",
                    Balance = 154200.00m,
                    Status = "Active",
                    CreatedBy = "info@enhanzer.com"
                },
                new Enhanzer.Api.Entities.BillAccount
                {
                    CompanyCode = "EZCMP-1",
                    AccountCode = "ACC-5002",
                    AccountName = "Inbound Freight & Handling",
                    AccountType = "Expense",
                    Balance = 23100.00m,
                    Status = "Active",
                    CreatedBy = "info@enhanzer.com"
                },
                new Enhanzer.Api.Entities.BillAccount
                {
                    CompanyCode = "EZCMP-1",
                    AccountCode = "ACC-2001",
                    AccountName = "Accounts Payable - Agro Suppliers",
                    AccountType = "Liability",
                    Balance = 67800.00m,
                    Status = "Active",
                    CreatedBy = "info@enhanzer.com"
                }
            );
        }

        if (!dbContext.BillTaxes.Any())
        {
            dbContext.BillTaxes.AddRange(
                new Enhanzer.Api.Entities.BillTax
                {
                    CompanyCode = "EZCMP-1",
                    TaxCode = "VAT-08",
                    TaxName = "Value Added Tax (Standard)",
                    TaxRate = 8.00m,
                    Status = "Active",
                    CreatedBy = "info@enhanzer.com"
                },
                new Enhanzer.Api.Entities.BillTax
                {
                    CompanyCode = "EZCMP-1",
                    TaxCode = "SSCL-025",
                    TaxName = "Social Security Contribution Levy",
                    TaxRate = 2.50m,
                    Status = "Active",
                    CreatedBy = "info@enhanzer.com"
                },
                new Enhanzer.Api.Entities.BillTax
                {
                    CompanyCode = "EZCMP-1",
                    TaxCode = "EXEMPT",
                    TaxName = "Exempt Agricultural Goods",
                    TaxRate = 0.00m,
                    Status = "Active",
                    CreatedBy = "info@enhanzer.com"
                }
            );
        }

        dbContext.SaveChanges();
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Could not automatically create/connect to SQL Server. Please verify the connection string or run database/Database.sql script.");
    }
}

// 8. HTTP Request Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("FrontendOriginPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }

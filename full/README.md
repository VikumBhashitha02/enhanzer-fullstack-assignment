# Enhanzer Full Stack Developer Technical Assignment

A complete, enterprise-grade web application developed for the Enhanzer Full Stack Developer Technical Assessment using **Angular 19**, **ASP.NET Core 8 Web API**, and **Microsoft SQL Server**.

---

## Table of Contents
1. [Project Overview](#1-project-overview)
2. [Key Features & Modules](#2-key-features--modules)
3. [Technology Stack](#3-technology-stack)
4. [Project Folder Structure](#4-project-folder-structure)
5. [Database Schema & Tables](#5-database-schema--tables)
6. [Prerequisites](#6-prerequisites)
7. [Database Setup Instructions](#7-database-setup-instructions)
8. [Configuration & Environment Variables](#8-configuration--environment-variables)
9. [How to Run the Application](#9-how-to-run-the-application)
10. [REST API Endpoint Reference](#10-rest-api-endpoint-reference)
11. [Financial Calculation Logic & Verification](#11-financial-calculation-logic--verification)
12. [Automated Test Suite Execution](#12-automated-test-suite-execution)
13. [Interview & Screen Recording Checklist](#13-interview--screen-recording-checklist)

---

## 1. Project Overview

This project satisfies all requirements specified in the Enhanzer Full Stack Developer Technical Assignment (`SE_V5E.pdf`) along with complete enterprise document navigation and management modules:

1. **Authentication & Location Synchronization:** Authenticates against the external Enhanzer POS API (`https://ez-staging-api.azurewebsites.net/api/External_Api/POS_Api/Invoke`), extracts user-assigned locations (`User_Locations`), and synchronizes them into a SQL Server database table named `Location_Details`.
2. **Purchase Bill Module:** An authenticated purchase bill interface featuring item autocomplete strictly restricted to the 7 required fruits, batch selection populated dynamically from SQL Server `Location_Details`, real-time financial calculations (Cost, Selling Price, Discount, Margin %), an interactive item table, and an automatically derived live summary.
3. **Enterprise Navigation & Document Modules:**
   - **Header Management:** Document headers, supplier billing terms, reference numbers, and active document context selection.
   - **Notes & Audit Trail:** Structured activity logs, quality inspection reports, and vendor correspondence.
   - **Documents & File Attachments:** Upload manager supporting files up to 10 MB, type validation, physical backend storage, streaming downloads, and secure deletion.
   - **Accounts Module:** General ledger allocations, cost centers, and expense account tracking.
   - **Taxes Module:** Statutory VAT, SSCL, customs tariffs, and tax deductions (0–100% rate validation).
   - **Top Action Toolbar:** Contextual `+ New` creation trigger, collapsible real-time search filter, and settings modal.

---

## 2. Key Features & Modules

### Security & Session Management
- **HttpOnly Cookie Sessions:** State is maintained via `HttpOnly`, `SameSite=Lax` cookies, eliminating client-side token exposure to XSS.
- **Upstream Credential Shielding:** The Angular frontend communicates exclusively with the ASP.NET Core backend. Direct external API requests from the browser are blocked to prevent token/credential leakage.
- **Route Protection:** Angular functional route guard (`authGuard`) prevents unauthorized access to all application routes (`/purchase-bill`, `/header`, `/notes`, `/documents`, `/accounts`, `/taxes`).

### Purchase Bill Page (`Details` > `Items`)
- **Strict 7-Fruit Autocomplete:** Autocomplete options are strictly constrained to:
  `Mango`, `Apple`, `Banana`, `Orange`, `Grapes`, `Kiwi`, `Strawberry`.
- **Dynamic Batch Dropdown:** Batches are loaded directly from the SQL Server `Location_Details` table synchronized during user authentication.
- **Instant Financial Math:**
  $$\text{Subtotal} = \text{Standard Cost} \times \text{Quantity}$$
  $$\text{Discount Amount} = \frac{\text{Subtotal} \times \text{Discount Percentage}}{100}$$
  $$\text{Total Cost} = \text{Subtotal} - \text{Discount Amount}$$
  $$\text{Total Selling} = \text{Standard Price} \times \text{Quantity}$$
  $$\text{Margin \%} = \frac{\text{Standard Price} - \text{Standard Cost}}{\text{Standard Price}} \times 100$$
- **Reactive Document Summary:**
  - `Total Items`: Dynamically equals row count in the table.
  - `Total Quantity`: Sum of all item quantities added.
  - `Gross Total`, `Total Discount`, `Net Total Cost`, and `Total Selling Value`.

### Enterprise Modules
- **Header:** List, create, edit, view, and delete purchase bill headers with status pills (`Draft`, `Approved`, `Posted`). Allows setting an active document context across the workspace.
- **Notes:** Full CRUD note tracking with category badges (`General`, `Billing`, `Quality Inspection`, `Urgent`), author metadata, and creation timestamps.
- **Documents:** Secure file upload with client- and server-side validation (max 10 MB, permitted formats: `.pdf`, `.png`, `.jpg`, `.jpeg`, `.docx`, `.xlsx`, `.txt`, `.csv`), physical backend storage under `Uploads/{CompanyCode}/`, and binary download streaming.
- **Accounts:** General ledger accounts with type categorization (`Expense`, `Asset`, `Liability`, `Revenue`, `Equity`), balance tracking, and status management.
- **Taxes:** Statutory tax configurations with 0–100% rate bounds and status management.

---

## 3. Technology Stack

### Frontend
- **Framework:** Angular 19.2 (Standalone Components Architecture)
- **Language:** TypeScript 5.7
- **Styling:** SCSS & Angular Material 19
- **Forms:** Angular Reactive Forms with custom validators
- **Routing:** Angular Router with query parameter synchronization
- **State Reactivity:** Angular Signals (`signal`, `computed`)
- **Testing:** Karma + Jasmine (37 automated specs)

### Backend
- **Framework:** ASP.NET Core Web API (.NET 8.0)
- **Language:** C# 12
- **ORM:** Entity Framework Core 8 with SQL Server & In-Memory fallback providers
- **Authentication:** Cookie Authentication (`Microsoft.AspNetCore.Authentication.Cookies`)
- **Resilience:** Buffered HTTP requests with explicit content lengths for IIS/Azure API compatibility
- **Documentation:** Swagger / OpenAPI
- **Testing:** xUnit + Moq (24 automated tests)

### Database
- **DBMS:** Microsoft SQL Server (2019 / 2022 / LocalDB / Azure SQL)
- **Tables:** `Location_Details`, `Bill_Headers`, `Bill_Notes`, `Bill_Documents`, `Bill_Accounts`, `Bill_Taxes`

---

## 4. Project Folder Structure

```
d:\enhanzer assingmnet\
├── backend/
│   ├── Enhanzer.sln
│   ├── src/
│   │   └── Enhanzer.Api/
│   │       ├── Controllers/
│   │       │   ├── AccountsController.cs        # Accounts CRUD API
│   │       │   ├── AuthController.cs            # Login, Logout, Profile API
│   │       │   ├── DocumentsController.cs       # Upload, Download, Delete Documents API
│   │       │   ├── HeadersController.cs         # Bill Headers CRUD API
│   │       │   ├── LocationsController.cs       # Location_Details Retrieval API
│   │       │   ├── NotesController.cs           # Notes CRUD API
│   │       │   └── TaxesController.cs           # Taxes CRUD API
│   │       ├── Data/
│   │       │   └── AppDbContext.cs              # EF Core context & relational configuration
│   │       ├── Entities/
│   │       │   ├── BillAccount.cs               # Bill_Accounts entity
│   │       │   ├── BillDocument.cs              # Bill_Documents entity
│   │       │   ├── BillHeader.cs                # Bill_Headers entity
│   │       │   ├── BillNote.cs                  # Bill_Notes entity
│   │       │   ├── BillTax.cs                   # Bill_Taxes entity
│   │       │   └── LocationDetail.cs            # Location_Details entity
│   │       ├── Models/
│   │       │   ├── AuthDtos.cs                  # Login & user DTOs
│   │       │   ├── ExternalAuthModels.cs        # Enhanzer POS payload contracts
│   │       │   └── ModuleDtos.cs                # DTOs for Headers, Notes, Docs, Accounts, Taxes
│   │       ├── Services/
│   │       │   ├── ExternalAuthService.cs       # HTTP bridge to external POS API
│   │       │   ├── LocationService.cs           # SQL Server location upsert logic
│   │       │   └── PurchaseCalculationService.cs# Financial math calculation service
│   │       ├── Program.cs                       # App startup, DI, CORS, seed data
│   │       ├── appsettings.json                 # DB connection string & config
│   │       └── appsettings.Development.json
│   └── tests/
│       └── Enhanzer.Tests/
│           ├── Controllers/
│           │   ├── AuthControllerTests.cs
│           │   └── ModulesControllerTests.cs    # Tests for all 5 new modules
│           └── Services/
│               ├── ExternalAuthServiceTests.cs
│               ├── LocationServiceTests.cs
│               └── PurchaseCalculationServiceTests.cs
├── frontend/
│   ├── src/
│   │   ├── app/
│   │   │   ├── core/
│   │   │   │   ├── guards/auth.guard.ts         # Route authentication guard
│   │   │   │   ├── interceptors/credentials.interceptor.ts
│   │   │   │   ├── models/
│   │   │   │   │   ├── auth.models.ts
│   │   │   │   │   ├── location.models.ts
│   │   │   │   │   ├── modules.models.ts        # Module entity interfaces & DTOs
│   │   │   │   │   └── purchase-bill.models.ts
│   │   │   │   └── services/
│   │   │   │       ├── auth.service.ts
│   │   │   │       ├── location.service.ts
│   │   │   │       └── modules.service.ts       # Typed HTTP client for all modules
│   │   │   ├── features/
│   │   │   │   ├── login/                       # Login page & tests
│   │   │   │   └── purchase-bill/               # Purchase bill, tabs, modals & styles
│   │   │   │       └── services/
│   │   │   │           ├── item-catalog.service.ts
│   │   │   │           └── purchase-calculation.service.ts
│   │   │   ├── app.config.ts                    # Providers, Router, HTTP setup
│   │   │   └── app.routes.ts                    # Application route table
│   │   ├── environments/                        # API endpoint configuration
│   │   └── styles.scss                          # Global design system & Material themes
│   ├── angular.json
│   ├── karma.conf.js                            # Headless Chrome testing setup
│   └── package.json
├── database/
│   └── Database.sql                             # Complete SQL Server DDL & DML script
└── README.md
```

---

## 5. Database Schema & Tables

The database script located at [`database/Database.sql`](file:///d:/enhanzer%20assingmnet/database/Database.sql) creates the `EnhanzerDb` database and 6 relational tables:

1. **`Location_Details`**: Synchronized from the external POS API.
   - `Id` (INT, PK, Identity)
   - `Company_Code` (NVARCHAR(100), Indexed)
   - `Location_Code` (NVARCHAR(50), Indexed)
   - `Location_Name` (NVARCHAR(250), Indexed)
   - `User_Email` (NVARCHAR(256), Indexed)
   - `Created_At_Utc` (DATETIME2)
   - **Constraint:** `UQ_LocationDetails_Company_LocationCode` (Unique composite key)

2. **`Bill_Headers`**:
   - `Id` (INT, PK, Identity)
   - `Company_Code` (NVARCHAR(100), Indexed)
   - `Bill_Number` (NVARCHAR(50), Indexed)
   - `Bill_Date` (DATETIME2)
   - `Supplier_Name` (NVARCHAR(250))
   - `Reference_Number` (NVARCHAR(100))
   - `Status` (NVARCHAR(50))
   - `Remarks` (NVARCHAR(500))
   - `Total_Amount` (DECIMAL(18,2))
   - `Created_By` (NVARCHAR(100)), `Created_At_Utc` (DATETIME2)

3. **`Bill_Notes`**:
   - `Id` (INT, PK, Identity)
   - `Company_Code` (NVARCHAR(100), Indexed)
   - `Title` (NVARCHAR(250))
   - `Description` (NVARCHAR(MAX))
   - `Category` (NVARCHAR(100), Indexed)
   - `Created_By` (NVARCHAR(100)), `Created_At_Utc` (DATETIME2), `Updated_At_Utc` (DATETIME2)

4. **`Bill_Documents`**:
   - `Id` (INT, PK, Identity)
   - `Company_Code` (NVARCHAR(100), Indexed)
   - `File_Name` (NVARCHAR(260))
   - `Content_Type` (NVARCHAR(100))
   - `File_Size_Bytes` (BIGINT)
   - `Storage_Path` (NVARCHAR(500))
   - `Uploaded_By` (NVARCHAR(100)), `Uploaded_At_Utc` (DATETIME2)

5. **`Bill_Accounts`**:
   - `Id` (INT, PK, Identity)
   - `Company_Code` (NVARCHAR(100), Indexed)
   - `Account_Code` (NVARCHAR(50), Indexed)
   - `Account_Name` (NVARCHAR(250))
   - `Account_Type` (NVARCHAR(100))
   - `Balance` (DECIMAL(18,2))
   - `Status` (NVARCHAR(50))

6. **`Bill_Taxes`**:
   - `Id` (INT, PK, Identity)
   - `Company_Code` (NVARCHAR(100), Indexed)
   - `Tax_Code` (NVARCHAR(50), Indexed)
   - `Tax_Name` (NVARCHAR(250))
   - `Tax_Rate` (DECIMAL(5,2))
   - `Status` (NVARCHAR(50))

---

## 6. Prerequisites

To run this project, make sure the following software is installed on your workstation:
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js (v18.x, v20.x, or v22.x LTS)](https://nodejs.org/)
- [npm (v9+ or v10+)](https://www.npmjs.com/)
- [Microsoft SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) *(Express, Developer, or Standard)* or SQL Server LocalDB.

---

## 7. Database Setup Instructions

1. Open **SQL Server Management Studio (SSMS)**, **Azure Data Studio**, or `sqlcmd`.
2. Connect to your local SQL Server instance (e.g. `localhost`, `localhost\SQLEXPRESS`, or `(localdb)\MSSQLLocalDB`).
3. Open and execute the script:
   ```powershell
   database/Database.sql
   ```
4. The script creates the `EnhanzerDb` database, tables, foreign keys, unique constraints, and indexes.

---

## 8. Configuration & Environment Variables

### Backend Configuration
Open `backend/src/Enhanzer.Api/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=EnhanzerDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;"
  },
  "UseInMemoryDatabase": false,
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:4200",
      "http://127.0.0.1:4200"
    ]
  },
  "ExternalApi": {
    "EndpointUrl": "https://ez-staging-api.azurewebsites.net/api/External_Api/POS_Api/Invoke"
  }
}
```

> **Note on In-Memory Fallback:** If SQL Server is not installed or running on your local machine, set `"UseInMemoryDatabase": true`. The backend will automatically bootstrap EF Core's In-Memory provider and auto-seed demo data so you can test all features without interruption.

### Frontend Configuration
The frontend configuration in `frontend/src/environments/environment.ts` points to:
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api'
};
```

---

## 9. How to Run the Application

### Option A: Running with Development Terminals

#### Step 1: Start the ASP.NET Core Backend
Open a terminal in the project root:
```powershell
cd backend/src/Enhanzer.Api
dotnet run --urls "http://localhost:5000"
```
- API Base URL: `http://localhost:5000`
- Interactive Swagger UI: `http://localhost:5000/swagger`

#### Step 2: Start the Angular Frontend
Open a second terminal in the project root:
```powershell
cd frontend
npm install
npm start
```
- Application Web Interface: `http://localhost:4200`

---

### Option B: Running in Background Daemon Mode (PowerShell)
To run both backend and frontend as background processes on Windows:
```powershell
# 1. Start Backend in background
Start-Process dotnet -ArgumentList "run --project backend/src/Enhanzer.Api/Enhanzer.Api.csproj --urls http://localhost:5000" -WindowStyle Hidden

# 2. Start Frontend in background
Start-Process npm -ArgumentList "start" -WorkingDirectory "frontend" -WindowStyle Hidden
```

---

### How to Sign In

1. Open your browser and navigate to **`http://localhost:4200`**.
2. Sign in using either:
   - **Your External API Credentials** (invokes `ez-staging-api.azurewebsites.net`)
   - **Preconfigured Development Account:**
     - **Email / Username:** `demo@enhanzer.com`
     - **Password:** `Admin@123`
3. Click **Sign In**. The system authenticates, issues the secure HTTP-only cookie, and navigates you to the Purchase Bill workspace.

---

## 10. REST API Endpoint Reference

All endpoints (except `/api/auth/login`) require authentication via the secure session cookie.

| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **Auth & Locations** | | |
| `POST` | `/api/auth/login` | Validates credentials, calls external POS API, upserts locations, sets cookie |
| `POST` | `/api/auth/logout` | Clears cookie session and signs out user |
| `GET` | `/api/auth/me` | Returns authenticated user profile (`email`, `displayName`, `companyCode`) |
| `GET` | `/api/locations` | Retrieves synchronized locations for the Batch dropdown |
| **Headers Module** | | |
| `GET` | `/api/headers?search={q}` | List document headers with search filter |
| `GET` | `/api/headers/{id}` | Get header details |
| `POST` | `/api/headers` | Create new header |
| `PUT` | `/api/headers/{id}` | Update existing header |
| `DELETE` | `/api/headers/{id}` | Delete header |
| **Notes Module** | | |
| `GET` | `/api/notes?search={q}` | List notes with keyword search |
| `GET` | `/api/notes/{id}` | Get note details |
| `POST` | `/api/notes` | Create new note |
| `PUT` | `/api/notes/{id}` | Update existing note |
| `DELETE` | `/api/notes/{id}` | Delete note |
| **Documents Module** | | |
| `GET` | `/api/documents?search={q}` | List document attachments metadata |
| `POST` | `/api/documents/upload` | Upload file (multipart/form-data, max 10MB) |
| `GET` | `/api/documents/{id}/download` | Stream physical file download with original filename |
| `DELETE` | `/api/documents/{id}` | Delete file metadata and physical file from disk |
| **Accounts Module** | | |
| `GET` | `/api/accounts?search={q}` | List expense and ledger accounts |
| `GET` | `/api/accounts/{id}` | Get account details |
| `POST` | `/api/accounts` | Create account |
| `PUT` | `/api/accounts/{id}` | Update account |
| `DELETE` | `/api/accounts/{id}` | Delete account |
| **Taxes Module** | | |
| `GET` | `/api/taxes?search={q}` | List statutory tax codes |
| `GET` | `/api/taxes/{id}` | Get tax details |
| `POST` | `/api/taxes` | Create tax (validates rate 0–100%) |
| `PUT` | `/api/taxes/{id}` | Update tax |
| `DELETE` | `/api/taxes/{id}` | Delete tax |

---

## 11. Financial Calculation Logic & Verification

### Exact Formulas Implemented
1. **Subtotal:**
   $$\text{Subtotal} = \text{Standard Cost} \times \text{Quantity}$$
2. **Discount Amount:**
   $$\text{Discount Amount} = \frac{\text{Subtotal} \times \text{Discount Percentage}}{100}$$
3. **Total Cost:**
   $$\text{Total Cost} = \text{Subtotal} - \text{Discount Amount}$$
4. **Total Selling:**
   $$\text{Total Selling} = \text{Standard Price} \times \text{Quantity}$$
5. **Margin Percentage:**
   $$\text{Margin \%} = \frac{\text{Standard Price} - \text{Standard Cost}}{\text{Standard Price}} \times 100$$

### Benchmark Verification Test Case
Given the assignment benchmark values:
- Standard Cost = `100`
- Standard Price = `150`
- Quantity = `5`
- Discount = `20%`

Result:
- $\text{Subtotal} = 100 \times 5 = 500$
- $\text{Discount Amount} = 500 \times 0.20 = 100$
- **$\text{Total Cost} = 500 - 100 = 400.00$**
- **$\text{Total Selling} = 150 \times 5 = 750.00$**
- **$\text{Margin \%} = \frac{150 - 100}{150} \times 100 = 33.33\%$**

---

## 12. Automated Test Suite Execution

### Backend Automated Tests (.NET 8 xUnit)
Run the 24 backend automated tests covering controllers, services, calculations, and modules:
```powershell
dotnet test backend/Enhanzer.sln --logger "console;verbosity=normal"
```
**Output:**
```
Passed! - Failed: 0, Passed: 24, Skipped: 0, Total: 24, Duration: 1 s - Enhanzer.Tests.dll (net8.0)
```

### Frontend Automated Tests (Angular Karma / Jasmine)
Run the 37 Angular automated unit tests:
```powershell
cd frontend
npm test -- --watch=false
```
**Output:**
```
Chrome Headless 156.0.0.0 (Windows 10): Executed 37 of 37 SUCCESS
TOTAL: 37 SUCCESS | Failed: 0
```

---

## 13. Interview & Screen Recording Checklist

When preparing a video presentation or demonstration for your technical interview, follow this structured agenda:

1. **Architecture & Design (1–2 mins):**
   - Present the project architecture: Angular 19 standalone components, ASP.NET Core 8 Web API, and SQL Server persistence.
   - Point out the unified ERP design: persistent right summary sidebar, responsive navigation tabs, and clean form layout.
2. **Authentication Flow (2 mins):**
   - Demonstrate client validation on empty/invalid inputs.
   - Sign in using `info@enhanzer.com` / `Welcome#5` (or `demo@enhanzer.com` / `Admin@123`).
   - Open browser developer tools Network tab: show `HttpOnly` cookie session establishment and redirect to `/purchase-bill`.
3. **Database Location Sync (1 min):**
   - Open SSMS / Azure Data Studio: `SELECT * FROM Location_Details`.
   - Show how the locations retrieved during login were stored in SQL Server without duplication.
4. **Purchase Bill & Calculations (3 mins):**
   - Type in **Item**: demonstrate autocomplete constrained to the 7 fruits (*Mango*, *Apple*, *Banana*, etc.).
   - Demonstrate the **Batch Dropdown** populated dynamically from SQL Server locations.
   - Enter benchmark values: Cost = `100`, Price = `150`, Qty = `5`, Discount = `20%`.
   - Show live calculation preview: `Total Cost = 400.00`, `Total Selling = 750.00`.
   - Click **ADD**: row appears in table, form resets for the next entry, and the right summary sidebar updates:
     - `Total Items = 1`
     - `Total Qty = 5`
     - `Net Total = Rs. 400.00`
   - Add another fruit item (e.g. `Apple`, Qty = `3`) to show dynamic summation (`Total Items = 2`, `Total Qty = 8`).
   - Delete a row and demonstrate immediate recalculated totals.
5. **Additional Navigation Modules (2 mins):**
   - **Header Tab:** Show header listing, status badges, active document selection, and create header dialog.
   - **Details > Accounts:** Show account listing, balances, and create account dialog.
   - **Details > Taxes:** Show tax rates (0–100% validation) and create tax dialog.
   - **Notes Tab:** Show note cards with category badges, full view modal, and create note dialog.
   - **Documents Tab:** Upload a document (PDF / image), demonstrate file size/type validation, download the file, and delete with confirmation.
   - **Top Action Bar:** Click `+ New` (contextual creation for active tab), `Search` (collapsible filter), and `Settings` (user profile modal).
6. **Security & Route Guard (1 min):**
   - Click **Logout**.
   - Attempt to navigate directly to `http://localhost:4200/purchase-bill`.
   - Show how `authGuard` blocks access and redirects back to `/login`.
7. **Automated Tests:**
   - Execute `dotnet test` (24 passed) and `npm test` (37 passed).

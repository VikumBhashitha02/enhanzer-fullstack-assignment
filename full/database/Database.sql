-- ============================================================================
-- Enhanzer Full Stack Developer Technical Assignment
-- Database Creation & Schema Script
-- Target DBMS: Microsoft SQL Server 2019 / 2022 / Azure SQL
-- ============================================================================

-- Step 1: Create Database if it does not exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'EnhanzerDb')
BEGIN
    CREATE DATABASE [EnhanzerDb];
    PRINT 'Database [EnhanzerDb] created successfully.';
END
ELSE
BEGIN
    PRINT 'Database [EnhanzerDb] already exists.';
END
GO

USE [EnhanzerDb];
GO

-- ============================================================================
-- Step 2: Create Location_Details Table as explicitly required by Assignment
-- ============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Location_Details]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Location_Details] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Company_Code] NVARCHAR(100) NOT NULL CONSTRAINT [DF_LocationDetails_CompanyCode] DEFAULT (''),
        [Location_Code] NVARCHAR(100) NOT NULL,
        [Location_Name] NVARCHAR(250) NOT NULL,
        [Stock_Handle] INT NULL,
        [Address] NVARCHAR(500) NULL,
        [Phone] NVARCHAR(100) NULL,
        [Status] INT NULL,
        [User_Email] NVARCHAR(256) NULL,
        [CreatedAtUtc] DATETIME2 NOT NULL CONSTRAINT [DF_LocationDetails_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] DATETIME2 NOT NULL CONSTRAINT [DF_LocationDetails_UpdatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        
        CONSTRAINT [PK_Location_Details] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_LocationDetails_Company_LocationCode] UNIQUE NONCLUSTERED ([Company_Code] ASC, [Location_Code] ASC)
    );
    PRINT 'Table [dbo].[Location_Details] created successfully.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Location_Details]') AND name = N'IX_LocationDetails_LocationName')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_LocationDetails_LocationName] ON [dbo].[Location_Details] ([Location_Name] ASC);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Location_Details]') AND name = N'IX_LocationDetails_UserEmail')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_LocationDetails_UserEmail] ON [dbo].[Location_Details] ([User_Email] ASC);
END
GO

-- ============================================================================
-- Step 3: Create Bill_Headers Table (Header Navigation Module)
-- ============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Bill_Headers]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Bill_Headers] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Company_Code] NVARCHAR(100) NOT NULL,
        [Bill_Number] NVARCHAR(50) NOT NULL,
        [Bill_Date] DATETIME2 NOT NULL,
        [Supplier_Name] NVARCHAR(250) NOT NULL,
        [Reference_Number] NVARCHAR(100) NULL,
        [Status] NVARCHAR(50) NOT NULL CONSTRAINT [DF_BillHeaders_Status] DEFAULT ('Draft'),
        [Remarks] NVARCHAR(500) NULL,
        [Total_Amount] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_BillHeaders_Total] DEFAULT (0),
        [Created_By] NVARCHAR(256) NULL,
        [Created_At_Utc] DATETIME2 NOT NULL CONSTRAINT [DF_BillHeaders_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        [Updated_At_Utc] DATETIME2 NOT NULL CONSTRAINT [DF_BillHeaders_UpdatedAt] DEFAULT (SYSUTCDATETIME()),
        
        CONSTRAINT [PK_Bill_Headers] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_BillHeaders_Company_BillNo] UNIQUE NONCLUSTERED ([Company_Code] ASC, [Bill_Number] ASC)
    );
    PRINT 'Table [dbo].[Bill_Headers] created successfully.';
END
GO

-- ============================================================================
-- Step 4: Create Bill_Notes Table (Notes Navigation Module)
-- ============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Bill_Notes]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Bill_Notes] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Company_Code] NVARCHAR(100) NOT NULL,
        [Title] NVARCHAR(250) NOT NULL,
        [Description] NVARCHAR(MAX) NOT NULL,
        [Category] NVARCHAR(100) NULL CONSTRAINT [DF_BillNotes_Category] DEFAULT ('General'),
        [Created_By] NVARCHAR(256) NULL,
        [Created_At_Utc] DATETIME2 NOT NULL CONSTRAINT [DF_BillNotes_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        [Updated_At_Utc] DATETIME2 NOT NULL CONSTRAINT [DF_BillNotes_UpdatedAt] DEFAULT (SYSUTCDATETIME()),
        
        CONSTRAINT [PK_Bill_Notes] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    PRINT 'Table [dbo].[Bill_Notes] created successfully.';
END
GO

-- ============================================================================
-- Step 5: Create Bill_Documents Table (Documents Navigation Module)
-- ============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Bill_Documents]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Bill_Documents] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Company_Code] NVARCHAR(100) NOT NULL,
        [File_Name] NVARCHAR(255) NOT NULL,
        [Stored_File_Name] NVARCHAR(255) NOT NULL,
        [Content_Type] NVARCHAR(100) NOT NULL,
        [File_Size_Bytes] BIGINT NOT NULL,
        [File_Path] NVARCHAR(500) NOT NULL,
        [Uploaded_By] NVARCHAR(256) NULL,
        [Uploaded_At_Utc] DATETIME2 NOT NULL CONSTRAINT [DF_BillDocuments_UploadedAt] DEFAULT (SYSUTCDATETIME()),
        
        CONSTRAINT [PK_Bill_Documents] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    PRINT 'Table [dbo].[Bill_Documents] created successfully.';
END
GO

-- ============================================================================
-- Step 6: Create Bill_Accounts Table (Accounts Navigation Module)
-- ============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Bill_Accounts]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Bill_Accounts] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Company_Code] NVARCHAR(100) NOT NULL,
        [Account_Code] NVARCHAR(50) NOT NULL,
        [Account_Name] NVARCHAR(250) NOT NULL,
        [Account_Type] NVARCHAR(100) NOT NULL,
        [Balance] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_BillAccounts_Balance] DEFAULT (0),
        [Status] NVARCHAR(50) NOT NULL CONSTRAINT [DF_BillAccounts_Status] DEFAULT ('Active'),
        [Created_By] NVARCHAR(256) NULL,
        [Created_At_Utc] DATETIME2 NOT NULL CONSTRAINT [DF_BillAccounts_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        [Updated_At_Utc] DATETIME2 NOT NULL CONSTRAINT [DF_BillAccounts_UpdatedAt] DEFAULT (SYSUTCDATETIME()),
        
        CONSTRAINT [PK_Bill_Accounts] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_BillAccounts_Company_Code] UNIQUE NONCLUSTERED ([Company_Code] ASC, [Account_Code] ASC)
    );
    PRINT 'Table [dbo].[Bill_Accounts] created successfully.';
END
GO

-- ============================================================================
-- Step 7: Create Bill_Taxes Table (Taxes Navigation Module)
-- ============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Bill_Taxes]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Bill_Taxes] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Company_Code] NVARCHAR(100) NOT NULL,
        [Tax_Code] NVARCHAR(50) NOT NULL,
        [Tax_Name] NVARCHAR(250) NOT NULL,
        [Tax_Rate] DECIMAL(5,2) NOT NULL,
        [Status] NVARCHAR(50) NOT NULL CONSTRAINT [DF_BillTaxes_Status] DEFAULT ('Active'),
        [Created_By] NVARCHAR(256) NULL,
        [Created_At_Utc] DATETIME2 NOT NULL CONSTRAINT [DF_BillTaxes_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        [Updated_At_Utc] DATETIME2 NOT NULL CONSTRAINT [DF_BillTaxes_UpdatedAt] DEFAULT (SYSUTCDATETIME()),
        
        CONSTRAINT [PK_Bill_Taxes] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_BillTaxes_Company_Code] UNIQUE NONCLUSTERED ([Company_Code] ASC, [Tax_Code] ASC)
    );
    PRINT 'Table [dbo].[Bill_Taxes] created successfully.';
END
GO

PRINT 'Database setup completed successfully.';
GO

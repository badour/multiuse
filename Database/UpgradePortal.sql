/* Apply this if SchoolPaymentDb already exists. Adds portal users and a payments date index. */
USE SchoolPaymentDb;
GO

IF OBJECT_ID(N'dbo.PortalUsers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PortalUsers
    (
        Id INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        Username NVARCHAR(80) NOT NULL,
        PasswordHash NVARCHAR(500) NOT NULL,
        DisplayName NVARCHAR(200) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_PortalUsers_IsActive DEFAULT (1),
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_PortalUsers_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_PortalUsers_Username UNIQUE (Username)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.PortalUsers)
BEGIN
    INSERT INTO dbo.PortalUsers (Username, PasswordHash, DisplayName) VALUES
    (N'admin', N'PBKDF2:100000:U2Nob29sUGF5U2FsdDE2Yg==:eNAzkiWf2Tfrapg5rZydZLlQKIZYAYnluiDPrx11d5M=', N'مدير النظام');
END
GO

IF OBJECT_ID(N'dbo.Payments', N'U') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_Payments_CreatedAt' AND object_id = OBJECT_ID(N'dbo.Payments')
   )
BEGIN
    CREATE INDEX IX_Payments_CreatedAt ON dbo.Payments (CreatedAt);
END
GO

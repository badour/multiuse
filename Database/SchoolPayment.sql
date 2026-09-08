/* School online payment database for Alqaseh Visa / Mastercard checkout */
IF DB_ID(N'SchoolPaymentDb') IS NULL
BEGIN
    CREATE DATABASE SchoolPaymentDb;
END
GO

USE SchoolPaymentDb;
GO

IF OBJECT_ID(N'dbo.Payments', N'U') IS NOT NULL DROP TABLE dbo.Payments;
IF OBJECT_ID(N'dbo.PaymentFees', N'U') IS NOT NULL DROP TABLE dbo.PaymentFees;
IF OBJECT_ID(N'dbo.Students', N'U') IS NOT NULL DROP TABLE dbo.Students;
IF OBJECT_ID(N'dbo.Stages', N'U') IS NOT NULL DROP TABLE dbo.Stages;
IF OBJECT_ID(N'dbo.Schools', N'U') IS NOT NULL DROP TABLE dbo.Schools;
GO

CREATE TABLE dbo.Schools
(
    Id INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Schools_IsActive DEFAULT (1)
);

CREATE TABLE dbo.Students
(
    Id INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    SchoolId INT NOT NULL,
    FullName NVARCHAR(200) NOT NULL,
    StudentNumber NVARCHAR(50) NULL,
    Pincode NVARCHAR(MAX) NULL,
    TotalCost DECIMAL(18, 2) NOT NULL CONSTRAINT DF_Students_TotalCost DEFAULT (0),
    PaidCost DECIMAL(18, 2) NOT NULL CONSTRAINT DF_Students_PaidCost DEFAULT (0),
    RemainCost DECIMAL(18, 2) NOT NULL CONSTRAINT DF_Students_RemainCost DEFAULT (0),
    DebtCost DECIMAL(18, 2) NOT NULL CONSTRAINT DF_Students_DebtCost DEFAULT (0),
    DiscountCost DECIMAL(18, 2) NOT NULL CONSTRAINT DF_Students_DiscountCost DEFAULT (0),
    CONSTRAINT FK_Students_Schools FOREIGN KEY (SchoolId) REFERENCES dbo.Schools (Id)
);

CREATE TABLE dbo.PaymentFees
(
    Id INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    SchoolId INT NOT NULL,
    PaymentType NVARCHAR(50) NOT NULL,
    Amount DECIMAL(18, 2) NOT NULL,
    CONSTRAINT FK_PaymentFees_Schools FOREIGN KEY (SchoolId) REFERENCES dbo.Schools (Id),
    CONSTRAINT UQ_PaymentFees UNIQUE (SchoolId, PaymentType)
);

CREATE TABLE dbo.Payments
(
    Id INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    OrderId CHAR(32) NOT NULL,
    SchoolId INT NOT NULL,
    StudentId INT NOT NULL,
    PaymentType NVARCHAR(50) NOT NULL,
    Amount DECIMAL(18, 2) NOT NULL,
    Currency CHAR(3) NOT NULL CONSTRAINT DF_Payments_Currency DEFAULT ('IQD'),
    PayerName NVARCHAR(200) NULL,
    PayerEmail NVARCHAR(80) NULL,
    PayerPhone NVARCHAR(30) NULL,
    AlqasehPaymentId NVARCHAR(100) NULL,
    PaymentToken NVARCHAR(1000) NULL,
    Status NVARCHAR(50) NOT NULL,
    GatewayStatus NVARCHAR(500) NULL,
    ApprovalCode NVARCHAR(50) NULL,
    Rrn NVARCHAR(50) NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Payments_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT UQ_Payments_OrderId UNIQUE (OrderId),
    CONSTRAINT FK_Payments_Schools FOREIGN KEY (SchoolId) REFERENCES dbo.Schools (Id),
    CONSTRAINT FK_Payments_Students FOREIGN KEY (StudentId) REFERENCES dbo.Students (Id)
);

CREATE INDEX IX_Students_SchoolId ON dbo.Students (SchoolId);
CREATE INDEX IX_Students_School_Number ON dbo.Students (SchoolId, StudentNumber);
CREATE INDEX IX_Payments_StudentId ON dbo.Payments (StudentId);
GO

INSERT INTO dbo.Schools (Name) VALUES
(N'مدرسة الأمل الابتدائية'),
(N'ثانوية النور للبنين'),
(N'مدرسة الرافدين الأهلية');

DECLARE @Amal INT = (SELECT Id FROM dbo.Schools WHERE Name = N'مدرسة الأمل الابتدائية');
DECLARE @Noor INT = (SELECT Id FROM dbo.Schools WHERE Name = N'ثانوية النور للبنين');
DECLARE @Rafidain INT = (SELECT Id FROM dbo.Schools WHERE Name = N'مدرسة الرافدين الأهلية');

INSERT INTO dbo.Students (SchoolId, FullName, StudentNumber, Pincode, TotalCost, PaidCost, RemainCost, DebtCost, DiscountCost) VALUES
(@Amal, N'أحمد علي كاظم', N'A-1001', N'1001', 150000, 50000, 90000, 25000, 10000),
(@Amal, N'فاطمة محمد حسن', N'A-1002', N'1002', 150000, 150000, 0, 0, 0),
(@Amal, N'يوسف سالم جاسم', N'A-2001', N'2001', 160000, 40000, 110000, 40000, 10000),
(@Amal, N'زينب عبد الحسين', N'A-3001', N'3001', 170000, 70000, 90000, 15000, 10000),
(@Noor, N'حسين كريم عباس', N'N-4001', N'4001', 350000, 100000, 230000, 80000, 20000),
(@Noor, N'مصطفى نزار مهدي', N'N-4002', N'4002', 350000, 350000, 0, 0, 0),
(@Noor, N'علي جبار محمد', N'N-5001', N'5001', 375000, 125000, 230000, 120000, 20000),
(@Noor, N'حسن وليد سعيد', N'N-6001', N'6001', 400000, 150000, 230000, 50000, 20000),
(@Rafidain, N'مريم سامي رشيد', N'R-1101', N'1101', 500000, 200000, 270000, 30000, 30000),
(@Rafidain, N'نور الهدى قاسم', N'R-2101', N'2101', 650000, 250000, 360000, 60000, 40000);

INSERT INTO dbo.PaymentFees (SchoolId, PaymentType, Amount) VALUES
(@Amal, N'اقساط عام حالي', 150000),
(@Noor, N'اقساط عام حالي', 350000),
(@Rafidain, N'اقساط عام حالي', 500000);
GO

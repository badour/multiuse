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

CREATE TABLE dbo.Stages
(
    Id INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    SchoolId INT NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    SortOrder INT NOT NULL CONSTRAINT DF_Stages_SortOrder DEFAULT (0),
    CONSTRAINT FK_Stages_Schools FOREIGN KEY (SchoolId) REFERENCES dbo.Schools (Id)
);

CREATE TABLE dbo.Students
(
    Id INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    SchoolId INT NOT NULL,
    StageId INT NOT NULL,
    FullName NVARCHAR(200) NOT NULL,
    StudentNumber NVARCHAR(50) NULL,
    OutstandingDebt DECIMAL(18, 2) NOT NULL CONSTRAINT DF_Students_OutstandingDebt DEFAULT (0),
    CONSTRAINT FK_Students_Schools FOREIGN KEY (SchoolId) REFERENCES dbo.Schools (Id),
    CONSTRAINT FK_Students_Stages FOREIGN KEY (StageId) REFERENCES dbo.Stages (Id)
);

CREATE TABLE dbo.PaymentFees
(
    Id INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    SchoolId INT NOT NULL,
    StageId INT NOT NULL,
    PaymentType NVARCHAR(50) NOT NULL,
    Amount DECIMAL(18, 2) NOT NULL,
    CONSTRAINT FK_PaymentFees_Schools FOREIGN KEY (SchoolId) REFERENCES dbo.Schools (Id),
    CONSTRAINT FK_PaymentFees_Stages FOREIGN KEY (StageId) REFERENCES dbo.Stages (Id),
    CONSTRAINT UQ_PaymentFees UNIQUE (SchoolId, StageId, PaymentType)
);

CREATE TABLE dbo.Payments
(
    Id INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    OrderId CHAR(32) NOT NULL,
    SchoolId INT NOT NULL,
    StageId INT NOT NULL,
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
    CONSTRAINT FK_Payments_Stages FOREIGN KEY (StageId) REFERENCES dbo.Stages (Id),
    CONSTRAINT FK_Payments_Students FOREIGN KEY (StudentId) REFERENCES dbo.Students (Id)
);

CREATE INDEX IX_Stages_SchoolId ON dbo.Stages (SchoolId);
CREATE INDEX IX_Students_School_Stage ON dbo.Students (SchoolId, StageId);
CREATE INDEX IX_Payments_StudentId ON dbo.Payments (StudentId);
GO

INSERT INTO dbo.Schools (Name) VALUES
(N'مدرسة الأمل الابتدائية'),
(N'ثانوية النور للبنين'),
(N'مدرسة الرافدين الأهلية');

DECLARE @Amal INT = (SELECT Id FROM dbo.Schools WHERE Name = N'مدرسة الأمل الابتدائية');
DECLARE @Noor INT = (SELECT Id FROM dbo.Schools WHERE Name = N'ثانوية النور للبنين');
DECLARE @Rafidain INT = (SELECT Id FROM dbo.Schools WHERE Name = N'مدرسة الرافدين الأهلية');

INSERT INTO dbo.Stages (SchoolId, Name, SortOrder) VALUES
(@Amal, N'الصف الأول', 1),
(@Amal, N'الصف الثاني', 2),
(@Amal, N'الصف الثالث', 3),
(@Noor, N'الرابع العلمي', 1),
(@Noor, N'الخامس العلمي', 2),
(@Noor, N'السادس الأدبي', 3),
(@Rafidain, N'المرحلة المتوسطة', 1),
(@Rafidain, N'المرحلة الإعدادية', 2);

DECLARE @Amal1 INT = (SELECT Id FROM dbo.Stages WHERE SchoolId = @Amal AND Name = N'الصف الأول');
DECLARE @Amal2 INT = (SELECT Id FROM dbo.Stages WHERE SchoolId = @Amal AND Name = N'الصف الثاني');
DECLARE @Amal3 INT = (SELECT Id FROM dbo.Stages WHERE SchoolId = @Amal AND Name = N'الصف الثالث');
DECLARE @Noor4 INT = (SELECT Id FROM dbo.Stages WHERE SchoolId = @Noor AND Name = N'الرابع العلمي');
DECLARE @Noor5 INT = (SELECT Id FROM dbo.Stages WHERE SchoolId = @Noor AND Name = N'الخامس العلمي');
DECLARE @Noor6 INT = (SELECT Id FROM dbo.Stages WHERE SchoolId = @Noor AND Name = N'السادس الأدبي');
DECLARE @RafMid INT = (SELECT Id FROM dbo.Stages WHERE SchoolId = @Rafidain AND Name = N'المرحلة المتوسطة');
DECLARE @RafPrep INT = (SELECT Id FROM dbo.Stages WHERE SchoolId = @Rafidain AND Name = N'المرحلة الإعدادية');

INSERT INTO dbo.Students (SchoolId, StageId, FullName, StudentNumber, OutstandingDebt) VALUES
(@Amal, @Amal1, N'أحمد علي كاظم', N'A-1001', 25000),
(@Amal, @Amal1, N'فاطمة محمد حسن', N'A-1002', 0),
(@Amal, @Amal2, N'يوسف سالم جاسم', N'A-2001', 40000),
(@Amal, @Amal3, N'زينب عبد الحسين', N'A-3001', 15000),
(@Noor, @Noor4, N'حسين كريم عباس', N'N-4001', 80000),
(@Noor, @Noor4, N'مصطفى نزار مهدي', N'N-4002', 0),
(@Noor, @Noor5, N'علي جبار محمد', N'N-5001', 120000),
(@Noor, @Noor6, N'حسن وليد سعيد', N'N-6001', 50000),
(@Rafidain, @RafMid, N'مريم سامي رشيد', N'R-1101', 30000),
(@Rafidain, @RafPrep, N'نور الهدى قاسم', N'R-2101', 60000);

INSERT INTO dbo.PaymentFees (SchoolId, StageId, PaymentType, Amount) VALUES
(@Amal, @Amal1, N'اقساط عام حالي', 150000),
(@Amal, @Amal2, N'اقساط عام حالي', 160000),
(@Amal, @Amal3, N'اقساط عام حالي', 170000),
(@Noor, @Noor4, N'اقساط عام حالي', 350000),
(@Noor, @Noor5, N'اقساط عام حالي', 375000),
(@Noor, @Noor6, N'اقساط عام حالي', 400000),
(@Rafidain, @RafMid, N'اقساط عام حالي', 500000),
(@Rafidain, @RafPrep, N'اقساط عام حالي', 650000);
GO

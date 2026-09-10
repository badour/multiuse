/* Apply this if SchoolPaymentDb already exists from the previous script. */
USE SchoolPaymentDb;
GO

IF COL_LENGTH(N'dbo.Students', N'TotalCost') IS NULL
BEGIN
    ALTER TABLE dbo.Students ADD TotalCost DECIMAL(18, 2) NOT NULL CONSTRAINT DF_Students_TotalCost DEFAULT (0);
END

IF COL_LENGTH(N'dbo.Students', N'PaidCost') IS NULL
BEGIN
    ALTER TABLE dbo.Students ADD PaidCost DECIMAL(18, 2) NOT NULL CONSTRAINT DF_Students_PaidCost DEFAULT (0);
END

IF COL_LENGTH(N'dbo.Students', N'RemainCost') IS NULL
BEGIN
    ALTER TABLE dbo.Students ADD RemainCost DECIMAL(18, 2) NOT NULL CONSTRAINT DF_Students_RemainCost DEFAULT (0);
END

IF COL_LENGTH(N'dbo.Students', N'DebtCost') IS NULL
BEGIN
    ALTER TABLE dbo.Students ADD DebtCost DECIMAL(18, 2) NOT NULL CONSTRAINT DF_Students_DebtCost DEFAULT (0);
END

IF COL_LENGTH(N'dbo.Students', N'DiscountCost') IS NULL
BEGIN
    ALTER TABLE dbo.Students ADD DiscountCost DECIMAL(18, 2) NOT NULL CONSTRAINT DF_Students_DiscountCost DEFAULT (0);
END

IF COL_LENGTH(N'dbo.Students', N'OutstandingDebt') IS NOT NULL
BEGIN
    EXEC(N'UPDATE dbo.Students SET DebtCost = OutstandingDebt WHERE DebtCost = 0 AND OutstandingDebt <> 0');
END

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Students_School_Number' AND object_id = OBJECT_ID(N'dbo.Students')
)
BEGIN
    CREATE INDEX IX_Students_School_Number ON dbo.Students (SchoolId, StudentNumber);
END
GO

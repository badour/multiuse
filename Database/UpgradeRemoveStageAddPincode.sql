/* Apply this if SchoolPaymentDb already exists. Adds Students.Pincode and removes StageId / Stages. */
USE SchoolPaymentDb;
GO

IF COL_LENGTH(N'dbo.Students', N'Pincode') IS NULL
BEGIN
    ALTER TABLE dbo.Students ADD Pincode NVARCHAR(MAX) NULL;
END
GO

DECLARE @dropFk NVARCHAR(MAX) = N'';
SELECT @dropFk = @dropFk + N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id))
    + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id))
    + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';'
FROM sys.foreign_keys fk
WHERE fk.referenced_object_id = OBJECT_ID(N'dbo.Stages')
   OR EXISTS (
        SELECT 1
        FROM sys.foreign_key_columns fkc
        INNER JOIN sys.columns c ON c.object_id = fkc.parent_object_id AND c.column_id = fkc.parent_column_id
        WHERE fkc.constraint_object_id = fk.object_id
          AND c.name = N'StageId'
   );

IF @dropFk <> N''
BEGIN
    EXEC sp_executesql @dropFk;
END
GO

DECLARE @dropIx NVARCHAR(MAX) = N'';
SELECT @dropIx = @dropIx + N'DROP INDEX ' + QUOTENAME(i.name) + N' ON '
    + QUOTENAME(OBJECT_SCHEMA_NAME(i.object_id)) + N'.' + QUOTENAME(OBJECT_NAME(i.object_id)) + N';'
FROM sys.indexes i
WHERE i.is_primary_key = 0
  AND i.is_unique_constraint = 0
  AND EXISTS (
        SELECT 1
        FROM sys.index_columns ic
        INNER JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
        WHERE ic.object_id = i.object_id
          AND ic.index_id = i.index_id
          AND c.name = N'StageId'
   );

IF @dropIx <> N''
BEGIN
    EXEC sp_executesql @dropIx;
END
GO

IF OBJECT_ID(N'dbo.PaymentFees', N'U') IS NOT NULL
   AND EXISTS (
        SELECT 1 FROM sys.key_constraints
        WHERE name = N'UQ_PaymentFees' AND parent_object_id = OBJECT_ID(N'dbo.PaymentFees')
   )
BEGIN
    ALTER TABLE dbo.PaymentFees DROP CONSTRAINT UQ_PaymentFees;
END
GO

IF COL_LENGTH(N'dbo.Students', N'StageId') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Students DROP COLUMN StageId;
END

IF COL_LENGTH(N'dbo.Payments', N'StageId') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Payments DROP COLUMN StageId;
END

IF COL_LENGTH(N'dbo.PaymentFees', N'StageId') IS NOT NULL
BEGIN
    ALTER TABLE dbo.PaymentFees DROP COLUMN StageId;
END
GO

IF OBJECT_ID(N'dbo.PaymentFees', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.PaymentFees', N'StageId') IS NULL
BEGIN
    ;WITH ranked AS
    (
        SELECT Id,
               ROW_NUMBER() OVER (PARTITION BY SchoolId, PaymentType ORDER BY Id) AS rn
        FROM dbo.PaymentFees
    )
    DELETE FROM ranked WHERE rn > 1;
END
GO

IF OBJECT_ID(N'dbo.PaymentFees', N'U') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1 FROM sys.key_constraints
        WHERE name = N'UQ_PaymentFees' AND parent_object_id = OBJECT_ID(N'dbo.PaymentFees')
   )
   AND COL_LENGTH(N'dbo.PaymentFees', N'StageId') IS NULL
BEGIN
    ALTER TABLE dbo.PaymentFees ADD CONSTRAINT UQ_PaymentFees UNIQUE (SchoolId, PaymentType);
END
GO

IF OBJECT_ID(N'dbo.Stages', N'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Stages;
END
GO

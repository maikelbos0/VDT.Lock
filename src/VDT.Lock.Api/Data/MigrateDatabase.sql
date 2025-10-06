IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251004101908_Add_DataStore'
)
BEGIN
    CREATE TABLE [DataStores] (
        [Id] uniqueidentifier NOT NULL,
        [Password] nvarchar(max) NULL,
        [Data] varbinary(max) NULL,
        CONSTRAINT [PK_DataStores] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251004101908_Add_DataStore'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251004101908_Add_DataStore', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251006152339_Switch_DataStore_Password_To_Split_Secret'
)
BEGIN
    DECLARE @var sysname;
    SELECT @var = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DataStores]') AND [c].[name] = N'Password');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [DataStores] DROP CONSTRAINT [' + @var + '];');
    ALTER TABLE [DataStores] DROP COLUMN [Password];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251006152339_Switch_DataStore_Password_To_Split_Secret'
)
BEGIN
    ALTER TABLE [DataStores] ADD [SecretHash] varbinary(max) NOT NULL DEFAULT 0x;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251006152339_Switch_DataStore_Password_To_Split_Secret'
)
BEGIN
    ALTER TABLE [DataStores] ADD [SecretSalt] varbinary(max) NOT NULL DEFAULT 0x;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251006152339_Switch_DataStore_Password_To_Split_Secret'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251006152339_Switch_DataStore_Password_To_Split_Secret', N'9.0.9');
END;

COMMIT;
GO


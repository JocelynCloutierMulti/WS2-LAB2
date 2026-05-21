USE FR-WS2-BASELAB
GO
IF OBJECT_ID(N'[dbo].[CategoryImages]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CategoryImages] (
        [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_CategoryImages] PRIMARY KEY,
        [CategoryId] int NOT NULL,
        [FileName] nvarchar(160) NOT NULL,
        [OriginalFileName] nvarchar(255) NOT NULL,
        [ContentType] nvarchar(100) NOT NULL,
        [SizeInBytes] bigint NOT NULL,
        [AltText] nvarchar(120) NULL,
        [UploadedAtUtc] datetime2 NOT NULL CONSTRAINT [DF_CategoryImages_UploadedAtUtc] DEFAULT SYSUTCDATETIME(),
        CONSTRAINT [FK_CategoryImages_Categories_CategoryId]
            FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[Categories]([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_CategoryImages_CategoryId]
        ON [dbo].[CategoryImages]([CategoryId]);
END

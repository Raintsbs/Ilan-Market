using AdvertisementApp.DataAccess.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdvertisementApp.DataAccess.Migrations
{
    [DbContext(typeof(AdvertisementAppDbContext))]
    [Migration("20260616150000_RemoveRealEstateProjects")]
    public partial class RemoveRealEstateProjects : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DECLARE @fk sysname;
                SELECT @fk = fk.name
                FROM sys.foreign_keys fk
                INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
                INNER JOIN sys.columns c ON fkc.parent_column_id = c.column_id AND fkc.parent_object_id = c.object_id
                WHERE fk.parent_object_id = OBJECT_ID(N'Advertisements') AND c.name = N'ProjectId';

                IF @fk IS NOT NULL
                BEGIN
                    DECLARE @sql NVARCHAR(MAX) = N'ALTER TABLE Advertisements DROP CONSTRAINT ' + QUOTENAME(@fk);
                    EXEC sp_executesql @sql;
                END

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Advertisements') AND name = 'ProjectId')
                    ALTER TABLE Advertisements DROP COLUMN ProjectId;

                IF OBJECT_ID(N'RealEstateProjects', N'U') IS NOT NULL
                    DROP TABLE RealEstateProjects;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'RealEstateProjects', N'U') IS NULL
                BEGIN
                    CREATE TABLE RealEstateProjects (
                        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        UserId INT NOT NULL,
                        Title NVARCHAR(256) NOT NULL,
                        Description NVARCHAR(MAX) NOT NULL,
                        City NVARCHAR(128) NULL,
                        District NVARCHAR(128) NULL,
                        DeveloperName NVARCHAR(256) NULL,
                        CoverImagePath NVARCHAR(512) NULL,
                        DeliveryDate DATETIME2 NULL,
                        UnitCount INT NOT NULL CONSTRAINT DF_RealEstateProjects_UnitCount DEFAULT 0,
                        IsActive BIT NOT NULL CONSTRAINT DF_RealEstateProjects_IsActive DEFAULT 1,
                        CreatedTime DATETIME2 NOT NULL,
                        UpdatedTime DATETIME2 NULL
                    );
                    CREATE INDEX IX_RealEstateProjects_City_IsActive ON RealEstateProjects(City, IsActive);
                END

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Advertisements') AND name = 'ProjectId')
                    ALTER TABLE Advertisements ADD ProjectId INT NULL;

                IF NOT EXISTS (
                    SELECT 1 FROM sys.foreign_keys
                    WHERE parent_object_id = OBJECT_ID(N'Advertisements')
                      AND referenced_object_id = OBJECT_ID(N'RealEstateProjects'))
                BEGIN
                    ALTER TABLE Advertisements
                    ADD CONSTRAINT FK_Advertisements_RealEstateProjects_ProjectId
                    FOREIGN KEY (ProjectId) REFERENCES RealEstateProjects(Id) ON DELETE SET NULL;
                END
                """);
        }
    }
}

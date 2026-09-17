using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaleFashion.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class GetShopProductsSP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = """
CREATE OR ALTER PROCEDURE [dbo].[GetShopProducts]
    @PageIndex       INT = 1,
    @PageSize        INT = 12,
    @OrderBy         NVARCHAR(50) = 'Newest',
    @SearchText      NVARCHAR(250) = NULL,
    @CategoryId      UNIQUEIDENTIFIER = NULL,
    @BrandId         UNIQUEIDENTIFIER = NULL,
    @PriceFrom       DECIMAL(18,2) = NULL,
    @PriceTo         DECIMAL(18,2) = NULL,
    @Size            NVARCHAR(50) = NULL,
    @Color           NVARCHAR(50) = NULL,
    @TagName         NVARCHAR(100) = NULL,
    @Total           INT OUTPUT,
    @TotalDisplay    INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. Whitelist sorting expression to prevent SQL Injection
    DECLARE @SortExpression NVARCHAR(100);
    SET @SortExpression = CASE LOWER(LTRIM(RTRIM(@OrderBy)))
        WHEN 'priceasc'     THEN 'EffectivePrice ASC'
        WHEN 'pricedesc'    THEN 'EffectivePrice DESC'
        WHEN 'rating'       THEN 'p.Rating DESC'
        WHEN 'nameasc'      THEN 'p.Name ASC'
        WHEN 'namedesc'     THEN 'p.Name DESC'
        WHEN 'newest'       THEN 'p.CreatedAt DESC'
        ELSE 'p.CreatedAt DESC'
    END;

    -- 2. Clean empty or whitespace search string
    IF LTRIM(RTRIM(@SearchText)) = '' SET @SearchText = NULL;
    IF LTRIM(RTRIM(@Size)) = '' SET @Size = NULL;
    IF LTRIM(RTRIM(@Color)) = '' SET @Color = NULL;
    IF LTRIM(RTRIM(@TagName)) = '' SET @TagName = NULL;

    -- 3. Total active products count
    SELECT @Total = COUNT(1) 
    FROM [dbo].[Products] 
    WHERE [IsActive] = 1;

    -- 4. Build dynamic WHERE clause
    DECLARE @whereClause NVARCHAR(MAX) = N' WHERE p.IsActive = 1 ';

    IF @SearchText IS NOT NULL
        SET @whereClause += N' AND (p.Name LIKE ''%'' + @xSearchText + ''%'' OR p.Sku LIKE ''%'' + @xSearchText + ''%'' OR p.Description LIKE ''%'' + @xSearchText + ''%'') ';

    IF @CategoryId IS NOT NULL
        SET @whereClause += N' AND p.CategoryId = @xCategoryId ';

    IF @BrandId IS NOT NULL
        SET @whereClause += N' AND p.BrandId = @xBrandId ';

    IF @PriceFrom IS NOT NULL
        SET @whereClause += N' AND COALESCE(p.SalePrice, p.BasePrice) >= @xPriceFrom ';

    IF @PriceTo IS NOT NULL
        SET @whereClause += N' AND COALESCE(p.SalePrice, p.BasePrice) <= @xPriceTo ';

    IF @Size IS NOT NULL
        SET @whereClause += N' AND EXISTS (SELECT 1 FROM [dbo].[ProductVariants] pv WHERE pv.ProductId = p.Id AND pv.Size = @xSize AND pv.StockQuantity > 0) ';

    IF @Color IS NOT NULL
        SET @whereClause += N' AND EXISTS (SELECT 1 FROM [dbo].[ProductVariants] pv WHERE pv.ProductId = p.Id AND (pv.ColorName = @xColor OR pv.ColorHex = @xColor) AND pv.StockQuantity > 0) ';

    IF @TagName IS NOT NULL
        SET @whereClause += N' AND EXISTS (SELECT 1 FROM [dbo].[ProductTags] pt WHERE pt.ProductId = p.Id AND pt.Name = @xTagName) ';

    -- 5. Calculate TotalDisplay count
    DECLARE @countSql NVARCHAR(MAX);
    DECLARE @countParamList NVARCHAR(MAX);

    SET @countSql = N'SELECT @xTotalDisplay = COUNT(1) FROM [dbo].[Products] p ' + @whereClause;

    SET @countParamList = N'@xSearchText NVARCHAR(250),
                           @xCategoryId UNIQUEIDENTIFIER,
                           @xBrandId UNIQUEIDENTIFIER,
                           @xPriceFrom DECIMAL(18,2),
                           @xPriceTo DECIMAL(18,2),
                           @xSize NVARCHAR(50),
                           @xColor NVARCHAR(50),
                           @xTagName NVARCHAR(100),
                           @xTotalDisplay INT OUTPUT';

    EXEC sp_executesql @countSql, @countParamList,
        @SearchText,
        @CategoryId,
        @BrandId,
        @PriceFrom,
        @PriceTo,
        @Size,
        @Color,
        @TagName,
        @xTotalDisplay = @TotalDisplay OUTPUT;

    -- 6. Main Query with Paging & Column Mapping for SqlUtility
    DECLARE @sql NVARCHAR(MAX);
    DECLARE @paramList NVARCHAR(MAX);

    SET @sql = N'
    WITH ProductCTE AS (
        SELECT 
            p.Id,
            p.Name,
            p.Sku,
            p.BasePrice,
            p.SalePrice,
            COALESCE(p.SalePrice, p.BasePrice) AS EffectivePrice,
            p.Rating,
            p.IsNewArrival,
            p.IsHotSale,
            p.CreatedAt,
            c.Name AS CategoryName,
            b.Name AS BrandName,
            COALESCE((
                SELECT TOP 1 pi.ImageUrl 
                FROM [dbo].[ProductImages] pi 
                WHERE pi.ProductId = p.Id 
                ORDER BY pi.IsPrimary DESC, pi.DisplayOrder ASC
            ), ''/malefashion/img/product/product-1.jpg'') AS ImageUrl,
            (
                SELECT STRING_AGG(pv.ColorHex, '','') 
                FROM (
                    SELECT DISTINCT pvInner.ColorHex 
                    FROM [dbo].[ProductVariants] pvInner 
                    WHERE pvInner.ProductId = p.Id AND pvInner.ColorHex IS NOT NULL AND pvInner.ColorHex <> ''''
                ) pv
            ) AS AvailableColorsHex
        FROM [dbo].[Products] p
        LEFT JOIN [dbo].[Categories] c ON p.CategoryId = c.Id
        LEFT JOIN [dbo].[Brands] b ON p.BrandId = b.Id
        ' + @whereClause + N'
    )
    SELECT 
        Id,
        Name,
        Sku,
        BasePrice,
        SalePrice,
        EffectivePrice,
        Rating,
        IsNewArrival,
        IsHotSale,
        CreatedAt,
        CategoryName,
        BrandName,
        ImageUrl,
        AvailableColorsHex
    FROM ProductCTE p
    ORDER BY ' + @SortExpression + N'
    OFFSET @xPageSize * (@xPageIndex - 1) ROWS
    FETCH NEXT @xPageSize ROWS ONLY;
    ';

    SET @paramList = N'@xSearchText NVARCHAR(250),
                       @xCategoryId UNIQUEIDENTIFIER,
                       @xBrandId UNIQUEIDENTIFIER,
                       @xPriceFrom DECIMAL(18,2),
                       @xPriceTo DECIMAL(18,2),
                       @xSize NVARCHAR(50),
                       @xColor NVARCHAR(50),
                       @xTagName NVARCHAR(100),
                       @xPageIndex INT,
                       @xPageSize INT';

    EXEC sp_executesql @sql, @paramList,
        @SearchText,
        @CategoryId,
        @BrandId,
        @PriceFrom,
        @PriceTo,
        @Size,
        @Color,
        @TagName,
        @PageIndex,
        @PageSize;
END;
""";

            migrationBuilder.Sql(sql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = """
                    DROP PROCEDURE IF EXISTS [dbo].[GetShopProducts];
                    """;
            migrationBuilder.Sql(sql);
        }
    }
}


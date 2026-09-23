using Microsoft.Data.SqlClient;
using NASA_APOD_Gallery.Models;

namespace NASA_APOD_Gallery.Repositories
{
    public class ApodRepository
    {
        private readonly string _connectionString;

        public ApodRepository(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "DefaultConnection was not found.");

            // Ensure the APOD table exists on first use.
            EnsureTableExistsAsync().GetAwaiter().GetResult();
        }

        private async Task EnsureTableExistsAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = @"
IF NOT EXISTS (
    SELECT 1 FROM sys.tables WHERE name = 'APOD' AND schema_id = SCHEMA_ID('dbo')
)
BEGIN
    CREATE TABLE dbo.APOD (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Date] DATE NOT NULL CONSTRAINT UQ_APOD_Date UNIQUE,
        [Title] NVARCHAR(500) NOT NULL,
        [Explanation] NVARCHAR(MAX) NULL,
        [Url] NVARCHAR(2048) NOT NULL,
        [MediaType] NVARCHAR(50) NULL,
        [ServiceVersion] NVARCHAR(50) NULL,
        [SavedAt] DATETIME2 NOT NULL DEFAULT GETDATE()
    );
END
ELSE
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes 
        WHERE name = 'UQ_APOD_Date' AND object_id = OBJECT_ID('dbo.APOD')
    ) AND NOT EXISTS (
        SELECT 1 FROM sys.key_constraints 
        WHERE parent_object_id = OBJECT_ID('dbo.APOD') AND type = 'PK' 
        AND EXISTS (
            SELECT 1 FROM sys.index_columns ic 
            JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id 
            WHERE ic.object_id = OBJECT_ID('dbo.APOD') AND c.name = 'Date'
        )
    )
    BEGIN
        CREATE UNIQUE INDEX UQ_APOD_Date ON dbo.APOD([Date]);
    END
END
";

            using var command = new SqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync();
        }

        public async Task SaveApodAsync(ApodDto apod)
        {
            using var connection =
                new SqlConnection(_connectionString);

            await connection.OpenAsync();

            const string sql = @"
                IF NOT EXISTS (
                    SELECT 1
                    FROM dbo.APOD
                    WHERE Date = @Date
                )
                BEGIN
                    INSERT INTO dbo.APOD
                    (
                        Date,
                        Title,
                        Explanation,
                        Url,
                        MediaType,
                        ServiceVersion
                    )
                    VALUES
                    (
                        @Date,
                        @Title,
                        @Explanation,
                        @Url,
                        @MediaType,
                        @ServiceVersion
                    )
                END";

            using var command =
                new SqlCommand(sql, connection);

            // API returns date as a string (yyyy-MM-dd). Store as DATE.
            if (!DateTime.TryParse(apod.Date, out var parsedDate))
            {
                parsedDate = DateTime.UtcNow.Date;
            }

            command.Parameters.Add("@Date", System.Data.SqlDbType.Date).Value = parsedDate.Date;

            command.Parameters.AddWithValue(
                "@Title",
                (object?)apod.Title ?? DBNull.Value);

            command.Parameters.AddWithValue(
                "@Explanation",
                (object?)apod.Explanation ?? DBNull.Value);

            // Prefer hdurl when available; fall back to url.
            var urlValue = !string.IsNullOrWhiteSpace(apod.HdUrl) ? apod.HdUrl : apod.Url;

            command.Parameters.AddWithValue(
                "@Url",
                (object?)urlValue ?? DBNull.Value);

            command.Parameters.AddWithValue(
                "@MediaType",
                (object?)apod.MediaType ?? DBNull.Value);

            command.Parameters.AddWithValue(
                "@ServiceVersion",
                (object?)apod.ServiceVersion ?? DBNull.Value);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<List<ApodDto>> GetAllApodAsync()
        {
            var apodList = new List<ApodDto>();

            using var connection =
                new SqlConnection(_connectionString);

            await connection.OpenAsync();

            const string sql = @"
                SELECT
                    Date,
                    Title,
                    Explanation,
                    Url,
                    MediaType,
                    ServiceVersion
                FROM APOD
                ORDER BY Date ASC";

            using var command =
                new SqlCommand(sql, connection);

            using var reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var apod = new ApodDto
                {
                    Date = reader.GetDateTime(
                        reader.GetOrdinal("Date"))
                        .ToString("yyyy-MM-dd"),

                    Title = reader.GetString(
                        reader.GetOrdinal("Title")),

                    Explanation = reader.IsDBNull(
                        reader.GetOrdinal("Explanation"))
                        ? null
                        : reader.GetString(
                            reader.GetOrdinal("Explanation")),

                    Url = reader.GetString(
                        reader.GetOrdinal("Url")) + "",

                    MediaType = reader.IsDBNull(
                        reader.GetOrdinal("MediaType"))
                        ? null
                        : reader.GetString(
                            reader.GetOrdinal("MediaType")),

                    ServiceVersion = reader.IsDBNull(
                        reader.GetOrdinal("ServiceVersion"))
                        ? null
                        : reader.GetString(
                            reader.GetOrdinal("ServiceVersion"))
                };

                apodList.Add(apod);
            }

            return apodList;
        }
    }
}
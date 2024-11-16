using System.Drawing;
using APIMAC;
using Npgsql;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;

// Include the Npgsql namespace for PostgreSQL

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();


// Always enable Swagger, regardless of the environment.
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");

app.UseHttpsRedirection();
var connectionString =
    "Host=localhost;Port=5432;Database=music_database;Username=postgres;Password=strategikon;Trust Server Certificate=True;";

// New route: Query total sales by month, with optional start and end dates.
app.MapGet("/totalsales", async (DateTime? startDate, DateTime? endDate) =>
{
    // Hardcoded connection string
    var totalSales = new List<SalesData>();

    // ADO.NET connection and query execution
    using (var connection = new NpgsqlConnection(connectionString))
    {
        await connection.OpenAsync(); // Open connection asynchronously

        // Base SQL query with optional date filtering
        var queryBuilder = @"
            SELECT 
                DATE_TRUNC('month', i.Invoice_Date) AS Month,
                SUM(il.unit_price * il.Quantity) AS TotalSales,
                SUM(il.Quantity) AS TotalQuantity";


        queryBuilder += @"
            FROM 
                Invoice i
            JOIN 
                Invoice_Line il ON i.Invoice_Id = il.Invoice_Id
            JOIN 
                Track t ON il.Track_Id = t.Track_Id
            WHERE 
                1=1"; // This allows for easier conditional appending of WHERE clauses

        // If a start date is provided, append the corresponding condition
        if (startDate.HasValue)
            queryBuilder +=
                $" AND i.Invoice_Date >= '{startDate.Value:yyyy-MM-dd} 00:00:00'"; // Directly interpolating date

        // If an end date is provided, append the corresponding condition
        if (endDate.HasValue)
            queryBuilder +=
                $" AND i.Invoice_Date <= '{endDate.Value:yyyy-MM-dd} 23:59:59'"; // Directly interpolating date

        // Finish the SQL query
        queryBuilder += @"
            GROUP BY 
                DATE_TRUNC('month', i.Invoice_Date)
            ORDER BY 
                Month ASC;";

        using (var command = new NpgsqlCommand(queryBuilder, connection)) // Use NpgsqlCommand
        {
            using (var reader = await command.ExecuteReaderAsync()) // Use NpgsqlDataReader
            {
                while (await reader.ReadAsync()) // Read asynchronously
                    totalSales.Add(new SalesData
                    {
                        Month = reader.GetDateTime(0),
                        // Conditionally read TotalSales or TotalQuantity
                        TotalSales = reader.GetDouble(1),
                        NumberOfSales = reader.GetDouble(2)
                    });
            }
        }
    }

    return totalSales;
});


app.MapGet("/genresales/{genre}", async (string genre, DateTime? startDate, DateTime? endDate) =>
{
    var genreSales = new List<GenreSalesData>();

    // ADO.NET connection and query execution
    using (var connection = new NpgsqlConnection(connectionString))
    {
        await connection.OpenAsync(); // Open connection asynchronously

        // Base SQL query with optional date filtering
        var queryBuilder = @"
            SELECT 
                g.Name AS Genre,
                DATE_TRUNC('month', i.Invoice_Date) AS Month,
                SUM(il.unit_price * il.Quantity) AS TotalSales,
                COUNT(i.Invoice_Id) AS NumberOfSales
            FROM 
                Invoice i
            JOIN 
                Invoice_Line il ON i.Invoice_Id = il.Invoice_Id
            JOIN 
                Track t ON il.Track_Id = t.Track_Id
            JOIN 
                Genre g ON t.Genre_Id = g.Genre_Id
            WHERE 
                UPPER(g.Name) = UPPER(@genre)"; // Using parameterized query to prevent SQL injection

        // If a start date is provided, append the corresponding condition
        if (startDate.HasValue)
            queryBuilder +=
                $" AND i.Invoice_Date >= '{startDate.Value:yyyy-MM-dd} 00:00:00'"; // Directly interpolating date

        // If an end date is provided, append the corresponding condition
        if (endDate.HasValue)
            queryBuilder +=
                $" AND i.Invoice_Date <= '{endDate.Value:yyyy-MM-dd} 23:59:59'"; // Directly interpolating date

        queryBuilder += @"
            GROUP BY 
                g.Name, DATE_TRUNC('month', i.Invoice_Date)
            ORDER BY 
                Month ASC;";

        using (var command = new NpgsqlCommand(queryBuilder, connection)) // Use NpgsqlCommand
        {
            // Add the genre parameter
            command.Parameters.AddWithValue("genre", genre); // Parameterized query

            using (var reader = await command.ExecuteReaderAsync()) // Use NpgsqlDataReader
            {
                while (await reader.ReadAsync()) // Read asynchronously
                    genreSales.Add(new GenreSalesData
                    {
                        Genre = reader.GetString(0),
                        Month = reader.GetDateTime(1),
                        TotalSales = reader.GetDouble(2),
                        NumberOfSales = reader.GetInt32(3)
                    });
            }
        }
    }

    return genreSales;
});

app.MapGet("/countrySales/{country}", async (string country, DateTime? startDate, DateTime? endDate) =>
    {
        var countrySales = new List<CountrySalesData>();

        // ADO.NET connection and query execution
        using (var connection = new NpgsqlConnection(connectionString))
        {
            await connection.OpenAsync(); // Open connection asynchronously

            // Base SQL query with optional date filtering
            var queryBuilder = @"
            SELECT 
                DATE_TRUNC('month', i.invoice_date) AS month,
                c.country AS customer_country,
                SUM(il.unit_price * il.quantity) AS total_sales,
                COUNT(i.invoice_id) AS number_of_sales
            FROM 
                invoice i
            JOIN 
                customer c ON i.customer_id = c.customer_id
            JOIN 
                invoice_line il ON i.invoice_id = il.invoice_id
            WHERE 
                UPPER(c.country) = UPPER(@country)"; // Using parameterized query to prevent SQL injection

            // If a start date is provided, append the corresponding condition
            if (startDate.HasValue)
                queryBuilder +=
                    $" AND i.invoice_date >= '{startDate.Value:yyyy-MM-dd} 00:00:00'"; // Directly interpolating date

            // If an end date is provided, append the corresponding condition
            if (endDate.HasValue)
                queryBuilder +=
                    $" AND i.invoice_date <= '{endDate.Value:yyyy-MM-dd} 23:59:59'"; // Directly interpolating date

            queryBuilder += @"
            GROUP BY 
                month, c.country
            ORDER BY 
                month ASC;"; // Ordering by month for chronological results

            using (var command = new NpgsqlCommand(queryBuilder, connection)) // Use NpgsqlCommand
            {
                // Add the country parameter
                command.Parameters.AddWithValue("country", country); // Parameterized query

                using (var reader = await command.ExecuteReaderAsync()) // Use NpgsqlDataReader
                {
                    while (await reader.ReadAsync()) // Read asynchronously
                        countrySales.Add(new CountrySalesData
                        {
                            Month = reader.GetDateTime(0),
                            CustomerCountry = reader.GetString(1),
                            TotalSales = reader.GetDouble(2),
                            NumberOfSales = reader.GetInt32(3)
                        });
                }
            }
        }

        return countrySales;
    })
    .WithName("GetCountrySalesPerMonth");

app.MapGet("/countryGenreSales/{country}/{genre}",
        async (string country, string genre, DateTime? startDate, DateTime? endDate) =>
        {
            var genreSales = new List<CountrySalesData>();

            // ADO.NET connection and query execution
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync(); // Open connection asynchronously

                // Base SQL query with optional date filtering
                var queryBuilder = @"
                SELECT 
                    c.country AS customer_country,
                    g.Name AS genre,
                    DATE_TRUNC('month', i.invoice_date) AS month,
                    SUM(il.unit_price * il.quantity) AS total_sales,
                    COUNT(i.invoice_id) AS number_of_sales
                FROM 
                    invoice i
                JOIN 
                    customer c ON i.customer_id = c.customer_id
                JOIN 
                    invoice_line il ON i.invoice_id = il.invoice_id
                JOIN 
                    Track t ON il.Track_Id = t.Track_Id
                JOIN 
                    Genre g ON t.Genre_Id = g.Genre_Id
                WHERE 
                    UPPER(c.country) = UPPER(@country) AND
                    UPPER(g.Name) = UPPER(@genre)"; // Using parameterized query to prevent SQL injection

                // If a start date is provided, append the corresponding condition
                if (startDate.HasValue)
                    queryBuilder +=
                        $" AND i.invoice_date >= '{startDate.Value:yyyy-MM-dd} 00:00:00'"; // Directly interpolating date

                // If an end date is provided, append the corresponding condition
                if (endDate.HasValue)
                    queryBuilder +=
                        $" AND i.invoice_date <= '{endDate.Value:yyyy-MM-dd} 23:59:59'"; // Directly interpolating date

                queryBuilder += @"
                GROUP BY 
                    c.country, g.Name, month
                ORDER BY 
                    3 asc;"; // Order by total sales in descending order

                using (var command = new NpgsqlCommand(queryBuilder, connection)) // Use NpgsqlCommand
                {
                    // Add the country and genre parameters
                    command.Parameters.AddWithValue("country", country); // Parameterized query
                    command.Parameters.AddWithValue("genre", genre); // Parameterized query

                    using (var reader = await command.ExecuteReaderAsync()) // Use NpgsqlDataReader
                    {
                        while (await reader.ReadAsync()) // Read asynchronously
                            genreSales.Add(new CountrySalesData
                            {
                                CustomerCountry = reader.GetString(0),
                                Genre = reader.GetString(1),
                                Month = reader.GetDateTime(2),
                                TotalSales = reader.GetDouble(3),
                                NumberOfSales = reader.GetInt32(4)
                            });
                    }
                }
            }

            return genreSales;
        })
    .WithName("GetCountryGenreSalesPerMonth");

app.MapGet("/totalsales/export",
    async (DateTime? startDate, DateTime? endDate, int forecastMonths, TrendEstimationFunction trendFunction) =>
    {
        var totalSales = new List<SalesData>();

        // ADO.NET connection and query execution
        using (var connection = new NpgsqlConnection(connectionString))
        {
            await connection.OpenAsync();

            var queryBuilder = @"
            SELECT 
                DATE_TRUNC('month', i.Invoice_Date) AS Month,
                SUM(il.unit_price * il.Quantity) AS TotalSales,
                SUM(il.Quantity) AS TotalQuantity";

            queryBuilder += @"
            FROM 
                Invoice i
            JOIN 
                Invoice_Line il ON i.Invoice_Id = il.Invoice_Id
            JOIN 
                Track t ON il.Track_Id = t.Track_Id
            WHERE 
                1=1";

            if (startDate.HasValue) queryBuilder += $" AND i.Invoice_Date >= '{startDate.Value:yyyy-MM-dd} 00:00:00'";

            if (endDate.HasValue) queryBuilder += $" AND i.Invoice_Date <= '{endDate.Value:yyyy-MM-dd} 23:59:59'";

            queryBuilder += @"
            GROUP BY 
                DATE_TRUNC('month', i.Invoice_Date)
            ORDER BY 
                Month ASC;";

            using (var command = new NpgsqlCommand(queryBuilder, connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                        totalSales.Add(new SalesData
                        {
                            Month = reader.GetDateTime(0),
                            TotalSales = reader.GetDouble(1),
                            NumberOfSales = reader.GetDouble(2)
                        });
                }
            }

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Total Sales");

                worksheet.Cells[1, 1].Value = "Month";
                worksheet.Cells[1, 2].Value = "Total Sales";
                worksheet.Cells[1, 3].Value = "Number of Sales";

                worksheet.Row(1).Style.Font.Bold = true;


                for (var i = 0; i < totalSales.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = totalSales[i].Month.ToString("yyyy-MM");
                    worksheet.Cells[i + 2, 2].Value = totalSales[i].TotalSales;
                    worksheet.Cells[i + 2, 3].Value = totalSales[i].NumberOfSales;
                }

                worksheet.ConditionalFormatting.AddDatabar(new ExcelAddress(2, 2, totalSales.Count + 1, 2), Color.Blue);
                // Add Data Bars for the Number of Sales column (column 3)
                worksheet.ConditionalFormatting.AddDatabar(new ExcelAddress(2, 3, totalSales.Count + 1, 3),
                    Color.Green);

                worksheet.Column(2).Style.Numberformat.Format = "$#,##0.00";

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                var chart = worksheet.Drawings.AddChart("TotalSalesChart", eChartType.Line);
                chart.Title.Text = "Total Sales with Forecast";
                chart.SetPosition(0, 0, 4, 0);
                chart.SetSize(800, 400);

                var dataSeries = chart.Series.Add(
                    worksheet.Cells[2, 2, totalSales.Count + forecastMonths + 1, 2], // Includes future rows
                    worksheet.Cells[2, 1, totalSales.Count + forecastMonths + 1, 1] // Includes future rows
                );
                dataSeries.Header = "Total Sales";

                eTrendLine trendlineType;
                switch (trendFunction)
                {
                    case TrendEstimationFunction.Linear:
                        trendlineType = eTrendLine.Linear;
                        break;

                    case TrendEstimationFunction.Polynomial:
                        trendlineType = eTrendLine.Polynomial;
                        break;

                    case TrendEstimationFunction.Logarithmic:
                        trendlineType = eTrendLine.Logarithmic;
                        break;

                    case TrendEstimationFunction.Exponential:
                        trendlineType = eTrendLine.Exponential;
                        break;

                    case TrendEstimationFunction.Power:
                        trendlineType = eTrendLine.Power;
                        break;

                    case TrendEstimationFunction.MovingAverage:
                        trendlineType = eTrendLine.MovingAverage;
                        break;

                    default:
                        trendlineType = eTrendLine.Linear;
                        break;
                }

                var trendline = dataSeries.TrendLines.Add(trendlineType);

                trendline.DisplayRSquaredValue = true;
                trendline.DisplayEquation = true;
                trendline.Forward = forecastMonths;

                var excelData = package.GetAsByteArray();

                return Results.File(
                    excelData,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "TotalSalesWithForecast.xlsx"
                );
            }
        }
    });

app.MapGet("/genresales/export", async (DateTime? startDate, DateTime? endDate) =>
{
    var genreSales = new List<GenreSalesData>();

    // ADO.NET connection and query execution
    using (var connection = new NpgsqlConnection(connectionString))
    {
        await connection.OpenAsync(); // Open connection asynchronously

        // Base SQL query with optional date filtering
        var queryBuilder = @"
            SELECT 
                g.Name AS Genre,
                DATE_TRUNC('month', i.Invoice_Date) AS Month,
                SUM(il.unit_price * il.Quantity) AS TotalSales,
                COUNT(i.Invoice_Id) AS NumberOfSales
            FROM 
                Invoice i
            JOIN 
                Invoice_Line il ON i.Invoice_Id = il.Invoice_Id
            JOIN 
                Track t ON il.Track_Id = t.Track_Id
            JOIN 
                Genre g ON t.Genre_Id = g.Genre_Id";


        if (startDate.HasValue)
            queryBuilder +=
                $" AND i.Invoice_Date >= '{startDate.Value:yyyy-MM-dd} 00:00:00'"; // Directly interpolating date

        // If an end date is provided, append the corresponding condition
        if (endDate.HasValue)
            queryBuilder +=
                $" AND i.Invoice_Date <= '{endDate.Value:yyyy-MM-dd} 23:59:59'"; // Directly interpolating date

        queryBuilder += @"
            GROUP BY 
                g.Name, DATE_TRUNC('month', i.Invoice_Date)
            ORDER BY 
                Month ASC;";

        using (var command = new NpgsqlCommand(queryBuilder, connection)) // Use NpgsqlCommand
        {
            using (var reader = await command.ExecuteReaderAsync()) // Use NpgsqlDataReader
            {
                while (await reader.ReadAsync()) // Read asynchronously
                    genreSales.Add(new GenreSalesData
                    {
                        Genre = reader.GetString(0),
                        Month = reader.GetDateTime(1),
                        TotalSales = reader.GetDouble(2),
                        NumberOfSales = reader.GetInt32(3)
                    });
            }
        }
    }

    using (var package = new ExcelPackage())
    {
        var worksheet = package.Workbook.Worksheets.Add("Total Sales");

        worksheet.Cells[1, 1].Value = "Genre";
        worksheet.Cells[1, 2].Value = "Total Sales";
        worksheet.Cells[1, 3].Value = "Number of Sales";
        worksheet.Cells[1, 4].Value = "Month";

        worksheet.Row(1).Style.Font.Bold = true;

        for (var i = 0; i < genreSales.Count; i++)
        {
            worksheet.Cells[i + 2, 1].Value = genreSales[i].Genre;
            worksheet.Cells[i + 2, 2].Value = genreSales[i].TotalSales;
            worksheet.Cells[i + 2, 3].Value = genreSales[i].NumberOfSales;
            worksheet.Cells[i + 2, 4].Value = genreSales[i].Month.ToString("yyyy-MM");
        }

        worksheet.ConditionalFormatting.AddDatabar(new ExcelAddress(2, 2, genreSales.Count + 1, 2), Color.Blue);
        worksheet.ConditionalFormatting.AddDatabar(new ExcelAddress(2, 3, genreSales.Count + 1, 3), Color.Green);

        worksheet.Column(2).Style.Numberformat.Format = "$#,##0.00";

        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        var aggregatedSales = genreSales
            .GroupBy(s => s.Genre)
            .ToDictionary(g => g.Key, g => g.Sum(s => s.TotalSales));

        var topGenres = aggregatedSales
            .OrderByDescending(kvp => kvp.Value)
            .Take(3)
            .ToList();

        var pieChart = worksheet.Drawings.AddChart("GenreSalesPieChart", eChartType.Pie);
        pieChart.Title.Text = "Total Sales by Genre";

        var dataStartColumn = 6;
        var aggregatedRowIndex = 2;

        foreach (var (genre, totalSales) in aggregatedSales)
        {
            worksheet.Cells[aggregatedRowIndex, dataStartColumn].Value = genre;
            worksheet.Cells[aggregatedRowIndex, dataStartColumn + 1].Value = totalSales;
            if (topGenres.Any(tg => tg.Key == genre))
            {
                int rank = topGenres.FindIndex(tg => tg.Key == genre) + 1;
                string icon = rank switch
                {
                    1 => "🥇",
                    2 => "🥈",
                    3 => "🥉",
                    _ => string.Empty
                };
                worksheet.Cells[aggregatedRowIndex, dataStartColumn + 2].Value = icon;
            }

            aggregatedRowIndex++;
        }

        worksheet.Cells[1, dataStartColumn].Value = "Genre (Aggregated)";
        worksheet.Cells[1, dataStartColumn + 1].Value = "Total Sales (Aggregated)";
        worksheet.Row(1).Style.Font.Bold = true;

        worksheet.Cells[1, dataStartColumn, aggregatedRowIndex - 1, dataStartColumn + 1].AutoFitColumns();

        worksheet.ConditionalFormatting.AddDatabar(
            new ExcelAddress(2, dataStartColumn + 1, aggregatedRowIndex - 1, dataStartColumn + 1), Color.Red
        );


        pieChart.SetPosition(1, 0, dataStartColumn + 3, 0);
        pieChart.SetSize(600, 400);

        pieChart.Series.Add(
            worksheet.Cells[2, dataStartColumn + 1, aggregatedRowIndex - 1, dataStartColumn + 1],
            worksheet.Cells[2, dataStartColumn, aggregatedRowIndex - 1, dataStartColumn]
        );

        var excelData = package.GetAsByteArray();

        return Results.File(
            excelData,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "TotalSalesWithForecast.xlsx"
        );
    }
});

app.MapGet("/countrySales/export/{country}", async (string country, DateTime? startDate, DateTime? endDate, int forecastMonths, TrendEstimationFunction trendFunction) =>
{
    var countrySales = new List<CountrySalesData>();

    // ADO.NET connection and query execution
    using (var connection = new NpgsqlConnection(connectionString))
    {
        await connection.OpenAsync();

        // Base SQL query with optional date filtering
        var queryBuilder = @"
        SELECT 
            DATE_TRUNC('month', i.invoice_date) AS month,
            c.country AS customer_country,
            SUM(il.unit_price * il.quantity) AS total_sales,
            COUNT(i.invoice_id) AS number_of_sales
        FROM 
            invoice i
        JOIN 
            customer c ON i.customer_id = c.customer_id
        JOIN 
            invoice_line il ON i.invoice_id = il.invoice_id
        WHERE 
            UPPER(c.country) = UPPER(@country)";

        if (startDate.HasValue)
            queryBuilder += " AND i.invoice_date >= @startDate";

        if (endDate.HasValue)
            queryBuilder += " AND i.invoice_date <= @endDate";

        queryBuilder += @"
        GROUP BY 
            month, c.country
        ORDER BY 
            month ASC;";

        using (var command = new NpgsqlCommand(queryBuilder, connection))
        {
            // Add parameters to prevent SQL injection
            command.Parameters.AddWithValue("country", country);
            if (startDate.HasValue) command.Parameters.AddWithValue("startDate", startDate.Value);
            if (endDate.HasValue) command.Parameters.AddWithValue("endDate", endDate.Value);

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    countrySales.Add(new CountrySalesData
                    {
                        Month = reader.GetDateTime(0),
                        CustomerCountry = reader.GetString(1),
                        TotalSales = reader.GetDouble(2),
                        NumberOfSales = reader.GetInt32(3)
                    });
                }
            }
        }
    }

    // Generate Excel file
    using (var package = new ExcelPackage())
    {
        var worksheet = package.Workbook.Worksheets.Add($"{country} Sales");

        // Headers
        worksheet.Cells[1, 1].Value = "Month";
        worksheet.Cells[1, 2].Value = "Customer Country";
        worksheet.Cells[1, 3].Value = "Total Sales";
        worksheet.Cells[1, 4].Value = "Number of Sales";
        worksheet.Row(1).Style.Font.Bold = true;

        // Fill in the data
        for (var i = 0; i < countrySales.Count; i++)
        {
            worksheet.Cells[i + 2, 1].Value = countrySales[i].Month.ToString("yyyy-MM");
            worksheet.Cells[i + 2, 2].Value = countrySales[i].CustomerCountry;
            worksheet.Cells[i + 2, 3].Value = countrySales[i].TotalSales;
            worksheet.Cells[i + 2, 4].Value = countrySales[i].NumberOfSales;
        }

        worksheet.ConditionalFormatting.AddDatabar(new ExcelAddress(2, 3, countrySales.Count + 1, 3), Color.Blue);
        worksheet.ConditionalFormatting.AddDatabar(new ExcelAddress(2, 4, countrySales.Count + 1, 4), Color.Green);
        // Optional: Format columns
        worksheet.Column(3).Style.Numberformat.Format = "$#,##0.00";
        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        // Add Chart
        var chart = worksheet.Drawings.AddChart("TotalSalesChart", eChartType.Line);
        chart.Title.Text = "Total Sales with Forecast";
        chart.SetPosition(0, 0, 4, 0);
        chart.SetSize(800, 400);

        // Corrected Series Data Ranges
        var dataSeries = chart.Series.Add(
            worksheet.Cells[2, 3, countrySales.Count + 1, 3], // Y-axis (Total Sales)
            worksheet.Cells[2, 1, countrySales.Count + 1, 1] // X-axis (Month)
        );
        dataSeries.Header = "Total Sales";

        // Add Trendline for Forecast
        var trendline = dataSeries.TrendLines.Add(eTrendLine.Linear);
        trendline.DisplayRSquaredValue = true;
        trendline.DisplayEquation = true;
        trendline.Forward = forecastMonths; // Predict future months

        // Save Excel File
        var excelData = package.GetAsByteArray();

        return Results.File(
            excelData,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"{country}_MonthlySales.xlsx"
        );
    }
})
.WithName("ExportCountrySales");

app.MapGet("/countryGenreSales/export/{country}/{genre}",
        async (string country, string genre, DateTime? startDate, DateTime? endDate, int forecastMonths,
            TrendEstimationFunction trendFunction) =>
        {
            var genreSales = new List<CountrySalesData>();

            // ADO.NET connection and query execution
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync(); // Open connection asynchronously

                // Base SQL query with optional date filtering
                var queryBuilder = @"
            SELECT 
                c.country AS customer_country,
                g.Name AS genre,
                DATE_TRUNC('month', i.invoice_date) AS month,
                SUM(il.unit_price * il.quantity) AS total_sales,
                COUNT(i.invoice_id) AS number_of_sales
            FROM 
                invoice i
            JOIN 
                customer c ON i.customer_id = c.customer_id
            JOIN 
                invoice_line il ON i.invoice_id = il.invoice_id
            JOIN 
                Track t ON il.Track_Id = t.Track_Id
            JOIN 
                Genre g ON t.Genre_Id = g.Genre_Id
            WHERE 
                UPPER(c.country) = UPPER(@country) AND
                UPPER(g.Name) = UPPER(@genre)"; // Using parameterized query to prevent SQL injection

                // If a start date is provided, append the corresponding condition
                if (startDate.HasValue)
                    queryBuilder +=
                        $" AND i.invoice_date >= '{startDate.Value:yyyy-MM-dd} 00:00:00'"; // Directly interpolating date

                // If an end date is provided, append the corresponding condition
                if (endDate.HasValue)
                    queryBuilder +=
                        $" AND i.invoice_date <= '{endDate.Value:yyyy-MM-dd} 23:59:59'"; // Directly interpolating date

                queryBuilder += @"
            GROUP BY 
                c.country, g.Name, month
            ORDER BY 
                3 asc;"; // Order by total sales in ascending order (or adjust based on needs)

                using (var command = new NpgsqlCommand(queryBuilder, connection)) // Use NpgsqlCommand
                {
                    // Add the country and genre parameters
                    command.Parameters.AddWithValue("country", country); // Parameterized query
                    command.Parameters.AddWithValue("genre", genre); // Parameterized query

                    using (var reader = await command.ExecuteReaderAsync()) // Use NpgsqlDataReader
                    {
                        while (await reader.ReadAsync()) // Read asynchronously
                            genreSales.Add(new CountrySalesData
                            {
                                CustomerCountry = reader.GetString(0),
                                Genre = reader.GetString(1),
                                Month = reader.GetDateTime(2),
                                TotalSales = reader.GetDouble(3),
                                NumberOfSales = reader.GetInt32(4)
                            });
                    }
                }
            }

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add($"{country} - {genre} Sales");

                worksheet.Cells[1, 1].Value = "Month";
                worksheet.Cells[1, 2].Value = "Customer Country";
                worksheet.Cells[1, 3].Value = "Genre";
                worksheet.Cells[1, 4].Value = "Total Sales";
                worksheet.Cells[1, 5].Value = "Number of Sales";
                worksheet.Row(1).Style.Font.Bold = true;

                for (var i = 0; i < genreSales.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = genreSales[i].Month.ToString("yyyy-MM");
                    worksheet.Cells[i + 2, 2].Value = genreSales[i].CustomerCountry;
                    worksheet.Cells[i + 2, 3].Value = genreSales[i].Genre;
                    worksheet.Cells[i + 2, 4].Value = genreSales[i].TotalSales;
                    worksheet.Cells[i + 2, 5].Value = genreSales[i].NumberOfSales;
                }

                worksheet.ConditionalFormatting.AddDatabar(new ExcelAddress(2, 4, genreSales.Count + 1, 4), Color.Blue);
                worksheet.ConditionalFormatting.AddDatabar(new ExcelAddress(2, 5, genreSales.Count + 1, 5), Color.Green);
                
                worksheet.Column(4).Style.Numberformat.Format = "$#,##0.00";
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                var chart = worksheet.Drawings.AddChart("TotalSalesChart", eChartType.Line);
                chart.Title.Text = "Total Sales Of Country per Genre";
                chart.SetPosition(0, 0, 6, 0);
                chart.SetSize(800, 400);

                var dataSeries = chart.Series.Add(
                    worksheet.Cells[2, 4, genreSales.Count + 1, 4], // Y-axis (Total Sales)
                    worksheet.Cells[2, 1, genreSales.Count + 1, 1] // X-axis (Month)
                );
                dataSeries.Header = "Total Sales";

                eTrendLine trendlineType;
                switch (trendFunction)
                {
                    case TrendEstimationFunction.Linear:
                        trendlineType = eTrendLine.Linear;
                        break;

                    case TrendEstimationFunction.Polynomial:
                        trendlineType = eTrendLine.Polynomial;
                        break;

                    case TrendEstimationFunction.Logarithmic:
                        trendlineType = eTrendLine.Logarithmic;
                        break;

                    case TrendEstimationFunction.Exponential:
                        trendlineType = eTrendLine.Exponential;
                        break;

                    case TrendEstimationFunction.Power:
                        trendlineType = eTrendLine.Power;
                        break;

                    case TrendEstimationFunction.MovingAverage:
                        trendlineType = eTrendLine.MovingAverage;
                        break;

                    default:
                        trendlineType = eTrendLine.Linear;
                        break;
                }

                var trendline = dataSeries.TrendLines.Add(trendlineType);

                trendline.DisplayRSquaredValue = true;
                trendline.DisplayEquation = true;
                trendline.Forward = forecastMonths;

                var excelData = package.GetAsByteArray();

                return Results.File(
                    excelData,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"{country}_{genre}_Sales.xlsx"
                );
            }
        })
    .WithName("GetCountryGenreSalesExport");

app.MapGet("/countrySalesAll", async (DateTime? startDate, DateTime? endDate) =>
    {
        var countrySales = new List<CountryAllSales>();

        // ADO.NET connection and query execution
        using (var connection = new NpgsqlConnection(connectionString))
        {
            await connection.OpenAsync(); // Open connection asynchronously

            // Base SQL query with optional date filtering
            var queryBuilder = @"
            SELECT 
                c.country AS customer_country,
                SUM(il.unit_price * il.quantity) AS total_sales
            FROM 
                invoice i
            JOIN 
                customer c ON i.customer_id = c.customer_id
            JOIN 
                invoice_line il ON i.invoice_id = il.invoice_id
            "; // Using parameterized query to prevent SQL injection

            // If a start date is provided, append the corresponding condition
            if (startDate.HasValue)
                queryBuilder +=
                    $" AND i.invoice_date >= '{startDate.Value:yyyy-MM-dd} 00:00:00'"; // Directly interpolating date

            // If an end date is provided, append the corresponding condition
            if (endDate.HasValue)
                queryBuilder +=
                    $" AND i.invoice_date <= '{endDate.Value:yyyy-MM-dd} 23:59:59'"; // Directly interpolating date

            queryBuilder += @"
            GROUP BY 
                 c.country
            ;"; 

            using (var command = new NpgsqlCommand(queryBuilder, connection)) // Use NpgsqlCommand
            {
                using (var reader = await command.ExecuteReaderAsync()) // Use NpgsqlDataReader
                {
                    while (await reader.ReadAsync()) // Read asynchronously
                        countrySales.Add(new CountryAllSales()
                        {
                            CustomerCountry = reader.GetString(0),
                            Genre = "All",
                            TotalSales = reader.GetDouble(1),
                        });
                }
            }
        }

        return countrySales;
    })
    .WithName("GetCountrySalesTotal");

app.MapGet("/countryGenreSalesAll/{genre}",
    async (string genre, DateTime? startDate, DateTime? endDate) =>
    {
        var genreSales = new List<CountryAllSales>();

        // ADO.NET connection and query execution
        using (var connection = new NpgsqlConnection(connectionString))
        {
            await connection.OpenAsync(); // Open connection asynchronously

            // Base SQL query with optional date filtering
            var queryBuilder = @"
                SELECT 
                    c.country AS customer_country,
                    g.Name AS genre,
                    SUM(il.unit_price * il.quantity) AS total_sales
                FROM 
                    invoice i
                JOIN 
                    customer c ON i.customer_id = c.customer_id
                JOIN 
                    invoice_line il ON i.invoice_id = il.invoice_id
                JOIN 
                    Track t ON il.Track_Id = t.Track_Id
                JOIN 
                    Genre g ON t.Genre_Id = g.Genre_Id
                WHERE 
                    UPPER(g.Name) = UPPER(@genre)"; // Using parameterized query to prevent SQL injection

            // If a start date is provided, append the corresponding condition
            if (startDate.HasValue)
                queryBuilder +=
                    $" AND i.invoice_date >= '{startDate.Value:yyyy-MM-dd} 00:00:00'"; // Directly interpolating date

            // If an end date is provided, append the corresponding condition
            if (endDate.HasValue)
                queryBuilder +=
                    $" AND i.invoice_date <= '{endDate.Value:yyyy-MM-dd} 23:59:59'"; // Directly interpolating date

            queryBuilder += @"
                GROUP BY 
                    c.country, g.Name;";

            using (var command = new NpgsqlCommand(queryBuilder, connection)) // Use NpgsqlCommand
            {
                // Add the country and genre parameters
                command.Parameters.AddWithValue("genre", genre); // Parameterized query

                using (var reader = await command.ExecuteReaderAsync()) // Use NpgsqlDataReader
                {
                    while (await reader.ReadAsync()) // Read asynchronously
                        genreSales.Add(new CountryAllSales
                        {
                            CustomerCountry = reader.GetString(0),
                            Genre = reader.GetString(1),
                            TotalSales = reader.GetDouble(2),
                        });
                }
            }
        }

        return genreSales;
    });
    
app.Run();


public class SalesData
{
    public DateTime Month { get; set; }
    public double TotalSales { get; set; }
    public double NumberOfSales { get; set; }
}

public class GenreSalesData
{
    public string Genre { get; set; }
    public DateTime Month { get; set; }
    public double TotalSales { get; set; }
    public int NumberOfSales { get; set; }
}

public class CountrySalesData
{
    public DateTime Month { get; set; }
    public string CustomerCountry { get; set; }
    public double TotalSales { get; set; }
    public int NumberOfSales { get; set; }
    public string Genre { get; set; }
}

public class CountryAllSales
{
    public string CustomerCountry { get; set; }
    public double TotalSales { get; set; }
    public string Genre { get; set; }
}
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Npgsql; // Include the Npgsql namespace for PostgreSQL

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
string connectionString = "Host=localhost;Port=5432;Database=music_database;Username=postgres;Password=strategikon;Trust Server Certificate=True;";

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
        var queryBuilder = $@"
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
        {
            queryBuilder += $" AND i.Invoice_Date >= '{startDate.Value:yyyy-MM-dd} 00:00:00'"; // Directly interpolating date
        }

        // If an end date is provided, append the corresponding condition
        if (endDate.HasValue)
        {
            queryBuilder += $" AND i.Invoice_Date <= '{endDate.Value:yyyy-MM-dd} 23:59:59'"; // Directly interpolating date
        }

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
                {
                    totalSales.Add(new SalesData
                    {
                        Month = reader.GetDateTime(0),
                        // Conditionally read TotalSales or TotalQuantity
                        TotalSales = reader.GetDouble(1),
                        NumberOfSales = reader.GetDouble(2),
                    });
                }
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
        var queryBuilder = $@"
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
        {
            queryBuilder += $" AND i.Invoice_Date >= '{startDate.Value:yyyy-MM-dd} 00:00:00'"; // Directly interpolating date
        }

        // If an end date is provided, append the corresponding condition
        if (endDate.HasValue)
        {
            queryBuilder += $" AND i.Invoice_Date <= '{endDate.Value:yyyy-MM-dd} 23:59:59'"; // Directly interpolating date
        }

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
                {
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
        var queryBuilder = $@"
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
        {
            queryBuilder += $" AND i.invoice_date >= '{startDate.Value:yyyy-MM-dd} 00:00:00'"; // Directly interpolating date
        }

        // If an end date is provided, append the corresponding condition
        if (endDate.HasValue)
        {
            queryBuilder += $" AND i.invoice_date <= '{endDate.Value:yyyy-MM-dd} 23:59:59'"; // Directly interpolating date
        }

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

    return countrySales;
})
.WithName("GetCountrySalesPerMonth");

app.MapGet("/countryGenreSales/{country}/{genre}", async (string country, string genre, DateTime? startDate, DateTime? endDate) =>
{
    var genreSales = new List<CountrySalesData>();

    // ADO.NET connection and query execution
    using (var connection = new NpgsqlConnection(connectionString))
    {
    await connection.OpenAsync(); // Open connection asynchronously

    // Base SQL query with optional date filtering
    var queryBuilder = $@"
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
    {
        queryBuilder += $" AND i.invoice_date >= '{startDate.Value:yyyy-MM-dd} 00:00:00'"; // Directly interpolating date
    }

    // If an end date is provided, append the corresponding condition
    if (endDate.HasValue)
    {
        queryBuilder += $" AND i.invoice_date <= '{endDate.Value:yyyy-MM-dd} 23:59:59'"; // Directly interpolating date
    }

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
            {
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
    }

    return genreSales;
})
.WithName("GetCountryGenreSalesPerMonth");


// Run the application
app.Run();

// Model for SalesData
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
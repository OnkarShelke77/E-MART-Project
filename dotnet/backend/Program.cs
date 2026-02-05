using System.IdentityModel.Tokens.Jwt;
using System.Text;
using EMart.Data;
using EMart.Middleware;
using EMart.Repositories;
using EMart.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Force port to 8080 to match Java backend expectations in frontend
builder.WebHost.UseUrls("http://localhost:8080");

// Standard claim mapping is preferred for ClaimTypes.Name integration
// JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

// Add services to the container.
builder.Services.AddControllers();

// Configure CORS for React App
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowReactApp",
        policy =>
        {
            policy
                .WithOrigins("http://localhost:5173", "http://localhost:5174") // React URL
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    );
});

// Configure MySQL Connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<EMartDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(
    builder.Configuration["JwtSettings:Key"] ?? "emart_super_secret_key_1234567890_antigravity"
);

builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            ClockSkew = TimeSpan.FromMinutes(5),
            NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
        };
    });

// Dependency Injection
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ILoyaltycardService, LoyaltycardService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IInvoicePdfService, InvoicePdfService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseGlobalExceptionHandler();

// app.UseHttpsRedirection();

app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ---------------------------------------------------------
// AUTO-MIGRATION: Add missing columns if they don't exist
// ---------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<EMartDbContext>();
        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();

        string[] tables = { "orderitem", "cartitem" };
        foreach (var table in tables)
        {
            // Add points_used if missing
            using (var command = connection.CreateCommand())
            {
                command.CommandText = $@"
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = '{table}' AND COLUMN_NAME = 'points_used' 
                    AND TABLE_SCHEMA = DATABASE()";
                var exists = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
                if (!exists)
                {
                    using (var alterCmd = connection.CreateCommand())
                    {
                        alterCmd.CommandText = $"ALTER TABLE {table} ADD COLUMN points_used INT NOT NULL DEFAULT 0";
                        await alterCmd.ExecuteNonQueryAsync();
                        Console.WriteLine($"✅ Added 'points_used' to {table}");
                    }
                }
            }

            // Add price_type if missing
            using (var command = connection.CreateCommand())
            {
                command.CommandText = $@"
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = '{table}' AND COLUMN_NAME = 'price_type' 
                    AND TABLE_SCHEMA = DATABASE()";
                var exists = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
                if (!exists)
                {
                    using (var alterCmd = connection.CreateCommand())
                    {
                        alterCmd.CommandText = $"ALTER TABLE {table} ADD COLUMN price_type VARCHAR(10) DEFAULT 'MRP'";
                        await alterCmd.ExecuteNonQueryAsync();
                        Console.WriteLine($"✅ Added 'price_type' to {table}");
                    }
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ DB Schema Update Warning: {ex.Message}");
    }
}

app.Run();

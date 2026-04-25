using CloudinaryDotNet;
using Microsoft.EntityFrameworkCore;
using Racha629.Api.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── CORS ──────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ── Database (PostgreSQL / Neon) ───────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Cloudinary ────────────────────────────────────────────────────────────
var cloudAccount = new Account(
    builder.Configuration["Cloudinary:CloudName"],
    builder.Configuration["Cloudinary:ApiKey"],
    builder.Configuration["Cloudinary:ApiSecret"]
);
builder.Services.AddSingleton(new Cloudinary(cloudAccount));

// ── JWT Auth ──────────────────────────────────────────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(builder.Configuration["AppSettings:Token"]!)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddControllers();

var app = builder.Build();

// ── Auto-create DB schema on first boot ───────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.EnsureCreated();
        Console.WriteLine("✅ Database schema ensured.");

        // ── Idempotent table creation for newly added models ──
        db.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS ""TourPackages"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""NameGeo"" TEXT NOT NULL DEFAULT '',
                ""DescriptionGeo"" TEXT,
                ""Price"" NUMERIC NOT NULL DEFAULT 0,
                ""ImageUrl"" TEXT,
                ""DurationHours"" DOUBLE PRECISION,
                ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
            );
            CREATE TABLE IF NOT EXISTS ""TourWaypoints"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""TourPackageId"" INTEGER NOT NULL REFERENCES ""TourPackages""(""Id"") ON DELETE CASCADE,
                ""Name"" TEXT NOT NULL DEFAULT '',
                ""Latitude"" DOUBLE PRECISION NOT NULL,
                ""Longitude"" DOUBLE PRECISION NOT NULL,
                ""OrderIndex"" INTEGER NOT NULL DEFAULT 0
            );
            CREATE TABLE IF NOT EXISTS ""TransportBookings"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""FullName"" TEXT NOT NULL DEFAULT '',
                ""Phone"" TEXT NOT NULL DEFAULT '',
                ""FromLocation"" TEXT NOT NULL DEFAULT '',
                ""ToLocation"" TEXT NOT NULL DEFAULT '',
                ""DistanceKm"" DOUBLE PRECISION NOT NULL DEFAULT 0,
                ""VehicleType"" TEXT NOT NULL DEFAULT 'taxi',
                ""Price"" DOUBLE PRECISION NOT NULL DEFAULT 0,
                ""Status"" TEXT NOT NULL DEFAULT 'Pending',
                ""BookingDate"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                ""Notes"" TEXT
            );
            CREATE TABLE IF NOT EXISTS ""DirectorySubmissions"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""FullName"" TEXT NOT NULL DEFAULT '',
                ""Phone"" TEXT NOT NULL DEFAULT '',
                ""District"" TEXT NOT NULL DEFAULT '',
                ""Village"" TEXT NOT NULL DEFAULT '',
                ""LocationType"" TEXT NOT NULL DEFAULT '',
                ""Latitude"" DOUBLE PRECISION NOT NULL DEFAULT 0,
                ""Longitude"" DOUBLE PRECISION NOT NULL DEFAULT 0,
                ""Status"" TEXT NOT NULL DEFAULT 'Pending',
                ""SubmittedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                ""Notes"" TEXT,
                ""Description"" TEXT,
                ""PhotoUrl"" TEXT
            );
            ALTER TABLE IF EXISTS ""DirectorySubmissions"" ADD COLUMN IF NOT EXISTS ""Phone"" TEXT NOT NULL DEFAULT '';
            ALTER TABLE IF EXISTS ""DirectorySubmissions"" ADD COLUMN IF NOT EXISTS ""Description"" TEXT;
            ALTER TABLE IF EXISTS ""DirectorySubmissions"" ADD COLUMN IF NOT EXISTS ""PhotoUrl"" TEXT;
        ");
        Console.WriteLine("✅ New tables ensured.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ EnsureCreated failed: {ex.Message}");
        Console.WriteLine(ex.ToString());
    }
}

app.UseExceptionHandler(errApp => errApp.Run(async ctx =>
{
    var ex = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
    ctx.Response.StatusCode = 500;
    ctx.Response.ContentType = "application/json";
    var msg = ex?.Message ?? "Unknown error";
    var detail = ex?.ToString() ?? "";
    Console.WriteLine($"❌ Unhandled exception: {detail}");
    await ctx.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new { error = msg, detail }));
}));

app.UseCors("AllowFrontend");
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

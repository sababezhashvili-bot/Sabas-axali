using CloudinaryDotNet;
using Microsoft.EntityFrameworkCore;
using Racha629.Api.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── CORS ──────────────────────────────────────────────────────────────────
var allowedOrigins = builder.Configuration["Cors:AllowedOrigins"]
    ?? "https://sabas-axali.vercel.app,http://localhost:5173";

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(allowedOrigins.Split(',', StringSplitOptions.TrimEntries))
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

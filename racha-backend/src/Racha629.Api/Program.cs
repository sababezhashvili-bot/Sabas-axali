using Microsoft.EntityFrameworkCore;
using Racha629.Api.Data;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Filters;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value!)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

var app = builder.Build();

// using (var scope = app.Services.CreateScope())
// {
//     var services = scope.ServiceProvider;
//     var context = services.GetRequiredService<AppDbContext>();
//     // context.Database.EnsureDeleted();
//     // context.Database.EnsureCreated();

//     // if (!context.Locations.Any())
//     // {
//     //     context.Locations.AddRange(
//     //         new Location
//     //         {
//     //             NameGeo = "ამბროლაური",
//     //             NameEng = "Ambrolauri",
//     //             TypeGeo = "ქალაქი",
//     //             TypeEng = "City",
//     //             InfoGeo = "რაჭის ადმინისტრაციული ცენტრი",
//     //             InfoEng = "Administrative center of Racha",
//     //             Latitude = 42.5176,
//     //             Longitude = 43.1481
//     //         }
//     //     );
//     //     context.SaveChanges();
//     // }
// }

app.UseStaticFiles(); // Serves uploaded images from wwwroot/images/

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
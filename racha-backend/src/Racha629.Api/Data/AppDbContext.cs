using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Racha629.Api.Data
{
    // 1. მონაცემთა ბაზის კონტექსტი
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Location> Locations { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<AdSpace> AdSpaces { get; set; }
        public DbSet<RentRequest> RentRequests { get; set; }
        public DbSet<SiteSetting> SiteSettings { get; set; }
        public DbSet<TourPackage> TourPackages { get; set; }
        public DbSet<TourWaypoint> TourWaypoints { get; set; }
        public DbSet<TransportBooking> TransportBookings { get; set; }
        public DbSet<DirectorySubmission> DirectorySubmissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // უზრუნველყოფს ცხრილების სწორ სახელებს SQL-ში
            modelBuilder.Entity<Location>().ToTable("Locations");
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Permission>().ToTable("Permissions");
            modelBuilder.Entity<ActivityLog>().ToTable("ActivityLogs");

            // Relationships
            modelBuilder.Entity<User>()
                .HasOne(u => u.Permission)
                .WithOne(p => p.User)
                .HasForeignKey<Permission>(p => p.UserId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.ActivityLogs)
                .WithOne(l => l.User)
                .HasForeignKey(l => l.UserId);

            modelBuilder.Entity<TourPackage>()
                .HasMany(t => t.Waypoints)
                .WithOne(w => w.TourPackage)
                .HasForeignKey(w => w.TourPackageId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }

    // 2. მომხმარებლის მოდელი (კლასის გარეთ)
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; } 
        public string PasswordHash { get; set; }
        public string Role { get; set; } = "User"; // SuperAdmin, Admin, User
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public Permission Permission { get; set; }
        public List<Location> Locations { get; set; } = new List<Location>();
        public List<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
    }

    // 4. უფლებების მოდელი
    public class Permission
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public bool CanViewUsers { get; set; } = false;
        public bool CanEditServices { get; set; } = false;
        public bool CanDeleteData { get; set; } = false;
        
        [System.Text.Json.Serialization.JsonIgnore]
        public User User { get; set; }
    }

    // 5. აქტივობის ლოგები
    public class ActivityLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Action { get; set; } // Login, Create, Delete, etc.
        public string ServiceType { get; set; } // Auth, Location, Admin
        public string Description { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [System.Text.Json.Serialization.JsonIgnore]
        public User User { get; set; }
    }

    // 3. ლოკაციის მოდელი (კლასის გარეთ)
    public class Location
    {
        public int Id { get; set; }
        public string NameGeo { get; set; }
        public string NameEng { get; set; }
        public string TypeGeo { get; set; } 
        public string TypeEng { get; set; }
        public string Category { get; set; } 
        public string InfoGeo { get; set; }
        public string InfoEng { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? ImageUrl { get; set; }
        public string? GalleryUrls { get; set; } // comma-separated Cloudinary URLs
        public string? Description { get; set; }

        public int? UserId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public User? User { get; set; }
    }

    // 6. სარეკლამო სივრცე (Ad Space)
    public class AdSpace
    {
        public int Id { get; set; }
        public string Name { get; set; } 
        public string Type { get; set; } // Billboard, Banner
        public double PriceMonthly { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Status { get; set; } = "Available"; // Available, Pending, Rented
        public string? CurrentImageUrl { get; set; }
        public int? CurrentRenterId { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public User? CurrentRenter { get; set; } // New: Navigation property for the current renter
        public List<RentRequest> RentRequests { get; set; } = new List<RentRequest>(); // New: Collection of rent requests for this ad space
    }

    // 7. ქირაობის მოთხოვნა (Rent Request)
    public class RentRequest
    {
        public int Id { get; set; }
        public int AdSpaceId { get; set; }
        public int UserId { get; set; }
        public string ImageUrl { get; set; }
        public int DurationMonths { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;

        [System.Text.Json.Serialization.JsonIgnore]
        public AdSpace AdSpace { get; set; } // New: Navigation property for the ad space
        [System.Text.Json.Serialization.JsonIgnore]
        public User User { get; set; } // New: Navigation property for the user who made the request
    }

    // 8. საიტის პარამეტრები (Site Settings — key/value)
    public class SiteSetting
    {
        public int Id { get; set; }
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
    }

    // 9. ტური (Tour Package)
    public class TourPackage
    {
        public int Id { get; set; }
        public string NameGeo { get; set; } = "";
        public string? DescriptionGeo { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public double? DurationHours { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<TourWaypoint> Waypoints { get; set; } = new();
    }

    // 10. ტურის საყრდენი წერტილი (Tour Waypoint)
    public class TourWaypoint
    {
        public int Id { get; set; }
        public int TourPackageId { get; set; }
        public string Name { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int OrderIndex { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public TourPackage? TourPackage { get; set; }
    }

    // 11. ტრანსპორტის ჯავშანი (Transport Booking)
    public class TransportBooking
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string FromLocation { get; set; } = "";
        public string ToLocation { get; set; } = "";
        public double DistanceKm { get; set; }
        public string VehicleType { get; set; } = "taxi"; // taxi, comfort
        public double Price { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }
    }

    // 12. ცნობარის გამოწერა (Directory Submission)
    public class DirectorySubmission
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string District { get; set; } = ""; // ამბროლაური or ონი
        public string Village { get; set; } = "";
        public string LocationType { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }
    }
}
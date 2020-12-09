﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

var factory = new HotelContextFactory();
var context = factory.CreateDbContext();

if (args[0] == "add")
{
    await AddData();
    Console.WriteLine("Hotels added");
}
else if (args[0] == "query")
{
    await QueryData();
}
else if (args[0] == "drop")
{
    await context.Database.ExecuteSqlRawAsync("DELETE FROM HotelSpecials");
    await context.Database.ExecuteSqlRawAsync("DELETE FROM RoomPrices");
    await context.Database.ExecuteSqlRawAsync("DELETE FROM RoomTypes");
    await context.Database.ExecuteSqlRawAsync("DELETE FROM HotelHotelSpecial");
    await context.Database.ExecuteSqlRawAsync("DELETE FROM Hotels");
}

async Task AddData()
{
    HotelSpecial dogFriendly, organicFood, spa, sauna, indoorPool, outdoorPool;
    await context.HotelSpecials.AddRangeAsync(new[]
    {
        dogFriendly = new HotelSpecial()
        {
            Special = Special.DogFriendly,
        },
        organicFood = new HotelSpecial()
        {
            Special = Special.OrganicFood,
        },
        spa = new HotelSpecial()
        {
            Special = Special.Spa,
        },
        sauna = new HotelSpecial()
        {
            Special = Special.Sauna,
        },
        indoorPool = new HotelSpecial()
        {
            Special = Special.IndoorPool,
        },
        outdoorPool = new HotelSpecial()
        {
            Special = Special.OutdoorPool,
        },
    });

    RoomType singleRoom3x10, doubleRoom10x15, singleRoom10x15, doubleRoom25x30, juniorSuite5x45, honeymoonSuite1x100;
    await context.RoomTypes.AddRangeAsync(new[]
    {
        singleRoom3x10 = new RoomType()
        {
            Title = "Single room",
            Size = 10,
            AvailableRooms = 3,
        },
        doubleRoom10x15 = new RoomType()
        {
            Title = "Double room",
            Size = 15,
            AvailableRooms = 10,
        },
        singleRoom10x15 = new RoomType()
        {
            Title = "Single room",
            Size = 15,
            AvailableRooms = 10,
            IsDisabilityAccessible = true,
        },
        doubleRoom25x30 = new RoomType()
        {
            Title = "Double room",
            Size = 30,
            AvailableRooms = 25,
            IsDisabilityAccessible = true,
        },
        juniorSuite5x45 = new RoomType()
        {
            Title = "Junior suite",
            Size = 45,
            AvailableRooms = 5,
            IsDisabilityAccessible = true,
        },
        honeymoonSuite1x100 = new RoomType()
        {
            Title = "Honeymoon suite",
            Size = 100,
            AvailableRooms = 1,
            IsDisabilityAccessible = true,
        },
    });

    await context.RoomPrices.AddRangeAsync(new[]
    {
        new Price()
        {
            PriceEurPerNight = 40,
            RoomType = singleRoom3x10,
        },
        new Price()
        {
            PriceEurPerNight = 60,
            RoomType = doubleRoom10x15,
        },
        new Price()
        {
            PriceEurPerNight = 70,
            RoomType = singleRoom10x15,
        },
        new Price()
        {
            PriceEurPerNight = 120,
            RoomType = doubleRoom25x30,
        },
        new Price()
        {
            PriceEurPerNight = 190,
            RoomType = juniorSuite5x45,
        },
        new Price()
        {
            PriceEurPerNight = 300,
            RoomType = honeymoonSuite1x100,
        },
    });

    await context.Hotels.AddRangeAsync(new[]
    {
        new Hotel()
        {
            Name = "Pension Marianne",
            Address = "Am Hausberg 17, 1234 Irgendwo",
            Specials = new List<HotelSpecial> { dogFriendly, organicFood},
            RoomTypes = new List<RoomType> { singleRoom3x10, doubleRoom10x15},
        },
        new Hotel()
        {
            Name = "Grand Hotel Goldener Hirsch",
            Address = "Im stillen Tal 42, 4711 Schönberg",
            Specials = new List<HotelSpecial> { spa, sauna, indoorPool, outdoorPool},
            RoomTypes = new List<RoomType> { singleRoom10x15, doubleRoom25x30, juniorSuite5x45, honeymoonSuite1x100},
        },
    });

    await context.SaveChangesAsync();
}

async Task QueryData()
{
    Console.OutputEncoding = Encoding.UTF8;

    foreach (var hotel in await context.Hotels.ToArrayAsync())
    {
        Console.WriteLine($"# {hotel.Name}");
        Console.WriteLine();
        Console.WriteLine("## Location");
        Console.WriteLine();
        Console.WriteLine($"{hotel.Address}");
        Console.WriteLine();
        Console.WriteLine("## Specials");
        Console.WriteLine();
        var specials = await context.Hotels
            .Include(h => h.Specials)
            .Where(h => h.Id == hotel.Id)
            .SelectMany(h => h.Specials)
            .ToArrayAsync();

        foreach (var special in specials)
        {
            Console.WriteLine($"* {special.Special}");
        }
        Console.WriteLine();
        Console.WriteLine("## Room Types");
        Console.WriteLine();
        Console.WriteLine($"| {"Room Type",-20} | {"Size",6} | {"Price Valid From",-16} | {"Price Valid To",-14} | {"Price in €",10} |");
        Console.WriteLine($"| {new string('-', 20)} | {new string('-', 5)}: | {new string('-', 16)} | {new string('-', 14)} | {new string('-', 9)}: |");

        var roomTypesWithPrice = await context.RoomPrices
            .Where(rp => rp.RoomType.HotelId == hotel.Id)
            .Include(b => b.RoomType)
            .ToArrayAsync();

        foreach (var roomTypeWithPrice in roomTypesWithPrice)
        {
            Console.WriteLine($"| {roomTypeWithPrice.RoomType.Title,-20} | {roomTypeWithPrice.RoomType.Size + " m²",6} | {roomTypeWithPrice.ValidFrom,-16} | {roomTypeWithPrice.ValidUntil,-14} | {roomTypeWithPrice.PriceEurPerNight + " €",10} |");
        }

        Console.WriteLine();
    }
}



#region Model
enum Special
{
    Spa,
    Sauna,
    DogFriendly,
    IndoorPool,
    OutdoorPool,
    BikeRental,
    ECarChargingStation,
    VegetarianCuisine,
    OrganicFood
}

class Hotel
{
    public int Id { get; set; }
    [MaxLength(50)]
    public string Name { get; set; }
    [MaxLength(50)]
    public string Address { get; set; }
    public List<HotelSpecial> Specials { get; set; } = new();
    public List<RoomType> RoomTypes { get; set; } = new();
}

class HotelSpecial
{
    public int Id { get; set; }
    public Special Special { get; set; }
    public List<Hotel> Hotels { get; set; }
}

class RoomType
{
    public int Id { get; set; }

    public Hotel Hotel { get; set; }

    public int HotelId { get; set; }

    [MaxLength(250)]
    public string Title { get; set; }

    [MaxLength(250)]
    public string Description { get; set; }

    public int Size { get; set; }

    public bool IsDisabilityAccessible { get; set; } = false;

    public int AvailableRooms { get; set; }
}

class Price
{
    public int Id { get; set; }

    public RoomType RoomType { get; set; }

    public int RoomTypeId { get; set; }

    public DateTime? ValidFrom { get; set; }

    public DateTime? ValidUntil { get; set; }

    [Column(TypeName = "decimal(8, 2)")]
    public decimal PriceEurPerNight { get; set; }

}
#endregion
#region DataContext
class HotelContext : DbContext
{
    public HotelContext(DbContextOptions<HotelContext> options)
        : base(options)
    { }

    public DbSet<Hotel> Hotels { get; set; }

    public DbSet<HotelSpecial> HotelSpecials { get; set; }

    public DbSet<RoomType> RoomTypes { get; set; }

    public DbSet<Price> RoomPrices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Hotel>()
            .HasIndex(h => h.Id)
            .IsUnique();
        modelBuilder.Entity<HotelSpecial>()
            .HasIndex(h => h.Id)
            .IsUnique();
        modelBuilder.Entity<RoomType>()
            .HasIndex(h => h.Id)
            .IsUnique();
        modelBuilder.Entity<Price>()
            .HasIndex(h => h.Id)
            .IsUnique();
        base.OnModelCreating(modelBuilder);
    }
}

class HotelContextFactory : IDesignTimeDbContextFactory<HotelContext>
{
    public HotelContext CreateDbContext(string[]? args = null)
    {
        var configuration = new ConfigurationBuilder().AddJsonFile("C:\\Users\\Olive\\Documents\\Schule\\4CHIF\\Prog_C#\\entity-framework-hotel-quiz-ReschOliver\\HotelExercise\\HotelExercise\\appsettings.json").Build();

        var optionsBuilder = new DbContextOptionsBuilder<HotelContext>();
        optionsBuilder
            // Uncomment the following line if you want to print generated
            // SQL statements on the console.
            //.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
            .UseSqlServer(configuration["ConnectionStrings:DefaultConnection"]);

        return new HotelContext(optionsBuilder.Options);
    }
}

#endregion

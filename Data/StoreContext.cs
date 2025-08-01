using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class StoreContext(DbContextOptions options) : IdentityDbContext<User>(options)
{
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<IdentityRole>()
            .HasData(
                new IdentityRole {Id = "0967d782-f282-4bc4-aac8-6a5d03a21ea1", Name = "Member", NormalizedName = "MEMBER" },
                new IdentityRole {Id = "c1f24a97-16d4-47ab-b749-073bc54873d7", Name = "Admin", NormalizedName = "ADMIN" }
            );
    }

}
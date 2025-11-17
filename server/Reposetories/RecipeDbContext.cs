using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Smart_Recipe_Generator.Models;

namespace Smart_Recipe_Generator.Repository
{
    public partial class RecipeDbContext : DbContext
    {
        public RecipeDbContext()
        {
        }

        public RecipeDbContext(DbContextOptions<RecipeDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Product> Products { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // אפשר להגדיר פה את ה-connection string אם צריך
            // => optionsBuilder.UseSqlServer("...");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.CategoryId)
                      .HasAnnotation("Relational:Name", "PK__Categori__19093A0BFD30E339"); // שם מפתח ראשי

                entity.Property(e => e.Color).HasMaxLength(50);
                entity.Property(e => e.Emoji).HasMaxLength(10);
                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.ProductId)
                      .HasAnnotation("Relational:Name", "PK__Products__B40CC6CD2DC0DF8F");

                entity.HasIndex(e => e.CategoryId)
                      .HasAnnotation("Relational:Name", "IX_Products_CategoryId");

                entity.Property(e => e.Color).HasMaxLength(50);
                entity.Property(e => e.Emoji).HasMaxLength(10);
                entity.Property(e => e.Name).HasMaxLength(100);

                // קשר ל-Category
                entity.HasOne(d => d.Category)
                      .WithMany(p => p.Products)
                      .HasForeignKey(d => d.CategoryId)
                      .HasAnnotation("Relational:Name", "FK_Products_Categories"); // שם קונסטריינט
            });



            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

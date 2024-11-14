using EmailHubApp.Model.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailHubApp.Database
{
    public class EmailHubDB : DbContext
    {
        public DbSet<User> User { get; set; }
        public DbSet<UserType> UserType { get; set; }
        public DbSet<Operations> Operations { get; set; }
        public DbSet<SearchQuery> SearchQuery { get; set; }
        public DbSet<SearchedData> SearchedData { get; set; }
        public DbSet<SearchRequirements> SearchRequirements { get; set; }
        

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = @"Data Source = TIPU\SQLEXPRESS;
                                      Database = EmailHubDB;
                                      Trusted_Connection = True;
                                      Encrypt = False;";
            optionsBuilder.UseSqlServer(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(u => u.UserName).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

            modelBuilder.Entity<UserType>().HasIndex(t => t.Name).IsUnique();

            modelBuilder.Entity<SearchRequirements>().HasIndex(s => s.CX).IsUnique();
            modelBuilder.Entity<SearchRequirements>().HasIndex(s => s.ApiKey).IsUnique();

            modelBuilder.Entity<User>().HasOne(u => u.UserType).WithMany().HasForeignKey(u => u.TypeID).OnDelete(DeleteBehavior.Cascade);

            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}

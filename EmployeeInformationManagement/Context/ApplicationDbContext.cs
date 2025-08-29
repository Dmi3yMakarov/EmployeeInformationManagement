using EmployeeInformationManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeInformationManagement.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    public DbSet<Employee> Employees { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeID);
            entity.Property(e => e.Salary).HasColumnType("decimal(10,2)");
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }
}
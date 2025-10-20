//using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// https://docs.microsoft.com/en-us/ef/core/miscellaneous/configuring-dbcontext
// https://docs.microsoft.com/en-us/ef/core/modeling/
// https://docs.microsoft.com/en-us/ef/core/managing-schemas/
// If you want your EF Core model to be the source of truth, use Migrations. As you make changes to your EF Core model, this approach incrementally applies the corresponding schema changes to your database 
// so that it remains compatible with your EF Core model.

// Use Reverse Engineering if you want your database schema to be the source of truth. This approach allows you to scaffold a DbContext and the entity type classes by reverse engineering your database schema into an EF Core model.
// https://docs.microsoft.com/en-us/ef/core/managing-schemas/scaffolding
// Reverse engineering is the process of scaffolding entity type classes and a DbContext class based on a database schema.
// It can be performed using the Scaffold-DbContext command of the EF Core Package Manager Console (PMC) tools or the dotnet ef dbcontext scaffold command of the .NET Command-line Interface (CLI) tools.

// https://xamlbrewer.wordpress.com/category/entity-framework/

namespace JAGLog.Models
{
    //public class LoggingContext : DbContext
    //{
    //    public LoggingContext(DbContextOptions<LoggingContext> options)
    //        : base(options)
    //    { }
    //    #region Required
    //    protected override void OnModelCreating(ModelBuilder modelBuilder)
    //    {
    //        modelBuilder.Entity<SeriLog>()
    //            .Property(b => b.Id)
    //            .IsRequired();
     
    //        modelBuilder.Entity<SeriLog>(entity =>
    //        {
    //            entity.HasKey(c => new { c.Id });
    //        });

    //        modelBuilder.Entity<SeriLog>()
    //        .Property(b => b.TimeStamp);


    //        // https://entityframeworkcore.com/knowledge-base/48267925/invalid-object-name-error---entityframeworkcore-2-0
    //        // https://stackoverflow.com/questions/8262590/entity-framework-code-first-fluent-api-adding-indexes-to-columns
    //        // https://docs.microsoft.com/en-us/ef/ef6/modeling/code-first/fluent/types-and-properties
    //        // The code first fluent API is most commonly accessed by overriding the OnModelCreating method on your derived DbContext. 

    //        // 20200602 - added index
    //        modelBuilder.Entity<SeriLog>().ToTable("SeriLog")
    //                                      .HasIndex(s => new { s.TimeStamp }).IsClustered(false);

    //    }
    //    #endregion

    //    public DbSet<SeriLog> SeriLogs { get; set; }
    //}
}

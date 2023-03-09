using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Ticketingtool.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // This DbSet property maps to the 'JiraTaskDetails' table in the database
        // The JiraTaskDetail entity class is used to represent the rows in this table
        public DbSet<JiraTaskDetail> JiraTaskDetails { get; set; }
        public DbSet<jirastatus> jirastatus { get; set; }

        public DbSet<xref_productdevelopment_team> xref_productdevelopment_team { get; set; }

     

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // This line configures the 'JiraTaskDetails' entity to not have a primary key
            // This is necessary because the 'JiraTaskDetails' table does not have a primary key column

          
            modelBuilder.Entity<JiraTaskDetail>().HasNoKey();
            modelBuilder.Entity<jirastatus>().HasNoKey();
            modelBuilder.Entity<xref_productdevelopment_team>().HasNoKey();
        }
    }
}



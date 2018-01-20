using System;
using Microsoft.EntityFrameworkCore;
using Core.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Core.Context
{
	public class CoreContext : DbContext
	{
		public DbSet<Org> Orgs { get; set; }
        public DbSet<Portfolio> Portfolios { get; set; }
		public DbSet<Project> Projects { get; set; }
        public DbSet<Metric> Metrics { get; set; }
        public DbSet<OrgAccess> OrgAccess { get; set; }

        public CoreContext(DbContextOptions<CoreContext> options) : base(options) { } 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Cascade;
			}

            modelBuilder.Entity<Org>().ToTable("Org");//.HasIndex(c=>c.Name).IsUnique();
            modelBuilder.Entity<Portfolio>().ToTable("Portfolio");//.HasOne(a => a.Organization);
            modelBuilder.Entity<Project>().ToTable("Project");
		}
	}
}

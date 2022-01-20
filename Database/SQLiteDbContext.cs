using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BombMoney.Database
{
    public class SQLiteDbContext : DbContext
    {
        public DbSet<BoardroomDatum> BoardroomData { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("FileName=BombmoneyDB", option =>
                {
                    option.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
                });
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BoardroomDatum>().ToTable("BoardroomData", "bshareBot");
            modelBuilder.Entity<BoardroomDatum>(entity =>
            {
                entity.HasKey(k => k.BoardroomDataID);
                entity.HasIndex(i => i.Epoch).IsUnique();
            });
            base.OnModelCreating(modelBuilder);
        }
    }
}

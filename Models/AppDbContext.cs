using Microsoft.EntityFrameworkCore;

namespace ApiPaisesMVC_SQLServer.Models
{
    /// <summary>
    /// Contexto de Entity Framework Core. Gestiona el acceso a la base de datos
    /// SQL Server y expone las entidades como conjuntos consultables (DbSet).
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Pais> Paises { get; set; }
        public DbSet<Universidad> Universidades { get; set; }

        // Índices únicos que evitan duplicados a nivel de base de datos
        // (la unicidad no distingue mayúsculas por la collation de SQL Server).
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Pais>()
                .HasIndex(p => p.Nombre)
                .IsUnique();

            modelBuilder.Entity<Universidad>()
                .HasIndex(u => new { u.Nombre, u.Pais })
                .IsUnique();
        }
    }
}

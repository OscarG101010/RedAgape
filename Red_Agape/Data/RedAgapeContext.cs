using Microsoft.EntityFrameworkCore;
using Red_Agape.Models;

namespace Red_Agape.Data
{
    /*
        DbContextOptions: permite configurar la conexión a la base de datos y otras opciones de EF Core.
        DbContextOptions<RedAgapeContext> options: recibe la configuración específica del contexto.
        : base(options): envía esa configuración a la clase padre.

        public DbSet<TEntity> Nombres { get; set; }: Conjunto de entidades de un sólo tipo (EF Core) para consultar y modificar
        OnModelCreating(): método para configurar las entidades, llaves compuestas, relaciones y restricciones mediante Fluent API
            protected override void OnModelCreating(
                ModelBuilder modelBuilder
            )

        Métodos heredados: Add(entidad), Update(entidad), Remove(entidad), 
        SaveChanges(), SaveChangesAsync(), Set<TEntity>()
     */
    public class RedAgapeContext: DbContext // Hereda de DbContext
    {
        public RedAgapeContext(DbContextOptions<RedAgapeContext> options) : base(options) { } // Pasa opciones configuradas, cómo el proveedor de sql, cadena de conexión y otras opciones (constructor)

        public DbSet<Zona> Zonas { get; set; } // Definir entidad según una clase
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Organizacion> Organizaciones { get; set; }
        public DbSet<PeticionOrg> PeticionesOrg { get; set; }
        public DbSet<EstadoDonacion> EstadosDonacion { get; set; }
        public DbSet<Donacion> Donaciones { get; set; }
        public DbSet<ArticuloDonado> ArticulosDonados { get; set; }
        public DbSet<CategoriaPeticionOrg> CategoriasPeticionOrg { get; set; }
        public DbSet<HistorialEstado> HistorialEstados { get; set; }

        protected override void OnModelCreating(ModelBuilder mb) // configuraciones específicas
        {
            // Tablas en singular, como en SQL Server
            mb.Entity<Zona>().ToTable("Zona"); // Parámetro de ToTable: nombre de tabla
            mb.Entity<Categoria>().ToTable("Categoria");
            mb.Entity<Usuario>().ToTable("Usuario");
            mb.Entity<Rol>().ToTable("Rol");
            mb.Entity<Organizacion>().ToTable("Organizacion");
            mb.Entity<PeticionOrg>().ToTable("PeticionOrg");
            mb.Entity<EstadoDonacion>().ToTable("EstadoDonacion");
            mb.Entity<Donacion>().ToTable("Donacion");
            mb.Entity<ArticuloDonado>().ToTable("ArticuloDonado");
            mb.Entity<CategoriaPeticionOrg>().ToTable("CategoriaPeticionOrg");
            mb.Entity<HistorialEstado>().ToTable("HistorialEstado");

            // Fechas con DEFAULT de SQL Server
            mb.Entity<Usuario>()
              .Property(u => u.FechaRegistro).HasDefaultValueSql("SYSDATETIME()");
            mb.Entity<Donacion>()
              .Property(d => d.FechaRegistro).HasDefaultValueSql("SYSDATETIME()");
            mb.Entity<HistorialEstado>()
              .Property(h => h.Fecha).HasDefaultValueSql("SYSDATETIME()");

            // Enums guardados como texto (ya que guardan indices int)
            mb.Entity<Donacion>() // Tipo entrega varchar(20) utf-8
              .Property(d => d.TipoEntrega)
              .HasConversion<string>()
              .HasMaxLength(20).IsUnicode(false);

            mb.Entity<ArticuloDonado>() // Condicion varchar(100) utf-8
              .Property(a => a.Condicion)
              .HasConversion<string>()
              .HasMaxLength(100).IsUnicode(false);

            // Columnas VARCHAR utf-8 (no NVARCHAR utf-16)
            mb.Entity<Usuario>().Property(u => u.Telefono).HasMaxLength(20).IsUnicode(false);
            mb.Entity<Usuario>().Property(u => u.ContrasenaHash).HasMaxLength(255).IsUnicode(false);
            mb.Entity<Organizacion>().Property(o => o.Telefono).HasMaxLength(20).IsUnicode(false);

            // Muchos a muchos sin clase intermedia (por medio de Listas genéricas)
            mb.Entity<Usuario>()
              .HasMany(u => u.Roles)
              .WithMany(r => r.Usuarios)
              .UsingEntity(j => j.ToTable("UsuarioRol")); // Define la entidad intermedia UsuarioRol

            mb.Entity<ArticuloDonado>()
              .HasMany(a => a.Categorias)
              .WithMany(c => c.ArticulosDonados)
              .UsingEntity(j => j.ToTable("ArticuloCategoria")); // Define la entidad intermedia ArticuloCategoria

            // Muchos a muchos con clase intermedia (tiene CantidadSolicitada)
            mb.Entity<CategoriaPeticionOrg>(e =>
            {
                e.HasKey(x => new { x.PeticionOrgId, x.CategoriaId }); // Define la llave compuesta (no se definió en clase EF)
                e.HasOne(x => x.PeticionOrg).WithMany().HasForeignKey(x => x.PeticionOrgId);
                e.HasOne(x => x.Categoria).WithMany().HasForeignKey(x => x.CategoriaId);
            });
        }
    }
}
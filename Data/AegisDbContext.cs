using Microsoft.EntityFrameworkCore;
using ApiAegis.Models;
using BCrypt.Net;

namespace ApiAegis.Data
{
    public class AegisDbContext : DbContext
    {
        public AegisDbContext(DbContextOptions<AegisDbContext> options) : base(options)
        {
        }

        public DbSet<Rol> Roles { get; set; }
        public DbSet<Permiso> Permisos { get; set; }
        public DbSet<RolPermiso> RolPermisos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<MovimientoInventario> MovimientosInventario { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Compra> Compras { get; set; }
        public DbSet<CompraDetalle> ComprasDetalle { get; set; }
        public DbSet<MetodoPago> MetodosPago { get; set; }
        public DbSet<Caja> Cajas { get; set; }
        public DbSet<CajaApertura> CajaAperturas { get; set; }
        public DbSet<CajaCierre> CajaCierres { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<VentaDetalle> VentasDetalle { get; set; }
        public DbSet<VentaPago> VentaPagos { get; set; }
        public DbSet<Factura> Facturas { get; set; }
        public DbSet<Devolucion> Devoluciones { get; set; }
        public DbSet<DevolucionDetalle> DevolucionesDetalle { get; set; }
        public DbSet<Auditoria> Auditorias { get; set; }
        public DbSet<Configuracion> Configuraciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuraciones de precisión decimal para MariaDB/MySQL
            modelBuilder.Entity<Cliente>()
                .Property(c => c.LimiteCredito)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Producto>()
                .Property(p => p.Costo)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Producto>()
                .Property(p => p.PrecioVenta)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Producto>()
                .Property(p => p.PrecioMayoreo)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Compra>()
                .Property(c => c.Subtotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Compra>()
                .Property(c => c.Impuestos)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Compra>()
                .Property(c => c.Total)
                .HasPrecision(18, 2);

            modelBuilder.Entity<CompraDetalle>()
                .Property(cd => cd.CostoUnitario)
                .HasPrecision(18, 2);

            modelBuilder.Entity<CompraDetalle>()
                .Property(cd => cd.Subtotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<CajaApertura>()
                .Property(ca => ca.MontoInicial)
                .HasPrecision(18, 2);

            modelBuilder.Entity<CajaCierre>()
                .Property(cc => cc.MontoFinal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Venta>()
                .Property(v => v.Subtotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Venta>()
                .Property(v => v.Descuento)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Venta>()
                .Property(v => v.Impuestos)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Venta>()
                .Property(v => v.Total)
                .HasPrecision(18, 2);

            modelBuilder.Entity<VentaDetalle>()
                .Property(vd => vd.PrecioUnitario)
                .HasPrecision(18, 2);

            modelBuilder.Entity<VentaDetalle>()
                .Property(vd => vd.Descuento)
                .HasPrecision(18, 2);

            modelBuilder.Entity<VentaDetalle>()
                .Property(vd => vd.Subtotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<VentaPago>()
                .Property(vp => vp.Monto)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Venta>()
                .Property(v => v.Vuelto)
                .HasPrecision(18, 2);

            // Relaciones de Eliminación Restringida (para evitar deletes en cascada complejos)
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(u => u.RolId)
                .OnDelete(DeleteBehavior.Restrict);

            // Semillas de Datos (Data Seeding)
            // 1. Roles
            modelBuilder.Entity<Rol>().HasData(
                new Rol { Id = 1, Nombre = "Administrador", Descripcion = "Gestión total del sistema, configuración, inventario, reportes, usuarios y facturación" },
                new Rol { Id = 2, Nombre = "Supervisor", Descripcion = "Ventas, inventario, reportes y clientes" },
                new Rol { Id = 3, Nombre = "Vendedor", Descripcion = "Crear ventas, consultar productos y clientes" },
                new Rol { Id = 4, Nombre = "Caja", Descripcion = "Cobros, apertura y cierre de caja" }
            );

            // 2. Métodos de Pago
            modelBuilder.Entity<MetodoPago>().HasData(
                new MetodoPago { Id = 1, Nombre = "Efectivo", Activo = true },
                new MetodoPago { Id = 2, Nombre = "Tarjeta", Activo = true },
                new MetodoPago { Id = 3, Nombre = "Transferencia", Activo = true },
                new MetodoPago { Id = 4, Nombre = "Cheque", Activo = true },
                new MetodoPago { Id = 5, Nombre = "Crédito", Activo = true },
                new MetodoPago { Id = 6, Nombre = "Mixto", Activo = true }
            );

            // 3. Permisos básicos
            modelBuilder.Entity<Permiso>().HasData(
                new Permiso { Id = 1, Nombre = "GestionUsuarios", Descripcion = "Crear, editar y desactivar usuarios" },
                new Permiso { Id = 2, Nombre = "GestionRoles", Descripcion = "Gestionar roles y permisos" },
                new Permiso { Id = 3, Nombre = "GestionProductos", Descripcion = "Gestionar productos y categorías" },
                new Permiso { Id = 4, Nombre = "GestionClientes", Descripcion = "Gestionar clientes y límites de crédito" },
                new Permiso { Id = 5, Nombre = "CrearVentas", Descripcion = "Registrar ventas en el sistema" },
                new Permiso { Id = 6, Nombre = "AnularVentas", Descripcion = "Anular ventas registradas" },
                new Permiso { Id = 7, Nombre = "GestionInventario", Descripcion = "Registrar movimientos y ver stock" },
                new Permiso { Id = 8, Nombre = "GestionCaja", Descripcion = "Abrir y cerrar caja, ver reportes de cortes" },
                new Permiso { Id = 9, Nombre = "VerReportes", Descripcion = "Ver reportes y dashboard administrativo" },
                new Permiso { Id = 10, Nombre = "GestionConfiguracion", Descripcion = "Configurar parámetros del sistema" }
            );

            // 4. Asignar Permisos a Roles (RolPermisos)
            // Administrador tiene todos (1-10)
            modelBuilder.Entity<RolPermiso>().HasData(
                new RolPermiso { Id = 1, RolId = 1, PermisoId = 1 },
                new RolPermiso { Id = 2, RolId = 1, PermisoId = 2 },
                new RolPermiso { Id = 3, RolId = 1, PermisoId = 3 },
                new RolPermiso { Id = 4, RolId = 1, PermisoId = 4 },
                new RolPermiso { Id = 5, RolId = 1, PermisoId = 5 },
                new RolPermiso { Id = 6, RolId = 1, PermisoId = 6 },
                new RolPermiso { Id = 7, RolId = 1, PermisoId = 7 },
                new RolPermiso { Id = 8, RolId = 1, PermisoId = 8 },
                new RolPermiso { Id = 9, RolId = 1, PermisoId = 9 },
                new RolPermiso { Id = 10, RolId = 1, PermisoId = 10 }
            );

            // Supervisor tiene 3, 4, 5, 7, 9
            modelBuilder.Entity<RolPermiso>().HasData(
                new RolPermiso { Id = 11, RolId = 2, PermisoId = 3 },
                new RolPermiso { Id = 12, RolId = 2, PermisoId = 4 },
                new RolPermiso { Id = 13, RolId = 2, PermisoId = 5 },
                new RolPermiso { Id = 14, RolId = 2, PermisoId = 7 },
                new RolPermiso { Id = 15, RolId = 2, PermisoId = 9 }
            );

            // Vendedor tiene 5
            modelBuilder.Entity<RolPermiso>().HasData(
                new RolPermiso { Id = 16, RolId = 3, PermisoId = 5 }
            );

            // Caja tiene 8 y 5
            modelBuilder.Entity<RolPermiso>().HasData(
                new RolPermiso { Id = 17, RolId = 4, PermisoId = 5 },
                new RolPermiso { Id = 18, RolId = 4, PermisoId = 8 }
            );

            // 5. Usuario Administrador por defecto
            // Contraseña de prueba: admin123
            string passwordHash = BCrypt.Net.BCrypt.HashPassword("admin123");
            modelBuilder.Entity<Usuario>().HasData(
                new Usuario
                {
                    Id = 1,
                    Nombre = "Administrador",
                    Apellido = "Aegis",
                    Correo = "admin@aegispos.com",
                    NombreUsuario = "admin",
                    PasswordHash = passwordHash,
                    RolId = 1,
                    Activo = true,
                    FechaCreacion = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // 6. Cliente por defecto (Consumidor Final)
            modelBuilder.Entity<Cliente>().HasData(
                new Cliente
                {
                    Id = 1,
                    Nit = "CF",
                    Nombre = "Consumidor Final",
                    Direccion = "Ciudad",
                    Activo = true,
                    FechaCreacion = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // 7. Categoria por defecto
            modelBuilder.Entity<Categoria>().HasData(
                new Categoria { Id = 1, Nombre = "General", Descripcion = "Categoría general de productos", Activo = true }
            );

            // 8. Caja por defecto
            modelBuilder.Entity<Caja>().HasData(
                new Caja { Id = 1, Nombre = "Caja Principal", Estado = "Cerrada" }
            );
        }
    }
}

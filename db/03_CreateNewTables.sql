-- Script para crear tablas de Inventario, Ventas, Proveedores y Corte de Caja

-- 1. Proveedores
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Proveedores')
BEGIN
    CREATE TABLE Proveedores (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(150) NOT NULL,
        NitRfc NVARCHAR(50) NOT NULL,
        Telefono NVARCHAR(20) NULL,
        FechaCreacion DATETIME DEFAULT GETDATE(),
        Activo BIT DEFAULT 1
    );

    -- Datos de prueba
    INSERT INTO Proveedores (Nombre, NitRfc, Telefono) VALUES
    ('Proveedor Global S.A.', 'NIT-12345678-9', '555-1234'),
    ('Distribuidora Nacional', 'RFC-DINA900101', '555-5678');
END
GO

-- 2. Productos (Inventario)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Productos')
BEGIN
    CREATE TABLE Productos (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CodigoBarras NVARCHAR(50) NOT NULL UNIQUE,
        Nombre NVARCHAR(150) NOT NULL,
        Descripcion NVARCHAR(MAX) NULL,
        Categoria NVARCHAR(100) NULL,
        Precio DECIMAL(18, 2) NOT NULL,
        Stock INT NOT NULL DEFAULT 0,
        StockMinimo INT NOT NULL DEFAULT 5,
        Activo BIT DEFAULT 1,
        FechaCreacion DATETIME DEFAULT GETDATE()
    );

    -- Datos de prueba
    INSERT INTO Productos (CodigoBarras, Nombre, Descripcion, Categoria, Precio, Stock, StockMinimo) VALUES
    ('7501234567890', 'Camiseta Polo', 'Camiseta de algodón talla M', 'Ropa', 250.00, 50, 10),
    ('7509876543210', 'Pantalón Jean', 'Pantalón de mezclilla talla 32', 'Ropa', 450.00, 30, 5),
    ('7501112223334', 'Zapatos Deportivos', 'Zapatos para correr talla 40', 'Calzado', 850.00, 15, 5);
END
GO

-- 3. Corte de Caja
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CortesCaja')
BEGIN
    CREATE TABLE CortesCaja (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        IdUsuario INT NOT NULL FOREIGN KEY REFERENCES Usuarios(Id),
        FechaApertura DATETIME NOT NULL DEFAULT GETDATE(),
        FechaCierre DATETIME NULL,
        SaldoInicial DECIMAL(18, 2) NOT NULL,
        TotalVentas DECIMAL(18, 2) NOT NULL DEFAULT 0,
        TotalEgresos DECIMAL(18, 2) NOT NULL DEFAULT 0,
        SaldoFinal DECIMAL(18, 2) NULL,
        Estado NVARCHAR(20) DEFAULT 'Abierta' -- 'Abierta', 'Cerrada'
    );
    
    -- No insertamos datos de prueba aquí porque depende de las operaciones
END
GO

-- 4. Ventas
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Ventas')
BEGIN
    CREATE TABLE Ventas (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        IdUsuario INT NOT NULL FOREIGN KEY REFERENCES Usuarios(Id),
        IdCorteCaja INT NULL FOREIGN KEY REFERENCES CortesCaja(Id),
        Total DECIMAL(18, 2) NOT NULL,
        FechaVenta DATETIME DEFAULT GETDATE(),
        Estado NVARCHAR(20) DEFAULT 'Completada' -- 'Completada', 'Cancelada'
    );
END
GO

-- 5. Detalle de Ventas
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DetalleVentas')
BEGIN
    CREATE TABLE DetalleVentas (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        IdVenta INT NOT NULL FOREIGN KEY REFERENCES Ventas(Id),
        IdProducto INT NOT NULL FOREIGN KEY REFERENCES Productos(Id),
        Cantidad INT NOT NULL,
        PrecioUnitario DECIMAL(18, 2) NOT NULL,
        Subtotal DECIMAL(18, 2) NOT NULL
    );
END
GO

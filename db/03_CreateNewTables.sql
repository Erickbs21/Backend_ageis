-- Script para crear tablas de Inventario, Ventas, Proveedores y Corte de Caja

-- 1. Proveedores
CREATE TABLE IF NOT EXISTS Proveedores (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(150) NOT NULL,
    NitRfc VARCHAR(50) NOT NULL,
    Telefono VARCHAR(20) NULL,
    FechaCreacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    Activo TINYINT(1) DEFAULT 1
);

-- Datos de prueba
INSERT INTO Proveedores (Nombre, NitRfc, Telefono)
SELECT 'Proveedor Global S.A.', 'NIT-12345678-9', '555-1234' FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM Proveedores WHERE NitRfc = 'NIT-12345678-9');

INSERT INTO Proveedores (Nombre, NitRfc, Telefono)
SELECT 'Distribuidora Nacional', 'RFC-DINA900101', '555-5678' FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM Proveedores WHERE NitRfc = 'RFC-DINA900101');


-- 2. Productos (Inventario)
CREATE TABLE IF NOT EXISTS Productos (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CodigoBarras VARCHAR(50) NOT NULL UNIQUE,
    Nombre VARCHAR(150) NOT NULL,
    Descripcion TEXT NULL,
    Categoria VARCHAR(100) NULL,
    Precio DECIMAL(18, 2) NOT NULL,
    Stock INT NOT NULL DEFAULT 0,
    StockMinimo INT NOT NULL DEFAULT 5,
    Activo TINYINT(1) DEFAULT 1,
    FechaCreacion DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Datos de prueba
INSERT INTO Productos (CodigoBarras, Nombre, Descripcion, Categoria, Precio, Stock, StockMinimo)
SELECT '7501234567890', 'Camiseta Polo', 'Camiseta de algodón talla M', 'Ropa', 250.00, 50, 10 FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM Productos WHERE CodigoBarras = '7501234567890');

INSERT INTO Productos (CodigoBarras, Nombre, Descripcion, Categoria, Precio, Stock, StockMinimo)
SELECT '7509876543210', 'Pantalón Jean', 'Pantalón de mezclilla talla 32', 'Ropa', 450.00, 30, 5 FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM Productos WHERE CodigoBarras = '7509876543210');

INSERT INTO Productos (CodigoBarras, Nombre, Descripcion, Categoria, Precio, Stock, StockMinimo)
SELECT '7501112223334', 'Zapatos Deportivos', 'Zapatos para correr talla 40', 'Calzado', 850.00, 15, 5 FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM Productos WHERE CodigoBarras = '7501112223334');


-- 3. Corte de Caja
CREATE TABLE IF NOT EXISTS CortesCaja (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    IdUsuario INT NOT NULL,
    FechaApertura DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaCierre DATETIME NULL,
    SaldoInicial DECIMAL(18, 2) NOT NULL,
    TotalVentas DECIMAL(18, 2) NOT NULL DEFAULT 0,
    TotalEgresos DECIMAL(18, 2) NOT NULL DEFAULT 0,
    SaldoFinal DECIMAL(18, 2) NULL,
    Estado VARCHAR(20) DEFAULT 'Abierta', -- 'Abierta', 'Cerrada'
    FOREIGN KEY (IdUsuario) REFERENCES Usuarios(Id)
);


-- 4. Ventas
CREATE TABLE IF NOT EXISTS Ventas (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    IdUsuario INT NOT NULL,
    IdCorteCaja INT NULL,
    Total DECIMAL(18, 2) NOT NULL,
    FechaVenta DATETIME DEFAULT CURRENT_TIMESTAMP,
    Estado VARCHAR(20) DEFAULT 'Completada', -- 'Completada', 'Cancelada'
    FOREIGN KEY (IdUsuario) REFERENCES Usuarios(Id),
    FOREIGN KEY (IdCorteCaja) REFERENCES CortesCaja(Id)
);


-- 5. Detalle de Ventas
CREATE TABLE IF NOT EXISTS DetalleVentas (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    IdVenta INT NOT NULL,
    IdProducto INT NOT NULL,
    Cantidad INT NOT NULL,
    PrecioUnitario DECIMAL(18, 2) NOT NULL,
    Subtotal DECIMAL(18, 2) NOT NULL,
    FOREIGN KEY (IdVenta) REFERENCES Ventas(Id),
    FOREIGN KEY (IdProducto) REFERENCES Productos(Id)
);

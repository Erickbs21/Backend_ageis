-- PROCEDIMIENTOS ALMACENADOS PARA NUEVOS MÓDULOS

----------------------------------------------------
-- USUARIOS
----------------------------------------------------
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ObtenerUsuarios') DROP PROCEDURE sp_ObtenerUsuarios
CREATE PROCEDURE sp_ObtenerUsuarios
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Nombre, Username, Role, FechaCreacion, Activo
    FROM Usuarios
    WHERE Activo = 1;
END

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_CrearUsuario') DROP PROCEDURE sp_CrearUsuario
CREATE PROCEDURE sp_CrearUsuario
    @Nombre NVARCHAR(100),
    @Username NVARCHAR(100),
    @PasswordHash NVARCHAR(MAX),
    @Role NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM Usuarios WHERE Username = @Username)
    BEGIN
        RAISERROR('El nombre de usuario ya existe.', 16, 1);
        RETURN;
    END

    INSERT INTO Usuarios (Nombre, Username, PasswordHash, Role)
    VALUES (@Nombre, @Username, @PasswordHash, @Role);
    
    SELECT SCOPE_IDENTITY() AS IdNuevoUsuario;
END

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ResetearPasswordUsuario') DROP PROCEDURE sp_ResetearPasswordUsuario
CREATE PROCEDURE sp_ResetearPasswordUsuario
    @IdUsuario INT,
    @NuevoPasswordHash NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Usuarios
    SET PasswordHash = @NuevoPasswordHash
    WHERE Id = @IdUsuario;
END


----------------------------------------------------
-- PRODUCTOS E INVENTARIO
----------------------------------------------------
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ObtenerProductos') DROP PROCEDURE sp_ObtenerProductos
CREATE PROCEDURE sp_ObtenerProductos
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, CodigoBarras, Nombre, Descripcion, Categoria, Precio, Stock, StockMinimo
    FROM Productos
    WHERE Activo = 1;
END

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_CrearProducto') DROP PROCEDURE sp_CrearProducto
CREATE PROCEDURE sp_CrearProducto
    @CodigoBarras NVARCHAR(50),
    @Nombre NVARCHAR(150),
    @Descripcion NVARCHAR(MAX),
    @Categoria NVARCHAR(100),
    @Precio DECIMAL(18,2),
    @Stock INT,
    @StockMinimo INT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM Productos WHERE CodigoBarras = @CodigoBarras AND Activo = 1)
    BEGIN
        RAISERROR('El código de barras ya está registrado.', 16, 1);
        RETURN;
    END

    INSERT INTO Productos (CodigoBarras, Nombre, Descripcion, Categoria, Precio, Stock, StockMinimo)
    VALUES (@CodigoBarras, @Nombre, @Descripcion, @Categoria, @Precio, @Stock, @StockMinimo);
    
    SELECT SCOPE_IDENTITY() AS IdNuevoProducto;
END

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ActualizarProducto') DROP PROCEDURE sp_ActualizarProducto
CREATE PROCEDURE sp_ActualizarProducto
    @Id INT,
    @Precio DECIMAL(18,2),
    @Descripcion NVARCHAR(MAX),
    @StockMinimo INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Productos
    SET Precio = @Precio,
        Descripcion = @Descripcion,
        StockMinimo = @StockMinimo
    WHERE Id = @Id AND Activo = 1;
END

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_EliminarProducto') DROP PROCEDURE sp_EliminarProducto
CREATE PROCEDURE sp_EliminarProducto
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    -- Soft delete
    UPDATE Productos
    SET Activo = 0
    WHERE Id = @Id;
END


----------------------------------------------------
-- VENTAS (CON TRANSACCIÓN PARA DESCONTAR STOCK)
----------------------------------------------------
-- (Nota: Para un caso real con múltiples productos en una venta, usaremos un Type de Tabla, 
--  o se insertará el maestro y luego los detalles llamando a otro SP.
--  Aquí optamos por un SP que inserta la cabecera y otro para detalles con descuento de stock).

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_CrearVenta') DROP PROCEDURE sp_CrearVenta
CREATE PROCEDURE sp_CrearVenta
    @IdUsuario INT,
    @Total DECIMAL(18,2),
    @IdNuevaVenta INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @IdCorteCaja INT = NULL;
    
    -- Buscar si hay una caja abierta
    SELECT TOP 1 @IdCorteCaja = Id 
    FROM CortesCaja 
    WHERE Estado = 'Abierta' 
    ORDER BY Id DESC;

    INSERT INTO Ventas (IdUsuario, IdCorteCaja, Total)
    VALUES (@IdUsuario, @IdCorteCaja, @Total);

    SET @IdNuevaVenta = SCOPE_IDENTITY();
    
    -- Si hay caja, sumar total ventas
    IF @IdCorteCaja IS NOT NULL
    BEGIN
        UPDATE CortesCaja
        SET TotalVentas = TotalVentas + @Total
        WHERE Id = @IdCorteCaja;
    END
END

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_AgregarDetalleVenta') DROP PROCEDURE sp_AgregarDetalleVenta
CREATE PROCEDURE sp_AgregarDetalleVenta
    @IdVenta INT,
    @IdProducto INT,
    @Cantidad INT,
    @PrecioUnitario DECIMAL(18,2),
    @Subtotal DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. Verificar si hay stock
        DECLARE @StockActual INT;
        SELECT @StockActual = Stock FROM Productos WHERE Id = @IdProducto;

        IF @StockActual < @Cantidad
        BEGIN
            RAISERROR('Stock insuficiente para el producto.', 16, 1);
        END

        -- 2. Insertar detalle
        INSERT INTO DetalleVentas (IdVenta, IdProducto, Cantidad, PrecioUnitario, Subtotal)
        VALUES (@IdVenta, @IdProducto, @Cantidad, @PrecioUnitario, @Subtotal);

        -- 3. Descontar Stock
        UPDATE Productos
        SET Stock = Stock - @Cantidad
        WHERE Id = @IdProducto;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ObtenerHistorialVentas') DROP PROCEDURE sp_ObtenerHistorialVentas
CREATE PROCEDURE sp_ObtenerHistorialVentas
    @FechaInicio DATETIME = NULL,
    @FechaFin DATETIME = NULL,
    @IdUsuario INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT V.Id, V.IdUsuario, U.Nombre AS Cajero, V.Total, V.FechaVenta, V.Estado
    FROM Ventas V
    INNER JOIN Usuarios U ON V.IdUsuario = U.Id
    WHERE (@IdUsuario IS NULL OR V.IdUsuario = @IdUsuario)
      AND (@FechaInicio IS NULL OR V.FechaVenta >= @FechaInicio)
      AND (@FechaFin IS NULL OR V.FechaVenta <= @FechaFin)
    ORDER BY V.FechaVenta DESC;
END

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ObtenerTicketVenta') DROP PROCEDURE sp_ObtenerTicketVenta
CREATE PROCEDURE sp_ObtenerTicketVenta
    @IdVenta INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT DV.Id, DV.IdProducto, P.Nombre AS Producto, P.CodigoBarras, DV.Cantidad, DV.PrecioUnitario, DV.Subtotal
    FROM DetalleVentas DV
    INNER JOIN Productos P ON DV.IdProducto = P.Id
    WHERE DV.IdVenta = @IdVenta;
END


----------------------------------------------------
-- PROVEEDORES
----------------------------------------------------
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ObtenerProveedores') DROP PROCEDURE sp_ObtenerProveedores
CREATE PROCEDURE sp_ObtenerProveedores
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Nombre, NitRfc, Telefono
    FROM Proveedores
    WHERE Activo = 1;
END

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_CrearProveedor') DROP PROCEDURE sp_CrearProveedor
CREATE PROCEDURE sp_CrearProveedor
    @Nombre NVARCHAR(150),
    @NitRfc NVARCHAR(50),
    @Telefono NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Proveedores (Nombre, NitRfc, Telefono)
    VALUES (@Nombre, @NitRfc, @Telefono);
    
    SELECT SCOPE_IDENTITY() AS IdNuevoProveedor;
END

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ActualizarProveedor') DROP PROCEDURE sp_ActualizarProveedor
CREATE PROCEDURE sp_ActualizarProveedor
    @Id INT,
    @Nombre NVARCHAR(150),
    @NitRfc NVARCHAR(50),
    @Telefono NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Proveedores
    SET Nombre = @Nombre,
        NitRfc = @NitRfc,
        Telefono = @Telefono
    WHERE Id = @Id AND Activo = 1;
END


----------------------------------------------------
-- CORTE DE CAJA
----------------------------------------------------
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_EstadoCaja') DROP PROCEDURE sp_EstadoCaja
CREATE PROCEDURE sp_EstadoCaja
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 Id, IdUsuario, FechaApertura, SaldoInicial, TotalVentas, TotalEgresos, Estado
    FROM CortesCaja
    ORDER BY Id DESC;
END

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_AbrirCaja') DROP PROCEDURE sp_AbrirCaja
CREATE PROCEDURE sp_AbrirCaja
    @IdUsuario INT,
    @SaldoInicial DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM CortesCaja WHERE Estado = 'Abierta')
    BEGIN
        RAISERROR('Ya existe una caja abierta.', 16, 1);
        RETURN;
    END

    INSERT INTO CortesCaja (IdUsuario, SaldoInicial)
    VALUES (@IdUsuario, @SaldoInicial);
END

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_CerrarCaja') DROP PROCEDURE sp_CerrarCaja
CREATE PROCEDURE sp_CerrarCaja
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Estado NVARCHAR(20);
    SELECT @Estado = Estado FROM CortesCaja WHERE Id = @Id;

    IF @Estado <> 'Abierta'
    BEGIN
        RAISERROR('La caja ya se encuentra cerrada o no existe.', 16, 1);
        RETURN;
    END

    UPDATE CortesCaja
    SET FechaCierre = GETDATE(),
        Estado = 'Cerrada',
        SaldoFinal = SaldoInicial + TotalVentas - TotalEgresos
    WHERE Id = @Id;
    
    SELECT SaldoFinal FROM CortesCaja WHERE Id = @Id;
END

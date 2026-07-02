-- PROCEDIMIENTOS ALMACENADOS PARA NUEVOS MÓDULOS

----------------------------------------------------
-- USUARIOS
----------------------------------------------------
DROP PROCEDURE IF EXISTS sp_ObtenerUsuarios;
DELIMITER //
CREATE PROCEDURE sp_ObtenerUsuarios()
BEGIN
    SELECT Id, Nombre, Username, Role, FechaCreacion, Activo
    FROM Usuarios
    WHERE Activo = 1;
END //
DELIMITER ;

DROP PROCEDURE IF EXISTS sp_CrearUsuario;
DELIMITER //
CREATE PROCEDURE sp_CrearUsuario(
    IN p_Nombre VARCHAR(100),
    IN p_Username VARCHAR(100),
    IN p_PasswordHash TEXT,
    IN p_Role VARCHAR(50)
)
BEGIN
    IF EXISTS (SELECT 1 FROM Usuarios WHERE Username = p_Username) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El nombre de usuario ya existe.';
    ELSE
        INSERT INTO Usuarios (Nombre, Username, PasswordHash, Role)
        VALUES (p_Nombre, p_Username, p_PasswordHash, p_Role);
        
        SELECT LAST_INSERT_ID() AS IdNuevoUsuario;
    END IF;
END //
DELIMITER ;

DROP PROCEDURE IF EXISTS sp_ResetearPasswordUsuario;
DELIMITER //
CREATE PROCEDURE sp_ResetearPasswordUsuario(
    IN p_IdUsuario INT,
    IN p_NuevoPasswordHash TEXT
)
BEGIN
    UPDATE Usuarios
    SET PasswordHash = p_NuevoPasswordHash
    WHERE Id = p_IdUsuario;
END //
DELIMITER ;


----------------------------------------------------
-- PRODUCTOS E INVENTARIO
----------------------------------------------------
DROP PROCEDURE IF EXISTS sp_ObtenerProductos;
DELIMITER //
CREATE PROCEDURE sp_ObtenerProductos()
BEGIN
    SELECT Id, CodigoBarras, Nombre, Descripcion, Categoria, Precio, Stock, StockMinimo
    FROM Productos
    WHERE Activo = 1;
END //
DELIMITER ;

DROP PROCEDURE IF EXISTS sp_CrearProducto;
DELIMITER //
CREATE PROCEDURE sp_CrearProducto(
    IN p_CodigoBarras VARCHAR(50),
    IN p_Nombre VARCHAR(150),
    IN p_Descripcion TEXT,
    IN p_Categoria VARCHAR(100),
    IN p_Precio DECIMAL(18,2),
    IN p_Stock INT,
    IN p_StockMinimo INT
)
BEGIN
    IF EXISTS (SELECT 1 FROM Productos WHERE CodigoBarras = p_CodigoBarras AND Activo = 1) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El código de barras ya está registrado.';
    ELSE
        INSERT INTO Productos (CodigoBarras, Nombre, Descripcion, Categoria, Precio, Stock, StockMinimo)
        VALUES (p_CodigoBarras, p_Nombre, p_Descripcion, p_Categoria, p_Precio, p_Stock, p_StockMinimo);
        
        SELECT LAST_INSERT_ID() AS IdNuevoProducto;
    END IF;
END //
DELIMITER ;

DROP PROCEDURE IF EXISTS sp_ActualizarProducto;
DELIMITER //
CREATE PROCEDURE sp_ActualizarProducto(
    IN p_Id INT,
    IN p_Precio DECIMAL(18,2),
    IN p_Descripcion TEXT,
    IN p_StockMinimo INT
)
BEGIN
    UPDATE Productos
    SET Precio = p_Precio,
        Descripcion = p_Descripcion,
        StockMinimo = p_StockMinimo
    WHERE Id = p_Id AND Activo = 1;
END //
DELIMITER ;

DROP PROCEDURE IF EXISTS sp_EliminarProducto;
DELIMITER //
CREATE PROCEDURE sp_EliminarProducto(
    IN p_Id INT
)
BEGIN
    -- Soft delete
    UPDATE Productos
    SET Activo = 0
    WHERE Id = p_Id;
END //
DELIMITER ;


----------------------------------------------------
-- VENTAS (CON TRANSACCIÓN PARA DESCONTAR STOCK)
----------------------------------------------------
DROP PROCEDURE IF EXISTS sp_CrearVenta;
DELIMITER //
CREATE PROCEDURE sp_CrearVenta(
    IN p_IdUsuario INT,
    IN p_Total DECIMAL(18,2),
    OUT p_IdNuevaVenta INT
)
BEGIN
    DECLARE v_IdCorteCaja INT DEFAULT NULL;
    
    -- Buscar si hay una caja abierta
    SELECT Id INTO v_IdCorteCaja 
    FROM CortesCaja 
    WHERE Estado = 'Abierta' 
    ORDER BY Id DESC 
    LIMIT 1;

    INSERT INTO Ventas (IdUsuario, IdCorteCaja, Total)
    VALUES (p_IdUsuario, v_IdCorteCaja, p_Total);

    SET p_IdNuevaVenta = LAST_INSERT_ID();
    
    -- Si hay caja, sumar total ventas
    IF v_IdCorteCaja IS NOT NULL THEN
        UPDATE CortesCaja
        SET TotalVentas = TotalVentas + p_Total
        WHERE Id = v_IdCorteCaja;
    END IF;
END //
DELIMITER ;

DROP PROCEDURE IF EXISTS sp_AgregarDetalleVenta;
DELIMITER //
CREATE PROCEDURE sp_AgregarDetalleVenta(
    IN p_IdVenta INT,
    IN p_IdProducto INT,
    IN p_Cantidad INT,
    IN p_PrecioUnitario DECIMAL(18,2),
    IN p_Subtotal DECIMAL(18,2)
)
BEGIN
    DECLARE v_StockActual INT;
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Error al agregar detalle de venta y descontar stock.';
    END;

    START TRANSACTION;

    -- 1. Verificar si hay stock
    SELECT Stock INTO v_StockActual FROM Productos WHERE Id = p_IdProducto;

    IF v_StockActual < p_Cantidad THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Stock insuficiente para el producto.';
    ELSE
        -- 2. Insertar detalle
        INSERT INTO DetalleVentas (IdVenta, IdProducto, Cantidad, PrecioUnitario, Subtotal)
        VALUES (p_IdVenta, p_IdProducto, p_Cantidad, p_PrecioUnitario, p_Subtotal);

        -- 3. Descontar Stock
        UPDATE Productos
        SET Stock = Stock - p_Cantidad
        WHERE Id = p_IdProducto;

        COMMIT;
    END IF;
END //
DELIMITER ;

DROP PROCEDURE IF EXISTS sp_ObtenerHistorialVentas;
DELIMITER //
CREATE PROCEDURE sp_ObtenerHistorialVentas(
    IN p_FechaInicio DATETIME,
    IN p_FechaFin DATETIME,
    IN p_IdUsuario INT
)
BEGIN
    SELECT V.Id, V.IdUsuario, U.Nombre AS Cajero, V.Total, V.FechaVenta, V.Estado
    FROM Ventas V
    INNER JOIN Usuarios U ON V.IdUsuario = U.Id
    WHERE (p_IdUsuario IS NULL OR V.IdUsuario = p_IdUsuario)
      AND (p_FechaInicio IS NULL OR V.FechaVenta >= p_FechaInicio)
      AND (p_FechaFin IS NULL OR V.FechaVenta <= p_FechaFin)
    ORDER BY V.FechaVenta DESC;
END //
DELIMITER ;

DROP PROCEDURE IF EXISTS sp_ObtenerTicketVenta;
DELIMITER //
CREATE PROCEDURE sp_ObtenerTicketVenta(
    IN p_IdVenta INT
)
BEGIN
    SELECT DV.Id, DV.IdProducto, P.Nombre AS Producto, P.CodigoBarras, DV.Cantidad, DV.PrecioUnitario, DV.Subtotal
    FROM DetalleVentas DV
    INNER JOIN Productos P ON DV.IdProducto = P.Id
    WHERE DV.IdVenta = p_IdVenta;
END //
DELIMITER ;


----------------------------------------------------
-- PROVEEDORES
----------------------------------------------------
DROP PROCEDURE IF EXISTS sp_ObtenerProveedores;
DELIMITER //
CREATE PROCEDURE sp_ObtenerProveedores()
BEGIN
    SELECT Id, Nombre, NitRfc, Telefono
    FROM Proveedores
    WHERE Activo = 1;
END //
DELIMITER ;

DROP PROCEDURE IF EXISTS sp_CrearProveedor;
DELIMITER //
CREATE PROCEDURE sp_CrearProveedor(
    IN p_Nombre VARCHAR(150),
    IN p_NitRfc VARCHAR(50),
    IN p_Telefono VARCHAR(20)
)
BEGIN
    INSERT INTO Proveedores (Nombre, NitRfc, Telefono)
    VALUES (p_Nombre, p_NitRfc, p_Telefono);
    
    SELECT LAST_INSERT_ID() AS IdNuevoProveedor;
END //
DELIMITER ;

DROP PROCEDURE IF EXISTS sp_ActualizarProveedor;
DELIMITER //
CREATE PROCEDURE sp_ActualizarProveedor(
    IN p_Id INT,
    IN p_Nombre VARCHAR(150),
    IN p_NitRfc VARCHAR(50),
    IN p_Telefono VARCHAR(20)
)
BEGIN
    UPDATE Proveedores
    SET Nombre = p_Nombre,
        NitRfc = p_NitRfc,
        Telefono = p_Telefono
    WHERE Id = p_Id AND Activo = 1;
END //
DELIMITER ;


----------------------------------------------------
-- CORTE DE CAJA
----------------------------------------------------
DROP PROCEDURE IF EXISTS sp_EstadoCaja;
DELIMITER //
CREATE PROCEDURE sp_EstadoCaja()
BEGIN
    SELECT Id, IdUsuario, FechaApertura, SaldoInicial, TotalVentas, TotalEgresos, Estado
    FROM CortesCaja
    ORDER BY Id DESC
    LIMIT 1;
END //
DELIMITER ;

DROP PROCEDURE IF EXISTS sp_AbrirCaja;
DELIMITER //
CREATE PROCEDURE sp_AbrirCaja(
    IN p_IdUsuario INT,
    IN p_SaldoInicial DECIMAL(18,2)
)
BEGIN
    IF EXISTS (SELECT 1 FROM CortesCaja WHERE Estado = 'Abierta') THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Ya existe una caja abierta.';
    ELSE
        INSERT INTO CortesCaja (IdUsuario, SaldoInicial)
        VALUES (p_IdUsuario, p_SaldoInicial);
    END IF;
END //
DELIMITER ;

DROP PROCEDURE IF EXISTS sp_CerrarCaja;
DELIMITER //
CREATE PROCEDURE sp_CerrarCaja(
    IN p_Id INT
)
BEGIN
    DECLARE v_Estado VARCHAR(20);
    SELECT Estado INTO v_Estado FROM CortesCaja WHERE Id = p_Id;

    IF v_Estado <> 'Abierta' OR v_Estado IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La caja ya se encuentra cerrada o no existe.';
    ELSE
        UPDATE CortesCaja
        SET FechaCierre = NOW(),
            Estado = 'Cerrada',
            SaldoFinal = SaldoInicial + TotalVentas - TotalEgresos
        WHERE Id = p_Id;
        
        SELECT SaldoFinal FROM CortesCaja WHERE Id = p_Id;
    END IF;
END //
DELIMITER ;

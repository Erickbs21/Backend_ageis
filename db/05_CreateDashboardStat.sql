-- PROCEDIMIENTO ALMACENADO PARA DASHBOARD STATS

DROP PROCEDURE IF EXISTS sp_ObtenerDashboardStats;

DELIMITER //

CREATE PROCEDURE sp_ObtenerDashboardStats()
BEGIN
    DECLARE v_TotalVentasHoy DECIMAL(18,2) DEFAULT 0.00;
    DECLARE v_ProductosBajoStock INT DEFAULT 0;
    DECLARE v_ProveedoresActivos INT DEFAULT 0;

    -- 1. Total ventas del día de hoy
    SELECT COALESCE(SUM(Total), 0.00) INTO v_TotalVentasHoy
    FROM Ventas
    WHERE DATE(FechaVenta) = CURDATE()
      AND Estado = 'Completada';

    -- 2. Productos con stock bajo (Stock <= StockMinimo)
    SELECT COUNT(*) INTO v_ProductosBajoStock
    FROM Productos
    WHERE Activo = 1 AND Stock <= StockMinimo;

    -- 3. Total de proveedores activos
    SELECT COUNT(*) INTO v_ProveedoresActivos
    FROM Proveedores
    WHERE Activo = 1;

    -- Devolver los resultados
    SELECT 
        v_TotalVentasHoy AS VentasHoy,
        v_ProductosBajoStock AS AlertasStock,
        v_ProveedoresActivos AS ProveedoresActivos;
END //

DELIMITER ;

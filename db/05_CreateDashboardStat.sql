-- PROCEDIMIENTO ALMACENADO PARA DASHBOARD STATS

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ObtenerDashboardStats') DROP PROCEDURE sp_ObtenerDashboardStats
GO
CREATE PROCEDURE sp_ObtenerDashboardStats
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @TotalVentasHoy DECIMAL(18,2) = 0;
    DECLARE @ProductosBajoStock INT = 0;
    DECLARE @ProveedoresActivos INT = 0;

    -- 1. Total ventas del día de hoy
    SELECT @TotalVentasHoy = ISNULL(SUM(Total), 0)
    FROM Ventas
    WHERE FORMAT(FechaVenta, 'yyyy-MM-dd') = FORMAT(GETDATE(), 'yyyy-MM-dd')
      AND Estado = 'Completada';

    -- 2. Productos con stock bajo (Stock <= StockMinimo)
    SELECT @ProductosBajoStock = COUNT(*)
    FROM Productos
    WHERE Activo = 1 AND Stock <= StockMinimo;

    -- 3. Total de proveedores activos
    SELECT @ProveedoresActivos = COUNT(*)
    FROM Proveedores
    WHERE Activo = 1;

    -- Devolver los resultados
    SELECT 
        @TotalVentasHoy AS VentasHoy,
        @ProductosBajoStock AS AlertasStock,
        @ProveedoresActivos AS ProveedoresActivos;
END
GO

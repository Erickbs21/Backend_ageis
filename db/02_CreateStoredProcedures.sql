-- Script para el Procedimiento Almacenado de Autenticación

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_AutenticarUsuario')
    DROP PROCEDURE sp_AutenticarUsuario

CREATE PROCEDURE sp_AutenticarUsuario
    @Username NVARCHAR(100),
    @Password NVARCHAR(MAX), -- Idealmente se usa Hash
    @Dispositivo NVARCHAR(100) = NULL,
    @TipoDispositivo NVARCHAR(100) = NULL,
    @VersionSO NVARCHAR(50) = NULL,
    @VersionApp NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Lógica simple: Verificar Username y Password
    -- (Ajustado para el ejemplo que devuelve código 200 con el role, etc.)
    SELECT 
        Id,
        Nombre,
        Username,
        Role
    FROM Usuarios
    WHERE Username = @Username 
      AND PasswordHash = @Password 
      AND Activo = 1;
END

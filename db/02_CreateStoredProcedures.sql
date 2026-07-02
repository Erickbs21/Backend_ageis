-- Script para el Procedimiento Almacenado de Autenticación

DROP PROCEDURE IF EXISTS sp_AutenticarUsuario;

DELIMITER //

CREATE PROCEDURE sp_AutenticarUsuario(
    IN p_Username VARCHAR(100),
    IN p_Password TEXT,
    IN p_Dispositivo VARCHAR(100),
    IN p_TipoDispositivo VARCHAR(100),
    IN p_VersionSO VARCHAR(50),
    IN p_VersionApp VARCHAR(50)
)
BEGIN
    -- Lógica simple: Verificar Username y Password
    -- (Ajustado para el ejemplo que devuelve código 200 con el role, etc.)
    SELECT 
        Id,
        Nombre,
        Username,
        Role
    FROM Usuarios
    WHERE Username = p_Username 
      AND PasswordHash = p_Password 
      AND Activo = 1;
END //

DELIMITER ;

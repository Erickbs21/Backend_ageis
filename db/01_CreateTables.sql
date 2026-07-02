-- Script para crear la tabla de Usuarios

CREATE TABLE IF NOT EXISTS Usuarios (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Username VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL,
    Role VARCHAR(50) NOT NULL,
    FechaCreacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    Activo TINYINT(1) DEFAULT 1
);

-- Insertar usuario administrador por defecto para pruebas
-- NOTA: En un entorno real, la contraseña no debe estar en texto plano, sino hasheada.
-- Aquí dejaremos un hash ficticio o el password en texto plano para el propósito de este ejemplo
INSERT INTO Usuarios (Nombre, Username, PasswordHash, Role)
SELECT 'Erick Admin', 'usuario@ejemplo.com', 'tu_password', 'Admin'
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM Usuarios WHERE Username = 'usuario@ejemplo.com'
);

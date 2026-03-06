-- Script para crear la tabla de Usuarios

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Usuarios')
BEGIN
    CREATE TABLE Usuarios (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL,
        Username NVARCHAR(100) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(MAX) NOT NULL,
        Role NVARCHAR(50) NOT NULL,
        FechaCreacion DATETIME DEFAULT GETDATE(),
        Activo BIT DEFAULT 1
    );

    -- Insertar usuario administrador por defecto para pruebas
    -- NOTA: En un entorno real, la contraseña no debe estar en texto plano, sino hasheada.
    -- Aquí dejaremos un hash ficticio o el password en texto plano para el propósito de este ejemplo
    INSERT INTO Usuarios (Nombre, Username, PasswordHash, Role)
    VALUES ('Erick Admin', 'usuario@ejemplo.com', 'tu_password', 'Admin');
END
GO

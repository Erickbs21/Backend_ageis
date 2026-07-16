CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;
ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `cajas` (
    `id` int NOT NULL AUTO_INCREMENT,
    `nombre` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `estado` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_cajas` PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `categorias` (
    `id` int NOT NULL AUTO_INCREMENT,
    `nombre` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `descripcion` varchar(255) CHARACTER SET utf8mb4 NULL,
    `activo` tinyint(1) NOT NULL,
    CONSTRAINT `PK_categorias` PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `clientes` (
    `id` int NOT NULL AUTO_INCREMENT,
    `nit` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `dpi` varchar(20) CHARACTER SET utf8mb4 NULL,
    `nombre` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `direccion` varchar(255) CHARACTER SET utf8mb4 NULL,
    `telefono` varchar(50) CHARACTER SET utf8mb4 NULL,
    `correo` varchar(150) CHARACTER SET utf8mb4 NULL,
    `credito_habilitado` tinyint(1) NOT NULL,
    `limite_credito` decimal(18,2) NOT NULL,
    `activo` tinyint(1) NOT NULL,
    `fecha_creacion` datetime(6) NOT NULL,
    CONSTRAINT `PK_clientes` PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `configuraciones` (
    `id` int NOT NULL AUTO_INCREMENT,
    `clave` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `valor` longtext CHARACTER SET utf8mb4 NOT NULL,
    `descripcion` varchar(255) CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_configuraciones` PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `metodos_pago` (
    `id` int NOT NULL AUTO_INCREMENT,
    `nombre` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `activo` tinyint(1) NOT NULL,
    CONSTRAINT `PK_metodos_pago` PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `permisos` (
    `id` int NOT NULL AUTO_INCREMENT,
    `nombre` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `descripcion` varchar(255) CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_permisos` PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `proveedores` (
    `id` int NOT NULL AUTO_INCREMENT,
    `nombre` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `nit` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `telefono` varchar(50) CHARACTER SET utf8mb4 NULL,
    `correo` varchar(150) CHARACTER SET utf8mb4 NULL,
    `direccion` varchar(255) CHARACTER SET utf8mb4 NULL,
    `activo` tinyint(1) NOT NULL,
    CONSTRAINT `PK_proveedores` PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `roles` (
    `id` int NOT NULL AUTO_INCREMENT,
    `nombre` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `descripcion` varchar(255) CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_roles` PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `productos` (
    `id` int NOT NULL AUTO_INCREMENT,
    `codigo` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `codigo_barras` varchar(100) CHARACTER SET utf8mb4 NULL,
    `nombre` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `descripcion` varchar(500) CHARACTER SET utf8mb4 NULL,
    `categoria_id` int NOT NULL,
    `marca` varchar(100) CHARACTER SET utf8mb4 NULL,
    `costo` decimal(18,2) NOT NULL,
    `precio_venta` decimal(18,2) NOT NULL,
    `precio_mayoreo` decimal(18,2) NOT NULL,
    `stock_minimo` int NOT NULL,
    `stock_actual` int NOT NULL,
    `usa_codigo_barras` tinyint(1) NOT NULL,
    `activo` tinyint(1) NOT NULL,
    `fecha_creacion` datetime(6) NOT NULL,
    CONSTRAINT `PK_productos` PRIMARY KEY (`id`),
    CONSTRAINT `FK_productos_categorias_categoria_id` FOREIGN KEY (`categoria_id`) REFERENCES `categorias` (`id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `rol_permisos` (
    `id` int NOT NULL AUTO_INCREMENT,
    `rol_id` int NOT NULL,
    `permiso_id` int NOT NULL,
    CONSTRAINT `PK_rol_permisos` PRIMARY KEY (`id`),
    CONSTRAINT `FK_rol_permisos_permisos_permiso_id` FOREIGN KEY (`permiso_id`) REFERENCES `permisos` (`id`) ON DELETE CASCADE,
    CONSTRAINT `FK_rol_permisos_roles_rol_id` FOREIGN KEY (`rol_id`) REFERENCES `roles` (`id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `usuarios` (
    `id` int NOT NULL AUTO_INCREMENT,
    `nombre` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `apellido` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `correo` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `usuario` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `password_hash` longtext CHARACTER SET utf8mb4 NOT NULL,
    `rol_id` int NOT NULL,
    `activo` tinyint(1) NOT NULL,
    `ultimo_acceso` datetime(6) NULL,
    `fecha_creacion` datetime(6) NOT NULL,
    `refresh_token` varchar(500) CHARACTER SET utf8mb4 NULL,
    `refresh_token_expira` datetime(6) NULL,
    CONSTRAINT `PK_usuarios` PRIMARY KEY (`id`),
    CONSTRAINT `FK_usuarios_roles_rol_id` FOREIGN KEY (`rol_id`) REFERENCES `roles` (`id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE TABLE `auditoria` (
    `id` int NOT NULL AUTO_INCREMENT,
    `usuario_id` int NULL,
    `accion` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `tabla_afectada` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `registro_id` int NULL,
    `ip` varchar(45) CHARACTER SET utf8mb4 NULL,
    `fecha` datetime(6) NOT NULL,
    CONSTRAINT `PK_auditoria` PRIMARY KEY (`id`),
    CONSTRAINT `FK_auditoria_usuarios_usuario_id` FOREIGN KEY (`usuario_id`) REFERENCES `usuarios` (`id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `caja_aperturas` (
    `id` int NOT NULL AUTO_INCREMENT,
    `caja_id` int NOT NULL,
    `usuario_id` int NOT NULL,
    `monto_inicial` decimal(18,2) NOT NULL,
    `fecha_apertura` datetime(6) NOT NULL,
    CONSTRAINT `PK_caja_aperturas` PRIMARY KEY (`id`),
    CONSTRAINT `FK_caja_aperturas_cajas_caja_id` FOREIGN KEY (`caja_id`) REFERENCES `cajas` (`id`) ON DELETE CASCADE,
    CONSTRAINT `FK_caja_aperturas_usuarios_usuario_id` FOREIGN KEY (`usuario_id`) REFERENCES `usuarios` (`id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `caja_cierres` (
    `id` int NOT NULL AUTO_INCREMENT,
    `caja_id` int NOT NULL,
    `usuario_id` int NOT NULL,
    `monto_final` decimal(18,2) NOT NULL,
    `fecha_cierre` datetime(6) NOT NULL,
    CONSTRAINT `PK_caja_cierres` PRIMARY KEY (`id`),
    CONSTRAINT `FK_caja_cierres_cajas_caja_id` FOREIGN KEY (`caja_id`) REFERENCES `cajas` (`id`) ON DELETE CASCADE,
    CONSTRAINT `FK_caja_cierres_usuarios_usuario_id` FOREIGN KEY (`usuario_id`) REFERENCES `usuarios` (`id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `compras` (
    `id` int NOT NULL AUTO_INCREMENT,
    `proveedor_id` int NOT NULL,
    `usuario_id` int NOT NULL,
    `subtotal` decimal(18,2) NOT NULL,
    `impuestos` decimal(18,2) NOT NULL,
    `total` decimal(18,2) NOT NULL,
    `fecha_compra` datetime(6) NOT NULL,
    CONSTRAINT `PK_compras` PRIMARY KEY (`id`),
    CONSTRAINT `FK_compras_proveedores_proveedor_id` FOREIGN KEY (`proveedor_id`) REFERENCES `proveedores` (`id`) ON DELETE CASCADE,
    CONSTRAINT `FK_compras_usuarios_usuario_id` FOREIGN KEY (`usuario_id`) REFERENCES `usuarios` (`id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `movimientos_inventario` (
    `id` int NOT NULL AUTO_INCREMENT,
    `producto_id` int NOT NULL,
    `tipo_movimiento` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `cantidad` int NOT NULL,
    `existencia_anterior` int NOT NULL,
    `existencia_nueva` int NOT NULL,
    `usuario_id` int NOT NULL,
    `observacion` varchar(255) CHARACTER SET utf8mb4 NULL,
    `fecha_movimiento` datetime(6) NOT NULL,
    CONSTRAINT `PK_movimientos_inventario` PRIMARY KEY (`id`),
    CONSTRAINT `FK_movimientos_inventario_productos_producto_id` FOREIGN KEY (`producto_id`) REFERENCES `productos` (`id`) ON DELETE CASCADE,
    CONSTRAINT `FK_movimientos_inventario_usuarios_usuario_id` FOREIGN KEY (`usuario_id`) REFERENCES `usuarios` (`id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `ventas` (
    `id` int NOT NULL AUTO_INCREMENT,
    `numero_documento` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `cliente_id` int NOT NULL,
    `usuario_id` int NOT NULL,
    `metodo_pago_id` int NOT NULL,
    `nombre_cliente` varchar(150) CHARACTER SET utf8mb4 NULL,
    `tipo_documento` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `subtotal` decimal(18,2) NOT NULL,
    `descuento` decimal(18,2) NOT NULL,
    `impuestos` decimal(18,2) NOT NULL,
    `total` decimal(18,2) NOT NULL,
    `vuelto` decimal(18,2) NOT NULL,
    `estado` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `fecha_venta` datetime(6) NOT NULL,
    CONSTRAINT `PK_ventas` PRIMARY KEY (`id`),
    CONSTRAINT `FK_ventas_clientes_cliente_id` FOREIGN KEY (`cliente_id`) REFERENCES `clientes` (`id`) ON DELETE CASCADE,
    CONSTRAINT `FK_ventas_metodos_pago_metodo_pago_id` FOREIGN KEY (`metodo_pago_id`) REFERENCES `metodos_pago` (`id`) ON DELETE CASCADE,
    CONSTRAINT `FK_ventas_usuarios_usuario_id` FOREIGN KEY (`usuario_id`) REFERENCES `usuarios` (`id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `compras_detalle` (
    `id` int NOT NULL AUTO_INCREMENT,
    `compra_id` int NOT NULL,
    `producto_id` int NOT NULL,
    `cantidad` int NOT NULL,
    `costo_unitario` decimal(18,2) NOT NULL,
    `subtotal` decimal(18,2) NOT NULL,
    CONSTRAINT `PK_compras_detalle` PRIMARY KEY (`id`),
    CONSTRAINT `FK_compras_detalle_compras_compra_id` FOREIGN KEY (`compra_id`) REFERENCES `compras` (`id`) ON DELETE CASCADE,
    CONSTRAINT `FK_compras_detalle_productos_producto_id` FOREIGN KEY (`producto_id`) REFERENCES `productos` (`id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `devoluciones` (
    `id` int NOT NULL AUTO_INCREMENT,
    `venta_id` int NOT NULL,
    `usuario_id` int NOT NULL,
    `motivo` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `fecha` datetime(6) NOT NULL,
    CONSTRAINT `PK_devoluciones` PRIMARY KEY (`id`),
    CONSTRAINT `FK_devoluciones_usuarios_usuario_id` FOREIGN KEY (`usuario_id`) REFERENCES `usuarios` (`id`) ON DELETE CASCADE,
    CONSTRAINT `FK_devoluciones_ventas_venta_id` FOREIGN KEY (`venta_id`) REFERENCES `ventas` (`id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `facturas` (
    `id` int NOT NULL AUTO_INCREMENT,
    `venta_id` int NOT NULL,
    `serie` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `numero` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `uuid` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `estado` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `fecha_emision` datetime(6) NOT NULL,
    CONSTRAINT `PK_facturas` PRIMARY KEY (`id`),
    CONSTRAINT `FK_facturas_ventas_venta_id` FOREIGN KEY (`venta_id`) REFERENCES `ventas` (`id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `venta_pagos` (
    `id` int NOT NULL AUTO_INCREMENT,
    `venta_id` int NOT NULL,
    `metodo_pago_id` int NOT NULL,
    `monto` decimal(18,2) NOT NULL,
    `fecha_pago` datetime(6) NOT NULL,
    CONSTRAINT `PK_venta_pagos` PRIMARY KEY (`id`),
    CONSTRAINT `FK_venta_pagos_metodos_pago_metodo_pago_id` FOREIGN KEY (`metodo_pago_id`) REFERENCES `metodos_pago` (`id`) ON DELETE CASCADE,
    CONSTRAINT `FK_venta_pagos_ventas_venta_id` FOREIGN KEY (`venta_id`) REFERENCES `ventas` (`id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `ventas_detalle` (
    `id` int NOT NULL AUTO_INCREMENT,
    `venta_id` int NOT NULL,
    `producto_id` int NOT NULL,
    `cantidad` int NOT NULL,
    `precio_unitario` decimal(18,2) NOT NULL,
    `descuento` decimal(18,2) NOT NULL,
    `subtotal` decimal(18,2) NOT NULL,
    CONSTRAINT `PK_ventas_detalle` PRIMARY KEY (`id`),
    CONSTRAINT `FK_ventas_detalle_productos_producto_id` FOREIGN KEY (`producto_id`) REFERENCES `productos` (`id`) ON DELETE CASCADE,
    CONSTRAINT `FK_ventas_detalle_ventas_venta_id` FOREIGN KEY (`venta_id`) REFERENCES `ventas` (`id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `devoluciones_detalle` (
    `id` int NOT NULL AUTO_INCREMENT,
    `devolucion_id` int NOT NULL,
    `producto_id` int NOT NULL,
    `cantidad` int NOT NULL,
    CONSTRAINT `PK_devoluciones_detalle` PRIMARY KEY (`id`),
    CONSTRAINT `FK_devoluciones_detalle_devoluciones_devolucion_id` FOREIGN KEY (`devolucion_id`) REFERENCES `devoluciones` (`id`) ON DELETE CASCADE,
    CONSTRAINT `FK_devoluciones_detalle_productos_producto_id` FOREIGN KEY (`producto_id`) REFERENCES `productos` (`id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

INSERT INTO `cajas` (`id`, `estado`, `nombre`)
VALUES (1, 'Cerrada', 'Caja Principal');

INSERT INTO `categorias` (`id`, `activo`, `descripcion`, `nombre`)
VALUES (1, TRUE, 'Categoría general de productos', 'General');

INSERT INTO `clientes` (`id`, `activo`, `correo`, `credito_habilitado`, `direccion`, `dpi`, `fecha_creacion`, `limite_credito`, `nit`, `nombre`, `telefono`)
VALUES (1, TRUE, NULL, FALSE, 'Ciudad', NULL, TIMESTAMP '2026-01-01 00:00:00', 0.0, 'CF', 'Consumidor Final', NULL);

INSERT INTO `metodos_pago` (`id`, `activo`, `nombre`)
VALUES (1, TRUE, 'Efectivo'),
(2, TRUE, 'Tarjeta'),
(3, TRUE, 'Transferencia'),
(4, TRUE, 'Cheque'),
(5, TRUE, 'Crédito'),
(6, TRUE, 'Mixto');

INSERT INTO `permisos` (`id`, `descripcion`, `nombre`)
VALUES (1, 'Crear, editar y desactivar usuarios', 'GestionUsuarios'),
(2, 'Gestionar roles y permisos', 'GestionRoles'),
(3, 'Gestionar productos y categorías', 'GestionProductos'),
(4, 'Gestionar clientes y límites de crédito', 'GestionClientes'),
(5, 'Registrar ventas en el sistema', 'CrearVentas'),
(6, 'Anular ventas registradas', 'AnularVentas'),
(7, 'Registrar movimientos y ver stock', 'GestionInventario'),
(8, 'Abrir y cerrar caja, ver reportes de cortes', 'GestionCaja'),
(9, 'Ver reportes y dashboard administrativo', 'VerReportes'),
(10, 'Configurar parámetros del sistema', 'GestionConfiguracion');

INSERT INTO `roles` (`id`, `descripcion`, `nombre`)
VALUES (1, 'Gestión total del sistema, configuración, inventario, reportes, usuarios y facturación', 'Administrador'),
(2, 'Ventas, inventario, reportes y clientes', 'Supervisor'),
(3, 'Crear ventas, consultar productos y clientes', 'Vendedor'),
(4, 'Cobros, apertura y cierre de caja', 'Caja');

INSERT INTO `rol_permisos` (`id`, `permiso_id`, `rol_id`)
VALUES (1, 1, 1),
(2, 2, 1),
(3, 3, 1),
(4, 4, 1),
(5, 5, 1),
(6, 6, 1),
(7, 7, 1),
(8, 8, 1),
(9, 9, 1),
(10, 10, 1),
(11, 3, 2),
(12, 4, 2),
(13, 5, 2),
(14, 7, 2),
(15, 9, 2),
(16, 5, 3),
(17, 5, 4),
(18, 8, 4);

INSERT INTO `usuarios` (`id`, `activo`, `apellido`, `correo`, `fecha_creacion`, `nombre`, `usuario`, `password_hash`, `refresh_token`, `refresh_token_expira`, `rol_id`, `ultimo_acceso`)
VALUES (1, TRUE, 'Aegis', 'admin@aegispos.com', TIMESTAMP '2026-01-01 00:00:00', 'Administrador', 'admin', '$2a$11$t1CAJzmIaY2TGRiq4iyQMOAtsMBzI5sOHPV3L2Ne8G8rKXhup7p0u', NULL, NULL, 1, NULL);

CREATE INDEX `IX_auditoria_usuario_id` ON `auditoria` (`usuario_id`);

CREATE INDEX `IX_caja_aperturas_caja_id` ON `caja_aperturas` (`caja_id`);

CREATE INDEX `IX_caja_aperturas_usuario_id` ON `caja_aperturas` (`usuario_id`);

CREATE INDEX `IX_caja_cierres_caja_id` ON `caja_cierres` (`caja_id`);

CREATE INDEX `IX_caja_cierres_usuario_id` ON `caja_cierres` (`usuario_id`);

CREATE INDEX `IX_compras_proveedor_id` ON `compras` (`proveedor_id`);

CREATE INDEX `IX_compras_usuario_id` ON `compras` (`usuario_id`);

CREATE INDEX `IX_compras_detalle_compra_id` ON `compras_detalle` (`compra_id`);

CREATE INDEX `IX_compras_detalle_producto_id` ON `compras_detalle` (`producto_id`);

CREATE INDEX `IX_devoluciones_usuario_id` ON `devoluciones` (`usuario_id`);

CREATE INDEX `IX_devoluciones_venta_id` ON `devoluciones` (`venta_id`);

CREATE INDEX `IX_devoluciones_detalle_devolucion_id` ON `devoluciones_detalle` (`devolucion_id`);

CREATE INDEX `IX_devoluciones_detalle_producto_id` ON `devoluciones_detalle` (`producto_id`);

CREATE INDEX `IX_facturas_venta_id` ON `facturas` (`venta_id`);

CREATE INDEX `IX_movimientos_inventario_producto_id` ON `movimientos_inventario` (`producto_id`);

CREATE INDEX `IX_movimientos_inventario_usuario_id` ON `movimientos_inventario` (`usuario_id`);

CREATE INDEX `IX_productos_categoria_id` ON `productos` (`categoria_id`);

CREATE INDEX `IX_rol_permisos_permiso_id` ON `rol_permisos` (`permiso_id`);

CREATE INDEX `IX_rol_permisos_rol_id` ON `rol_permisos` (`rol_id`);

CREATE INDEX `IX_usuarios_rol_id` ON `usuarios` (`rol_id`);

CREATE INDEX `IX_venta_pagos_metodo_pago_id` ON `venta_pagos` (`metodo_pago_id`);

CREATE INDEX `IX_venta_pagos_venta_id` ON `venta_pagos` (`venta_id`);

CREATE INDEX `IX_ventas_cliente_id` ON `ventas` (`cliente_id`);

CREATE INDEX `IX_ventas_metodo_pago_id` ON `ventas` (`metodo_pago_id`);

CREATE INDEX `IX_ventas_usuario_id` ON `ventas` (`usuario_id`);

CREATE INDEX `IX_ventas_detalle_producto_id` ON `ventas_detalle` (`producto_id`);

CREATE INDEX `IX_ventas_detalle_venta_id` ON `ventas_detalle` (`venta_id`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260716041152_InitialCreate', '9.0.0');

COMMIT;


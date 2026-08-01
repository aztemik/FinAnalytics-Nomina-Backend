USE NominaDB;
GO

DELETE FROM detalle_recibo;
DELETE FROM recibos_nomina;
DELETE FROM periodos_nomina;
DELETE FROM empleados;
DELETE FROM usuarios WHERE username <> 'admin';
GO

-- Reinicia los contadores IDENTITY para que las pruebas nuevas empiecen en 1
DBCC CHECKIDENT ('detalle_recibo', RESEED, 0);
DBCC CHECKIDENT ('recibos_nomina', RESEED, 0);
DBCC CHECKIDENT ('periodos_nomina', RESEED, 0);
DBCC CHECKIDENT ('empleados', RESEED, 0);

DECLARE @maxIdUsuario INT;
SELECT @maxIdUsuario = MAX(id) FROM usuarios;
DBCC CHECKIDENT ('usuarios', RESEED, @maxIdUsuario);
GO

-- Restaura parametros_nomina a los valores originales del seed (bd/NominaDB.sql SS3.2),
-- por si se modificaron durante pruebas (ej. demostracion de "cambio en vivo" del ISR).
UPDATE parametros_nomina SET valor = 0.10000 WHERE clave = 'ISR_TASA';
UPDATE parametros_nomina SET valor = 0.02375 WHERE clave = 'IMSS_OBRERO';
UPDATE parametros_nomina SET valor = 0.20400 WHERE clave = 'IMSS_PATRONAL';
UPDATE parametros_nomina SET valor = 0.05000 WHERE clave = 'INFONAVIT';
UPDATE parametros_nomina SET valor = 0.02000 WHERE clave = 'SAR';
UPDATE parametros_nomina SET valor = 0.03000 WHERE clave = 'ISN';
UPDATE parametros_nomina SET valor = 0.10000 WHERE clave = 'RET_ISR_HON';
UPDATE parametros_nomina SET valor = 0.10667 WHERE clave = 'RET_IVA_HON';
GO
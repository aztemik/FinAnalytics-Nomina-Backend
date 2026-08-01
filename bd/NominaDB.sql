-- ============================================================
-- NominaDB — FinAnalytics OS · Modulo de Nomina y Seguridad
-- Producto 03 · Desarrollo Web Integral · UTP
-- Ejecutar en SQL Server Management Studio (SSMS).
-- ============================================================

CREATE DATABASE NominaDB;
GO
USE NominaDB;
GO

CREATE TABLE roles (
    id          INT IDENTITY(1,1) PRIMARY KEY,
    nombre      NVARCHAR(30)  NOT NULL UNIQUE,   -- ADMIN | RH | FINANZAS | EMPLEADO
    descripcion NVARCHAR(150) NULL
);

CREATE TABLE usuarios (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    username        NVARCHAR(50)  NOT NULL UNIQUE,
    password_hash   NVARCHAR(255) NOT NULL,
    nombre_completo NVARCHAR(120) NOT NULL,
    rol_id          INT NOT NULL FOREIGN KEY REFERENCES roles(id),
    activo          BIT NOT NULL DEFAULT 1,
    fecha_creacion  DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE parametros_nomina (
    id          INT IDENTITY(1,1) PRIMARY KEY,
    clave       NVARCHAR(40)  NOT NULL UNIQUE,
    descripcion NVARCHAR(150) NOT NULL,
    valor       DECIMAL(8,5)  NOT NULL
);

CREATE TABLE empleados (
    id                INT IDENTITY(1,1) PRIMARY KEY,
    numero_empleado   NVARCHAR(20)  NOT NULL UNIQUE,
    nombre            NVARCHAR(80)  NOT NULL,
    apellidos         NVARCHAR(80)  NOT NULL,
    rfc               NVARCHAR(13)  NOT NULL,
    puesto            NVARCHAR(80)  NULL,
    departamento      NVARCHAR(80)  NULL,
    tipo_contratacion NVARCHAR(15)  NOT NULL,            -- NOMINA | HONORARIOS
    moneda            NVARCHAR(3)   NOT NULL DEFAULT 'MXN',  -- MXN | USD
    salario_mensual   DECIMAL(12,2) NOT NULL,
    fecha_ingreso     DATE NOT NULL,
    usuario_id        INT NULL FOREIGN KEY REFERENCES usuarios(id),
    activo            BIT NOT NULL DEFAULT 1
);

CREATE TABLE periodos_nomina (
    id                   INT IDENTITY(1,1) PRIMARY KEY,
    descripcion          NVARCHAR(100) NOT NULL,
    fecha_inicio         DATE NOT NULL,
    fecha_fin            DATE NOT NULL,
    estado               NVARCHAR(15)  NOT NULL DEFAULT 'BORRADOR',  -- BORRADOR | APROBADO
    tipo_cambio_usd      DECIMAL(10,4) NULL,
    fuente_tipo_cambio   NVARCHAR(20)  NULL,             -- API | CACHE | MANUAL
    total_percepciones   DECIMAL(14,2) NOT NULL DEFAULT 0,
    total_deducciones    DECIMAL(14,2) NOT NULL DEFAULT 0,
    total_neto           DECIMAL(14,2) NOT NULL DEFAULT 0,
    total_carga_patronal DECIMAL(14,2) NOT NULL DEFAULT 0,
    creado_por           INT NULL FOREIGN KEY REFERENCES usuarios(id),
    fecha_creacion       DATETIME NOT NULL DEFAULT GETDATE(),
    aprobado_por         INT NULL FOREIGN KEY REFERENCES usuarios(id),
    fecha_aprobacion     DATETIME NULL
);

CREATE TABLE recibos_nomina (
    id                 INT IDENTITY(1,1) PRIMARY KEY,
    periodo_id         INT NOT NULL FOREIGN KEY REFERENCES periodos_nomina(id),
    empleado_id        INT NOT NULL FOREIGN KEY REFERENCES empleados(id),
    sueldo_base        DECIMAL(12,2) NOT NULL,
    total_percepciones DECIMAL(12,2) NOT NULL,
    total_deducciones  DECIMAL(12,2) NOT NULL,
    neto_pagar         DECIMAL(12,2) NOT NULL,
    carga_patronal     DECIMAL(12,2) NOT NULL,
    CONSTRAINT UQ_recibo UNIQUE (periodo_id, empleado_id)
);

CREATE TABLE detalle_recibo (
    id        INT IDENTITY(1,1) PRIMARY KEY,
    recibo_id INT NOT NULL FOREIGN KEY REFERENCES recibos_nomina(id),
    concepto  NVARCHAR(60)  NOT NULL,
    tipo      NVARCHAR(15)  NOT NULL,          -- PERCEPCION | DEDUCCION | PATRONAL
    monto     DECIMAL(12,2) NOT NULL
);
GO

INSERT INTO roles (nombre, descripcion) VALUES
 ('ADMIN',    'Administra usuarios y parametros del sistema'),
 ('RH',       'Administra empleados y ejecuta el calculo de nomina'),
 ('FINANZAS', 'Consulta, analiza y aprueba periodos de nomina'),
 ('EMPLEADO', 'Consulta unicamente sus propios recibos');

INSERT INTO parametros_nomina (clave, descripcion, valor) VALUES
 ('ISR_TASA',      'Tasa de ISR retenido a empleados (simplificada)', 0.10000),
 ('IMSS_OBRERO',   'Cuota obrera IMSS',                              0.02375),
 ('IMSS_PATRONAL', 'Cuota patronal IMSS',                            0.20400),
 ('INFONAVIT',     'Aportacion patronal INFONAVIT',                  0.05000),
 ('SAR',           'Aportacion patronal SAR / Retiro',               0.02000),
 ('ISN',           'Impuesto Sobre Nomina estatal (Puebla)',         0.03000),
 ('RET_ISR_HON',   'Retencion de ISR sobre honorarios',              0.10000),
 ('RET_IVA_HON',   'Retencion de IVA sobre honorarios',              0.10667);

-- Usuario administrador inicial.
-- Contrasena en claro: Admin123  (hash BCrypt generado en BE-06)
INSERT INTO usuarios (username, password_hash, nombre_completo, rol_id)
VALUES ('admin', '$2a$11$/BfOEEurECpBxC69BmY6WeZ47.0qioo6JBxt9sOuQtaVuVhohjmtq', 'Administrador del Sistema', 1);
GO

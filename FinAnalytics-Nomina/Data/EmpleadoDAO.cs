using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using FinAnalytics_Nomina.Models;
using FinAnalytics_Nomina.Models.DTO;

namespace FinAnalytics_Nomina.Data
{
    public static class EmpleadoDAO
    {
        // Usado por AuthController (BE-16) para armar el claim empleadoId del login.
        public static int? ObtenerIdPorUsuarioId(int usuarioId)
        {
            using (var conexion = ConexionBD.Obtener())
            using (var cmd = new SqlCommand("SELECT id FROM empleados WHERE usuario_id = @usuarioId", conexion))
            {
                cmd.Parameters.Add("@usuarioId", SqlDbType.Int).Value = usuarioId;
                conexion.Open();
                var resultado = cmd.ExecuteScalar();
                return resultado == null ? (int?)null : (int)resultado;
            }
        }

        // El esquema no marca rfc como UNIQUE (solo numero_empleado); la unicidad del
        // RFC es una regla de negocio que vive aqui, en el DAO.
        public static bool ExisteRfc(string rfc, int? idExcluir = null)
        {
            using (var conexion = ConexionBD.Obtener())
            using (var cmd = new SqlCommand(
                "SELECT COUNT(1) FROM empleados WHERE rfc = @rfc AND (@idExcluir IS NULL OR id <> @idExcluir)", conexion))
            {
                cmd.Parameters.Add("@rfc", SqlDbType.NVarChar, 13).Value = rfc;
                cmd.Parameters.Add("@idExcluir", SqlDbType.Int).Value = (object)idExcluir ?? DBNull.Value;
                conexion.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        public static bool ExisteNumeroEmpleado(string numeroEmpleado, int? idExcluir = null)
        {
            using (var conexion = ConexionBD.Obtener())
            using (var cmd = new SqlCommand(
                "SELECT COUNT(1) FROM empleados WHERE numero_empleado = @numeroEmpleado AND (@idExcluir IS NULL OR id <> @idExcluir)", conexion))
            {
                cmd.Parameters.Add("@numeroEmpleado", SqlDbType.NVarChar, 20).Value = numeroEmpleado;
                cmd.Parameters.Add("@idExcluir", SqlDbType.Int).Value = (object)idExcluir ?? DBNull.Value;
                conexion.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        public static List<EmpleadoDTO> ObtenerTodos()
        {
            var lista = new List<EmpleadoDTO>();

            using (var conexion = ConexionBD.Obtener())
            using (var cmd = new SqlCommand(
                "SELECT id, numero_empleado, nombre, apellidos, rfc, puesto, departamento, " +
                "tipo_contratacion, moneda, salario_mensual, fecha_ingreso, usuario_id, activo " +
                "FROM empleados ORDER BY nombre, apellidos", conexion))
            {
                conexion.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(MapearDTO(reader));
                    }
                }
            }

            return lista;
        }

        public static EmpleadoDTO ObtenerPorId(int id)
        {
            using (var conexion = ConexionBD.Obtener())
            using (var cmd = new SqlCommand(
                "SELECT id, numero_empleado, nombre, apellidos, rfc, puesto, departamento, " +
                "tipo_contratacion, moneda, salario_mensual, fecha_ingreso, usuario_id, activo " +
                "FROM empleados WHERE id = @id", conexion))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                conexion.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    return reader.Read() ? MapearDTO(reader) : null;
                }
            }
        }

        public static int Crear(Empleado empleado)
        {
            using (var conexion = ConexionBD.Obtener())
            using (var cmd = new SqlCommand(
                "INSERT INTO empleados (numero_empleado, nombre, apellidos, rfc, puesto, departamento, " +
                "tipo_contratacion, moneda, salario_mensual, fecha_ingreso, usuario_id, activo) " +
                "VALUES (@numeroEmpleado, @nombre, @apellidos, @rfc, @puesto, @departamento, " +
                "@tipoContratacion, @moneda, @salarioMensual, @fechaIngreso, @usuarioId, @activo); " +
                "SELECT CAST(SCOPE_IDENTITY() AS INT);", conexion))
            {
                AgregarParametros(cmd, empleado);
                cmd.Parameters.Add("@activo", SqlDbType.Bit).Value = empleado.Activo;

                conexion.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public static void Actualizar(Empleado empleado)
        {
            using (var conexion = ConexionBD.Obtener())
            using (var cmd = new SqlCommand(
                "UPDATE empleados SET numero_empleado = @numeroEmpleado, nombre = @nombre, apellidos = @apellidos, " +
                "rfc = @rfc, puesto = @puesto, departamento = @departamento, tipo_contratacion = @tipoContratacion, " +
                "moneda = @moneda, salario_mensual = @salarioMensual, fecha_ingreso = @fechaIngreso, " +
                "usuario_id = @usuarioId WHERE id = @id", conexion))
            {
                AgregarParametros(cmd, empleado);
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = empleado.Id;

                conexion.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Baja logica: conserva la integridad historica de los recibos ya calculados.
        // Nunca se hace DELETE fisico sobre un empleado.
        public static void Desactivar(int id)
        {
            using (var conexion = ConexionBD.Obtener())
            using (var cmd = new SqlCommand("UPDATE empleados SET activo = 0 WHERE id = @id", conexion))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                conexion.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private static void AgregarParametros(SqlCommand cmd, Empleado empleado)
        {
            cmd.Parameters.Add("@numeroEmpleado", SqlDbType.NVarChar, 20).Value = empleado.NumeroEmpleado;
            cmd.Parameters.Add("@nombre", SqlDbType.NVarChar, 80).Value = empleado.Nombre;
            cmd.Parameters.Add("@apellidos", SqlDbType.NVarChar, 80).Value = empleado.Apellidos;
            cmd.Parameters.Add("@rfc", SqlDbType.NVarChar, 13).Value = empleado.Rfc;
            cmd.Parameters.Add("@puesto", SqlDbType.NVarChar, 80).Value = (object)empleado.Puesto ?? DBNull.Value;
            cmd.Parameters.Add("@departamento", SqlDbType.NVarChar, 80).Value = (object)empleado.Departamento ?? DBNull.Value;
            cmd.Parameters.Add("@tipoContratacion", SqlDbType.NVarChar, 15).Value = empleado.TipoContratacion;
            cmd.Parameters.Add("@moneda", SqlDbType.NVarChar, 3).Value = empleado.Moneda;
            cmd.Parameters.Add("@salarioMensual", SqlDbType.Decimal).Value = empleado.SalarioMensual;
            cmd.Parameters.Add("@fechaIngreso", SqlDbType.Date).Value = empleado.FechaIngreso;
            cmd.Parameters.Add("@usuarioId", SqlDbType.Int).Value = (object)empleado.UsuarioId ?? DBNull.Value;
        }

        private static EmpleadoDTO MapearDTO(SqlDataReader reader)
        {
            return new EmpleadoDTO
            {
                Id = (int)reader["id"],
                NumeroEmpleado = (string)reader["numero_empleado"],
                Nombre = (string)reader["nombre"],
                Apellidos = (string)reader["apellidos"],
                Rfc = (string)reader["rfc"],
                Puesto = reader["puesto"] as string,
                Departamento = reader["departamento"] as string,
                TipoContratacion = (string)reader["tipo_contratacion"],
                Moneda = (string)reader["moneda"],
                SalarioMensual = (decimal)reader["salario_mensual"],
                FechaIngreso = (DateTime)reader["fecha_ingreso"],
                UsuarioId = reader["usuario_id"] == DBNull.Value ? (int?)null : (int)reader["usuario_id"],
                Activo = (bool)reader["activo"]
            };
        }
    }
}

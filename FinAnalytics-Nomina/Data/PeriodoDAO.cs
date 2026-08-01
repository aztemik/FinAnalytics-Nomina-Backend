using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using FinAnalytics_Nomina.Models;
using FinAnalytics_Nomina.Models.DTO;

namespace FinAnalytics_Nomina.Data
{
    public static class PeriodoDAO
    {
        public static List<PeriodoDTO> ObtenerTodos()
        {
            var lista = new List<PeriodoDTO>();

            using (var conexion = ConexionBD.Obtener())
            using (var cmd = new SqlCommand(
                "SELECT id, descripcion, fecha_inicio, fecha_fin, estado, tipo_cambio_usd, fuente_tipo_cambio, " +
                "total_percepciones, total_deducciones, total_neto, total_carga_patronal, fecha_creacion, fecha_aprobacion " +
                "FROM periodos_nomina ORDER BY fecha_inicio DESC", conexion))
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

        public static PeriodoDTO ObtenerPorId(int id)
        {
            using (var conexion = ConexionBD.Obtener())
            using (var cmd = new SqlCommand(
                "SELECT id, descripcion, fecha_inicio, fecha_fin, estado, tipo_cambio_usd, fuente_tipo_cambio, " +
                "total_percepciones, total_deducciones, total_neto, total_carga_patronal, fecha_creacion, fecha_aprobacion " +
                "FROM periodos_nomina WHERE id = @id", conexion))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                conexion.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    return reader.Read() ? MapearDTO(reader) : null;
                }
            }
        }

        // El estado nace en BORRADOR por el DEFAULT de la columna; el cliente nunca lo fija.
        public static int Crear(PeriodoNomina periodo)
        {
            using (var conexion = ConexionBD.Obtener())
            using (var cmd = new SqlCommand(
                "INSERT INTO periodos_nomina (descripcion, fecha_inicio, fecha_fin, creado_por) " +
                "VALUES (@descripcion, @fechaInicio, @fechaFin, @creadoPor); " +
                "SELECT CAST(SCOPE_IDENTITY() AS INT);", conexion))
            {
                cmd.Parameters.Add("@descripcion", SqlDbType.NVarChar, 100).Value = periodo.Descripcion;
                cmd.Parameters.Add("@fechaInicio", SqlDbType.Date).Value = periodo.FechaInicio;
                cmd.Parameters.Add("@fechaFin", SqlDbType.Date).Value = periodo.FechaFin;
                cmd.Parameters.Add("@creadoPor", SqlDbType.Int).Value = (object)periodo.CreadoPor ?? DBNull.Value;

                conexion.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        // Solo descripcion y fechas: el controlador (BE-28) valida que el periodo este en
        // BORRADOR antes de llamar esto; el DAO no repite esa regla de negocio.
        public static void Actualizar(PeriodoNomina periodo)
        {
            using (var conexion = ConexionBD.Obtener())
            using (var cmd = new SqlCommand(
                "UPDATE periodos_nomina SET descripcion = @descripcion, fecha_inicio = @fechaInicio, fecha_fin = @fechaFin " +
                "WHERE id = @id", conexion))
            {
                cmd.Parameters.Add("@descripcion", SqlDbType.NVarChar, 100).Value = periodo.Descripcion;
                cmd.Parameters.Add("@fechaInicio", SqlDbType.Date).Value = periodo.FechaInicio;
                cmd.Parameters.Add("@fechaFin", SqlDbType.Date).Value = periodo.FechaFin;
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = periodo.Id;

                conexion.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Elimina en cascada (detalle_recibo -> recibos_nomina -> periodo): un periodo en
        // BORRADOR puede ya tener recibos si RH lo calculo antes de decidir borrarlo, y no
        // hay ON DELETE CASCADE en el esquema.
        public static void Eliminar(int id)
        {
            using (var conexion = ConexionBD.Obtener())
            using (var cmd = new SqlCommand(
                "DELETE FROM detalle_recibo WHERE recibo_id IN (SELECT id FROM recibos_nomina WHERE periodo_id = @id); " +
                "DELETE FROM recibos_nomina WHERE periodo_id = @id; " +
                "DELETE FROM periodos_nomina WHERE id = @id;", conexion))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                conexion.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Congela en el periodo los totales y el tipo de cambio usados al calcular.
        public static void ActualizarResultadoCalculo(int id, decimal totalPercepciones, decimal totalDeducciones,
            decimal totalNeto, decimal totalCargaPatronal, decimal? tipoCambioUsd, string fuenteTipoCambio)
        {
            using (var conexion = ConexionBD.Obtener())
            using (var cmd = new SqlCommand(
                "UPDATE periodos_nomina SET total_percepciones = @totalPercepciones, total_deducciones = @totalDeducciones, " +
                "total_neto = @totalNeto, total_carga_patronal = @totalCargaPatronal, tipo_cambio_usd = @tipoCambioUsd, " +
                "fuente_tipo_cambio = @fuenteTipoCambio WHERE id = @id", conexion))
            {
                cmd.Parameters.Add("@totalPercepciones", SqlDbType.Decimal).Value = totalPercepciones;
                cmd.Parameters.Add("@totalDeducciones", SqlDbType.Decimal).Value = totalDeducciones;
                cmd.Parameters.Add("@totalNeto", SqlDbType.Decimal).Value = totalNeto;
                cmd.Parameters.Add("@totalCargaPatronal", SqlDbType.Decimal).Value = totalCargaPatronal;
                cmd.Parameters.Add("@tipoCambioUsd", SqlDbType.Decimal).Value = (object)tipoCambioUsd ?? DBNull.Value;
                cmd.Parameters.Add("@fuenteTipoCambio", SqlDbType.NVarChar, 20).Value = (object)fuenteTipoCambio ?? DBNull.Value;
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

                conexion.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static void Aprobar(int id, int aprobadoPor)
        {
            using (var conexion = ConexionBD.Obtener())
            using (var cmd = new SqlCommand(
                "UPDATE periodos_nomina SET estado = 'APROBADO', aprobado_por = @aprobadoPor, fecha_aprobacion = GETDATE() " +
                "WHERE id = @id", conexion))
            {
                cmd.Parameters.Add("@aprobadoPor", SqlDbType.Int).Value = aprobadoPor;
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

                conexion.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Usado por TipoCambioService (BE-23) para la cascada de fallback.
        public static (decimal Valor, DateTime Fecha)? ObtenerUltimoTipoCambioCache()
        {
            using (var conexion = ConexionBD.Obtener())
            using (var cmd = new SqlCommand(
                "SELECT TOP 1 tipo_cambio_usd, fecha_creacion FROM periodos_nomina " +
                "WHERE tipo_cambio_usd IS NOT NULL ORDER BY fecha_creacion DESC", conexion))
            {
                conexion.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    return ((decimal)reader["tipo_cambio_usd"], (DateTime)reader["fecha_creacion"]);
                }
            }
        }

        private static PeriodoDTO MapearDTO(SqlDataReader reader)
        {
            return new PeriodoDTO
            {
                Id = (int)reader["id"],
                Descripcion = (string)reader["descripcion"],
                FechaInicio = (DateTime)reader["fecha_inicio"],
                FechaFin = (DateTime)reader["fecha_fin"],
                Estado = (string)reader["estado"],
                TipoCambioUsd = reader["tipo_cambio_usd"] == DBNull.Value ? (decimal?)null : (decimal)reader["tipo_cambio_usd"],
                FuenteTipoCambio = reader["fuente_tipo_cambio"] as string,
                TotalPercepciones = (decimal)reader["total_percepciones"],
                TotalDeducciones = (decimal)reader["total_deducciones"],
                TotalNeto = (decimal)reader["total_neto"],
                TotalCargaPatronal = (decimal)reader["total_carga_patronal"],
                FechaCreacion = (DateTime)reader["fecha_creacion"],
                FechaAprobacion = reader["fecha_aprobacion"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["fecha_aprobacion"]
            };
        }
    }
}

using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using FinAnalytics_Nomina.Models;
using FinAnalytics_Nomina.Models.DTO;

namespace FinAnalytics_Nomina.Data
{
    public static class ReciboDAO
    {
        // Usado antes de recalcular un periodo: borra sus recibos previos para regenerarlos.
        public static void EliminarPorPeriodo(int periodoId)
        {
            using (var conexion = ConexionBD.Obtener())
            using (var cmd = new SqlCommand(
                "DELETE FROM detalle_recibo WHERE recibo_id IN (SELECT id FROM recibos_nomina WHERE periodo_id = @periodoId); " +
                "DELETE FROM recibos_nomina WHERE periodo_id = @periodoId;", conexion))
            {
                cmd.Parameters.Add("@periodoId", SqlDbType.Int).Value = periodoId;
                conexion.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Inserta cada recibo y su detalle. Sin SqlTransaction: envolverlo es BE-30, un
        // CHECKPOINT que todavia no se aprueba (ver PLAN_BACKEND.md §9).
        public static void GuardarRecibos(int periodoId, List<(ReciboNomina Recibo, List<DetalleRecibo> Detalle)> recibos)
        {
            using (var conexion = ConexionBD.Obtener())
            {
                conexion.Open();

                foreach (var item in recibos)
                {
                    int reciboId = InsertarRecibo(conexion, periodoId, item.Recibo);

                    foreach (var detalle in item.Detalle)
                    {
                        InsertarDetalle(conexion, reciboId, detalle);
                    }
                }
            }
        }

        public static List<ReciboDTO> ObtenerPorPeriodo(int periodoId)
        {
            var lista = new List<ReciboDTO>();

            using (var conexion = ConexionBD.Obtener())
            using (var cmd = new SqlCommand(ConsultaBase() + "WHERE r.periodo_id = @periodoId ORDER BY e.nombre, e.apellidos", conexion))
            {
                cmd.Parameters.Add("@periodoId", SqlDbType.Int).Value = periodoId;
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

        // Solo recibos de periodos APROBADOs: el empleado no debe ver un borrador a medio calcular.
        public static List<ReciboDTO> ObtenerPorEmpleado(int empleadoId)
        {
            var lista = new List<ReciboDTO>();

            using (var conexion = ConexionBD.Obtener())
            using (var cmd = new SqlCommand(
                ConsultaBase() + "JOIN periodos_nomina p ON p.id = r.periodo_id " +
                "WHERE r.empleado_id = @empleadoId AND p.estado = 'APROBADO' ORDER BY p.fecha_inicio DESC", conexion))
            {
                cmd.Parameters.Add("@empleadoId", SqlDbType.Int).Value = empleadoId;
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

        public static ReciboDTO ObtenerPorId(int id)
        {
            ReciboDTO recibo;

            using (var conexion = ConexionBD.Obtener())
            using (var cmd = new SqlCommand(ConsultaBase() + "WHERE r.id = @id", conexion))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                conexion.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    recibo = MapearDTO(reader);
                }
            }

            recibo.Detalle = ObtenerDetalle(id);
            return recibo;
        }

        private static List<DetalleReciboDTO> ObtenerDetalle(int reciboId)
        {
            var lista = new List<DetalleReciboDTO>();

            using (var conexion = ConexionBD.Obtener())
            using (var cmd = new SqlCommand(
                "SELECT concepto, tipo, monto FROM detalle_recibo WHERE recibo_id = @reciboId ORDER BY id", conexion))
            {
                cmd.Parameters.Add("@reciboId", SqlDbType.Int).Value = reciboId;
                conexion.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new DetalleReciboDTO
                        {
                            Concepto = (string)reader["concepto"],
                            Tipo = (string)reader["tipo"],
                            Monto = (decimal)reader["monto"]
                        });
                    }
                }
            }

            return lista;
        }

        private static int InsertarRecibo(SqlConnection conexion, int periodoId, ReciboNomina recibo)
        {
            using (var cmd = new SqlCommand(
                "INSERT INTO recibos_nomina (periodo_id, empleado_id, sueldo_base, total_percepciones, total_deducciones, neto_pagar, carga_patronal) " +
                "VALUES (@periodoId, @empleadoId, @sueldoBase, @totalPercepciones, @totalDeducciones, @netoPagar, @cargaPatronal); " +
                "SELECT CAST(SCOPE_IDENTITY() AS INT);", conexion))
            {
                cmd.Parameters.Add("@periodoId", SqlDbType.Int).Value = periodoId;
                cmd.Parameters.Add("@empleadoId", SqlDbType.Int).Value = recibo.EmpleadoId;
                cmd.Parameters.Add("@sueldoBase", SqlDbType.Decimal).Value = recibo.SueldoBase;
                cmd.Parameters.Add("@totalPercepciones", SqlDbType.Decimal).Value = recibo.TotalPercepciones;
                cmd.Parameters.Add("@totalDeducciones", SqlDbType.Decimal).Value = recibo.TotalDeducciones;
                cmd.Parameters.Add("@netoPagar", SqlDbType.Decimal).Value = recibo.NetoPagar;
                cmd.Parameters.Add("@cargaPatronal", SqlDbType.Decimal).Value = recibo.CargaPatronal;

                return (int)cmd.ExecuteScalar();
            }
        }

        private static void InsertarDetalle(SqlConnection conexion, int reciboId, DetalleRecibo detalle)
        {
            using (var cmd = new SqlCommand(
                "INSERT INTO detalle_recibo (recibo_id, concepto, tipo, monto) VALUES (@reciboId, @concepto, @tipo, @monto)", conexion))
            {
                cmd.Parameters.Add("@reciboId", SqlDbType.Int).Value = reciboId;
                cmd.Parameters.Add("@concepto", SqlDbType.NVarChar, 60).Value = detalle.Concepto;
                cmd.Parameters.Add("@tipo", SqlDbType.NVarChar, 15).Value = detalle.Tipo;
                cmd.Parameters.Add("@monto", SqlDbType.Decimal).Value = detalle.Monto;

                cmd.ExecuteNonQuery();
            }
        }

        // Columnas comunes a las tres consultas de lectura; cada una agrega su propio WHERE.
        // Detalle no se incluye aqui: solo ObtenerPorId lo necesita.
        private static string ConsultaBase()
        {
            return "SELECT r.id, r.periodo_id, r.empleado_id, e.numero_empleado, e.nombre + ' ' + e.apellidos AS nombre_completo, e.moneda, " +
                   "r.sueldo_base, r.total_percepciones, r.total_deducciones, r.neto_pagar, r.carga_patronal " +
                   "FROM recibos_nomina r JOIN empleados e ON e.id = r.empleado_id ";
        }

        private static ReciboDTO MapearDTO(SqlDataReader reader)
        {
            return new ReciboDTO
            {
                Id = (int)reader["id"],
                PeriodoId = (int)reader["periodo_id"],
                EmpleadoId = (int)reader["empleado_id"],
                NumeroEmpleado = (string)reader["numero_empleado"],
                NombreEmpleado = (string)reader["nombre_completo"],
                Moneda = (string)reader["moneda"],
                SueldoBase = (decimal)reader["sueldo_base"],
                TotalPercepciones = (decimal)reader["total_percepciones"],
                TotalDeducciones = (decimal)reader["total_deducciones"],
                NetoPagar = (decimal)reader["neto_pagar"],
                CargaPatronal = (decimal)reader["carga_patronal"]
            };
        }
    }
}

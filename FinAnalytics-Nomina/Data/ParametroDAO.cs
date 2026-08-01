using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using FinAnalytics_Nomina.Models;

namespace FinAnalytics_Nomina.Data
{
    public static class ParametroDAO
    {
        public static List<ParametroNomina> ObtenerTodos()
        {
            var lista = new List<ParametroNomina>();

            using (var conexion = ConexionBD.Obtener())
            using (var cmd = new SqlCommand("SELECT id, clave, descripcion, valor FROM parametros_nomina ORDER BY id", conexion))
            {
                conexion.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(Mapear(reader));
                    }
                }
            }

            return lista;
        }

        public static ParametroNomina ObtenerPorId(int id)
        {
            using (var conexion = ConexionBD.Obtener())
            using (var cmd = new SqlCommand("SELECT id, clave, descripcion, valor FROM parametros_nomina WHERE id = @id", conexion))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                conexion.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    return reader.Read() ? Mapear(reader) : null;
                }
            }
        }

        public static void ActualizarValor(int id, decimal valor)
        {
            using (var conexion = ConexionBD.Obtener())
            using (var cmd = new SqlCommand("UPDATE parametros_nomina SET valor = @valor WHERE id = @id", conexion))
            {
                cmd.Parameters.Add("@valor", SqlDbType.Decimal).Value = valor;
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                conexion.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private static ParametroNomina Mapear(SqlDataReader reader)
        {
            return new ParametroNomina
            {
                Id = (int)reader["id"],
                Clave = (string)reader["clave"],
                Descripcion = (string)reader["descripcion"],
                Valor = (decimal)reader["valor"]
            };
        }
    }
}

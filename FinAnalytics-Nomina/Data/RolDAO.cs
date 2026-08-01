using System.Collections.Generic;
using System.Data.SqlClient;
using FinAnalytics_Nomina.Models;

namespace FinAnalytics_Nomina.Data
{
    public static class RolDAO
    {
        public static List<Rol> ObtenerTodos()
        {
            var lista = new List<Rol>();

            using (var conexion = ConexionBD.Obtener())
            using (var cmd = new SqlCommand("SELECT id, nombre, descripcion FROM roles ORDER BY id", conexion))
            {
                conexion.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new Rol
                        {
                            Id = (int)reader["id"],
                            Nombre = (string)reader["nombre"],
                            Descripcion = reader["descripcion"] as string
                        });
                    }
                }
            }

            return lista;
        }
    }
}

using System.Data;
using System.Data.SqlClient;

namespace FinAnalytics_Nomina.Data
{
    // BE-16 solo necesita esta consulta para armar el claim empleadoId del login.
    // El CRUD completo (alta, baja logica, RFC unico, etc.) se agrega en BE-21.
    public static class EmpleadoDAO
    {
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
    }
}

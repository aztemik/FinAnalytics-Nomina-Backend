using System.Configuration;
using System.Data.SqlClient;

namespace FinAnalytics_Nomina.Data
{
    public static class ConexionBD
    {
        public static SqlConnection Obtener()
        {
            string cadena = ConfigurationManager.ConnectionStrings["NominaDB"].ConnectionString;
            return new SqlConnection(cadena);
        }
    }
}

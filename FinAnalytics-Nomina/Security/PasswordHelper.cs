using System;

namespace FinAnalytics_Nomina.Security
{
    public static class PasswordHelper
    {
        public static string Hash(string texto)
        {
            return BCrypt.Net.BCrypt.HashPassword(texto);
        }

        public static bool Verificar(string texto, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(texto, hash);
        }
    }
}

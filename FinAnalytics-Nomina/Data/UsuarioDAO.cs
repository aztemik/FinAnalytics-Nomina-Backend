using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using FinAnalytics_Nomina.Models;
using FinAnalytics_Nomina.Models.DTO;

namespace FinAnalytics_Nomina.Data
{
    public static class UsuarioDAO
    {
        // Incluye password_hash: uso interno exclusivo del login (AuthController).
        // Nunca se expone tal cual al cliente.
        public static Usuario ObtenerPorUsername(string username)
        {
            using (var conexion = ConexionBD.Obtener())
            using (var cmd = new SqlCommand(
                "SELECT id, username, password_hash, nombre_completo, rol_id, activo, fecha_creacion " +
                "FROM usuarios WHERE username = @username", conexion))
            {
                cmd.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = username;
                conexion.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    return reader.Read() ? MapearUsuario(reader) : null;
                }
            }
        }

        public static string ObtenerNombreRol(int rolId)
        {
            using (var conexion = ConexionBD.Obtener())
            using (var cmd = new SqlCommand("SELECT nombre FROM roles WHERE id = @rolId", conexion))
            {
                cmd.Parameters.Add("@rolId", SqlDbType.Int).Value = rolId;
                conexion.Open();
                return cmd.ExecuteScalar() as string;
            }
        }

        public static bool ExisteUsername(string username, int? idExcluir = null)
        {
            using (var conexion = ConexionBD.Obtener())
            using (var cmd = new SqlCommand(
                "SELECT COUNT(1) FROM usuarios WHERE username = @username AND (@idExcluir IS NULL OR id <> @idExcluir)", conexion))
            {
                cmd.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = username;
                cmd.Parameters.Add("@idExcluir", SqlDbType.Int).Value = (object)idExcluir ?? DBNull.Value;
                conexion.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        // No selecciona password_hash: para listar no hace falta y asi nunca sale de esta capa.
        public static List<UsuarioDTO> ObtenerTodos()
        {
            var lista = new List<UsuarioDTO>();

            using (var conexion = ConexionBD.Obtener())
            using (var cmd = new SqlCommand(
                "SELECT u.id, u.username, u.nombre_completo, u.rol_id, r.nombre AS rol_nombre, u.activo " +
                "FROM usuarios u INNER JOIN roles r ON r.id = u.rol_id " +
                "ORDER BY u.username", conexion))
            {
                conexion.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(MapearUsuarioDTO(reader));
                    }
                }
            }

            return lista;
        }

        public static UsuarioDTO ObtenerPorId(int id)
        {
            using (var conexion = ConexionBD.Obtener())
            using (var cmd = new SqlCommand(
                "SELECT u.id, u.username, u.nombre_completo, u.rol_id, r.nombre AS rol_nombre, u.activo " +
                "FROM usuarios u INNER JOIN roles r ON r.id = u.rol_id " +
                "WHERE u.id = @id", conexion))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                conexion.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    return reader.Read() ? MapearUsuarioDTO(reader) : null;
                }
            }
        }

        public static int Crear(Usuario usuario)
        {
            using (var conexion = ConexionBD.Obtener())
            using (var cmd = new SqlCommand(
                "INSERT INTO usuarios (username, password_hash, nombre_completo, rol_id, activo) " +
                "VALUES (@username, @passwordHash, @nombreCompleto, @rolId, @activo); " +
                "SELECT CAST(SCOPE_IDENTITY() AS INT);", conexion))
            {
                cmd.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = usuario.Username;
                cmd.Parameters.Add("@passwordHash", SqlDbType.NVarChar, 255).Value = usuario.PasswordHash;
                cmd.Parameters.Add("@nombreCompleto", SqlDbType.NVarChar, 120).Value = usuario.NombreCompleto;
                cmd.Parameters.Add("@rolId", SqlDbType.Int).Value = usuario.RolId;
                cmd.Parameters.Add("@activo", SqlDbType.Bit).Value = usuario.Activo;

                conexion.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public static void Actualizar(Usuario usuario)
        {
            using (var conexion = ConexionBD.Obtener())
            using (var cmd = new SqlCommand(
                "UPDATE usuarios SET nombre_completo = @nombreCompleto, rol_id = @rolId, activo = @activo " +
                "WHERE id = @id", conexion))
            {
                cmd.Parameters.Add("@nombreCompleto", SqlDbType.NVarChar, 120).Value = usuario.NombreCompleto;
                cmd.Parameters.Add("@rolId", SqlDbType.Int).Value = usuario.RolId;
                cmd.Parameters.Add("@activo", SqlDbType.Bit).Value = usuario.Activo;
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = usuario.Id;

                conexion.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static void ActualizarPassword(int id, string passwordHash)
        {
            using (var conexion = ConexionBD.Obtener())
            using (var cmd = new SqlCommand(
                "UPDATE usuarios SET password_hash = @passwordHash WHERE id = @id", conexion))
            {
                cmd.Parameters.Add("@passwordHash", SqlDbType.NVarChar, 255).Value = passwordHash;
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

                conexion.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Baja logica: el DELETE del endpoint desactiva, nunca borra el registro.
        // Un usuario puede estar referenciado por empleados.usuario_id o por
        // periodos_nomina.creado_por/aprobado_por; borrarlo de verdad rompería ese historial.
        public static void Desactivar(int id)
        {
            using (var conexion = ConexionBD.Obtener())
            using (var cmd = new SqlCommand("UPDATE usuarios SET activo = 0 WHERE id = @id", conexion))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                conexion.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private static Usuario MapearUsuario(SqlDataReader reader)
        {
            return new Usuario
            {
                Id = (int)reader["id"],
                Username = (string)reader["username"],
                PasswordHash = (string)reader["password_hash"],
                NombreCompleto = (string)reader["nombre_completo"],
                RolId = (int)reader["rol_id"],
                Activo = (bool)reader["activo"],
                FechaCreacion = (DateTime)reader["fecha_creacion"]
            };
        }

        private static UsuarioDTO MapearUsuarioDTO(SqlDataReader reader)
        {
            return new UsuarioDTO
            {
                Id = (int)reader["id"],
                Username = (string)reader["username"],
                NombreCompleto = (string)reader["nombre_completo"],
                RolId = (int)reader["rol_id"],
                Rol = (string)reader["rol_nombre"],
                Activo = (bool)reader["activo"]
            };
        }
    }
}

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AuthSystem.Scripts
{
    public class CrearSuperAdmin
    {
        public static async Task Main(string[] args)
        {
            // Cargar configuración
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            string connectionString = configuration.GetConnectionString("DefaultConnection");
            
            Console.WriteLine("Iniciando script para crear rol de Super Administrador...");
            
            try
            {
                // Leer el archivo SQL
                string scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "scripts", "crear-superadmin.sql");
                string sqlScript = File.ReadAllText(scriptPath);
                
                // Ejecutar el script
                await using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                
                await using var command = new SqlCommand(sqlScript, connection);
                await command.ExecuteNonQueryAsync();
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("¡Script ejecutado correctamente!");
                Console.WriteLine("Se ha creado el rol de Super Administrador con todos los permisos necesarios.");
                Console.WriteLine("Se ha creado un usuario administrador con el nombre de usuario 'admin'.");
                Console.WriteLine("Recuerda cambiar la contraseña del usuario administrador después de iniciar sesión por primera vez.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error al ejecutar el script: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}

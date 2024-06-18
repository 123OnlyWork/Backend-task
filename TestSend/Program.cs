using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;

namespace SendingEmail
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Инициализация Serilog для логирования в консоль и файл
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File("logs\\log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                Log.Information("Starting application");

                // Проверка подключения к базе данных
                CheckDatabaseConnection();

                // Запуск хоста ASP.NET Core
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application failed to start");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void CheckDatabaseConnection()
        {
            string connectionString = "Data Source=DESKTOP-SB517S9\\MSQLSERVER;Initial Catalog=verification;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False;";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    Log.Information("Database connection successful!");
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Database connection failed");
                throw; // Пробрасываем исключение для дальнейшей обработки
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog() // Используем Serilog для логирования
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}

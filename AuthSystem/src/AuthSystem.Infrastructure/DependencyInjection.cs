using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// DependencyInjection.cs
using System;
using AuthSystem.Core.Interfaces;
using AuthSystem.Infrastructure.Data;
using AuthSystem.Infrastructure.Data.Repositories;
using AuthSystem.Infrastructure.Email;
using AuthSystem.Infrastructure.Security;
using AuthSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;

namespace AuthSystem.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment = null)
        {
            // Configurar DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            // Registrar repositorios
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddScoped<IModuleRepository, ModuleRepository>();
            services.AddScoped<IAuditRepository, AuditRepository>();
            services.AddScoped<IUserRoleRepository, UserRoleRepository>();

            // Registrar servicios
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ITotpService, TotpService>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddSingleton<IEmailTemplateService, EmailTemplateService>();
            services.AddScoped<IUserPermissionService, UserPermissionService>();
            
            // Configurar servicio de email (real o mock según configuración)
            var useMockEmailService = configuration.GetValue<bool>("UseMockEmailService");
            
            // Registrar servicios de email como singleton para evitar problemas de inyección de dependencias
            if (useMockEmailService || (environment != null && environment.IsDevelopment()))
            {
                services.AddSingleton<IEmailService, MockEmailService>();
                services.AddSingleton<IEmailQueueService, MockEmailQueueService>();
            }
            else
            {
                services.AddSingleton<IEmailService, EmailService>();
                services.AddSingleton<IEmailQueueService, EmailQueueService>();
            }
            
            // Registrar el servicio de envío de emails en segundo plano
            // Comentado temporalmente para resolver problemas de inyección de dependencias
            // services.AddHostedService<BackgroundEmailSender>();
            
            // Registrar servicio de limitación de intentos
            services.AddSingleton<IRateLimitService, RateLimitService>();
            
            services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

            // Configuración
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.Configure<RecaptchaSettings>(configuration.GetSection("RecaptchaSettings"));

            // HttpClient para servicios externos
            services.AddHttpClient<IRecaptchaService, RecaptchaService>();

            // Redis Cache (opcional)
            var useRedisCache = configuration.GetValue<bool>("UseRedisCache");
            if (useRedisCache)
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = configuration.GetConnectionString("RedisConnection");
                    options.InstanceName = "AuthSystem:";
                });
            }
            else
            {
                services.AddDistributedMemoryCache();
            }
            
            // Configurar servicio de SMS (activado/desactivado según configuración)
            var useSmsService = configuration.GetValue<bool>("UseSmsService");
            if (useSmsService)
            {
                // Usar servicio real o mock según configuración
                var useMockSmsService = configuration.GetValue<bool>("UseMockSmsService");
                if (useMockSmsService)
                {
                    services.AddScoped<ISmsService, MockSmsService>();
                }
                else
                {
                    services.Configure<AzureCommunicationSettings>(configuration.GetSection("AzureCommunicationSettings"));
                    services.AddScoped<ISmsService, AzureSmsService>();
                }
            }
            else
            {
                // Si el servicio SMS está desactivado, usar una implementación vacía
                services.AddScoped<ISmsService, MockSmsService>();
            }

            return services;
        }
    }
}

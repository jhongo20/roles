using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AuthSystem.API.Swagger
{
    /// <summary>
    /// Configuración mejorada de Swagger para la API de AuthSystem
    /// </summary>
    public static class SwaggerConfiguration
    {
        /// <summary>
        /// Configura los servicios de Swagger con documentación mejorada
        /// </summary>
        public static void ConfigureSwaggerServices(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                // Información básica de la API
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "AuthSystem API",
                    Version = "v1",
                    Description = "API para el sistema de autenticación y autorización AuthSystem",
                    Contact = new OpenApiContact
                    {
                        Name = "Equipo de Desarrollo",
                        Email = "desarrollo@authsystem.com",
                        Url = new Uri("https://authsystem.com/contacto")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Uso Interno",
                        Url = new Uri("https://authsystem.com/licencia")
                    }
                });

                // Agrupar endpoints por área
                options.TagActionsBy(api => new List<string> { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });
                options.DocInclusionPredicate((docName, api) => true);

                // Configuración de seguridad para JWT
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });

                // Incluir comentarios XML para mejorar la documentación
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }

                // Personalizar la UI de Swagger
                // Nota: Si deseas usar anotaciones, necesitas instalar el paquete Swashbuckle.AspNetCore.Annotations
            });
        }

        /// <summary>
        /// Configura la aplicación para usar Swagger con UI mejorada
        /// </summary>
        public static void ConfigureSwagger(this IApplicationBuilder app)
        {
            app.UseSwagger(options =>
            {
                options.RouteTemplate = "api-docs/{documentName}/swagger.json";
            });

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/api-docs/v1/swagger.json", "AuthSystem API v1");
                options.RoutePrefix = "api-docs";
                options.DocumentTitle = "AuthSystem - Documentación API";
                options.DefaultModelsExpandDepth(2);
                options.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Model);
                options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
                options.EnableDeepLinking();
                options.DisplayRequestDuration();
            });
        }
    }
}

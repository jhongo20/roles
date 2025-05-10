using System;
using System.Collections.Generic;
using AuthSystem.Application.DTOs;
using AuthSystem.Core.Enums;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AuthSystem.API.Swagger
{
    /// <summary>
    /// Filtro para añadir ejemplos a los esquemas de Swagger
    /// </summary>
    public class SwaggerExamplesSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(UserDto))
            {
                ApplyUserDtoExample(schema);
            }
            else if (context.Type == typeof(RoleDto))
            {
                ApplyRoleDtoExample(schema);
            }
            else if (context.Type == typeof(PermissionDto))
            {
                ApplyPermissionDtoExample(schema);
            }
            else if (context.Type == typeof(ModuleDto))
            {
                ApplyModuleDtoExample(schema);
            }
        }

        private void ApplyUserDtoExample(OpenApiSchema schema)
        {
            schema.Example = new OpenApiObject
            {
                ["id"] = new OpenApiString(Guid.NewGuid().ToString()),
                ["username"] = new OpenApiString("john.doe"),
                ["email"] = new OpenApiString("john.doe@example.com"),
                ["firstName"] = new OpenApiString("John"),
                ["lastName"] = new OpenApiString("Doe"),
                ["phoneNumber"] = new OpenApiString("+1234567890"),
                ["isActive"] = new OpenApiBoolean(true),
                ["emailConfirmed"] = new OpenApiBoolean(true),
                ["twoFactorEnabled"] = new OpenApiBoolean(false),
                ["createdAt"] = new OpenApiDateTime(DateTime.UtcNow),
                ["updatedAt"] = new OpenApiDateTime(DateTime.UtcNow),
                ["roles"] = new OpenApiArray
                {
                    new OpenApiObject
                    {
                        ["id"] = new OpenApiString(Guid.NewGuid().ToString()),
                        ["name"] = new OpenApiString("Administrador"),
                        ["description"] = new OpenApiString("Acceso completo al sistema")
                    }
                }
            };
        }

        private void ApplyRoleDtoExample(OpenApiSchema schema)
        {
            schema.Example = new OpenApiObject
            {
                ["id"] = new OpenApiString(Guid.NewGuid().ToString()),
                ["name"] = new OpenApiString("Administrador"),
                ["description"] = new OpenApiString("Acceso completo al sistema"),
                ["isActive"] = new OpenApiBoolean(true),
                ["isDefault"] = new OpenApiBoolean(false),
                ["priority"] = new OpenApiInteger(1),
                ["createdAt"] = new OpenApiDateTime(DateTime.UtcNow),
                ["permissions"] = new OpenApiArray
                {
                    new OpenApiObject
                    {
                        ["id"] = new OpenApiString(Guid.NewGuid().ToString()),
                        ["name"] = new OpenApiString("Crear Usuario"),
                        ["code"] = new OpenApiString("CREATE_USER"),
                        ["description"] = new OpenApiString("Permite crear nuevos usuarios")
                    }
                }
            };
        }

        private void ApplyPermissionDtoExample(OpenApiSchema schema)
        {
            schema.Example = new OpenApiObject
            {
                ["id"] = new OpenApiString(Guid.NewGuid().ToString()),
                ["name"] = new OpenApiString("Crear Usuario"),
                ["code"] = new OpenApiString("CREATE_USER"),
                ["description"] = new OpenApiString("Permite crear nuevos usuarios"),
                ["category"] = new OpenApiString("Gestión de Usuarios"),
                ["isActive"] = new OpenApiBoolean(true),
                ["createdAt"] = new OpenApiDateTime(DateTime.UtcNow),
                ["updatedAt"] = new OpenApiDateTime(DateTime.UtcNow)
            };
        }

        private void ApplyModuleDtoExample(OpenApiSchema schema)
        {
            schema.Example = new OpenApiObject
            {
                ["id"] = new OpenApiString(Guid.NewGuid().ToString()),
                ["name"] = new OpenApiString("Gestión de Usuarios"),
                ["code"] = new OpenApiString("USER_MANAGEMENT"),
                ["description"] = new OpenApiString("Módulo para administrar usuarios"),
                ["icon"] = new OpenApiString("fa-users"),
                ["route"] = new OpenApiString("/admin/users"),
                ["isActive"] = new OpenApiBoolean(true),
                ["displayOrder"] = new OpenApiInteger(1),
                ["parentId"] = new OpenApiNull(),
                ["createdAt"] = new OpenApiDateTime(DateTime.UtcNow),
                ["updatedAt"] = new OpenApiDateTime(DateTime.UtcNow)
            };
        }
    }

    /// <summary>
    /// Filtro para añadir ejemplos a las operaciones de Swagger
    /// </summary>
    public class SwaggerExamplesOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Añadir descripciones detalladas a los parámetros
            foreach (var parameter in operation.Parameters)
            {
                if (parameter.Name.Equals("userId", StringComparison.OrdinalIgnoreCase))
                {
                    parameter.Description = "Identificador único del usuario (GUID)";
                    parameter.Example = new OpenApiString(Guid.NewGuid().ToString());
                }
                else if (parameter.Name.Equals("roleId", StringComparison.OrdinalIgnoreCase))
                {
                    parameter.Description = "Identificador único del rol (GUID)";
                    parameter.Example = new OpenApiString(Guid.NewGuid().ToString());
                }
                else if (parameter.Name.Equals("permissionId", StringComparison.OrdinalIgnoreCase))
                {
                    parameter.Description = "Identificador único del permiso (GUID)";
                    parameter.Example = new OpenApiString(Guid.NewGuid().ToString());
                }
                else if (parameter.Name.Equals("moduleId", StringComparison.OrdinalIgnoreCase))
                {
                    parameter.Description = "Identificador único del módulo (GUID)";
                    parameter.Example = new OpenApiString(Guid.NewGuid().ToString());
                }
                else if (parameter.Name.Equals("includeInactive", StringComparison.OrdinalIgnoreCase))
                {
                    parameter.Description = "Indica si se deben incluir elementos inactivos en los resultados";
                    parameter.Example = new OpenApiBoolean(false);
                }
                else if (parameter.Name.Equals("searchTerm", StringComparison.OrdinalIgnoreCase))
                {
                    parameter.Description = "Término de búsqueda para filtrar resultados (nombre, email, etc.)";
                    parameter.Example = new OpenApiString("admin");
                }
                else if (parameter.Name.Equals("pageNumber", StringComparison.OrdinalIgnoreCase))
                {
                    parameter.Description = "Número de página para paginación (comienza en 1)";
                    parameter.Example = new OpenApiInteger(1);
                }
                else if (parameter.Name.Equals("pageSize", StringComparison.OrdinalIgnoreCase))
                {
                    parameter.Description = "Tamaño de página para paginación (número de elementos por página)";
                    parameter.Example = new OpenApiInteger(10);
                }
            }

            // Añadir ejemplos de respuestas
            if (operation.Responses.ContainsKey("200"))
            {
                var route = context.ApiDescription.RelativePath?.ToLower();
                if (route != null)
                {
                    if (route.Contains("users") && route.EndsWith("roles") && context.MethodInfo.Name.Contains("Get"))
                    {
                        AddUserRolesResponseExample(operation);
                    }
                    else if (route.Contains("users") && context.MethodInfo.Name.Contains("GetAll"))
                    {
                        AddUsersListResponseExample(operation);
                    }
                    else if (route.Contains("roles") && context.MethodInfo.Name.Contains("GetAll"))
                    {
                        AddRolesListResponseExample(operation);
                    }
                }
            }
        }

        private void AddUserRolesResponseExample(OpenApiOperation operation)
        {
            var example = new List<object>
            {
                new
                {
                    id = Guid.NewGuid(),
                    name = "Administrador",
                    description = "Acceso completo al sistema",
                    isActive = true,
                    isDefault = false,
                    priority = 1,
                    createdAt = DateTime.UtcNow
                },
                new
                {
                    id = Guid.NewGuid(),
                    name = "Usuario",
                    description = "Acceso básico al sistema",
                    isActive = true,
                    isDefault = true,
                    priority = 2,
                    createdAt = DateTime.UtcNow
                }
            };

            operation.Responses["200"].Description = "Lista de roles asignados al usuario";
            // Aquí se podría añadir el ejemplo como contenido de la respuesta
        }

        private void AddUsersListResponseExample(OpenApiOperation operation)
        {
            var example = new
            {
                items = new List<object>
                {
                    new
                    {
                        id = Guid.NewGuid(),
                        username = "john.doe",
                        email = "john.doe@example.com",
                        firstName = "John",
                        lastName = "Doe",
                        isActive = true,
                        createdAt = DateTime.UtcNow
                    },
                    new
                    {
                        id = Guid.NewGuid(),
                        username = "jane.smith",
                        email = "jane.smith@example.com",
                        firstName = "Jane",
                        lastName = "Smith",
                        isActive = true,
                        createdAt = DateTime.UtcNow
                    }
                },
                totalCount = 2,
                pageNumber = 1,
                pageSize = 10,
                totalPages = 1
            };

            operation.Responses["200"].Description = "Lista paginada de usuarios";
            // Aquí se podría añadir el ejemplo como contenido de la respuesta
        }

        private void AddRolesListResponseExample(OpenApiOperation operation)
        {
            var example = new List<object>
            {
                new
                {
                    id = Guid.NewGuid(),
                    name = "Administrador",
                    description = "Acceso completo al sistema",
                    isActive = true,
                    isDefault = false,
                    priority = 1,
                    createdAt = DateTime.UtcNow
                },
                new
                {
                    id = Guid.NewGuid(),
                    name = "Usuario",
                    description = "Acceso básico al sistema",
                    isActive = true,
                    isDefault = true,
                    priority = 2,
                    createdAt = DateTime.UtcNow
                }
            };

            operation.Responses["200"].Description = "Lista de roles";
            // Aquí se podría añadir el ejemplo como contenido de la respuesta
        }
    }
}

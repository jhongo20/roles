# Seguridad y Auditoría

## Introducción

El sistema AuthSystem implementa un conjunto completo de medidas de seguridad y auditoría para garantizar la integridad, confidencialidad y disponibilidad de la información. Este documento describe los componentes, configuraciones y mejores prácticas relacionadas con la seguridad y auditoría del sistema.

## Sistema de Logging

AuthSystem utiliza un sistema de logging avanzado basado en Serilog, que proporciona capacidades de registro estructurado y flexible para capturar información detallada sobre el funcionamiento del sistema.

### Configuración de Serilog

Serilog está configurado en `Program.cs` para proporcionar logging estructurado con múltiples destinos (sinks):

```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .UseSerilog((context, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithProperty("Application", "AuthSystem")
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    path: "logs/authsystem-.log",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}");
        })
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        });
```

### Sinks Configurados

1. **Console**: Muestra logs en la consola durante el desarrollo y depuración.
2. **File**: Almacena logs en archivos con rotación diaria para facilitar el análisis histórico.

### Enriquecimiento de Logs

Los logs se enriquecen con información contextual adicional:
- Nombre de la máquina
- Nombre del entorno (Development, Staging, Production)
- Nombre de la aplicación
- Información de contexto específica de cada solicitud

## Middleware de Logging de Solicitudes

El sistema incluye un middleware personalizado (`RequestLoggingMiddleware`) que registra información detallada sobre cada solicitud HTTP que llega al sistema.

```csharp
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;

        try
        {
            // Captura información de la solicitud
            var request = await FormatRequest(context.Request);
            _logger.LogInformation("HTTP {Method} {Path} received from {IP} with User-Agent {UserAgent}",
                context.Request.Method,
                context.Request.Path,
                context.Connection.RemoteIpAddress,
                context.Request.Headers["User-Agent"]);

            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            // Continúa con el pipeline
            await _next(context);

            // Captura información de la respuesta
            var response = await FormatResponse(context.Response);
            sw.Stop();

            _logger.LogInformation("HTTP {StatusCode} returned in {ElapsedMilliseconds}ms",
                context.Response.StatusCode,
                sw.ElapsedMilliseconds);

            // Copia la respuesta al stream original
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in request pipeline");
            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private async Task<string> FormatRequest(HttpRequest request)
    {
        // Implementación para formatear la solicitud
    }

    private async Task<string> FormatResponse(HttpResponse response)
    {
        // Implementación para formatear la respuesta
    }
}
```

### Información Registrada

El middleware registra la siguiente información para cada solicitud:
- Método HTTP (GET, POST, PUT, DELETE, etc.)
- Ruta de la solicitud
- Dirección IP del cliente
- User-Agent del cliente
- Código de estado de la respuesta
- Tiempo de respuesta en milisegundos
- Cuerpo de la solicitud (opcional, configurable)
- Cuerpo de la respuesta (opcional, configurable)

## Servicio de Auditoría

El sistema implementa un servicio de auditoría (`AuditService`) que registra acciones importantes realizadas por los usuarios, especialmente aquellas relacionadas con la seguridad y la gestión de identidad.

```csharp
public interface IAuditService
{
    Task LogLoginAttemptAsync(Guid userId, string username, bool success, string ipAddress, string userAgent);
    Task LogPasswordChangeAsync(Guid userId, string username);
    Task LogUserCreationAsync(Guid userId, string username, string createdBy);
    Task LogUserUpdateAsync(Guid userId, string username, string updatedBy, Dictionary<string, (string OldValue, string NewValue)> changedProperties);
    Task LogRoleAssignmentAsync(Guid userId, string username, Guid roleId, string roleName, string assignedBy);
    Task LogEntityChangeAsync<T>(string entityType, Guid entityId, string action, string performedBy, Dictionary<string, (string OldValue, string NewValue)> changedProperties);
}
```

### Eventos Auditados

El servicio de auditoría registra los siguientes tipos de eventos:

1. **Intentos de inicio de sesión**:
   - Intentos exitosos y fallidos
   - Información del usuario
   - Dirección IP y User-Agent
   - Fecha y hora del intento

2. **Cambios de contraseña**:
   - Usuario que cambió la contraseña
   - Fecha y hora del cambio
   - Información sobre si fue un cambio forzado

3. **Creación y actualización de usuarios**:
   - Usuario creado/actualizado
   - Usuario que realizó la acción
   - Propiedades cambiadas (valores antiguos y nuevos)
   - Fecha y hora de la acción

4. **Asignación de roles**:
   - Usuario al que se asignó el rol
   - Rol asignado
   - Usuario que realizó la asignación
   - Fecha y hora de la asignación

5. **Cambios en entidades**:
   - Tipo de entidad
   - ID de la entidad
   - Acción realizada (creación, actualización, eliminación)
   - Usuario que realizó la acción
   - Propiedades cambiadas (valores antiguos y nuevos)
   - Fecha y hora de la acción

### Almacenamiento de Auditoría

Los registros de auditoría se almacenan en una tabla específica de la base de datos, lo que permite consultas y análisis detallados. Además, los eventos críticos también se registran en los logs del sistema para facilitar la detección temprana de problemas.

## Integración de ILogger en Servicios

Todos los servicios y handlers de comandos del sistema utilizan la interfaz `ILogger` para registrar información relevante sobre su funcionamiento.

```csharp
public class AuthenticateCommandHandler : IRequestHandler<AuthenticateCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IAuditService _auditService;
    private readonly ILogger<AuthenticateCommandHandler> _logger;

    public AuthenticateCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        IAuditService auditService,
        ILogger<AuthenticateCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<AuthResponseDto> Handle(AuthenticateCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Authentication attempt for user {Username}", request.Username);

        // Implementación...

        if (isAuthenticated)
        {
            _logger.LogInformation("User {Username} authenticated successfully", request.Username);
            await _auditService.LogLoginAttemptAsync(user.Id, user.Username, true, request.IpAddress, request.UserAgent);
        }
        else
        {
            _logger.LogWarning("Failed authentication attempt for user {Username}", request.Username);
            await _auditService.LogLoginAttemptAsync(user.Id, user.Username, false, request.IpAddress, request.UserAgent);
        }

        // Resto de la implementación...
    }
}
```

### Niveles de Logging

El sistema utiliza los siguientes niveles de logging:

1. **Trace**: Información muy detallada, útil solo durante el desarrollo intensivo.
2. **Debug**: Información útil para depuración y desarrollo.
3. **Information**: Información general sobre el funcionamiento del sistema.
4. **Warning**: Situaciones potencialmente problemáticas que no impiden el funcionamiento.
5. **Error**: Errores que impiden una operación específica pero no detienen el sistema.
6. **Critical**: Errores críticos que pueden detener el sistema.

## Medidas de Seguridad Adicionales

### RateLimitService

El servicio `RateLimitService` implementa limitación de tasa para prevenir ataques de fuerza bruta y denegación de servicio.

```csharp
public interface IRateLimitService
{
    Task<bool> IsRateLimitedAsync(string key, string category, int limit, TimeSpan period);
    Task RecordAttemptAsync(string key, string category);
    Task ResetAttemptsAsync(string key, string category);
}
```

#### Funcionalidades principales:

- Limitación de intentos de inicio de sesión por usuario y por IP
- Limitación de solicitudes de restablecimiento de contraseña
- Limitación de solicitudes de confirmación de correo electrónico
- Limitación de solicitudes de API por cliente

### Historial de Contraseñas

El sistema mantiene un historial de contraseñas para cada usuario, lo que impide la reutilización de contraseñas recientes.

```csharp
public class PasswordHistory
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string PasswordHash { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Constructor y métodos...
}
```

#### Funcionalidades principales:

- Almacenamiento de hashes de contraseñas anteriores
- Verificación de nuevas contraseñas contra el historial
- Configuración del número de contraseñas a recordar

### Protección contra Ataques Comunes

El sistema implementa protecciones contra varios tipos de ataques comunes:

1. **Cross-Site Scripting (XSS)**:
   - Sanitización de entrada
   - Encabezados de seguridad adecuados
   - Uso de políticas de seguridad de contenido (CSP)

2. **Cross-Site Request Forgery (CSRF)**:
   - Tokens anti-CSRF para formularios
   - Validación de origen de solicitudes

3. **Inyección SQL**:
   - Uso de Entity Framework Core con parámetros
   - Validación de entrada

4. **Man-in-the-Middle (MITM)**:
   - Forzar HTTPS
   - Encabezados HSTS

## Configuración de Seguridad

### Opciones de Logging

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning",
        "Microsoft.AspNetCore.Authentication": "Information"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/authsystem-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
  }
}
```

### Opciones de Limitación de Tasa

```json
{
  "RateLimitSettings": {
    "LoginAttempts": {
      "Limit": 5,
      "PeriodMinutes": 15
    },
    "PasswordReset": {
      "Limit": 3,
      "PeriodMinutes": 60
    },
    "EmailConfirmation": {
      "Limit": 3,
      "PeriodMinutes": 60
    },
    "ApiRequests": {
      "Limit": 100,
      "PeriodMinutes": 1
    }
  }
}
```

### Opciones de Auditoría

```json
{
  "AuditSettings": {
    "EnableDetailedLogs": true,
    "LogRequestBodies": false,
    "LogResponseBodies": false,
    "ExcludePaths": [
      "/api/health",
      "/api/metrics"
    ]
  }
}
```

## Mejores Prácticas

### Logging

1. **No registrar información sensible**: Evitar registrar contraseñas, tokens, información personal identificable (PII) y datos financieros.
2. **Usar logging estructurado**: Utilizar plantillas de mensajes con parámetros en lugar de concatenación de strings.
3. **Incluir contexto**: Enriquecer los logs con información contextual relevante.
4. **Niveles adecuados**: Utilizar el nivel de logging apropiado para cada mensaje.
5. **Rotación de logs**: Configurar la rotación de archivos de log para evitar problemas de espacio.

### Auditoría

1. **Auditar eventos críticos**: Asegurarse de auditar todos los eventos relacionados con seguridad y cambios importantes.
2. **Proteger registros de auditoría**: Implementar medidas para prevenir la manipulación de los registros de auditoría.
3. **Retención adecuada**: Definir políticas de retención de registros de auditoría según requisitos legales y de negocio.
4. **Monitoreo proactivo**: Implementar alertas para detectar patrones sospechosos en los registros de auditoría.
5. **Revisiones periódicas**: Establecer un proceso para la revisión periódica de los registros de auditoría.

### Seguridad General

1. **Actualizaciones regulares**: Mantener todas las dependencias y componentes actualizados.
2. **Principio de privilegio mínimo**: Asignar a los usuarios solo los permisos necesarios.
3. **Defensa en profundidad**: Implementar múltiples capas de seguridad.
4. **Validación de entrada**: Validar y sanitizar todas las entradas de usuario.
5. **Gestión segura de secretos**: Utilizar servicios de gestión de secretos o variables de entorno para información sensible.

## Solución de Problemas

### Análisis de Logs

Para analizar los logs del sistema, se pueden utilizar las siguientes herramientas y técnicas:

1. **Búsqueda en archivos de log**:
   ```powershell
   Get-Content -Path "logs/authsystem-*.log" | Select-String -Pattern "error"
   ```

2. **Filtrado por nivel**:
   ```powershell
   Get-Content -Path "logs/authsystem-*.log" | Select-String -Pattern "\[ERR\]|\[WRN\]"
   ```

3. **Filtrado por usuario**:
   ```powershell
   Get-Content -Path "logs/authsystem-*.log" | Select-String -Pattern "Username: usuario@ejemplo.com"
   ```

4. **Análisis de intentos de inicio de sesión fallidos**:
   ```powershell
   Get-Content -Path "logs/authsystem-*.log" | Select-String -Pattern "Failed authentication attempt"
   ```

### Problemas comunes

1. **Exceso de logs**:
   - Ajustar los niveles mínimos de logging
   - Implementar filtros para reducir el ruido
   - Configurar la rotación y compresión de logs

2. **Pérdida de logs**:
   - Verificar permisos de escritura en el directorio de logs
   - Comprobar espacio en disco
   - Implementar un sistema de alerta para problemas de logging

3. **Rendimiento afectado por logging**:
   - Utilizar logging asíncrono
   - Ajustar los niveles de logging en producción
   - Optimizar las consultas de auditoría

4. **Detección de actividades sospechosas**:
   - Implementar alertas para patrones sospechosos
   - Utilizar herramientas de análisis de logs
   - Correlacionar eventos de diferentes fuentes

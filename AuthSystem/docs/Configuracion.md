# Configuración del Sistema

## Introducción

El sistema AuthSystem proporciona una configuración flexible que permite personalizar su comportamiento según las necesidades específicas de cada implementación. Este documento describe las diferentes opciones de configuración disponibles, cómo configurarlas y las mejores prácticas para gestionar la configuración en diferentes entornos.

## Archivo de Configuración Principal

La configuración principal del sistema se realiza a través del archivo `appsettings.json` ubicado en el proyecto AuthSystem.API. Este archivo contiene todas las opciones de configuración organizadas en secciones lógicas.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=AuthSystem;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "JwtSettings": {
    "Secret": "your-secret-key-at-least-16-characters",
    "Issuer": "authsystem",
    "Audience": "authsystem-clients",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "PasswordSettings": {
    "RequireDigit": true,
    "RequireLowercase": true,
    "RequireUppercase": true,
    "RequireNonAlphanumeric": true,
    "RequiredLength": 8,
    "MaxFailedAttempts": 5,
    "DefaultLockoutMinutes": 30,
    "PasswordHistoryLimit": 5
  },
  "EmailSettings": {
    "SmtpServer": "smtp.ejemplo.com",
    "SmtpPort": 587,
    "SmtpUsername": "usuario@ejemplo.com",
    "SmtpPassword": "contraseña",
    "SenderEmail": "noreply@ejemplo.com",
    "SenderName": "AuthSystem",
    "UseSsl": true,
    "UseMockEmailService": false
  },
  "SmsSettings": {
    "UseSmsService": true,
    "UseMockSmsService": false,
    "AzureCommunicationSettings": {
      "ConnectionString": "your-azure-communication-services-connection-string",
      "SenderPhoneNumber": "+1234567890"
    }
  },
  "TwoFactorSettings": {
    "DefaultMethod": "Email",
    "CodeLength": 6,
    "CodeExpirationMinutes": 10,
    "MaxFailedAttempts": 3,
    "RecoveryCodeCount": 10,
    "RecoveryCodeLength": 8
  },
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
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
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
  },
  "AllowedHosts": "*",
  "CorsSettings": {
    "AllowedOrigins": [
      "https://example.com",
      "https://www.example.com",
      "http://localhost:3000"
    ]
  }
}
```

## Configuración por Entorno

AuthSystem soporta diferentes configuraciones según el entorno de ejecución (desarrollo, pruebas, producción). Esto se logra mediante archivos de configuración específicos para cada entorno:

- `appsettings.Development.json`
- `appsettings.Staging.json`
- `appsettings.Production.json`

Estos archivos contienen configuraciones que sobrescriben las del archivo principal `appsettings.json` para cada entorno específico.

Ejemplo de `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=AuthSystem_Dev;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "EmailSettings": {
    "UseMockEmailService": true
  },
  "SmsSettings": {
    "UseMockSmsService": true
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"
    }
  }
}
```

## Secciones de Configuración

### ConnectionStrings

Contiene las cadenas de conexión a la base de datos.

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=AuthSystem;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

### JwtSettings

Configuración para la generación y validación de tokens JWT.

```json
"JwtSettings": {
  "Secret": "your-secret-key-at-least-16-characters",
  "Issuer": "authsystem",
  "Audience": "authsystem-clients",
  "AccessTokenExpirationMinutes": 60,
  "RefreshTokenExpirationDays": 7
}
```

| Opción | Descripción | Valor por defecto |
|--------|-------------|-------------------|
| Secret | Clave secreta utilizada para firmar los tokens JWT. Debe ser una cadena de al menos 16 caracteres. | - |
| Issuer | Emisor del token JWT. | "authsystem" |
| Audience | Audiencia del token JWT. | "authsystem-clients" |
| AccessTokenExpirationMinutes | Tiempo de expiración del token de acceso en minutos. | 60 |
| RefreshTokenExpirationDays | Tiempo de expiración del refresh token en días. | 7 |

### PasswordSettings

Configuración para las políticas de contraseñas y bloqueo de cuentas.

```json
"PasswordSettings": {
  "RequireDigit": true,
  "RequireLowercase": true,
  "RequireUppercase": true,
  "RequireNonAlphanumeric": true,
  "RequiredLength": 8,
  "MaxFailedAttempts": 5,
  "DefaultLockoutMinutes": 30,
  "PasswordHistoryLimit": 5
}
```

| Opción | Descripción | Valor por defecto |
|--------|-------------|-------------------|
| RequireDigit | Requiere al menos un dígito en la contraseña. | true |
| RequireLowercase | Requiere al menos una letra minúscula en la contraseña. | true |
| RequireUppercase | Requiere al menos una letra mayúscula en la contraseña. | true |
| RequireNonAlphanumeric | Requiere al menos un carácter no alfanumérico en la contraseña. | true |
| RequiredLength | Longitud mínima de la contraseña. | 8 |
| MaxFailedAttempts | Número máximo de intentos fallidos de inicio de sesión antes de bloquear la cuenta. | 5 |
| DefaultLockoutMinutes | Duración del bloqueo de cuenta en minutos. | 30 |
| PasswordHistoryLimit | Número de contraseñas anteriores que se almacenan para prevenir la reutilización. | 5 |

### EmailSettings

Configuración para el servicio de correo electrónico.

```json
"EmailSettings": {
  "SmtpServer": "smtp.ejemplo.com",
  "SmtpPort": 587,
  "SmtpUsername": "usuario@ejemplo.com",
  "SmtpPassword": "contraseña",
  "SenderEmail": "noreply@ejemplo.com",
  "SenderName": "AuthSystem",
  "UseSsl": true,
  "UseMockEmailService": false
}
```

| Opción | Descripción | Valor por defecto |
|--------|-------------|-------------------|
| SmtpServer | Servidor SMTP para envío de correos. | - |
| SmtpPort | Puerto del servidor SMTP. | 587 |
| SmtpUsername | Nombre de usuario para autenticación SMTP. | - |
| SmtpPassword | Contraseña para autenticación SMTP. | - |
| SenderEmail | Dirección de correo del remitente. | - |
| SenderName | Nombre del remitente. | "AuthSystem" |
| UseSsl | Indica si se debe utilizar SSL para la conexión SMTP. | true |
| UseMockEmailService | Indica si se debe utilizar un servicio simulado de correo para desarrollo. | false |

### SmsSettings

Configuración para el servicio de SMS utilizado en la autenticación de dos factores.

```json
"SmsSettings": {
  "UseSmsService": true,
  "UseMockSmsService": false,
  "AzureCommunicationSettings": {
    "ConnectionString": "your-azure-communication-services-connection-string",
    "SenderPhoneNumber": "+1234567890"
  }
}
```

| Opción | Descripción | Valor por defecto |
|--------|-------------|-------------------|
| UseSmsService | Activa o desactiva completamente el servicio de SMS. | true |
| UseMockSmsService | Indica si se debe utilizar un servicio simulado de SMS para desarrollo. | false |
| AzureCommunicationSettings.ConnectionString | Cadena de conexión para Azure Communication Services. | - |
| AzureCommunicationSettings.SenderPhoneNumber | Número de teléfono del remitente. | - |

### TwoFactorSettings

Configuración para la autenticación de dos factores.

```json
"TwoFactorSettings": {
  "DefaultMethod": "Email",
  "CodeLength": 6,
  "CodeExpirationMinutes": 10,
  "MaxFailedAttempts": 3,
  "RecoveryCodeCount": 10,
  "RecoveryCodeLength": 8
}
```

| Opción | Descripción | Valor por defecto |
|--------|-------------|-------------------|
| DefaultMethod | Método predeterminado para 2FA: "Email", "SMS", "Authenticator". | "Email" |
| CodeLength | Longitud de los códigos de verificación. | 6 |
| CodeExpirationMinutes | Tiempo de expiración de los códigos de verificación en minutos. | 10 |
| MaxFailedAttempts | Número máximo de intentos fallidos de verificación antes de bloquear. | 3 |
| RecoveryCodeCount | Número de códigos de recuperación generados. | 10 |
| RecoveryCodeLength | Longitud de los códigos de recuperación. | 8 |

### RateLimitSettings

Configuración para la limitación de tasa de solicitudes.

```json
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
```

| Categoría | Opción | Descripción | Valor por defecto |
|-----------|--------|-------------|-------------------|
| LoginAttempts | Limit | Número máximo de intentos de inicio de sesión. | 5 |
| LoginAttempts | PeriodMinutes | Período de tiempo en minutos para los intentos de inicio de sesión. | 15 |
| PasswordReset | Limit | Número máximo de solicitudes de restablecimiento de contraseña. | 3 |
| PasswordReset | PeriodMinutes | Período de tiempo en minutos para las solicitudes de restablecimiento. | 60 |
| EmailConfirmation | Limit | Número máximo de solicitudes de confirmación de correo. | 3 |
| EmailConfirmation | PeriodMinutes | Período de tiempo en minutos para las solicitudes de confirmación. | 60 |
| ApiRequests | Limit | Número máximo de solicitudes de API. | 100 |
| ApiRequests | PeriodMinutes | Período de tiempo en minutos para las solicitudes de API. | 1 |

### Serilog

Configuración para el sistema de logging Serilog.

```json
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
```

| Sección | Opción | Descripción |
|---------|--------|-------------|
| MinimumLevel | Default | Nivel mínimo de log por defecto. |
| MinimumLevel | Override | Sobrescribe el nivel mínimo para namespaces específicos. |
| WriteTo | Name | Nombre del sink (destino) de logs. |
| WriteTo | Args | Argumentos específicos para el sink. |
| Enrich | - | Enriquecedores que añaden información adicional a los logs. |

### CorsSettings

Configuración para Cross-Origin Resource Sharing (CORS).

```json
"CorsSettings": {
  "AllowedOrigins": [
    "https://example.com",
    "https://www.example.com",
    "http://localhost:3000"
  ]
}
```

| Opción | Descripción |
|--------|-------------|
| AllowedOrigins | Lista de orígenes permitidos para CORS. |

## Configuración en Código

La configuración se carga en el código a través de la clase `Startup` y se inyecta en los servicios correspondientes mediante el patrón de opciones.

### Registro de Opciones

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Configuración de JWT
    services.Configure<JwtSettings>(Configuration.GetSection("JwtSettings"));
    
    // Configuración de contraseñas
    services.Configure<PasswordSettings>(Configuration.GetSection("PasswordSettings"));
    
    // Configuración de correo electrónico
    services.Configure<EmailSettings>(Configuration.GetSection("EmailSettings"));
    
    // Configuración de SMS
    services.Configure<SmsSettings>(Configuration.GetSection("SmsSettings"));
    
    // Configuración de 2FA
    services.Configure<TwoFactorSettings>(Configuration.GetSection("TwoFactorSettings"));
    
    // Configuración de limitación de tasa
    services.Configure<RateLimitSettings>(Configuration.GetSection("RateLimitSettings"));
    
    // Configuración de CORS
    services.Configure<CorsSettings>(Configuration.GetSection("CorsSettings"));
    
    // Resto de la configuración...
}
```

### Inyección de Opciones

Las opciones se inyectan en los servicios que las necesitan:

```csharp
public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;
    
    public JwtService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }
    
    // Implementación...
}
```

## Configuración de Servicios Condicional

AuthSystem implementa la configuración condicional de servicios basada en las opciones configuradas. Por ejemplo, para el servicio de SMS:

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuración del servicio de SMS
        var smsSettings = configuration.GetSection("SmsSettings").Get<SmsSettings>();
        
        if (smsSettings.UseSmsService)
        {
            if (smsSettings.UseMockSmsService)
            {
                services.AddScoped<ISmsService, MockSmsService>();
            }
            else
            {
                services.AddScoped<ISmsService, AzureSmsService>();
            }
        }
        else
        {
            services.AddScoped<ISmsService, NullSmsService>();
        }
        
        // Resto de la configuración...
        
        return services;
    }
}
```

## Variables de Entorno

Además de los archivos de configuración, AuthSystem soporta la configuración mediante variables de entorno. Esto es especialmente útil para información sensible como contraseñas y claves de API en entornos de producción.

Las variables de entorno deben seguir el formato `AUTHSYSTEM_SECTIONNAME__KEYNAME`, por ejemplo:

- `AUTHSYSTEM_JWTSETTINGS__SECRET`
- `AUTHSYSTEM_EMAILSETTINGS__SMTPPASSWORD`
- `AUTHSYSTEM_SMSSETTINGS__AZURECOMMUNICATIONSETTINGS__CONNECTIONSTRING`

## Secretos de Usuario en Desarrollo

Para el desarrollo local, se recomienda utilizar los secretos de usuario de .NET Core para almacenar información sensible:

```powershell
dotnet user-secrets init --project src/AuthSystem.API
dotnet user-secrets set "JwtSettings:Secret" "your-secret-key" --project src/AuthSystem.API
dotnet user-secrets set "EmailSettings:SmtpPassword" "your-smtp-password" --project src/AuthSystem.API
```

## Mejores Prácticas

### Seguridad

1. **No almacenar secretos en el control de versiones**: Utilizar variables de entorno, secretos de usuario o servicios de gestión de secretos.
2. **Utilizar diferentes valores para diferentes entornos**: Especialmente para claves y secretos.
3. **Limitar el acceso a la configuración**: Asegurarse de que solo los servicios que necesitan ciertas configuraciones tengan acceso a ellas.

### Mantenibilidad

1. **Organizar la configuración en secciones lógicas**: Facilita la comprensión y el mantenimiento.
2. **Documentar todas las opciones**: Incluir descripciones, valores por defecto y ejemplos.
3. **Validar la configuración al inicio**: Verificar que todas las opciones requeridas estén presentes y sean válidas.

### Flexibilidad

1. **Permitir anular la configuración**: Proporcionar múltiples formas de configurar el sistema.
2. **Implementar valores por defecto sensatos**: Asegurarse de que el sistema funcione con configuración mínima.
3. **Soportar recarga de configuración**: Permitir cambios de configuración sin reiniciar la aplicación cuando sea posible.

## Solución de Problemas

### Problemas comunes

1. **Configuración no aplicada**:
   - Verificar que la sección y las claves estén correctamente escritas.
   - Comprobar que la configuración se esté cargando en el orden correcto.
   - Verificar que no haya conflictos entre diferentes fuentes de configuración.

2. **Valores sensibles expuestos**:
   - Utilizar variables de entorno o secretos de usuario para información sensible.
   - Implementar enmascaramiento de valores sensibles en los logs.
   - Verificar que los archivos de configuración con información sensible no se incluyan en el control de versiones.

3. **Configuración incorrecta en producción**:
   - Implementar validación de configuración al inicio.
   - Utilizar listas de verificación para la configuración de producción.
   - Automatizar la validación de configuración como parte del proceso de despliegue.

# Guías de Implementación

## Introducción

Este documento proporciona guías detalladas para la implementación, configuración y uso del sistema AuthSystem. Estas guías están diseñadas para ayudar a los desarrolladores y administradores a configurar correctamente el sistema y aprovechar todas sus funcionalidades.

## Índice

1. [Configuración Inicial](#configuración-inicial)
2. [Implementación de Autenticación Básica](#implementación-de-autenticación-básica)
3. [Configuración de Confirmación de Email](#configuración-de-confirmación-de-email)
4. [Implementación de Autenticación de Dos Factores](#implementación-de-autenticación-de-dos-factores)
5. [Configuración del Servicio SMS](#configuración-del-servicio-sms)
6. [Gestión de Roles y Permisos](#gestión-de-roles-y-permisos)
7. [Configuración de Logging y Auditoría](#configuración-de-logging-y-auditoría)
8. [Despliegue en Producción](#despliegue-en-producción)
9. [Solución de Problemas Comunes](#solución-de-problemas-comunes)

## Configuración Inicial

### Requisitos Previos

Antes de comenzar, asegúrese de tener instalados los siguientes componentes:

- .NET 8 SDK o superior
- SQL Server (o una base de datos compatible con Entity Framework Core)
- IDE (Visual Studio, VS Code, Rider, etc.)
- Git (opcional, para control de versiones)

### Clonar el Repositorio

```bash
git clone https://github.com/su-organizacion/AuthSystem.git
cd AuthSystem
```

### Restaurar Dependencias

```bash
dotnet restore
```

### Configurar la Base de Datos

1. Abra el archivo `appsettings.json` en el proyecto AuthSystem.API.
2. Modifique la cadena de conexión en la sección `ConnectionStrings` para que apunte a su servidor de base de datos:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=su-servidor;Database=AuthSystem;User Id=su-usuario;Password=su-contraseña;MultipleActiveResultSets=true"
}
```

3. Ejecute las migraciones para crear la base de datos:

```bash
cd src/AuthSystem.API
dotnet ef database update
```

### Configurar Secretos

Para información sensible, utilice los secretos de usuario de .NET Core:

```bash
dotnet user-secrets init --project src/AuthSystem.API
dotnet user-secrets set "JwtSettings:Secret" "su-clave-secreta-jwt" --project src/AuthSystem.API
dotnet user-secrets set "EmailSettings:SmtpPassword" "su-contraseña-smtp" --project src/AuthSystem.API
```

### Ejecutar la Aplicación

```bash
cd src/AuthSystem.API
dotnet run
```

La API estará disponible en `https://localhost:5001` y `http://localhost:5000`.

## Implementación de Autenticación Básica

### Configuración de JWT

1. Configure las opciones de JWT en `appsettings.json`:

```json
"JwtSettings": {
  "Secret": "su-clave-secreta-jwt-al-menos-16-caracteres",
  "Issuer": "authsystem",
  "Audience": "authsystem-clients",
  "AccessTokenExpirationMinutes": 60,
  "RefreshTokenExpirationDays": 7
}
```

> **Nota**: En producción, no almacene la clave secreta en `appsettings.json`. Utilice variables de entorno o servicios de gestión de secretos.

### Configuración de Políticas de Contraseñas

Configure las políticas de contraseñas en `appsettings.json`:

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

### Implementación en el Cliente

Para implementar la autenticación en el cliente, siga estos pasos:

1. **Registro de Usuario**:

```javascript
async function registerUser(userData) {
  const response = await fetch('https://su-api.com/api/users', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(userData)
  });
  return await response.json();
}
```

2. **Inicio de Sesión**:

```javascript
async function login(username, password) {
  const response = await fetch('https://su-api.com/api/auth/login', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ username, password })
  });
  const data = await response.json();
  
  if (data.succeeded) {
    // Almacenar tokens
    localStorage.setItem('token', data.token);
    localStorage.setItem('refreshToken', data.refreshToken);
    return data;
  } else if (data.requiresTwoFactor) {
    // Redirigir a la página de 2FA
    return { requiresTwoFactor: true, userId: data.userId };
  } else {
    throw new Error(data.message);
  }
}
```

3. **Renovación de Token**:

```javascript
async function refreshToken() {
  const token = localStorage.getItem('token');
  const refreshToken = localStorage.getItem('refreshToken');
  
  const response = await fetch('https://su-api.com/api/auth/refresh-token', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ token, refreshToken })
  });
  
  const data = await response.json();
  
  if (data.succeeded) {
    localStorage.setItem('token', data.token);
    localStorage.setItem('refreshToken', data.refreshToken);
    return data;
  } else {
    // Forzar cierre de sesión
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    throw new Error(data.message);
  }
}
```

4. **Cierre de Sesión**:

```javascript
async function logout() {
  const token = localStorage.getItem('token');
  
  await fetch('https://su-api.com/api/auth/logout', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify({ token })
  });
  
  localStorage.removeItem('token');
  localStorage.removeItem('refreshToken');
}
```

## Configuración de Confirmación de Email

### Configuración del Servicio de Email

1. Configure las opciones de email en `appsettings.json`:

```json
"EmailSettings": {
  "SmtpServer": "smtp.su-proveedor.com",
  "SmtpPort": 587,
  "SmtpUsername": "su-usuario@su-dominio.com",
  "SmtpPassword": "su-contraseña",
  "SenderEmail": "noreply@su-dominio.com",
  "SenderName": "AuthSystem",
  "UseSsl": true,
  "UseMockEmailService": false
}
```

2. Para desarrollo, puede utilizar un servicio simulado de email:

```json
"EmailSettings": {
  "UseMockEmailService": true
}
```

### Personalización de Plantillas de Email

Las plantillas de email se encuentran en la carpeta `Templates` del proyecto AuthSystem.Infrastructure. Puede personalizarlas según sus necesidades:

1. Abra el archivo `EmailConfirmation.html`.
2. Modifique el HTML y CSS para que coincida con la imagen de su aplicación.
3. Asegúrese de mantener los marcadores de posición como `{{UserName}}`, `{{ConfirmationUrl}}`, etc.

### Implementación en el Cliente

Para implementar la confirmación de email en el cliente, siga estos pasos:

1. **Solicitar Reenvío de Correo de Confirmación**:

```javascript
async function resendConfirmationEmail(email) {
  const response = await fetch('https://su-api.com/api/email-confirmation/resend', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      email,
      callbackUrl: 'https://su-app.com/confirmar-email?token={token}&userId={userId}'
    })
  });
  return await response.json();
}
```

2. **Verificar Token de Confirmación**:

```javascript
async function verifyEmailToken(userId, token) {
  const response = await fetch('https://su-api.com/api/email-confirmation/verify', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ userId, token })
  });
  return await response.json();
}
```

## Implementación de Autenticación de Dos Factores

### Configuración de 2FA

1. Configure las opciones de 2FA en `appsettings.json`:

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

### Habilitar 2FA para un Usuario

Para habilitar la autenticación de dos factores para un usuario, siga estos pasos:

1. **Habilitar 2FA**:

```javascript
async function enableTwoFactor(userId, phoneNumber) {
  const token = localStorage.getItem('token');
  
  const response = await fetch('https://su-api.com/api/two-factor/enable', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify({ userId, phoneNumber })
  });
  return await response.json();
}
```

2. **Enviar Código de Verificación**:

```javascript
async function sendTwoFactorCode(userId) {
  const response = await fetch('https://su-api.com/api/two-factor/send-code', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ userId })
  });
  return await response.json();
}
```

3. **Verificar Código**:

```javascript
async function verifyTwoFactorCode(userId, code) {
  const response = await fetch('https://su-api.com/api/two-factor/verify-code', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ userId, code })
  });
  
  const data = await response.json();
  
  if (data.succeeded) {
    localStorage.setItem('token', data.token);
    localStorage.setItem('refreshToken', data.refreshToken);
    return data;
  } else {
    throw new Error(data.message);
  }
}
```

4. **Deshabilitar 2FA**:

```javascript
async function disableTwoFactor(userId) {
  const token = localStorage.getItem('token');
  
  const response = await fetch('https://su-api.com/api/two-factor/disable', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify({ userId })
  });
  return await response.json();
}
```

## Configuración del Servicio SMS

### Configuración de Azure Communication Services

1. Cree una cuenta en Azure Communication Services:
   - Inicie sesión en el [Portal de Azure](https://portal.azure.com).
   - Cree un nuevo recurso de Azure Communication Services.
   - Copie la cadena de conexión.

2. Configure las opciones de SMS en `appsettings.json`:

```json
"SmsSettings": {
  "UseSmsService": true,
  "UseMockSmsService": false,
  "AzureCommunicationSettings": {
    "ConnectionString": "su-cadena-de-conexion-de-azure",
    "SenderPhoneNumber": "+1234567890"
  }
}
```

3. Para desarrollo, puede utilizar un servicio simulado de SMS:

```json
"SmsSettings": {
  "UseSmsService": true,
  "UseMockSmsService": true
}
```

### Implementación de 2FA con SMS

Para implementar la autenticación de dos factores con SMS, siga los mismos pasos que para la implementación de 2FA, pero asegúrese de que el método predeterminado sea "SMS":

```json
"TwoFactorSettings": {
  "DefaultMethod": "SMS",
  // Otras opciones...
}
```

## Gestión de Roles y Permisos

### Creación de Roles

Para crear un nuevo rol, utilice el siguiente endpoint:

```javascript
async function createRole(roleName, description) {
  const token = localStorage.getItem('token');
  
  const response = await fetch('https://su-api.com/api/roles', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify({ name: roleName, description })
  });
  return await response.json();
}
```

### Asignación de Roles a Usuarios

Para asignar un rol a un usuario, utilice el siguiente endpoint:

```javascript
async function assignRole(userId, roleId) {
  const token = localStorage.getItem('token');
  
  const response = await fetch(`https://su-api.com/api/users/${userId}/roles`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify({ roleId })
  });
  return await response.json();
}
```

### Verificación de Roles y Permisos

Para verificar si un usuario tiene un rol específico, puede utilizar el token JWT decodificado o consultar el endpoint correspondiente:

```javascript
async function getUserRoles(userId) {
  const token = localStorage.getItem('token');
  
  const response = await fetch(`https://su-api.com/api/users/${userId}/roles`, {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  return await response.json();
}
```

## Configuración de Logging y Auditoría

### Configuración de Serilog

1. Configure las opciones de Serilog en `appsettings.json`:

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

2. Para entornos de producción, considere configurar destinos adicionales como Elasticsearch, Seq o Azure Application Insights.

### Configuración de Auditoría

Configure las opciones de auditoría en `appsettings.json`:

```json
"AuditSettings": {
  "EnableDetailedLogs": true,
  "LogRequestBodies": false,
  "LogResponseBodies": false,
  "ExcludePaths": [
    "/api/health",
    "/api/metrics"
  ]
}
```

### Consulta de Registros de Auditoría

Para consultar los registros de auditoría, puede implementar un endpoint específico:

```javascript
async function getAuditLogs(filters) {
  const token = localStorage.getItem('token');
  
  const queryParams = new URLSearchParams(filters).toString();
  const response = await fetch(`https://su-api.com/api/audit?${queryParams}`, {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  return await response.json();
}
```

## Despliegue en Producción

### Requisitos de Producción

Para desplegar el sistema en producción, asegúrese de cumplir con los siguientes requisitos:

1. **Servidor Web**:
   - IIS en Windows Server
   - Nginx o Apache en Linux
   - Azure App Service, AWS Elastic Beanstalk, etc.

2. **Base de Datos**:
   - SQL Server (recomendado)
   - PostgreSQL, MySQL, etc. (con adaptaciones)

3. **Servicios Externos**:
   - Servidor SMTP para envío de correos
   - Azure Communication Services para SMS (si se utiliza 2FA con SMS)

### Configuración de Producción

1. **Configuración de appsettings.Production.json**:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-server;Database=AuthSystem_Prod;User Id=prod-user;Password=prod-password;MultipleActiveResultSets=true"
  },
  "JwtSettings": {
    "AccessTokenExpirationMinutes": 30,
    "RefreshTokenExpirationDays": 7
  },
  "EmailSettings": {
    "UseMockEmailService": false
  },
  "SmsSettings": {
    "UseMockSmsService": false
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Warning"
    }
  }
}
```

2. **Variables de Entorno**:

Para información sensible, utilice variables de entorno:

```
AUTHSYSTEM_JWTSETTINGS__SECRET=su-clave-secreta-jwt
AUTHSYSTEM_EMAILSETTINGS__SMTPPASSWORD=su-contraseña-smtp
AUTHSYSTEM_SMSSETTINGS__AZURECOMMUNICATIONSETTINGS__CONNECTIONSTRING=su-cadena-de-conexion-azure
```

### Proceso de Despliegue

1. **Publicación de la Aplicación**:

```bash
dotnet publish -c Release -o ./publish
```

2. **Despliegue en IIS**:
   - Cree un nuevo sitio web en IIS.
   - Configure el grupo de aplicaciones para .NET Core.
   - Apunte la ruta física al directorio de publicación.

3. **Despliegue en Docker**:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/AuthSystem.API/AuthSystem.API.csproj", "src/AuthSystem.API/"]
COPY ["src/AuthSystem.Application/AuthSystem.Application.csproj", "src/AuthSystem.Application/"]
COPY ["src/AuthSystem.Core/AuthSystem.Core.csproj", "src/AuthSystem.Core/"]
COPY ["src/AuthSystem.Infrastructure/AuthSystem.Infrastructure.csproj", "src/AuthSystem.Infrastructure/"]
RUN dotnet restore "src/AuthSystem.API/AuthSystem.API.csproj"
COPY . .
WORKDIR "/src/src/AuthSystem.API"
RUN dotnet build "AuthSystem.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AuthSystem.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuthSystem.API.dll"]
```

4. **Despliegue en Azure App Service**:
   - Cree un nuevo App Service en Azure.
   - Configure la implementación continua desde su repositorio de código.
   - Configure las variables de entorno en la sección "Configuración" del App Service.

### Lista de Verificación para Producción

- [ ] Configuración de secretos mediante variables de entorno
- [ ] Configuración de HTTPS
- [ ] Configuración de CORS
- [ ] Configuración de logging y monitoreo
- [ ] Configuración de copias de seguridad de la base de datos
- [ ] Pruebas de carga y rendimiento
- [ ] Plan de recuperación ante desastres
- [ ] Documentación de operaciones

## Solución de Problemas Comunes

### Problemas de Autenticación

1. **Token JWT inválido o expirado**:
   - Verificar que el token no haya expirado.
   - Comprobar que la firma del token sea válida.
   - Asegurarse de que el token no haya sido revocado.

   **Solución**: Utilizar el endpoint de refresh token para obtener un nuevo token.

2. **Problemas con 2FA**:
   - Verificar que el código de verificación sea correcto.
   - Comprobar que el código no haya expirado.
   - Asegurarse de que el servicio de SMS o correo esté configurado correctamente.

   **Solución**: Solicitar un nuevo código de verificación o utilizar códigos de recuperación.

### Problemas de Base de Datos

1. **Errores de migración**:
   - Verificar que la cadena de conexión sea correcta.
   - Comprobar que el usuario tenga permisos suficientes.
   - Asegurarse de que la base de datos exista.

   **Solución**: Ejecutar las migraciones manualmente o actualizar la cadena de conexión.

2. **Problemas de rendimiento**:
   - Verificar que la base de datos tenga índices adecuados.
   - Comprobar que las consultas sean eficientes.
   - Asegurarse de que la base de datos tenga recursos suficientes.

   **Solución**: Optimizar consultas, añadir índices o aumentar recursos.

### Problemas de Correo Electrónico

1. **No se envían correos**:
   - Verificar que la configuración SMTP sea correcta.
   - Comprobar que el servicio SMTP esté disponible.
   - Asegurarse de que el remitente esté autorizado.

   **Solución**: Actualizar la configuración SMTP o utilizar un servicio alternativo.

2. **Correos marcados como spam**:
   - Verificar que el dominio tenga registros SPF, DKIM y DMARC.
   - Comprobar que el contenido del correo no sea sospechoso.
   - Asegurarse de que el remitente tenga buena reputación.

   **Solución**: Configurar registros DNS adecuados y mejorar el contenido del correo.

### Problemas de SMS

1. **No se envían SMS**:
   - Verificar que la configuración de Azure Communication Services sea correcta.
   - Comprobar que el servicio esté disponible.
   - Asegurarse de que el número de teléfono del remitente esté autorizado.

   **Solución**: Actualizar la configuración o utilizar un servicio alternativo.

2. **SMS no recibidos**:
   - Verificar que el número de teléfono del destinatario sea válido.
   - Comprobar que el formato del número incluya el código de país.
   - Asegurarse de que el destinatario no haya bloqueado los SMS.

   **Solución**: Validar y formatear correctamente los números de teléfono.

### Problemas de Logging

1. **Logs no generados**:
   - Verificar que la configuración de Serilog sea correcta.
   - Comprobar que el nivel mínimo de log no sea demasiado alto.
   - Asegurarse de que los destinos de log estén disponibles.

   **Solución**: Actualizar la configuración de logging.

2. **Logs demasiado grandes**:
   - Verificar que la rotación de logs esté configurada.
   - Comprobar que el nivel mínimo de log no sea demasiado bajo.
   - Asegurarse de que los logs no contengan información innecesaria.

   **Solución**: Ajustar la configuración de logging y la rotación de archivos.

### Recursos Adicionales

- [Documentación de .NET](https://docs.microsoft.com/dotnet)
- [Documentación de Entity Framework Core](https://docs.microsoft.com/ef/core)
- [Documentación de Serilog](https://serilog.net)
- [Documentación de Azure Communication Services](https://docs.microsoft.com/azure/communication-services)
- [Mejores prácticas de seguridad para APIs](https://owasp.org/www-project-api-security)

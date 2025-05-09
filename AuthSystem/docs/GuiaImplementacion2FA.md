# Guía de Implementación de Autenticación de Dos Factores con SMS

## Introducción

Esta guía describe los pasos para implementar y utilizar la autenticación de dos factores (2FA) con SMS en el sistema AuthSystem.

## Requisitos previos

- Proyecto AuthSystem configurado y funcionando
- Azure Communication Services (para envío de SMS en producción)
- Número de teléfono adquirido para envío de SMS

## Pasos de implementación

### 1. Configuración del servicio SMS

Actualice el archivo `appsettings.json` con la configuración adecuada:

```json
{
  "SmsSettings": {
    "UseSmsService": true,
    "UseMockSmsService": false,
    "AzureCommunicationSettings": {
      "ConnectionString": "su-connection-string-de-azure",
      "FromNumber": "+1234567890"
    }
  }
}
```

Para desarrollo, puede utilizar el servicio simulado:

```json
{
  "SmsSettings": {
    "UseSmsService": true,
    "UseMockSmsService": true
  }
}
```

### 2. Registro de servicios

Asegúrese de que los servicios estén registrados en `DependencyInjection.cs`:

```csharp
public static IServiceCollection AddSmsServices(this IServiceCollection services, IConfiguration configuration)
{
    services.Configure<AzureCommunicationSettings>(
        configuration.GetSection("SmsSettings:AzureCommunicationSettings"));

    bool useSmsService = configuration.GetValue<bool>("SmsSettings:UseSmsService");
    bool useMockSmsService = configuration.GetValue<bool>("SmsSettings:UseMockSmsService");

    if (useSmsService)
    {
        if (useMockSmsService)
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

    return services;
}
```

### 3. Flujo de implementación de 2FA

#### Habilitar 2FA para un usuario

1. El usuario actualiza su número de teléfono en su perfil.
2. El usuario solicita habilitar 2FA en su cuenta.
3. El sistema genera una clave secreta para TOTP.
4. El sistema envía un código de verificación por SMS.
5. El usuario verifica el código para confirmar que puede recibir SMS.
6. El sistema habilita 2FA para el usuario.

#### Flujo de autenticación con 2FA

1. El usuario inicia sesión con su nombre de usuario y contraseña.
2. Si tiene 2FA habilitado, el sistema devuelve `requiresTwoFactor: true` y `userId`.
3. La aplicación cliente solicita el envío de un código de verificación.
4. El usuario recibe el código por SMS.
5. El usuario ingresa el código en la aplicación.
6. El sistema verifica el código y completa la autenticación.

## Uso de la API

### Autenticación básica

```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "usuario",
  "password": "contraseña",
  "rememberMe": true
}
```

### Envío de código 2FA

```http
POST /api/twofactor/send-code
Content-Type: application/json

{
  "userId": "guid-del-usuario"
}
```

### Verificación de código 2FA

```http
POST /api/twofactor/verify
Content-Type: application/json

{
  "userId": "guid-del-usuario",
  "code": "123456",
  "rememberMe": true
}
```

### Habilitar 2FA

```http
POST /api/twofactor/enable
Content-Type: application/json

{
  "phoneNumber": "+573001234567"
}
```

### Deshabilitar 2FA

```http
POST /api/twofactor/disable
Content-Type: application/json

{
  "password": "contraseña"
}
```

## Pruebas

### Prueba del servicio SMS

Para probar el servicio SMS, puede utilizar el siguiente código:

```csharp
// Inyectar ISmsService
private readonly ISmsService _smsService;

public MiControlador(ISmsService smsService)
{
    _smsService = smsService;
}

// Enviar un mensaje de prueba
await _smsService.SendMessageAsync("+573001234567", "Mensaje de prueba");

// Enviar un código de verificación
await _smsService.SendVerificationCodeAsync("+573001234567", "123456");
```

### Prueba del flujo completo

1. Habilite 2FA para un usuario de prueba.
2. Inicie sesión con ese usuario.
3. Verifique que se solicite 2FA.
4. Solicite el envío de un código.
5. Verifique el código.
6. Confirme que la autenticación se completa correctamente.

## Solución de problemas

### No se reciben SMS
- Verifique la configuración del servicio SMS.
- Compruebe que el número de teléfono esté en formato internacional.
- Revise los logs para ver si hay errores específicos.

### Error al verificar el código
- Asegúrese de que el código no haya expirado.
- Verifique que el usuario esté ingresando el código más reciente.
- Compruebe que el reloj del servidor esté sincronizado.

## Mejores prácticas

1. **Seguridad**: Implemente limitación de tasa para prevenir ataques de fuerza bruta.
2. **Formato de número**: Utilice siempre formato internacional para los números de teléfono.
3. **Mensajes claros**: Los mensajes SMS deben ser claros y concisos.
4. **Monitoreo**: Configure alertas para detectar fallos en el envío de SMS.
5. **Alternativas**: Proporcione métodos alternativos de recuperación.

## Recursos adicionales

- [Documentación de Azure Communication Services](https://docs.microsoft.com/es-es/azure/communication-services/)
- [Mejores prácticas para 2FA](https://www.nist.gov/itl/applied-cybersecurity/tig/back-basics-multi-factor-authentication)
- [Documentación completa de AuthSystem](./TwoFactorAuthentication.md)
- [Configuración detallada del servicio SMS](./ConfiguracionSMS.md)

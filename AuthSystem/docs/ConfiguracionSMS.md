# Configuración del Servicio SMS para Autenticación de Dos Factores

## Introducción

Este documento detalla la configuración e implementación del servicio SMS para la autenticación de dos factores (2FA) en el sistema AuthSystem. El servicio utiliza Azure Communication Services para el envío de mensajes SMS, con la opción de utilizar un servicio simulado para desarrollo y pruebas.

## Implementación

La implementación del servicio SMS incluye:

1. **Interfaz ISmsService**: Definida en el Core del sistema, proporciona métodos para enviar mensajes genéricos y códigos de verificación.
2. **Implementaciones concretas**:
   - **AzureSmsService**: Implementación real que utiliza Azure Communication Services.
   - **MockSmsService**: Implementación simulada para desarrollo y pruebas.
3. **Configuración flexible**: Permite activar/desactivar el servicio y elegir entre implementación real o simulada.

## Estructura del código

### Interfaz ISmsService

```csharp
public interface ISmsService
{
    Task SendMessageAsync(string phoneNumber, string message);
    Task SendVerificationCodeAsync(string phoneNumber, string code);
}
```

### AzureSmsService

```csharp
public class AzureSmsService : ISmsService
{
    private readonly string _connectionString;
    private readonly string _fromNumber;
    private readonly ILogger<AzureSmsService> _logger;

    public AzureSmsService(
        IOptions<AzureCommunicationSettings> settings,
        ILogger<AzureSmsService> logger)
    {
        _connectionString = settings.Value.ConnectionString;
        _fromNumber = settings.Value.FromNumber;
        _logger = logger;
    }

    public async Task SendMessageAsync(string phoneNumber, string message)
    {
        try
        {
            // Crear el cliente de SMS de Azure Communication Services
            var smsClient = new SmsClient(_connectionString);

            // Enviar el mensaje
            var response = await smsClient.SendAsync(
                from: _fromNumber,
                to: phoneNumber,
                message: message);

            // Registrar el resultado
            if (response.Successful)
            {
                _logger.LogInformation("SMS enviado exitosamente a {PhoneNumber}", phoneNumber);
            }
            else
            {
                _logger.LogWarning("Error al enviar SMS a {PhoneNumber}: {ErrorMessage}", 
                    phoneNumber, response.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar SMS a {PhoneNumber}", phoneNumber);
            throw;
        }
    }

    public async Task SendVerificationCodeAsync(string phoneNumber, string code)
    {
        string message = $"Su código de verificación para AuthSystem es: {code}. " +
                         "Este código es válido por 10 minutos.";
        await SendMessageAsync(phoneNumber, message);
    }
}
```

### MockSmsService

```csharp
public class MockSmsService : ISmsService
{
    private readonly ILogger<MockSmsService> _logger;

    public MockSmsService(ILogger<MockSmsService> logger)
    {
        _logger = logger;
    }

    public Task SendMessageAsync(string phoneNumber, string message)
    {
        _logger.LogInformation("MOCK SMS enviado a {PhoneNumber}: {Message}", phoneNumber, message);
        return Task.CompletedTask;
    }

    public Task SendVerificationCodeAsync(string phoneNumber, string code)
    {
        _logger.LogInformation("MOCK Código de verificación enviado a {PhoneNumber}: {Code}", 
            phoneNumber, code);
        return Task.CompletedTask;
    }
}
```

## Configuración

La configuración del servicio SMS se realiza en el archivo `appsettings.json`:

```json
{
  "SmsSettings": {
    "UseSmsService": true,
    "UseMockSmsService": false,
    "AzureCommunicationSettings": {
      "ConnectionString": "your-azure-connection-string",
      "FromNumber": "+1234567890"
    }
  }
}
```

### Opciones de configuración

- **UseSmsService**: Activa/desactiva completamente el servicio de SMS.
- **UseMockSmsService**: Determina si se usa la implementación real o simulada.
- **AzureCommunicationSettings**: Configuración específica para Azure Communication Services.
  - **ConnectionString**: Cadena de conexión de Azure Communication Services.
  - **FromNumber**: Número de teléfono desde el que se envían los SMS.

## Registro de servicios

El registro de los servicios en el contenedor de dependencias se realiza en `DependencyInjection.cs`:

```csharp
public static IServiceCollection AddSmsServices(this IServiceCollection services, IConfiguration configuration)
{
    // Registrar las opciones de configuración
    services.Configure<AzureCommunicationSettings>(
        configuration.GetSection("SmsSettings:AzureCommunicationSettings"));

    // Obtener la configuración
    bool useSmsService = configuration.GetValue<bool>("SmsSettings:UseSmsService");
    bool useMockSmsService = configuration.GetValue<bool>("SmsSettings:UseMockSmsService");

    if (useSmsService)
    {
        if (useMockSmsService)
        {
            // Registrar el servicio simulado
            services.AddScoped<ISmsService, MockSmsService>();
        }
        else
        {
            // Registrar el servicio real
            services.AddScoped<ISmsService, AzureSmsService>();
        }
    }
    else
    {
        // Registrar un servicio nulo si el servicio SMS está desactivado
        services.AddScoped<ISmsService, NullSmsService>();
    }

    return services;
}
```

## Configuración para diferentes entornos

### Desarrollo

Para desarrollo, se recomienda utilizar el servicio simulado:

```json
{
  "SmsSettings": {
    "UseSmsService": true,
    "UseMockSmsService": true
  }
}
```

### Pruebas

Para pruebas, puede utilizar el servicio simulado o el real con un número de teléfono de prueba:

```json
{
  "SmsSettings": {
    "UseSmsService": true,
    "UseMockSmsService": true
  }
}
```

### Producción

Para producción, debe utilizar el servicio real con la configuración adecuada:

```json
{
  "SmsSettings": {
    "UseSmsService": true,
    "UseMockSmsService": false,
    "AzureCommunicationSettings": {
      "ConnectionString": "su-connection-string-real",
      "FromNumber": "su-número-adquirido"
    }
  }
}
```

## Configuración de Azure Communication Services

Para configurar Azure Communication Services:

1. Cree un recurso de Azure Communication Services en el portal de Azure.
2. Adquiera un número de teléfono para envío de SMS.
3. Obtenga la cadena de conexión desde el portal de Azure.
4. Configure el servicio con la cadena de conexión y el número de teléfono.

## Consideraciones de seguridad

- **Protección de credenciales**: Asegúrese de que la cadena de conexión de Azure Communication Services esté protegida y no se incluya en el control de versiones.
- **Limitación de tasa**: Implemente limitación de tasa para prevenir abusos del servicio SMS.
- **Monitoreo**: Configure alertas para detectar patrones inusuales de uso o fallos en el envío de SMS.
- **Cumplimiento normativo**: Asegúrese de cumplir con las regulaciones locales sobre el envío de SMS.

## Solución de problemas

### No se envían SMS
- Verifique que `UseSmsService` esté establecido en `true`.
- Si `UseMockSmsService` es `false`, verifique que la configuración de Azure Communication Services sea correcta.
- Compruebe los logs de la aplicación para ver si hay errores específicos.

### Error en Azure Communication Services
- Verifique que la cadena de conexión sea válida y esté actualizada.
- Compruebe que el número de teléfono desde el que se envían los SMS esté activo y configurado correctamente.
- Verifique el saldo y los límites de su cuenta de Azure Communication Services.

### Problemas de formato de número de teléfono
- Asegúrese de que los números de teléfono estén en formato internacional (ej. +573001234567).
- Verifique que no haya espacios o caracteres especiales en los números de teléfono.

## Mejores prácticas

1. **Mensajes claros**: Los mensajes SMS deben ser claros y concisos, indicando el propósito del código y su tiempo de validez.
2. **Monitoreo**: Implemente monitoreo para detectar fallos en el envío de SMS y patrones sospechosos de uso.
3. **Pruebas regulares**: Realice pruebas regulares del servicio SMS para asegurarse de que funciona correctamente.
4. **Alternativas**: Proporcione métodos alternativos de autenticación en caso de que el servicio SMS no esté disponible.
5. **Auditoría**: Mantenga registros de auditoría de todos los SMS enviados para fines de seguridad y resolución de problemas.

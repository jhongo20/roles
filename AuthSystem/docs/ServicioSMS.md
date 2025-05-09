# Servicio SMS para Autenticación de Dos Factores

## Introducción

Este documento proporciona una descripción detallada de la implementación del servicio SMS en el sistema AuthSystem, utilizado principalmente para la autenticación de dos factores (2FA). El servicio SMS permite enviar códigos de verificación a los usuarios a través de mensajes de texto, proporcionando una capa adicional de seguridad en el proceso de autenticación.

## Arquitectura del Servicio SMS

### Componentes Principales

La implementación del servicio SMS se basa en una arquitectura flexible que permite cambiar fácilmente entre diferentes proveedores de servicios o utilizar implementaciones simuladas para desarrollo y pruebas.

#### Interfaces

La interfaz principal que define el contrato para el servicio SMS se encuentra en la capa Core:

```csharp
// AuthSystem.Core/Interfaces/ISmsService.cs
public interface ISmsService
{
    Task<bool> SendSmsAsync(string phoneNumber, string message);
    Task<bool> SendVerificationCodeAsync(string phoneNumber, string code);
}
```

Esta interfaz define dos métodos principales:
- `SendSmsAsync`: Envía un mensaje SMS genérico a un número de teléfono específico.
- `SendVerificationCodeAsync`: Envía un código de verificación específicamente formateado para 2FA.

#### Implementaciones

El sistema proporciona varias implementaciones de la interfaz `ISmsService`:

1. **AzureSmsService**: Implementación real que utiliza Azure Communication Services para enviar SMS.
2. **MockSmsService**: Implementación simulada para desarrollo y pruebas.
3. **NullSmsService**: Implementación que no hace nada, utilizada cuando el servicio SMS está desactivado.

##### AzureSmsService

```csharp
// AuthSystem.Infrastructure/Services/AzureSmsService.cs
public class AzureSmsService : ISmsService
{
    private readonly SmsClient _smsClient;
    private readonly string _senderPhoneNumber;
    private readonly ILogger<AzureSmsService> _logger;

    public AzureSmsService(IOptions<SmsSettings> smsSettings, ILogger<AzureSmsService> logger)
    {
        var settings = smsSettings.Value;
        _smsClient = new SmsClient(settings.AzureCommunicationSettings.ConnectionString);
        _senderPhoneNumber = settings.AzureCommunicationSettings.SenderPhoneNumber;
        _logger = logger;
    }

    public async Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        try
        {
            _logger.LogInformation("Enviando SMS a {PhoneNumber}", phoneNumber);
            
            var response = await _smsClient.SendAsync(
                from: _senderPhoneNumber,
                to: phoneNumber,
                message: message);

            var success = response.Successful.Count > 0;
            
            if (success)
            {
                _logger.LogInformation("SMS enviado exitosamente a {PhoneNumber}", phoneNumber);
            }
            else
            {
                _logger.LogWarning("Error al enviar SMS a {PhoneNumber}: {ErrorMessage}", 
                    phoneNumber, 
                    response.Failed.FirstOrDefault()?.Error.Message);
            }
            
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar SMS a {PhoneNumber}", phoneNumber);
            return false;
        }
    }

    public async Task<bool> SendVerificationCodeAsync(string phoneNumber, string code)
    {
        var message = $"Su código de verificación para AuthSystem es: {code}. Este código expirará en 10 minutos.";
        return await SendSmsAsync(phoneNumber, message);
    }
}
```

##### MockSmsService

```csharp
// AuthSystem.Infrastructure/Services/MockSmsService.cs
public class MockSmsService : ISmsService
{
    private readonly ILogger<MockSmsService> _logger;

    public MockSmsService(ILogger<MockSmsService> logger)
    {
        _logger = logger;
    }

    public Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        _logger.LogInformation("MOCK SMS - A: {PhoneNumber}, Mensaje: {Message}", phoneNumber, message);
        return Task.FromResult(true);
    }

    public Task<bool> SendVerificationCodeAsync(string phoneNumber, string code)
    {
        _logger.LogInformation("MOCK SMS - Código de verificación: {Code} enviado a {PhoneNumber}", code, phoneNumber);
        return Task.FromResult(true);
    }
}
```

##### NullSmsService

```csharp
// AuthSystem.Infrastructure/Services/NullSmsService.cs
public class NullSmsService : ISmsService
{
    private readonly ILogger<NullSmsService> _logger;

    public NullSmsService(ILogger<NullSmsService> logger)
    {
        _logger = logger;
    }

    public Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        _logger.LogDebug("Servicio SMS desactivado. No se enviará SMS a {PhoneNumber}", phoneNumber);
        return Task.FromResult(false);
    }

    public Task<bool> SendVerificationCodeAsync(string phoneNumber, string code)
    {
        _logger.LogDebug("Servicio SMS desactivado. No se enviará código de verificación a {PhoneNumber}", phoneNumber);
        return Task.FromResult(false);
    }
}
```

## Configuración del Servicio SMS

### Opciones de Configuración

La configuración del servicio SMS se realiza a través del archivo `appsettings.json`:

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

#### Opciones Disponibles

- `UseSmsService`: Activa o desactiva completamente el servicio SMS.
- `UseMockSmsService`: Indica si se debe utilizar la implementación simulada (para desarrollo) o la implementación real.
- `AzureCommunicationSettings.ConnectionString`: Cadena de conexión para Azure Communication Services.
- `AzureCommunicationSettings.SenderPhoneNumber`: Número de teléfono del remitente.

### Modelo de Configuración

```csharp
// AuthSystem.Infrastructure/Settings/SmsSettings.cs
public class SmsSettings
{
    public bool UseSmsService { get; set; } = true;
    public bool UseMockSmsService { get; set; } = false;
    public AzureCommunicationSettings AzureCommunicationSettings { get; set; } = new();
}

public class AzureCommunicationSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string SenderPhoneNumber { get; set; } = string.Empty;
}
```

### Registro de Servicios

El registro de los servicios SMS se realiza en la clase `DependencyInjection.cs` de la capa Infrastructure:

```csharp
// AuthSystem.Infrastructure/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Otras configuraciones...

        // Configuración del servicio SMS
        services.Configure<SmsSettings>(configuration.GetSection("SmsSettings"));
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

        // Otras configuraciones...

        return services;
    }
}
```

## Integración con Azure Communication Services

### Requisitos Previos

Para utilizar Azure Communication Services, debe:

1. Tener una suscripción de Azure.
2. Crear un recurso de Azure Communication Services en el portal de Azure.
3. Obtener la cadena de conexión del recurso.
4. Adquirir un número de teléfono para enviar SMS.

### Configuración en Azure

1. Inicie sesión en el [Portal de Azure](https://portal.azure.com).
2. Cree un nuevo recurso de Azure Communication Services.
3. Una vez creado el recurso, vaya a la sección "Claves" y copie la cadena de conexión.
4. Vaya a la sección "Números de teléfono" y adquiera un nuevo número de teléfono.
5. Configure la cadena de conexión y el número de teléfono en `appsettings.json` o mediante variables de entorno.

### Dependencias del Proyecto

Para utilizar Azure Communication Services, debe agregar el siguiente paquete NuGet al proyecto AuthSystem.Infrastructure:

```xml
<PackageReference Include="Azure.Communication.Sms" Version="1.0.1" />
```

## Uso del Servicio SMS en la Aplicación

### Envío de Códigos de Verificación para 2FA

El servicio SMS se utiliza principalmente en el proceso de autenticación de dos factores para enviar códigos de verificación a los usuarios. Esto se implementa en el handler de `SendTwoFactorCodeCommand`:

```csharp
// AuthSystem.Application/Commands/TwoFactor/SendTwoFactorCodeCommandHandler.cs
public class SendTwoFactorCodeCommandHandler : IRequestHandler<SendTwoFactorCodeCommand, SendTwoFactorCodeResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ITwoFactorService _twoFactorService;
    private readonly ISmsService _smsService;
    private readonly IEmailService _emailService;
    private readonly ILogger<SendTwoFactorCodeCommandHandler> _logger;

    public SendTwoFactorCodeCommandHandler(
        IUserRepository userRepository,
        ITwoFactorService twoFactorService,
        ISmsService smsService,
        IEmailService emailService,
        ILogger<SendTwoFactorCodeCommandHandler> logger)
    {
        _userRepository = userRepository;
        _twoFactorService = twoFactorService;
        _smsService = smsService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<SendTwoFactorCodeResponse> Handle(SendTwoFactorCodeCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            _logger.LogWarning("Intento de enviar código 2FA para usuario no existente: {UserId}", request.UserId);
            return new SendTwoFactorCodeResponse
            {
                Succeeded = false,
                Message = "Usuario no encontrado"
            };
        }

        if (!user.TwoFactorEnabled)
        {
            _logger.LogWarning("Intento de enviar código 2FA para usuario sin 2FA habilitado: {UserId}", request.UserId);
            return new SendTwoFactorCodeResponse
            {
                Succeeded = false,
                Message = "Autenticación de dos factores no habilitada para este usuario"
            };
        }

        // Generar código de verificación
        var code = _twoFactorService.GenerateCode();
        
        // Guardar el código en la base de datos
        await _twoFactorService.SaveVerificationCodeAsync(user.Id, code, request.IpAddress, request.UserAgent);

        // Enviar el código según el método preferido del usuario
        bool codeSent = false;
        
        if (user.TwoFactorMethod == TwoFactorMethod.SMS && !string.IsNullOrEmpty(user.PhoneNumber))
        {
            codeSent = await _smsService.SendVerificationCodeAsync(user.PhoneNumber, code);
        }
        else if (user.TwoFactorMethod == TwoFactorMethod.Email)
        {
            codeSent = await _emailService.SendTwoFactorCodeAsync(user.Email, code);
        }

        if (!codeSent)
        {
            _logger.LogError("Error al enviar código 2FA para usuario {UserId}", request.UserId);
            return new SendTwoFactorCodeResponse
            {
                Succeeded = false,
                Message = "Error al enviar el código de verificación"
            };
        }

        _logger.LogInformation("Código 2FA enviado exitosamente para usuario {UserId}", request.UserId);
        return new SendTwoFactorCodeResponse
        {
            Succeeded = true,
            Message = "Código de verificación enviado exitosamente"
        };
    }
}
```

## Consideraciones de Seguridad

### Protección de la Cadena de Conexión

La cadena de conexión de Azure Communication Services es información sensible y no debe almacenarse directamente en el código fuente o en archivos de configuración que se incluyan en el control de versiones. En su lugar, utilice:

1. **Variables de Entorno**:
   ```
   AUTHSYSTEM_SMSSETTINGS__AZURECOMMUNICATIONSETTINGS__CONNECTIONSTRING=su-cadena-de-conexion
   ```

2. **Secretos de Usuario de .NET Core** (para desarrollo):
   ```bash
   dotnet user-secrets set "SmsSettings:AzureCommunicationSettings:ConnectionString" "su-cadena-de-conexion" --project src/AuthSystem.API
   ```

3. **Azure Key Vault** u otro servicio de gestión de secretos (para producción).

### Limitación de Tasa

Para prevenir abusos del servicio SMS, implemente limitación de tasa para el envío de códigos de verificación:

```csharp
// Verificar si el usuario ha alcanzado el límite de envíos
if (await _rateLimitService.IsRateLimitedAsync(
    user.Id.ToString(), 
    "TwoFactorCodeSend", 
    3, // Máximo 3 intentos
    TimeSpan.FromMinutes(15))) // En un período de 15 minutos
{
    return new SendTwoFactorCodeResponse
    {
        Succeeded = false,
        Message = "Se ha alcanzado el límite de envíos de códigos. Intente nuevamente más tarde."
    };
}
```

### Validación de Números de Teléfono

Implemente validación adecuada de los números de teléfono para asegurarse de que estén en formato E.164 (formato internacional):

```csharp
public static class PhoneNumberValidator
{
    public static bool IsValidE164(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // Formato E.164: + seguido de 1 a 15 dígitos
        return Regex.IsMatch(phoneNumber, @"^\+[1-9]\d{1,14}$");
    }

    public static string FormatToE164(string phoneNumber, string defaultCountryCode = "+1")
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return string.Empty;

        // Eliminar caracteres no numéricos excepto el signo +
        var cleaned = Regex.Replace(phoneNumber, @"[^\d+]", "");

        // Si ya está en formato E.164, devolverlo tal cual
        if (Regex.IsMatch(cleaned, @"^\+[1-9]\d{1,14}$"))
            return cleaned;

        // Si comienza con 00, reemplazar por +
        if (cleaned.StartsWith("00"))
            return "+" + cleaned.Substring(2);

        // Si no comienza con +, agregar el código de país predeterminado
        if (!cleaned.StartsWith("+"))
            return defaultCountryCode + (cleaned.StartsWith("0") ? cleaned.Substring(1) : cleaned);

        return cleaned;
    }
}
```

## Monitoreo y Logging

### Logging de Eventos

El servicio SMS implementa logging detallado para facilitar el monitoreo y la solución de problemas:

```csharp
_logger.LogInformation("Enviando SMS a {PhoneNumber}", phoneNumber);

// Si el envío es exitoso
_logger.LogInformation("SMS enviado exitosamente a {PhoneNumber}", phoneNumber);

// Si hay un error
_logger.LogWarning("Error al enviar SMS a {PhoneNumber}: {ErrorMessage}", 
    phoneNumber, 
    response.Failed.FirstOrDefault()?.Error.Message);

// Si hay una excepción
_logger.LogError(ex, "Error al enviar SMS a {PhoneNumber}", phoneNumber);
```

### Métricas

Para un monitoreo más avanzado, considere implementar métricas para el servicio SMS:

```csharp
public class SmsMetrics
{
    private readonly IMetricsFactory _metricsFactory;
    private readonly Counter _smsSentCounter;
    private readonly Counter _smsFailedCounter;
    private readonly Histogram _smsLatencyHistogram;

    public SmsMetrics(IMetricsFactory metricsFactory)
    {
        _metricsFactory = metricsFactory;
        _smsSentCounter = _metricsFactory.CreateCounter("sms_sent_total", "Total de SMS enviados");
        _smsFailedCounter = _metricsFactory.CreateCounter("sms_failed_total", "Total de SMS fallidos");
        _smsLatencyHistogram = _metricsFactory.CreateHistogram("sms_latency_seconds", "Latencia de envío de SMS en segundos");
    }

    public void RecordSmsSent()
    {
        _smsSentCounter.Increment();
    }

    public void RecordSmsFailed()
    {
        _smsFailedCounter.Increment();
    }

    public void RecordSmsLatency(TimeSpan latency)
    {
        _smsLatencyHistogram.Record(latency.TotalSeconds);
    }
}
```

## Pruebas

### Pruebas Unitarias

Para facilitar las pruebas unitarias, utilice la implementación `MockSmsService` y asegúrese de que su código esté diseñado para ser testeable:

```csharp
[Fact]
public async Task SendVerificationCode_ShouldSendCodeViaPreferredMethod()
{
    // Arrange
    var userId = Guid.NewGuid();
    var phoneNumber = "+1234567890";
    var mockUserRepository = new Mock<IUserRepository>();
    var mockTwoFactorService = new Mock<ITwoFactorService>();
    var mockSmsService = new Mock<ISmsService>();
    var mockEmailService = new Mock<IEmailService>();
    var mockLogger = new Mock<ILogger<SendTwoFactorCodeCommandHandler>>();

    var user = new User("testuser", "test@example.com", "hashedpassword")
    {
        Id = userId,
        PhoneNumber = phoneNumber,
        TwoFactorEnabled = true,
        TwoFactorMethod = TwoFactorMethod.SMS
    };

    mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
    mockTwoFactorService.Setup(s => s.GenerateCode()).Returns("123456");
    mockSmsService.Setup(s => s.SendVerificationCodeAsync(phoneNumber, "123456")).ReturnsAsync(true);

    var handler = new SendTwoFactorCodeCommandHandler(
        mockUserRepository.Object,
        mockTwoFactorService.Object,
        mockSmsService.Object,
        mockEmailService.Object,
        mockLogger.Object);

    var command = new SendTwoFactorCodeCommand
    {
        UserId = userId,
        IpAddress = "127.0.0.1",
        UserAgent = "Test"
    };

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.True(result.Succeeded);
    mockSmsService.Verify(s => s.SendVerificationCodeAsync(phoneNumber, "123456"), Times.Once);
    mockEmailService.Verify(s => s.SendTwoFactorCodeAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
}
```

### Pruebas de Integración

Para pruebas de integración, puede utilizar la implementación `MockSmsService` o crear un entorno de prueba con Azure Communication Services:

```csharp
[Fact]
public async Task AzureSmsService_SendSms_ShouldSendMessage()
{
    // Arrange
    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.Test.json")
        .AddUserSecrets<SmsServiceTests>()
        .Build();

    var services = new ServiceCollection();
    services.Configure<SmsSettings>(configuration.GetSection("SmsSettings"));
    services.AddLogging();
    services.AddScoped<ISmsService, AzureSmsService>();

    var serviceProvider = services.BuildServiceProvider();
    var smsService = serviceProvider.GetRequiredService<ISmsService>();

    // Act
    var result = await smsService.SendSmsAsync("+1234567890", "Test message from integration test");

    // Assert
    Assert.True(result);
}
```

## Solución de Problemas Comunes

### Problemas con Azure Communication Services

1. **Error de autenticación**:
   - Verificar que la cadena de conexión sea correcta y esté actualizada.
   - Comprobar que el recurso de Azure Communication Services esté activo.

2. **Número de teléfono no válido**:
   - Asegurarse de que el número de teléfono esté en formato E.164 (por ejemplo, +1234567890).
   - Verificar que el número de teléfono sea válido y esté activo.

3. **Límites de servicio alcanzados**:
   - Verificar los límites de cuota de Azure Communication Services.
   - Considerar aumentar los límites o implementar una estrategia de limitación de tasa más restrictiva.

### Problemas con la Configuración

1. **Servicio SMS no registrado**:
   - Verificar que la configuración en `appsettings.json` sea correcta.
   - Comprobar que el servicio esté correctamente registrado en `DependencyInjection.cs`.

2. **Servicio simulado en producción**:
   - Verificar que `UseMockSmsService` esté configurado como `false` en producción.
   - Comprobar que las variables de entorno no estén sobrescribiendo la configuración.

## Conclusión

El servicio SMS proporciona una capa adicional de seguridad para el sistema AuthSystem a través de la autenticación de dos factores. La implementación flexible permite cambiar fácilmente entre diferentes proveedores de servicios o utilizar implementaciones simuladas para desarrollo y pruebas.

La integración con Azure Communication Services proporciona una solución robusta y escalable para el envío de SMS, mientras que las implementaciones alternativas permiten adaptarse a diferentes escenarios y requisitos.

## Referencias

- [Documentación de Azure Communication Services](https://docs.microsoft.com/azure/communication-services)
- [SDK de Azure Communication Services para .NET](https://github.com/Azure/azure-sdk-for-net/tree/master/sdk/communication)
- [Mejores prácticas para la autenticación de dos factores](https://www.nist.gov/itl/applied-cybersecurity/tig/back-basics-multi-factor-authentication)
- [Formato E.164 para números de teléfono](https://en.wikipedia.org/wiki/E.164)

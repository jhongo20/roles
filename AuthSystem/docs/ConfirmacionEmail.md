# Confirmación de Correo Electrónico

## Introducción

El sistema de confirmación de correo electrónico en AuthSystem proporciona un mecanismo seguro para verificar la identidad de los usuarios a través de su dirección de correo electrónico. Este proceso es fundamental para garantizar que los usuarios proporcionen direcciones de correo válidas y que tengan control sobre ellas, lo que aumenta la seguridad del sistema y reduce el riesgo de cuentas fraudulentas.

## Componentes Principales

### EmailConfirmationController

El controlador `EmailConfirmationController` expone los endpoints necesarios para la confirmación de correo electrónico.

**Endpoints principales:**
- Generación de tokens de confirmación
- Verificación de tokens
- Reenvío de correos de confirmación
- Consulta del estado de confirmación

### Comandos y Handlers

El sistema utiliza el patrón CQRS con MediatR para implementar los casos de uso relacionados con la confirmación de correo electrónico:

1. **GenerateEmailConfirmationTokenCommand**: Genera un token único para la confirmación de correo.
2. **VerifyEmailConfirmationTokenCommand**: Verifica un token de confirmación y marca el correo como confirmado.
3. **ResendEmailConfirmationCommand**: Reenvía el correo de confirmación con un nuevo token.
4. **GetEmailConfirmationStatusQuery**: Consulta el estado de confirmación de un correo electrónico.

### Servicios de Email

El sistema incluye servicios para el envío de correos electrónicos:

1. **IEmailService**: Interfaz que define los métodos para enviar correos.
2. **SmtpEmailService**: Implementación que utiliza SMTP para enviar correos reales.
3. **MockEmailService**: Implementación simulada para desarrollo y pruebas.

### Plantillas HTML

El sistema utiliza plantillas HTML personalizables para los correos electrónicos de confirmación, lo que permite una presentación profesional y coherente con la imagen de la aplicación.

### RateLimitService

El servicio de limitación de intentos (`RateLimitService`) previene abusos del sistema, limitando el número de solicitudes de confirmación que un usuario puede realizar en un período determinado.

## Flujo de Confirmación de Email

### Registro de Usuario y Generación de Token

1. Un usuario se registra en el sistema proporcionando su información, incluyendo su dirección de correo electrónico.
2. El sistema crea la cuenta de usuario con el estado "Registrado" (no "Activo").
3. Se genera un token único de confirmación utilizando `GenerateEmailConfirmationTokenCommand`.
4. El token se almacena en la base de datos asociado al usuario mediante `SaveEmailConfirmationTokenAsync`.
5. Se envía un correo electrónico al usuario con un enlace que contiene el token de confirmación.

### Verificación del Token

1. El usuario recibe el correo y hace clic en el enlace de confirmación.
2. El enlace dirige al usuario a la aplicación con el token como parámetro.
3. La aplicación envía el token al endpoint de verificación.
4. El sistema valida el token utilizando `ValidateEmailConfirmationTokenAsync`.
5. Si el token es válido, se marca el correo electrónico como confirmado mediante `ConfirmEmailAsync`.
6. Se actualiza el estado del usuario a "Activo" si estaba en estado "Registrado".
7. Se elimina el token utilizado de la base de datos.

### Reenvío de Correo de Confirmación

1. Si el usuario no recibe el correo o el token expira, puede solicitar un nuevo correo de confirmación.
2. El sistema verifica que el usuario exista y que su correo no esté ya confirmado.
3. Se eliminan los tokens anteriores mediante `DeleteAllEmailConfirmationTokensForUserAsync`.
4. Se genera un nuevo token y se envía un nuevo correo de confirmación.
5. Se aplican limitaciones de tasa para prevenir abusos.

## Implementación Técnica

### Entidades y Modelos

```csharp
// Token de confirmación de correo electrónico
public class EmailConfirmationToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
}
```

### Interfaces del Repositorio

La interfaz `IUserRepository` incluye métodos específicos para la confirmación de correo electrónico:

```csharp
public interface IUserRepository
{
    // Métodos para confirmación de email
    Task SaveEmailConfirmationTokenAsync(EmailConfirmationToken token);
    Task<EmailConfirmationToken> GetEmailConfirmationTokenAsync(Guid userId, string token);
    Task DeleteEmailConfirmationTokenAsync(Guid tokenId);
    Task DeleteAllEmailConfirmationTokensForUserAsync(Guid userId);
    Task StoreEmailConfirmationTokenAsync(Guid userId, string token);
    Task<(bool IsValid, User User)> ValidateEmailConfirmationTokenAsync(Guid userId, string token);
    Task<bool> ConfirmEmailAsync(Guid userId);
    
    // Otros métodos...
}
```

### Comandos y Handlers

#### GenerateEmailConfirmationTokenCommand

```csharp
public class GenerateEmailConfirmationTokenCommand : IRequest<GenerateEmailConfirmationTokenResponse>
{
    public string Email { get; set; }
    public string CallbackUrl { get; set; }
}

public class GenerateEmailConfirmationTokenCommandHandler : IRequestHandler<GenerateEmailConfirmationTokenCommand, GenerateEmailConfirmationTokenResponse>
{
    // Implementación...
}
```

#### VerifyEmailConfirmationTokenCommand

```csharp
public class VerifyEmailConfirmationTokenCommand : IRequest<VerifyEmailConfirmationTokenResponse>
{
    public Guid UserId { get; set; }
    public string Token { get; set; }
}

public class VerifyEmailConfirmationTokenCommandHandler : IRequestHandler<VerifyEmailConfirmationTokenCommand, VerifyEmailConfirmationTokenResponse>
{
    // Implementación...
}
```

### Validadores

El sistema utiliza FluentValidation para validar los comandos y consultas:

```csharp
public class GenerateEmailConfirmationTokenCommandValidator : AbstractValidator<GenerateEmailConfirmationTokenCommand>
{
    public GenerateEmailConfirmationTokenCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
            .EmailAddress().WithMessage("El formato del correo electrónico no es válido.");

        RuleFor(x => x.CallbackUrl)
            .NotEmpty().WithMessage("La URL de retorno es obligatoria.");
    }
}
```

### Servicio de Email

```csharp
public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task SendTemplatedEmailAsync(string to, string subject, string templateName, object model);
}
```

## Endpoints de API

### POST /api/email-confirmation/generate

Genera un token de confirmación y envía un correo electrónico.

**Request:**
```json
{
  "email": "usuario@ejemplo.com",
  "callbackUrl": "https://miapp.com/confirmar-email?token={token}&userId={userId}"
}
```

**Response:**
```json
{
  "succeeded": true,
  "message": "Correo de confirmación enviado exitosamente"
}
```

### POST /api/email-confirmation/verify

Verifica un token de confirmación.

**Request:**
```json
{
  "userId": "guid-del-usuario",
  "token": "token-de-confirmacion"
}
```

**Response:**
```json
{
  "succeeded": true,
  "message": "Correo electrónico confirmado exitosamente"
}
```

### POST /api/email-confirmation/resend

Reenvía un correo de confirmación.

**Request:**
```json
{
  "email": "usuario@ejemplo.com",
  "callbackUrl": "https://miapp.com/confirmar-email?token={token}&userId={userId}"
}
```

**Response:**
```json
{
  "succeeded": true,
  "message": "Correo de confirmación reenviado exitosamente"
}
```

### GET /api/email-confirmation/status/{userId}

Consulta el estado de confirmación de un correo electrónico.

**Response:**
```json
{
  "isConfirmed": true,
  "email": "usuario@ejemplo.com",
  "confirmationDate": "2023-01-01T12:00:00Z"
}
```

## Plantillas de Correo Electrónico

El sistema utiliza plantillas HTML para los correos electrónicos, que pueden personalizarse según las necesidades de la aplicación. Las plantillas se almacenan en la carpeta `Templates` y se procesan utilizando un motor de plantillas.

Ejemplo de plantilla para confirmación de correo:

```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <title>Confirmación de Correo Electrónico</title>
    <style>
        /* Estilos CSS */
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <img src="{{LogoUrl}}" alt="Logo" />
        </div>
        <div class="content">
            <h1>Confirmación de Correo Electrónico</h1>
            <p>Hola {{UserName}},</p>
            <p>Gracias por registrarte en nuestra plataforma. Por favor, confirma tu dirección de correo electrónico haciendo clic en el siguiente botón:</p>
            <div class="button-container">
                <a href="{{ConfirmationUrl}}" class="button">Confirmar Correo Electrónico</a>
            </div>
            <p>Si no solicitaste esta confirmación, puedes ignorar este correo.</p>
            <p>Este enlace expirará en {{ExpirationHours}} horas.</p>
        </div>
        <div class="footer">
            <p>&copy; {{CurrentYear}} AuthSystem. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>
```

## Configuración

La configuración del sistema de confirmación de correo electrónico se realiza en `appsettings.json`:

```json
{
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
  "EmailConfirmationSettings": {
    "TokenExpirationHours": 24,
    "MaxResendAttempts": 5,
    "ResendCooldownMinutes": 15
  }
}
```

## Seguridad y Consideraciones

### Expiración de Tokens

Los tokens de confirmación tienen una fecha de expiración configurable (por defecto, 24 horas). Después de este período, el token ya no es válido y el usuario debe solicitar uno nuevo.

### Limitación de Tasa

El sistema implementa limitación de tasa para prevenir abusos:
- Límite en el número de solicitudes de reenvío por usuario
- Período de enfriamiento entre solicitudes
- Bloqueo temporal después de múltiples intentos fallidos

### Protección contra Ataques de Fuerza Bruta

Los tokens de confirmación son suficientemente largos y aleatorios para resistir ataques de fuerza bruta. Además, el sistema limita el número de intentos de verificación fallidos.

### Auditoría

Todas las operaciones relacionadas con la confirmación de correo electrónico se registran en el sistema de auditoría, lo que permite rastrear actividades sospechosas.

## Mejores Prácticas

1. **Tokens Seguros**: Utilizar tokens suficientemente largos y aleatorios.
2. **Expiración de Tokens**: Configurar un tiempo de expiración adecuado para los tokens.
3. **Limitación de Tasa**: Implementar limitación de tasa para prevenir abusos.
4. **Mensajes Claros**: Proporcionar mensajes claros y útiles en los correos electrónicos y en la interfaz de usuario.
5. **Plantillas Responsivas**: Diseñar plantillas de correo electrónico que se vean bien en diferentes dispositivos y clientes de correo.

## Solución de Problemas

### Problemas comunes

1. **No se recibe el correo de confirmación**
   - Verificar que la dirección de correo sea correcta
   - Comprobar la carpeta de spam
   - Verificar la configuración SMTP
   - Solicitar un reenvío del correo

2. **Error al confirmar el correo**
   - Verificar que el token no haya expirado
   - Comprobar que el token no haya sido utilizado previamente
   - Verificar que el usuario exista en la base de datos

3. **Limitación de tasa**
   - Esperar el período de enfriamiento antes de solicitar un nuevo reenvío
   - Contactar al soporte si el problema persiste

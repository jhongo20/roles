# Sistema de Confirmación de Email

## Descripción General

El sistema de confirmación de email permite verificar la autenticidad de las direcciones de correo electrónico de los usuarios. Esto ayuda a prevenir el registro de cuentas falsas y mejora la seguridad del sistema.

## Componentes Principales

### 1. Controlador de Confirmación de Email

El controlador `EmailConfirmationController` proporciona endpoints para:
- Generar tokens de confirmación
- Verificar tokens y confirmar cuentas
- Reenviar correos de confirmación
- Verificar el estado de confirmación de un email

### 2. Servicio de Plantillas de Email

El servicio `EmailTemplateService` permite:
- Cargar plantillas HTML desde archivos
- Reemplazar variables en las plantillas
- Renderizar el contenido HTML final

### 3. Servicio de Email

El servicio `EmailService` se encarga de:
- Enviar correos electrónicos utilizando SMTP
- Utilizar las plantillas para generar el contenido de los correos
- Manejar errores y reintentos

### 4. Servicio de Email de Prueba

El servicio `MockEmailService` permite:
- Simular el envío de correos durante el desarrollo
- Registrar los correos enviados para pruebas
- Evitar la dependencia de un servidor SMTP real

### 5. Sistema de Cola de Emails

El servicio `BackgroundEmailSender` proporciona:
- Procesamiento asíncrono de correos electrónicos
- Reintentos automáticos en caso de fallos
- Mejor rendimiento y escalabilidad

### 6. Seguridad y Limitación de Intentos

El servicio `RateLimitService` implementa:
- Limitación de intentos para prevenir abusos
- Ventanas de tiempo configurables
- Protección contra ataques de fuerza bruta

## Flujo de Confirmación de Email

1. **Registro de Usuario**:
   - El usuario se registra proporcionando su email
   - El sistema genera un token único
   - Se envía un correo de confirmación con el token

2. **Confirmación**:
   - El usuario hace clic en el enlace del correo
   - El sistema verifica el token
   - Si es válido, se confirma la cuenta

3. **Reenvío de Confirmación**:
   - Si el usuario no recibe el correo, puede solicitar un reenvío
   - El sistema genera un nuevo token
   - Se envía un nuevo correo de confirmación

## Personalización de Plantillas

Las plantillas de correo se encuentran en la carpeta `Email/Templates` y pueden ser personalizadas según las necesidades del proyecto.

### Variables Disponibles

- `{{UserName}}`: Nombre del usuario
- `{{ConfirmationUrl}}`: URL para confirmar la cuenta
- `{{ResetUrl}}`: URL para restablecer la contraseña
- `{{VerificationCode}}`: Código de verificación para 2FA
- `{{CurrentYear}}`: Año actual
- `{{CompanyName}}`: Nombre de la empresa

### Ejemplo de Personalización

Para personalizar una plantilla:

1. Abra el archivo HTML correspondiente
2. Modifique el diseño según sus preferencias
3. Asegúrese de mantener las variables entre dobles llaves `{{Variable}}`
4. Guarde los cambios

## Configuración

La configuración del servicio de email se realiza en el archivo `appsettings.json`:

```json
"EmailSettings": {
  "FromEmail": "noreply@example.com",
  "FromName": "Nombre de la Aplicación",
  "SmtpHost": "smtp.example.com",
  "SmtpPort": 587,
  "EnableSsl": true,
  "Username": "usuario-smtp",
  "Password": "contraseña-smtp",
  "WebsiteBaseUrl": "https://example.com"
},
"UseMockEmailService": true
```

Para habilitar el servicio de email de prueba, establezca `UseMockEmailService` en `true`. Para utilizar un servidor SMTP real, establezca este valor en `false` y configure los parámetros en `EmailSettings`.

## Endpoints de la API

### Generar Token de Confirmación

```
POST /api/EmailConfirmation/generate-token
```

**Cuerpo de la solicitud**:
```json
{
  "email": "usuario@example.com"
}
```

**Respuesta exitosa**:
```json
{
  "message": "Se ha enviado un correo de confirmación"
}
```

### Verificar Token

```
POST /api/EmailConfirmation/verify-token
```

**Cuerpo de la solicitud**:
```json
{
  "userId": "00000000-0000-0000-0000-000000000000",
  "token": "token-de-confirmacion"
}
```

**Respuesta exitosa**:
```json
{
  "message": "Cuenta confirmada exitosamente"
}
```

### Reenviar Correo de Confirmación

```
POST /api/EmailConfirmation/resend
```

**Cuerpo de la solicitud**:
```json
{
  "email": "usuario@example.com"
}
```

**Respuesta exitosa**:
```json
{
  "message": "Se ha reenviado el correo de confirmación"
}
```

### Verificar Estado de Confirmación

```
GET /api/EmailConfirmation/status?email=usuario@example.com
```

**Respuesta exitosa**:
```json
{
  "isConfirmed": true,
  "message": "Email confirmado"
}
```

## Seguridad

El sistema implementa varias medidas de seguridad:

- Tokens de un solo uso
- Expiración de tokens
- Limitación de intentos de reenvío
- Protección contra ataques de fuerza bruta

## Pruebas

Se han implementado pruebas unitarias y de integración para verificar el correcto funcionamiento del sistema:

- Pruebas del servicio de plantillas
- Pruebas del servicio de email
- Pruebas de los comandos y handlers

## Solución de Problemas

### Correos no enviados

1. Verifique la configuración SMTP en `appsettings.json`
2. Compruebe los logs para ver si hay errores específicos
3. Asegúrese de que el servidor SMTP esté accesible

### Tokens inválidos

1. Verifique que el token no haya expirado
2. Compruebe que el usuario exista y no esté bloqueado
3. Asegúrese de que el token no haya sido utilizado previamente

### Plantillas no encontradas

1. Verifique que las plantillas estén en la ubicación correcta
2. Compruebe que los nombres de las plantillas sean correctos
3. Asegúrese de que las plantillas se copien a la carpeta de salida durante la compilación

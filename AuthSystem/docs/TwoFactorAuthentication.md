# Documentación de Autenticación de Dos Factores (2FA) con SMS

## Introducción

Este documento describe la implementación de autenticación de dos factores (2FA) mediante SMS en el sistema AuthSystem. Esta funcionalidad proporciona una capa adicional de seguridad al proceso de inicio de sesión, requiriendo que los usuarios verifiquen su identidad a través de un código enviado a su teléfono móvil.

## Arquitectura

La implementación de 2FA con SMS sigue la arquitectura general del sistema, basada en:

- **Patrón CQRS**: Utilizando comandos y consultas para separar las operaciones de lectura y escritura.
- **MediatR**: Para la implementación del patrón mediador y manejo de comandos.
- **Capas de aplicación**: Separación clara entre API, Application, Core e Infrastructure.

### Componentes principales

1. **Controladores**:
   - `AuthController`: Maneja la autenticación básica.
   - `TwoFactorController`: Gestiona las operaciones específicas de 2FA.
   - `UserProfileController`: Permite a los usuarios gestionar su información de perfil, incluyendo su número de teléfono.

2. **Servicios**:
   - `ISmsService`: Interfaz para el envío de SMS.
   - `AzureSmsService`: Implementación real utilizando Azure Communication Services.
   - `MockSmsService`: Implementación simulada para desarrollo y pruebas.
   - `ITotpService`: Servicio para generar y validar códigos TOTP (Time-based One-Time Password).

3. **Comandos**:
   - `SendTwoFactorCodeCommand`: Envía un código de verificación por SMS.
   - `VerifyTwoFactorCommand`: Verifica el código proporcionado por el usuario.
   - `EnableTwoFactorCommand`: Habilita 2FA para un usuario.
   - `DisableTwoFactorCommand`: Deshabilita 2FA para un usuario.
   - `TwoFactorLoginCommand`: Completa el proceso de login con 2FA.

## Endpoints de API

### Autenticación básica

#### POST /api/auth/login
Inicia el proceso de autenticación con nombre de usuario y contraseña.

**Request:**
```json
{
  "username": "string",
  "password": "string",
  "rememberMe": boolean,
  "recaptchaToken": "string"
}
```

**Response (sin 2FA):**
```json
{
  "succeeded": true,
  "token": "string",
  "refreshToken": "string",
  "requirePasswordChange": boolean,
  "user": {
    "id": "guid",
    "username": "string",
    "email": "string",
    "firstName": "string",
    "lastName": "string",
    "status": "string",
    "lastLoginDate": "datetime",
    "twoFactorEnabled": false,
    "createdAt": "datetime"
  },
  "message": "string"
}
```

**Response (con 2FA habilitado):**
```json
{
  "succeeded": true,
  "requiresTwoFactor": true,
  "userId": "guid",
  "message": "Se requiere verificación de dos factores"
}
```

#### POST /api/auth/refresh-token
Renueva un token JWT expirado.

**Request:**
```json
{
  "token": "string",
  "refreshToken": "string"
}
```

**Response:**
```json
{
  "succeeded": true,
  "token": "string",
  "refreshToken": "string",
  "requirePasswordChange": boolean,
  "user": {
    "id": "guid",
    "username": "string",
    "email": "string",
    "firstName": "string",
    "lastName": "string",
    "status": "string",
    "lastLoginDate": "datetime",
    "twoFactorEnabled": boolean,
    "createdAt": "datetime"
  },
  "message": "string"
}
```

#### POST /api/auth/logout
Cierra la sesión del usuario.

**Request:**
```json
{
  "token": "string"
}
```

**Response:**
```json
{
  "succeeded": true,
  "message": "Sesión cerrada exitosamente"
}
```

### Autenticación de dos factores

#### POST /api/twofactor/send-code
Envía un código de verificación por SMS al usuario.

**Request:**
```json
{
  "userId": "guid"
}
```

**Response:**
```json
{
  "succeeded": true,
  "message": "Código enviado exitosamente"
}
```

#### POST /api/twofactor/verify
Verifica el código de autenticación de dos factores.

**Request:**
```json
{
  "userId": "guid",
  "code": "string",
  "rememberMe": boolean
}
```

**Response:**
```json
{
  "succeeded": true,
  "token": "string",
  "refreshToken": "string",
  "requirePasswordChange": boolean,
  "user": {
    "id": "guid",
    "username": "string",
    "email": "string",
    "firstName": "string",
    "lastName": "string",
    "status": "string",
    "lastLoginDate": "datetime",
    "twoFactorEnabled": boolean,
    "createdAt": "datetime"
  },
  "message": "string"
}
```

#### POST /api/twofactor/enable
Habilita la autenticación de dos factores para un usuario.

**Request:**
```json
{
  "phoneNumber": "string"
}
```

**Response:**
```json
{
  "succeeded": true,
  "message": "Autenticación de dos factores habilitada exitosamente"
}
```

#### POST /api/twofactor/disable
Deshabilita la autenticación de dos factores para un usuario.

**Request:**
```json
{
  "password": "string"
}
```

**Response:**
```json
{
  "succeeded": true,
  "message": "Autenticación de dos factores deshabilitada exitosamente"
}
```

### Gestión de perfil de usuario

#### GET /api/userprofile
Obtiene el perfil del usuario actual.

**Response:**
```json
{
  "id": "guid",
  "username": "string",
  "email": "string",
  "firstName": "string",
  "lastName": "string",
  "phoneNumber": "string",
  "status": "string",
  "lastLoginDate": "datetime",
  "twoFactorEnabled": boolean,
  "createdAt": "datetime"
}
```

#### PUT /api/userprofile
Actualiza el perfil del usuario.

**Request:**
```json
{
  "firstName": "string",
  "lastName": "string",
  "phoneNumber": "string"
}
```

**Response:**
```json
{
  "succeeded": true,
  "message": "Perfil actualizado exitosamente"
}
```

#### PUT /api/userprofile/phone
Actualiza específicamente el número de teléfono.

**Request:**
```json
{
  "phoneNumber": "string"
}
```

**Response:**
```json
{
  "succeeded": true,
  "message": "Número de teléfono actualizado exitosamente"
}
```

#### PUT /api/userprofile/password
Cambia la contraseña del usuario.

**Request:**
```json
{
  "currentPassword": "string",
  "newPassword": "string",
  "confirmPassword": "string"
}
```

**Response:**
```json
{
  "succeeded": true,
  "message": "Contraseña cambiada exitosamente"
}
```

## Flujo de autenticación con 2FA

1. **Inicio de sesión**: El usuario inicia sesión con su nombre de usuario y contraseña.
2. **Verificación de 2FA**: Si el usuario tiene 2FA habilitado, el sistema devuelve `requiresTwoFactor: true` y `userId`.
3. **Envío de código**: La aplicación cliente solicita el envío de un código de verificación.
4. **Recepción de código**: El usuario recibe el código por SMS.
5. **Verificación de código**: El usuario ingresa el código en la aplicación, que lo envía al servidor para su verificación.
6. **Autenticación completa**: Si el código es válido, el sistema devuelve un token JWT para autenticar al usuario.

## Configuración

La funcionalidad de SMS para 2FA se configura en el archivo `appsettings.json`:

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

## Configuración para producción

Para usar el servicio SMS en producción, siga estos pasos:

1. Cree un recurso de Azure Communication Services en el portal de Azure.
2. Adquiera un número de teléfono para envío de SMS.
3. Actualice la configuración en `appsettings.json`:
   ```json
   "UseSmsService": true,
   "UseMockSmsService": false,
   "AzureCommunicationSettings": {
     "ConnectionString": "su-connection-string-real",
     "FromNumber": "su-número-adquirido"
   }
   ```

## Seguridad

La implementación de 2FA con SMS incluye varias medidas de seguridad:

1. **Códigos de un solo uso**: Los códigos generados son válidos por un tiempo limitado.
2. **Límite de intentos**: Se implementa un límite de intentos fallidos para prevenir ataques de fuerza bruta.
3. **Auditoría**: Se registran todos los intentos de autenticación, tanto exitosos como fallidos.
4. **Revocación de sesiones**: Los usuarios pueden cerrar sesión y revocar todas sus sesiones activas.

## Consideraciones para desarrollo y pruebas

Durante el desarrollo y las pruebas, puede utilizar el servicio simulado de SMS configurando:

```json
"UseSmsService": true,
"UseMockSmsService": true
```

Esto evitará el envío real de SMS y mostrará los códigos en los logs de la aplicación.

## Solución de problemas comunes

### No se reciben los SMS
- Verifique que `UseSmsService` esté establecido en `true`.
- Si `UseMockSmsService` es `false`, verifique que la configuración de Azure Communication Services sea correcta.
- Compruebe que el número de teléfono del usuario esté en formato internacional (ej. +573001234567).

### Error al verificar el código
- Asegúrese de que el código no haya expirado (validez típica de 5-10 minutos).
- Verifique que el usuario esté ingresando el código más reciente enviado.
- Compruebe que el reloj del servidor esté sincronizado correctamente.

### Problemas al habilitar 2FA
- Verifique que el usuario tenga un número de teléfono válido registrado.
- Compruebe que el servicio de SMS esté correctamente configurado.
- Asegúrese de que el usuario tenga los permisos necesarios para modificar su configuración de 2FA.

## Mejores prácticas

1. **Formato de número de teléfono**: Almacene y utilice siempre números de teléfono en formato internacional (ej. +573001234567).
2. **Mensajes claros**: Los mensajes SMS deben ser claros y concisos, indicando el propósito del código y su tiempo de validez.
3. **Alternativas de recuperación**: Proporcione métodos alternativos de recuperación en caso de que el usuario no pueda acceder a su teléfono.
4. **Monitoreo**: Implemente monitoreo para detectar fallos en el envío de SMS y patrones sospechosos de uso.

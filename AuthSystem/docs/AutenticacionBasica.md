# Autenticación Básica

## Introducción

El sistema AuthSystem proporciona un mecanismo robusto de autenticación básica que permite a los usuarios iniciar sesión utilizando sus credenciales (nombre de usuario o correo electrónico y contraseña). Este documento describe los componentes, flujos y configuraciones relacionados con la autenticación básica.

## Componentes Principales

### JwtService

El servicio `JwtService` es responsable de la generación y validación de tokens JWT (JSON Web Tokens), que se utilizan para mantener el estado de autenticación del usuario.

**Funcionalidades principales:**
- Generación de tokens JWT con claims personalizados
- Validación de tokens JWT
- Generación de refresh tokens para mantener la sesión
- Revocación de tokens

**Implementación:**
```csharp
public interface IJwtService
{
    Task<(string token, string refreshToken)> GenerateTokensAsync(User user, bool rememberMe = false);
    Task<(bool isValid, string userId, string jti)> ValidateTokenAsync(string token);
    Task<bool> IsTokenRevokedAsync(string userId, string jti);
}
```

### PasswordHasher

El servicio `PasswordHasher` se encarga de la generación y verificación segura de hashes de contraseñas utilizando BCrypt.

**Funcionalidades principales:**
- Generación de hashes seguros para contraseñas
- Verificación de contraseñas contra hashes almacenados
- Configuración de factores de trabajo para el algoritmo de hash

**Implementación:**
```csharp
public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string passwordHash, string password);
}
```

### UserRepository

El repositorio `UserRepository` proporciona métodos para interactuar con la base de datos de usuarios, incluyendo la validación de credenciales.

**Métodos relevantes para autenticación:**
- `ValidateCredentialsAsync`: Valida el nombre de usuario y contraseña
- `GetByUsernameAsync`: Obtiene un usuario por su nombre de usuario
- `GetByEmailAsync`: Obtiene un usuario por su correo electrónico
- `AddSessionAsync`: Registra una nueva sesión de usuario
- `RevokeSessionAsync`: Revoca una sesión específica
- `RevokeAllUserSessionsAsync`: Revoca todas las sesiones de un usuario

## Flujo de Autenticación

### Inicio de Sesión

1. El usuario envía sus credenciales (nombre de usuario/correo y contraseña) al endpoint `/api/auth/login`.
2. El controlador `AuthController` convierte la solicitud en un comando `AuthenticateCommand`.
3. El handler `AuthenticateCommandHandler` valida las credenciales utilizando `UserRepository` y `PasswordHasher`.
4. Si las credenciales son válidas, se verifica el estado del usuario (activo, bloqueado, etc.).
5. Si el usuario tiene habilitada la autenticación de dos factores, se devuelve una respuesta indicando que se requiere 2FA.
6. Si no se requiere 2FA, se generan tokens JWT utilizando `JwtService`.
7. Se registra la nueva sesión en la base de datos.
8. Se devuelve la respuesta con los tokens y la información del usuario.

### Renovación de Token (Refresh Token)

1. El cliente envía el token JWT expirado y el refresh token al endpoint `/api/auth/refresh-token`.
2. El controlador `AuthController` convierte la solicitud en un comando `RefreshTokenCommand`.
3. El handler `RefreshTokenCommandHandler` valida el token JWT y el refresh token.
4. Si son válidos, se revoca el token anterior y se generan nuevos tokens.
5. Se registra la nueva sesión en la base de datos.
6. Se devuelve la respuesta con los nuevos tokens.

### Cierre de Sesión

1. El cliente envía el token JWT al endpoint `/api/auth/logout`.
2. El controlador `AuthController` convierte la solicitud en un comando `LogoutCommand`.
3. El handler `LogoutCommandHandler` revoca la sesión asociada al token.
4. Se devuelve una respuesta indicando el éxito de la operación.

## Endpoints de API

### POST /api/auth/login

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

### POST /api/auth/refresh-token

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

### POST /api/auth/logout

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

## Seguridad

### Almacenamiento de Contraseñas

Las contraseñas se almacenan utilizando BCrypt, un algoritmo de hash diseñado específicamente para contraseñas. BCrypt incorpora:
- Sal (salt) única para cada contraseña
- Factor de trabajo configurable para ajustar la resistencia a ataques de fuerza bruta
- Función de derivación de clave que dificulta los ataques con hardware especializado

### Protección contra Ataques de Fuerza Bruta

El sistema implementa un servicio de limitación de tasa (`RateLimitService`) que restringe el número de intentos de inicio de sesión fallidos en un período determinado. Después de un número configurable de intentos fallidos, la cuenta puede ser bloqueada temporalmente.

### Tokens JWT

Los tokens JWT incluyen:
- Identificador único (JTI)
- Fecha de emisión (IAT)
- Fecha de expiración (EXP)
- Emisor (ISS)
- Audiencia (AUD)
- Claims personalizados para el usuario (ID, roles, permisos)

La configuración de JWT se realiza en `appsettings.json`:

```json
{
  "JwtSettings": {
    "Secret": "your-secret-key-at-least-16-characters",
    "Issuer": "authsystem",
    "Audience": "authsystem-clients",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

### Auditoría

Todas las operaciones de autenticación (intentos de inicio de sesión, cierres de sesión, renovaciones de token) se registran utilizando el servicio de auditoría (`AuditService`). Esto permite rastrear actividades sospechosas y proporcionar un registro completo de las acciones de autenticación.

## Configuración

### Opciones de Contraseña

La configuración de las políticas de contraseñas se realiza en `appsettings.json`:

```json
{
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
}
```

### Opciones de JWT

```json
{
  "JwtSettings": {
    "Secret": "your-secret-key-at-least-16-characters",
    "Issuer": "authsystem",
    "Audience": "authsystem-clients",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

## Mejores Prácticas

1. **Rotación de Secretos**: Cambiar periódicamente la clave secreta utilizada para firmar los tokens JWT.
2. **Monitoreo de Sesiones**: Implementar un sistema para que los usuarios puedan ver y gestionar sus sesiones activas.
3. **Notificaciones de Inicio de Sesión**: Enviar notificaciones por correo electrónico cuando se detecta un inicio de sesión desde una ubicación o dispositivo desconocido.
4. **Políticas de Contraseñas**: Implementar políticas de contraseñas fuertes y forzar cambios periódicos.
5. **Caducidad de Tokens**: Configurar tiempos de expiración adecuados para los tokens JWT y refresh tokens.

## Solución de Problemas

### Problemas comunes de autenticación

1. **Token JWT inválido o expirado**
   - Verificar que el token no haya expirado
   - Comprobar que la firma del token sea válida
   - Asegurarse de que el token no haya sido revocado

2. **Credenciales inválidas**
   - Verificar que el nombre de usuario y contraseña sean correctos
   - Comprobar que la cuenta no esté bloqueada o suspendida
   - Verificar que el usuario exista en la base de datos

3. **Problemas con refresh tokens**
   - Asegurarse de que el refresh token no haya expirado
   - Verificar que el refresh token no haya sido revocado
   - Comprobar que el refresh token corresponda al token JWT

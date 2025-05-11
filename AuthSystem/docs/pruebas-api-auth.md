# Guía de Pruebas para la API de Autenticación

Este documento proporciona ejemplos detallados para probar los endpoints de autenticación del sistema AuthSystem.

## Índice

1. [Configuración Inicial](#configuración-inicial)
2. [Login](#login)
3. [Refresh Token](#refresh-token)
4. [Logout](#logout)
5. [Cambio de Contraseña](#cambio-de-contraseña)
6. [Recuperación de Contraseña](#recuperación-de-contraseña)
7. [Ejemplos con Postman](#ejemplos-con-postman)
8. [Ejemplos con cURL](#ejemplos-con-curl)

## Configuración Inicial

### URL Base
```
http://localhost:5000/api
```

### Headers Comunes
- Para peticiones sin autenticación:
  ```
  Content-Type: application/json
  ```

- Para peticiones autenticadas:
  ```
  Content-Type: application/json
  Authorization: Bearer {tu-token-jwt}
  ```

## Login

El endpoint de login permite a los usuarios autenticarse y obtener un token JWT para acceder a recursos protegidos.

### Endpoint
```
POST /api/Auth/login
```

### Cuerpo de la Petición
```json
{
  "username": "admin",
  "password": "Admin123!",
  "ipAddress": "127.0.0.1",
  "userAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
  "rememberMe": true,
  "recaptchaToken": "6LefBxwrAAAAAHhSf3zM7NfH7_Ay7TSSmsylgOo8"
}
```

### Respuesta Exitosa (200 OK)
```json
{
  "success": true,
  "message": "Login exitoso",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "a1b2c3d4-e5f6-7g8h-9i0j-k1l2m3n4o5p6",
    "expiresIn": 3600,
    "user": {
      "id": "12345678-1234-1234-1234-123456789012",
      "username": "admin",
      "email": "admin@authsystem.com",
      "firstName": "Administrador",
      "lastName": "Sistema",
      "roles": [
        "Super Administrador"
      ]
    }
  }
}
```

### Respuesta de Error (401 Unauthorized)
```json
{
  "success": false,
  "message": "Credenciales inválidas",
  "errors": [
    "El nombre de usuario o la contraseña son incorrectos"
  ]
}
```

### Ejemplo con cURL
```bash
curl -X POST http://localhost:5000/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "Admin123!",
    "ipAddress": "127.0.0.1",
    "userAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
    "rememberMe": true,
    "recaptchaToken": "6LefBxwrAAAAAHhSf3zM7NfH7_Ay7TSSmsylgOo8"
  }'
```

## Refresh Token

El endpoint de refresh token permite renovar un token JWT expirado sin necesidad de volver a introducir credenciales.

### Endpoint
```
POST /api/Auth/refresh-token
```

### Cuerpo de la Petición
```json
{
  "refreshToken": "a1b2c3d4-e5f6-7g8h-9i0j-k1l2m3n4o5p6"
}
```

### Respuesta Exitosa (200 OK)
```json
{
  "success": true,
  "message": "Token renovado exitosamente",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "q7w8e9r0-t1y2-u3i4-o5p6-a7s8d9f0g1h2",
    "expiresIn": 3600
  }
}
```

### Respuesta de Error (401 Unauthorized)
```json
{
  "success": false,
  "message": "Refresh token inválido o expirado",
  "errors": [
    "El refresh token proporcionado no es válido o ha expirado"
  ]
}
```

### Ejemplo con cURL
```bash
curl -X POST http://localhost:5000/api/Auth/refresh-token \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "a1b2c3d4-e5f6-7g8h-9i0j-k1l2m3n4o5p6"
  }'
```

## Logout

El endpoint de logout permite invalidar un token de refresco para cerrar sesión de forma segura.

### Endpoint
```
POST /api/Auth/logout
```

### Cuerpo de la Petición
```json
{
  "refreshToken": "a1b2c3d4-e5f6-7g8h-9i0j-k1l2m3n4o5p6"
}
```

### Respuesta Exitosa (200 OK)
```json
{
  "success": true,
  "message": "Sesión cerrada exitosamente"
}
```

### Ejemplo con cURL
```bash
curl -X POST http://localhost:5000/api/Auth/logout \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -d '{
    "refreshToken": "a1b2c3d4-e5f6-7g8h-9i0j-k1l2m3n4o5p6"
  }'
```

## Ejemplos con Postman

### Configuración de una Colección para Pruebas de Autenticación

1. **Crear una nueva colección** llamada "AuthSystem API"
2. **Configurar variables de entorno**:
   - `baseUrl`: http://localhost:5000/api
   - `token`: (inicialmente vacío)
   - `refreshToken`: (inicialmente vacío)

3. **Crear una carpeta** llamada "Auth" dentro de la colección

4. **Crear una petición POST para login**:
   - URL: `{{baseUrl}}/Auth/login`
   - Body (raw JSON):
     ```json
     {
       "username": "admin",
       "password": "Admin123!",
       "rememberMe": true
     }
     ```
   - Tests (para guardar automáticamente el token):
     ```javascript
     var jsonData = JSON.parse(responseBody);
     if (jsonData.success && jsonData.data && jsonData.data.token) {
         pm.environment.set("token", jsonData.data.token);
         pm.environment.set("refreshToken", jsonData.data.refreshToken);
     }
     ```

5. **Crear una petición POST para refresh-token**:
   - URL: `{{baseUrl}}/Auth/refresh-token`
   - Body (raw JSON):
     ```json
     {
       "refreshToken": "{{refreshToken}}"
     }
     ```
   - Tests (para actualizar el token):
     ```javascript
     var jsonData = JSON.parse(responseBody);
     if (jsonData.success && jsonData.data && jsonData.data.token) {
         pm.environment.set("token", jsonData.data.token);
         pm.environment.set("refreshToken", jsonData.data.refreshToken);
     }
     ```

6. **Configurar autorización para todas las peticiones** en la colección:
   - Tipo: Bearer Token
   - Token: `{{token}}`

### Flujo de Prueba Recomendado

1. Ejecutar la petición de login para obtener el token inicial
2. Probar endpoints protegidos usando el token obtenido
3. Si el token expira, usar la petición de refresh-token para obtener uno nuevo
4. Al finalizar, ejecutar logout para invalidar el token

## Consejos para Depuración

1. **Verificar el token JWT**: Puedes decodificar el token JWT en [jwt.io](https://jwt.io/) para verificar su contenido y validez.

2. **Revisar los logs del servidor**: Si algo no funciona como se espera, revisa los logs del servidor para obtener más información sobre el error.

3. **Comprobar la expiración del token**: Si recibes un error 401, es posible que el token haya expirado. Usa el endpoint de refresh-token para obtener uno nuevo.

4. **Validar los claims del token**: Asegúrate de que el token contiene los claims necesarios para acceder al recurso solicitado.

## Notas de Seguridad

1. **Almacenamiento seguro de tokens**: En aplicaciones de producción, almacena los tokens de forma segura (por ejemplo, en HttpOnly cookies para aplicaciones web).

2. **Validación de tokens**: Siempre valida los tokens en el servidor antes de permitir el acceso a recursos protegidos.

3. **Tiempo de expiración**: Configura un tiempo de expiración razonable para los tokens JWT (por defecto 60 minutos en este sistema).

4. **Revocación de tokens**: Utiliza el endpoint de logout para revocar tokens cuando un usuario cierra sesión.

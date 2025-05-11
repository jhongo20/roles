# Ejemplos para Probar los Endpoints de Autenticación

Este documento proporciona ejemplos prácticos para probar los endpoints de autenticación del sistema AuthSystem, con enfoque en los endpoints de login y refresh token.

## Endpoint: Login (`/api/Auth/login`)

Este endpoint permite a los usuarios autenticarse y obtener un token JWT para acceder a recursos protegidos.

### Ejemplo con Postman

1. **Configuración de la petición**:
   - Método: **POST**
   - URL: `http://localhost:5000/api/Auth/login`
   - Headers:
     ```
     Content-Type: application/json
     ```
   - Body (raw JSON):
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

2. **Respuesta esperada** (200 OK):
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

### Ejemplo con JavaScript (Fetch API)

```javascript
const loginData = {
  username: 'admin',
  password: 'Admin123!',
  ipAddress: '127.0.0.1',
  userAgent: navigator.userAgent,
  rememberMe: true,
  recaptchaToken: '6LefBxwrAAAAAHhSf3zM7NfH7_Ay7TSSmsylgOo8'
};

fetch('http://localhost:5000/api/Auth/login', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify(loginData)
})
.then(response => response.json())
.then(data => {
  if (data.success) {
    // Guardar el token y refresh token
    localStorage.setItem('token', data.data.token);
    localStorage.setItem('refreshToken', data.data.refreshToken);
    console.log('Login exitoso:', data);
  } else {
    console.error('Error de login:', data.message);
  }
})
.catch(error => console.error('Error:', error));
```

## Endpoint: Refresh Token (`/api/Auth/refresh-token`)

Este endpoint permite renovar un token JWT expirado sin necesidad de volver a introducir credenciales.

### Ejemplo con Postman

1. **Configuración de la petición**:
   - Método: **POST**
   - URL: `http://localhost:5000/api/Auth/refresh-token`
   - Headers:
     ```
     Content-Type: application/json
     ```
   - Body (raw JSON):
     ```json
     {
       "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
       "refreshToken": "a1b2c3d4-e5f6-7g8h-9i0j-k1l2m3n4o5p6",
       "ipAddress": "127.0.0.1",
       "userAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"
     }
     ```

2. **Respuesta esperada** (200 OK):
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

### Ejemplo con cURL

```bash
curl -X POST http://localhost:5000/api/Auth/refresh-token \
  -H "Content-Type: application/json" \
  -d '{
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "a1b2c3d4-e5f6-7g8h-9i0j-k1l2m3n4o5p6",
    "ipAddress": "127.0.0.1",
    "userAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"
  }'
```

### Ejemplo con JavaScript (Fetch API)

```javascript
const refreshTokenData = {
  token: localStorage.getItem('token'),
  refreshToken: localStorage.getItem('refreshToken'),
  ipAddress: '127.0.0.1',
  userAgent: navigator.userAgent
};

fetch('http://localhost:5000/api/Auth/refresh-token', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify(refreshTokenData)
})
.then(response => response.json())
.then(data => {
  if (data.success) {
    // Actualizar el token y refresh token
    localStorage.setItem('token', data.data.token);
    localStorage.setItem('refreshToken', data.data.refreshToken);
    console.log('Token renovado exitosamente:', data);
  } else {
    console.error('Error al renovar token:', data.message);
  }
})
.catch(error => console.error('Error:', error));
```

## Flujo Completo de Autenticación

### 1. Login para obtener token inicial

Realizar una petición POST a `/api/Auth/login` con las credenciales del usuario.

### 2. Usar el token para acceder a recursos protegidos

Incluir el token en el header de autorización:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### 3. Renovar el token cuando expire

Cuando recibas un error 401 Unauthorized, usa el endpoint `/api/Auth/refresh-token` para obtener un nuevo token.

### 4. Cerrar sesión

Para cerrar sesión de forma segura, invalida el refresh token con una petición a `/api/Auth/logout`.

## Consejos para Pruebas

1. **Verificar el token JWT**: Puedes decodificar el token JWT en [jwt.io](https://jwt.io/) para verificar su contenido.

2. **Automatizar el proceso de renovación**: En aplicaciones frontend, implementa la renovación automática del token cuando expire.

3. **Manejar errores de autenticación**: Implementa un manejo adecuado de errores 401 y 403 en tu aplicación.

4. **Pruebas con Swagger**: También puedes usar la documentación Swagger en `http://localhost:5000/api-docs` para probar los endpoints de autenticación.

## Colección de Postman

Para facilitar las pruebas, puedes importar la siguiente colección de Postman:

```json
{
  "info": {
    "name": "AuthSystem API",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Auth",
      "item": [
        {
          "name": "Login",
          "request": {
            "method": "POST",
            "header": [
              {
                "key": "Content-Type",
                "value": "application/json"
              }
            ],
            "body": {
              "mode": "raw",
              "raw": "{\n  \"username\": \"admin\",\n  \"password\": \"Admin123!\",\n  \"rememberMe\": true\n}"
            },
            "url": {
              "raw": "{{baseUrl}}/Auth/login",
              "host": ["{{baseUrl}}"],
              "path": ["Auth", "login"]
            }
          },
          "response": []
        },
        {
          "name": "Refresh Token",
          "request": {
            "method": "POST",
            "header": [
              {
                "key": "Content-Type",
                "value": "application/json"
              }
            ],
            "body": {
              "mode": "raw",
              "raw": "{\n  \"refreshToken\": \"{{refreshToken}}\"\n}"
            },
            "url": {
              "raw": "{{baseUrl}}/Auth/refresh-token",
              "host": ["{{baseUrl}}"],
              "path": ["Auth", "refresh-token"]
            }
          },
          "response": []
        }
      ]
    }
  ],
  "variable": [
    {
      "key": "baseUrl",
      "value": "http://localhost:5000/api"
    },
    {
      "key": "token",
      "value": ""
    },
    {
      "key": "refreshToken",
      "value": ""
    }
  ]
}
```

Guarda este JSON en un archivo con extensión `.json` e impórtalo en Postman para comenzar a probar los endpoints de autenticación rápidamente.

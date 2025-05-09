# API Reference

## Introducción

Este documento proporciona una referencia completa de todos los endpoints de la API del sistema AuthSystem. La API sigue los principios RESTful y utiliza JSON para el intercambio de datos. Todos los endpoints están protegidos mediante autenticación JWT, excepto aquellos específicamente marcados como públicos.

## Base URL

La URL base para todas las solicitudes de API es:

```
https://[dominio]/api
```

## Autenticación

La mayoría de los endpoints requieren autenticación mediante un token JWT. El token debe incluirse en el encabezado `Authorization` de la solicitud HTTP:

```
Authorization: Bearer [token]
```

## Formato de Respuesta

Todas las respuestas de la API siguen un formato estándar:

```json
{
  "succeeded": true|false,
  "message": "Mensaje descriptivo",
  "data": { ... },
  "errors": [ ... ]
}
```

- `succeeded`: Indica si la operación fue exitosa.
- `message`: Mensaje descriptivo sobre el resultado de la operación.
- `data`: Contiene los datos de respuesta cuando la operación es exitosa.
- `errors`: Lista de errores cuando la operación falla.

## Códigos de Estado HTTP

La API utiliza los siguientes códigos de estado HTTP:

- `200 OK`: La solicitud se completó con éxito.
- `201 Created`: El recurso se creó con éxito.
- `204 No Content`: La solicitud se completó con éxito, pero no hay contenido para devolver.
- `400 Bad Request`: La solicitud contiene datos inválidos o falta información requerida.
- `401 Unauthorized`: La solicitud requiere autenticación.
- `403 Forbidden`: El usuario autenticado no tiene permiso para acceder al recurso.
- `404 Not Found`: El recurso solicitado no existe.
- `409 Conflict`: La solicitud no pudo completarse debido a un conflicto con el estado actual del recurso.
- `422 Unprocessable Entity`: La solicitud está bien formada pero contiene errores semánticos.
- `429 Too Many Requests`: El cliente ha enviado demasiadas solicitudes en un período de tiempo determinado.
- `500 Internal Server Error`: Error interno del servidor.

## Endpoints

### Autenticación

#### POST /api/auth/register

Registra un nuevo usuario en el sistema.

**Acceso**: Público

**Request:**
```json
{
  "username": "string",
  "email": "string",
  "password": "string",
  "confirmPassword": "string",
  "firstName": "string",
  "lastName": "string"
}
```

**Response (200 OK):**
```json
{
  "succeeded": true,
  "userId": "guid",
  "requiresEmailConfirmation": true,
  "message": "Usuario registrado exitosamente. Se ha enviado un correo de confirmación."
}
```

**Posibles errores:**
- `400 Bad Request`: Datos de registro inválidos.
- `409 Conflict`: El nombre de usuario o correo electrónico ya existe.

#### POST /api/auth/login

Inicia sesión con nombre de usuario/correo y contraseña.

**Acceso**: Público

**Request:**
```json
{
  "username": "string",
  "password": "string",
  "rememberMe": boolean,
  "recaptchaToken": "string"
}
```

**Response (200 OK) - Sin 2FA:**
```json
{
  "succeeded": true,
  "token": "string",
  "refreshToken": "string",
  "requirePasswordChange": false,
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
  "message": "Inicio de sesión exitoso"
}
```

**Response (200 OK) - Con 2FA:**
```json
{
  "succeeded": true,
  "requiresTwoFactor": true,
  "userId": "guid",
  "message": "Se requiere verificación de dos factores"
}
```

**Posibles errores:**
- `400 Bad Request`: Credenciales inválidas.
- `401 Unauthorized`: Usuario no autorizado.
- `403 Forbidden`: Cuenta bloqueada o desactivada.
- `429 Too Many Requests`: Demasiados intentos fallidos de inicio de sesión.

#### POST /api/auth/refresh-token

Renueva un token JWT expirado.

**Acceso**: Público

**Request:**
```json
{
  "token": "string",
  "refreshToken": "string"
}
```

**Response (200 OK):**
```json
{
  "succeeded": true,
  "token": "string",
  "refreshToken": "string",
  "requirePasswordChange": false,
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
  "message": "Token renovado exitosamente"
}
```

**Posibles errores:**
- `400 Bad Request`: Token o refresh token inválido.
- `401 Unauthorized`: Refresh token expirado o revocado.

#### POST /api/auth/logout

Cierra la sesión del usuario.

**Acceso**: Autenticado

**Request:**
```json
{
  "token": "string"
}
```

**Response (200 OK):**
```json
{
  "succeeded": true,
  "message": "Sesión cerrada exitosamente"
}
```

**Posibles errores:**
- `401 Unauthorized`: Usuario no autenticado.

### Confirmación de Email

#### POST /api/email-confirmation/generate

Genera un token de confirmación y envía un correo electrónico.

**Acceso**: Público

**Request:**
```json
{
  "email": "string",
  "callbackUrl": "string"
}
```

**Response (200 OK):**
```json
{
  "succeeded": true,
  "message": "Correo de confirmación enviado exitosamente"
}
```

**Posibles errores:**
- `400 Bad Request`: Email inválido o no registrado.
- `409 Conflict`: Email ya confirmado.
- `429 Too Many Requests`: Demasiadas solicitudes de confirmación.

#### POST /api/email-confirmation/verify

Verifica un token de confirmación.

**Acceso**: Público

**Request:**
```json
{
  "userId": "guid",
  "token": "string"
}
```

**Response (200 OK):**
```json
{
  "succeeded": true,
  "message": "Correo electrónico confirmado exitosamente"
}
```

**Posibles errores:**
- `400 Bad Request`: Token inválido.
- `404 Not Found`: Usuario no encontrado.
- `409 Conflict`: Email ya confirmado.

#### POST /api/email-confirmation/resend

Reenvía un correo de confirmación.

**Acceso**: Público

**Request:**
```json
{
  "email": "string",
  "callbackUrl": "string"
}
```

**Response (200 OK):**
```json
{
  "succeeded": true,
  "message": "Correo de confirmación reenviado exitosamente"
}
```

**Posibles errores:**
- `400 Bad Request`: Email inválido o no registrado.
- `409 Conflict`: Email ya confirmado.
- `429 Too Many Requests`: Demasiadas solicitudes de reenvío.

#### GET /api/email-confirmation/status/{userId}

Consulta el estado de confirmación de un correo electrónico.

**Acceso**: Autenticado (Usuario o Administrador)

**Response (200 OK):**
```json
{
  "isConfirmed": true,
  "email": "string",
  "confirmationDate": "datetime"
}
```

**Posibles errores:**
- `401 Unauthorized`: Usuario no autenticado.
- `403 Forbidden`: Usuario no autorizado.
- `404 Not Found`: Usuario no encontrado.

### Autenticación de Dos Factores

#### POST /api/two-factor/enable

Habilita la autenticación de dos factores para un usuario.

**Acceso**: Autenticado

**Request:**
```json
{
  "userId": "guid",
  "phoneNumber": "string"
}
```

**Response (200 OK):**
```json
{
  "succeeded": true,
  "message": "Autenticación de dos factores habilitada exitosamente"
}
```

**Posibles errores:**
- `400 Bad Request`: Datos inválidos.
- `401 Unauthorized`: Usuario no autenticado.
- `403 Forbidden`: Usuario no autorizado.
- `404 Not Found`: Usuario no encontrado.

#### POST /api/two-factor/disable

Deshabilita la autenticación de dos factores para un usuario.

**Acceso**: Autenticado

**Request:**
```json
{
  "userId": "guid"
}
```

**Response (200 OK):**
```json
{
  "succeeded": true,
  "message": "Autenticación de dos factores deshabilitada exitosamente"
}
```

**Posibles errores:**
- `401 Unauthorized`: Usuario no autenticado.
- `403 Forbidden`: Usuario no autorizado.
- `404 Not Found`: Usuario no encontrado.

#### POST /api/two-factor/send-code

Envía un código de verificación para autenticación de dos factores.

**Acceso**: Autenticado

**Request:**
```json
{
  "userId": "guid",
  "ipAddress": "string",
  "userAgent": "string"
}
```

**Response (200 OK):**
```json
{
  "succeeded": true,
  "message": "Código de verificación enviado exitosamente"
}
```

**Posibles errores:**
- `400 Bad Request`: Datos inválidos.
- `401 Unauthorized`: Usuario no autenticado.
- `403 Forbidden`: Usuario no autorizado.
- `404 Not Found`: Usuario no encontrado.
- `429 Too Many Requests`: Demasiadas solicitudes de código.

#### POST /api/two-factor/verify-code

Verifica un código de autenticación de dos factores.

**Acceso**: Autenticado

**Request:**
```json
{
  "userId": "guid",
  "code": "string",
  "rememberDevice": boolean
}
```

**Response (200 OK):**
```json
{
  "succeeded": true,
  "token": "string",
  "refreshToken": "string",
  "user": {
    "id": "guid",
    "username": "string",
    "email": "string",
    "firstName": "string",
    "lastName": "string",
    "status": "string",
    "lastLoginDate": "datetime",
    "twoFactorEnabled": true,
    "createdAt": "datetime"
  },
  "message": "Código verificado exitosamente"
}
```

**Posibles errores:**
- `400 Bad Request`: Código inválido.
- `401 Unauthorized`: Usuario no autenticado.
- `403 Forbidden`: Usuario no autorizado.
- `404 Not Found`: Usuario no encontrado.
- `429 Too Many Requests`: Demasiados intentos fallidos.

### Gestión de Usuarios

#### GET /api/users

Obtiene una lista paginada de usuarios.

**Acceso**: Autenticado (Administrador)

**Query Parameters:**
- `page`: Número de página (por defecto: 1)
- `pageSize`: Tamaño de página (por defecto: 10)
- `search`: Término de búsqueda
- `status`: Filtro por estado
- `sortBy`: Campo para ordenar
- `sortDirection`: Dirección de ordenamiento (asc, desc)

**Response (200 OK):**
```json
{
  "users": [
    {
      "id": "guid",
      "username": "string",
      "email": "string",
      "firstName": "string",
      "lastName": "string",
      "status": "string",
      "emailConfirmed": boolean,
      "phoneNumber": "string",
      "phoneNumberConfirmed": boolean,
      "twoFactorEnabled": boolean,
      "createdAt": "datetime",
      "lastLoginAt": "datetime"
    }
  ],
  "totalCount": 0,
  "pageCount": 0,
  "currentPage": 0,
  "pageSize": 0
}
```

**Posibles errores:**
- `401 Unauthorized`: Usuario no autenticado.
- `403 Forbidden`: Usuario no autorizado.

#### GET /api/users/{id}

Obtiene un usuario por su ID.

**Acceso**: Autenticado (Usuario o Administrador)

**Response (200 OK):**
```json
{
  "id": "guid",
  "username": "string",
  "email": "string",
  "firstName": "string",
  "lastName": "string",
  "status": "string",
  "emailConfirmed": boolean,
  "phoneNumber": "string",
  "phoneNumberConfirmed": boolean,
  "twoFactorEnabled": boolean,
  "createdAt": "datetime",
  "lastLoginAt": "datetime"
}
```

**Posibles errores:**
- `401 Unauthorized`: Usuario no autenticado.
- `403 Forbidden`: Usuario no autorizado.
- `404 Not Found`: Usuario no encontrado.

#### PUT /api/users/profile

Actualiza el perfil de un usuario.

**Acceso**: Autenticado

**Request:**
```json
{
  "userId": "guid",
  "firstName": "string",
  "lastName": "string",
  "phoneNumber": "string"
}
```

**Response (200 OK):**
```json
{
  "succeeded": true,
  "user": {
    "id": "guid",
    "username": "string",
    "email": "string",
    "firstName": "string",
    "lastName": "string",
    "phoneNumber": "string"
  },
  "message": "Perfil actualizado exitosamente"
}
```

**Posibles errores:**
- `400 Bad Request`: Datos inválidos.
- `401 Unauthorized`: Usuario no autenticado.
- `403 Forbidden`: Usuario no autorizado.
- `404 Not Found`: Usuario no encontrado.

#### PUT /api/users/phone-number

Actualiza el número de teléfono de un usuario.

**Acceso**: Autenticado

**Request:**
```json
{
  "userId": "guid",
  "phoneNumber": "string"
}
```

**Response (200 OK):**
```json
{
  "succeeded": true,
  "message": "Número de teléfono actualizado exitosamente"
}
```

**Posibles errores:**
- `400 Bad Request`: Número de teléfono inválido.
- `401 Unauthorized`: Usuario no autenticado.
- `403 Forbidden`: Usuario no autorizado.
- `404 Not Found`: Usuario no encontrado.

#### PUT /api/users/password

Cambia la contraseña de un usuario.

**Acceso**: Autenticado

**Request:**
```json
{
  "userId": "guid",
  "currentPassword": "string",
  "newPassword": "string",
  "confirmPassword": "string"
}
```

**Response (200 OK):**
```json
{
  "succeeded": true,
  "message": "Contraseña cambiada exitosamente"
}
```

**Posibles errores:**
- `400 Bad Request`: Contraseñas inválidas o no coinciden.
- `401 Unauthorized`: Usuario no autenticado.
- `403 Forbidden`: Usuario no autorizado.
- `404 Not Found`: Usuario no encontrado.

#### POST /api/users/{id}/lock

Bloquea temporalmente una cuenta de usuario.

**Acceso**: Autenticado (Administrador)

**Request:**
```json
{
  "lockoutMinutes": 0
}
```

**Response (200 OK):**
```json
{
  "succeeded": true,
  "message": "Usuario bloqueado exitosamente"
}
```

**Posibles errores:**
- `401 Unauthorized`: Usuario no autenticado.
- `403 Forbidden`: Usuario no autorizado.
- `404 Not Found`: Usuario no encontrado.

#### POST /api/users/{id}/unlock

Desbloquea una cuenta de usuario.

**Acceso**: Autenticado (Administrador)

**Response (200 OK):**
```json
{
  "succeeded": true,
  "message": "Usuario desbloqueado exitosamente"
}
```

**Posibles errores:**
- `401 Unauthorized`: Usuario no autenticado.
- `403 Forbidden`: Usuario no autorizado.
- `404 Not Found`: Usuario no encontrado.

### Gestión de Roles

#### GET /api/roles

Obtiene todos los roles.

**Acceso**: Autenticado (Administrador)

**Response (200 OK):**
```json
{
  "roles": [
    {
      "id": "guid",
      "name": "string",
      "description": "string",
      "isSystem": boolean,
      "createdAt": "datetime"
    }
  ]
}
```

**Posibles errores:**
- `401 Unauthorized`: Usuario no autenticado.
- `403 Forbidden`: Usuario no autorizado.

#### GET /api/roles/{id}

Obtiene un rol por su ID.

**Acceso**: Autenticado (Administrador)

**Response (200 OK):**
```json
{
  "id": "guid",
  "name": "string",
  "description": "string",
  "isSystem": boolean,
  "createdAt": "datetime",
  "permissions": [
    {
      "id": "guid",
      "name": "string",
      "description": "string",
      "category": "string"
    }
  ]
}
```

**Posibles errores:**
- `401 Unauthorized`: Usuario no autenticado.
- `403 Forbidden`: Usuario no autorizado.
- `404 Not Found`: Rol no encontrado.

#### POST /api/roles

Crea un nuevo rol.

**Acceso**: Autenticado (Administrador)

**Request:**
```json
{
  "name": "string",
  "description": "string"
}
```

**Response (201 Created):**
```json
{
  "succeeded": true,
  "roleId": "guid",
  "message": "Rol creado exitosamente"
}
```

**Posibles errores:**
- `400 Bad Request`: Datos inválidos.
- `401 Unauthorized`: Usuario no autenticado.
- `403 Forbidden`: Usuario no autorizado.
- `409 Conflict`: Nombre de rol ya existe.

#### PUT /api/roles/{id}

Actualiza un rol existente.

**Acceso**: Autenticado (Administrador)

**Request:**
```json
{
  "name": "string",
  "description": "string"
}
```

**Response (200 OK):**
```json
{
  "succeeded": true,
  "message": "Rol actualizado exitosamente"
}
```

**Posibles errores:**
- `400 Bad Request`: Datos inválidos.
- `401 Unauthorized`: Usuario no autenticado.
- `403 Forbidden`: Usuario no autorizado.
- `404 Not Found`: Rol no encontrado.
- `409 Conflict`: Nombre de rol ya existe.

#### DELETE /api/roles/{id}

Elimina un rol.

**Acceso**: Autenticado (Administrador)

**Response (200 OK):**
```json
{
  "succeeded": true,
  "message": "Rol eliminado exitosamente"
}
```

**Posibles errores:**
- `401 Unauthorized`: Usuario no autenticado.
- `403 Forbidden`: Usuario no autorizado.
- `404 Not Found`: Rol no encontrado.
- `409 Conflict`: No se puede eliminar un rol del sistema.

#### GET /api/users/{userId}/roles

Obtiene los roles asignados a un usuario.

**Acceso**: Autenticado (Usuario o Administrador)

**Response (200 OK):**
```json
{
  "roles": [
    {
      "id": "guid",
      "name": "string",
      "description": "string"
    }
  ]
}
```

**Posibles errores:**
- `401 Unauthorized`: Usuario no autenticado.
- `403 Forbidden`: Usuario no autorizado.
- `404 Not Found`: Usuario no encontrado.

#### POST /api/users/{userId}/roles

Asigna un rol a un usuario.

**Acceso**: Autenticado (Administrador)

**Request:**
```json
{
  "roleId": "guid"
}
```

**Response (200 OK):**
```json
{
  "succeeded": true,
  "message": "Rol asignado exitosamente"
}
```

**Posibles errores:**
- `400 Bad Request`: Datos inválidos.
- `401 Unauthorized`: Usuario no autenticado.
- `403 Forbidden`: Usuario no autorizado.
- `404 Not Found`: Usuario o rol no encontrado.
- `409 Conflict`: El usuario ya tiene asignado el rol.

#### DELETE /api/users/{userId}/roles/{roleId}

Elimina un rol de un usuario.

**Acceso**: Autenticado (Administrador)

**Response (200 OK):**
```json
{
  "succeeded": true,
  "message": "Rol eliminado exitosamente"
}
```

**Posibles errores:**
- `401 Unauthorized`: Usuario no autenticado.
- `403 Forbidden`: Usuario no autorizado.
- `404 Not Found`: Usuario o rol no encontrado.
- `409 Conflict`: No se puede eliminar un rol requerido.

### Permisos

#### GET /api/permissions

Obtiene todos los permisos.

**Acceso**: Autenticado (Administrador)

**Response (200 OK):**
```json
{
  "permissions": [
    {
      "id": "guid",
      "name": "string",
      "description": "string",
      "category": "string",
      "isSystem": boolean
    }
  ]
}
```

**Posibles errores:**
- `401 Unauthorized`: Usuario no autenticado.
- `403 Forbidden`: Usuario no autorizado.

#### GET /api/roles/{roleId}/permissions

Obtiene los permisos asignados a un rol.

**Acceso**: Autenticado (Administrador)

**Response (200 OK):**
```json
{
  "permissions": [
    {
      "id": "guid",
      "name": "string",
      "description": "string",
      "category": "string"
    }
  ]
}
```

**Posibles errores:**
- `401 Unauthorized`: Usuario no autenticado.
- `403 Forbidden`: Usuario no autorizado.
- `404 Not Found`: Rol no encontrado.

#### POST /api/roles/{roleId}/permissions

Asigna un permiso a un rol.

**Acceso**: Autenticado (Administrador)

**Request:**
```json
{
  "permissionId": "guid"
}
```

**Response (200 OK):**
```json
{
  "succeeded": true,
  "message": "Permiso asignado exitosamente"
}
```

**Posibles errores:**
- `400 Bad Request`: Datos inválidos.
- `401 Unauthorized`: Usuario no autenticado.
- `403 Forbidden`: Usuario no autorizado.
- `404 Not Found`: Rol o permiso no encontrado.
- `409 Conflict`: El rol ya tiene asignado el permiso.

#### DELETE /api/roles/{roleId}/permissions/{permissionId}

Elimina un permiso de un rol.

**Acceso**: Autenticado (Administrador)

**Response (200 OK):**
```json
{
  "succeeded": true,
  "message": "Permiso eliminado exitosamente"
}
```

**Posibles errores:**
- `401 Unauthorized`: Usuario no autenticado.
- `403 Forbidden`: Usuario no autorizado.
- `404 Not Found`: Rol o permiso no encontrado.
- `409 Conflict`: No se puede eliminar un permiso requerido.

## Limitación de Tasa

La API implementa limitación de tasa para prevenir abusos. Los límites varían según el endpoint:

- Endpoints de autenticación: 5 solicitudes por 15 minutos por IP/usuario.
- Endpoints de restablecimiento de contraseña: 3 solicitudes por 60 minutos por IP/usuario.
- Endpoints de confirmación de correo: 3 solicitudes por 60 minutos por IP/usuario.
- Endpoints generales de API: 100 solicitudes por minuto por IP/usuario.

Cuando se alcanza el límite, la API devuelve un código de estado `429 Too Many Requests` con un encabezado `Retry-After` que indica cuántos segundos debe esperar el cliente antes de realizar una nueva solicitud.

## Versionado de API

La API soporta versionado mediante encabezados HTTP:

```
Accept: application/json;version=1.0
```

## CORS

La API soporta Cross-Origin Resource Sharing (CORS) para permitir solicitudes desde dominios específicos. Los dominios permitidos se configuran en `appsettings.json`.

## Swagger / OpenAPI

La documentación interactiva de la API está disponible en:

```
https://[dominio]/swagger
```

Esta interfaz permite explorar todos los endpoints, ver los modelos de datos y probar las solicitudes directamente desde el navegador.

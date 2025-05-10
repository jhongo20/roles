# Documentación del UserManagementController

## Descripción General
El `UserManagementController` proporciona endpoints para la gestión de usuarios del sistema, permitiendo crear, listar, actualizar, cambiar estado, restablecer contraseñas y reenviar correos de activación a los usuarios.

## Endpoints

### 1. Obtener todos los usuarios
**Endpoint:** `GET /api/user-management`

**Parámetros de consulta:**
- `includeInactive` (boolean, opcional): Si se deben incluir usuarios inactivos. Valor predeterminado: `false`.
- `searchTerm` (string, opcional): Término de búsqueda para filtrar usuarios por nombre, apellido, correo o nombre de usuario.
- `pageNumber` (integer, opcional): Número de página para paginación.
- `pageSize` (integer, opcional): Tamaño de página para paginación.

**Respuesta exitosa (200 OK):**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "username": "johndoe",
    "email": "john.doe@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "phoneNumber": "+1234567890",
    "isActive": true,
    "isEmailConfirmed": true,
    "isLocked": false,
    "lockoutEnd": null,
    "lastLoginAt": "2025-05-09T12:00:00Z",
    "createdAt": "2025-05-01T12:00:00Z",
    "updatedAt": "2025-05-09T12:00:00Z"
  }
]
```

**Permisos requeridos:** `ViewUsers`

### 2. Obtener un usuario por su ID
**Endpoint:** `GET /api/user-management/{id}`

**Parámetros de ruta:**
- `id` (Guid): ID del usuario a obtener.

**Respuesta exitosa (200 OK):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "username": "johndoe",
  "email": "john.doe@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+1234567890",
  "isActive": true,
  "isEmailConfirmed": true,
  "isLocked": false,
  "lockoutEnd": null,
  "lastLoginAt": "2025-05-09T12:00:00Z",
  "createdAt": "2025-05-01T12:00:00Z",
  "updatedAt": "2025-05-09T12:00:00Z"
}
```

**Respuesta de error (404 Not Found):**
```json
"No se encontró el usuario con ID '{id}'"
```

**Permisos requeridos:** `ViewUsers`

### 3. Crear un nuevo usuario
**Endpoint:** `POST /api/user-management`

**Cuerpo de la solicitud:**
```json
{
  "username": "janedoe",
  "email": "jane.doe@example.com",
  "password": "SecurePassword123!",
  "firstName": "Jane",
  "lastName": "Doe",
  "phoneNumber": "+1987654321",
  "sendActivationEmail": true,
  "requirePasswordChange": true,
  "isActive": true
}
```

**Respuesta exitosa (201 Created):**
```json
{
  "id": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
  "username": "janedoe",
  "email": "jane.doe@example.com",
  "firstName": "Jane",
  "lastName": "Doe",
  "isActive": true,
  "success": true,
  "message": "Usuario 'janedoe' creado exitosamente"
}
```

**Respuesta de error (400 Bad Request):**
```json
"Ya existe un usuario con el nombre de usuario 'janedoe'"
```

**Permisos requeridos:** `CreateUsers`

### 4. Actualizar un usuario existente
**Endpoint:** `PUT /api/user-management/{id}`

**Parámetros de ruta:**
- `id` (Guid): ID del usuario a actualizar.

**Cuerpo de la solicitud:**
```json
{
  "id": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
  "firstName": "Jane M.",
  "lastName": "Doe",
  "phoneNumber": "+1987654322",
  "isActive": true
}
```

**Respuesta exitosa (200 OK):**
```json
{
  "id": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
  "username": "janedoe",
  "email": "jane.doe@example.com",
  "firstName": "Jane M.",
  "lastName": "Doe",
  "phoneNumber": "+1987654322",
  "isActive": true,
  "success": true,
  "message": "Usuario 'janedoe' actualizado exitosamente"
}
```

**Respuesta de error (400 Bad Request):**
```json
"El ID del usuario en la URL no coincide con el ID en el cuerpo de la solicitud"
```

**Permisos requeridos:** `UpdateUsers`

### 5. Cambiar el estado de un usuario
**Endpoint:** `PATCH /api/user-management/{id}/status`

**Parámetros de ruta:**
- `id` (Guid): ID del usuario.

**Cuerpo de la solicitud:**
```json
{
  "status": "Suspended",
  "reason": "Violación de términos de servicio",
  "lockoutEnd": "2025-06-09T12:00:00Z"
}
```

**Valores permitidos para `status`:**
- `Active`: Activa la cuenta del usuario.
- `Suspended`: Suspende temporalmente la cuenta del usuario.
- `Locked`: Bloquea la cuenta del usuario hasta la fecha especificada en `lockoutEnd`.

**Respuesta exitosa (200 OK):**
```json
{
  "id": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
  "username": "janedoe",
  "email": "jane.doe@example.com",
  "isActive": false,
  "isLocked": true,
  "lockoutEnd": "2025-06-09T12:00:00Z",
  "success": true,
  "message": "Estado del usuario 'janedoe' cambiado a 'Suspended'"
}
```

**Respuesta de error (400 Bad Request):**
```json
"Estado de usuario no válido. Los valores permitidos son: Active, Suspended, Locked"
```

**Permisos requeridos:** `ManageUserStatus`

### 6. Restablecer la contraseña de un usuario
**Endpoint:** `POST /api/user-management/{id}/reset-password`

**Parámetros de ruta:**
- `id` (Guid): ID del usuario.

**Cuerpo de la solicitud:**
```json
{
  "newPassword": "NewSecurePassword456!",
  "requirePasswordChange": true,
  "sendPasswordResetEmail": true
}
```

**Respuesta exitosa (200 OK):**
```json
{
  "id": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
  "username": "janedoe",
  "success": true,
  "message": "Contraseña del usuario 'janedoe' restablecida exitosamente"
}
```

**Respuesta de error (404 Not Found):**
```json
"No se encontró el usuario con ID '{id}'"
```

**Permisos requeridos:** `ResetUserPasswords`

### 7. Reenviar correo de activación
**Endpoint:** `POST /api/user-management/{id}/resend-activation`

**Parámetros de ruta:**
- `id` (Guid): ID del usuario.

**Respuesta exitosa (200 OK):**
```json
{
  "id": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
  "username": "janedoe",
  "email": "jane.doe@example.com",
  "success": true,
  "message": "Correo de activación reenviado exitosamente a 'jane.doe@example.com'"
}
```

**Respuesta de error (400 Bad Request):**
```json
"El usuario 'janedoe' ya ha confirmado su correo electrónico"
```

**Permisos requeridos:** `ManageUsers`

## Manejo de errores
Todos los endpoints pueden devolver los siguientes códigos de estado en caso de error:

- `400 Bad Request`: Cuando la solicitud es inválida o contiene datos incorrectos.
- `401 Unauthorized`: Cuando el usuario no está autenticado.
- `403 Forbidden`: Cuando el usuario no tiene los permisos necesarios.
- `404 Not Found`: Cuando el recurso solicitado no existe.
- `500 Internal Server Error`: Cuando ocurre un error interno en el servidor.

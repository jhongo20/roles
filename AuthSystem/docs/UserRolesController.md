# Documentación del UserRolesController

## Descripción General
El `UserRolesController` proporciona endpoints para la gestión de roles de usuarios, permitiendo asignar roles a usuarios, quitar roles de usuarios y obtener los roles asignados a un usuario específico.

## Endpoints

### 1. Obtener roles de un usuario
**Endpoint:** `GET /api/user-roles/{userId}`

**Parámetros de ruta:**
- `userId` (Guid): ID del usuario.

**Respuesta exitosa (200 OK):**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Administrador",
    "description": "Rol con acceso completo al sistema",
    "isActive": true,
    "createdAt": "2025-05-01T12:00:00Z",
    "updatedAt": "2025-05-01T12:00:00Z"
  },
  {
    "id": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
    "name": "Editor",
    "description": "Rol con permisos de edición",
    "isActive": true,
    "createdAt": "2025-05-01T12:00:00Z",
    "updatedAt": "2025-05-01T12:00:00Z"
  }
]
```

**Respuesta de error (404 Not Found):**
```json
"No se encontró el usuario con ID '{userId}'"
```

**Permisos requeridos:** `ViewUserRoles`

### 2. Asignar un rol a un usuario
**Endpoint:** `POST /api/user-roles`

**Cuerpo de la solicitud:**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "roleId": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
  "assignedBy": "5fa85f64-5717-4562-b3fc-2c963f66afa8"
}
```

**Respuesta exitosa (200 OK):**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userName": "johndoe",
  "roleId": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
  "roleName": "Editor",
  "assignedAt": "2025-05-09T12:00:00Z",
  "assignedBy": "5fa85f64-5717-4562-b3fc-2c963f66afa8",
  "assignedByName": "admin",
  "success": true,
  "message": "Rol 'Editor' asignado exitosamente al usuario 'johndoe'"
}
```

**Respuesta de error (400 Bad Request):**
```json
"El rol 'Editor' ya está asignado al usuario 'johndoe'"
```

**Permisos requeridos:** `AssignUserRoles`

### 3. Quitar un rol de un usuario
**Endpoint:** `DELETE /api/user-roles/{userId}/{roleId}`

**Parámetros de ruta:**
- `userId` (Guid): ID del usuario.
- `roleId` (Guid): ID del rol a quitar.

**Respuesta exitosa (200 OK):**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userName": "johndoe",
  "roleId": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
  "roleName": "Editor",
  "success": true,
  "message": "Rol 'Editor' quitado exitosamente del usuario 'johndoe'"
}
```

**Respuesta de error (400 Bad Request):**
```json
"El rol 'Editor' no está asignado al usuario 'johndoe'"
```

**Permisos requeridos:** `RemoveUserRoles`

## Manejo de errores
Todos los endpoints pueden devolver los siguientes códigos de estado en caso de error:

- `400 Bad Request`: Cuando la solicitud es inválida o contiene datos incorrectos.
- `401 Unauthorized`: Cuando el usuario no está autenticado.
- `403 Forbidden`: Cuando el usuario no tiene los permisos necesarios.
- `404 Not Found`: Cuando el recurso solicitado no existe.
- `500 Internal Server Error`: Cuando ocurre un error interno en el servidor.

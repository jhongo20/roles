# Documentación del PermissionsController

## Descripción General
El `PermissionsController` proporciona endpoints para la gestión de permisos del sistema, permitiendo crear, listar, actualizar y eliminar permisos, así como obtener permisos por categoría.

## Endpoints

### 1. Obtener todos los permisos
**Endpoint:** `GET /api/permissions`

**Parámetros de consulta:**
- `includeInactive` (boolean, opcional): Si se deben incluir permisos inactivos. Valor predeterminado: `false`.
- `category` (string, opcional): Filtrar permisos por categoría.

**Respuesta exitosa (200 OK):**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Ver Usuarios",
    "code": "VIEW_USERS",
    "description": "Permite ver la lista de usuarios",
    "category": "Usuarios",
    "isActive": true,
    "createdAt": "2025-05-01T12:00:00Z",
    "updatedAt": "2025-05-01T12:00:00Z"
  },
  {
    "id": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
    "name": "Crear Usuarios",
    "code": "CREATE_USERS",
    "description": "Permite crear nuevos usuarios",
    "category": "Usuarios",
    "isActive": true,
    "createdAt": "2025-05-01T12:00:00Z",
    "updatedAt": "2025-05-01T12:00:00Z"
  }
]
```

**Permisos requeridos:** `ViewPermissions`

### 2. Obtener un permiso por su ID
**Endpoint:** `GET /api/permissions/{id}`

**Parámetros de ruta:**
- `id` (Guid): ID del permiso a obtener.

**Respuesta exitosa (200 OK):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Ver Usuarios",
  "code": "VIEW_USERS",
  "description": "Permite ver la lista de usuarios",
  "category": "Usuarios",
  "isActive": true,
  "createdAt": "2025-05-01T12:00:00Z",
  "updatedAt": "2025-05-01T12:00:00Z"
}
```

**Respuesta de error (404 Not Found):**
```json
"No se encontró el permiso con ID '{id}'"
```

**Permisos requeridos:** `ViewPermissions`

### 3. Obtener permisos por categoría
**Endpoint:** `GET /api/permissions/categories`

**Respuesta exitosa (200 OK):**
```json
{
  "Usuarios": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "Ver Usuarios",
      "code": "VIEW_USERS",
      "description": "Permite ver la lista de usuarios",
      "category": "Usuarios",
      "isActive": true,
      "createdAt": "2025-05-01T12:00:00Z",
      "updatedAt": "2025-05-01T12:00:00Z"
    },
    {
      "id": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
      "name": "Crear Usuarios",
      "code": "CREATE_USERS",
      "description": "Permite crear nuevos usuarios",
      "category": "Usuarios",
      "isActive": true,
      "createdAt": "2025-05-01T12:00:00Z",
      "updatedAt": "2025-05-01T12:00:00Z"
    }
  ],
  "Roles": [
    {
      "id": "5fa85f64-5717-4562-b3fc-2c963f66afa8",
      "name": "Ver Roles",
      "code": "VIEW_ROLES",
      "description": "Permite ver la lista de roles",
      "category": "Roles",
      "isActive": true,
      "createdAt": "2025-05-01T12:00:00Z",
      "updatedAt": "2025-05-01T12:00:00Z"
    }
  ]
}
```

**Permisos requeridos:** `ViewPermissions`

### 4. Crear un nuevo permiso
**Endpoint:** `POST /api/permissions`

**Cuerpo de la solicitud:**
```json
{
  "name": "Editar Usuarios",
  "code": "EDIT_USERS",
  "description": "Permite editar usuarios existentes",
  "category": "Usuarios",
  "isActive": true
}
```

**Respuesta exitosa (201 Created):**
```json
{
  "id": "6fa85f64-5717-4562-b3fc-2c963f66afa9",
  "name": "Editar Usuarios",
  "code": "EDIT_USERS",
  "description": "Permite editar usuarios existentes",
  "category": "Usuarios",
  "isActive": true,
  "createdAt": "2025-05-09T12:00:00Z",
  "updatedAt": null,
  "success": true,
  "message": "Permiso 'Editar Usuarios' creado exitosamente"
}
```

**Respuesta de error (400 Bad Request):**
```json
"Ya existe un permiso con el código 'EDIT_USERS'"
```

**Permisos requeridos:** `CreatePermissions`

### 5. Actualizar un permiso existente
**Endpoint:** `PUT /api/permissions/{id}`

**Parámetros de ruta:**
- `id` (Guid): ID del permiso a actualizar.

**Cuerpo de la solicitud:**
```json
{
  "id": "6fa85f64-5717-4562-b3fc-2c963f66afa9",
  "name": "Modificar Usuarios",
  "description": "Permite modificar usuarios existentes",
  "category": "Usuarios",
  "isActive": true
}
```

**Respuesta exitosa (200 OK):**
```json
{
  "id": "6fa85f64-5717-4562-b3fc-2c963f66afa9",
  "name": "Modificar Usuarios",
  "code": "EDIT_USERS",
  "description": "Permite modificar usuarios existentes",
  "category": "Usuarios",
  "isActive": true,
  "createdAt": "2025-05-09T12:00:00Z",
  "updatedAt": "2025-05-09T13:00:00Z",
  "success": true,
  "message": "Permiso 'Modificar Usuarios' actualizado exitosamente"
}
```

**Respuesta de error (400 Bad Request):**
```json
"El ID del permiso en la URL no coincide con el ID en el cuerpo de la solicitud"
```

**Permisos requeridos:** `UpdatePermissions`

### 6. Eliminar un permiso
**Endpoint:** `DELETE /api/permissions/{id}`

**Parámetros de ruta:**
- `id` (Guid): ID del permiso a eliminar.

**Respuesta exitosa (200 OK):**
```json
{
  "id": "6fa85f64-5717-4562-b3fc-2c963f66afa9",
  "name": "Modificar Usuarios",
  "code": "EDIT_USERS",
  "isActive": false,
  "success": true,
  "message": "Permiso 'Modificar Usuarios' eliminado exitosamente"
}
```

**Respuesta de error (400 Bad Request):**
```json
"No se puede eliminar el permiso 'Modificar Usuarios' porque está siendo utilizado por roles o módulos"
```

**Permisos requeridos:** `DeletePermissions`

## Manejo de errores
Todos los endpoints pueden devolver los siguientes códigos de estado en caso de error:

- `400 Bad Request`: Cuando la solicitud es inválida o contiene datos incorrectos.
- `401 Unauthorized`: Cuando el usuario no está autenticado.
- `403 Forbidden`: Cuando el usuario no tiene los permisos necesarios.
- `404 Not Found`: Cuando el recurso solicitado no existe.
- `500 Internal Server Error`: Cuando ocurre un error interno en el servidor.

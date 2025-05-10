# Documentación del ModulesController

## Descripción General
El `ModulesController` proporciona endpoints para la gestión de módulos del sistema, permitiendo crear, listar, actualizar y eliminar módulos, así como asociar y desasociar permisos a estos módulos.

## Endpoints

### 1. Obtener todos los módulos
**Endpoint:** `GET /api/modules`

**Parámetros de consulta:**
- `includeInactive` (boolean, opcional): Si se deben incluir módulos inactivos. Valor predeterminado: `false`.
- `includePermissions` (boolean, opcional): Si se deben incluir los permisos de cada módulo. Valor predeterminado: `false`.

**Respuesta exitosa (200 OK):**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Administración",
    "code": "ADMIN",
    "description": "Módulo de administración del sistema",
    "icon": "fa-cogs",
    "route": "/admin",
    "displayOrder": 1,
    "isActive": true,
    "parentId": null,
    "parentModuleName": null,
    "createdAt": "2025-05-09T12:00:00Z",
    "updatedAt": "2025-05-09T12:00:00Z",
    "childModules": [
      {
        "id": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
        "name": "Usuarios",
        "code": "USERS",
        "description": "Gestión de usuarios",
        "icon": "fa-users",
        "route": "/admin/users",
        "displayOrder": 1,
        "isActive": true,
        "parentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "parentModuleName": "Administración",
        "createdAt": "2025-05-09T12:00:00Z",
        "updatedAt": "2025-05-09T12:00:00Z",
        "childModules": [],
        "permissions": []
      }
    ],
    "permissions": [
      {
        "id": "5fa85f64-5717-4562-b3fc-2c963f66afa8",
        "name": "Ver Módulos",
        "code": "VIEW_MODULES",
        "description": "Permite ver los módulos del sistema",
        "category": "Módulos",
        "isActive": true,
        "createdAt": "2025-05-09T12:00:00Z",
        "updatedAt": "2025-05-09T12:00:00Z"
      }
    ]
  }
]
```

**Permisos requeridos:** `ViewModules`

### 2. Obtener un módulo por su ID
**Endpoint:** `GET /api/modules/{id}`

**Parámetros de ruta:**
- `id` (Guid): ID del módulo a obtener.

**Parámetros de consulta:**
- `includePermissions` (boolean, opcional): Si se deben incluir los permisos del módulo. Valor predeterminado: `false`.

**Respuesta exitosa (200 OK):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Administración",
  "code": "ADMIN",
  "description": "Módulo de administración del sistema",
  "icon": "fa-cogs",
  "route": "/admin",
  "displayOrder": 1,
  "isActive": true,
  "parentId": null,
  "parentModuleName": null,
  "createdAt": "2025-05-09T12:00:00Z",
  "updatedAt": "2025-05-09T12:00:00Z",
  "childModules": [],
  "permissions": []
}
```

**Respuesta de error (404 Not Found):**
```json
"No se encontró el módulo con ID '{id}'"
```

**Permisos requeridos:** `ViewModules`

### 3. Obtener los permisos de un módulo
**Endpoint:** `GET /api/modules/{id}/permissions`

**Parámetros de ruta:**
- `id` (Guid): ID del módulo.

**Respuesta exitosa (200 OK):**
```json
[
  {
    "id": "5fa85f64-5717-4562-b3fc-2c963f66afa8",
    "name": "Ver Módulos",
    "code": "VIEW_MODULES",
    "description": "Permite ver los módulos del sistema",
    "category": "Módulos",
    "isActive": true,
    "createdAt": "2025-05-09T12:00:00Z",
    "updatedAt": "2025-05-09T12:00:00Z"
  }
]
```

**Respuesta de error (404 Not Found):**
```json
"No se encontró el módulo con ID '{id}'"
```

**Permisos requeridos:** `ViewModules`

### 4. Crear un nuevo módulo
**Endpoint:** `POST /api/modules`

**Cuerpo de la solicitud:**
```json
{
  "name": "Reportes",
  "code": "REPORTS",
  "description": "Módulo de reportes del sistema",
  "icon": "fa-chart-bar",
  "route": "/reports",
  "displayOrder": 2,
  "parentId": null,
  "isActive": true
}
```

**Respuesta exitosa (201 Created):**
```json
{
  "id": "6fa85f64-5717-4562-b3fc-2c963f66afa9",
  "name": "Reportes",
  "code": "REPORTS",
  "description": "Módulo de reportes del sistema",
  "icon": "fa-chart-bar",
  "route": "/reports",
  "displayOrder": 2,
  "isActive": true,
  "parentId": null,
  "createdAt": "2025-05-09T12:00:00Z",
  "updatedAt": null,
  "success": true,
  "message": "Módulo 'Reportes' creado exitosamente"
}
```

**Respuesta de error (400 Bad Request):**
```json
"Ya existe un módulo con el código 'REPORTS'"
```

**Permisos requeridos:** `CreateModules`

### 5. Actualizar un módulo existente
**Endpoint:** `PUT /api/modules/{id}`

**Parámetros de ruta:**
- `id` (Guid): ID del módulo a actualizar.

**Cuerpo de la solicitud:**
```json
{
  "id": "6fa85f64-5717-4562-b3fc-2c963f66afa9",
  "name": "Reportes Avanzados",
  "description": "Módulo de reportes avanzados del sistema",
  "icon": "fa-chart-line",
  "route": "/advanced-reports",
  "displayOrder": 3,
  "parentId": null,
  "isActive": true
}
```

**Respuesta exitosa (200 OK):**
```json
{
  "id": "6fa85f64-5717-4562-b3fc-2c963f66afa9",
  "name": "Reportes Avanzados",
  "code": "REPORTS",
  "description": "Módulo de reportes avanzados del sistema",
  "icon": "fa-chart-line",
  "route": "/advanced-reports",
  "displayOrder": 3,
  "isActive": true,
  "parentId": null,
  "createdAt": "2025-05-09T12:00:00Z",
  "updatedAt": "2025-05-09T13:00:00Z",
  "success": true,
  "message": "Módulo 'Reportes Avanzados' actualizado exitosamente"
}
```

**Respuesta de error (400 Bad Request):**
```json
"El ID del módulo en la URL no coincide con el ID en el cuerpo de la solicitud"
```

**Permisos requeridos:** `UpdateModules`

### 6. Eliminar un módulo
**Endpoint:** `DELETE /api/modules/{id}`

**Parámetros de ruta:**
- `id` (Guid): ID del módulo a eliminar.

**Respuesta exitosa (200 OK):**
```json
{
  "id": "6fa85f64-5717-4562-b3fc-2c963f66afa9",
  "name": "Reportes Avanzados",
  "code": "REPORTS",
  "isActive": false,
  "success": true,
  "message": "Módulo 'Reportes Avanzados' eliminado exitosamente"
}
```

**Respuesta de error (400 Bad Request):**
```json
"No se puede eliminar el módulo 'Reportes Avanzados' porque tiene submódulos asociados"
```

**Permisos requeridos:** `DeleteModules`

### 7. Asociar un permiso a un módulo
**Endpoint:** `POST /api/modules/{moduleId}/permissions/{permissionId}`

**Parámetros de ruta:**
- `moduleId` (Guid): ID del módulo.
- `permissionId` (Guid): ID del permiso a asociar.

**Respuesta exitosa (200 OK):**
```json
{
  "moduleId": "6fa85f64-5717-4562-b3fc-2c963f66afa9",
  "moduleName": "Reportes Avanzados",
  "permissionId": "5fa85f64-5717-4562-b3fc-2c963f66afa8",
  "permissionName": "Ver Reportes",
  "permissionCode": "VIEW_REPORTS",
  "success": true,
  "message": "Permiso 'Ver Reportes' asociado exitosamente al módulo 'Reportes Avanzados'"
}
```

**Respuesta de error (400 Bad Request):**
```json
"El permiso 'Ver Reportes' ya está asociado al módulo 'Reportes Avanzados'"
```

**Permisos requeridos:** `UpdateModules`

### 8. Quitar un permiso de un módulo
**Endpoint:** `DELETE /api/modules/{moduleId}/permissions/{permissionId}`

**Parámetros de ruta:**
- `moduleId` (Guid): ID del módulo.
- `permissionId` (Guid): ID del permiso a quitar.

**Respuesta exitosa (200 OK):**
```json
{
  "moduleId": "6fa85f64-5717-4562-b3fc-2c963f66afa9",
  "moduleName": "Reportes Avanzados",
  "permissionId": "5fa85f64-5717-4562-b3fc-2c963f66afa8",
  "permissionName": "Ver Reportes",
  "permissionCode": "VIEW_REPORTS",
  "success": true,
  "message": "Permiso 'Ver Reportes' quitado exitosamente del módulo 'Reportes Avanzados'"
}
```

**Respuesta de error (400 Bad Request):**
```json
"El permiso 'Ver Reportes' no está asociado al módulo 'Reportes Avanzados'"
```

**Permisos requeridos:** `UpdateModules`

## Manejo de errores
Todos los endpoints pueden devolver los siguientes códigos de estado en caso de error:

- `400 Bad Request`: Cuando la solicitud es inválida o contiene datos incorrectos.
- `401 Unauthorized`: Cuando el usuario no está autenticado.
- `403 Forbidden`: Cuando el usuario no tiene los permisos necesarios.
- `404 Not Found`: Cuando el recurso solicitado no existe.
- `500 Internal Server Error`: Cuando ocurre un error interno en el servidor.

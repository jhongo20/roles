# Documentación de DTOs para Módulos

## Descripción General
Este documento describe los objetos de transferencia de datos (DTOs) utilizados para la gestión de módulos en el sistema.

## ModuleDto

Este DTO representa un módulo del sistema y se utiliza principalmente para respuestas de consultas.

```csharp
public class ModuleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
    public string Route { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public Guid? ParentId { get; set; }
    public string ParentModuleName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<ModuleDto> ChildModules { get; set; } = new List<ModuleDto>();
    public List<PermissionDto> Permissions { get; set; } = new List<PermissionDto>();
}
```

### Propiedades
- `Id`: Identificador único del módulo.
- `Name`: Nombre del módulo.
- `Code`: Código único del módulo.
- `Description`: Descripción del módulo.
- `Icon`: Icono asociado al módulo (generalmente una clase CSS o nombre de icono).
- `Route`: Ruta de navegación del módulo.
- `DisplayOrder`: Orden de visualización del módulo.
- `IsActive`: Indica si el módulo está activo.
- `ParentId`: ID del módulo padre (si es un submódulo).
- `ParentModuleName`: Nombre del módulo padre (si es un submódulo).
- `CreatedAt`: Fecha y hora de creación.
- `UpdatedAt`: Fecha y hora de última actualización.
- `ChildModules`: Lista de submódulos.
- `Permissions`: Lista de permisos asociados al módulo.

## ModuleResponseDto

Este DTO se utiliza para respuestas de operaciones de creación, actualización y eliminación de módulos.

```csharp
public class ModuleResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
    public string Route { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public Guid? ParentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; }
}
```

### Propiedades
- `Id`: Identificador único del módulo.
- `Name`: Nombre del módulo.
- `Code`: Código único del módulo.
- `Description`: Descripción del módulo.
- `Icon`: Icono asociado al módulo.
- `Route`: Ruta de navegación del módulo.
- `DisplayOrder`: Orden de visualización del módulo.
- `IsActive`: Indica si el módulo está activo.
- `ParentId`: ID del módulo padre (si es un submódulo).
- `CreatedAt`: Fecha y hora de creación.
- `UpdatedAt`: Fecha y hora de última actualización.
- `Success`: Indica si la operación fue exitosa.
- `Message`: Mensaje descriptivo del resultado de la operación.

## ModulePermissionResponseDto

Este DTO se utiliza para respuestas de operaciones de asociación y desasociación de permisos a módulos.

```csharp
public class ModulePermissionResponseDto
{
    public Guid ModuleId { get; set; }
    public string ModuleName { get; set; }
    public Guid PermissionId { get; set; }
    public string PermissionName { get; set; }
    public string PermissionCode { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; }
}
```

### Propiedades
- `ModuleId`: ID del módulo.
- `ModuleName`: Nombre del módulo.
- `PermissionId`: ID del permiso.
- `PermissionName`: Nombre del permiso.
- `PermissionCode`: Código del permiso.
- `Success`: Indica si la operación fue exitosa.
- `Message`: Mensaje descriptivo del resultado de la operación.

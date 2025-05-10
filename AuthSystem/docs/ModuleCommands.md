# Documentación de Comandos para Módulos

## Descripción General
Este documento describe los comandos utilizados para la gestión de módulos en el sistema, siguiendo el patrón CQRS (Command Query Responsibility Segregation).

## CreateModuleCommand

Este comando se utiliza para crear un nuevo módulo en el sistema.

```csharp
public class CreateModuleCommand : IRequest<ModuleResponseDto>
{
    public string Name { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
    public string Route { get; set; }
    public int DisplayOrder { get; set; }
    public Guid? ParentId { get; set; }
    public bool IsActive { get; set; } = true;
}
```

### Propiedades
- `Name`: Nombre del módulo (requerido).
- `Code`: Código único del módulo (requerido).
- `Description`: Descripción del módulo.
- `Icon`: Icono asociado al módulo.
- `Route`: Ruta de navegación del módulo.
- `DisplayOrder`: Orden de visualización del módulo.
- `ParentId`: ID del módulo padre (si es un submódulo).
- `IsActive`: Indica si el módulo está activo. Valor predeterminado: `true`.

### Validaciones
- El nombre y código son obligatorios.
- El código debe ser único en el sistema.
- Si se especifica un ParentId, debe existir un módulo con ese ID.

### Comportamiento
1. Verifica si ya existe un módulo con el mismo código.
2. Si se especifica un módulo padre, verifica que exista.
3. Crea un nuevo módulo con los datos proporcionados.
4. Registra la acción en el sistema de auditoría.
5. Devuelve un ModuleResponseDto con los datos del módulo creado.

## UpdateModuleCommand

Este comando se utiliza para actualizar un módulo existente.

```csharp
public class UpdateModuleCommand : IRequest<ModuleResponseDto>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
    public string Route { get; set; }
    public int DisplayOrder { get; set; }
    public Guid? ParentId { get; set; }
    public bool IsActive { get; set; }
}
```

### Propiedades
- `Id`: ID del módulo a actualizar (requerido).
- `Name`: Nuevo nombre del módulo (requerido).
- `Description`: Nueva descripción del módulo.
- `Icon`: Nuevo icono asociado al módulo.
- `Route`: Nueva ruta de navegación del módulo.
- `DisplayOrder`: Nuevo orden de visualización del módulo.
- `ParentId`: Nuevo ID del módulo padre.
- `IsActive`: Nuevo estado del módulo.

### Validaciones
- El ID y el nombre son obligatorios.
- El módulo con el ID especificado debe existir.
- Si se especifica un ParentId, debe existir un módulo con ese ID.
- Un módulo no puede ser su propio padre.

### Comportamiento
1. Verifica que el módulo a actualizar exista.
2. Si se especifica un módulo padre, verifica que exista y que no sea el mismo módulo.
3. Actualiza las propiedades del módulo con los nuevos valores.
4. Registra la acción en el sistema de auditoría.
5. Devuelve un ModuleResponseDto con los datos actualizados del módulo.

## DeleteModuleCommand

Este comando se utiliza para eliminar (desactivar) un módulo existente.

```csharp
public class DeleteModuleCommand : IRequest<ModuleResponseDto>
{
    public Guid Id { get; set; }
}
```

### Propiedades
- `Id`: ID del módulo a eliminar (requerido).

### Validaciones
- El módulo con el ID especificado debe existir.
- El módulo no debe tener submódulos asociados.
- El módulo no debe tener permisos asociados.

### Comportamiento
1. Verifica que el módulo a eliminar exista.
2. Verifica que el módulo no tenga submódulos asociados.
3. Verifica que el módulo no tenga permisos asociados.
4. Marca el módulo como inactivo (eliminación lógica).
5. Registra la acción en el sistema de auditoría.
6. Devuelve un ModuleResponseDto con los datos del módulo eliminado.

## AddPermissionToModuleCommand

Este comando se utiliza para asociar un permiso a un módulo.

```csharp
public class AddPermissionToModuleCommand : IRequest<ModulePermissionResponseDto>
{
    public Guid ModuleId { get; set; }
    public Guid PermissionId { get; set; }
    public Guid? AssignedBy { get; set; }
}
```

### Propiedades
- `ModuleId`: ID del módulo (requerido).
- `PermissionId`: ID del permiso a asociar (requerido).
- `AssignedBy`: ID del usuario que realiza la asociación.

### Validaciones
- El módulo con el ID especificado debe existir.
- El permiso con el ID especificado debe existir.
- El permiso no debe estar ya asociado al módulo.

### Comportamiento
1. Verifica que el módulo exista.
2. Verifica que el permiso exista.
3. Verifica que el permiso no esté ya asociado al módulo.
4. Asocia el permiso al módulo.
5. Registra la acción en el sistema de auditoría.
6. Devuelve un ModulePermissionResponseDto con los datos de la asociación.

## RemovePermissionFromModuleCommand

Este comando se utiliza para quitar un permiso de un módulo.

```csharp
public class RemovePermissionFromModuleCommand : IRequest<ModulePermissionResponseDto>
{
    public Guid ModuleId { get; set; }
    public Guid PermissionId { get; set; }
}
```

### Propiedades
- `ModuleId`: ID del módulo (requerido).
- `PermissionId`: ID del permiso a quitar (requerido).

### Validaciones
- El módulo con el ID especificado debe existir.
- El permiso con el ID especificado debe existir.
- El permiso debe estar asociado al módulo.

### Comportamiento
1. Verifica que el módulo exista.
2. Verifica que el permiso exista.
3. Verifica que el permiso esté asociado al módulo.
4. Quita la asociación entre el permiso y el módulo.
5. Registra la acción en el sistema de auditoría.
6. Devuelve un ModulePermissionResponseDto con los datos de la desasociación.

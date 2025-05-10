# Documentación de Consultas para Módulos

## Descripción General
Este documento describe las consultas (queries) utilizadas para obtener información sobre los módulos en el sistema, siguiendo el patrón CQRS (Command Query Responsibility Segregation).

## GetAllModulesQuery

Esta consulta se utiliza para obtener todos los módulos del sistema, con opciones para incluir módulos inactivos y sus permisos asociados.

```csharp
public class GetAllModulesQuery : IRequest<List<ModuleDto>>
{
    public bool IncludeInactive { get; set; } = false;
    public bool IncludePermissions { get; set; } = false;
}
```

### Propiedades
- `IncludeInactive`: Indica si se deben incluir módulos inactivos. Valor predeterminado: `false`.
- `IncludePermissions`: Indica si se deben incluir los permisos asociados a cada módulo. Valor predeterminado: `false`.

### Comportamiento
1. Obtiene todos los módulos del sistema, filtrando por estado activo si `IncludeInactive` es `false`.
2. Si `IncludePermissions` es `true`, incluye los permisos asociados a cada módulo.
3. Organiza los módulos jerárquicamente, asignando los submódulos a sus módulos padre.
4. Devuelve una lista de `ModuleDto` con la estructura jerárquica de módulos.

### Ejemplo de uso
```csharp
// Obtener todos los módulos activos sin permisos
var query = new GetAllModulesQuery();
var modules = await _mediator.Send(query);

// Obtener todos los módulos (activos e inactivos) con sus permisos
var queryWithPermissions = new GetAllModulesQuery
{
    IncludeInactive = true,
    IncludePermissions = true
};
var modulesWithPermissions = await _mediator.Send(queryWithPermissions);
```

## GetModuleByIdQuery

Esta consulta se utiliza para obtener un módulo específico por su ID, con opción para incluir sus permisos asociados.

```csharp
public class GetModuleByIdQuery : IRequest<ModuleDto>
{
    public Guid ModuleId { get; set; }
    public bool IncludePermissions { get; set; } = false;
}
```

### Propiedades
- `ModuleId`: ID del módulo a obtener (requerido).
- `IncludePermissions`: Indica si se deben incluir los permisos asociados al módulo. Valor predeterminado: `false`.

### Comportamiento
1. Busca el módulo con el ID especificado.
2. Si no existe, devuelve `null`.
3. Si `IncludePermissions` es `true`, incluye los permisos asociados al módulo.
4. Obtiene los submódulos directos del módulo.
5. Devuelve un `ModuleDto` con la información del módulo, sus permisos (si se solicitaron) y sus submódulos directos.

### Ejemplo de uso
```csharp
// Obtener un módulo por su ID sin permisos
var query = new GetModuleByIdQuery
{
    ModuleId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6")
};
var module = await _mediator.Send(query);

// Obtener un módulo por su ID con sus permisos
var queryWithPermissions = new GetModuleByIdQuery
{
    ModuleId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
    IncludePermissions = true
};
var moduleWithPermissions = await _mediator.Send(queryWithPermissions);
```

## GetModulePermissionsQuery

Esta consulta se utiliza para obtener los permisos asociados a un módulo específico.

```csharp
public class GetModulePermissionsQuery : IRequest<List<PermissionDto>>
{
    public Guid ModuleId { get; set; }
}
```

### Propiedades
- `ModuleId`: ID del módulo cuyos permisos se desean obtener (requerido).

### Validaciones
- El módulo con el ID especificado debe existir.

### Comportamiento
1. Verifica que el módulo exista.
2. Obtiene todos los permisos asociados al módulo.
3. Devuelve una lista de `PermissionDto` con los permisos asociados al módulo.

### Ejemplo de uso
```csharp
// Obtener los permisos de un módulo
var query = new GetModulePermissionsQuery
{
    ModuleId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6")
};
var permissions = await _mediator.Send(query);
```

## Manejo de Errores

Las consultas pueden lanzar las siguientes excepciones:

- `InvalidOperationException`: Cuando ocurre un error de validación, como cuando el módulo no existe.
- Excepciones generales: Cuando ocurre un error inesperado durante la ejecución de la consulta.

Es importante manejar estas excepciones adecuadamente en el código cliente para proporcionar mensajes de error claros al usuario final.

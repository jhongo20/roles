# Visión General del Sistema AuthSystem

## Introducción

AuthSystem es un sistema completo de gestión de autenticación y autorización diseñado para aplicaciones empresariales. Proporciona funcionalidades robustas para la gestión de usuarios, roles, permisos y módulos, permitiendo un control granular sobre quién puede acceder a qué partes de la aplicación.

## Arquitectura

El sistema está construido siguiendo una arquitectura de capas y principios de diseño como Clean Architecture y CQRS (Command Query Responsibility Segregation):

1. **Capa de Presentación (API)**
   - Controllers: Exponen endpoints RESTful para interactuar con el sistema.
   - Filters: Implementan validación y autorización a nivel de controlador.

2. **Capa de Aplicación**
   - Commands: Implementan operaciones que modifican el estado del sistema.
   - Queries: Implementan operaciones de solo lectura.
   - DTOs: Objetos de transferencia de datos para comunicación entre capas.
   - Validators: Validación de comandos y consultas.

3. **Capa de Dominio (Core)**
   - Entities: Modelos de dominio que representan los conceptos del negocio.
   - Interfaces: Contratos para servicios y repositorios.
   - Constants: Valores constantes utilizados en toda la aplicación.

4. **Capa de Infraestructura**
   - Data: Implementaciones de repositorios y acceso a datos.
   - Services: Implementaciones de servicios como email, auditoría, etc.
   - Security: Servicios relacionados con seguridad como JWT, TOTP, etc.

## Componentes Principales

### 1. Gestión de Usuarios

El sistema proporciona funcionalidades completas para la gestión de usuarios:

- Creación de usuarios con opciones para envío de emails de activación
- Actualización de datos de usuarios
- Cambio de estado (activar, suspender, bloquear)
- Restablecimiento de contraseñas
- Reenvío de emails de activación
- Listado y búsqueda de usuarios

### 2. Gestión de Roles

Los roles agrupan permisos y se asignan a usuarios:

- Creación y gestión de roles
- Asignación de permisos a roles
- Asignación de roles a usuarios

### 3. Gestión de Permisos

Los permisos definen acciones específicas que se pueden realizar en el sistema:

- Creación y gestión de permisos
- Agrupación de permisos por categorías
- Asignación de permisos a roles y módulos

### 4. Gestión de Módulos

Los módulos representan secciones o funcionalidades de la aplicación:

- Creación y gestión de módulos
- Estructura jerárquica (módulos y submódulos)
- Asociación de permisos a módulos
- Navegación basada en permisos del usuario

## Flujo de Autorización

1. **Autenticación**: El usuario se autentica mediante credenciales o token.
2. **Carga de Permisos**: Se cargan los permisos del usuario basados en:
   - Permisos asignados directamente al usuario
   - Permisos heredados de los roles asignados
3. **Verificación de Acceso**: Para cada acción o recurso:
   - Se verifica si el usuario tiene el permiso requerido
   - Se filtran los módulos y funcionalidades según los permisos

## Modelo de Datos

### Entidades Principales

1. **User**
   - Información básica del usuario (nombre, email, etc.)
   - Credenciales y estado de la cuenta
   - Historial de actividad

2. **Role**
   - Nombre y descripción
   - Colección de permisos asociados

3. **Permission**
   - Nombre, código y descripción
   - Categoría para agrupación

4. **Module**
   - Nombre, código y descripción
   - Información de navegación (ruta, icono)
   - Estructura jerárquica (módulo padre)
   - Permisos asociados

### Relaciones

- **UserRole**: Relación muchos a muchos entre usuarios y roles
- **RolePermission**: Relación muchos a muchos entre roles y permisos
- **UserPermission**: Relación muchos a muchos entre usuarios y permisos (para asignaciones directas)
- **ModulePermission**: Relación muchos a muchos entre módulos y permisos

## Implementación CQRS

El sistema utiliza el patrón CQRS para separar las operaciones de lectura y escritura:

### Commands (Escritura)

Los comandos representan intenciones de cambiar el estado del sistema:

- Cada comando tiene un único propósito
- Incluyen validación de datos y reglas de negocio
- Devuelven DTOs de respuesta con información sobre el resultado

### Queries (Lectura)

Las consultas recuperan datos sin modificar el estado:

- Optimizadas para rendimiento de lectura
- Pueden incluir filtros y opciones de paginación
- Devuelven DTOs específicos para cada caso de uso

### Mediator

Se utiliza el patrón Mediator (implementado con MediatR) para:

- Desacoplar los controladores de la lógica de aplicación
- Permitir un flujo de procesamiento consistente
- Facilitar la implementación de comportamientos transversales (logging, validación, etc.)

## Seguridad

El sistema implementa múltiples capas de seguridad:

1. **Autenticación**
   - JWT (JSON Web Tokens) para API
   - Soporte para autenticación de dos factores (TOTP)
   - Bloqueo de cuentas tras intentos fallidos

2. **Autorización**
   - Basada en permisos granulares
   - Filtros de autorización a nivel de controlador y acción
   - Verificación de permisos en tiempo de ejecución

3. **Protección de Datos**
   - Almacenamiento seguro de contraseñas (hash + salt)
   - Validación de datos de entrada
   - Protección contra ataques comunes (CSRF, XSS, etc.)

4. **Auditoría**
   - Registro de acciones importantes
   - Seguimiento de cambios en entidades críticas
   - Registro de intentos de acceso fallidos

## Integración Frontend

La API está diseñada para ser consumida por aplicaciones frontend modernas:

- Endpoints RESTful con convenciones consistentes
- Respuestas JSON estructuradas
- Códigos de estado HTTP apropiados
- Mensajes de error descriptivos

Para más detalles sobre cómo integrar con frontend, consulta la [Guía de Integración Frontend](./FrontendIntegrationGuide.md).

## Documentación Detallada

Para información más detallada sobre componentes específicos, consulta:

- [Documentación del ModulesController](./ModulesController.md)
- [Documentación del UserManagementController](./UserManagementController.md)
- [Documentación del UserRolesController](./UserRolesController.md)
- [Documentación del PermissionsController](./PermissionsController.md)
- [Documentación de DTOs para Módulos](./ModuleDTOs.md)
- [Documentación de Comandos para Módulos](./ModuleCommands.md)
- [Documentación de Consultas para Módulos](./ModuleQueries.md)

## Conclusión

AuthSystem proporciona una solución completa y flexible para la gestión de autenticación y autorización en aplicaciones empresariales. Su arquitectura modular y el uso de patrones de diseño modernos facilitan su mantenimiento y extensión para adaptarse a requisitos específicos.

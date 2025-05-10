# Índice de Documentación - AuthSystem

## Visión General
- [Visión General del Sistema](./SystemOverview.md) - Descripción completa de la arquitectura y componentes del sistema

## Guías de Integración
- [Guía de Integración Frontend](./FrontendIntegrationGuide.md) - Instrucciones detalladas para integrar aplicaciones frontend con la API

## Documentación de API

### Controladores
- [ModulesController](./ModulesController.md) - Endpoints para gestión de módulos
- [UserManagementController](./UserManagementController.md) - Endpoints para gestión de usuarios
- [UserRolesController](./UserRolesController.md) - Endpoints para gestión de roles de usuarios
- [PermissionsController](./PermissionsController.md) - Endpoints para gestión de permisos

### Componentes Internos
- [DTOs para Módulos](./ModuleDTOs.md) - Objetos de transferencia de datos para módulos
- [Comandos para Módulos](./ModuleCommands.md) - Comandos CQRS para operaciones de módulos
- [Consultas para Módulos](./ModuleQueries.md) - Consultas CQRS para operaciones de módulos

## Cómo Usar Esta Documentación

1. Comience con la [Visión General del Sistema](./SystemOverview.md) para entender la arquitectura y los componentes principales.
2. Revise la documentación de los controladores específicos que necesite utilizar.
3. Consulte la [Guía de Integración Frontend](./FrontendIntegrationGuide.md) para ejemplos prácticos de cómo consumir la API.
4. Para detalles de implementación, revise la documentación de DTOs, comandos y consultas.

## Notas para Desarrolladores

### Desarrolladores Backend
- La implementación sigue los principios de Clean Architecture y CQRS.
- Cada operación está encapsulada en un comando o consulta específico.
- Los controladores son ligeros y delegan la lógica a los manejadores de comandos y consultas.

### Desarrolladores Frontend
- Todos los endpoints devuelven respuestas JSON estructuradas.
- Los errores incluyen mensajes descriptivos y códigos de estado HTTP apropiados.
- La autenticación se realiza mediante tokens JWT que deben incluirse en el encabezado Authorization.

## Próximos Pasos

1. Implementar pruebas unitarias y de integración para todos los componentes.
2. Documentar los procesos de despliegue y configuración del sistema.
3. Crear ejemplos adicionales de integración con diferentes frameworks frontend.

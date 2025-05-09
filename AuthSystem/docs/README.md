# Sistema AuthSystem - Documentación

## Introducción

AuthSystem es un sistema completo de autenticación y autorización diseñado para proporcionar una solución robusta, segura y escalable para la gestión de identidad de usuarios. El sistema implementa múltiples características de seguridad modernas, incluyendo autenticación de dos factores, confirmación de correo electrónico, gestión de roles y permisos, y auditoría detallada.

## Tabla de Contenidos

1. [Arquitectura del Sistema](./Arquitectura.md)
2. [Autenticación Básica](./AutenticacionBasica.md)
3. [Autenticación de Dos Factores](./TwoFactorAuthentication.md)
4. [Confirmación de Correo Electrónico](./ConfirmacionEmail.md)
5. [Gestión de Usuarios y Roles](./GestionUsuariosRoles.md)
6. [Seguridad y Auditoría](./SeguridadAuditoria.md)
7. [Configuración del Sistema](./Configuracion.md)
8. [Guías de Implementación](./GuiasImplementacion.md)
9. [API Reference](./APIReference.md)

## Características Principales

- **Autenticación Básica**
  - Inicio de sesión con nombre de usuario/correo y contraseña
  - Generación y validación de tokens JWT
  - Refresh tokens para mantener la sesión
  - Cierre de sesión y revocación de sesiones

- **Autenticación de Dos Factores (2FA)**
  - Soporte para múltiples métodos: Authenticator, Email, SMS
  - Generación y validación de códigos TOTP
  - Códigos de recuperación para acceso de emergencia
  - Configuración flexible por usuario

- **Confirmación de Correo Electrónico**
  - Generación y envío de tokens de confirmación
  - Verificación de correos electrónicos
  - Reenvío de correos de confirmación
  - Plantillas HTML personalizables

- **Gestión de Usuarios y Roles**
  - Registro y administración de usuarios
  - Sistema de roles y permisos granular
  - Bloqueo de cuentas y seguimiento de intentos fallidos
  - Perfiles de usuario personalizables

- **Seguridad Avanzada**
  - Limitación de tasa para prevenir ataques de fuerza bruta
  - Historial de contraseñas para prevenir la reutilización
  - Auditoría detallada de acciones de usuario
  - Logging estructurado para monitoreo y diagnóstico

## Arquitectura

AuthSystem sigue una arquitectura de capas bien definida:

- **Core**: Contiene entidades de dominio, interfaces y lógica de negocio central.
- **Application**: Implementa casos de uso mediante comandos y consultas (CQRS).
- **Infrastructure**: Proporciona implementaciones concretas de interfaces definidas en Core.
- **API**: Expone endpoints RESTful para interactuar con el sistema.

El sistema utiliza patrones modernos como:
- Command Query Responsibility Segregation (CQRS)
- Mediator Pattern (implementado con MediatR)
- Repository Pattern
- Dependency Injection

## Tecnologías Utilizadas

- **.NET 8**: Framework base para el desarrollo
- **Entity Framework Core**: ORM para acceso a datos
- **MediatR**: Implementación del patrón mediador
- **FluentValidation**: Validación de entrada
- **JWT**: Tokens de autenticación
- **Azure Communication Services**: Envío de SMS para 2FA
- **Serilog**: Logging estructurado
- **BCrypt**: Hash seguro de contraseñas

## Requisitos del Sistema

- .NET 8 SDK o superior
- SQL Server (o compatible con Entity Framework Core)
- Azure Communication Services (para SMS en producción)
- Servidor SMTP (para envío de correos electrónicos)

## Licencia

Este proyecto está licenciado bajo [Licencia Propietaria] - ver el archivo LICENSE para más detalles.

## Contacto

Para soporte técnico o consultas, contacte a [equipo de soporte].

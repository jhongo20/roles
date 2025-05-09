# Arquitectura del Sistema AuthSystem

## Visión General

AuthSystem implementa una arquitectura de capas bien definida, siguiendo los principios de Clean Architecture y Domain-Driven Design (DDD). Esta arquitectura proporciona una separación clara de responsabilidades, facilitando el mantenimiento, las pruebas y la evolución del sistema.

## Estructura de Capas

El sistema está dividido en cuatro capas principales:

### 1. Core (Núcleo)

La capa Core contiene la lógica de negocio central y es independiente de cualquier framework o tecnología específica.

**Componentes principales:**
- **Entidades de dominio**: Clases que representan los conceptos fundamentales del sistema (User, Role, Permission, etc.).
- **Interfaces**: Definiciones de contratos que serán implementados en capas externas.
- **Enumeraciones y constantes**: Valores y tipos que definen el dominio.
- **Excepciones de dominio**: Excepciones específicas del negocio.

**Características clave:**
- No tiene dependencias de otras capas del sistema.
- Contiene reglas de negocio puras.
- Las entidades tienen propiedades de solo lectura (private set) para garantizar la integridad del dominio.
- Expone métodos públicos para modificar el estado de las entidades de forma controlada.

### 2. Application (Aplicación)

La capa Application implementa los casos de uso específicos de la aplicación, orquestando el flujo de datos y las operaciones entre las entidades del dominio.

**Componentes principales:**
- **Comandos y Queries**: Implementación del patrón CQRS.
- **Handlers**: Manejadores para comandos y queries utilizando MediatR.
- **DTOs**: Objetos de transferencia de datos para la comunicación entre capas.
- **Validadores**: Validación de entrada utilizando FluentValidation.
- **Mappers**: Transformación entre entidades de dominio y DTOs.

**Características clave:**
- Depende de la capa Core, pero no de Infrastructure o API.
- Implementa la lógica de aplicación específica.
- Utiliza interfaces definidas en Core para acceder a servicios externos.

### 3. Infrastructure (Infraestructura)

La capa Infrastructure proporciona implementaciones concretas de las interfaces definidas en Core, integrando tecnologías y servicios externos.

**Componentes principales:**
- **Repositorios**: Implementaciones de las interfaces de repositorio.
- **Servicios externos**: Implementaciones para servicios como email, SMS, etc.
- **Configuración de persistencia**: Configuración de Entity Framework Core.
- **Implementaciones de seguridad**: Servicios de hash, JWT, etc.
- **Logging y auditoría**: Implementación de servicios de registro.

**Características clave:**
- Depende de Core y Application.
- Proporciona implementaciones concretas de interfaces abstractas.
- Integra tecnologías y frameworks específicos.

### 4. API (Interfaz de Aplicación)

La capa API expone la funcionalidad del sistema a través de endpoints RESTful y gestiona la interacción con los clientes.

**Componentes principales:**
- **Controladores**: Endpoints REST para las diferentes funcionalidades.
- **Middleware**: Componentes para procesar solicitudes HTTP.
- **Filtros**: Lógica para procesar solicitudes antes o después de los controladores.
- **Configuración de la aplicación**: Startup, DI, etc.

**Características clave:**
- Depende de todas las demás capas.
- Gestiona la autenticación y autorización a nivel de HTTP.
- Maneja la serialización/deserialización de datos.
- Implementa la documentación de la API (Swagger).

## Patrones de Diseño Utilizados

### Command Query Responsibility Segregation (CQRS)

El sistema separa las operaciones de lectura (queries) y escritura (commands), lo que permite optimizar cada tipo de operación de forma independiente.

**Implementación:**
- **Commands**: Operaciones que modifican el estado del sistema (ej. RegisterUserCommand).
- **Queries**: Operaciones que solo leen datos sin modificar el estado (ej. GetUserByIdQuery).
- **Handlers**: Clases específicas que procesan cada comando o consulta.

### Mediator Pattern

Implementado mediante MediatR, este patrón desacopla los componentes del sistema, permitiendo que se comuniquen sin conocerse directamente.

**Beneficios:**
- Reduce el acoplamiento entre componentes.
- Facilita la implementación de comportamientos transversales (cross-cutting concerns).
- Simplifica la implementación de CQRS.

### Repository Pattern

Proporciona una abstracción sobre la capa de persistencia, permitiendo que la lógica de negocio trabaje con objetos de dominio sin conocer los detalles de almacenamiento.

**Características:**
- Interfaces definidas en Core (ej. IUserRepository).
- Implementaciones concretas en Infrastructure.
- Métodos específicos para operaciones comunes de CRUD.
- Métodos especializados para operaciones específicas del dominio.

### Dependency Injection

Utilizado extensivamente para desacoplar componentes y facilitar las pruebas unitarias.

**Implementación:**
- Registro de servicios en el contenedor DI de .NET.
- Inyección por constructor en controladores y handlers.
- Configuración centralizada en la capa API.

## Flujo de Datos

1. **Solicitud HTTP**: Llega a un controlador en la capa API.
2. **Controlador**: Convierte la solicitud en un comando o consulta.
3. **Mediator**: Enruta el comando/consulta al handler correspondiente.
4. **Handler**: Procesa la solicitud utilizando servicios y repositorios.
5. **Repositorio/Servicio**: Interactúa con la base de datos o servicios externos.
6. **Respuesta**: El resultado vuelve a través de la cadena hasta el cliente.

## Estructura de Directorios

```
AuthSystem/
├── src/
│   ├── AuthSystem.Core/
│   │   ├── Entities/
│   │   ├── Interfaces/
│   │   ├── Enums/
│   │   └── Constants/
│   ├── AuthSystem.Application/
│   │   ├── Commands/
│   │   ├── Queries/
│   │   ├── DTOs/
│   │   ├── Validators/
│   │   └── Mappings/
│   ├── AuthSystem.Infrastructure/
│   │   ├── Persistence/
│   │   ├── Services/
│   │   ├── Repositories/
│   │   └── Identity/
│   └── AuthSystem.API/
│       ├── Controllers/
│       ├── Middleware/
│       ├── Filters/
│       └── Configuration/
└── tests/
    ├── AuthSystem.UnitTests/
    ├── AuthSystem.IntegrationTests/
    └── AuthSystem.FunctionalTests/
```

## Consideraciones de Diseño

### Inmutabilidad de Entidades

Las entidades de dominio, como `User`, tienen propiedades de solo lectura (private set) y requieren constructores específicos con parámetros obligatorios. Esto garantiza que las entidades siempre estén en un estado válido y que las modificaciones se realicen a través de métodos públicos que encapsulan la lógica de negocio.

### Separación de Responsabilidades

Cada capa tiene responsabilidades claramente definidas, lo que facilita el mantenimiento y las pruebas. Por ejemplo, la validación de entrada se realiza en la capa Application, mientras que la lógica de persistencia está encapsulada en la capa Infrastructure.

### Testabilidad

La arquitectura está diseñada para facilitar las pruebas unitarias e integración:
- Las interfaces permiten crear mocks para pruebas unitarias.
- La inyección de dependencias facilita la sustitución de componentes reales por mocks.
- La separación de comandos y consultas simplifica las pruebas de cada caso de uso.

## Diagramas

### Diagrama de Capas

```
┌─────────────────────────────────────┐
│               API                   │
└───────────────┬─────────────────────┘
                │
                ▼
┌─────────────────────────────────────┐
│            Application              │
└───────────────┬─────────────────────┘
                │
        ┌───────┴───────┐
        │               │
        ▼               ▼
┌───────────────┐ ┌─────────────────┐
│     Core      │ │ Infrastructure  │
└───────────────┘ └─────────────────┘
```

### Flujo de Autenticación

```
┌──────────┐    1. Login Request     ┌────────────────┐
│  Client  │─────────────────────────▶  AuthController│
└────┬─────┘                         └────────┬───────┘
     │                                        │
     │                                        │ 2. AuthenticateCommand
     │                                        ▼
     │                               ┌────────────────┐
     │                               │MediatR Pipeline│
     │                               └────────┬───────┘
     │                                        │
     │                                        │ 3. Process Command
     │                                        ▼
     │                               ┌────────────────┐
     │                               │    Handler     │
     │                               └────────┬───────┘
     │                                        │
     │                                        │ 4. Validate User
     │                                        ▼
     │                               ┌────────────────┐
     │                               │UserRepository  │
     │                               └────────┬───────┘
     │                                        │
     │                                        │ 5. Generate Token
     │                                        ▼
     │                               ┌────────────────┐
     │                               │   JwtService   │
     │                               └────────┬───────┘
     │                                        │
     │         6. Auth Response               │
     │◀───────────────────────────────────────┘
┌────┴─────┐
│  Client  │
└──────────┘
```

## Conclusión

La arquitectura de AuthSystem está diseñada para proporcionar un sistema robusto, mantenible y extensible. La separación clara de responsabilidades, junto con el uso de patrones de diseño modernos, facilita la evolución del sistema y la incorporación de nuevas funcionalidades.

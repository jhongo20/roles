# Gestión de Usuarios y Roles

## Introducción

El sistema AuthSystem proporciona una gestión completa de usuarios y roles, permitiendo administrar de manera eficiente las identidades de los usuarios, sus permisos y roles dentro de la aplicación. Este documento describe los componentes, flujos y configuraciones relacionados con la gestión de usuarios y roles.

## Componentes Principales

### Entidades de Dominio

#### User

La entidad `User` es el componente central del sistema de gestión de usuarios. Representa a un usuario registrado en el sistema y contiene toda la información relevante sobre su identidad y estado.

**Características principales:**
- Propiedades de solo lectura (private set) para garantizar la integridad del dominio
- Métodos públicos para modificar el estado de manera controlada
- Seguimiento de intentos fallidos de inicio de sesión
- Soporte para bloqueo de cuentas

**Estructura básica:**
```csharp
public class User
{
    // Propiedades principales
    public Guid Id { get; private set; }
    public string Username { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string PhoneNumber { get; private set; }
    public bool EmailConfirmed { get; private set; }
    public bool PhoneNumberConfirmed { get; private set; }
    public UserStatus Status { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockoutEnd { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    // Relaciones
    public ICollection<UserRole> UserRoles { get; private set; }
    public ICollection<UserSession> Sessions { get; private set; }
    public UserTwoFactorSettings TwoFactorSettings { get; private set; }

    // Constructor principal
    public User(string username, string email, string passwordHash, string firstName = null, string lastName = null)
    {
        // Inicialización...
    }

    // Métodos públicos para modificar el estado
    public void ConfirmEmail() { /* ... */ }
    public void UpdateProfile(string firstName, string lastName, string phoneNumber) { /* ... */ }
    public bool VerifyPassword(IPasswordHasher passwordHasher, string password) { /* ... */ }
    public void ChangePassword(string newPasswordHash) { /* ... */ }
    public void LockAccount(TimeSpan duration) { /* ... */ }
    public void UnlockAccount() { /* ... */ }
    public void RecordLoginAttempt(bool successful) { /* ... */ }
    public void RecordLoginSuccess() { /* ... */ }
    // Otros métodos...
}
```

#### Role

La entidad `Role` representa un conjunto de permisos y responsabilidades que pueden asignarse a los usuarios.

```csharp
public class Role
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public bool IsSystem { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public ICollection<RolePermission> RolePermissions { get; private set; }
    public ICollection<UserRole> UserRoles { get; private set; }

    // Constructor y métodos...
}
```

#### Permission

La entidad `Permission` representa una acción específica que un usuario puede realizar en el sistema.

```csharp
public class Permission
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Category { get; private set; }
    public bool IsSystem { get; private set; }

    public ICollection<RolePermission> RolePermissions { get; private set; }

    // Constructor y métodos...
}
```

### Repositorios

#### IUserRepository

La interfaz `IUserRepository` proporciona métodos para interactuar con la base de datos de usuarios.

```csharp
public interface IUserRepository
{
    // Métodos básicos CRUD
    Task<User> GetByIdAsync(Guid id);
    Task<User> GetByUsernameAsync(string username);
    Task<User> GetByEmailAsync(string email);
    Task<bool> ExistsAsync(string username, string email);
    Task<Guid> CreateAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(Guid id);

    // Métodos para gestión de sesiones
    Task AddSessionAsync(UserSession session);
    Task<UserSession> GetSessionAsync(Guid userId, string refreshToken);
    Task RevokeSessionAsync(Guid userId, string jti);
    Task RevokeAllUserSessionsAsync(Guid userId);
    Task<bool> ValidateRefreshTokenAsync(Guid userId, string refreshToken);

    // Métodos para gestión de roles
    Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId);
    Task AssignRoleAsync(Guid userId, Guid roleId);
    Task RemoveRoleAsync(Guid userId, Guid roleId);
    Task<bool> UserHasRoleAsync(Guid userId, string roleName);

    // Métodos para gestión de contraseñas
    Task<bool> ChangePasswordAsync(Guid userId, string newPasswordHash);
    Task AddPasswordHistoryAsync(Guid userId, string passwordHash);
    Task<bool> IsPasswordInHistoryAsync(Guid userId, string passwordHash);

    // Otros métodos...
}
```

#### IRoleRepository

La interfaz `IRoleRepository` proporciona métodos para gestionar roles y permisos.

```csharp
public interface IRoleRepository
{
    Task<Role> GetByIdAsync(Guid id);
    Task<Role> GetByNameAsync(string name);
    Task<IEnumerable<Role>> GetAllAsync();
    Task<Guid> CreateAsync(Role role);
    Task UpdateAsync(Role role);
    Task DeleteAsync(Guid id);

    Task<IEnumerable<Permission>> GetRolePermissionsAsync(Guid roleId);
    Task AssignPermissionAsync(Guid roleId, Guid permissionId);
    Task RemovePermissionAsync(Guid roleId, Guid permissionId);
    Task<bool> RoleHasPermissionAsync(Guid roleId, string permissionName);

    // Otros métodos...
}
```

### Comandos y Queries

El sistema utiliza el patrón CQRS con MediatR para implementar los casos de uso relacionados con la gestión de usuarios y roles:

#### Comandos para Usuarios

1. **RegisterUserCommand**: Registra un nuevo usuario en el sistema.
2. **UpdateUserProfileCommand**: Actualiza la información del perfil de un usuario.
3. **ChangePasswordCommand**: Cambia la contraseña de un usuario.
4. **LockUserCommand**: Bloquea temporalmente una cuenta de usuario.
5. **UnlockUserCommand**: Desbloquea una cuenta de usuario.
6. **DeleteUserCommand**: Elimina un usuario del sistema.

#### Comandos para Roles

1. **CreateRoleCommand**: Crea un nuevo rol en el sistema.
2. **UpdateRoleCommand**: Actualiza la información de un rol.
3. **DeleteRoleCommand**: Elimina un rol del sistema.
4. **AssignRoleCommand**: Asigna un rol a un usuario.
5. **RemoveRoleCommand**: Elimina un rol de un usuario.

#### Queries para Usuarios

1. **GetUserByIdQuery**: Obtiene un usuario por su ID.
2. **GetUserByUsernameQuery**: Obtiene un usuario por su nombre de usuario.
3. **GetUserByEmailQuery**: Obtiene un usuario por su correo electrónico.
4. **GetAllUsersQuery**: Obtiene todos los usuarios del sistema con paginación.

#### Queries para Roles

1. **GetRoleByIdQuery**: Obtiene un rol por su ID.
2. **GetAllRolesQuery**: Obtiene todos los roles del sistema.
3. **GetUserRolesQuery**: Obtiene los roles asignados a un usuario.
4. **GetRolePermissionsQuery**: Obtiene los permisos asignados a un rol.

## Flujos de Gestión de Usuarios

### Registro de Usuario

1. El cliente envía una solicitud de registro con la información del usuario.
2. El controlador `UserController` convierte la solicitud en un comando `RegisterUserCommand`.
3. El handler `RegisterUserCommandHandler` valida la información del usuario.
4. Se verifica que el nombre de usuario y correo electrónico no existan en el sistema.
5. Se genera un hash de la contraseña utilizando `PasswordHasher`.
6. Se crea un nuevo usuario con estado "Registrado".
7. Si está habilitada la confirmación de correo electrónico, se genera un token y se envía un correo.
8. Se devuelve una respuesta con el ID del usuario y el siguiente paso requerido.

### Actualización de Perfil

1. El cliente envía una solicitud de actualización con la nueva información del perfil.
2. El controlador `UserProfileController` convierte la solicitud en un comando `UpdateUserProfileCommand`.
3. El handler `UpdateUserProfileCommandHandler` valida la información del perfil.
4. Se obtiene el usuario de la base de datos.
5. Se actualiza la información del perfil utilizando el método `UpdateProfile` del usuario.
6. Se guarda el usuario actualizado en la base de datos.
7. Se devuelve una respuesta con la información actualizada.

### Cambio de Contraseña

1. El cliente envía una solicitud de cambio de contraseña con la contraseña actual y la nueva.
2. El controlador `UserProfileController` convierte la solicitud en un comando `ChangePasswordCommand`.
3. El handler `ChangePasswordCommandHandler` valida las contraseñas.
4. Se obtiene el usuario de la base de datos.
5. Se verifica la contraseña actual utilizando `PasswordHasher`.
6. Se verifica que la nueva contraseña no esté en el historial de contraseñas.
7. Se genera un hash de la nueva contraseña.
8. Se actualiza la contraseña utilizando el método `ChangePassword` del usuario.
9. Se guarda la contraseña en el historial.
10. Se devuelve una respuesta indicando el éxito de la operación.

## Flujos de Gestión de Roles

### Creación de Rol

1. El administrador envía una solicitud de creación de rol con la información del rol.
2. El controlador `RoleController` convierte la solicitud en un comando `CreateRoleCommand`.
3. El handler `CreateRoleCommandHandler` valida la información del rol.
4. Se verifica que el nombre del rol no exista en el sistema.
5. Se crea un nuevo rol.
6. Se devuelve una respuesta con el ID del rol creado.

### Asignación de Rol a Usuario

1. El administrador envía una solicitud para asignar un rol a un usuario.
2. El controlador `UserRoleController` convierte la solicitud en un comando `AssignRoleCommand`.
3. El handler `AssignRoleCommandHandler` valida la información.
4. Se verifica que el usuario y el rol existan.
5. Se verifica que el usuario no tenga ya asignado el rol.
6. Se asigna el rol al usuario.
7. Se devuelve una respuesta indicando el éxito de la operación.

## Endpoints de API

### Usuarios

#### POST /api/users

Registra un nuevo usuario.

**Request:**
```json
{
  "username": "string",
  "email": "string",
  "password": "string",
  "confirmPassword": "string",
  "firstName": "string",
  "lastName": "string"
}
```

**Response:**
```json
{
  "succeeded": true,
  "userId": "guid",
  "requiresEmailConfirmation": boolean,
  "message": "string"
}
```

#### GET /api/users/{id}

Obtiene un usuario por su ID.

**Response:**
```json
{
  "id": "guid",
  "username": "string",
  "email": "string",
  "firstName": "string",
  "lastName": "string",
  "status": "string",
  "emailConfirmed": boolean,
  "phoneNumber": "string",
  "phoneNumberConfirmed": boolean,
  "twoFactorEnabled": boolean,
  "createdAt": "datetime",
  "lastLoginAt": "datetime"
}
```

#### PUT /api/users/profile

Actualiza el perfil de un usuario.

**Request:**
```json
{
  "userId": "guid",
  "firstName": "string",
  "lastName": "string",
  "phoneNumber": "string"
}
```

**Response:**
```json
{
  "succeeded": true,
  "user": {
    "id": "guid",
    "username": "string",
    "email": "string",
    "firstName": "string",
    "lastName": "string",
    "phoneNumber": "string"
  },
  "message": "string"
}
```

#### PUT /api/users/password

Cambia la contraseña de un usuario.

**Request:**
```json
{
  "userId": "guid",
  "currentPassword": "string",
  "newPassword": "string",
  "confirmPassword": "string"
}
```

**Response:**
```json
{
  "succeeded": true,
  "message": "string"
}
```

### Roles

#### POST /api/roles

Crea un nuevo rol.

**Request:**
```json
{
  "name": "string",
  "description": "string"
}
```

**Response:**
```json
{
  "succeeded": true,
  "roleId": "guid",
  "message": "string"
}
```

#### GET /api/roles

Obtiene todos los roles.

**Response:**
```json
{
  "roles": [
    {
      "id": "guid",
      "name": "string",
      "description": "string",
      "isSystem": boolean,
      "createdAt": "datetime"
    }
  ]
}
```

#### POST /api/users/{userId}/roles

Asigna un rol a un usuario.

**Request:**
```json
{
  "roleId": "guid"
}
```

**Response:**
```json
{
  "succeeded": true,
  "message": "string"
}
```

#### DELETE /api/users/{userId}/roles/{roleId}

Elimina un rol de un usuario.

**Response:**
```json
{
  "succeeded": true,
  "message": "string"
}
```

## Seguridad

### Protección de Contraseñas

Las contraseñas se almacenan utilizando BCrypt, un algoritmo de hash diseñado específicamente para contraseñas. Además, el sistema mantiene un historial de contraseñas para prevenir la reutilización.

### Bloqueo de Cuentas

El sistema implementa un mecanismo de bloqueo de cuentas después de un número configurable de intentos fallidos de inicio de sesión. La duración del bloqueo puede configurarse y aumentar progresivamente con intentos fallidos consecutivos.

### Auditoría

Todas las operaciones relacionadas con la gestión de usuarios y roles se registran en el sistema de auditoría, lo que permite rastrear actividades sospechosas y cambios importantes.

## Configuración

### Opciones de Usuario

```json
{
  "UserSettings": {
    "DefaultStatus": "Registered",
    "RequireEmailConfirmation": true,
    "RequireUniqueEmail": true,
    "AllowedUsernameCharacters": "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+"
  }
}
```

### Opciones de Bloqueo

```json
{
  "LockoutSettings": {
    "MaxFailedAttempts": 5,
    "DefaultLockoutMinutes": 30,
    "ProgressiveLockout": true,
    "ProgressiveLockoutMultiplier": 2
  }
}
```

### Opciones de Historial de Contraseñas

```json
{
  "PasswordHistorySettings": {
    "PasswordHistoryLimit": 5,
    "MinimumPasswordAge": 1
  }
}
```

## Roles y Permisos Predefinidos

El sistema incluye roles y permisos predefinidos que se crean durante la inicialización:

### Roles

1. **Administrator**: Acceso completo a todas las funcionalidades del sistema.
2. **User**: Acceso básico a las funcionalidades del sistema.
3. **Guest**: Acceso limitado a funcionalidades públicas.

### Permisos

Los permisos se organizan por categorías:

1. **Users**: Permisos relacionados con la gestión de usuarios.
   - `users.view`: Ver información de usuarios.
   - `users.create`: Crear nuevos usuarios.
   - `users.update`: Actualizar información de usuarios.
   - `users.delete`: Eliminar usuarios.

2. **Roles**: Permisos relacionados con la gestión de roles.
   - `roles.view`: Ver roles.
   - `roles.create`: Crear nuevos roles.
   - `roles.update`: Actualizar roles.
   - `roles.delete`: Eliminar roles.
   - `roles.assign`: Asignar roles a usuarios.

3. **System**: Permisos relacionados con la configuración del sistema.
   - `system.settings.view`: Ver configuración del sistema.
   - `system.settings.update`: Actualizar configuración del sistema.
   - `system.logs.view`: Ver logs del sistema.

## Mejores Prácticas

1. **Validación de Entrada**: Validar toda la información proporcionada por los usuarios antes de procesarla.
2. **Principio de Privilegio Mínimo**: Asignar a los usuarios solo los permisos necesarios para realizar sus tareas.
3. **Auditoría Detallada**: Registrar todas las operaciones sensibles relacionadas con usuarios y roles.
4. **Rotación de Contraseñas**: Forzar cambios periódicos de contraseñas para usuarios con acceso a información sensible.
5. **Monitoreo de Actividad**: Implementar sistemas para detectar actividades sospechosas, como múltiples intentos fallidos de inicio de sesión.

## Solución de Problemas

### Problemas comunes

1. **Usuario bloqueado**
   - Verificar el número de intentos fallidos de inicio de sesión
   - Comprobar la fecha de finalización del bloqueo
   - Utilizar la función de desbloqueo si es necesario

2. **Problemas con roles y permisos**
   - Verificar que el rol exista y esté correctamente asignado al usuario
   - Comprobar que el rol tenga los permisos necesarios
   - Verificar que los permisos estén correctamente definidos

3. **Problemas con el cambio de contraseña**
   - Verificar que la contraseña actual sea correcta
   - Comprobar que la nueva contraseña cumpla con los requisitos de complejidad
   - Verificar que la nueva contraseña no esté en el historial de contraseñas

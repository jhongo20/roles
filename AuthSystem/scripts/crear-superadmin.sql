-- Script para crear un rol de Super Administrador con todos los permisos
-- Fecha: 10/05/2025

-- Variables para IDs
DECLARE @SuperAdminRoleId UNIQUEIDENTIFIER = NEWID();
DECLARE @AdminUserId UNIQUEIDENTIFIER = NEWID();

-- 1. Crear rol de Super Administrador
INSERT INTO Roles (Id, Name, NormalizedName, Description, IsActive, IsDefault, Priority, CreatedAt, UpdatedAt)
VALUES (@SuperAdminRoleId, 'Super Administrador', 'SUPER ADMINISTRADOR', 'Rol con acceso completo al sistema', 1, 0, 100, GETUTCDATE(), GETUTCDATE());

-- 2. Crear módulos principales
DECLARE @DashboardModuleId UNIQUEIDENTIFIER = NEWID();
DECLARE @UsersModuleId UNIQUEIDENTIFIER = NEWID();
DECLARE @RolesModuleId UNIQUEIDENTIFIER = NEWID();
DECLARE @PermissionsModuleId UNIQUEIDENTIFIER = NEWID();
DECLARE @AuditModuleId UNIQUEIDENTIFIER = NEWID();
DECLARE @ConfigModuleId UNIQUEIDENTIFIER = NEWID();

-- Módulo Dashboard
INSERT INTO Modules (Id, Name, Description, Icon, Route, IsActive, DisplayOrder, ParentId, CreatedAt, UpdatedAt)
VALUES (@DashboardModuleId, 'Dashboard', 'Panel principal del sistema', 'dashboard', '/dashboard', 1, 1, NULL, GETUTCDATE(), GETUTCDATE());

-- Módulo Usuarios
INSERT INTO Modules (Id, Name, Description, Icon, Route, IsActive, DisplayOrder, ParentId, CreatedAt, UpdatedAt)
VALUES (@UsersModuleId, 'Usuarios', 'Gestión de usuarios', 'people', '/users', 1, 2, NULL, GETUTCDATE(), GETUTCDATE());

-- Módulo Roles
INSERT INTO Modules (Id, Name, Description, Icon, Route, IsActive, DisplayOrder, ParentId, CreatedAt, UpdatedAt)
VALUES (@RolesModuleId, 'Roles', 'Gestión de roles', 'assignment', '/roles', 1, 3, NULL, GETUTCDATE(), GETUTCDATE());

-- Módulo Permisos
INSERT INTO Modules (Id, Name, Description, Icon, Route, IsActive, DisplayOrder, ParentId, CreatedAt, UpdatedAt)
VALUES (@PermissionsModuleId, 'Permisos', 'Gestión de permisos', 'security', '/permissions', 1, 4, NULL, GETUTCDATE(), GETUTCDATE());

-- Módulo Auditoría
INSERT INTO Modules (Id, Name, Description, Icon, Route, IsActive, DisplayOrder, ParentId, CreatedAt, UpdatedAt)
VALUES (@AuditModuleId, 'Auditoría', 'Logs de auditoría', 'history', '/audit', 1, 5, NULL, GETUTCDATE(), GETUTCDATE());

-- Módulo Configuración
INSERT INTO Modules (Id, Name, Description, Icon, Route, IsActive, DisplayOrder, ParentId, CreatedAt, UpdatedAt)
VALUES (@ConfigModuleId, 'Configuración', 'Configuración del sistema', 'settings', '/settings', 1, 6, NULL, GETUTCDATE(), GETUTCDATE());

-- 3. Crear permisos básicos
-- Permisos para Usuarios
DECLARE @ViewUsersPermId UNIQUEIDENTIFIER = NEWID();
DECLARE @CreateUserPermId UNIQUEIDENTIFIER = NEWID();
DECLARE @EditUserPermId UNIQUEIDENTIFIER = NEWID();
DECLARE @DeleteUserPermId UNIQUEIDENTIFIER = NEWID();
DECLARE @ResetPasswordPermId UNIQUEIDENTIFIER = NEWID();

-- Permisos para Roles
DECLARE @ViewRolesPermId UNIQUEIDENTIFIER = NEWID();
DECLARE @CreateRolePermId UNIQUEIDENTIFIER = NEWID();
DECLARE @EditRolePermId UNIQUEIDENTIFIER = NEWID();
DECLARE @DeleteRolePermId UNIQUEIDENTIFIER = NEWID();
DECLARE @AssignRolePermId UNIQUEIDENTIFIER = NEWID();

-- Permisos para Permisos
DECLARE @ViewPermissionsPermId UNIQUEIDENTIFIER = NEWID();
DECLARE @CreatePermissionPermId UNIQUEIDENTIFIER = NEWID();
DECLARE @EditPermissionPermId UNIQUEIDENTIFIER = NEWID();
DECLARE @DeletePermissionPermId UNIQUEIDENTIFIER = NEWID();
DECLARE @AssignPermissionPermId UNIQUEIDENTIFIER = NEWID();

-- Permisos para Auditoría
DECLARE @ViewAuditLogsPermId UNIQUEIDENTIFIER = NEWID();
DECLARE @ExportAuditLogsPermId UNIQUEIDENTIFIER = NEWID();

-- Permisos para Configuración
DECLARE @ViewConfigPermId UNIQUEIDENTIFIER = NEWID();
DECLARE @EditConfigPermId UNIQUEIDENTIFIER = NEWID();

-- Insertar permisos de Usuarios
INSERT INTO Permissions (Id, Name, Code, Description, Category, CreatedAt, UpdatedAt)
VALUES 
(@ViewUsersPermId, 'Ver Usuarios', 'USERS_VIEW', 'Permite ver la lista de usuarios', 'Usuarios', GETUTCDATE(), GETUTCDATE()),
(@CreateUserPermId, 'Crear Usuario', 'USERS_CREATE', 'Permite crear nuevos usuarios', 'Usuarios', GETUTCDATE(), GETUTCDATE()),
(@EditUserPermId, 'Editar Usuario', 'USERS_EDIT', 'Permite editar usuarios existentes', 'Usuarios', GETUTCDATE(), GETUTCDATE()),
(@DeleteUserPermId, 'Eliminar Usuario', 'USERS_DELETE', 'Permite eliminar usuarios', 'Usuarios', GETUTCDATE(), GETUTCDATE()),
(@ResetPasswordPermId, 'Resetear Contraseña', 'USERS_RESET_PASSWORD', 'Permite resetear contraseñas de usuarios', 'Usuarios', GETUTCDATE(), GETUTCDATE());

-- Insertar permisos de Roles
INSERT INTO Permissions (Id, Name, Code, Description, Category, CreatedAt, UpdatedAt)
VALUES 
(@ViewRolesPermId, 'Ver Roles', 'ROLES_VIEW', 'Permite ver la lista de roles', 'Roles', GETUTCDATE(), GETUTCDATE()),
(@CreateRolePermId, 'Crear Rol', 'ROLES_CREATE', 'Permite crear nuevos roles', 'Roles', GETUTCDATE(), GETUTCDATE()),
(@EditRolePermId, 'Editar Rol', 'ROLES_EDIT', 'Permite editar roles existentes', 'Roles', GETUTCDATE(), GETUTCDATE()),
(@DeleteRolePermId, 'Eliminar Rol', 'ROLES_DELETE', 'Permite eliminar roles', 'Roles', GETUTCDATE(), GETUTCDATE()),
(@AssignRolePermId, 'Asignar Rol', 'ROLES_ASSIGN', 'Permite asignar roles a usuarios', 'Roles', GETUTCDATE(), GETUTCDATE());

-- Insertar permisos de Permisos
INSERT INTO Permissions (Id, Name, Code, Description, Category, CreatedAt, UpdatedAt)
VALUES 
(@ViewPermissionsPermId, 'Ver Permisos', 'PERMISSIONS_VIEW', 'Permite ver la lista de permisos', 'Permisos', GETUTCDATE(), GETUTCDATE()),
(@CreatePermissionPermId, 'Crear Permiso', 'PERMISSIONS_CREATE', 'Permite crear nuevos permisos', 'Permisos', GETUTCDATE(), GETUTCDATE()),
(@EditPermissionPermId, 'Editar Permiso', 'PERMISSIONS_EDIT', 'Permite editar permisos existentes', 'Permisos', GETUTCDATE(), GETUTCDATE()),
(@DeletePermissionPermId, 'Eliminar Permiso', 'PERMISSIONS_DELETE', 'Permite eliminar permisos', 'Permisos', GETUTCDATE(), GETUTCDATE()),
(@AssignPermissionPermId, 'Asignar Permiso', 'PERMISSIONS_ASSIGN', 'Permite asignar permisos a roles', 'Permisos', GETUTCDATE(), GETUTCDATE());

-- Insertar permisos de Auditoría
INSERT INTO Permissions (Id, Name, Code, Description, Category, CreatedAt, UpdatedAt)
VALUES 
(@ViewAuditLogsPermId, 'Ver Logs de Auditoría', 'AUDIT_VIEW', 'Permite ver los logs de auditoría', 'Auditoría', GETUTCDATE(), GETUTCDATE()),
(@ExportAuditLogsPermId, 'Exportar Logs de Auditoría', 'AUDIT_EXPORT', 'Permite exportar los logs de auditoría', 'Auditoría', GETUTCDATE(), GETUTCDATE());

-- Insertar permisos de Configuración
INSERT INTO Permissions (Id, Name, Code, Description, Category, CreatedAt, UpdatedAt)
VALUES 
(@ViewConfigPermId, 'Ver Configuración', 'CONFIG_VIEW', 'Permite ver la configuración del sistema', 'Configuración', GETUTCDATE(), GETUTCDATE()),
(@EditConfigPermId, 'Editar Configuración', 'CONFIG_EDIT', 'Permite editar la configuración del sistema', 'Configuración', GETUTCDATE(), GETUTCDATE());

-- 4. Asignar permisos al rol de Super Administrador
-- Permisos de Usuarios
INSERT INTO RolePermissions (RoleId, PermissionId, AssignedBy, AssignedAt)
VALUES 
(@SuperAdminRoleId, @ViewUsersPermId, NULL, GETUTCDATE()),
(@SuperAdminRoleId, @CreateUserPermId, NULL, GETUTCDATE()),
(@SuperAdminRoleId, @EditUserPermId, NULL, GETUTCDATE()),
(@SuperAdminRoleId, @DeleteUserPermId, NULL, GETUTCDATE()),
(@SuperAdminRoleId, @ResetPasswordPermId, NULL, GETUTCDATE());

-- Permisos de Roles
INSERT INTO RolePermissions (RoleId, PermissionId, AssignedBy, AssignedAt)
VALUES 
(@SuperAdminRoleId, @ViewRolesPermId, NULL, GETUTCDATE()),
(@SuperAdminRoleId, @CreateRolePermId, NULL, GETUTCDATE()),
(@SuperAdminRoleId, @EditRolePermId, NULL, GETUTCDATE()),
(@SuperAdminRoleId, @DeleteRolePermId, NULL, GETUTCDATE()),
(@SuperAdminRoleId, @AssignRolePermId, NULL, GETUTCDATE());

-- Permisos de Permisos
INSERT INTO RolePermissions (RoleId, PermissionId, AssignedBy, AssignedAt)
VALUES 
(@SuperAdminRoleId, @ViewPermissionsPermId, NULL, GETUTCDATE()),
(@SuperAdminRoleId, @CreatePermissionPermId, NULL, GETUTCDATE()),
(@SuperAdminRoleId, @EditPermissionPermId, NULL, GETUTCDATE()),
(@SuperAdminRoleId, @DeletePermissionPermId, NULL, GETUTCDATE()),
(@SuperAdminRoleId, @AssignPermissionPermId, NULL, GETUTCDATE());

-- Permisos de Auditoría
INSERT INTO RolePermissions (RoleId, PermissionId, AssignedBy, AssignedAt)
VALUES 
(@SuperAdminRoleId, @ViewAuditLogsPermId, NULL, GETUTCDATE()),
(@SuperAdminRoleId, @ExportAuditLogsPermId, NULL, GETUTCDATE());

-- Permisos de Configuración
INSERT INTO RolePermissions (RoleId, PermissionId, AssignedBy, AssignedAt)
VALUES 
(@SuperAdminRoleId, @ViewConfigPermId, NULL, GETUTCDATE()),
(@SuperAdminRoleId, @EditConfigPermId, NULL, GETUTCDATE());

-- 5. Asignar permisos a los módulos
-- Módulo Usuarios
INSERT INTO ModulePermissions (ModuleId, PermissionId)
VALUES 
(@UsersModuleId, @ViewUsersPermId),
(@UsersModuleId, @CreateUserPermId),
(@UsersModuleId, @EditUserPermId),
(@UsersModuleId, @DeleteUserPermId),
(@UsersModuleId, @ResetPasswordPermId);

-- Módulo Roles
INSERT INTO ModulePermissions (ModuleId, PermissionId)
VALUES 
(@RolesModuleId, @ViewRolesPermId),
(@RolesModuleId, @CreateRolePermId),
(@RolesModuleId, @EditRolePermId),
(@RolesModuleId, @DeleteRolePermId),
(@RolesModuleId, @AssignRolePermId);

-- Módulo Permisos
INSERT INTO ModulePermissions (ModuleId, PermissionId)
VALUES 
(@PermissionsModuleId, @ViewPermissionsPermId),
(@PermissionsModuleId, @CreatePermissionPermId),
(@PermissionsModuleId, @EditPermissionPermId),
(@PermissionsModuleId, @DeletePermissionPermId),
(@PermissionsModuleId, @AssignPermissionPermId);

-- Módulo Auditoría
INSERT INTO ModulePermissions (ModuleId, PermissionId)
VALUES 
(@AuditModuleId, @ViewAuditLogsPermId),
(@AuditModuleId, @ExportAuditLogsPermId);

-- Módulo Configuración
INSERT INTO ModulePermissions (ModuleId, PermissionId)
VALUES 
(@ConfigModuleId, @ViewConfigPermId),
(@ConfigModuleId, @EditConfigPermId);

-- 6. Crear usuario administrador
-- Nota: La contraseña debe ser hasheada correctamente. Este es un ejemplo y debe ser reemplazado con un hash real.
-- La contraseña en este ejemplo es 'Admin123!' pero debe ser hasheada con BCrypt o el algoritmo que uses en tu aplicación
DECLARE @PasswordHash NVARCHAR(MAX) = '$2a$11$K3lZSd2qTGbRHXKBgAJXR.TXa/F5TfYoqwNlE.j8J.k9GEfEWlK7G'; -- Esto es solo un ejemplo
DECLARE @SecurityStamp NVARCHAR(MAX) = NEWID(); -- Generar un security stamp

INSERT INTO Users (Id, Username, Email, PasswordHash, SecurityStamp, FirstName, LastName, PhoneNumber, Status, EmailConfirmed, 
                  PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount, LastLoginDate, 
                  CreatedAt, UpdatedAt, RequirePasswordChange, ProfilePictureUrl, IsDeleted, NormalizedEmail)
VALUES (@AdminUserId, 'admin', 'admin@authsystem.com', @PasswordHash, @SecurityStamp, 'Administrador', 'Sistema', '', 1, 1, 
        0, 0, NULL, 0, 0, NULL, GETUTCDATE(), GETUTCDATE(), 0, '', 0, 'ADMIN@AUTHSYSTEM.COM');

-- 7. Asignar rol de Super Administrador al usuario administrador
INSERT INTO UserRoles (UserId, RoleId, AssignedBy, AssignedAt, ExpirationDate, IsActive)
VALUES (@AdminUserId, @SuperAdminRoleId, NULL, GETUTCDATE(), NULL, 1);

-- Mensaje de confirmación
PRINT 'Se ha creado el rol de Super Administrador con todos los permisos necesarios.';
PRINT 'Se ha creado un usuario administrador con el nombre de usuario "admin".';
PRINT 'Recuerda cambiar la contraseña del usuario administrador después de iniciar sesión por primera vez.';

# Guía de Integración Frontend para AuthSystem

## Introducción

Esta guía proporciona instrucciones detalladas para que los desarrolladores frontend puedan consumir correctamente los endpoints de la API de AuthSystem. Incluye ejemplos de código en Angular para cada operación principal.

## Configuración Inicial

### 1. Configuración del Servicio HTTP

Primero, crea un servicio base para manejar las solicitudes HTTP:

```typescript
// api.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  get<T>(endpoint: string, params?: any): Observable<T> {
    let httpParams = new HttpParams();
    if (params) {
      Object.keys(params).forEach(key => {
        if (params[key] !== null && params[key] !== undefined) {
          httpParams = httpParams.set(key, params[key]);
        }
      });
    }
    return this.http.get<T>(`${this.baseUrl}/${endpoint}`, { params: httpParams });
  }

  post<T>(endpoint: string, data: any): Observable<T> {
    return this.http.post<T>(`${this.baseUrl}/${endpoint}`, data);
  }

  put<T>(endpoint: string, data: any): Observable<T> {
    return this.http.put<T>(`${this.baseUrl}/${endpoint}`, data);
  }

  delete<T>(endpoint: string): Observable<T> {
    return this.http.delete<T>(`${this.baseUrl}/${endpoint}`);
  }

  patch<T>(endpoint: string, data: any): Observable<T> {
    return this.http.patch<T>(`${this.baseUrl}/${endpoint}`, data);
  }
}
```

### 2. Configuración del Interceptor para Tokens

Crea un interceptor para añadir automáticamente el token de autenticación a las solicitudes:

```typescript
// auth.interceptor.ts
import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private authService: AuthService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    const token = this.authService.getToken();
    if (token) {
      request = request.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
    }
    return next.handle(request);
  }
}
```

### 3. Configuración en el Módulo Principal

Registra el interceptor en tu módulo principal:

```typescript
// app.module.ts
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { AuthInterceptor } from './auth.interceptor';

@NgModule({
  // ...
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true }
  ],
  // ...
})
export class AppModule { }
```

## Servicios para Consumir la API

### 1. Servicio para Módulos

```typescript
// module.service.ts
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface Module {
  id?: string;
  name: string;
  code: string;
  description?: string;
  icon?: string;
  route?: string;
  displayOrder: number;
  isActive: boolean;
  parentId?: string;
  parentModuleName?: string;
  createdAt?: Date;
  updatedAt?: Date;
  childModules?: Module[];
  permissions?: Permission[];
}

export interface Permission {
  id?: string;
  name: string;
  code: string;
  description?: string;
  category?: string;
  isActive: boolean;
  createdAt?: Date;
  updatedAt?: Date;
}

export interface ModuleResponse {
  id: string;
  name: string;
  code: string;
  description?: string;
  icon?: string;
  route?: string;
  displayOrder: number;
  isActive: boolean;
  parentId?: string;
  createdAt: Date;
  updatedAt?: Date;
  success: boolean;
  message: string;
}

export interface ModulePermissionResponse {
  moduleId: string;
  moduleName: string;
  permissionId: string;
  permissionName: string;
  permissionCode: string;
  success: boolean;
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class ModuleService {
  constructor(private apiService: ApiService) { }

  getAllModules(includeInactive: boolean = false, includePermissions: boolean = false): Observable<Module[]> {
    return this.apiService.get<Module[]>('modules', { includeInactive, includePermissions });
  }

  getModuleById(id: string, includePermissions: boolean = false): Observable<Module> {
    return this.apiService.get<Module>(`modules/${id}`, { includePermissions });
  }

  getModulePermissions(id: string): Observable<Permission[]> {
    return this.apiService.get<Permission[]>(`modules/${id}/permissions`);
  }

  createModule(module: Module): Observable<ModuleResponse> {
    return this.apiService.post<ModuleResponse>('modules', module);
  }

  updateModule(id: string, module: Module): Observable<ModuleResponse> {
    return this.apiService.put<ModuleResponse>(`modules/${id}`, module);
  }

  deleteModule(id: string): Observable<ModuleResponse> {
    return this.apiService.delete<ModuleResponse>(`modules/${id}`);
  }

  addPermissionToModule(moduleId: string, permissionId: string): Observable<ModulePermissionResponse> {
    return this.apiService.post<ModulePermissionResponse>(`modules/${moduleId}/permissions/${permissionId}`, {});
  }

  removePermissionFromModule(moduleId: string, permissionId: string): Observable<ModulePermissionResponse> {
    return this.apiService.delete<ModulePermissionResponse>(`modules/${moduleId}/permissions/${permissionId}`);
  }
}
```

### 2. Servicio para Gestión de Usuarios

```typescript
// user-management.service.ts
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface User {
  id?: string;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  isActive: boolean;
  isEmailConfirmed?: boolean;
  isLocked?: boolean;
  lockoutEnd?: Date;
  lastLoginAt?: Date;
  createdAt?: Date;
  updatedAt?: Date;
}

export interface UserResponse {
  id: string;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  isActive: boolean;
  success: boolean;
  message: string;
}

export enum UserStatus {
  Active = 'Active',
  Suspended = 'Suspended',
  Locked = 'Locked'
}

export interface UserStatusRequest {
  status: UserStatus;
  reason?: string;
  lockoutEnd?: Date;
}

export interface PasswordResetRequest {
  newPassword: string;
  requirePasswordChange: boolean;
  sendPasswordResetEmail: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class UserManagementService {
  constructor(private apiService: ApiService) { }

  getAllUsers(includeInactive: boolean = false, searchTerm?: string, pageNumber?: number, pageSize?: number): Observable<User[]> {
    return this.apiService.get<User[]>('user-management', { 
      includeInactive, 
      searchTerm, 
      pageNumber, 
      pageSize 
    });
  }

  getUserById(id: string): Observable<User> {
    return this.apiService.get<User>(`user-management/${id}`);
  }

  createUser(user: User & { password: string, sendActivationEmail?: boolean, requirePasswordChange?: boolean }): Observable<UserResponse> {
    return this.apiService.post<UserResponse>('user-management', user);
  }

  updateUser(id: string, user: Pick<User, 'id' | 'firstName' | 'lastName' | 'phoneNumber' | 'isActive'>): Observable<UserResponse> {
    return this.apiService.put<UserResponse>(`user-management/${id}`, user);
  }

  changeUserStatus(id: string, statusRequest: UserStatusRequest): Observable<UserResponse> {
    return this.apiService.patch<UserResponse>(`user-management/${id}/status`, statusRequest);
  }

  resetUserPassword(id: string, resetRequest: PasswordResetRequest): Observable<UserResponse> {
    return this.apiService.post<UserResponse>(`user-management/${id}/reset-password`, resetRequest);
  }

  resendActivationEmail(id: string): Observable<UserResponse> {
    return this.apiService.post<UserResponse>(`user-management/${id}/resend-activation`, {});
  }
}
```

### 3. Servicio para Roles de Usuario

```typescript
// user-role.service.ts
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface Role {
  id?: string;
  name: string;
  description?: string;
  isActive: boolean;
  createdAt?: Date;
  updatedAt?: Date;
}

export interface UserRoleResponse {
  userId: string;
  userName: string;
  roleId: string;
  roleName: string;
  assignedAt?: Date;
  assignedBy?: string;
  assignedByName?: string;
  success: boolean;
  message: string;
}

export interface UserRoleAssignment {
  userId: string;
  roleId: string;
  assignedBy?: string;
}

@Injectable({
  providedIn: 'root'
})
export class UserRoleService {
  constructor(private apiService: ApiService) { }

  getUserRoles(userId: string): Observable<Role[]> {
    return this.apiService.get<Role[]>(`user-roles/${userId}`);
  }

  assignRoleToUser(assignment: UserRoleAssignment): Observable<UserRoleResponse> {
    return this.apiService.post<UserRoleResponse>('user-roles', assignment);
  }

  removeRoleFromUser(userId: string, roleId: string): Observable<UserRoleResponse> {
    return this.apiService.delete<UserRoleResponse>(`user-roles/${userId}/${roleId}`);
  }
}
```

### 4. Servicio para Permisos

```typescript
// permission.service.ts
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Permission } from './module.service';

export interface PermissionResponse {
  id: string;
  name: string;
  code: string;
  description?: string;
  category?: string;
  isActive: boolean;
  createdAt: Date;
  updatedAt?: Date;
  success: boolean;
  message: string;
}

export interface PermissionsByCategory {
  [category: string]: Permission[];
}

@Injectable({
  providedIn: 'root'
})
export class PermissionService {
  constructor(private apiService: ApiService) { }

  getAllPermissions(includeInactive: boolean = false, category?: string): Observable<Permission[]> {
    return this.apiService.get<Permission[]>('permissions', { includeInactive, category });
  }

  getPermissionById(id: string): Observable<Permission> {
    return this.apiService.get<Permission>(`permissions/${id}`);
  }

  getPermissionsByCategory(): Observable<PermissionsByCategory> {
    return this.apiService.get<PermissionsByCategory>('permissions/categories');
  }

  createPermission(permission: Permission): Observable<PermissionResponse> {
    return this.apiService.post<PermissionResponse>('permissions', permission);
  }

  updatePermission(id: string, permission: Permission): Observable<PermissionResponse> {
    return this.apiService.put<PermissionResponse>(`permissions/${id}`, permission);
  }

  deletePermission(id: string): Observable<PermissionResponse> {
    return this.apiService.delete<PermissionResponse>(`permissions/${id}`);
  }
}
```

## Ejemplos de Uso en Componentes Angular

### 1. Listar Módulos

```typescript
// modules-list.component.ts
import { Component, OnInit } from '@angular/core';
import { Module, ModuleService } from '../services/module.service';

@Component({
  selector: 'app-modules-list',
  templateUrl: './modules-list.component.html',
  styleUrls: ['./modules-list.component.scss']
})
export class ModulesListComponent implements OnInit {
  modules: Module[] = [];
  loading = false;
  error: string | null = null;
  includeInactive = false;
  includePermissions = false;

  constructor(private moduleService: ModuleService) { }

  ngOnInit(): void {
    this.loadModules();
  }

  loadModules(): void {
    this.loading = true;
    this.error = null;
    this.moduleService.getAllModules(this.includeInactive, this.includePermissions)
      .subscribe({
        next: (data) => {
          this.modules = data;
          this.loading = false;
        },
        error: (err) => {
          this.error = err.message || 'Error al cargar los módulos';
          this.loading = false;
        }
      });
  }

  toggleFilters(): void {
    this.loadModules();
  }
}
```

```html
<!-- modules-list.component.html -->
<div class="container">
  <h2>Módulos del Sistema</h2>
  
  <div class="filters">
    <mat-checkbox [(ngModel)]="includeInactive" (change)="toggleFilters()">
      Incluir inactivos
    </mat-checkbox>
    <mat-checkbox [(ngModel)]="includePermissions" (change)="toggleFilters()">
      Incluir permisos
    </mat-checkbox>
  </div>
  
  <div *ngIf="loading" class="loading">
    <mat-spinner diameter="40"></mat-spinner>
    <span>Cargando módulos...</span>
  </div>
  
  <div *ngIf="error" class="error-message">
    {{ error }}
  </div>
  
  <mat-tree *ngIf="!loading && !error && modules.length > 0">
    <!-- Implementación del árbol de módulos -->
  </mat-tree>
  
  <div *ngIf="!loading && !error && modules.length === 0" class="no-data">
    No se encontraron módulos.
  </div>
</div>
```

### 2. Crear un Nuevo Módulo

```typescript
// module-create.component.ts
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Module, ModuleService } from '../services/module.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-module-create',
  templateUrl: './module-create.component.html',
  styleUrls: ['./module-create.component.scss']
})
export class ModuleCreateComponent {
  moduleForm: FormGroup;
  parentModules: Module[] = [];
  submitting = false;
  error: string | null = null;

  constructor(
    private fb: FormBuilder,
    private moduleService: ModuleService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {
    this.moduleForm = this.fb.group({
      name: ['', [Validators.required]],
      code: ['', [Validators.required]],
      description: [''],
      icon: [''],
      route: [''],
      displayOrder: [0, [Validators.required, Validators.min(0)]],
      parentId: [null],
      isActive: [true]
    });
    
    this.loadParentModules();
  }

  loadParentModules(): void {
    this.moduleService.getAllModules(false, false)
      .subscribe({
        next: (modules) => {
          this.parentModules = modules;
        },
        error: (err) => {
          this.error = 'Error al cargar los módulos padre';
          console.error(err);
        }
      });
  }

  onSubmit(): void {
    if (this.moduleForm.invalid) {
      return;
    }

    this.submitting = true;
    this.error = null;
    
    const moduleData: Module = this.moduleForm.value;
    
    this.moduleService.createModule(moduleData)
      .subscribe({
        next: (response) => {
          this.submitting = false;
          this.snackBar.open(response.message, 'Cerrar', { duration: 3000 });
          this.router.navigate(['/modules']);
        },
        error: (err) => {
          this.submitting = false;
          this.error = err.error || 'Error al crear el módulo';
          this.snackBar.open(this.error, 'Cerrar', { duration: 5000 });
        }
      });
  }
}
```

```html
<!-- module-create.component.html -->
<div class="container">
  <h2>Crear Nuevo Módulo</h2>
  
  <form [formGroup]="moduleForm" (ngSubmit)="onSubmit()">
    <mat-form-field appearance="outline">
      <mat-label>Nombre</mat-label>
      <input matInput formControlName="name" required>
      <mat-error *ngIf="moduleForm.get('name')?.hasError('required')">
        El nombre es obligatorio
      </mat-error>
    </mat-form-field>
    
    <mat-form-field appearance="outline">
      <mat-label>Código</mat-label>
      <input matInput formControlName="code" required>
      <mat-error *ngIf="moduleForm.get('code')?.hasError('required')">
        El código es obligatorio
      </mat-error>
    </mat-form-field>
    
    <mat-form-field appearance="outline">
      <mat-label>Descripción</mat-label>
      <textarea matInput formControlName="description" rows="3"></textarea>
    </mat-form-field>
    
    <mat-form-field appearance="outline">
      <mat-label>Icono</mat-label>
      <input matInput formControlName="icon">
    </mat-form-field>
    
    <mat-form-field appearance="outline">
      <mat-label>Ruta</mat-label>
      <input matInput formControlName="route">
    </mat-form-field>
    
    <mat-form-field appearance="outline">
      <mat-label>Orden</mat-label>
      <input matInput type="number" formControlName="displayOrder" required>
      <mat-error *ngIf="moduleForm.get('displayOrder')?.hasError('min')">
        El orden debe ser mayor o igual a 0
      </mat-error>
    </mat-form-field>
    
    <mat-form-field appearance="outline">
      <mat-label>Módulo Padre</mat-label>
      <mat-select formControlName="parentId">
        <mat-option [value]="null">Ninguno</mat-option>
        <mat-option *ngFor="let module of parentModules" [value]="module.id">
          {{ module.name }}
        </mat-option>
      </mat-select>
    </mat-form-field>
    
    <mat-checkbox formControlName="isActive">Activo</mat-checkbox>
    
    <div class="error-message" *ngIf="error">{{ error }}</div>
    
    <div class="actions">
      <button mat-button type="button" routerLink="/modules">Cancelar</button>
      <button mat-raised-button color="primary" type="submit" [disabled]="moduleForm.invalid || submitting">
        <mat-spinner diameter="20" *ngIf="submitting"></mat-spinner>
        <span *ngIf="!submitting">Guardar</span>
      </button>
    </div>
  </form>
</div>
```

### 3. Asignar Roles a Usuarios

```typescript
// user-roles.component.ts
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Role, UserRoleService } from '../services/user-role.service';
import { UserManagementService, User } from '../services/user-management.service';

@Component({
  selector: 'app-user-roles',
  templateUrl: './user-roles.component.html',
  styleUrls: ['./user-roles.component.scss']
})
export class UserRolesComponent implements OnInit {
  userId: string;
  user: User | null = null;
  userRoles: Role[] = [];
  availableRoles: Role[] = [];
  loading = false;
  error: string | null = null;
  
  constructor(
    private route: ActivatedRoute,
    private userRoleService: UserRoleService,
    private userService: UserManagementService,
    private snackBar: MatSnackBar
  ) {
    this.userId = this.route.snapshot.paramMap.get('id') || '';
  }
  
  ngOnInit(): void {
    this.loadUser();
    this.loadUserRoles();
  }
  
  loadUser(): void {
    this.userService.getUserById(this.userId)
      .subscribe({
        next: (user) => {
          this.user = user;
        },
        error: (err) => {
          this.error = 'Error al cargar el usuario';
          console.error(err);
        }
      });
  }
  
  loadUserRoles(): void {
    this.loading = true;
    this.userRoleService.getUserRoles(this.userId)
      .subscribe({
        next: (roles) => {
          this.userRoles = roles;
          this.loading = false;
          this.loadAvailableRoles();
        },
        error: (err) => {
          this.error = 'Error al cargar los roles del usuario';
          this.loading = false;
          console.error(err);
        }
      });
  }
  
  loadAvailableRoles(): void {
    // Aquí deberías tener un servicio para obtener todos los roles
    // Por ahora, simulamos algunos roles
    const allRoles: Role[] = [
      { id: '1', name: 'Administrador', isActive: true },
      { id: '2', name: 'Editor', isActive: true },
      { id: '3', name: 'Lector', isActive: true }
    ];
    
    // Filtrar roles que el usuario ya tiene
    this.availableRoles = allRoles.filter(role => 
      !this.userRoles.some(userRole => userRole.id === role.id)
    );
  }
  
  assignRole(roleId: string): void {
    this.userRoleService.assignRoleToUser({
      userId: this.userId,
      roleId: roleId
    }).subscribe({
      next: (response) => {
        this.snackBar.open(response.message, 'Cerrar', { duration: 3000 });
        this.loadUserRoles(); // Recargar roles
      },
      error: (err) => {
        this.snackBar.open(err.error || 'Error al asignar el rol', 'Cerrar', { duration: 5000 });
      }
    });
  }
  
  removeRole(roleId: string): void {
    this.userRoleService.removeRoleFromUser(this.userId, roleId)
      .subscribe({
        next: (response) => {
          this.snackBar.open(response.message, 'Cerrar', { duration: 3000 });
          this.loadUserRoles(); // Recargar roles
        },
        error: (err) => {
          this.snackBar.open(err.error || 'Error al quitar el rol', 'Cerrar', { duration: 5000 });
        }
      });
  }
}
```

```html
<!-- user-roles.component.html -->
<div class="container">
  <h2 *ngIf="user">Roles de Usuario: {{ user.firstName }} {{ user.lastName }}</h2>
  
  <div *ngIf="loading" class="loading">
    <mat-spinner diameter="40"></mat-spinner>
    <span>Cargando roles...</span>
  </div>
  
  <div *ngIf="error" class="error-message">
    {{ error }}
  </div>
  
  <div class="roles-container" *ngIf="!loading && !error">
    <div class="assigned-roles">
      <h3>Roles Asignados</h3>
      <mat-list>
        <mat-list-item *ngFor="let role of userRoles">
          <span>{{ role.name }}</span>
          <button mat-icon-button color="warn" (click)="removeRole(role.id!)">
            <mat-icon>delete</mat-icon>
          </button>
        </mat-list-item>
        <mat-list-item *ngIf="userRoles.length === 0">
          <span>No hay roles asignados</span>
        </mat-list-item>
      </mat-list>
    </div>
    
    <div class="available-roles">
      <h3>Roles Disponibles</h3>
      <mat-list>
        <mat-list-item *ngFor="let role of availableRoles">
          <span>{{ role.name }}</span>
          <button mat-icon-button color="primary" (click)="assignRole(role.id!)">
            <mat-icon>add</mat-icon>
          </button>
        </mat-list-item>
        <mat-list-item *ngIf="availableRoles.length === 0">
          <span>No hay roles disponibles</span>
        </mat-list-item>
      </mat-list>
    </div>
  </div>
  
  <div class="actions">
    <button mat-button routerLink="/users">Volver a la lista de usuarios</button>
  </div>
</div>
```

## Manejo de Errores

Para manejar errores de manera consistente, puedes crear un interceptor de errores:

```typescript
// error.interceptor.ts
import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AuthService } from './auth.service';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  constructor(
    private snackBar: MatSnackBar,
    private authService: AuthService
  ) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        let errorMessage = 'Ha ocurrido un error desconocido';
        
        if (error.error instanceof ErrorEvent) {
          // Error del lado del cliente
          errorMessage = `Error: ${error.error.message}`;
        } else {
          // Error del lado del servidor
          switch (error.status) {
            case 400:
              errorMessage = error.error || 'Solicitud incorrecta';
              break;
            case 401:
              errorMessage = 'No autorizado. Por favor, inicie sesión nuevamente.';
              this.authService.logout();
              break;
            case 403:
              errorMessage = 'No tiene permisos para realizar esta acción';
              break;
            case 404:
              errorMessage = error.error || 'Recurso no encontrado';
              break;
            case 500:
              errorMessage = 'Error interno del servidor';
              break;
            default:
              errorMessage = `Error ${error.status}: ${error.error || error.statusText}`;
          }
        }
        
        // Mostrar mensaje de error
        this.snackBar.open(errorMessage, 'Cerrar', {
          duration: 5000,
          panelClass: ['error-snackbar']
        });
        
        return throwError(() => new Error(errorMessage));
      })
    );
  }
}
```

## Conclusión

Esta guía proporciona una base sólida para integrar una aplicación frontend en Angular con la API de AuthSystem. Siguiendo estos ejemplos, los desarrolladores pueden implementar rápidamente la funcionalidad necesaria para gestionar módulos, usuarios, roles y permisos.

Recuerda que es importante manejar adecuadamente los errores y proporcionar retroalimentación clara al usuario cuando las operaciones fallan. También es crucial implementar un sistema de autenticación robusto y asegurarse de que solo los usuarios con los permisos adecuados puedan acceder a ciertas funcionalidades.

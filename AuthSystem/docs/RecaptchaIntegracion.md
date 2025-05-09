# Integración de Google reCAPTCHA v3

## Introducción

Este documento describe la implementación y configuración de Google reCAPTCHA v3 en el sistema AuthSystem. reCAPTCHA es una herramienta de seguridad que ayuda a proteger el sistema contra bots y spam, evaluando el comportamiento de los usuarios y asignando una puntuación de riesgo.

## Actualizaciones Recientes

- Se ha creado un controlador dedicado para reCAPTCHA (`RecaptchaController`) con endpoints específicos para verificación y registro.
- Se ha ampliado la interfaz `IRecaptchaService` para incluir un método que proporciona la configuración pública.
- Se ha agregado una guía detallada para la implementación en aplicaciones frontend Angular.

## Componentes de la Implementación

La implementación de reCAPTCHA en AuthSystem consta de los siguientes componentes:

### 1. Interfaz del Servicio

```csharp
// AuthSystem.Core/Interfaces/IRecaptchaService.cs
public interface IRecaptchaService
{
    Task<bool> ValidateTokenAsync(string token, string ipAddress);
    RecaptchaPublicConfig GetPublicConfig();
}

public class RecaptchaPublicConfig
{
    public string SiteKey { get; set; }
}
```

Esta interfaz define el contrato para validar tokens de reCAPTCHA y obtener la configuración pública (clave del sitio) que puede ser compartida con el cliente.

### 2. Implementación del Servicio

```csharp
// AuthSystem.Infrastructure/Security/RecaptchaService.cs
public class RecaptchaService : IRecaptchaService
{
    private readonly HttpClient _httpClient;
    private readonly RecaptchaSettings _settings;

    public RecaptchaService(HttpClient httpClient, IOptions<RecaptchaSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task<bool> ValidateTokenAsync(string token, string ipAddress)
    {
        if (string.IsNullOrEmpty(token))
        {
            return false;
        }

        var response = await _httpClient.GetStringAsync(
            $"https://www.google.com/recaptcha/api/siteverify?secret={_settings.SecretKey}&response={token}&remoteip={ipAddress}");

        var recaptchaResponse = JsonSerializer.Deserialize<RecaptchaResponse>(response);

        return recaptchaResponse?.Success == true && recaptchaResponse.Score >= _settings.MinimumScore;
    }
    
    public RecaptchaPublicConfig GetPublicConfig()
    {
        return new RecaptchaPublicConfig
        {
            SiteKey = _settings.SiteKey
        };
    }

    private class RecaptchaResponse
    {
        public bool Success { get; set; }
        public float Score { get; set; }
        public string Action { get; set; }
        public string Hostname { get; set; }
    }
}
```

Esta clase implementa la interfaz `IRecaptchaService` y utiliza la API de Google para validar los tokens de reCAPTCHA.

### 3. Modelo de Configuración

```csharp
public class RecaptchaSettings
{
    public string SiteKey { get; set; }
    public string SecretKey { get; set; }
    public float MinimumScore { get; set; } = 0.5f;
}
```

Este modelo define la configuración necesaria para reCAPTCHA:
- `SiteKey`: Clave del sitio proporcionada por Google (para el lado del cliente).
- `SecretKey`: Clave secreta proporcionada por Google (para el lado del servidor).
- `MinimumScore`: Puntuación mínima requerida para considerar válida la interacción (0.0 a 1.0).

### 4. Registro del Servicio

El servicio se registra en el contenedor de dependencias en `DependencyInjection.cs`:

```csharp
// AuthSystem.Infrastructure/DependencyInjection.cs
public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
{
    // Otras configuraciones...

    // Configuración de reCAPTCHA
    services.Configure<RecaptchaSettings>(configuration.GetSection("RecaptchaSettings"));
    services.AddHttpClient<IRecaptchaService, RecaptchaService>();

    // Otras configuraciones...

    return services;
}
```

### 5. Uso en Comandos

El servicio se utiliza en los comandos de autenticación y registro:

```csharp
// AuthSystem.Application/Commands/AuthenticateCommand.cs
public async Task<AuthResponseDto> Handle(AuthenticateCommand request, CancellationToken cancellationToken)
{
    try
    {
        // 1. Verificar reCAPTCHA si se proporciona un token
        if (!string.IsNullOrEmpty(request.RecaptchaToken))
        {
            var isValidRecaptcha = await _recaptchaService.ValidateTokenAsync(
                request.RecaptchaToken, request.IpAddress);

            if (!isValidRecaptcha)
            {
                await LogFailedLoginAttempt(request, null, "reCAPTCHA inválido");
                return new AuthResponseDto
                {
                    Succeeded = false,
                    Error = "Verificación de reCAPTCHA fallida"
                };
            }
        }

        // Resto de la lógica de autenticación...
    }
    catch (Exception ex)
    {
        // Manejo de excepciones...
    }
}
```

## Configuración

### Configuración en appsettings.json

La configuración de reCAPTCHA se define en el archivo `appsettings.json`:

```json
"RecaptchaSettings": {
  "SiteKey": "your-recaptcha-site-key",
  "SecretKey": "your-recaptcha-secret-key",
  "MinimumScore": 0.5
}
```

### Obtención de Claves de reCAPTCHA

Para obtener las claves de reCAPTCHA, siga estos pasos:

1. Vaya a la [Consola de Administración de reCAPTCHA](https://www.google.com/recaptcha/admin).
2. Inicie sesión con su cuenta de Google.
3. Haga clic en "+" para crear un nuevo sitio.
4. Seleccione "reCAPTCHA v3" como tipo de reCAPTCHA.
5. Ingrese el dominio de su sitio (por ejemplo, `localhost` para desarrollo local).
6. Acepte los términos de servicio y haga clic en "Enviar".
7. Copie las claves "Clave del sitio" y "Clave secreta" y configúrelas en `appsettings.json`.

## Controlador de reCAPTCHA

Se ha implementado un controlador dedicado para gestionar las operaciones relacionadas con reCAPTCHA:

```csharp
// AuthSystem.API/Controllers/RecaptchaController.cs
[ApiController]
[Route("api/[controller]")]
public class RecaptchaController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IRecaptchaService _recaptchaService;
    private readonly ILogger<RecaptchaController> _logger;

    public RecaptchaController(
        IMediator mediator,
        IRecaptchaService recaptchaService,
        ILogger<RecaptchaController> logger)
    {
        _mediator = mediator;
        _recaptchaService = recaptchaService;
        _logger = logger;
    }

    // Verifica un token de reCAPTCHA
    [HttpPost("verify")]
    [AllowAnonymous]
    public async Task<ActionResult<RecaptchaVerificationResponse>> VerifyToken([FromBody] RecaptchaVerificationRequest request)
    {
        // Implementación de verificación de token
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var isValid = await _recaptchaService.ValidateTokenAsync(request.Token, ipAddress);

        return Ok(new RecaptchaVerificationResponse
        {
            Success = isValid,
            Message = isValid 
                ? "Verificación exitosa" 
                : "Verificación fallida. Por favor, intente nuevamente."
        });
    }

    // Registra un nuevo usuario con validación de reCAPTCHA
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto request)
    {
        // Implementación de registro con validación de reCAPTCHA
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var isValidRecaptcha = await _recaptchaService.ValidateTokenAsync(request.RecaptchaToken, ipAddress);

        if (!isValidRecaptcha)
        {
            return BadRequest(new AuthResponseDto
            {
                Succeeded = false,
                Error = "Verificación de reCAPTCHA fallida. Por favor, intente nuevamente."
            });
        }

        // Procesar el registro si el reCAPTCHA es válido
        var command = new RegisterCommand
        {
            // Mapeo de propiedades
            RecaptchaToken = request.RecaptchaToken,
            IpAddress = ipAddress
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    // Obtiene la configuración pública de reCAPTCHA (solo la clave del sitio)
    [HttpGet("config")]
    [AllowAnonymous]
    public ActionResult<RecaptchaConfigResponse> GetConfig()
    {
        // Obtener la configuración del servicio
        var config = _recaptchaService.GetPublicConfig();
        
        return Ok(new RecaptchaConfigResponse
        {
            SiteKey = config.SiteKey
        });
    }
}
```

### Endpoints Disponibles

| Endpoint | Método | Descripción |
|----------|--------|-------------|
| `/api/recaptcha/verify` | POST | Verifica un token de reCAPTCHA |
| `/api/recaptcha/register` | POST | Registra un nuevo usuario con validación de reCAPTCHA |
| `/api/recaptcha/config` | GET | Obtiene la configuración pública de reCAPTCHA |

## Implementación en el Cliente

### Implementación en JavaScript Vanilla

#### 1. Agregar el Script de reCAPTCHA

Agregue el script de reCAPTCHA a su página HTML:

```html
<script src="https://www.google.com/recaptcha/api.js?render=YOUR_SITE_KEY"></script>
```

#### 2. Generar un Token al Enviar un Formulario

```javascript
// Función para obtener un token de reCAPTCHA
function getRecaptchaToken(action) {
    return new Promise((resolve, reject) => {
        grecaptcha.ready(() => {
            grecaptcha.execute('YOUR_SITE_KEY', { action: action })
                .then(token => resolve(token))
                .catch(error => reject(error));
        });
    });
}

// Ejemplo de uso en un formulario de inicio de sesión
document.getElementById('loginForm').addEventListener('submit', async function(event) {
    event.preventDefault();
    
    try {
        // Obtener token de reCAPTCHA
        const recaptchaToken = await getRecaptchaToken('login');
        
        // Agregar el token al objeto de datos
        const formData = {
            username: document.getElementById('username').value,
            password: document.getElementById('password').value,
            recaptchaToken: recaptchaToken
        };
        
        // Enviar datos al servidor
        const response = await fetch('/api/auth/login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(formData)
        });
        
        const result = await response.json();
        
        if (result.succeeded) {
            // Manejar inicio de sesión exitoso
            window.location.href = '/dashboard';
        } else {
            // Manejar error
            document.getElementById('errorMessage').textContent = result.error;
        }
    } catch (error) {
        console.error('Error:', error);
    }
});
```

### Implementación en Angular

A continuación se detalla cómo implementar reCAPTCHA v3 en una aplicación Angular:

#### 1. Instalar el paquete de reCAPTCHA para Angular

```bash
ng add @angular/forms
npm install ng-recaptcha --save
```

#### 2. Configurar el Módulo de reCAPTCHA

En tu `app.module.ts`:

```typescript
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RECAPTCHA_V3_SITE_KEY, RecaptchaV3Module } from 'ng-recaptcha';

import { AppComponent } from './app.component';
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    RegisterComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    ReactiveFormsModule,
    HttpClientModule,
    RecaptchaV3Module
  ],
  providers: [
    { provide: RECAPTCHA_V3_SITE_KEY, useValue: 'your-recaptcha-site-key' }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
```

#### 3. Crear un Servicio para reCAPTCHA

```typescript
// recaptcha.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ReCaptchaV3Service } from 'ng-recaptcha';

@Injectable({
  providedIn: 'root'
})
export class RecaptchaService {
  constructor(
    private http: HttpClient,
    private recaptchaV3Service: ReCaptchaV3Service
  ) {}

  // Obtener la configuración de reCAPTCHA del servidor
  getConfig(): Observable<any> {
    return this.http.get<any>('/api/recaptcha/config');
  }

  // Verificar un token de reCAPTCHA
  verifyToken(token: string): Observable<any> {
    return this.http.post<any>('/api/recaptcha/verify', { token });
  }

  // Ejecutar reCAPTCHA y obtener un token
  executeRecaptcha(action: string): Observable<string> {
    return this.recaptchaV3Service.execute(action);
  }
}
```

#### 4. Implementar en un Componente de Login

```typescript
// login.component.ts
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { RecaptchaService } from '../../services/recaptcha.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  loginForm: FormGroup;
  errorMessage: string = '';
  loading: boolean = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private recaptchaService: RecaptchaService,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      username: ['', [Validators.required]],
      password: ['', [Validators.required]]
    });
  }

  ngOnInit(): void {}

  onSubmit(): void {
    if (this.loginForm.valid) {
      this.loading = true;
      this.errorMessage = '';

      // Ejecutar reCAPTCHA para obtener un token
      this.recaptchaService.executeRecaptcha('login')
        .subscribe({
          next: (token) => {
            // Agregar el token a los datos del formulario
            const loginData = {
              ...this.loginForm.value,
              recaptchaToken: token
            };

            // Enviar solicitud de inicio de sesión
            this.authService.login(loginData)
              .subscribe({
                next: (response) => {
                  if (response.succeeded) {
                    // Inicio de sesión exitoso
                    this.router.navigate(['/dashboard']);
                  } else {
                    // Error en la autenticación
                    this.errorMessage = response.error || 'Error de autenticación';
                    this.loading = false;
                  }
                },
                error: (error) => {
                  this.errorMessage = 'Error al conectar con el servidor';
                  console.error('Error de login:', error);
                  this.loading = false;
                }
              });
          },
          error: (error) => {
            this.errorMessage = 'Error al obtener token de reCAPTCHA';
            console.error('Error de reCAPTCHA:', error);
            this.loading = false;
          }
        });
    }
  }
}
```

#### 5. Implementar en un Componente de Registro

```typescript
// register.component.ts
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { RecaptchaService } from '../../services/recaptcha.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  registerForm: FormGroup;
  errorMessage: string = '';
  loading: boolean = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private recaptchaService: RecaptchaService,
    private router: Router
  ) {
    this.registerForm = this.fb.group({
      username: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]],
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]]
    }, {
      validators: this.passwordMatchValidator
    });
  }

  ngOnInit(): void {}

  passwordMatchValidator(form: FormGroup) {
    const password = form.get('password')?.value;
    const confirmPassword = form.get('confirmPassword')?.value;
    return password === confirmPassword ? null : { mismatch: true };
  }

  onSubmit(): void {
    if (this.registerForm.valid) {
      this.loading = true;
      this.errorMessage = '';

      // Ejecutar reCAPTCHA para obtener un token
      this.recaptchaService.executeRecaptcha('register')
        .subscribe({
          next: (token) => {
            // Agregar el token a los datos del formulario
            const registerData = {
              ...this.registerForm.value,
              recaptchaToken: token
            };

            // Enviar solicitud de registro al nuevo endpoint
            this.authService.register(registerData, true) // true indica usar el nuevo endpoint
              .subscribe({
                next: (response) => {
                  if (response.succeeded) {
                    // Registro exitoso
                    this.router.navigate(['/login'], { 
                      queryParams: { registered: 'true' } 
                    });
                  } else {
                    // Error en el registro
                    this.errorMessage = response.error || 'Error en el registro';
                    this.loading = false;
                  }
                },
                error: (error) => {
                  this.errorMessage = 'Error al conectar con el servidor';
                  console.error('Error de registro:', error);
                  this.loading = false;
                }
              });
          },
          error: (error) => {
            this.errorMessage = 'Error al obtener token de reCAPTCHA';
            console.error('Error de reCAPTCHA:', error);
            this.loading = false;
          }
        });
    }
  }
}
```

#### 6. Actualizar el Servicio de Autenticación

```typescript
// auth.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  constructor(private http: HttpClient) {}

  login(loginData: any): Observable<any> {
    return this.http.post<any>('/api/auth/login', loginData);
  }

  register(registerData: any, useRecaptchaEndpoint: boolean = false): Observable<any> {
    // Usar el nuevo endpoint de reCAPTCHA si se especifica
    const endpoint = useRecaptchaEndpoint 
      ? '/api/recaptcha/register' 
      : '/api/auth/register';
      
    return this.http.post<any>(endpoint, registerData);
  }

  logout(): Observable<any> {
    return this.http.post<any>('/api/auth/logout', {});
  }
}
```

## Consideraciones de Seguridad

### 1. Protección de Claves

- Nunca exponga la clave secreta de reCAPTCHA en el código del cliente.
- Considere el uso de variables de entorno o User Secrets para almacenar la clave secreta en producción.
- Utilice el endpoint `/api/recaptcha/config` para obtener la clave del sitio de forma dinámica.

### 2. Validación en el Servidor

- Siempre valide los tokens de reCAPTCHA en el servidor, nunca confíe en la validación del cliente.
- Implemente una validación adicional para los casos en que reCAPTCHA no esté disponible.
- Utilice el endpoint `/api/recaptcha/verify` para validar tokens de forma independiente cuando sea necesario.

### 3. Puntuación Mínima

- Ajuste la puntuación mínima según las necesidades de seguridad de su aplicación.
- Una puntuación más alta (más cercana a 1.0) reduce el riesgo de bots, pero puede afectar a usuarios legítimos.
- Una puntuación más baja (más cercana a 0.0) permite más usuarios, pero aumenta el riesgo de bots.

## Pruebas y Solución de Problemas

### 1. Modo de Prueba

Para probar reCAPTCHA en un entorno de desarrollo:

1. Utilice las claves de prueba proporcionadas por Google:
   - Clave del sitio: `6LeIxAcTAAAAAJcZVRqyHh71UMIEGNQ_MXjiZKhI`
   - Clave secreta: `6LeIxAcTAAAAAGG-vFI1TnRWxMZNFuojJ4WifJWe`

2. Estas claves siempre devuelven `success=true` y `score=0.9`.

### 2. Solución de Problemas Comunes

1. **Token inválido o caducado**:
   - Los tokens de reCAPTCHA caducan después de 2 minutos.
   - Asegúrese de enviar el token al servidor inmediatamente después de generarlo.

2. **Puntuación baja**:
   - Si los usuarios legítimos reciben puntuaciones bajas, considere reducir la puntuación mínima.
   - Implemente un mecanismo de respaldo para usuarios con puntuaciones bajas (por ejemplo, CAPTCHA tradicional).

3. **Problemas de rendimiento**:
   - Si la carga de reCAPTCHA afecta al rendimiento, considere cargar el script de forma asíncrona.
   - Implemente un mecanismo de caché para evitar solicitudes excesivas a la API de Google.

## Conclusión

La integración de Google reCAPTCHA v3 en AuthSystem proporciona una capa adicional de seguridad para proteger contra bots y spam. Con la implementación del nuevo controlador dedicado y los endpoints específicos, ahora es más fácil integrar reCAPTCHA en aplicaciones frontend, especialmente en aplicaciones Angular.

Recuerde ajustar la configuración según las necesidades específicas de su aplicación y monitorear regularmente las puntuaciones de reCAPTCHA para encontrar el equilibrio adecuado entre seguridad y experiencia de usuario.

## Introducción

Este documento describe la implementación y configuración de Google reCAPTCHA v3 en el sistema AuthSystem. reCAPTCHA es una herramienta de seguridad que ayuda a proteger el sistema contra bots y spam, evaluando el comportamiento de los usuarios y asignando una puntuación de riesgo.

## Actualizaciones Recientes

- Se ha creado un controlador dedicado para reCAPTCHA (`RecaptchaController`) con endpoints específicos para verificación y registro.
- Se ha ampliado la interfaz `IRecaptchaService` para incluir un método que proporciona la configuración pública.
- Se ha agregado una guía detallada para la implementación en aplicaciones frontend Angular.

## Componentes de la Implementación

La implementación de reCAPTCHA en AuthSystem consta de los siguientes componentes:

### 1. Interfaz del Servicio

```csharp
// AuthSystem.Core/Interfaces/IRecaptchaService.cs
public interface IRecaptchaService
{
    Task<bool> ValidateTokenAsync(string token, string ipAddress);
    RecaptchaPublicConfig GetPublicConfig();
}

public class RecaptchaPublicConfig
{
    public string SiteKey { get; set; }
}
```

Esta interfaz define el contrato para validar tokens de reCAPTCHA y obtener la configuración pública (clave del sitio) que puede ser compartida con el cliente.

### 2. Implementación del Servicio

```csharp
// AuthSystem.Infrastructure/Security/RecaptchaService.cs
public class RecaptchaService : IRecaptchaService
{
    private readonly HttpClient _httpClient;
    private readonly RecaptchaSettings _settings;

    public RecaptchaService(HttpClient httpClient, IOptions<RecaptchaSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task<bool> ValidateTokenAsync(string token, string ipAddress)
    {
        if (string.IsNullOrEmpty(token))
        {
            return false;
        }

        var response = await _httpClient.GetStringAsync(
            $"https://www.google.com/recaptcha/api/siteverify?secret={_settings.SecretKey}&response={token}&remoteip={ipAddress}");

        var recaptchaResponse = JsonSerializer.Deserialize<RecaptchaResponse>(response);

        return recaptchaResponse?.Success == true && recaptchaResponse.Score >= _settings.MinimumScore;
    }
    
    public RecaptchaPublicConfig GetPublicConfig()
    {
        return new RecaptchaPublicConfig
        {
            SiteKey = _settings.SiteKey
        };
    }

    private class RecaptchaResponse
    {
        public bool Success { get; set; }
        public float Score { get; set; }
        public string Action { get; set; }
        public string Hostname { get; set; }
    }
}
```

Esta clase implementa la interfaz `IRecaptchaService` y utiliza la API de Google para validar los tokens de reCAPTCHA.

### 3. Modelo de Configuración

```csharp
public class RecaptchaSettings
{
    public string SiteKey { get; set; }
    public string SecretKey { get; set; }
    public float MinimumScore { get; set; } = 0.5f;
}
```

Este modelo define la configuración necesaria para reCAPTCHA:
- `SiteKey`: Clave del sitio proporcionada por Google (para el lado del cliente).
- `SecretKey`: Clave secreta proporcionada por Google (para el lado del servidor).
- `MinimumScore`: Puntuación mínima requerida para considerar válida la interacción (0.0 a 1.0).

### 4. Registro del Servicio

El servicio se registra en el contenedor de dependencias en `DependencyInjection.cs`:

```csharp
// AuthSystem.Infrastructure/DependencyInjection.cs
public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
{
    // Otras configuraciones...

    // Configuración de reCAPTCHA
    services.Configure<RecaptchaSettings>(configuration.GetSection("RecaptchaSettings"));
    services.AddHttpClient<IRecaptchaService, RecaptchaService>();

    // Otras configuraciones...

    return services;
}
```

### 5. Uso en Comandos

El servicio se utiliza en los comandos de autenticación y registro:

```csharp
// AuthSystem.Application/Commands/AuthenticateCommand.cs
public async Task<AuthResponseDto> Handle(AuthenticateCommand request, CancellationToken cancellationToken)
{
    try
    {
        // 1. Verificar reCAPTCHA si se proporciona un token
        if (!string.IsNullOrEmpty(request.RecaptchaToken))
        {
            var isValidRecaptcha = await _recaptchaService.ValidateTokenAsync(
                request.RecaptchaToken, request.IpAddress);

            if (!isValidRecaptcha)
            {
                await LogFailedLoginAttempt(request, null, "reCAPTCHA inválido");
                return new AuthResponseDto
                {
                    Succeeded = false,
                    Error = "Verificación de reCAPTCHA fallida"
                };
            }
        }

        // Resto de la lógica de autenticación...
    }
    catch (Exception ex)
    {
        // Manejo de excepciones...
    }
}
```

## Configuración

### Configuración en appsettings.json

La configuración de reCAPTCHA se define en el archivo `appsettings.json`:

```json
"RecaptchaSettings": {
  "SiteKey": "your-recaptcha-site-key",
  "SecretKey": "your-recaptcha-secret-key",
  "MinimumScore": 0.5
}
```

### Obtención de Claves de reCAPTCHA

Para obtener las claves de reCAPTCHA, siga estos pasos:

1. Vaya a la [Consola de Administración de reCAPTCHA](https://www.google.com/recaptcha/admin).
2. Inicie sesión con su cuenta de Google.
3. Haga clic en "+" para crear un nuevo sitio.
4. Seleccione "reCAPTCHA v3" como tipo de reCAPTCHA.
5. Ingrese el dominio de su sitio (por ejemplo, `localhost` para desarrollo local).
6. Acepte los términos de servicio y haga clic en "Enviar".
7. Copie las claves "Clave del sitio" y "Clave secreta" y configúrelas en `appsettings.json`.

## Controlador de reCAPTCHA

Se ha implementado un controlador dedicado para gestionar las operaciones relacionadas con reCAPTCHA:

```csharp
// AuthSystem.API/Controllers/RecaptchaController.cs
[ApiController]
[Route("api/[controller]")]
public class RecaptchaController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IRecaptchaService _recaptchaService;
    private readonly ILogger<RecaptchaController> _logger;

    public RecaptchaController(
        IMediator mediator,
        IRecaptchaService recaptchaService,
        ILogger<RecaptchaController> logger)
    {
        _mediator = mediator;
        _recaptchaService = recaptchaService;
        _logger = logger;
    }

    // Verifica un token de reCAPTCHA
    [HttpPost("verify")]
    [AllowAnonymous]
    public async Task<ActionResult<RecaptchaVerificationResponse>> VerifyToken([FromBody] RecaptchaVerificationRequest request)
    {
        // Implementación de verificación de token
    }

    // Registra un nuevo usuario con validación de reCAPTCHA
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto request)
    {
        // Implementación de registro con validación de reCAPTCHA
    }

    // Obtiene la configuración pública de reCAPTCHA (solo la clave del sitio)
    [HttpGet("config")]
    [AllowAnonymous]
    public ActionResult<RecaptchaConfigResponse> GetConfig()
    {
        // Obtener la configuración del servicio
        var config = _recaptchaService.GetPublicConfig();
        
        return Ok(new RecaptchaConfigResponse
        {
            SiteKey = config.SiteKey
        });
    }
}
```

### Endpoints Disponibles

| Endpoint | Método | Descripción |
|----------|--------|-------------|
| `/api/recaptcha/verify` | POST | Verifica un token de reCAPTCHA |
| `/api/recaptcha/register` | POST | Registra un nuevo usuario con validación de reCAPTCHA |
| `/api/recaptcha/config` | GET | Obtiene la configuración pública de reCAPTCHA |

## Implementación en el Cliente

### Implementación en JavaScript Vanilla

#### 1. Agregar el Script de reCAPTCHA

Agregue el script de reCAPTCHA a su página HTML:

```html
<script src="https://www.google.com/recaptcha/api.js?render=YOUR_SITE_KEY"></script>
```

#### 2. Generar un Token al Enviar un Formulario

```javascript
// Función para obtener un token de reCAPTCHA
function getRecaptchaToken(action) {
    return new Promise((resolve, reject) => {
        grecaptcha.ready(() => {
            grecaptcha.execute('YOUR_SITE_KEY', { action: action })
                .then(token => resolve(token))
                .catch(error => reject(error));
        });
    });
}

// Ejemplo de uso en un formulario de inicio de sesión
document.getElementById('loginForm').addEventListener('submit', async function(event) {
    event.preventDefault();
    
    try {
        // Obtener token de reCAPTCHA
        const recaptchaToken = await getRecaptchaToken('login');
        
        // Agregar el token al objeto de datos
        const formData = {
            username: document.getElementById('username').value,
            password: document.getElementById('password').value,
            recaptchaToken: recaptchaToken
        };
        
        // Enviar datos al servidor
        const response = await fetch('/api/auth/login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
{{ ... }}
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(formData)
        });
        
        const result = await response.json();
        
        if (result.succeeded) {
            // Manejar inicio de sesión exitoso
            window.location.href = '/dashboard';
        } else {
            // Manejar error
            document.getElementById('errorMessage').textContent = result.error;
        }
    } catch (error) {
        console.error('Error:', error);
    }
});
```

### Implementación en Angular

A continuación se detalla cómo implementar reCAPTCHA v3 en una aplicación Angular:

#### 1. Instalar el paquete de reCAPTCHA para Angular

```bash
ng add @angular/forms
npm install ng-recaptcha --save
```

#### 2. Configurar el Módulo de reCAPTCHA

En tu `app.module.ts`:

```typescript
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RECAPTCHA_V3_SITE_KEY, RecaptchaV3Module } from 'ng-recaptcha';

import { AppComponent } from './app.component';
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    RegisterComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    ReactiveFormsModule,
    HttpClientModule,
    RecaptchaV3Module
  ],
  providers: [
    { provide: RECAPTCHA_V3_SITE_KEY, useValue: 'your-recaptcha-site-key' }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }

Para probar el servicio de reCAPTCHA, puede crear un mock de `IRecaptchaService`:

```csharp
// Configurar el mock
var mockRecaptchaService = new Mock<IRecaptchaService>();
mockRecaptchaService.Setup(s => s.ValidateTokenAsync(It.IsAny<string>(), It.IsAny<string>()))
    .ReturnsAsync(true); // Simular validación exitosa

// Inyectar el mock en el handler
var handler = new AuthenticateCommandHandler(
    mockUserRepository.Object,
    mockPasswordHasher.Object,
    mockJwtService.Object,
    mockRecaptchaService.Object,
    mockLogger.Object);

// Ejecutar la prueba
var result = await handler.Handle(command, CancellationToken.None);
```

### Pruebas Manuales

Para probar manualmente reCAPTCHA:

1. Configure reCAPTCHA con su clave de sitio y clave secreta.
2. Implemente la generación de tokens en el cliente.
3. Envíe formularios con y sin tokens válidos.
4. Verifique que las solicitudes sin tokens válidos sean rechazadas.
5. Verifique que las solicitudes con tokens válidos sean aceptadas.

## Solución de Problemas

### Problemas Comunes

1. **Token no válido**:
   - Verifique que esté utilizando la clave de sitio correcta en el cliente.
   - Asegúrese de que el dominio esté registrado en la consola de reCAPTCHA.
   - Compruebe que el token no haya expirado (los tokens expiran después de 2 minutos).

2. **Puntuación baja**:
   - Si los usuarios legítimos reciben puntuaciones bajas, considere reducir la puntuación mínima.
   - Implemente un mecanismo de respaldo para usuarios con puntuaciones bajas (por ejemplo, CAPTCHA tradicional).

3. **Problemas de rendimiento**:
   - La validación del token requiere una llamada a la API de Google, lo que puede afectar el rendimiento.
   - Considere implementar caché para reducir el número de llamadas a la API.

## Conclusión

La integración de Google reCAPTCHA v3 en AuthSystem proporciona una capa adicional de seguridad contra bots y spam. La implementación es flexible y permite ajustar la seguridad según las necesidades específicas del sistema.

Al seguir las prácticas recomendadas y consideraciones de seguridad descritas en este documento, puede implementar reCAPTCHA de manera efectiva y segura en su aplicación.

## Referencias

- [Documentación oficial de Google reCAPTCHA v3](https://developers.google.com/recaptcha/docs/v3)
- [Consola de Administración de reCAPTCHA](https://www.google.com/recaptcha/admin)
- [Guía de integración de reCAPTCHA para .NET](https://developers.google.com/recaptcha/docs/verify)

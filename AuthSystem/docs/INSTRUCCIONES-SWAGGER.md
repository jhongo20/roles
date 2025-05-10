# Instrucciones para Implementar la Configuración Mejorada de Swagger

## Cambios Necesarios en Program.cs

Para implementar la configuración mejorada de Swagger que hemos creado, necesitas realizar los siguientes cambios en el archivo `Program.cs`:

### 1. Añadir el Namespace

Asegúrate de añadir el siguiente namespace al inicio del archivo:

```csharp
using AuthSystem.API.Swagger;
```

### 2. Reemplazar la Configuración de Servicios

Localiza el bloque de código que comienza con `// Agregar Swagger` (aproximadamente en la línea 120) y reemplázalo con:

```csharp
// Agregar Swagger con configuración mejorada
builder.Services.ConfigureSwaggerServices();
```

Este cambio reemplaza toda la configuración actual de Swagger con nuestra configuración personalizada.

### 3. Reemplazar la Configuración de Middleware

Localiza las líneas donde se configura el middleware de Swagger (aproximadamente líneas 174-175):

```csharp
app.UseSwagger();
app.UseSwaggerUI();
```

Y reemplázalas con:

```csharp
// Configurar Swagger UI mejorada
app.ConfigureSwagger();
```

## Verificación

Después de realizar estos cambios:

1. Compila el proyecto para asegurarte de que no hay errores:
   ```
   dotnet build
   ```

2. Ejecuta la aplicación:
   ```
   dotnet run --project src/AuthSystem.API/AuthSystem.API.csproj
   ```

3. Navega a la URL de la documentación de Swagger (normalmente será algo como `https://localhost:5001/api-docs`)

## Solución de Problemas

Si encuentras algún error relacionado con el método `EnableAnnotations()`, es porque este método requiere el paquete `Swashbuckle.AspNetCore.Annotations`. Puedes:

1. Instalar el paquete:
   ```
   dotnet add src/AuthSystem.API/AuthSystem.API.csproj package Swashbuckle.AspNetCore.Annotations
   ```

2. O simplemente eliminar/comentar esa línea en `SwaggerConfiguration.cs` si no necesitas las anotaciones.

## Próximos Pasos

Una vez que la configuración básica esté funcionando, puedes:

1. Añadir comentarios XML a tus controladores y DTOs para mejorar la documentación
2. Personalizar los ejemplos en `SwaggerExamples.cs` para que se ajusten mejor a tu dominio
3. Ajustar la información de la API en `SwaggerConfiguration.cs` con datos más precisos

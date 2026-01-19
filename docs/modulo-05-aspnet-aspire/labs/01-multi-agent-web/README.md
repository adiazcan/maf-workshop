# Lab 01: Multi-Agent Web API con ASP.NET Core y .NET Aspire

**Duración**: 60 minutos  
**Tipo**: Capstone Project (Proyecto Integrador)  
**Complejidad**: Avanzada  
**Prerequisitos**: Módulos 1-4 completados

## 🎯 Objetivo

Construir una aplicación web completa que expone múltiples agentes de Microsoft Agent Framework como APIs REST, orquestada con .NET Aspire para observabilidad integrada.

Al completar este lab, habrás creado:
- Una API REST con ASP.NET Core Minimal APIs
- Dos servicios de agentes (WeatherAgent y SummaryAgent)
- Orquestación con .NET Aspire
- Dashboard unificado para logs, traces y métricas

---

## 📋 Prerrequisitos

Antes de comenzar, verifica que tienes:

- [x] .NET 10 SDK instalado
- [x] VS Code o Visual Studio 2022
- [x] Azure OpenAI deployment configurado
- [x] API key de Azure OpenAI

**Nota**: A partir de .NET Aspire 13.x, no es necesario instalar un workload separado. Los paquetes NuGet de Aspire se referencian directamente en los proyectos.

---

## 📁 Estructura del Proyecto

```
01-multi-agent-web/
├── MultiAgentWeb.sln           # Solución de Visual Studio
├── AppHost/                    # Orquestador de Aspire
│   ├── AppHost.csproj
│   └── Program.cs
├── WebApi/                     # API REST
│   ├── WebApi.csproj
│   ├── Program.cs
│   └── appsettings.json
└── AgentServices/              # Biblioteca de agentes
    ├── AgentServices.csproj
    ├── WeatherAgentService.cs
    └── SummaryAgentService.cs
```

---

## 🚀 Pasos de Implementación

### Paso 1: Configurar API Key de Azure OpenAI

El proyecto necesita acceso a Azure OpenAI. Configura tu API key usando user-secrets:

```bash
# Navegar a la carpeta del lab
cd docs/modulo-05-aspnet-aspire/labs/01-multi-agent-web

# Inicializar user-secrets para WebApi
cd WebApi
dotnet user-secrets init

# Agregar tu API key (reemplaza con tu clave real)
dotnet user-secrets set "AzureOpenAI:ApiKey" "tu-api-key-aquí"

# Configurar endpoint si es diferente al default
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://tu-recurso.openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:DeploymentName" "gpt-4o"

cd ..
```

**⚠️ Importante**: Nunca comitas API keys en el código fuente.

---

### Paso 2: Restaurar Dependencias

```bash
# Desde la raíz del lab (01-multi-agent-web)
dotnet restore MultiAgentWeb.sln
```

**Verificación**: No deberías ver errores de paquetes faltantes.

---

### Paso 3: Compilar la Solución

```bash
dotnet build MultiAgentWeb.sln
```

**Resultado esperado**:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

### Paso 4: Ejecutar con Aspire

Inicia la aplicación usando el proyecto AppHost:

```bash
dotnet run --project AppHost
```

**Resultado esperado**:
```
info: Aspire.Hosting.DistributedApplication[0]
      Aspire version: 13.1.0
info: Aspire.Hosting.DistributedApplication[0]
      Distributed application starting.
info: Aspire.Hosting.DistributedApplication[0]
      Application started successfully.
info: Aspire.Hosting.DistributedApplication[0]
      Dashboard is running at: http://localhost:15888
```

**📌 Anota la URL del dashboard**: `http://localhost:15888`

---

### Paso 5: Explorar el Dashboard de Aspire

Abre el dashboard en tu navegador: `http://localhost:15888`

#### Panel "Resources"
Verás los servicios corriendo:
- **webapi**: Tu API REST (estado: Running)

#### Panel "Console"
Logs en tiempo real de cada servicio.

#### Panel "Traces"
Distributed tracing - aún vacío hasta que hagas requests.

---

### Paso 6: Probar el Endpoint de Clima

Abre otra terminal y usa curl o tu herramienta favorita:

```bash
# Usando curl
curl -X POST http://localhost:5000/api/chat/weather \
  -H "Content-Type: application/json" \
  -d '{"city": "Madrid"}'
```

**O usa PowerShell**:
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/chat/weather" `
  -Method POST `
  -ContentType "application/json" `
  -Body '{"city": "Madrid"}'
```

**Respuesta esperada**:
```json
{
  "response": "🌡️ **Temperatura**: 18°C\n🌤️ **Condiciones**: Parcialmente nublado...",
  "agent": "WeatherAgent",
  "timestamp": "2026-01-12T10:30:00Z"
}
```

---

### Paso 7: Probar el Endpoint de Resumen

```bash
curl -X POST http://localhost:5000/api/chat/summary \
  -H "Content-Type: application/json" \
  -d '{
    "text": "La inteligencia artificial ha transformado múltiples industrias en los últimos años. Desde la automatización de procesos hasta el análisis predictivo, las empresas están adoptando estas tecnologías para mejorar su eficiencia operativa. Los modelos de lenguaje grandes, como GPT, han revolucionado la forma en que interactuamos con las computadoras, permitiendo conversaciones naturales y generación de contenido. Sin embargo, también surgen preocupaciones éticas sobre el uso responsable de estas herramientas y su impacto en el empleo.",
    "maxLength": 50
  }'
```

**Respuesta esperada**:
```json
{
  "response": "📋 **Resumen**\nLa IA transforma industrias mediante automatización...",
  "agent": "SummaryAgent",
  "timestamp": "2026-01-12T10:31:00Z"
}
```

---

### Paso 8: Verificar Traces en Dashboard

1. Regresa al dashboard de Aspire (`http://localhost:15888`)
2. Navega a la pestaña **"Traces"**
3. Deberías ver traces de tus requests recientes

Los traces muestran:
- Tiempo total del request
- Spans internos (HTTP, OpenAI calls)
- Latencia de cada operación

---

### Paso 9: Explorar Swagger UI

Abre en tu navegador: `http://localhost:5000`

Swagger UI muestra:
- Todos los endpoints disponibles
- Esquemas de request/response
- Botón "Try it out" para probar directamente

**Prueba desde Swagger**:
1. Expande `POST /api/chat/weather`
2. Click "Try it out"
3. Ingresa `{"city": "Barcelona"}`
4. Click "Execute"
5. Observa la respuesta

---

### Paso 10: Validación Final

Verifica que has completado todos los objetivos:

| Objetivo | Método de Verificación | ✓ |
|----------|------------------------|---|
| API responde | GET `/health` retorna 200 | ☐ |
| WeatherAgent funciona | POST `/api/chat/weather` retorna clima | ☐ |
| SummaryAgent funciona | POST `/api/chat/summary` retorna resumen | ☐ |
| Dashboard muestra logs | Verificar panel Console en Aspire | ☐ |
| Traces visibles | Verificar panel Traces después de requests | ☐ |
| Swagger documenta API | Abrir `/` muestra Swagger UI | ☐ |

**✅ Criterio de éxito**: Todos los checkmarks completados.

---

## 🔧 Troubleshooting

### "Aspire dashboard not accessible"

**Síntoma**: No puedes abrir `http://localhost:15888`

**Causas posibles**:
1. Puerto 15888 en uso
2. Firewall bloqueando

**Solución**:
```bash
# Especificar puerto alternativo
dotnet run --project AppHost -- --dashboard-port 15889
```

---

### "Agents not resolving in DI"

**Síntoma**: Error `Unable to resolve service for type 'WeatherAgentService'`

**Causa**: Servicios no registrados correctamente en DI

**Solución**: Verificar en `WebApi/Program.cs`:
```csharp
builder.Services.AddSingleton<WeatherAgentService>();
builder.Services.AddSingleton<SummaryAgentService>();
```

---

### "401 Unauthorized" de Azure OpenAI

**Síntoma**: `HttpRequestException: Response status code does not indicate success: 401`

**Causas**:
1. API key incorrecta
2. API key no configurada

**Solución**:
```bash
cd WebApi
dotnet user-secrets list  # Verificar que la key está configurada
dotnet user-secrets set "AzureOpenAI:ApiKey" "nueva-api-key"
```

---

### "CORS errors" en frontend

**Síntoma**: `Access to fetch blocked by CORS policy`

**Causa**: Frontend en diferente origen

**Solución**: El proyecto ya incluye CORS permisivo. Si necesitas restricciones:
```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000") // Tu frontend
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

---

### "Connection refused" al llamar la API

**Síntoma**: `curl: (7) Failed to connect to localhost port 5000`

**Causas**:
1. API no está corriendo
2. Puerto incorrecto

**Solución**:
1. Verificar que AppHost está corriendo
2. Revisar la URL en el dashboard de Aspire
3. El puerto puede variar - usar la URL del dashboard

---

### Timeout en respuestas del agente

**Síntoma**: Requests toman más de 30 segundos y fallan

**Causa**: Modelo de Azure OpenAI sobrecargado o rate limits

**Solución**:
```bash
# Verificar quota en Azure Portal
# O usar un deployment con mayor capacidad
```

---

## 📊 Métricas de Éxito

Para el instructor:
- **Meta**: 70% de participantes completan el capstone
- **Tiempo límite**: 60 minutos
- **Checkpoint intermedio**: API respondiendo (minuto 30)

---

## 🎓 Conceptos Aprendidos

Al completar este lab has practicado:

1. **ASP.NET Core Minimal APIs**: Endpoints REST concisos
2. **Dependency Injection**: Inyección de IChatClient y servicios
3. **.NET Aspire Orchestration**: AppHost y dashboard
4. **OpenTelemetry Integration**: Traces automáticos
5. **Error Handling**: Manejo de errores HTTP
6. **Swagger/OpenAPI**: Documentación automática de API

---

## 🚀 Desafíos Opcionales

Si terminaste antes, intenta:

### Desafío 1: Agregar Rate Limiting
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
        context => RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1)
            }
        )
    );
});
```

### Desafío 2: Agregar Endpoint de Traducción
Crea un `TranslationAgentService` que traduzca texto entre idiomas.

### Desafío 3: Health Checks Detallados
```csharp
builder.Services.AddHealthChecks()
    .AddCheck<AzureOpenAIHealthCheck>("azure-openai");
```

---

## 📚 Recursos Adicionales

- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire)
- [ASP.NET Core Minimal APIs](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/languages/dotnet/)
- [Microsoft Agent Framework](https://learn.microsoft.com/microsoft-agent-framework)

---

## ✅ Checkpoint de Validación

**Antes de continuar al siguiente módulo, verifica**:

```bash
# Test rápido de todos los endpoints
curl http://localhost:5000/health
curl -X POST http://localhost:5000/api/chat/weather -H "Content-Type: application/json" -d '{"city":"México"}'
curl -X POST http://localhost:5000/api/chat/summary -H "Content-Type: application/json" -d '{"text":"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam."}'
```

**Todos retornan respuestas válidas**: ✅ Lab completado

---

**¡Felicidades!** Has completado el proyecto capstone de integración ASP.NET + Aspire. 🎉

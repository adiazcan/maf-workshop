# Lab 02: Trazas Distribuidas (Distributed Tracing)

**Duración**: 25 minutos  
**Nivel**: Intermedio  
**Módulo**: 04 - Observabilidad

---

## Objetivos

Al completar este laboratorio, podrás:

1. ✅ Crear spans padre e hijo para representar jerarquías de llamadas
2. ✅ Propagar contexto de traza entre múltiples agentes
3. ✅ Añadir atributos personalizados a los spans
4. ✅ Visualizar la estructura jerárquica de trazas
5. ✅ Entender cómo fluyen las solicitudes a través de un sistema multi-agente

---

## Conceptos Clave

### ¿Qué son las Trazas Distribuidas?

Las **trazas** permiten seguir el flujo de una solicitud a través de múltiples servicios o componentes:

```text
┌─────────────────────────────────────────────────────────────┐
│                     ESTRUCTURA DE TRAZA                      │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Trace (TraceId: abc123)                                     │
│  │                                                           │
│  └─ Span: Workflow.ProcessQuery (SpanId: span1)             │
│     │   └─ Tags: correlation_id, query_length               │
│     │                                                        │
│     ├─ Span: WeatherAgent.GetWeather (SpanId: span2)        │
│     │   │   └─ Tags: agent.name, weather.city               │
│     │   │                                                    │
│     │   └─ Span: HTTP POST (auto-generated)                 │
│     │       └─ Tags: http.url, http.status_code             │
│     │                                                        │
│     └─ Span: NewsAgent.GetNews (SpanId: span3)              │
│         │   └─ Tags: agent.name, news.location              │
│         │                                                    │
│         └─ Span: HTTP POST (auto-generated)                 │
│             └─ Tags: http.url, http.status_code             │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

### Terminología

| Término | Descripción |
|---------|-------------|
| **Trace** | El viaje completo de una solicitud |
| **Span** | Una unidad de trabajo dentro de una traza |
| **TraceId** | Identificador único de la traza |
| **SpanId** | Identificador único del span |
| **ParentId** | SpanId del span padre (crea la jerarquía) |
| **Tags/Attributes** | Metadatos del span |
| **Baggage** | Contexto propagado entre spans |

### Propagación de Contexto

```csharp
// Span padre establece baggage
activity?.SetBaggage("correlation_id", correlationId);

// Spans hijos leen el baggage
var correlationId = Activity.Current?.GetBaggageItem("correlation_id");
```

---

## Paso 1: Preparación del Proyecto

### 1.1 Navega al directorio del laboratorio

```bash
cd docs/modulo-04-observability/labs/02-distributed-traces
```

### 1.2 Restaura los paquetes NuGet

```bash
dotnet restore
```

### 1.3 Configura tus credenciales

```bash
dotnet user-secrets init
dotnet user-secrets set "AzureOpenAI:ApiKey" "tu-api-key-aqui"
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://tu-recurso.openai.azure.com/"
```

---

## Paso 2: Entender la Arquitectura

### 2.1 Componentes del Sistema

```text
┌─────────────────────────────────────────────────────────────┐
│                    ARQUITECTURA DEL LAB                      │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Usuario → WorkflowOrchestrator                             │
│                    │                                         │
│                    ├──→ WeatherAgentService                 │
│                    │         └──→ Azure OpenAI              │
│                    │                                         │
│                    └──→ NewsAgentService                    │
│                              └──→ Azure OpenAI              │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

### 2.2 ActivitySources (Fuentes de Trazas)

El código define dos `ActivitySource`:

```csharp
// Para el orquestador del workflow
private static readonly ActivitySource ActivitySource = 
    new("Workshop.MAF.Workflow", "1.0.0");

// Para los agentes individuales
private static readonly ActivitySource ActivitySource = 
    new("Workshop.MAF.Agents", "1.0.0");
```

Y se registran en OpenTelemetry:

```csharp
.WithTracing(tracing =>
{
    tracing
        .AddSource("Workshop.MAF.Agents")      // Trazas de agentes
        .AddSource("Workshop.MAF.Workflow")    // Trazas del workflow
        .AddHttpClientInstrumentation()        // Trazas HTTP automáticas
        .AddConsoleExporter();
});
```

---

## Paso 3: Analizar el Código de Trazas

### 3.1 Span Padre (Workflow)

Abre `Program.cs` y observa el método `ProcessQueryAsync`:

```csharp
public async Task<string> ProcessQueryAsync(string query)
{
    // Crear span PADRE para todo el workflow
    using var activity = ActivitySource.StartActivity(
        name: "Workflow.ProcessQuery",
        kind: ActivityKind.Server);  // Server = punto de entrada
    
    // Generar ID de correlación
    var correlationId = Guid.NewGuid().ToString("N")[..8];
    
    // Añadir atributos al span
    activity?.SetTag("workflow.correlation_id", correlationId);
    activity?.SetTag("workflow.query_length", query.Length);
    
    // Propagar contexto a spans hijos via baggage
    activity?.SetBaggage("correlation_id", correlationId);
    
    // ... ejecutar agentes (crean spans hijos)
}
```

### 3.2 Spans Hijos (Agentes)

Observa el método `GetWeatherAsync`:

```csharp
public async Task<string> GetWeatherAsync(string city)
{
    // Crear span HIJO - automáticamente enlazado al span padre activo
    using var activity = ActivitySource.StartActivity(
        name: "WeatherAgent.GetWeather",
        kind: ActivityKind.Client);  // Client = llamada saliente
    
    // Recuperar contexto propagado del padre
    var correlationId = Activity.Current?.GetBaggageItem("correlation_id");
    
    // Añadir atributos específicos de este agente
    activity?.SetTag("agent.name", "WeatherAgent");
    activity?.SetTag("weather.city", city);
    activity?.SetTag("correlation_id", correlationId);
    
    // ... hacer la llamada a Azure OpenAI (genera sub-span HTTP)
    
    // Registrar métricas en el span
    activity?.SetTag("llm.prompt_tokens", response.Value.Usage.InputTokenCount);
    activity?.SetStatus(ActivityStatusCode.Ok);
}
```

---

## Paso 4: Ejecutar el Laboratorio

### 4.1 Compila el proyecto

```bash
dotnet build
```

### 4.2 Ejecuta la aplicación

```bash
dotnet run
```

### 4.3 Observa la salida

Verás algo similar a:

```text
╔══════════════════════════════════════════════════════════════════╗
║       Lab 02: Trazas Distribuidas con OpenTelemetry              ║
╚══════════════════════════════════════════════════════════════════╝

🔗 Iniciando demostración de trazas distribuidas...

📋 Este laboratorio simula un workflow multi-agente:
   1. Usuario hace una consulta compleja
   2. Orquestador crea un span padre
   3. WeatherAgent y NewsAgent crean spans hijos
   4. Cada llamada a Azure OpenAI crea sub-spans

═══════════════════════════════════════════════════════════

💬 Consulta del usuario: "Dame el clima y las noticias destacadas de Madrid"

⚡ Ejecutando agentes en paralelo...

   🌤️ WeatherAgent: Consultando clima para Madrid...
   📰 NewsAgent: Buscando noticias de Madrid...
   ✅ WeatherAgent completado en 1234ms
   ✅ NewsAgent completado en 1456ms

────────────────────────────────────────────────────────────
📊 Resultado del Workflow:
────────────────────────────────────────────────────────────
🌤️ CLIMA EN MADRID:
En Madrid el día está soleado con temperaturas de 22°C...

📰 NOTICIAS DE MADRID:
• El Real Madrid presenta su nueva camiseta...
• El metro de Madrid inaugura nueva línea...
```

---

## Paso 5: Analizar las Trazas Exportadas

### 5.1 Estructura de la Traza

La salida de OpenTelemetry mostrará algo como:

```text
Activity.TraceId:          abc123def456789...
Activity.SpanId:           span1111111111
Activity.TraceFlags:       Recorded
Activity.ActivitySourceName: Workshop.MAF.Workflow
Activity.DisplayName:      Workflow.ProcessQuery
Activity.Kind:             Server
Activity.StartTime:        2024-01-12T10:30:00.0000000Z
Activity.Duration:         00:00:02.8901234
Activity.Tags:
    workflow.correlation_id: a1b2c3d4
    workflow.query_length: 48
    workflow.agents_count: 2
    workflow.status: success
Activity.Status:           Ok
Resource associated with Activity:
    service.name: workshop-maf-tracing-lab
    service.version: 1.0.0

Activity.TraceId:          abc123def456789...  ← MISMO TraceId
Activity.SpanId:           span2222222222
Activity.ParentId:         span1111111111     ← REFERENCIA al padre
Activity.ActivitySourceName: Workshop.MAF.Agents
Activity.DisplayName:      WeatherAgent.GetWeather
Activity.Kind:             Client
Activity.Tags:
    agent.name: WeatherAgent
    weather.city: Madrid
    llm.prompt_tokens: 45
    llm.completion_tokens: 32
```

### 5.2 Claves para Interpretar

| Campo | Significado |
|-------|-------------|
| `TraceId` | Mismo valor = misma traza |
| `ParentId` | Apunta al SpanId del padre |
| `Duration` | Tiempo total del span |
| `Tags` | Atributos que añadimos |
| `Status` | Ok o Error |

---

## Paso 6: Experimentar

### 6.1 Agregar más atributos

Modifica `WeatherAgentService` para añadir más información:

```csharp
activity?.SetTag("weather.temperature_unit", "celsius");
activity?.SetTag("weather.forecast_days", 1);
```

### 6.2 Crear spans adicionales

Añade un span para medición interna:

```csharp
using (var parseActivity = ActivitySource.StartActivity("WeatherAgent.ParseResponse"))
{
    parseActivity?.SetTag("parse.format", "json");
    // ... lógica de parsing
}
```

### 6.3 Simular un error

Añade código para probar el manejo de errores:

```csharp
if (city == "error")
{
    throw new InvalidOperationException("Ciudad no válida para prueba");
}
```

---

## Validación del Laboratorio

### Criterios de Éxito

- [ ] El proyecto compila y ejecuta sin errores
- [ ] Las trazas muestran el TraceId compartido entre spans
- [ ] El span del workflow contiene los spans de los agentes como hijos
- [ ] Los atributos personalizados aparecen en los tags
- [ ] El correlation_id se propaga correctamente

### Preguntas de Comprensión

1. **¿Cómo sabe OpenTelemetry que un span es hijo de otro?**
   - El span activo actual se convierte automáticamente en el padre
   - El ParentId del hijo apunta al SpanId del padre

2. **¿Cuál es la diferencia entre Tags y Baggage?**
   - Tags: Atributos del span actual solamente
   - Baggage: Contexto propagado a TODOS los spans descendientes

3. **¿Por qué los spans HTTP aparecen automáticamente?**
   - Por `AddHttpClientInstrumentation()` en la configuración

---

## Troubleshooting

### "Los spans no muestran jerarquía (sin ParentId)"

**Causa**: El ActivitySource no está registrado correctamente.

**Solución**: Verifica que `AddSource("Workshop.MAF.Agents")` incluye el nombre exacto del ActivitySource.

### "Las trazas HTTP no aparecen"

**Causa**: Falta instrumentación HTTP.

**Solución**: Asegura que tienes `.AddHttpClientInstrumentation()` en la configuración.

### "El baggage no se propaga"

**Causa**: El span padre ya finalizó antes de crear los hijos.

**Solución**: Asegura que el `using var activity` del padre envuelve todo el código que crea hijos.

---

## Recursos Adicionales

- [System.Diagnostics.Activity](https://learn.microsoft.com/dotnet/core/diagnostics/distributed-tracing)
- [OpenTelemetry Tracing Concepts](https://opentelemetry.io/docs/concepts/signals/traces/)
- [W3C Trace Context](https://www.w3.org/TR/trace-context/)

---

## Siguiente Laboratorio

➡️ Continúa con [Lab 03: Azure Monitor](../03-azure-monitor/) para exportar telemetría a Application Insights y crear dashboards.

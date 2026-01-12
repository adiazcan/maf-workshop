# Lab 01: Métricas de Tokens y Latencia

**Duración**: 20 minutos  
**Nivel**: Intermedio  
**Módulo**: 04 - Observabilidad

---

## Objetivos

Al completar este laboratorio, podrás:

1. ✅ Configurar OpenTelemetry para métricas en aplicaciones .NET
2. ✅ Implementar contadores personalizados para invocaciones y errores
3. ✅ Crear histogramas para medir distribución de latencia
4. ✅ Registrar el consumo de tokens de Azure OpenAI
5. ✅ Exportar métricas a consola para visualización

---

## Conceptos Clave

### ¿Qué son las Métricas?

Las **métricas** son valores numéricos agregados que representan el estado del sistema en el tiempo:

| Tipo | Descripción | Ejemplo |
|------|-------------|---------|
| **Counter** | Valor que solo aumenta | Total de invocaciones |
| **Histogram** | Distribución de valores | Latencia (p50, p95, p99) |
| **Gauge** | Valor que puede subir o bajar | Conexiones activas |

### Métricas para Agentes de IA

```text
┌─────────────────────────────────────────────────────────────┐
│                    MÉTRICAS DEL AGENTE                       │
├─────────────────────────────────────────────────────────────┤
│  📊 agent.invocations.total    → ¿Cuántas veces se usó?     │
│  ❌ agent.invocations.errors   → ¿Cuántos errores hubo?     │
│  ⏱️ agent.latency              → ¿Qué tan rápido responde?  │
│  📝 agent.tokens.prompt        → ¿Cuánto cuesta la entrada? │
│  📤 agent.tokens.completion    → ¿Cuánto cuesta la salida?  │
└─────────────────────────────────────────────────────────────┘
```

---

## Paso 1: Preparación del Proyecto

### 1.1 Navega al directorio del laboratorio

```bash
cd docs/modulo-04-observability/labs/01-metrics-tokens
```

### 1.2 Restaura los paquetes NuGet

```bash
dotnet restore
```

### 1.3 Configura tus credenciales de Azure OpenAI

```bash
# Inicializar user-secrets para este proyecto
dotnet user-secrets init

# Configurar el API key
dotnet user-secrets set "AzureOpenAI:ApiKey" "tu-api-key-aqui"

# Configurar el endpoint (opcional si ya está en appsettings.json)
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://tu-recurso.openai.azure.com/"
```

> ⚠️ **Importante**: Nunca guardes claves API en código fuente. Usa user-secrets para desarrollo local.

---

## Paso 2: Entender la Estructura del Código

### 2.1 Configuración de OpenTelemetry

Abre `Program.cs` y observa la configuración de OpenTelemetry:

```csharp
// Definir el recurso que identifica nuestra aplicación
var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService(
        serviceName: "workshop-maf-metrics-lab",
        serviceVersion: "1.0.0")
    .AddAttributes(new Dictionary<string, object>
    {
        ["deployment.environment"] = "workshop",
        ["lab.module"] = "04-observability",
        ["lab.number"] = "01"
    });

// Configurar OpenTelemetry para métricas
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics
            .SetResourceBuilder(resourceBuilder)
            .AddMeter("Workshop.MAF.Agents")    // Nuestro Meter personalizado
            .AddRuntimeInstrumentation()        // Métricas del runtime .NET
            .AddHttpClientInstrumentation()     // Métricas HTTP
            .AddConsoleExporter();              // Exportar a consola
    });
```

**Puntos importantes:**
- `ResourceBuilder` define la identidad de la aplicación en los datos de telemetría
- `AddMeter()` registra el Meter personalizado que usaremos
- `AddConsoleExporter()` envía las métricas a la consola (para desarrollo)

### 2.2 Servicio de Métricas Personalizado

Observa la clase `AgentMetricsService`:

```csharp
public class AgentMetricsService
{
    private readonly Meter _meter;
    private readonly Counter<long> _invocationsCounter;
    private readonly Histogram<double> _latencyHistogram;
    
    public AgentMetricsService()
    {
        // El nombre del Meter debe coincidir con AddMeter()
        _meter = new Meter("Workshop.MAF.Agents", "1.0.0");
        
        // Contador para invocaciones
        _invocationsCounter = _meter.CreateCounter<long>(
            name: "agent.invocations.total",
            unit: "invocations",
            description: "Número total de invocaciones del agente");
        
        // Histograma para latencia
        _latencyHistogram = _meter.CreateHistogram<double>(
            name: "agent.latency",
            unit: "ms",
            description: "Tiempo de respuesta en milisegundos");
    }
}
```

---

## Paso 3: Ejecutar el Laboratorio

### 3.1 Compila el proyecto

```bash
dotnet build
```

### 3.2 Ejecuta la aplicación

```bash
dotnet run
```

### 3.3 Observa la salida

Deberías ver algo similar a:

```text
╔══════════════════════════════════════════════════════════════════╗
║     Lab 01: Métricas de Tokens y Latencia con OpenTelemetry      ║
║     Módulo 4: Observabilidad - Microsoft Agent Framework         ║
╚══════════════════════════════════════════════════════════════════╝

📊 Iniciando demostración de métricas...
   Endpoint: https://tu-recurso.openai.azure.com/
   Modelo: gpt-4

📝 Ejecutando 5 consultas para generar métricas...
────────────────────────────────────────────────────────────────

💬 Pregunta: ¿Cuál es la capital de España?
✅ Respuesta (845ms): La capital de España es Madrid.
   📈 Tokens - Entrada: 15, Salida: 8, Total: 23

💬 Pregunta: Dame un dato curioso sobre inteligencia artificial.
✅ Respuesta (1234ms): La IA puede componer música que suena...
   📈 Tokens - Entrada: 18, Salida: 45, Total: 63

...

📊 Resumen de métricas generadas:
   • Total invocaciones: 5
   • Total errores: 0
   • Tokens de entrada: 95
   • Tokens de salida: 180
   • Tokens totales: 275
```

---

## Paso 4: Analizar las Métricas Exportadas

### 4.1 Salida de OpenTelemetry

Después de unos segundos, verás la exportación de métricas:

```text
Export agent.invocations.total, Meter: Workshop.MAF.Agents/1.0.0
(2024-01-12T10:30:00.0000000Z, 2024-01-12T10:30:15.0000000Z] Sum LongSum
Value: 5

Export agent.latency, Meter: Workshop.MAF.Agents/1.0.0
(2024-01-12T10:30:00.0000000Z, 2024-01-12T10:30:15.0000000Z] Histogram
Value: Sum: 4520.5 Count: 5 Min: 645.2 Max: 1234.8
(-Infinity,0]:0
(0,5]:0
(5,10]:0
...
(500,1000]:3
(1000,2500]:2
...

Export agent.tokens.prompt, Meter: Workshop.MAF.Agents/1.0.0
Value: 95

Export agent.tokens.completion, Meter: Workshop.MAF.Agents/1.0.0  
Value: 180
```

### 4.2 Interpretación de Métricas

| Métrica | Valor | Interpretación |
|---------|-------|----------------|
| `agent.invocations.total` | 5 | Se realizaron 5 llamadas al agente |
| `agent.latency` Sum | 4520.5ms | Latencia total acumulada |
| `agent.latency` Count | 5 | Número de mediciones |
| `agent.latency` Min/Max | 645/1234ms | Rango de latencias |
| `agent.tokens.prompt` | 95 | Tokens de entrada totales |
| `agent.tokens.completion` | 180 | Tokens de salida totales |

---

## Paso 5: Experimentar con las Métricas

### 5.1 Modificar las preguntas

Edita el array `questions` en `Program.cs` para agregar más consultas:

```csharp
var questions = new[]
{
    "¿Cuál es la capital de España?",
    "Dame un dato curioso sobre inteligencia artificial.",
    // Agrega más preguntas aquí:
    "Explica qué es Kubernetes en 50 palabras.",
    "¿Cuál es el sentido de la vida?",
};
```

### 5.2 Ejecutar y comparar

```bash
dotnet run
```

Observa cómo cambian las métricas con diferentes cantidades y tipos de preguntas.

---

## Validación del Laboratorio

### Criterios de Éxito

Marca cada criterio que hayas completado:

- [ ] El proyecto compila sin errores
- [ ] La aplicación se conecta a Azure OpenAI correctamente
- [ ] Las métricas se muestran en la consola
- [ ] Puedes identificar el contador `agent.invocations.total`
- [ ] Puedes identificar el histograma `agent.latency`
- [ ] Puedes identificar los contadores de tokens

### Preguntas de Comprensión

1. **¿Cuál es la diferencia entre un Counter y un Histogram?**
   - Counter: Solo aumenta, ideal para totales
   - Histogram: Distribución de valores, permite percentiles

2. **¿Por qué es importante monitorear tokens?**
   - Los tokens tienen costo directo ($)
   - Ayuda a optimizar prompts
   - Detecta anomalías de uso

3. **¿Qué atributo del Meter debe coincidir con AddMeter()?**
   - El nombre del Meter ("Workshop.MAF.Agents")

---

## Troubleshooting

### "Error: Configuración de Azure OpenAI no encontrada"

**Causa**: No se configuraron los user-secrets correctamente.

**Solución**:
```bash
dotnet user-secrets set "AzureOpenAI:ApiKey" "sk-..."
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://..."
```

### "Las métricas no aparecen en consola"

**Causa**: OpenTelemetry exporta métricas periódicamente (cada ~15s por defecto).

**Solución**: Espera unos segundos después de que termine el programa, o aumenta el `Task.Delay` al final.

### "401 Unauthorized al llamar a Azure OpenAI"

**Causa**: API key incorrecta o expirada.

**Solución**: Verifica el API key en el portal de Azure y actualiza el user-secret.

---

## Recursos Adicionales

- [System.Diagnostics.Metrics Overview](https://learn.microsoft.com/dotnet/core/diagnostics/metrics)
- [OpenTelemetry .NET Metrics](https://opentelemetry.io/docs/languages/net/instrumentation/#metrics)
- [Azure OpenAI Token Counting](https://learn.microsoft.com/azure/ai-services/openai/how-to/manage-costs)

---

## Siguiente Laboratorio

➡️ Continúa con [Lab 02: Trazas Distribuidas](../02-distributed-traces/) para aprender a rastrear el flujo de solicitudes a través de múltiples agentes.

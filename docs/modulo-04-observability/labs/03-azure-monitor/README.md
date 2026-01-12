# Lab 03: Integración con Azure Monitor

**Duración**: 30 minutos  
**Nivel**: Intermedio  
**Módulo**: 04 - Observabilidad

---

## Objetivos

Al completar este laboratorio, podrás:

1. ✅ Configurar exportación de telemetría a Azure Monitor Application Insights
2. ✅ Implementar logging estructurado con redacción de PII
3. ✅ Enviar métricas y trazas a Application Insights
4. ✅ Crear dashboards con Azure Workbooks
5. ✅ Configurar alertas basadas en métricas de agentes

---

## Conceptos Clave

### Azure Monitor Application Insights

**Application Insights** es un servicio de Azure para monitoreo de aplicaciones que incluye:

```text
┌─────────────────────────────────────────────────────────────┐
│                   APPLICATION INSIGHTS                       │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  📊 Métricas     → Gráficos de tiempo real                  │
│  🔗 Trazas       → Mapa de aplicación y dependencias        │
│  📝 Logs         → Búsqueda y análisis con KQL              │
│  ⚠️ Alertas      → Notificaciones automáticas               │
│  📈 Dashboards   → Workbooks personalizados                 │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

### Redacción de PII

**PII (Personally Identifiable Information)** nunca debe aparecer en logs:

| ❌ NO Registrar | ✅ Sí Registrar |
|----------------|-----------------|
| `usuario@email.com` | `[EMAIL_REDACTED]` |
| `Juan Pérez` | `user-12345` |
| `4111-1111-1111-1111` | `[CC_REDACTED]` |
| `+34 612 345 678` | `[PHONE_REDACTED]` |
| Mensaje completo del usuario | Hash del mensaje |

---

## Paso 1: Crear Application Insights

### 1.1 En el Portal de Azure

1. Ve a [portal.azure.com](https://portal.azure.com)
2. Busca "Application Insights" en la barra de búsqueda
3. Haz clic en **+ Crear**
4. Configura:
   - **Subscription**: Tu suscripción
   - **Resource Group**: Crea uno nuevo o usa existente
   - **Name**: `workshop-maf-appinsights`
   - **Region**: La más cercana a ti
   - **Resource Mode**: Workspace-based (recomendado)

5. Haz clic en **Revisar + Crear** → **Crear**

### 1.2 Obtener el Connection String

1. Una vez creado, abre el recurso
2. En el panel **Información general**, copia el **Connection String**
3. Se verá algo como:
   ```
   InstrumentationKey=abc123...;IngestionEndpoint=https://westeurope.in.applicationinsights.azure.com/
   ```

---

## Paso 2: Configurar el Proyecto

### 2.1 Navega al directorio del laboratorio

```bash
cd docs/modulo-04-observability/labs/03-azure-monitor
```

### 2.2 Restaura los paquetes

```bash
dotnet restore
```

### 2.3 Configura las credenciales

```bash
# Inicializar user-secrets
dotnet user-secrets init

# Configurar Azure OpenAI
dotnet user-secrets set "AzureOpenAI:ApiKey" "tu-api-key"
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://tu-recurso.openai.azure.com/"

# Configurar Application Insights
dotnet user-secrets set "ApplicationInsights:ConnectionString" "tu-connection-string"
```

---

## Paso 3: Entender el Código

### 3.1 Configuración del Exportador

Abre `Program.cs` y observa la configuración:

```csharp
// Configurar Métricas con Azure Monitor
otelBuilder.WithMetrics(metrics =>
{
    metrics
        .SetResourceBuilder(resourceBuilder)
        .AddMeter("Workshop.MAF.Agents")
        .AddRuntimeInstrumentation()
        .AddHttpClientInstrumentation();
    
    if (useAzureMonitor)
    {
        // Exportar a Azure Monitor
        metrics.AddAzureMonitorMetricExporter(options =>
        {
            options.ConnectionString = appInsightsConnectionString;
        });
    }
});
```

### 3.2 Redacción de PII

Observa la clase `PiiRedactor`:

```csharp
public class PiiRedactor
{
    private static readonly Regex EmailRegex = 
        new(@"\b[\w\.-]+@[\w\.-]+\.\w{2,}\b");
    
    public string RedactPii(string text)
    {
        var result = text;
        result = EmailRegex.Replace(result, "[EMAIL_REDACTED]");
        return result;
    }
    
    public string HashString(string input)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash)[..12].ToLowerInvariant();
    }
}
```

### 3.3 Logging Estructurado

Observa cómo se usa logging con scopes:

```csharp
using (logger.BeginScope(new Dictionary<string, object>
{
    ["UserId"] = userId,
    ["QueryHash"] = piiRedactor.HashString(query),
    ["SessionId"] = sessionId
}))
{
    logger.LogInformation(
        "Query procesada para usuario {UserId}",
        userId);
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

Si configuraste Application Insights correctamente:

```text
╔══════════════════════════════════════════════════════════════════╗
║        Lab 03: Integración con Azure Monitor                     ║
╚══════════════════════════════════════════════════════════════════╝

📊 Métricas: Exportando a Azure Monitor
🔗 Trazas: Exportando a Azure Monitor
📝 Logs: Exportando a Azure Monitor

═══════════════════════════════════════════════════════════

🚀 Ejecutando demostración con telemetría completa...

👤 Usuario: user-001
📝 Query (redactado): ¿Cuál es la capital de Francia?
✅ Respuesta: La capital de Francia es París...

👤 Usuario: user-002
📝 Query (redactado): Mi email es [EMAIL_REDACTED] y necesito ayuda
✅ Respuesta: Por supuesto, estaré encantado de ayudarte...

...

📊 Resumen de Telemetría Generada:
   • Total invocaciones: 4
   • Total errores: 1
   • Tokens consumidos: 520
```

---

## Paso 5: Ver Datos en Azure Monitor

### 5.1 Explorar Métricas

1. En el portal de Azure, abre tu Application Insights
2. Ve a **Métricas** en el menú lateral
3. Selecciona el namespace **azure.applicationinsights**
4. Busca métricas como:
   - `agent.invocations.total`
   - `agent.latency`
   - `agent.tokens.total`

### 5.2 Ejecutar Queries KQL

1. Ve a **Logs** en el menú lateral
2. Ejecuta estas queries:

**Invocaciones por usuario:**
```kusto
customMetrics
| where name == "agent.invocations.total"
| summarize TotalInvocations = sum(value) by tostring(customDimensions.user_id)
| order by TotalInvocations desc
```

**Latencia por tiempo:**
```kusto
customMetrics
| where name == "agent.latency"
| summarize 
    p50 = percentile(value, 50),
    p95 = percentile(value, 95),
    p99 = percentile(value, 99)
    by bin(timestamp, 5m)
| render timechart
```

**Errores recientes:**
```kusto
traces
| where severityLevel >= 3
| where timestamp > ago(1h)
| project timestamp, message, customDimensions.UserId
| order by timestamp desc
```

### 5.3 Importar el Workbook

1. Ve a **Workbooks** en el menú lateral
2. Haz clic en **+ Nuevo**
3. Haz clic en el icono **</>** (Editor avanzado)
4. Copia el contenido de `workbook-template.json`
5. Pega y haz clic en **Aplicar**
6. Guarda el workbook

---

## Paso 6: Configurar Alertas

### 6.1 Alerta de Latencia Alta

1. Ve a **Alertas** → **+ Crear** → **Regla de alerta**
2. Selecciona tu Application Insights como recurso
3. Condición:
   - Señal: `customMetrics` / `agent.latency`
   - Lógica: `Promedio` > `5000` (ms)
   - Periodo: `5 minutos`
4. Acciones: Configura un grupo de acciones (email, SMS, webhook)
5. Nombre: "MAF Agent - Latencia Alta"

### 6.2 Alerta de Errores

1. Crea otra regla de alerta
2. Condición:
   - Señal: `customMetrics` / `agent.invocations.errors`
   - Lógica: `Suma` > `5` errores en `15 minutos`
3. Nombre: "MAF Agent - Tasa de Errores"

### 6.3 Alerta de Tokens

1. Crea alerta de presupuesto de tokens
2. Condición:
   - Señal: `customMetrics` / `agent.tokens.total`
   - Lógica: `Suma` > `80000` en `24 horas`
3. Nombre: "MAF Agent - Presupuesto de Tokens"

---

## Validación del Laboratorio

### Criterios de Éxito

- [ ] Application Insights recibe métricas del agente
- [ ] Los logs muestran PII redactada
- [ ] Las queries KQL devuelven datos
- [ ] El workbook muestra gráficos con datos
- [ ] Al menos una alerta está configurada

### Preguntas de Comprensión

1. **¿Por qué usamos Connection String en lugar de solo Instrumentation Key?**
   - Connection String incluye la región de ingesta, es más completo y soporta nuevas características.

2. **¿Cuál es el beneficio de usar logging estructurado con scopes?**
   - Todos los logs dentro del scope incluyen automáticamente el contexto (UserId, SessionId), facilitando la correlación.

3. **¿Por qué hasheamos el contenido del query en lugar de registrarlo?**
   - El hash permite correlacionar queries similares sin exponer el contenido potencialmente sensible del usuario.

---

## Troubleshooting

### "Los datos no aparecen en Application Insights"

**Causa**: Delay de ingesta (puede tardar 2-5 minutos).

**Solución**: Espera unos minutos y actualiza. Verifica que el Connection String sea correcto.

### "customMetrics está vacío"

**Causa**: El Meter no está registrado o los nombres no coinciden.

**Solución**: Verifica que `AddMeter("Workshop.MAF.Agents")` coincide con el nombre en `new Meter("Workshop.MAF.Agents")`.

### "Los logs no muestran customDimensions"

**Causa**: `IncludeScopes` no está habilitado.

**Solución**: Asegura que tienes `logging.IncludeScopes = true` en la configuración.

---

## Queries KQL de Referencia

### Dashboard Ejecutivo

```kusto
// Resumen de las últimas 24 horas
let timeRange = ago(24h);
customMetrics
| where timestamp > timeRange
| summarize 
    TotalInvocaciones = sumif(value, name == "agent.invocations.total"),
    TotalErrores = sumif(value, name == "agent.invocations.errors"),
    TokensConsumidos = sumif(value, name == "agent.tokens.total"),
    LatenciaPromedio = avgif(value, name == "agent.latency")
| extend ErrorRate = 100.0 * TotalErrores / TotalInvocaciones
```

### Análisis de Costos

```kusto
// Costo estimado de tokens (ajustar precios según modelo)
let tokenPriceInput = 0.00003;  // $/token para input
let tokenPriceOutput = 0.00006; // $/token para output
customMetrics
| where name in ("agent.tokens.prompt", "agent.tokens.completion")
| summarize TotalTokens = sum(value) by name
| extend CostoEstimado = case(
    name == "agent.tokens.prompt", TotalTokens * tokenPriceInput,
    name == "agent.tokens.completion", TotalTokens * tokenPriceOutput,
    0)
```

---

## Recursos Adicionales

- [Azure Monitor OpenTelemetry Exporter](https://learn.microsoft.com/azure/azure-monitor/app/opentelemetry-enable)
- [KQL Quick Reference](https://learn.microsoft.com/azure/data-explorer/kql-quick-reference)
- [Azure Workbooks](https://learn.microsoft.com/azure/azure-monitor/visualize/workbooks-overview)
- [Application Insights Alerts](https://learn.microsoft.com/azure/azure-monitor/alerts/alerts-overview)

---

## Resumen del Módulo

Has completado los tres laboratorios de Observabilidad:

| Lab | Habilidades Adquiridas |
|-----|------------------------|
| 01 | Métricas con OpenTelemetry, contadores, histogramas |
| 02 | Trazas distribuidas, spans jerárquicos, propagación de contexto |
| 03 | Azure Monitor, logging estructurado, PII redaction, dashboards, alertas |

**Checkpoint final**: ¿Puedes explicar cómo fluye la telemetría desde tu agente hasta un dashboard en Azure Monitor?

---

## Siguiente Módulo

➡️ Continúa con [Módulo 5: ASP.NET + Aspire](../../modulo-05-aspnet-aspire/) para integrar agentes en aplicaciones web con observabilidad completa.

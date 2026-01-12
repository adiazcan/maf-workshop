# Guía del Instructor - Módulo 4: Observabilidad

**Duración Total**: 75 minutos  
**Nivel**: Intermedio  
**Prerequisitos**: Módulos 1-3 completados

---

## Resumen del Módulo

Este módulo enseña los fundamentos de la observabilidad en aplicaciones de agentes IA, cubriendo métricas, trazas y logs con OpenTelemetry y Azure Monitor.

### Objetivos de Aprendizaje

Al finalizar, los participantes podrán:
- Explicar los tres pilares de la observabilidad
- Configurar OpenTelemetry en aplicaciones .NET
- Implementar métricas personalizadas para agentes
- Crear trazas distribuidas para workflows multi-agente
- Exportar telemetría a Azure Monitor
- Crear dashboards y configurar alertas

---

## Agenda Detallada

| Tiempo | Actividad | Duración |
|--------|-----------|----------|
| 0:00 | Teoría: Introducción a Observabilidad | 15 min |
| 0:15 | Demostración: OpenTelemetry básico | 5 min |
| 0:20 | **Lab 01**: Métricas de Tokens | 20 min |
| 0:40 | Pausa corta | 5 min |
| 0:45 | **Lab 02**: Trazas Distribuidas | 25 min |
| 1:10 | **Lab 03**: Azure Monitor | 30 min |
| 1:40 | Checkpoint y Q&A | 10 min |
| **Total** | | **85 min** |

> **Nota**: Si el tiempo es limitado, Lab 03 puede hacerse como demostración (15 min) en lugar de hands-on.

---

## Preparación Pre-Módulo

### Requisitos de Azure

- [ ] Application Insights creado y accesible
- [ ] Participantes tienen permisos para ver métricas y logs
- [ ] Connection String de App Insights disponible para compartir (opcional)

### Materiales Necesarios

- [ ] Slides de los tres pilares de observabilidad
- [ ] Diagrama de flujo: Agente → OpenTelemetry → Azure Monitor
- [ ] Terminal lista para demostración
- [ ] Azure Portal abierto con App Insights (para Lab 03)

### Verificación Rápida

```bash
# En el directorio de cualquier lab de este módulo
cd docs/modulo-04-observability/labs/01-metrics-tokens
dotnet build
# Debe compilar sin errores
```

---

## Sección por Sección

### Teoría: Introducción a Observabilidad (15 min)

**Puntos clave a cubrir:**

1. **¿Por qué es diferente observar agentes IA?** (3 min)
   - No determinísticos
   - Costos por token
   - Cadenas de decisión complejas

2. **Los tres pilares** (7 min)
   - **Métricas**: ¿Qué está pasando? (números agregados)
   - **Trazas**: ¿Cómo fluye? (seguir una solicitud)
   - **Logs**: ¿Por qué sucedió? (contexto detallado)

3. **OpenTelemetry como estándar** (3 min)
   - Vendor-neutral
   - Integración nativa en .NET
   - Un SDK, múltiples backends

4. **Vista previa de Azure Monitor** (2 min)
   - Mostrar screenshot del dashboard
   - Mencionar KQL para queries

**Preguntas de engagement:**
- "¿Quién ha usado Application Insights antes?"
- "¿Qué métricas creen que son importantes para un agente de IA?"

---

### Demostración: OpenTelemetry Básico (5 min)

Mostrar código mínimo de configuración:

```csharp
builder.Services.AddOpenTelemetry()
    .WithMetrics(m => m
        .AddMeter("MyApp")
        .AddConsoleExporter())
    .WithTracing(t => t
        .AddSource("MyApp")
        .AddConsoleExporter());
```

Ejecutar brevemente para mostrar la salida en consola.

---

### Lab 01: Métricas de Tokens (20 min)

**Objetivo**: Participantes entienden cómo medir latencia y tokens.

**Setup** (2 min):
```bash
cd docs/modulo-04-observability/labs/01-metrics-tokens
dotnet restore
```

**Instrucciones para participantes** (15 min):
1. Configurar user-secrets con API key
2. Revisar brevemente el código de `AgentMetricsService`
3. Ejecutar y observar métricas en consola

**Checkpoint** (3 min):
- "Levanten la mano si ven `agent.invocations.total` en la salida"
- Meta: 80% de participantes

**Problemas comunes:**
| Problema | Solución |
|----------|----------|
| "No veo métricas" | Esperar 10-15 segundos después de ejecución |
| "Error 401" | Verificar API key |
| "Meter no registrado" | Verificar nombre en `AddMeter()` |

---

### Pausa (5 min)

Aprovechar para:
- Responder preguntas pendientes
- Verificar que participantes rezagados alcancen

---

### Lab 02: Trazas Distribuidas (25 min)

**Objetivo**: Participantes entienden la jerarquía de spans.

**Concepto clave a enfatizar:**
```
Workflow (span padre)
  └─ WeatherAgent (span hijo)
      └─ HTTP Call (span nieto, auto-instrumentado)
```

**Instrucciones para participantes** (20 min):
1. Configurar user-secrets
2. Revisar cómo se crea el span padre
3. Observar cómo los spans hijos heredan el TraceId
4. Ejecutar y analizar la salida

**Checkpoint** (5 min):
- "¿Quién puede identificar el ParentId en un span hijo?"
- Pedir a 2 participantes que compartan pantalla y muestren su salida
- Meta: 75% de participantes completan

**Preguntas de refuerzo:**
- "¿Por qué todos los spans tienen el mismo TraceId?"
- "¿Qué pasa con el ParentId del span raíz?"

---

### Lab 03: Azure Monitor (30 min)

> **Decisión del instructor**: Si el tiempo es limitado, hacer como demostración (15 min).

**Opción A: Hands-on completo (30 min)**

Participantes necesitan:
- Acceso a Azure Portal
- Permisos para crear/ver Application Insights

**Setup** (5 min):
1. Crear Application Insights (si no existe)
2. Copiar Connection String
3. Configurar en user-secrets

**Ejecución** (15 min):
1. Ejecutar la aplicación
2. Esperar 2-3 minutos para ingesta
3. Ver métricas en Azure Portal
4. Ejecutar una query KQL básica

**Dashboard** (10 min):
1. Importar workbook-template.json
2. Explorar los gráficos generados
3. (Opcional) Configurar una alerta simple

**Opción B: Demostración (15 min)**

El instructor:
1. Muestra su App Insights con datos reales
2. Ejecuta queries KQL en vivo
3. Muestra el workbook importado
4. Explica cómo configurar alertas (sin hacerlo)

Participantes:
1. Siguen en sus pantallas
2. Toman notas de los pasos
3. Intentan replicar después del workshop

---

### Checkpoint Final y Q&A (10 min)

**Validación del módulo:**

1. "¿Quién puede explicar los tres pilares?" (pedir voluntario)
2. "¿Cuál es la diferencia entre tags y baggage en trazas?"
3. "¿Por qué es importante redactar PII?"

**Encuesta rápida:**
- "¿Quién completó Lab 01?" (meta: 90%)
- "¿Quién completó Lab 02?" (meta: 80%)
- "¿Quién completó o vio Lab 03?" (meta: 70%)

**Preguntas frecuentes:**

| Pregunta | Respuesta |
|----------|-----------|
| "¿Cuánto cuesta App Insights?" | Modelo basado en ingesta. ~$2.30/GB para logs. Métricas tienen tier gratuito. |
| "¿Puedo usar Prometheus en vez de Azure?" | Sí, solo cambiar el exporter |
| "¿Cómo evito costos altos de logs?" | Sampling, redacción de datos innecesarios, alertas de presupuesto |

---

## Ajustes de Tiempo

### Si vas adelantado (+10 min)

- Profundizar en KQL con más queries
- Mostrar Mapa de Aplicación en App Insights
- Discutir patrones avanzados (circuit breaker observability)

### Si vas atrasado (-10 min)

- Lab 03 como demostración en lugar de hands-on
- Reducir tiempo de Q&A
- Omitir importación de workbook (solo mostrar)

### Si participantes están atascados

- Compartir pantalla y guiar paso a paso
- Pedir a participantes que completaron que ayuden a otros
- Proporcionar código funcionando si es necesario

---

## Recursos para Compartir

### Después del módulo, enviar:

1. Links a documentación oficial
   - [OpenTelemetry .NET](https://opentelemetry.io/docs/languages/net/)
   - [Azure Monitor Exporter](https://learn.microsoft.com/azure/azure-monitor/app/opentelemetry-enable)

2. Queries KQL de referencia (del Lab 03 README)

3. Workbook template JSON para importar

---

## Transición al Siguiente Módulo

**Conexión con Módulo 5 (ASP.NET + Aspire):**

"Ahora que saben cómo instrumentar agentes para observabilidad, en el siguiente módulo veremos cómo integrar todo esto en una aplicación web con ASP.NET y .NET Aspire, que incluye observabilidad automática desde el dashboard de Aspire."

---

## Notas del Instructor

### Conceptos que suelen confundir:

1. **Meter vs ActivitySource**
   - Meter → Métricas (contadores, histogramas)
   - ActivitySource → Trazas (spans)

2. **Tags vs Baggage**
   - Tags: solo para el span actual
   - Baggage: se propaga a TODOS los spans descendientes

3. **Counter vs Histogram**
   - Counter: solo suma (monotónico)
   - Histogram: distribución, permite percentiles

### Demo tips:

- Tener una terminal con `dotnet watch run` lista
- Pre-cargar Azure Portal con App Insights abierto
- Tener queries KQL copiadas para pegar rápido

### Engagement tips:

- Preguntar "¿Qué métricas agregarían ustedes?" durante Lab 01
- Pedir que identifiquen el problema en logs mal formados
- Mostrar un ejemplo de alerta disparándose (si es posible)

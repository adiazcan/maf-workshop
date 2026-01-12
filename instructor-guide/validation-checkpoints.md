# Validation Checkpoints: Microsoft Agent Framework Workshop

**Propósito**: Definir criterios claros de éxito para cada módulo y métodos de validación manual del instructor.

**Filosofía**: Validación rápida (1-5 min) para mantener cohesión del grupo sin retrasar a participantes avanzados.

---

## 🎯 Checkpoint Framework

Cada checkpoint incluye:

1. **Criterio de Éxito**: Qué debe funcionar
2. **Método de Validación**: Cómo verificar
3. **Threshold de Éxito**: % mínimo de participantes que deben completar
4. **Tiempo de Validación**: Duración del checkpoint
5. **Acciones de Remediación**: Qué hacer si <threshold

---

## Módulo 1: Fundamentos

### Checkpoint 1.1: Hello Agent Funcional

**Criterio de Éxito**:
> El agente responde a "Hola" con una respuesta coherente y mantiene contexto en una conversación de 2-3 turnos.

**Método de Validación**: Show of Hands + Screen Share

```
Instructor: "¿Quién vio una respuesta del agente a 'Hola'?"
[Contar manos levantadas]

Instructor: "Alguien que pueda compartir su pantalla y mostrar la salida?"
[1-2 participantes comparten screen]

Validar:
✅ Console muestra: "🤖 Agent: [Respuesta coherente]"
✅ No hay exceptions en rojo
✅ La respuesta es relevante al saludo
```

**Threshold de Éxito**: 90% de participantes

**Tiempo**: 3-5 minutos

**Remediación si <90%**:
1. Revisar errores más comunes:
   - "dotnet command not found" → Verificar PATH
   - "401 Unauthorized" → Verificar API key en user-secrets
   - "Agent no responde" → Verificar deployment name correcto
2. Pedir a participantes exitosos que ayuden a su vecino
3. Continuar si ≥75% completan (enviar troubleshooting doc a los demás)

---

## Módulo 2: Function Tools

### Checkpoint 2.1: Function Tool Invocada Automáticamente

**Criterio de Éxito**:
> El agente llama automáticamente a `GetWeather()` cuando el usuario pregunta sobre clima, sin invocación manual.

**Método de Validación**: Patrón de Output + Screen Share

```
Instructor: "En la consola, deberían ver algo como:"
```
Function call: get_weather(city="Madrid", country="ES")
Function result: "Soleado, 22°C"
Agent response: "El clima en Madrid está..."
```

Instructor: "¿Quién ve 'Function call:' en su output?"
[Contar manos]

Instructor: "Alguien comparta pantalla para confirmar"
[Screen share de 2 participantes]
```

**Threshold de Éxito**: 85% de participantes

**Tiempo**: 4-5 minutos

**Remediación si <85%**:
1. Error común #1: Falta `[KernelFunction]` attribute
   ```csharp
   [KernelFunction("get_weather")]  // ← Verificar esto
   [Description("Gets current weather...")]
   public string GetWeather(...) { }
   ```

2. Error común #2: Función no registrada
   ```csharp
   builder.Plugins.AddFromType<WeatherService>();  // ← Verificar esto
   ```

3. Error común #3: Descripción ambigua
   - Mejorar [Description] para ser más explícito

---

### Checkpoint 2.2: Composición de Agentes

**Criterio de Éxito**:
> El MainAgent delega automáticamente preguntas matemáticas al CalculatorAgent.

**Método de Validación**: Output Verification + Pair Check

```
Instructor: "Ejecuten la pregunta: '¿Cuánto es 25 * 48?'"

Instructor: "Deberían ver:"
```
MainAgent: Calling CalculatorAgent...
CalculatorAgent: 25 * 48 = 1200
MainAgent: El resultado es 1200
```

Instructor: "Trabajen en pares: verifiquen el output de su compañero"
[2 minutos de pair checking]

Instructor: "¿Cuántas parejas confirmaron que funciona?"
[Contar parejas]
```

**Threshold de Éxito**: 85% de participantes

**Tiempo**: 3-4 minutos

**Remediación si <85%**:
1. Verificar que CalculatorAgent está registrado como función
2. Verificar que MainAgent tiene acceso al kernel con plugins
3. Demo rápida del instructor (2 min)

---

## Módulo 3: Workflows

### Checkpoint 3.1: Workflow Secuencial

**Criterio de Éxito**:
> El workflow ejecuta 3 pasos en orden: Research → Write → Review, pasando datos entre pasos.

**Método de Validación**: Log Analysis + Show of Hands

```
Instructor: "En su consola, busquen estas 3 líneas en orden:"
```
[Step 1] ResearchAgent completed: [datos de investigación]
[Step 2] WritingAgent completed: [borrador]
[Step 3] ReviewAgent completed: [documento final]
```

Instructor: "¿Quién ve las 3 líneas en ese orden?"
[Contar manos]

Instructor: "¿Alguien ve los pasos en orden diferente?"
[Identificar errores de sincronización]
```

**Threshold de Éxito**: 80% de participantes

**Tiempo**: 4-5 minutos

**Remediación si <80%**:
1. Error común: Ejecución paralela accidental
   - Verificar que no hay `Task.WhenAll()` en workflow secuencial
2. Error común: Datos no se pasan entre pasos
   - Verificar que output de Paso N se incluye en input de Paso N+1

---

### Checkpoint 3.2: Workflow Paralelo

**Criterio de Éxito**:
> 3 agentes ejecutan simultáneamente (NewsAgent, WeatherAgent, StocksAgent) y resultados se agregan.

**Método de Validación**: Timing Analysis + Output Check

```
Instructor: "Noten el tiempo total de ejecución en su consola:"
```
[INFO] Starting parallel execution...
[INFO] All agents completed in 3.2 seconds
```

Instructor: "Si fuera secuencial, tomaría ~9 segundos (3 agentes × 3s cada uno)"
Instructor: "¿Quién completó en menos de 5 segundos?"
[Contar manos - confirma ejecución paralela]

Instructor: "¿Quién tiene los 3 resultados agregados en la respuesta final?"
[Screen share de 1 participante]
```

**Threshold de Éxito**: 80% de participantes

**Tiempo**: 3-4 minutos

**Remediación si <80%**:
1. Verificar `Task.WhenAll()` en lugar de `await` secuencial
2. Demo rápida de timing: secuencial vs paralelo

---

### Checkpoint 3.3: Azure AI Agent Service (Opcional/Demo)

**Criterio de Éxito**:
> Thread persiste, se puede pausar aplicación y reanudarla.

**Método de Validación**: Instructor Demo (si tiempo permite)

```
Instructor: "Voy a demostrar pause/resume:"
1. Crear thread y enviar mensaje
2. Copiar thread ID
3. Cerrar aplicación
4. Reabrir aplicación
5. Usar thread ID para continuar conversación

[Demo 5 minutos]

Instructor: "Este lab es material de referencia para ustedes. Quien quiera implementarlo después del workshop, tienen el código completo."
```

**Threshold**: N/A (conceptual)

**Tiempo**: 5-7 minutos (demo únicamente)

---

## Módulo 4: Observabilidad

### Checkpoint 4.1: Métricas Visibles

**Criterio de Éxito**:
> Métricas de tokens y latencia se muestran en la consola.

**Método de Validación**: Console Output + Pattern Match

```
Instructor: "En la consola, busquen líneas como estas:"
```
[METRIC] llm.token.usage: 245 tokens
[METRIC] agent.invocation.duration: 2.3 seconds
```

Instructor: "¿Quién ve métricas en su consola?"
[Show of hands]

Instructor: "Para quienes no ven nada:"
```csharp
metrics.AddConsoleExporter();  // ← Verificar esta línea
```
```

**Threshold de Éxito**: 75% de participantes

**Tiempo**: 3-4 minutos

**Remediación si <75%**:
1. Verificar que `AddConsoleExporter()` está presente
2. Verificar que `AddMeter("Microsoft.AI.Agents*")` está configurado
3. Demo del instructor (1 min)

---

### Checkpoint 4.2: Traces Distribuidas

**Criterio de Éxito**:
> Traces muestran jerarquía de llamadas: MainAgent → SubAgent → Azure OpenAI.

**Método de Validación**: Trace Visualization + Screen Share

```
Instructor: "Busquen un trace en su consola con estructura jerárquica:"
```
│ MainAgent.Invoke (300ms)
│  ├─ WeatherAgent.Invoke (150ms)
│  │  └─ Azure OpenAI Call (120ms)
│  └─ NewsAgent.Invoke (140ms)
│     └─ Azure OpenAI Call (110ms)
```

Instructor: "Alguien comparta pantalla mostrando la jerarquía"
[Screen share de 1-2 participantes]
```

**Threshold de Éxito**: 75% de participantes

**Tiempo**: 4-5 minutos

**Remediación si <75%**:
1. Verificar `AddHttpClientInstrumentation()` para traces de HTTP
2. Verificar `AddSource("Microsoft.AI.Agents*")` para traces de agentes

---

### Checkpoint 4.3: Azure Monitor (Opcional/Demo)

**Criterio de Éxito**:
> Dashboard en Azure Monitor muestra métricas del agente.

**Método de Validación**: Instructor Demo

```
Instructor: [Compartir pantalla de Azure Portal]
1. Abrir Application Insights
2. Ir a "Metrics"
3. Mostrar gráfico de token usage
4. Mostrar gráfico de latency

[Demo 5 minutos]

Instructor: "El código para configurar esto está en el lab 03. Quien quiera implementarlo, está todo documentado."
```

**Threshold**: N/A (conceptual)

**Tiempo**: 5 minutos (demo únicamente)

---

## Módulo 5: ASP.NET + Aspire

### Checkpoint 5.1: API Responde

**Criterio de Éxito**:
> API en `http://localhost:5000/api/chat/weather` responde a POST request.

**Método de Validación**: cURL Test + Swagger UI

```
Instructor: "Abran http://localhost:5000/swagger en su navegador"

Instructor: "¿Quién ve la interfaz de Swagger con endpoints /api/chat/weather y /api/chat/summary?"
[Show of hands]

Instructor: "Desde Swagger, prueben enviar:"
```json
{
  "city": "Madrid"
}
```

Instructor: "¿Quién recibió una respuesta JSON del agente?"
[Screen share de 1 participante mostrando Swagger response]
```

**Threshold de Éxito**: 70% de participantes

**Tiempo**: 5-6 minutos

**Remediación si <70%**:
1. Verificar que `dotnet run --project AppHost` está corriendo
2. Verificar puerto (puede ser 5000, 5001, o diferente)
3. Verificar que Aspire Dashboard muestra "webapi" como Running
4. Demo rápida del instructor (2 min)

---

### Checkpoint 5.2: Aspire Dashboard Visible

**Criterio de Éxito**:
> Aspire Dashboard en `http://localhost:15888` muestra logs y traces.

**Método de Validación**: Dashboard Navigation + Screen Share

```
Instructor: "Abran http://localhost:15888"

Instructor: "¿Quién ve el Aspire Dashboard con tabs: Resources, Logs, Traces, Metrics?"
[Show of hands]

Instructor: "Naveguen a 'Traces' y busquen un trace de su API call"
[2 minutos de exploración]

Instructor: "Alguien comparta pantalla mostrando un trace"
[Screen share mostrando trace de /api/chat/weather]
```

**Threshold de Éxito**: 70% de participantes

**Tiempo**: 4-5 minutos

**Remediación si <70%**:
1. Verificar que AppHost está corriendo (no solo WebApi)
2. Verificar puerto de Aspire Dashboard (puede variar)
3. Demo del instructor navegando el dashboard (2 min)

---

## Módulo 6: DevUI

### Checkpoint 6.1: DevUI Conectado (Demo del Instructor)

**Criterio de Éxito**:
> DevUI muestra conversación con el agente en tiempo real.

**Método de Validación**: Instructor Demo Únicamente

```
Instructor: [Compartir pantalla]
1. Ejecutar `devui start`
2. Abrir http://localhost:5100
3. Conectar a agente
4. Enviar mensaje de prueba
5. Mostrar visualización de function call

[Demo 10 minutos con Q&A]

Instructor: "DevUI es para debugging local. El código de configuración está en el lab. Quien quiera probarlo, pueden hacerlo después."
```

**Threshold**: 100% (observación únicamente, no requiere implementación)

**Tiempo**: 10-12 minutos (demo + Q&A)

---

## Módulo 7: MCP

### Checkpoint 7.1: Comprensión Conceptual de MCP

**Criterio de Éxito**:
> Participantes pueden explicar qué es MCP y dar 1 ejemplo de uso.

**Método de Validación**: Q&A Oral

```
Instructor: "En una frase, ¿qué es Model Context Protocol?"
[Seleccionar 3-4 participantes aleatoriamente para responder]

Respuestas aceptables:
- "Un estándar para conectar agentes con datos/herramientas"
- "Protocolo para interoperabilidad entre frameworks de agentes"
- "API unificada para que agentes accedan a recursos externos"

Instructor: "¿Alguien puede dar un caso de uso donde MCP sea útil?"
[2-3 ejemplos de participantes]

Ejemplos válidos:
- "Reutilizar integraciones en múltiples frameworks"
- "Marketplace de herramientas para agentes"
- "Separar lógica de negocio de implementación de agentes"
```

**Threshold de Éxito**: 80% comprenden concepto

**Tiempo**: 5 minutos

**Remediación si <80%**:
- Ejemplo concreto: "Imaginen que construyen integración de Salesforce como MCP Server. Funciona con Copilot, Claude, Gemini sin cambios."

---

## 📊 Tracking de Checkpoints

### Formulario de Tracking (para instructor)

```markdown
| Módulo | Checkpoint | Meta | Actual | Notas |
|--------|------------|------|--------|-------|
| M1 | Hello Agent | 90% | ___% | ________________ |
| M2 | Function Tool | 85% | ___% | ________________ |
| M2 | Composición | 85% | ___% | ________________ |
| M3 | Sequential | 80% | ___% | ________________ |
| M3 | Parallel | 80% | ___% | ________________ |
| M4 | Metrics | 75% | ___% | ________________ |
| M4 | Traces | 75% | ___% | ________________ |
| M5 | API | 70% | ___% | ________________ |
| M5 | Aspire | 70% | ___% | ________________ |
| M6 | DevUI | 100% | ___% | ________________ |
| M7 | MCP | 80% | ___% | ________________ |
```

**Análisis Post-Workshop**:
- Identificar checkpoints con <threshold → Mejorar para próxima iteración
- Documentar errores comunes → Actualizar troubleshooting guide
- Ajustar timing si consistentemente adelantado/retrasado

---

## 🚨 Estrategias de Remediación Rápida

### Cuando <Threshold de Participantes Completan

#### Opción 1: Peer Support (2-3 min)
```
"Quienes completaron exitosamente, por favor ayuden a su vecino a resolver el error. Tienen 3 minutos."
```

#### Opción 2: Common Error Demo (2 min)
```
"Veo que muchos tienen el mismo error. Permítanme mostrar en pantalla cómo resolverlo..."
[Screen share del instructor, fix en vivo]
```

#### Opción 3: Continue + Async Support (0 min overhead)
```
"Para quienes aún tienen problemas, voy a compartir un documento de troubleshooting en el chat. Pueden continuar con el siguiente módulo y resolver esto durante el break."
```

#### Opción 4: Skip to Next + Mark Optional (0 min overhead)
```
"Este lab es opcional. El código completo está en el repositorio. Continuemos con el siguiente módulo que no depende de este."
```

---

## ✅ Best Practices para Checkpoints

### Do's ✅

- **Ser específico**: "¿Quién ve 'Function call:' en su consola?" (no "¿Quién terminó?")
- **Usar métodos variados**: Show of hands, screen share, pair check, output verification
- **Celebrar éxito**: "¡Excelente! Veo que 25 de 28 completaron. Eso es 89%."
- **Ser visual**: Proyectar output esperado en pantalla
- **Dar tiempo**: 30 segundos para que participantes verifiquen antes de preguntar

### Don'ts ❌

- **No hacer checkpoints demasiado largos**: >5 min retrasa el flujo
- **No esperar 100%**: Threshold realistas (70-90%)
- **No resolver 1-a-1 durante checkpoint**: Delegar a peer support o async
- **No avanzar si <50% completan**: Algo está mal con el lab
- **No ignorar patterns**: Si mismo error se repite, hay problema sistémico

---

## 📋 Checklist de Validación

- [ ] Criterios de éxito definidos para cada checkpoint
- [ ] Métodos de validación preparados
- [ ] Outputs esperados proyectables en pantalla
- [ ] Formulario de tracking impreso
- [ ] Estrategias de remediación listas

---

**Versión**: 1.0  
**Última actualización**: 2026-01-12  
**Instructor**: _______________

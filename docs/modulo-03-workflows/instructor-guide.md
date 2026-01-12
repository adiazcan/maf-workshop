# Guía del Instructor: Módulo 3 - Workflows

**Duración total**: 135 minutos (2h 15min)  
**Nivel**: Intermedio-Avanzado  
**Prerequisitos para participantes**: Módulos 1 y 2 completados

## Resumen del Módulo

Este módulo cubre los 4 patrones principales de orquestación multi-agente más la persistencia con Azure AI Agent Service. Es el módulo más extenso del workshop y requiere buena gestión del tiempo.

## Distribución de Tiempo

| Actividad | Duración | Acumulado |
|-----------|----------|-----------|
| Introducción teórica (README.md) | 20 min | 20 min |
| Lab 01: Sequential Workflow | 20 min | 40 min |
| Lab 02: Parallel Workflow | 25 min | 65 min |
| **CHECKPOINT 1** | 5 min | 70 min |
| Lab 03: Delegation Workflow | 25 min | 95 min |
| Lab 04: Group Chat | 30 min | 125 min |
| **CHECKPOINT 2** | 5 min | 130 min |
| Lab 05: Azure AI Agent Service | 35 min | 165 min |
| **CHECKPOINT 3** | 5 min | 170 min |

**Nota**: El tiempo total de 170 min incluye checkpoints. Ajustar si es necesario eliminando Lab 05 para workshops más cortos.

## Preparación Pre-Workshop

### Azure AI Foundry (para Lab 05)

⚠️ **CRÍTICO**: El Lab 05 requiere Azure AI Foundry configurado. Verificar ANTES del workshop:

1. **Proyecto de Azure AI Foundry creado**
   - Ir a [ai.azure.com](https://ai.azure.com)
   - Crear proyecto o usar existente
   - Copiar endpoint del proyecto

2. **Modelo desplegado**
   - Azure OpenAI conectado al proyecto
   - Modelo `gpt-5.2` o equivalente disponible

3. **Autenticación verificada**
   ```bash
   az login
   az account show  # Verificar suscripción correcta
   ```

4. **Cuotas suficientes**
   - TPM (Tokens Per Minute) > 50K para Lab 02 (paralelo)
   - Si hay rate limiting, reducir paralelismo

### Verificación de Código

Antes del workshop, ejecutar cada lab para verificar:

```bash
# Lab 01
cd docs/modulo-03-workflows/labs/01-sequential
dotnet run  # Debe completar los 3 pasos

# Lab 02
cd ../02-parallel
dotnet run  # Debe mostrar tiempos paralelos vs secuenciales

# Lab 03
cd ../03-delegation
dotnet run  # Debe delegar a 3 agentes diferentes

# Lab 04
cd ../04-group-chat
dotnet run  # Debe terminar por consenso o max turns

# Lab 05 (requiere Azure AI Foundry)
cd ../05-azure-agent-service
dotnet run  # Debe conectar y persistir thread
```

## Checkpoints de Validación

### Checkpoint 1 (después de Labs 01-02)

**Método**: Show of hands + 2-3 screen shares

**Preguntas**:
1. "¿Quién completó el workflow secuencial con los 3 pasos en orden?" 
   - Meta: 85%
2. "¿Quién vio que el tiempo paralelo era menor que la suma individual?"
   - Meta: 80%

**Indicadores de problemas**:
- Si <70% levanta la mano: Hacer demo en vivo de Lab 02
- Si hay confusión sobre Task.WhenAll: Explicar con diagrama

### Checkpoint 2 (después de Labs 03-04)

**Método**: Pair checking + preguntas dirigidas

**Preguntas**:
1. "¿Quién vio que la tarea de diseño fue a DesignerAgent?"
   - Meta: 80%
2. "¿El group chat terminó por consenso o por max turns?"
   - Ambos son válidos

**Indicadores de problemas**:
- Si el routing no funciona: Verificar keywords en instrucciones
- Si group chat no termina: Reducir MaxTurns a 5

### Checkpoint 3 (después de Lab 05)

**Método**: Screen share de 2-3 participantes

**Verificar**:
1. "¿Aparece 'ESTADO PREVIO ENCONTRADO' en la segunda ejecución?"
2. "¿El historial muestra mensajes de la sesión anterior?"

**Meta**: 75% (este lab es más complejo)

**Indicadores de problemas**:
- Authentication fails: Ejecutar `az login` en vivo
- Thread not found: Eliminar `thread_state.json`

## Problemas Comunes y Soluciones

### Lab 01: Sequential

| Problema | Causa | Solución |
|----------|-------|----------|
| "Los agentes no mantienen contexto" | Normal - cada agente tiene su ChatHistory | Explicar que el contexto se pasa explícitamente en el prompt |
| "Tarda mucho" | 3 llamadas secuenciales a LLM | Esperado. En Lab 02 veremos paralelismo |

### Lab 02: Parallel

| Problema | Causa | Solución |
|----------|-------|----------|
| "Rate limit exceeded" | TPM insuficiente | Aumentar en Azure Portal o ejecutar con menos participantes simultáneos |
| "Tiempo paralelo igual a secuencial" | Llamadas serializadas por rate limiting | Aumentar TPM o aceptar como limitación de demo |

### Lab 03: Delegation

| Problema | Causa | Solución |
|----------|-------|----------|
| "PM siempre delega al mismo agente" | Function calling no activado | El código tiene fallback heurístico, explicar que es comportamiento esperado |
| "Routing incorrecto" | Keywords ambiguos | Modificar la tarea para ser más explícita |

### Lab 04: Group Chat

| Problema | Causa | Solución |
|----------|-------|----------|
| "Chat no termina" | Consenso nunca alcanzado | Reducir MaxTurns a 5-6 |
| "Solo un agente habla" | RoundRobin no funciona | Verificar que los 3 agentes están en el constructor |

### Lab 05: Azure AI Agent Service

| Problema | Causa | Solución |
|----------|-------|----------|
| "Authentication failed" | Azure CLI no logueado | `az login` en vivo |
| "Endpoint invalid" | URL incorrecta | Mostrar dónde copiar desde Azure AI Foundry |
| "Run nunca completa" | Timeout de red | Verificar conectividad, esperar más tiempo |

## Ajustes de Tiempo

### Si vas adelantado (+15 min)

- Agregar experimentos opcionales de cada lab
- Demo de diferentes estrategias de terminación en Lab 04
- Discusión sobre cuándo usar cada patrón

### Si vas atrasado (-15 min)

**Opción A**: Omitir Lab 05 (Azure AI Agent Service)
- Explicar conceptualmente la persistencia
- Mostrar diagrama de arquitectura
- Los participantes pueden hacerlo después

**Opción B**: Convertir Labs 03-04 en demos
- Instructor ejecuta mientras explica
- Participantes siguen en pantalla
- Ahorra ~20 min

### Si vas muy atrasado (-30 min)

- Labs 01-02 como práctica guiada (todos juntos)
- Labs 03-04 como demo del instructor
- Lab 05 como tarea opcional post-workshop
- Enfocar en conceptos, no en ejecución

## Notas de Presentación

### Introducción Teórica (20 min)

1. **Abrir con pregunta**: "¿Cuándo usarían múltiples agentes en lugar de uno solo?"
2. **Diagrama de 4 patrones**: Usar la imagen del README.md
3. **Casos de uso reales**: Pipeline de contenido, análisis multi-fuente, routing de tickets
4. **Transición a labs**: "Ahora implementaremos cada patrón"

### Entre Labs

- **Resumen de 30 segundos** de lo que aprendieron
- **Preview** de lo que viene: "En el siguiente lab veremos X..."
- **Momento para preguntas** rápidas

### Cierre del Módulo

1. **Recapitular los 4+1 patrones**:
   - Sequential: Para pipelines con dependencias
   - Parallel: Para tareas independientes
   - Delegation: Para routing inteligente
   - Group Chat: Para colaboración y consenso
   - Persistencia: Para workflows de larga duración

2. **Cuándo usar cada uno**:
   - "Si las tareas dependen una de otra → Sequential"
   - "Si las tareas son independientes → Parallel"
   - "Si necesitas especialistas → Delegation"
   - "Si necesitas discusión → Group Chat"

3. **Transición al siguiente módulo**:
   - "Ahora que saben crear workflows complejos..."
   - "¿Cómo los monitorean y depuran?"
   - "→ Módulo 4: Observabilidad"

## Recursos Adicionales

### Links para compartir con participantes

- [Azure AI Agent Service Docs](https://learn.microsoft.com/azure/ai-services/agents)
- [MAF Orchestration Patterns](https://learn.microsoft.com/microsoft-agent-framework/orchestration)

### Código de respaldo

Si algún lab no funciona, tener preparado:
- Screenshots de salida esperada
- Video corto (30 seg) de ejecución exitosa
- Código alternativo simplificado

## Métricas de Éxito

| Métrica | Objetivo | Aceptable |
|---------|----------|-----------|
| Checkpoint 1 completado | 85% | 75% |
| Checkpoint 2 completado | 80% | 70% |
| Checkpoint 3 completado | 75% | 65% |
| Entiende diferencia entre patrones | 90% | 80% |
| Puede elegir patrón correcto para caso de uso | 85% | 75% |

## Feedback Loop

Al final del módulo, preguntar:
1. "¿Qué patrón les pareció más útil para su trabajo?"
2. "¿Qué parte necesita más clarificación?"
3. "¿Algún caso de uso que no cubrimos?"

Usar respuestas para ajustar en futuras sesiones.

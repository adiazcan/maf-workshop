# Guía del Instructor: Módulo 2 - Function Tools y Composición de Agentes

**Tiempo Total**: 75 minutos (1 hora 15 minutos)  
**Nivel**: Intermedio  
**Prerequisitos para Participantes**: Completar Módulo 1 (Fundamentos)

---

## Resumen del Módulo

Este módulo enseña a los participantes cómo extender las capacidades de los agentes mediante:
1. **Function Tools**: Funciones de C# que los agentes pueden invocar automáticamente
2. **Agent-as-Tool**: Usar agentes especializados como funciones de un coordinador
3. **Human-in-the-Loop**: Patrones de aprobación humana para operaciones sensibles

---

## Estructura de la Sesión

| Tiempo | Actividad | Duración | Formato |
|--------|-----------|----------|---------|
| 0:00 | Introducción teórica | 15 min | Presentación |
| 0:15 | Lab 01: Custom Function Tool | 20 min | Hands-on |
| 0:35 | Checkpoint 1 | 5 min | Validación |
| 0:40 | Lab 02: Agent-as-Tool | 25 min | Hands-on |
| 1:05 | Checkpoint 2 | 5 min | Validación |
| 1:10 | Lab 03: Human Approval | 20 min | Hands-on |
| 1:30 | Checkpoint Final | 5 min | Validación |

**Nota**: Los tiempos incluyen margen para resolución de problemas.

---

## Preparación Pre-Sesión (Instructor)

### Verificar Ambiente

```bash
# Verificar que los proyectos de ejemplo compilan
cd docs/modulo-02-function-tools/labs/01-custom-tool
dotnet build

cd ../02-agent-as-tool
dotnet build

cd ../03-human-approval
dotnet build
```

### Material a Tener Listo

- [ ] Diapositivas del módulo (si las hay)
- [ ] README.md teórico abierto en el proyector
- [ ] Terminales preparadas para demos en vivo
- [ ] Azure OpenAI funcionando (verificar quota disponible)
- [ ] Código de los 3 labs listo para copiar/pegar si es necesario

### Decisiones de Demo

**Opción A (Recomendada)**: Participantes hacen los labs siguiendo instrucciones
- Mejor retención de aprendizaje
- Requiere más tiempo de soporte

**Opción B (Si el tiempo es limitado)**: Instructor hace demo, participantes observan
- Más rápido
- Menor retención

---

## Introducción Teórica (15 minutos)

### Puntos Clave a Cubrir

1. **¿Por qué Function Tools?** (3 min)
   - Los agentes tienen conocimiento general pero necesitan acceso a datos específicos
   - Function tools permiten conectar con APIs, bases de datos, servicios externos
   - El modelo **decide automáticamente** cuándo llamar funciones

2. **Anatomía de una Function Tool** (5 min)
   - `[KernelFunction]`: Marca el método como invocable
   - `[Description]`: Explica AL MODELO qué hace la función
   - Descripciones de parámetros: Guían al modelo en qué valores pasar
   - **Demostrar** el código de WeatherService.cs

3. **Composición de Agentes** (4 min)
   - Patrón: Un agente coordinador con agentes especialistas
   - Beneficios: Modularidad, especialización, reutilización
   - `KernelFunctionFactory.CreateFromMethod` para convertir agentes en funciones

4. **Human-in-the-Loop** (3 min)
   - Cuándo usar: Operaciones destructivas, transacciones, comunicaciones
   - Patrón: Pausar → Mostrar → Preguntar → Ejecutar o Cancelar
   - Importancia para IA responsable

### Preguntas Frecuentes en Teoría

**P: ¿Cómo sabe el modelo cuándo llamar la función?**
R: El modelo recibe las descripciones de todas las funciones disponibles junto con el prompt. Basándose en la pregunta del usuario y las descripciones, decide si alguna función es relevante.

**P: ¿Qué pasa si la descripción es mala?**
R: El modelo puede no llamar la función cuando debería, o llamarla con parámetros incorrectos. Las descripciones claras son críticas.

**P: ¿Las funciones pueden fallar?**
R: Sí. Siempre maneja excepciones y retorna mensajes de error informativos que el agente pueda comunicar al usuario.

---

## Lab 01: Custom Function Tool (20 minutos)

### Objetivo del Lab
Crear un agente que consulta el clima usando una función personalizada.

### Flujo del Lab

1. **Setup inicial** (5 min)
   - Crear proyecto, instalar paquetes
   - Configurar appsettings.json y user secrets

2. **Implementar WeatherService.cs** (7 min)
   - Explicar cada atributo mientras escriben
   - Enfatizar la importancia de las descripciones

3. **Implementar Program.cs** (5 min)
   - Mostrar `AddFromType<WeatherService>()`
   - Explicar `FunctionChoiceBehavior.Auto()`

4. **Ejecutar y validar** (3 min)
   - Probar preguntas sobre clima
   - Verificar que el agente llama la función automáticamente

### Checkpoint 1: Validación (5 minutos)

**Criterio de Éxito**: El agente llama automáticamente a `GetWeather()` cuando se pregunta sobre clima.

**Método de Validación**:
1. Pedir que levanten la mano quienes vean la respuesta con datos de clima específicos
2. Verificar 2-3 pantallas: "¿Qué temperatura muestra para Madrid?"
3. Todos deben ver "22°C" y "Soleado"

**Problemas Comunes**:

| Problema | Señales | Solución Rápida |
|----------|---------|-----------------|
| Función no se llama | Agente inventa datos | Verificar `AddFromType<>()` y `FunctionChoiceBehavior.Auto()` |
| Error 401 | Mensaje de autorización | Verificar API key en user secrets |
| Compilación falla | Errores de build | Verificar paquetes instalados correctamente |

**Meta**: 90% de participantes completan Lab 01

---

## Lab 02: Agent-as-Tool (25 minutos)

### Objetivo del Lab
Crear un sistema donde el agente principal delega matemáticas a un agente especializado.

### Flujo del Lab

1. **Setup y configuración** (3 min)
   - Reutilizar configuración similar a Lab 01

2. **Implementar CalculatorAgent.cs** (8 min)
   - Explicar que es un agente completo con sus propias instrucciones
   - Mostrar el método `SolveMathProblemAsync()`

3. **Implementar MainAgent.cs** (7 min)
   - Clave: `KernelFunctionFactory.CreateFromMethod()`
   - Mostrar cómo el método del agente se convierte en función

4. **Implementar Program.cs** (4 min)
   - Crear ambos agentes
   - Usar `kernel.Clone()` para plugins separados

5. **Ejecutar y validar** (3 min)
   - Probar preguntas matemáticas (debe delegar)
   - Probar preguntas generales (no debe delegar)

### Checkpoint 2: Validación (5 minutos)

**Criterio de Éxito**: El agente principal delega matemáticas al agente calculadora.

**Método de Validación**:
1. Todos pregunten: "¿Cuánto es 15% de 850?"
2. Deben ver el log: `📊 [CalculadoraExperta recibió]`
3. Luego pregunten: "¿Cuál es la capital de Francia?"
4. NO debe aparecer log de CalculadoraExperta

**Prueba de Contraste**:
- Si aparece log para matemáticas → ✅ Delegación funciona
- Si NO aparece log para preguntas generales → ✅ Routing funciona

**Meta**: 85% de participantes completan Lab 02

---

## Lab 03: Human Approval (20 minutos)

### Objetivo del Lab
Implementar aprobación humana para operaciones sensibles.

### Flujo del Lab

1. **Setup y configuración** (3 min)

2. **Implementar SensitiveOperations.cs** (10 min)
   - Mostrar el patrón de aprobación
   - Explicar la diferencia entre operaciones de lectura y escritura
   - Énfasis en el flujo: Validar → Preguntar → Ejecutar/Cancelar

3. **Implementar Program.cs** (4 min)

4. **Ejecutar y validar** (3 min)
   - Probar operación de lectura (list_files) - sin aprobación
   - Probar operación sensible (delete_file) - con aprobación
   - Probar APROBAR y RECHAZAR

### Checkpoint Final: Validación (5 minutos)

**Criterio de Éxito**: El sistema pide aprobación para operaciones sensibles.

**Método de Validación**:
1. Todos pidan: "Lista los archivos" → NO debe pedir aprobación
2. Todos pidan: "Elimina /temporal/cache.tmp" → DEBE pedir aprobación
3. La mitad aprueben (s), la otra mitad rechacen (n)
4. Verificar que los que aprobaron ven "✅ eliminado"
5. Verificar que los que rechazaron ven "🚫 cancelado"

**Meta**: 85% de participantes completan Lab 03

---

## Ajustes de Ritmo

### Si el Grupo Va Adelantado (+15 min de margen)

**Opciones**:
1. **Experimentos opcionales**: Cada lab tiene sección de experimentación
2. **Discusión de casos reales**: 
   - "¿Qué funciones crearían para su trabajo?"
   - "¿Qué operaciones requerirían aprobación en su empresa?"
3. **Preview del Módulo 3**: Mostrar brevemente los patrones de workflow

### Si el Grupo Va Retrasado (-10 min o menos)

**Lab 01 atrasado**:
- Demo en proyector, participantes copian código completo
- Reducir tiempo de setup compartiendo carpeta de proyecto base

**Lab 02 atrasado**:
- Simplificar: Solo mostrar el concepto, copiar código completo
- Omitir experimentación

**Lab 03 atrasado**:
- Demo completa del instructor
- Participantes ejecutan código pre-escrito solo para ver el flujo de aprobación

---

## Problemas Comunes y Soluciones

### Problema: "El agente no llama ninguna función"

**Diagnóstico**:
```csharp
// Verificar que esto está presente:
builder.Plugins.AddFromType<WeatherService>();

// Y esto en el agente:
Arguments = new KernelArguments(
    new AzureOpenAIPromptExecutionSettings
    {
        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
    }
)
```

**Solución rápida**: Copiar el código exacto del lab.

### Problema: "Error: Plugin already registered"

**Causa**: Intentar agregar el mismo plugin dos veces.

**Solución**: Usar `kernel.Clone()` antes de agregar plugins adicionales.

### Problema: "La función se llama con parámetros incorrectos"

**Causa**: Descripciones de parámetros poco claras.

**Solución**: Mejorar las descripciones con ejemplos específicos:
```csharp
[Description("Nombre de la ciudad, por ejemplo: Madrid, Barcelona, Valencia")]
```

### Problema: "Error de timeout en Azure OpenAI"

**Causa**: Rate limiting o servicio lento.

**Solución**: Esperar 30 segundos y reintentar. Si persiste, verificar quota en Azure Portal.

---

## Notas de Presentación

### Mensajes Clave

1. **"Las descripciones son para el MODELO, no para ti"**
   - Los desarrolladores a menudo escriben descripciones técnicas
   - Las descripciones deben explicar CUÁNDO usar la función

2. **"El modelo decide, tú defines las opciones"**
   - No escribes lógica de routing
   - Defines funciones claras y dejas que el modelo elija

3. **"Human-in-the-loop es IA responsable"**
   - No todos los agentes deben ser autónomos
   - Las operaciones críticas merecen supervisión humana

### Ejemplos del Mundo Real

**Function Tools**:
- "En un e-commerce, función para consultar inventario"
- "En finanzas, función para obtener cotizaciones"
- "En soporte, función para buscar tickets"

**Agent-as-Tool**:
- "Agente de ventas delega análisis financiero a agente especializado"
- "Agente de soporte delega problemas técnicos a agente de ingeniería"

**Human-in-the-Loop**:
- "Agente de RH no puede despedir empleados sin aprobación"
- "Agente financiero no puede autorizar pagos grandes solo"
- "Agente de comunicaciones no puede enviar masivos sin revisión"

---

## Materiales de Respaldo

### Si Necesitas Código de Emergencia

Todos los labs tienen código completo en:
- `docs/modulo-02-function-tools/labs/01-custom-tool/`
- `docs/modulo-02-function-tools/labs/02-agent-as-tool/`
- `docs/modulo-02-function-tools/labs/03-human-approval/`

### Enlaces Útiles

- [Documentación de Semantic Kernel Plugins](https://learn.microsoft.com/semantic-kernel/agents/plugins)
- [Function Calling Guide](https://learn.microsoft.com/azure/ai-services/openai/how-to/function-calling)
- [Responsible AI Guidelines](https://learn.microsoft.com/azure/ai-services/responsible-use-of-ai-overview)

---

## Post-Módulo

### Verificación de Comprensión

Preguntas rápidas para verificar aprendizaje:
1. "¿Para qué sirve el atributo `[Description]`?" 
   → Para que el modelo sepa cuándo usar la función
2. "¿Cómo convertimos un agente en función?"
   → `KernelFunctionFactory.CreateFromMethod()`
3. "¿Cuándo debemos usar Human-in-the-Loop?"
   → Operaciones destructivas, transacciones, comunicaciones

### Transición al Módulo 3

"Ahora que saben crear funciones y componer agentes, en el siguiente módulo veremos cómo orquestar **múltiples agentes trabajando juntos** en workflows complejos: secuenciales, paralelos, delegación, y group chat."

---

## Métricas de Éxito del Módulo

| Checkpoint | Meta | Aceptable |
|------------|------|-----------|
| Lab 01 | 90% | 80% |
| Lab 02 | 85% | 75% |
| Lab 03 | 85% | 75% |

**Éxito del módulo**: Al menos 80% de participantes completan los 3 labs.

---

**Última actualización**: 2026-01-12  
**Versión**: 1.0

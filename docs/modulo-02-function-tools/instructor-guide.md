# Guía del Instructor: Módulo 2 - Function Tools y Composición de Agentes

**Tiempo Total**: 55 minutos  
**Nivel**: Intermedio  
**Prerequisitos para Participantes**: Completar Módulo 1 (Fundamentos)

---

## Resumen del Módulo

Este módulo enseña a los participantes cómo extender las capacidades de los agentes mediante:
1. **Function Tools**: Funciones de C# que los agentes pueden invocar automáticamente
2. **Agent-as-Tool**: Usar agentes especializados como funciones de un coordinador

---

## Estructura de la Sesión

| Tiempo | Actividad | Duración | Formato |
|--------|-----------|----------|---------|
| 0:00 | Introducción teórica | 10 min | Presentación |
| 0:10 | Lab 01: Custom Function Tool | 20 min | Hands-on |
| 0:30 | Checkpoint 1 | 5 min | Validación |
| 0:35 | Lab 02: Agent-as-Tool | 25 min | Hands-on |
| 1:00 | Checkpoint Final | 5 min | Validación |

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
```

### Material a Tener Listo

- [ ] Diapositivas del módulo (si las hay)
- [ ] README.md teórico abierto en el proyector
- [ ] Terminales preparadas para demos en vivo
- [ ] Azure OpenAI funcionando (verificar quota disponible)
- [ ] Código de los 2 labs listo para copiar/pegar si es necesario

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

2. **Anatomía de una Function Tool** (4 min)
   - `AIFunctionFactory.Create`: Convierte métodos en funciones para el agente
   - Parámetro `name`: Nombre que el modelo usará para llamar la función
   - Parámetro `description`: Explica AL MODELO qué hace la función
   - **Demostrar** el código de WeatherService.cs y Program.cs

3. **Composición de Agentes** (3 min)
   - Patrón: Un agente coordinador con agentes especialistas
   - Beneficios: Modularidad, especialización, reutilización
   - `AIFunctionFactory.Create` para convertir agentes en funciones

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
   - Explicar los métodos estáticos que serán las funciones
   - Enfatizar la importancia de las descripciones claras

3. **Implementar Program.cs** (5 min)
   - Mostrar `AIFunctionFactory.Create` para cada función
   - Explicar el parámetro `tools` en `ChatCompletionAgent`

4. **Ejecutar y validar** (3 min)
   - Probar preguntas sobre clima
   - Verificar que el agente llama la función automáticamente

### Checkpoint 1: Validación (5 minutos)

**Criterio de Éxito**: El agente llama automáticamente a `get_weather` cuando se pregunta sobre clima.

**Método de Validación**:
1. Pedir que levanten la mano quienes vean la respuesta con datos de clima específicos
2. Verificar 2-3 pantallas: "¿Qué temperatura muestra para Madrid?"
3. Todos deben ver "22°C" y "Soleado"

**Problemas Comunes**:

| Problema | Señales | Solución Rápida |
|----------|---------|-----------------|
| Función no se llama | Agente inventa datos | Verificar `AIFunctionFactory.Create()` y el array `tools` |
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

2. **Implementar el agente calculadora** (8 min)
   - Explicar que es un `ChatCompletionAgent` completo
   - Mostrar cómo crear una función que invoca al agente

3. **Implementar el agente principal** (7 min)
   - Clave: `AIFunctionFactory.Create` para envolver el agente
   - Mostrar cómo el agente se convierte en función para el coordinador

4. **Implementar Program.cs** (4 min)
   - Crear ambos agentes
   - Registrar calculadora como tool del agente principal

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

## Ajustes de Ritmo

### Si el Grupo Va Adelantado (+10 min de margen)

**Opciones**:
1. **Experimentos opcionales**: Cada lab tiene sección de experimentación
2. **Discusión de casos reales**: 
   - "¿Qué funciones crearían para su trabajo?"
3. **Preview del Módulo 3**: Mostrar brevemente los patrones de workflow

### Si el Grupo Va Retrasado (-10 min o menos)

**Lab 01 atrasado**:
- Demo en proyector, participantes copian código completo
- Reducir tiempo de setup compartiendo carpeta de proyecto base

**Lab 02 atrasado**:
- Simplificar: Solo mostrar el concepto, copiar código completo
- Omitir experimentación

---

## Problemas Comunes y Soluciones

### Problema: "El agente no llama ninguna función"

**Diagnóstico**:
```csharp
// Verificar que las funciones se crean correctamente:
var getWeatherFunction = AIFunctionFactory.Create(
    WeatherService.GetWeather,
    name: "get_weather",
    description: "Obtiene el clima actual de una ciudad..."
);

// Y se pasan al agente en el constructor:
var agent = new ChatCompletionAgent(
    chatClient: chatClient,
    name: "WeatherAgent",
    instructions: "...",
    tools: new[] { getWeatherFunction, getForecastFunction }
);
```

**Solución rápida**: Copiar el código exacto del lab.

### Problema: "Plugin already registered" o conflicto de herramientas

**Causa**: Intentar agregar la misma herramienta dos veces o conflicto de nombres.

**Solución**: Verificar que cada función tiene un nombre único en el array `tools`.

### Problema: "La función se llama con parámetros incorrectos"

**Causa**: Descripciones de funciones o parámetros poco claras.

**Solución**: Mejorar las descripciones en `AIFunctionFactory.Create`:
```csharp
var function = AIFunctionFactory.Create(
    MyService.MyMethod,
    name: "my_function",
    description: "Obtiene información de una ciudad. El parámetro city debe ser el nombre completo, por ejemplo: Madrid, Barcelona, Valencia"
);
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

### Ejemplos del Mundo Real

**Function Tools**:
- "En un e-commerce, función para consultar inventario"
- "En finanzas, función para obtener cotizaciones"
- "En soporte, función para buscar tickets"

**Agent-as-Tool**:
- "Agente de ventas delega análisis financiero a agente especializado"
- "Agente de soporte delega problemas técnicos a agente de ingeniería"

---

## Materiales de Respaldo

### Si Necesitas Código de Emergencia

Todos los labs tienen código completo en:
- `docs/modulo-02-function-tools/labs/01-custom-tool/`
- `docs/modulo-02-function-tools/labs/02-agent-as-tool/`

### Enlaces Útiles

- [Documentación de Microsoft Agent Framework](https://learn.microsoft.com/dotnet/ai/agents)
- [Azure OpenAI Function Calling Guide](https://learn.microsoft.com/azure/ai-services/openai/how-to/function-calling)
- [Responsible AI Guidelines](https://learn.microsoft.com/azure/ai-services/responsible-use-of-ai-overview)

---

## Post-Módulo

### Verificación de Comprensión

Preguntas rápidas para verificar aprendizaje:
1. "¿Cómo creamos una función para el agente en MAF?" 
   → Usando `AIFunctionFactory.Create` con nombre y descripción
2. "¿Cómo convertimos un agente en función?"
   → Creando una función que invoca al agente y registrándola con `AIFunctionFactory.Create`

### Transición al Módulo 3

"Ahora que saben crear funciones y componer agentes, en el siguiente módulo veremos cómo orquestar **múltiples agentes trabajando juntos** en workflows complejos: secuenciales, paralelos, delegación, y group chat."

---

## Métricas de Éxito del Módulo

| Checkpoint | Meta | Aceptable |
|------------|------|-----------|
| Lab 01 | 90% | 80% |
| Lab 02 | 85% | 75% |

**Éxito del módulo**: Al menos 80% de participantes completan los 2 labs.

---

**Última actualización**: 2026-01-12  
**Versión**: 1.0

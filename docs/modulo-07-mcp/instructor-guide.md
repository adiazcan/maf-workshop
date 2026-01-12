# Guía del Instructor: Módulo 7 - Model Context Protocol (MCP)

## Tiempo Total: 35 minutos

## Resumen del Módulo

Este es el módulo final del workshop y tiene un enfoque **conceptual** más que práctico. El objetivo es que los participantes comprendan el **valor de MCP** para interoperabilidad, no que implementen un servidor MCP completo.

### Enfoque Recomendado

- **70% teoría / 30% demo práctica**
- Énfasis en "¿por qué MCP?" más que "¿cómo implementar MCP?"
- Lab demostrativo (instructor-led o participante-optional)

---

## Estructura de la Sesión

| Actividad | Duración | Formato |
|-----------|----------|---------|
| Introducción teórica: ¿Qué es MCP? | 10 min | Presentación |
| Diagrama: MCP vs integración directa | 5 min | Pizarra/Mermaid |
| Lab demo: MCP + MAF (opcional hands-on) | 15 min | Demo/Hands-on |
| Q&A y checkpoint final | 5 min | Discusión |

---

## Puntos Clave de la Teoría

### 1. Problema que Resuelve MCP (5 min)

**Mensaje clave**: "MCP es como USB para agentes de IA"

Analogía para explicar:
- **Antes de USB**: Cada dispositivo tenía su conector propietario
- **Antes de MCP**: Cada framework de IA tiene su propia forma de conectar herramientas
- **Con MCP**: Un estándar universal que todos pueden usar

### 2. Cuándo Usar MCP vs Function Tools (3 min)

| Escenario | Recomendación |
|-----------|---------------|
| Herramienta interna de un solo agente | Function Tools |
| Herramienta compartida entre múltiples frameworks | **MCP** |
| Operación de baja latencia (<100ms crítico) | Function Tools |
| Marketplace de herramientas | **MCP** |
| Prototipo rápido | Function Tools |
| Producción multi-vendor | **MCP** |

### 3. Adopción de MCP (2 min)

Mencionar que MCP está siendo adoptado por:
- ✅ Microsoft (MAF, Copilot)
- ✅ Anthropic (Claude)
- ✅ Google (Gemini)
- ✅ OpenAI
- ✅ Frameworks open source

---

## Checkpoint de Validación

### Checkpoint Final del Workshop

**Criterios de Éxito** (comprensión conceptual):

1. ✅ El participante puede explicar qué es MCP en 1-2 frases
2. ✅ El participante identifica al menos 2 ventajas de MCP
3. ✅ El participante describe un caso de uso donde MCP es preferible

**Método de Validación**:
1. Preguntar: "¿Quién puede explicar qué es MCP en una frase?"
2. Esperar 2-3 respuestas voluntarias
3. Si no hay voluntarios, hacer preguntas directas suaves

**Respuestas Aceptables**:
- "MCP es un estándar para conectar herramientas de IA entre diferentes frameworks"
- "MCP permite reutilizar integraciones en múltiples agentes"
- "MCP es como una API universal para herramientas de agentes"

**Meta**: 80% de participantes comprenden el valor de MCP

**Tiempo Máximo para Checkpoint**: 5 minutos

---

## Lab Demo (15 min)

### Opción A: Instructor-Led Demo (Recomendado)

Si el tiempo es limitado o el grupo está cansado:

1. **Proyectar tu pantalla** con el lab pre-ejecutado
2. **Mostrar** la estructura del código brevemente (2 min)
3. **Ejecutar** `dotnet run` y hacer 3-4 preguntas demo (8 min)
4. **Explicar** las llamadas MCP visibles en la consola (3 min)
5. **Preguntas** del grupo (2 min)

**Preguntas demo sugeridas**:
```
¿Qué clima hace en Madrid?  → Muestra [MCP] en consola
Convierte 30°C a Fahrenheit → Muestra herramienta nativa
Dame las noticias de tecnología → Muestra otra herramienta MCP
¿Qué hora es? → Muestra herramienta nativa
```

### Opción B: Hands-On Participantes

Si hay tiempo y energía:

1. Participantes ejecutan el lab individualmente (10 min)
2. Checkpoint rápido: "¿Quién vio [MCP] aparecer en la consola?" (2 min)
3. Discusión grupal de observaciones (3 min)

---

## Problemas Comunes Anticipados

| Problema | Señales | Solución Rápida |
|----------|---------|-----------------|
| Confusión MCP vs REST | "¿Por qué no usar una API normal?" | Explicar: MCP es REST + convención de descubrimiento |
| Fatiga del workshop | Baja participación, bostezos | Acortar teoría, ir directo a demo |
| Preguntas muy técnicas | "¿Cómo implemento un servidor MCP en Go?" | "Excelente pregunta para después del workshop" + link a docs |
| Azure OpenAI no conecta | Errores 401/timeout | Hacer demo desde tu máquina pre-configurada |

---

## Ajustes de Ritmo

### Si el grupo va adelantado (termina con >10 min de margen):

1. **Discusión de casos de uso**:
   - "¿En qué proyecto de tu empresa usarías MCP?"
   - "¿Qué herramienta te gustaría exponer como MCP Server?"

2. **Demo adicional**:
   - Mostrar el MCP Inspector: https://github.com/modelcontextprotocol/inspector
   - Navegar por la galería de servidores MCP: https://github.com/modelcontextprotocol/servers

3. **Preview de recursos avanzados**:
   - Mencionar Azure AI Agent Service + MCP
   - Comentar sobre el futuro de la interoperabilidad de agentes

### Si el grupo va retrasado (quedan <5 min):

1. **Saltar lab hands-on** → Hacer demo de 3 min
2. **Reducir teoría** → Solo problema + solución
3. **Ir directo a checkpoint**:
   - "MCP = estándar para herramientas de IA interoperables"
   - "Beneficio principal = reutilización entre frameworks"
   - "¿Alguna pregunta?"

---

## Notas de Presentación

### Mensajes Clave a Transmitir

1. **MCP no reemplaza function tools** - son complementarios
2. **MCP es para interoperabilidad** - si solo usas MAF, function tools basta
3. **MCP está creciendo rápidamente** - Anthropic, Microsoft, Google lo adoptan
4. **No necesitas implementar MCP hoy** - pero debes saber que existe

### Preguntas Frecuentes Esperadas

**P: ¿MCP es obligatorio para usar MAF?**
R: No, es opcional. Function tools nativas funcionan perfectamente.

**P: ¿Puedo usar MCP en producción?**
R: Sí, pero evalúa si necesitas interoperabilidad multi-vendor primero.

**P: ¿Quién creó MCP?**
R: Anthropic lo inició, ahora es un esfuerzo multi-vendor con Microsoft y Google.

**P: ¿Hay latencia adicional con MCP?**
R: Sí, hay un hop de red adicional. Para operaciones de muy baja latencia, considera function tools.

### Ejemplos del Mundo Real para Mencionar

1. **Banco con múltiples chatbots**: Un MCP Server de "consulta de saldo" que sirve a Copilot (interno), asistente web (público), y app móvil.

2. **SaaS con 100+ integraciones**: Cada integración (Salesforce, HubSpot, SAP) es un MCP Server que los clientes conectan a sus agentes.

3. **Migración de framework**: Empresa que empezó con Claude, migra a Copilot, y sus MCP Servers siguen funcionando sin cambios.

---

## Cierre del Workshop

### Mensaje de Cierre (2 min)

"Este módulo cierra nuestro workshop de Microsoft Agent Framework. Han aprendido a:

1. ✅ Crear agentes conversacionales
2. ✅ Extenderlos con herramientas
3. ✅ Orquestar workflows complejos
4. ✅ Monitorizarlos en producción
5. ✅ Desplegarlos en web con Aspire
6. ✅ Depurarlos con DevUI
7. ✅ Integrarlos con el ecosistema MCP

Ahora tienen las bases para construir agentes de IA en producción. ¡Éxito en sus proyectos!"

### Recursos para Después del Workshop

Mostrar brevemente en pantalla:
- Documentación: https://learn.microsoft.com/microsoft-agent-framework
- Samples: https://github.com/microsoft/agent-framework-samples
- MCP: https://modelcontextprotocol.io

### Feedback

Recordar completar la encuesta post-workshop (si existe).

---

## Materiales Necesarios

- [ ] Lab pre-configurado y probado en tu máquina
- [ ] Conexión a Azure OpenAI funcionando
- [ ] Diagramas Mermaid listos (o pizarra)
- [ ] Links a recursos en un documento fácil de compartir

---

**Última actualización**: 2026-01-12  
**Módulo**: 7 - Model Context Protocol (MCP)

# Guía del Instructor: Módulo 1 - Fundamentos

**Tiempo Total**: 60 minutos  
**Complejidad**: Introductorio  
**Meta de Éxito**: 90% de participantes completan Lab 01 exitosamente

---

## Estructura de la Sesión

| Actividad | Duración | Formato | Descripción |
|-----------|----------|---------|-------------|
| Introducción teórica | 15 min | Presentación | Conceptos de MAF, tipos de agentes, arquitectura |
| Lab 01: Hello Agent | 30 min | Hands-on | Crear primer agente conversacional |
| Checkpoint de validación | 5 min | Validación manual | Verificar que agentes funcionan |
| Q&A y troubleshooting | 10 min | Interactivo | Resolver dudas y problemas |

**Total**: 60 minutos

---

## Preparación Pre-Sesión

### 1 Semana Antes

- [ ] Enviar email con guía de instalación a participantes
- [ ] Solicitar que completen: instalación de .NET 10, VS Code, configuración de Azure
- [ ] Verificar que todos tienen acceso a Azure OpenAI (permisos/cuotas)
- [ ] Preparar recurso de Azure OpenAI de respaldo (en caso de problemas de cuota)

### 1 Día Antes

- [ ] Probar Lab 01 end-to-end en máquina limpia
- [ ] Verificar que deployment `gpt-5.2` está activo
- [ ] Preparar API keys de respaldo (para participantes sin Azure)
- [ ] Crear documento con top 5 errores comunes + soluciones
- [ ] Preparar pantallas compartidas con código completo (backup)

### Día Del Workshop

- [ ] Llegar 15 minutos antes
- [ ] Probar proyector/pantalla compartida
- [ ] Abrir VS Code con Lab 01 completo en pestañas
- [ ] Abrir Azure Portal en pestaña (para demostrar despliegue de modelo)
- [ ] Iniciar Azure Monitor dashboard (para ver uso en tiempo real)
- [ ] Tener script de verificación listo: `verify-environment.sh`

---

## Checkpoint de Validación

### Checkpoint 1: Ambiente Configurado (Antes del Lab)

**Tiempo**: 5 minutos  
**Método**: Show of hands + verificación de pantalla

**Criterios de éxito**:
1. `.NET 10 SDK instalado` → "Levanten la mano si `dotnet --version` muestra 10.x"
2. `Azure OpenAI configurado` → "¿Quién tiene endpoint y API key listos?"
3. `VS Code instalado` → "¿Todos pueden abrir VS Code?"

**Meta**: 100% antes de comenzar Lab 01

**Si <90% tiene ambiente listo**:
- Hacer demo completa del Lab (no hands-on)
- Distribuir código completo vía USB/repo
- Continuar con teoría mientras ayudantes resuelven instalación

---

### Checkpoint 2: Lab 01 Completo

**Tiempo**: 5 minutos  
**Método**: Combinado (show of hands + pair checking + screen share)

**Criterio de Éxito**: "El agente responde a 'Hola' con un saludo coherente en español"

**Método de Validación**:

1. **Show of Hands** (30 segundos):
   - "Levanten la mano quienes vean la salida '🤖 AsistenteGeneral:' en su consola"
   - Contar aproximadamente cuántos tienen éxito

2. **Pair Checking** (2 minutos):
   - "Verifiquen con su compañero de al lado que el agente mantiene contexto"
   - "Pregúntenle al agente su nombre 2 veces, debe recordarlo"

3. **Screen Share** (2 minutos):
   - Pedir a 2-3 participantes compartir pantalla
   - Verificar que la conversación funciona correctamente
   - Celebrar éxitos públicamente: "¡Excelente! Ya tienes tu primer agente funcionando"

**Meta**: 90% de éxito (80% es aceptable para Módulo 1)

**Registro Manual** (llenar durante checkpoint):
```
Checkpoint Módulo 1 - Lab 01
Fecha: ___________
Participantes totales: ___
Completaron exitosamente: ___
Porcentaje de éxito: ___%

Problemas comunes encontrados:
1. ________________________________
2. ________________________________
3. ________________________________
```

---

## Problemas Comunes Anticipados

### Problema 1: ".NET SDK no encontrado"

**Señales**:
- Participante escribe `dotnet --version` → "command not found"
- No puede crear proyecto con `dotnet new console`

**Causa raíz**:
- SDK no instalado correctamente
- PATH no configurado después de instalación
- Terminal no reiniciada

**Solución Rápida** (2 minutos):
1. Verificar instalación manual: buscar carpeta `C:\Program Files\dotnet\` (Windows)
2. Reiniciar terminal
3. Si persiste: **usar laptop de respaldo** o compartir con vecino
4. Post-workshop: enviar guía de instalación detallada

**Solución Preventiva**:
- Email pre-workshop con script de verificación
- Pedir screenshot de `dotnet --version` antes del workshop

---

### Problema 2: "401 Unauthorized" al invocar agente

**Señales**:
- Programa compila correctamente
- Al ejecutar: `❌ Error de conexión: Unauthorized (401)`
- Agente no responde

**Causa raíz**:
- API Key incorrecta
- API Key no configurada en user secrets
- User secrets no inicializado

**Solución Rápida** (3 minutos):
1. Verificar user secrets:
   ```bash
   dotnet user-secrets list
   ```
2. Si está vacío:
   ```bash
   dotnet user-secrets set "AzureOpenAI:ApiKey" "la-key-correcta"
   ```
3. Re-ejecutar: `dotnet run`

**Solución Preventiva**:
- Demostrar configuración de user secrets en proyector ANTES del lab
- Compartir pantalla mostrando `dotnet user-secrets list` funcionando

**Alternativa de Emergencia**:
- Proporcionar API key compartida (con rate limit monitoring)
- Usar recurso de Azure del instructor temporalmente

---

### Problema 3: "El agente no responde (sin error)"

**Señales**:
- Programa ejecuta sin errores
- Usuario escribe mensaje
- No aparece respuesta del agente (se queda esperando)

**Causa raíz**:
- Endpoint incorrecto en `appsettings.json`
- Deployment name no coincide con Azure
- Rate limit silencioso (429)
- Modelo no desplegado o apagado

**Solución Rápida** (4 minutos):
1. Verificar endpoint:
   ```json
   "Endpoint": "https://NOMBRE-CORRECTO.openai.azure.com/"
   ```
   Debe terminar con `/`

2. Verificar deployment name:
   - Abrir Azure Portal → recurso OpenAI → "Deployments"
   - Copiar nombre EXACTO del deployment
   - Actualizar `appsettings.json`:
     ```json
     "DeploymentName": "gpt-5.2"
     ```

3. Verificar rate limit:
   - Azure Portal → recurso OpenAI → "Metrics"
   - Si hay 429 errors → esperar 1 minuto
   - Si persiste → usar deployment de respaldo

**Solución Preventiva**:
- Demostrar cómo verificar deployment name en Azure Portal
- Tener deployment de respaldo con más cuota

---

### Problema 4: "Firewall/Proxy bloquea Azure"

**Señales**:
- Error de conexión tipo "Unable to connect"
- Timeout en requests
- Funciona en casa pero no en la oficina/venue

**Causa raíz**:
- Firewall corporativo bloquea `*.openai.azure.com`
- Proxy no configurado
- Red del venue restrictiva

**Solución Rápida** (5 minutos):
1. **Hotspot móvil**: Conectar laptop a celular
2. **VPN corporativa**: Solicitar excepción para Azure OpenAI
3. **Laptop del instructor como proxy**: Configurar compartición

**Solución Preventiva**:
- Probar conectividad desde venue 1 día antes
- Coordinar con IT de la empresa para whitelist `*.openai.azure.com`
- Tener hotspots móviles de respaldo

---

## Ajustes de Ritmo

### Si el grupo va adelantado (>10 min de margen)

**Opciones** (elegir según contexto):

1. **Profundizar en conceptos** (5-10 min):
   - Explicar diferencias entre `InvokeAsync` vs `InvokeStreamingAsync`
   - Mostrar cómo funciona el truncamiento de historial
   - Demostrar parámetros de `ExecutionSettings` (Temperature, TopP, MaxTokens)

2. **Experimentación guiada** (10-15 min):
   - "Modifiquen las `Instructions` para crear una personalidad diferente"
   - "Cambien `Temperature` a 0.0 y luego a 1.5, ¿qué diferencias notan?"
   - "Agreguen validación de entrada (ej: detectar spam o inputs maliciosos)"

3. **Preview del Módulo 2** (5 min):
   - Mostrar brevemente cómo se agregan function tools
   - Demo rápida: agente con función `GetWeather()`
   - Generar anticipación para la siguiente sesión

---

### Si el grupo va retrasado (<10 min restantes)

**Opciones de aceleración**:

1. **Demo completa del lab** (15 min):
   - Instructor muestra código completo en proyector
   - Participantes siguen sin escribir (solo observan)
   - Distribuir código completo vía repo/USB
   - **Trade-off**: Menos hands-on, pero todos ven funcionamiento

2. **Pair programming** (20 min):
   - Formar parejas: uno avanzado + uno rezagado
   - El avanzado ayuda al rezagado a completar
   - Instructor circula resolviendo bloqueos críticos

3. **Simplificar lab** (15 min):
   - Proporcionar proyecto pre-configurado (solo falta API key)
   - Saltar paso de instalación de paquetes
   - Enfocarse solo en ejecutar y entender output

**No sacrificar**:
- Checkpoint de validación (crítico para medir éxito)
- Explicación de conceptos clave (Kernel, Agent, ChatHistory)

---

## Notas de Presentación

### Slide 1: ¿Qué es un Agente? (3 min)

**Puntos clave**:
- "Un agente NO es solo un chatbot"
- "Puede razonar, actuar, y usar herramientas"
- Ejemplo real: "Agente de viajes que reserva vuelos automáticamente"

**Storytelling**:
> "Imaginen que le piden a ChatGPT: 'Organiza mi viaje a Madrid'.  
> Un chatbot diría: 'Claro, aquí hay algunos consejos'.  
> Un agente diría: 'Ya reservé tu vuelo, hotel, y creé un itinerario' → Y LO HIZO."

**Demo visual**:
- Mostrar diagrama de arquitectura (README.md)
- Destacar: Usuario → Agente → LLM + Herramientas

---

### Slide 2: Componentes de MAF (4 min)

**Analogía didáctica**:
- **Kernel** = "Motor del coche" (gestiona todo)
- **Agent** = "Conductor" (decide qué hacer)
- **Plugins/Tools** = "Herramientas en el maletero" (extensiones)
- **ChatHistory** = "Memoria del conductor" (contexto)

**Código mínimo** (mostrar en pantalla):
```csharp
var agent = new ChatCompletionAgent { 
    Instructions = "Eres un asistente útil",
    Kernel = kernel 
};
```

**Pregunta interactiva**:
> "¿Qué pasaría si no tenemos ChatHistory?  
> El agente olvidaría todo después de cada respuesta (como Dory en Buscando a Nemo 🐠)"

---

### Slide 3: Tipos de Agentes (3 min)

**Tabla visual**:

| Tipo | Cuándo Usar | Ejemplo |
|------|-------------|---------|
| ChatCompletionAgent | Conversaciones simples | Chatbot de soporte |
| OpenAI Assistants | Workflows largos, documentos | Análisis de PDFs |
| Multi-Agent | Tareas complejas | Coordinación de equipos |

**Preview futuro**:
- "Hoy veremos ChatCompletionAgent (lo más simple)"
- "En Módulo 3, orquestaremos múltiples agentes trabajando juntos"

---

## Preguntas Frecuentes (Q&A Anticipado)

### P1: "¿Por qué no puedo usar OpenAI directamente en vez de Azure?"

**Respuesta**:
> "Azure OpenAI ofrece beneficios empresariales que OpenAI directo no tiene:
> - Tus datos NO se usan para entrenamiento (privacy garantizada)
> - SLA de 99.9% uptime (producción)
> - Cumplimiento GDPR y regulaciones regionales
> - Integración con Active Directory y seguridad Azure
> 
> Para este workshop, es obligatorio usar Azure (requisito del spec)."

---

### P2: "¿Cuánto cuesta ejecutar este agente?"

**Respuesta con números reales**:
> "Modelo gpt-5.2 cuesta aproximadamente:
> - Input: $3 por millón de tokens
> - Output: $12 por millón de tokens
> 
> Una conversación típica de 10 turnos ~2000 tokens ≈ $0.03 USD
> Este lab completo te costará menos de $0.10
> 
> Para producción, considera:
> - Cachear respuestas comunes
> - Truncar historial (como hacemos en el código)
> - Monitorear uso con Azure Monitor (Módulo 4)"

---

### P3: "¿Puedo usar modelos locales como Llama?"

**Respuesta**:
> "Técnicamente sí, pero para este workshop usamos Azure OpenAI exclusivamente.
> 
> Razones:
> - Garantizamos que todos tienen el mismo modelo
> - Evitamos problemas de hardware (GPUs)
> - MAF está optimizado para Azure
> 
> Post-workshop, puedes explorar modelos locales con Semantic Kernel (base de MAF)."

---

## Métricas de Éxito a Recolectar

### Durante el Workshop

**Checkpoint Manual** (llenar en papel/Excel):
```
Módulo 1 - Métricas
===================
Participantes totales: ___
Ambiente configurado: ___
Lab 01 completado: ___
Tiempo real usado: ___ minutos (objetivo: 60 min)

Problemas técnicos encontrados:
1. ________________________________ (# participantes: ___)
2. ________________________________ (# participantes: ___)
3. ________________________________ (# participantes: ___)

Preguntas frecuentes:
1. ________________________________
2. ________________________________
3. ________________________________
```

### Post-Workshop

**Encuesta digital** (enviar link después):

1. Claridad del contenido (1-5):  
   ⭐⭐⭐⭐⭐

2. Ritmo del módulo:  
   ○ Muy lento  
   ○ Lento  
   ○ Adecuado ✓  
   ○ Rápido  
   ○ Muy rápido

3. ¿Qué parte fue más difícil?  
   ○ Instalación de ambiente  
   ○ Configuración de Azure  
   ○ Entender código C#  
   ○ Troubleshooting errores

4. ¿Recomendarías este módulo? (Sí/No)

---

## Recursos de Respaldo

### Material de Contingencia

1. **USB con código completo**:
   - Proyecto HelloAgent funcional
   - Instaladores de .NET 10 (Windows/macOS)
   - PDF con guía de troubleshooting

2. **Azure de respaldo**:
   - Segundo recurso de Azure OpenAI con deployment separado
   - API keys adicionales (con rate limit diferente)
   - Subscription de backup

3. **Documentación offline**:
   - PDF de documentación de Microsoft Agent Framework
   - Ejemplos de código impresos
   - Diagramas de arquitectura en póster

---

## Post-Sesión

### Inmediatamente Después

- [ ] Recolectar métricas de checkpoints
- [ ] Identificar top 3 problemas encontrados
- [ ] Actualizar guía de troubleshooting si hay nuevos errores
- [ ] Enviar email con resumen + links a recursos adicionales

### Dentro de 24 Horas

- [ ] Analizar tasa de éxito del módulo
- [ ] Ajustar timing para próxima sesión si fue necesario
- [ ] Preparar respuestas a preguntas pendientes
- [ ] Enviar encuesta de feedback

### Antes del Siguiente Módulo

- [ ] Revisar retroalimentación de participantes
- [ ] Actualizar material si hubo confusiones recurrentes
- [ ] Preparar recap de 5 min para inicio de Módulo 2

---

## Contacto de Soporte

**Durante el workshop**:
- Instructor principal: [nombre]
- Ayudante técnico 1: [nombre]
- Ayudante técnico 2: [nombre]

**Post-workshop**:
- Email: maf-workshop-support@empresa.com
- Canal de Slack/Teams: #maf-workshop
- Office hours: Martes/Jueves 4-5pm

---

**Última actualización**: 2026-01-12  
**Versión**: 1.0  
**Mantenido por**: Equipo de Capacitación MAF

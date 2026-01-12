# Guía del Instructor: Módulo 6 - DevUI

## Tiempo Total: 15 minutos

## Estructura de la Sesión

| Actividad | Duración | Formato |
|-----------|----------|---------|
| Introducción teórica + Demo | 5 min | Presentación con demo en vivo |
| Lab 01: DevUI Setup | 8 min | Hands-on |
| Checkpoint de validación | 2 min | Verificación grupal |

---

## Enfoque Recomendado: Demo-First

**Este módulo funciona mejor con una demostración primero**, seguida del hands-on:

### Secuencia Óptima

1. **Demo en vivo (5 min)**:
   - Mostrar DevUI ya configurado en un proyecto de ejemplo
   - Hacer preguntas al agente y mostrar cómo aparecen en DevUI
   - Destacar el panel de function calls en tiempo real
   - Mostrar un caso de debugging: "¿Por qué no se llama la función?"

2. **Hands-on (8 min)**:
   - Los participantes replican la configuración
   - Con la demo fresca, saben qué esperar

3. **Checkpoint (2 min)**:
   - Verificar que DevUI se conecta
   - Ver function calls en sus pantallas

---

## Preparación Pre-Sesión

### Verificar antes del módulo:
- [ ] DevUI tool instalado en máquina del instructor (`devui --version`)
- [ ] Proyecto de demostración preparado y funcionando
- [ ] Puerto 5100 disponible y no bloqueado por firewall
- [ ] Navegador listo para mostrar DevUI interface

### Proyecto de demo preparado:
```bash
cd instructor-demos/devui-demo
dotnet run
# En otra terminal:
devui start --port 5100
```

---

## Checkpoint de Validación

### Checkpoint: DevUI Conectado (min 13)

**Criterio de Éxito**: DevUI muestra conversaciones y function calls

**Método de Validación**:
1. Pedir que levanten la mano quienes ven DevUI en `http://localhost:5100`
2. Verificar 2-3 pantallas compartidas mostrando function calls
3. Preguntar: "¿Quién ve 'get_weather' cuando pregunta por el clima?"

**Meta**: 85% de participantes con DevUI funcionando

**Tiempo máximo**: 2 minutos

### Si hay problemas masivos:
- Verificar que todos tienen el puerto correcto (5100)
- Comprobar que `AddDevUI()` está antes de `builder.Build()`
- Ofrecer proyecto de respaldo pre-configurado

---

## Problemas Comunes Anticipados

| Problema | Señales | Solución Rápida |
|----------|---------|-----------------|
| DevUI no instalado | "devui: command not found" | `dotnet tool install -g Microsoft.AI.Agents.DevUI` |
| Puerto ocupado | "Address already in use" | Cambiar a puerto 5101 |
| Sin function calls | Solo mensajes, no funciones | Verificar `AddSingleton(kernel)` |
| Firewall Windows | Popup de seguridad | Permitir acceso de red |

---

## Ajustes de Ritmo

### Si el grupo va adelantado (termina con >3 min de margen):
- Demostrar debugging avanzado: "¿Qué pasa si la función lanza excepción?"
- Mostrar múltiples function calls en una sola pregunta
- Discutir cuándo usar DevUI vs OpenTelemetry

### Si el grupo va retrasado (quedan <2 min):
- Simplificar a solo verificar que DevUI se conecta
- Mostrar demo completa del instructor en proyector
- Proporcionar proyecto pre-configurado para que copien

---

## Notas de Presentación

### Puntos Clave a Enfatizar

1. **DevUI es para desarrollo, no producción**
   - "Piensen en DevUI como los dev tools del navegador, pero para agentes"
   - Para producción, usamos OpenTelemetry (Módulo 4)

2. **El valor de ver function calls**
   - "La pregunta más común: '¿Por qué el agente no llama mi función?'"
   - DevUI responde eso instantáneamente

3. **Debugging de descripciones**
   - Demostrar: descripción vaga → función no se llama
   - Demostrar: descripción clara → función se llama correctamente

### Preguntas Frecuentes

**P: ¿Puedo usar DevUI en producción?**
> R: No es recomendable. DevUI agrega overhead y expone información interna. En producción, usa OpenTelemetry con Azure Monitor.

**P: ¿DevUI funciona con agentes remotos?**
> R: DevUI está diseñado para desarrollo local. Para debugging remoto, considera usar Application Insights con distributed tracing.

**P: ¿Cómo sé que mi función se registró correctamente?**
> R: En DevUI, hay un panel de "Available Functions" que lista todas las funciones registradas con sus descripciones.

---

## Conexión con Otros Módulos

### Relacionado con Módulo 2 (Function Tools):
- DevUI es la herramienta perfecta para debugging de function tools
- Si alguien tuvo problemas en Módulo 2, DevUI hubiera ayudado

### Relacionado con Módulo 4 (Observability):
- DevUI = debugging en desarrollo
- OpenTelemetry = observabilidad en producción
- Son complementarios, no mutuamente exclusivos

---

## Recursos para el Instructor

### Demo Script

```
"Voy a demostrar DevUI antes de que ustedes lo configuren.

[Ejecutar proyecto de demo]

Observen mi navegador - aquí tengo DevUI abierto.
Voy a preguntarle al agente sobre el clima.

[Escribir: ¿Cómo está el clima en Madrid?]

Miren DevUI - ¿ven cómo aparece 'get_weather'?
Pueden ver los parámetros exactos: city = "Madrid"
Y el resultado de la función.

Ahora voy a hacer algo interesante.
¿Qué pasa si pregunto sobre una ciudad que no existe?

[Escribir: ¿Clima en Tokio?]

La función se llama, retorna un error, y el agente reformula.
Todo visible en DevUI.

Ahora es su turno de configurarlo."
```

---

## Archivos del Lab

Los participantes crean estos archivos:

```
DevUIExample/
├── DevUIExample.csproj
├── appsettings.json
├── DemoFunctions.cs      (5 function tools)
└── Program.cs            (configuración DevUI)
```

---

## Criterios de Éxito del Módulo

✅ Participante puede instalar DevUI tool  
✅ Participante puede configurar DevUI en aplicación  
✅ Participante puede ver conversaciones en tiempo real  
✅ Participante puede identificar function calls con parámetros  
✅ Participante entiende cuándo usar DevUI vs otras herramientas

---

**Duración total**: 15 minutos  
**Complejidad**: Baja (herramienta de desarrollo)  
**Dependencias**: Módulo 1 (básico), Módulo 2 (function tools)

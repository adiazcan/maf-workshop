# Guía del Instructor - Módulo 5: ASP.NET Core + .NET Aspire

**Módulo**: 5 - Integración con ASP.NET Core y .NET Aspire  
**Duración Total**: 60 minutos  
**Tipo**: Capstone Project (Proyecto Integrador)

---

## 📊 Resumen de Timing

| Actividad | Duración | Tiempo Acumulado |
|-----------|----------|------------------|
| Introducción teórica | 10 min | 0:10 |
| Demo del instructor | 10 min | 0:20 |
| Lab hands-on | 35 min | 0:55 |
| Validación y Q&A | 5 min | 1:00 |

---

## 🎯 Objetivos del Módulo

Al finalizar, los participantes podrán:
1. Exponer agentes MAF como APIs HTTP con ASP.NET Core
2. Usar dependency injection para servicios de agentes
3. Orquestar aplicaciones con .NET Aspire
4. Interpretar el dashboard de Aspire (logs, traces, métricas)

---

## 📋 Preparación Pre-Módulo

### Verificar con Anticipación

```bash
# Verificar que .NET Aspire está instalado en las máquinas
dotnet workload list | grep aspire

# Si falta, instalarlo (puede tomar 5-10 minutos)
dotnet workload install aspire
```

### Materiales Necesarios

- [ ] Código del lab pre-descargado
- [ ] API keys de Azure OpenAI distribuidas
- [ ] Puertos 5000 y 15888 libres en las máquinas
- [ ] Navegador web disponible para dashboard

---

## 🎬 Guión de Presentación

### 1. Introducción Teórica (10 minutos)

**Puntos clave a cubrir**:

1. **¿Por qué ASP.NET + Aspire?**
   - Exponer agentes para aplicaciones web/móviles
   - Escalabilidad horizontal
   - Observabilidad integrada

2. **Arquitectura del proyecto**
   - Mostrar diagrama de la documentación
   - Explicar roles: AppHost, WebApi, AgentServices

3. **Beneficios de Aspire**
   - Un comando para iniciar todo
   - Dashboard unificado
   - Service discovery automático

**Tip del instructor**: Usar el diagrama Mermaid del README para visualizar la arquitectura.

---

### 2. Demo del Instructor (10 minutos)

**Secuencia de demo**:

1. **Mostrar estructura del proyecto** (2 min)
   ```bash
   tree 01-multi-agent-web
   ```

2. **Iniciar con Aspire** (2 min)
   ```bash
   dotnet run --project AppHost
   ```

3. **Explorar dashboard** (3 min)
   - Mostrar panel Resources
   - Mostrar panel Console con logs en tiempo real

4. **Hacer un request de prueba** (2 min)
   ```bash
   curl -X POST http://localhost:5000/api/chat/weather \
     -H "Content-Type: application/json" \
     -d '{"city": "Madrid"}'
   ```

5. **Mostrar traces** (1 min)
   - Navegar a Traces en dashboard
   - Mostrar jerarquía de spans

**Script de demo**:
> "Observen cómo con un solo comando tenemos toda la aplicación corriendo con observabilidad completa. El dashboard nos muestra logs en tiempo real, y cuando hacemos un request, podemos ver el trace completo incluyendo la llamada a Azure OpenAI."

---

### 3. Lab Hands-On (35 minutos)

**Checkpoint intermedio (minuto 15-20)**:
- Verificar que todos tienen la API respondiendo
- Show of hands: "¿Quién ya recibió respuesta del endpoint de clima?"
- Ayudar a los que están atrasados

**Señales de alerta**:
- Si más del 30% no tiene la API corriendo al minuto 20, hacer pausa grupal
- Problemas comunes: API key no configurada, puerto en uso

**Distribución del tiempo del lab**:
- Pasos 1-4 (Setup): 10 minutos
- Pasos 5-7 (Testing): 15 minutos
- Pasos 8-10 (Validación): 10 minutos

---

### 4. Validación y Q&A (5 minutos)

**Método de validación**:
1. Pedir que levanten la mano quienes tengan:
   - ✅ API respondiendo en `/health`
   - ✅ WeatherAgent funcionando
   - ✅ Swagger UI visible

2. Seleccionar 2-3 participantes para mostrar su dashboard

**Preguntas frecuentes**:

**P: ¿Puedo usar esto en producción?**
> R: Sí, Aspire incluye templates para deployment a Azure Container Apps con `azd up`. El patrón es production-ready.

**P: ¿Cómo escalo a múltiples instancias?**
> R: Aspire maneja esto automáticamente con Container Apps. En el AppHost puedes especificar `.WithReplicas(3)` para escalar.

**P: ¿Qué pasa con la autenticación?**
> R: El proyecto incluye ejemplos comentados de JWT. En producción, agregarías Azure AD o tu proveedor de identidad.

---

## ⚠️ Problemas Comunes y Soluciones

### Problema 1: Puerto 15888 en uso

**Síntoma**: Dashboard no abre

**Solución rápida**:
```bash
dotnet run --project AppHost -- --dashboard-port 15889
```

### Problema 2: API key no encontrada

**Síntoma**: Error 500 al llamar endpoints

**Diagnóstico**:
```bash
cd WebApi && dotnet user-secrets list
```

**Solución**:
```bash
dotnet user-secrets set "AzureOpenAI:ApiKey" "la-api-key"
```

### Problema 3: Aspire workload no instalado

**Síntoma**: Error al compilar AppHost

**Solución**:
```bash
dotnet workload install aspire
```

### Problema 4: Timeout en respuestas

**Causa**: Rate limiting de Azure OpenAI

**Solución temporal**: Esperar 1 minuto entre requests o usar deployment con mayor quota

---

## 📈 Métricas de Éxito

| Métrica | Objetivo | Mínimo Aceptable |
|---------|----------|------------------|
| Participantes con API funcionando | 80% | 70% |
| Participantes que ven traces | 75% | 65% |
| Tiempo promedio de completación | 45 min | 55 min |

---

## 🎓 Puntos de Énfasis Pedagógico

### Conexión con módulos anteriores

- **Módulo 1**: "Los agentes que creamos ahora están expuestos como APIs"
- **Módulo 2**: "Los function tools podrían agregarse aquí también"
- **Módulo 3**: "Los workflows podrían orquestarse desde un endpoint"
- **Módulo 4**: "La observabilidad viene integrada con Aspire"

### Aplicación en el mundo real

> "Este patrón es exactamente lo que usarían para exponer agentes a una aplicación móvil, un chatbot web, o integrar con sistemas existentes via API."

---

## 📝 Notas de Ajuste de Timing

### Si vas adelantado (>10 min de sobra)

- Mostrar desafíos opcionales
- Demo de `azd up` para deployment
- Discutir patrones de producción (rate limiting, auth)

### Si vas atrasado (>10 min de retraso)

- Saltar pasos 8-9 del lab (explorar dashboard/Swagger)
- Enfocarse en: API corriendo + un request exitoso
- Dejar validación de traces como tarea opcional

---

## 🔗 Transición al Siguiente Módulo

**Script de cierre**:
> "Ahora que tienen una aplicación multi-agente en producción con observabilidad, en el siguiente módulo veremos DevUI - una herramienta especializada para debugging de agentes durante el desarrollo. DevUI complementa lo que Aspire ofrece con características específicas para inspeccionar conversaciones y llamadas a funciones."

---

## 📚 Recursos de Referencia

Para consultas durante el módulo:
- [.NET Aspire Docs](https://learn.microsoft.com/dotnet/aspire)
- [Aspire Troubleshooting](https://learn.microsoft.com/dotnet/aspire/troubleshooting)
- [ASP.NET Core Minimal APIs](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis)

---

**Última actualización**: 2026-01-12  
**Versión del lab**: 1.0

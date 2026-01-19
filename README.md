# Workshop: Introducción a Microsoft Agent Framework

**Bienvenido al workshop de Microsoft Agent Framework (MAF)**

Este workshop práctico te enseñará a construir agentes inteligentes utilizando Microsoft Agent Framework con C# y .NET 10. Aprenderás desde los conceptos fundamentales hasta patrones avanzados de orquestación multi-agente.

---

## 📑 Tabla de Contenidos

- [Requisitos Previos](#-requisitos-previos)
- [Estructura del Workshop](#-estructura-del-workshop)
  - [Módulo 1: Fundamentos](#módulo-1-fundamentos)
  - [Módulo 2: Function Tools y Composición](#módulo-2-function-tools-y-composición)
  - [Módulo 3: Workflows y Orquestación](#módulo-3-workflows-y-orquestación)
  - [Módulo 4: Observabilidad y Monitoreo](#módulo-4-observabilidad-y-monitoreo)
  - [Módulo 5: Integración con ASP.NET y Aspire](#módulo-5-integración-con-aspnet-y-aspire)
  - [Módulo 7: Model Context Protocol (MCP)](#módulo-7-model-context-protocol-mcp)
- [Comenzar](#-comenzar)
- [Recursos Adicionales](#-recursos-adicionales)
- [Estimación de Costos](#-estimación-de-costos)
- [Certificación y Siguientes Pasos](#-certificación-y-siguientes-pasos)
- [Soporte](#-soporte)

---

## 📋 Requisitos Previos

Antes de comenzar el workshop, asegúrate de tener instalado:

- **.NET 10 SDK** ([Descargar](https://dot.net/download))
- **Visual Studio Code** ([Descargar](https://code.visualstudio.com))
- **Git** ([Descargar](https://git-scm.com/downloads))
- **Suscripción de Azure** con acceso a **Azure OpenAI Service**

### Verificación de Requisitos

```bash
# Verificar .NET SDK
dotnet --version  # Debe mostrar 10.0.x

# Verificar Visual Studio Code
code --version

# Verificar Git
git --version
```

## 🎯 Estructura del Workshop

El workshop está organizado en **6 módulos progresivos**, cada uno diseñado para desarrollar habilidades específicas:

### [Módulo 1: Fundamentos](docs/modulo-01-fundamentos/)
**Duración**: 60 minutos | **Nivel**: Principiante

Aprende los conceptos básicos de Microsoft Agent Framework y crea tu primer agente conversacional.

**Temas cubiertos**:
- ¿Qué es Microsoft Agent Framework?
- Tipos de agentes y conceptos de orquestación
- Configuración de Azure OpenAI
- Primer agente "Hello World"

**Labs**:
- 01-hello-agent: Crear y ejecutar tu primer agente

---

### [Módulo 2: Function Tools y Composición](docs/modulo-02-function-tools/)
**Duración**: 55 minutos | **Nivel**: Intermedio

Extiende las capacidades de tus agentes mediante function tools personalizadas y composición de agentes.

**Temas cubiertos**:
- Definir y registrar function tools en C#
- Function calling automático
- Composición: usar agentes como herramientas

**Labs**:
- 01-custom-tool: Implementar una function tool personalizada
- 02-agent-as-tool: Composición de agentes

---

### [Módulo 3: Workflows y Orquestación](docs/modulo-03-workflows/)
**Duración**: 90 minutos | **Nivel**: Intermedio-Avanzado

Domina los patrones de orquestación multi-agente y persistencia de estado.

**Temas cubiertos**:
- Workflows secuenciales, paralelos y de delegación
- Group chat entre múltiples agentes
- Persistencia con Azure AI Agent Service
- Estrategias de terminación

**Labs**:
- 01-sequential: Workflow secuencial (Research → Write → Review)
- 02-parallel: Ejecución paralela de tareas independientes
- 03-delegation: Coordinator pattern con agentes especialistas
- 04-group-chat: Conversación colaborativa multi-agente
- 05-azure-agent-service: Persistencia y pause/resume workflows

---

### [Módulo 4: Observabilidad y Monitoreo](docs/modulo-04-observability/)
**Duración**: 60 minutos | **Nivel**: Intermedio

Implementa métricas, trazas distribuidas y dashboards para aplicaciones de agentes.

**Temas cubiertos**:
- OpenTelemetry con MAF
- Métricas de tokens y latencia
- Distributed tracing

**Labs**:
- 01-metrics-tokens: Recolección de métricas básicas
- 02-distributed-traces: Tracing multi-agente

---

### [Módulo 5: Integración con ASP.NET y Aspire](docs/modulo-05-aspnet-aspire/)
**Duración**: 60 minutos | **Nivel**: Avanzado

Construye aplicaciones web multi-agente con ASP.NET Core y .NET Aspire.

**Temas cubiertos**:
- Exponer agentes como APIs HTTP
- Dependency injection con MAF
- Orquestación con .NET Aspire
- Escalabilidad y producción

**Labs**:
- 01-multi-agent-web: Aplicación web completa con múltiples agentes (Capstone Project)

---

### [Módulo 7: Model Context Protocol (MCP)](docs/modulo-07-mcp/)
**Duración**: 15 minutos | **Nivel**: Conceptual

Comprende el Model Context Protocol para interoperabilidad entre frameworks de agentes.

**Temas cubiertos**:
- ¿Qué es MCP y por qué importa?
- Interoperabilidad multi-vendor
- MAF como cliente MCP

**Labs**:
- 01-mcp-integration: Ejemplo de integración MCP (demostrativo)

---

## 🚀 Comenzar

### Clonar el Repositorio

```bash
git clone https://github.com/tu-org/maf-workshop.git
cd maf-workshop
```

### Configurar Azure OpenAI

1. **Crear recurso de Azure OpenAI**:
   - Ir al [Portal de Azure](https://portal.azure.com)
   - Crear un recurso "Azure OpenAI"
   - Región recomendada: East US 2 o Sweden Central

2. **Desplegar modelos**:
   - Desplegar **gpt-5.2** (modelo principal)
   - Desplegar **gpt-5.2-chat** (opcional, para conversaciones)

3. **Obtener credenciales**:
   - Copiar el **Endpoint** del recurso
   - Copiar una **API Key** desde "Keys and Endpoint"

4. **Configurar user secrets** (no commitear en Git):
   ```bash
   dotnet user-secrets set "AzureOpenAI:Endpoint" "https://tu-recurso.openai.azure.com/"
   dotnet user-secrets set "AzureOpenAI:ApiKey" "tu-api-key"
   dotnet user-secrets set "AzureOpenAI:DeploymentName" "gpt-5.2"
   ```

### Ejecutar tu Primer Lab

```bash
# Navegar al primer lab
cd docs/modulo-01-fundamentos/labs/01-hello-agent

# Restaurar dependencias
dotnet restore

# Ejecutar
dotnet run
```

**Salida esperada**:
```
🤖 Agent: ¡Hola! Soy un agente de Microsoft Agent Framework. ¿En qué puedo ayudarte?
```

---

## 📚 Recursos Adicionales

### Documentación Oficial
- [Microsoft Agent Framework Docs](https://learn.microsoft.com/microsoft-agent-framework)
- [Azure OpenAI Service](https://learn.microsoft.com/azure/ai-services/openai/)
- [.NET Aspire](https://learn.microsoft.com/dotnet/aspire)

### Guías del Instructor
- [Setup Checklist](instructor-guide/setup-checklist.md) - Preparación pre-workshop
- [Timing Schedule](instructor-guide/timing-schedule.md) - Cronograma detallado
- [Validation Checkpoints](instructor-guide/validation-checkpoints.md) - Criterios de éxito

### Solución de Problemas
- [Troubleshooting Guide](docs/troubleshooting.md) - Errores comunes y soluciones

---

## 💰 Estimación de Costos

El workshop utiliza **Azure OpenAI Service** con modelo de pago por uso:

| Concepto | Costo Estimado |
|----------|----------------|
| Workshop completo (7h) por participante | $8-15 USD |
| Módulos 1-2 únicamente (3h) | $3-5 USD |
| Pruebas pre-workshop (instructor) | $5-10 USD |

**Recomendación**: Configurar alertas de presupuesto en Azure antes del workshop.

---

## 🎓 Certificación y Siguientes Pasos

Después de completar el workshop:

1. **Microsoft Applied Skills**: [Build a copilot with Azure AI](https://learn.microsoft.com/credentials/)
2. **Proyecto Personal**: Implementa un agente para tu dominio específico
3. **Contribución**: Comparte tus experiencias y mejoras en GitHub

---

## 📧 Soporte

**Para instructores**:
- Revisa la [Guía del Instructor](instructor-guide/README.md)
- Contacta al equipo de soporte: support@example.com

**Para participantes**:
- Consulta el [Troubleshooting Guide](docs/troubleshooting.md)
- Pregunta a tu instructor durante el workshop

---

## 📄 Licencia

Este material educativo está disponible bajo licencia MIT. Ver [LICENSE](LICENSE) para más detalles.

---

**¡Disfruta el workshop y construye agentes increíbles!** 🚀🤖

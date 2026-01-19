# Feature Specification: Workshop de Introducción a Microsoft Agent Framework

**Feature Branch**: `001-maf-workshop`  
**Created**: January 12, 2026  
**Status**: Draft  
**Input**: User description: "Build a hands on lab workshop for introduction of Microsoft Agent Framework. Write the documentation and labs using markdown. For samples and labs code use C#. Everything in Spanish."

## Clarifications

### Session 2026-01-12

- Q: ¿Cuál es la estrategia para participantes sin acceso a Azure cuando los módulos 3-4 requieren Azure AI Agent Service y Azure Monitor? → A: Azure es obligatorio - los módulos 3 y 4 requieren subscripción activa de Azure y no se proporcionan alternativas
- Q: ¿Cuál es el nivel de complejidad y profundidad de los laboratorios - tutorial guiado, ejercicios prácticos intermedios, o desafíos avanzados? → A: Simple paso-a-paso básico - código completamente pre-escrito, participantes solo copian/pegan y ejecutan
- Q: ¿Qué modelos o proveedores de AI (OpenAI, Azure OpenAI, modelos locales) deben usar los ejemplos y laboratorios? → A: Solo Azure OpenAI - todos los labs usan exclusivamente Azure OpenAI Service (consistente con requisito Azure)
- Q: ¿Cómo se medirán o validarán los porcentajes de éxito especificados en los criterios (ej. "90% de participantes ejecuta Hello Agent exitosamente")? → A: Validación manual - instructor verifica completitud en checkpoints durante el workshop, recolecta métricas de éxito
- Q: ¿El workshop está diseñado para entrega presencial, virtual/online, auto-guiado, o híbrido? → A: Presencial guiado por instructor - workshop en persona con instructor presente para validación en checkpoints

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Aprender Fundamentos y Crear Primer Agente (Priority: P1)

Los participantes del workshop necesitan comprender los conceptos básicos de Microsoft Agent Framework y crear su primer agente funcional para establecer una base sólida antes de avanzar a temas más complejos.

**Why this priority**: Es el punto de entrada esencial que habilita todo el aprendizaje posterior. Sin estos fundamentos, los participantes no podrán aprovechar los módulos siguientes.

**Independent Test**: El participante puede completar exitosamente el módulo 1, configurar el entorno .NET, y ejecutar el demo 'Hello Agent' que responde correctamente a una consulta básica en consola.

**Acceptance Scenarios**:

1. **Given** un participante sin conocimiento previo de MAF, **When** lee la documentación del módulo 1, **Then** comprende qué es MAF, los tipos de agentes disponibles, y los conceptos de orquestación, MCP y A2A
2. **Given** un entorno con .NET instalado y Azure OpenAI configurado, **When** el participante sigue las instrucciones de instalación, **Then** puede ejecutar el demo 'Hello Agent' sin errores
3. **Given** el demo 'Hello Agent' ejecutándose, **When** el participante envía una consulta, **Then** el agente responde correctamente usando Azure OpenAI demostrando funcionalidad básica

---

### User Story 2 - Implementar Function Tools con Agentes (Priority: P1)

Los participantes necesitan aprender a extender las capacidades de los agentes mediante function tools, incluyendo el uso de agentes como herramientas y la implementación de aprobaciones humanas para casos críticos.

**Why this priority**: Las function tools son el mecanismo principal para que los agentes realicen acciones concretas, convirtiéndolos de chatbots simples en agentes funcionales capaces de ejecutar tareas.

**Independent Test**: El participante completa el módulo 2 y crea un agente que utiliza al menos una function tool personalizada, demuestra el uso de un agente como tool, e implementa un flujo con aprobación humana.

**Acceptance Scenarios**:

1. **Given** el conocimiento del módulo 1, **When** el participante lee la documentación de function tools, **Then** comprende cómo definir, registrar y usar function tools con un agente
2. **Given** un laboratorio guiado, **When** el participante implementa una function tool personalizada, **Then** el agente puede invocarla correctamente para realizar una tarea específica
3. **Given** un escenario que requiere composición de agentes, **When** el participante configura un agente como function tool, **Then** un agente principal puede delegar trabajo al agente secundario
4. **Given** una operación sensible, **When** el participante implementa human-in-the-loop approval, **Then** el agente solicita confirmación antes de ejecutar la action

---

### User Story 3 - Diseñar e Implementar Workflows Multi-paso (Priority: P2)

Los participantes necesitan aprender a orquestar múltiples agentes y tareas mediante workflows, comprendiendo patrones avanzados como Planner+Executor y la gestión de estado en Azure AI Agent Service.

**Why this priority**: Los workflows permiten construir soluciones de agentes más sofisticadas y realistas, preparando a los participantes para escenarios empresariales complejos.

**Independent Test**: El participante completa el módulo 3 y crea al menos un workflow de cada tipo (secuencial, paralelo, delegación, group chat), implementa un patrón Planner+Executor, y demuestra persistencia de estado.

**Acceptance Scenarios**:

1. **Given** comprensión de function tools, **When** el participante lee sobre workflows, **Then** entiende los 4 tipos de workflows y sus casos de uso apropiados
2. **Given** un laboratorio de workflow secuencial, **When** el participante implementa una cadena de tareas, **Then** el workflow ejecuta cada paso en orden con el output correcto
3. **Given** un laboratorio de workflow paralelo, **When** el participante configura tareas concurrentes, **Then** múltiples operaciones se ejecutan simultáneamente y los resultados se agregan correctamente
4. **Given** un escenario de delegación, **When** el participante implementa un workflow de delegación, **Then** un agente coordinador distribuye subtareas a agentes especializados
5. **Given** un escenario colaborativo, **When** el participante implementa un group chat workflow, **Then** múltiples agentes intercambian mensajes para resolver un problema conjunto
6. **Given** Azure AI Agent Service configurado, **When** el participante implementa persistencia de estado, **Then** el workflow puede pausarse y reanudarse manteniendo el contexto

---

### User Story 4 - Implementar Observabilidad Completa (Priority: P2)

Los participantes necesitan aprender a monitorear, trazar y diagnosticar aplicaciones de agentes en producción usando métricas, logs estructurados, trazas distribuidas y dashboards.

**Why this priority**: La observabilidad es crítica para operar agentes en producción de manera confiable y cumplir con SLAs, pero puede enseñarse independientemente de las capacidades funcionales de los agentes.

**Independent Test**: El participante completa el módulo 4 y configura un agente con métricas de latencia/errores/tokens, implementa trazas con OpenTelemetry, crea logs PII-safe, y visualiza todo en Azure Monitor con alertas.

**Acceptance Scenarios**:

1. **Given** un agente funcional, **When** el participante instrumenta métricas, **Then** puede capturar y visualizar latencia, tasa de errores, y consumo de tokens
2. **Given** OpenTelemetry configurado, **When** el participante implementa trazas distribuidas, **Then** puede seguir una solicitud completa a través de múltiples agentes y servicios
3. **Given** requisitos de privacidad, **When** el participante implementa logs estructurados, **Then** los logs capturan información útil sin exponer datos sensibles (PII-safe)
4. **Given** Azure Monitor configurado, **When** el participante crea dashboards, **Then** puede visualizar todas las métricas y trazas en tiempo real
5. **Given** SLAs definidos, **When** el participante configura alertas, **Then** recibe notificaciones cuando se violan umbrales de SLO/SLA

---

### User Story 5 - Construir Aplicación Multi-agente con ASP.NET y Aspire (Priority: P3)

Los participantes necesitan aprender a integrar MAF en aplicaciones web modernas usando ASP.NET y orquestar servicios con .NET Aspire para desplegar soluciones escalables.

**Why this priority**: Representa la integración de MAF en arquitecturas de aplicaciones reales, construyendo sobre todos los conceptos previos. Es importante pero requiere todos los fundamentos anteriores.

**Independent Test**: El participante completa el módulo 5 y crea una aplicación ASP.NET que expone múltiples agentes mediante APIs, orquestada con Aspire, que puede desplegarse y escalarse.

**Acceptance Scenarios**:

1. **Given** conocimiento de workflows y observabilidad, **When** el participante crea un proyecto ASP.NET con MAF, **Then** puede exponer agentes como endpoints HTTP/API
2. **Given** múltiples servicios de agentes, **When** el participante configura .NET Aspire, **Then** puede orquestar, monitorear y gestionar todos los servicios desde una sola interfaz
3. **Given** una aplicación web funcional, **When** el participante realiza pruebas de carga, **Then** el sistema escala apropiadamente manejando múltiples solicitudes concurrentes
4. **Given** mejores prácticas de arquitectura, **When** el participante implementa separación de concerns, **Then** la aplicación tiene controladores, servicios de agentes, y persistencia correctamente desacoplados

---

### User Story 6 - Explorar y Utilizar DevUI para Desarrollo y Depuración (Priority: P2)

Los participantes necesitan aprender a usar DevUI, la interfaz de desarrollo de MAF, para depurar, probar, e interactuar con agentes durante el desarrollo, mejorando la productividad y facilitando el troubleshooting.

**Why this priority**: DevUI es una herramienta esencial de productividad para desarrolladores que trabajan con MAF, permitiendo iteración rápida y depuración efectiva. Es importante para el flujo de trabajo de desarrollo pero no es requisito para operación en producción.

**Independent Test**: El participante completa el módulo 6, configura y ejecuta DevUI, puede visualizar conversaciones de agentes, inspeccionar estados internos, y usar la interfaz para probar diferentes escenarios interactivamente.

**Acceptance Scenarios**:

1. **Given** conocimiento de fundamentos MAF, **When** el participante lee sobre DevUI, **Then** comprende qué es DevUI, sus capacidades, y casos de uso para desarrollo y depuración
2. **Given** un agente MAF funcional, **When** el participante configura y lanza DevUI, **Then** puede ver la interfaz web y conectarse al agente
3. **Given** DevUI ejecutándose conectado a un agente, **When** el participante envía mensajes de prueba, **Then** puede visualizar la conversación completa, mensajes del sistema, y respuestas del agente en tiempo real
4. **Given** un agente con function tools, **When** el participante usa DevUI para inspeccionar, **Then** puede ver las invocaciones de tools, parámetros, y resultados
5. **Given** un escenario de depuración, **When** el participante usa DevUI para analizar el comportamiento del agente, **Then** puede identificar problemas y ajustar la configuración iterativamente

---

### User Story 7 - Integrar Model Context Protocol (MCP) con MAF (Priority: P3)

Los participantes necesitan comprender qué es MCP (Model Context Protocol), por qué es importante para la interoperabilidad de agentes, y cómo integrarlo con Microsoft Agent Framework.

**Why this priority**: MCP es un estándar emergente importante para la interoperabilidad, pero los participantes pueden construir agentes funcionales sin él. Es conocimiento avanzado que complementa el workshop.

**Independent Test**: El participante completa el módulo 7, comprende los conceptos de MCP, y puede demostrar un agente MAF que se comunica con otro componente usando el protocolo MCP.

**Acceptance Scenarios**:

1. **Given** conocimiento de fundamentos MAF, **When** el participante lee sobre MCP, **Then** comprende qué es MCP, sus beneficios, y casos de uso para interoperabilidad de agentes
2. **Given** un servidor MCP de ejemplo, **When** el participante configura la integración con MAF, **Then** un agente MAF puede consumir recursos/herramientas expuestas vía MCP
3. **Given** un agente MAF existente, **When** el participante lo expone vía MCP, **Then** otros clientes MCP pueden descubrir y usar las capacidades del agente
4. **Given** un escenario multi-proveedor, **When** el participante integra agentes de diferentes frameworks vía MCP, **Then** los agentes pueden colaborar independientemente de la tecnología subyacente

---

### Edge Cases

- ¿Qué sucede cuando un participante usa una versión diferente de .NET (no la recomendada)?
- ¿Cómo se asegura que el código pre-escrito funcione para participantes con poca experiencia en C# sin requerir debugging?
- ¿Cómo se comunica a los participantes el requisito obligatorio de Azure antes del workshop para evitar sorpresas?
- ¿Cómo se manejan errores comunes de configuración del entorno (API keys, permisos de Azure, networking)?
- ¿Qué alternativas se ofrecen si un laboratorio específico no funciona en el sistema del participante (excluyendo Azure que es obligatorio)?
- ¿Cómo se adapta el contenido si se agrega tiempo o se reduce (workshops de 3h vs 9h)?
- ¿Cómo maneja el instructor participantes que avanzan a diferentes velocidades en un formato presencial?

## Requirements *(mandatory)*

### Functional Requirements

#### Contenido y Estructura

- **FR-001**: El workshop DEBE organizarse en 7 módulos claramente diferenciados que correspondan a: (1) Fundamentos, (2) Function Tools, (3) Workflows, (4) Observabilidad, (5) Multi-agente con ASP.NET/Aspire, (6) DevUI, (7) MCP
- **FR-001a**: El workshop DEBE diseñarse para entrega presencial con instructor presente durante toda la sesión
- **FR-002**: Cada módulo DEBE incluir documentación teórica en markdown que explique conceptos, arquitectura, y mejores prácticas
- **FR-003**: Cada módulo DEBE incluir al menos un laboratorio práctico hands-on con instrucciones paso a paso y código completo pre-escrito listo para copiar/pegar y ejecutar
- **FR-004**: Todo el contenido (documentación, comentarios de código, instrucciones de laboratorios) DEBE estar en español
- **FR-005**: Los ejemplos de código y laboratorios DEBEN usar C# como lenguaje de programación
- **FR-006**: Los laboratorios DEBEN ser progresivos, construyendo sobre conceptos aprendidos en módulos anteriores

#### Módulo 1: Fundamentos

- **FR-007**: El módulo 1 DEBE explicar qué es Microsoft Agent Framework y sus casos de uso
- **FR-008**: El módulo 1 DEBE documentar los diferentes tipos de agentes disponibles en MAF
- **FR-009**: El módulo 1 DEBE explicar conceptos de orquestación de agentes
- **FR-010**: El módulo 1 DEBE describir los protocolos MCP (Model Context Protocol) y A2A (Agent-to-Agent)
- **FR-011**: El módulo 1 DEBE incluir instrucciones de instalación para .NET y MAF
- **FR-011a**: El módulo 1 DEBE incluir instrucciones de configuración de Azure OpenAI Service (deployment, API keys, endpoints)
- **FR-012**: El módulo 1 DEBE incluir un demo 'Hello Agent' completamente funcional y documentado

#### Módulo 2: Function Tools

- **FR-013**: El módulo 2 DEBE explicar qué son las function tools y cómo extender las capacidades de los agentes
- **FR-014**: El módulo 2 DEBE incluir un laboratorio que demuestre el uso de function tools con un agente
- **FR-015**: El módulo 2 DEBE incluir un laboratorio que demuestre el uso de un agente como function tool (composición de agentes)
- **FR-016**: El módulo 2 DEBE incluir un laboratorio que demuestre human-in-the-loop approvals para operaciones críticas

#### Módulo 3: Workflows

- **FR-017**: El módulo 3 DEBE documentar los 4 tipos de workflows: secuencial, paralelo, delegación, y group chat
- **FR-018**: El módulo 3 DEBE incluir laboratorios de cada tipo de workflow con ejemplos prácticos
- **FR-019**: El módulo 3 DEBE explicar e implementar el patrón Planner + Executor
- **FR-020**: El módulo 3 DEBE explicar e implementar el patrón Human-in-the-loop en workflows
- **FR-021**: El módulo 3 DEBE documentar la integración con Azure AI Agent Service
- **FR-022**: El módulo 3 DEBE incluir laboratorio sobre control de estado y persistencia de workflows

#### Módulo 4: Observabilidad

- **FR-023**: El módulo 4 DEBE documentar cómo capturar métricas de latencia, errores, y consumo de tokens
- **FR-024**: El módulo 4 DEBE explicar e implementar trazas distribuidas usando OpenTelemetry
- **FR-025**: El módulo 4 DEBE documentar mejores prácticas para logs estructurados y PII-safe (protección de datos sensibles)
- **FR-026**: El módulo 4 DEBE incluir laboratorio de creación de dashboards en Azure Monitor
- **FR-027**: El módulo 4 DEBE documentar configuración de alertas y definición de SLO/SLA

#### Módulo 5: Multi-agente con ASP.NET y Aspire

- **FR-028**: El módulo 5 DEBE demostrar la integración de MAF con aplicaciones ASP.NET
- **FR-029**: El módulo 5 DEBE explicar cómo exponer agentes mediante APIs HTTP/REST
- **FR-030**: El módulo 5 DEBE documentar el uso de .NET Aspire para orquestación de servicios
- **FR-031**: El módulo 5 DEBE incluir un laboratorio completo de aplicación multi-agente escalable

#### Módulo 6: DevUI

- **FR-045**: El módulo 6 DEBE explicar qué es DevUI y su propósito como herramienta de desarrollo para MAF
- **FR-046**: El módulo 6 DEBE documentar las capacidades de DevUI para depuración, testing, e inspección de agentes
- **FR-047**: El módulo 6 DEBE incluir instrucciones de configuración y ejecución de DevUI
- **FR-048**: El módulo 6 DEBE incluir laboratorio que demuestre el uso de DevUI para visualizar conversaciones y estados de agentes
- **FR-049**: El módulo 6 DEBE documentar cómo usar DevUI para inspeccionar invocaciones de function tools y depurar comportamiento de agentes
- **FR-050**: El módulo 6 DEBE incluir referencias a la documentación oficial de DevUI en Microsoft Learn

#### Módulo 7: MCP

- **FR-032**: El módulo 7 DEBE explicar qué es Model Context Protocol (MCP) y su propósito
- **FR-033**: El módulo 7 DEBE documentar los beneficios de MCP para interoperabilidad de agentes
- **FR-034**: El módulo 7 DEBE incluir laboratorio de integración de MCP con Microsoft Agent Framework
- **FR-035**: El módulo 7 DEBE demostrar casos de uso de comunicación entre agentes usando MCP

#### Requisitos Técnicos y de Formato

- **FR-036**: Todos los documentos de laboratorio DEBEN usar formato markdown
- **FR-037**: Los ejemplos de código DEBEN incluir comentarios explicativos en español y estar completos (listos para ejecutar sin modificaciones)
- **FR-038**: Los laboratorios DEBEN incluir prerequisitos claros (software necesario, configuración previa)
- **FR-038a**: El workshop DEBE comunicar claramente que una subscripción activa de Azure es obligatoria para completar los módulos 3 y 4
- **FR-038b**: Todos los laboratorios DEBEN usar exclusivamente Azure OpenAI Service como proveedor de modelos de lenguaje
- **FR-039**: Los laboratorios DEBEN incluir pasos de validación para verificar que funcionan correctamente
- **FR-039a**: Cada módulo DEBE incluir checkpoints claros donde el instructor puede verificar manualmente el progreso y éxito de los participantes
- **FR-040**: El contenido DEBE incluir referencias a documentación oficial de Microsoft cuando sea apropiado
- **FR-041**: Los ejemplos de código DEBEN seguir las convenciones de estilo de C# y .NET
- **FR-042**: Los laboratorios DEBEN incluir sección de troubleshooting para errores comunes
- **FR-043**: El workshop DEBE incluir una guía para instructores con checkpoints de validación y métricas a recolectar para medir el éxito de cada módulo
- **FR-044**: La guía del instructor DEBE incluir timing recomendado para cada sección, puntos de pausa, y estrategias para mantener al grupo sincronizado en formato presencial

### Key Entities

- **Módulo del Workshop**: Cada uno de los 6 módulos temáticos que contienen documentación teórica, laboratorios prácticos, y código de ejemplo diseñados para entrega presencial. Atributos: número de módulo, título, objetivos de aprendizaje, prerequisitos, duración estimada, lista de laboratorios, checkpoints de validación para instructor
- **Laboratorio Práctico**: Ejercicio hands-on que los participantes completan para aplicar conceptos mediante un enfoque simple de copiar/pegar código pre-escrito. Atributos: nombre, descripción, prerequisitos técnicos, pasos detallados paso-a-paso, código completo funcional listo para copiar, validación de resultados, troubleshooting
- **Ejemplo de Código**: Snippets o proyectos completos en C# que demuestran conceptos, completamente funcionales sin requerir modificaciones. Atributos: lenguaje (C#), propósito, módulo asociado, dependencias, instrucciones de ejecución, salida esperada
- **Recurso de Documentación**: Material teórico que explica conceptos de MAF. Atributos: título, contenido en markdown, módulo asociado, conceptos cubiertos, diagramas/imágenes, enlaces a documentación oficial
- **Prerequisito Técnico**: Software, configuración, o conocimiento necesario para completar el workshop. Atributos: nombre, tipo (software/configuración/conocimiento), instrucciones de instalación/configuración, versión requerida, obligatorio/opcional. Nota: Subscripción de Azure con Azure OpenAI Service es obligatoria para todos los módulos

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Los participantes pueden completar todos los 7 módulos en un tiempo razonable de 7-9 horas (1-1.5 horas por módulo promedio)
- **SC-002**: El 90% de los participantes puede ejecutar exitosamente el demo 'Hello Agent' sin asistencia después de completar el módulo 1 (validado manualmente por instructor en checkpoint)
- **SC-003**: El 85% de los participantes puede implementar correctamente un function tool personalizado después de completar el módulo 2 (validado manualmente por instructor en checkpoint)
- **SC-004**: El 80% de los participantes puede crear al menos un workflow funcional de cada tipo después de completar el módulo 3 (validado manualmente por instructor en checkpoint)
- **SC-005**: El 75% de los participantes puede configurar observabilidad básica (métricas + logs + trazas) después de completar el módulo 4 (validado manualmente por instructor en checkpoint)
- **SC-006**: El 70% de los participantes puede desplegar una aplicación multi-agente con ASP.NET y Aspire después de completar el módulo 5 (validado manualmente por instructor en checkpoint)
- **SC-007**: El 85% de los participantes puede configurar y ejecutar DevUI exitosamente después de completar el módulo 6 (validado manualmente por instructor en checkpoint)
- **SC-007a**: El 80% de los participantes puede usar DevUI para visualizar conversaciones de agentes y depurar function tools después de completar el módulo 6 (validado manualmente por instructor en checkpoint)
- **SC-007b**: El 80% de los participantes comprende qué es MCP y puede explicar su propósito después de completar el módulo 7 (validado mediante preguntas del instructor)
- **SC-008**: Los laboratorios pueden ejecutarse sin errores en entornos estándar de desarrollo .NET (Windows, Linux, macOS)
- **SC-009**: Los participantes califican el workshop con al menos 4/5 estrellas en claridad de contenido y utilidad práctica
- **SC-010**: Los participantes reportan sentirse capaces de aplicar los conceptos aprendidos en proyectos reales después de completar el workshop

# Checklist de Preparación Pre-Workshop

**Workshop**: Introducción a Microsoft Agent Framework  
**Tiempo de preparación**: 1-2 semanas antes  
**Responsable**: Instructor

---

## 📋 2 Semanas Antes del Workshop

### Azure Resources Setup

- [ ] **Crear grupo de recursos** `rg-maf-workshop` en región East US 2 o Sweden Central
- [ ] **Crear recurso de Azure OpenAI** en el grupo de recursos
- [ ] **Desplegar modelo gpt-5.2** con capacidad de 50 TPM mínimo
- [ ] **Desplegar modelo gpt-5.2-chat** (opcional, para ejemplos conversacionales)
- [ ] **Obtener y guardar**:
  - Endpoint de Azure OpenAI
  - API Key (Key 1)
  - Deployment names
- [ ] **Crear proyecto de Azure AI Foundry** para Módulo 3 (Azure AI Agent Service)
- [ ] **Crear Application Insights** para Módulo 4 (Observabilidad)
- [ ] **Obtener connection string** de Application Insights
- [ ] **Configurar alertas de presupuesto**:
  - Alert 1: $100 USD (80% de $125)
  - Alert 2: $150 USD (120% - alerta crítica)

### Verificación de Azure

- [ ] **Probar conectividad** a Azure OpenAI desde red local
- [ ] **Verificar cuota** de TPM suficiente (mínimo 50,000 TPM para 20 participantes)
- [ ] **Probar deployment** enviando request de prueba con curl/Postman
- [ ] **Documentar endpoint y keys** en documento seguro (Password Manager)

---

## 📋 1 Semana Antes del Workshop

### Materiales del Workshop

- [ ] **Clonar repositorio** del workshop en máquina del instructor
- [ ] **Ejecutar todos los labs** end-to-end:
  - [ ] Módulo 1: Hello Agent
  - [ ] Módulo 2: Custom Tool, Agent-as-Tool, Human Approval
  - [ ] Módulo 3: Sequential, Parallel, Delegation, Group Chat, Azure Agent Service
  - [ ] Módulo 4: Metrics, Traces, Azure Monitor
  - [ ] Módulo 5: Multi-Agent Web (Capstone)
  - [ ] Módulo 7: MCP Demo
- [ ] **Documentar tiempo real** de cada lab (comparar con estimaciones)
- [ ] **Identificar errores comunes** y agregar a troubleshooting guide
- [ ] **Crear copia de seguridad** de todos los labs en USB drive
- [ ] **Preparar laptop de backup** con ambiente configurado

### Ambiente de Desarrollo

- [ ] **Verificar instalación** en laptop del instructor:
  - [ ] .NET 10 SDK (`dotnet --version`)
  - [ ] Visual Studio Code (última versión)
  - [ ] Git (`git --version`)
  - [ ] Azure CLI (`az --version`)
- [ ] **Configurar user secrets** para Azure OpenAI en todos los labs
- [ ] **Probar ejecución** desde carpeta limpia (simular participante)

### Comunicación con Participantes

- [ ] **Enviar email de pre-work** con:
  - [ ] Requisitos de software (.NET 10, VS Code, Git)
  - [ ] Comandos de verificación
  - [ ] Instrucciones de clonar repositorio
  - [ ] Información de suscripción de Azure (si aplica)
- [ ] **Compartir agenda detallada** del workshop
- [ ] **Enviar recordatorio** de verificar ambiente 2 días antes

---

## 📋 3 Días Antes del Workshop

### Logística del Venue

- [ ] **Confirmar reserva** de sala
- [ ] **Verificar capacidad** de sala (asientos, mesas, electricidad)
- [ ] **Probar proyector/pantalla** con laptop del instructor
- [ ] **Verificar conexión a internet** del venue:
  - [ ] Velocidad de descarga (> 50 Mbps)
  - [ ] Velocidad de subida (> 10 Mbps)
  - [ ] Acceso a Azure endpoints (*.openai.azure.com, *.microsoft.com)
- [ ] **Obtener credenciales de WiFi** para participantes
- [ ] **Identificar ubicación** de tomas de corriente
- [ ] **Confirmar horarios** de breaks y lunch

### Recursos de Backup

- [ ] **Crear hotspot móvil** de respaldo (en caso de falla de WiFi)
- [ ] **Descargar packages de NuGet** offline (carpeta local)
- [ ] **Preparar USB drives** (3-5) con:
  - Repositorio completo
  - .NET 10 SDK installers (Windows, macOS, Linux)
  - VS Code installer
  - Soluciones completas de todos los labs
- [ ] **Configurar segundo recurso de Azure OpenAI** en región diferente (fallback para límites de cuota)

---

## 📋 1 Día Antes del Workshop

### Verificación Final

- [ ] **Re-ejecutar todos los labs** en ambiente fresco
- [ ] **Verificar Azure resources** están activos:
  - [ ] OpenAI deployments running
  - [ ] Application Insights recibiendo datos de prueba
  - [ ] Azure AI Foundry project accessible
- [ ] **Probar desde red del venue** (si es posible acceso anticipado)
- [ ] **Cargar laptop y backups**
- [ ] **Imprimir materiales físicos**:
  - [ ] Quick Reference Card (1 por participante)
  - [ ] Troubleshooting Guide (2 copias para instructor)
  - [ ] Azure credentials (en sobres cerrados si se distribuyen individuales)

### Dashboard de Monitoreo

- [ ] **Abrir Azure Monitor dashboard** en tab del navegador
- [ ] **Configurar alertas en tiempo real** (Slack/Teams/Email)
- [ ] **Preparar queries de KQL** para diagnosticar problemas comunes:
  ```kql
  // Rate limit errors en últimos 5 minutos
  exceptions
  | where timestamp > ago(5m)
  | where outerMessage contains "429"
  | summarize count() by cloud_RoleName
  ```

### Preparación Personal

- [ ] **Revisar agenda y timing**
- [ ] **Preparar introducción** (15 min)
- [ ] **Revisar checkpoints de validación** para cada módulo
- [ ] **Tener a mano contactos de soporte técnico** (Azure, IT del venue)
- [ ] **Dormir bien** 😴

---

## 📋 Día del Workshop - Mañana (antes de inicio)

### Setup del Aula (90 minutos antes)

- [ ] **Llegar temprano** al venue
- [ ] **Configurar proyector** y pantalla
- [ ] **Probar audio** (si habrá video/demos con sonido)
- [ ] **Conectar laptop a proyector**
- [ ] **Abrir todas las herramientas necesarias**:
  - [ ] VS Code con tabs de todos los labs
  - [ ] Azure Portal (OpenAI, Application Insights)
  - [ ] Terminal con directorios de labs
  - [ ] Navegador con Aspire Dashboard
- [ ] **Probar conexión a Azure** desde laptop del instructor
- [ ] **Escribir información importante** en pizarra/flip chart:
  - WiFi credentials
  - Repositorio URL
  - Slack/Teams channel para Q&A
  - Timing de breaks

### Recepción de Participantes (30 minutos antes)

- [ ] **Dar bienvenida** a participantes que llegan
- [ ] **Ayudar con setup** de ambiente (si aplica)
- [ ] **Verificar que tengan**:
  - [ ] Laptop cargada
  - [ ] .NET 10 SDK instalado
  - [ ] VS Code instalado
  - [ ] Repositorio clonado
- [ ] **Distribuir materiales físicos** (Quick Reference Card)
- [ ] **Recolectar información de contacto** (para certificados y seguimiento)

---

## 📋 Durante el Workshop

### Antes de Cada Módulo

- [ ] **Anunciar objetivo** del módulo
- [ ] **Explicar timing** y cuándo es el siguiente break
- [ ] **Recordar checkpoint de validación** al final

### Después de Cada Lab

- [ ] **Checkpoint de validación** (show of hands, screen share, etc.)
- [ ] **Registrar % de éxito** en formulario de tracking
- [ ] **Resolver problemas comunes** con grupo
- [ ] **Dar 2-3 minutos** para preguntas antes de continuar

### Monitoreo Continuo

- [ ] **Revisar Azure Monitor** cada hora para rate limits
- [ ] **Caminar por el aula** durante labs para identificar problemas
- [ ] **Tomar notas** de preguntas frecuentes y errores comunes
- [ ] **Ajustar timing** si un módulo toma más/menos de lo estimado

---

## 📋 Post-Workshop

### Inmediatamente Después

- [ ] **Agradecer a participantes**
- [ ] **Compartir link de encuesta** de feedback
- [ ] **Compartir recursos adicionales** (links, certificados, comunidad)
- [ ] **Recolectar USB drives** de backup

### Seguimiento (1-3 días después)

- [ ] **Enviar email de seguimiento** con:
  - [ ] Link al repositorio
  - [ ] Grabación del workshop (si aplica)
  - [ ] Recursos adicionales
  - [ ] Certificados de participación
- [ ] **Revisar feedback** de encuesta
- [ ] **Documentar mejoras** para siguiente iteración
- [ ] **Calcular costos reales** de Azure (comparar con estimaciones)

### Limpieza de Azure

- [ ] **Eliminar recursos de prueba** que no se reutilizarán
- [ ] **Mantener recurso de OpenAI** si hay workshops futuros
- [ ] **Documentar costos finales** para presupuestos futuros

---

## 🚨 Checklist de Emergencia (para imprimir y llevar)

| Problema | Solución Rápida |
|----------|-----------------|
| **WiFi caído** | Activar hotspot móvil, distribuir credentials |
| **Azure OpenAI 429 errors** | Cambiar a deployment de backup en otra región |
| **Laptop del instructor falla** | Usar laptop de backup con ambiente pre-configurado |
| **Proyector no funciona** | Tener HDMI-to-USB adapter, cable de backup |
| **Participante sin .NET SDK** | USB drive con installer, o trabajar en pares |
| **Labs no ejecutan** | USB con NuGet packages offline, soluciones completas |

---

## ✅ Checklist Completo

**Total de items**: 100+

**Fecha completado**: _______________

**Instructor**: _______________

**Firma**: _______________

---

**Notas adicionales**:

_______________________________________________________________________________

_______________________________________________________________________________

_______________________________________________________________________________

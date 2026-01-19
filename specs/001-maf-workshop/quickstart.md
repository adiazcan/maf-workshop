# Quickstart: Workshop Microsoft Agent Framework

**Versión**: 1.0  
**Fecha**: 2026-01-12  
**Audiencia**: Instructores

## Propósito

Esta guía proporciona los pasos esenciales para preparar y ejecutar el workshop "Introducción a Microsoft Agent Framework" de manera exitosa. Diseñada para instructores que entregarán el workshop de forma presencial.

---

## Pre-Workshop Setup (1-2 semanas antes)

### 1. Verificar Infraestructura de Azure

#### 1.1 Crear Recurso de Azure OpenAI

```bash
# Configurar CLI de Azure
az login

# Crear grupo de recursos
az group create \
  --name rg-maf-workshop \
  --location eastus2

# Crear recurso de Azure OpenAI
az cognitiveservices account create \
  --name maf-workshop-openai \
  --resource-group rg-maf-workshop \
  --kind OpenAI \
  --sku S0 \
  --location eastus2
```

#### 1.2 Desplegar Modelos

```bash
# Desplegar gpt-5.2 (modelo principal)
az cognitiveservices account deployment create \
  --name maf-workshop-openai \
  --resource-group rg-maf-workshop \
  --deployment-name gpt-5.2 \
  --model-name gpt-5.2 \
  --model-version "2026-01-01" \
  --model-format OpenAI \
  --sku-capacity 50 \
  --sku-name "Standard"

# Desplegar gpt-5.2-chat (modelo alternativo)
az cognitiveservices account deployment create \
  --name maf-workshop-openai \
  --resource-group rg-maf-workshop \
  --deployment-name gpt-5.2-chat \
  --model-name gpt-5.2-chat \
  --model-version "2026-01-01" \
  --model-format OpenAI \
  --sku-capacity 50 \
  --sku-name "Standard"
```

#### 1.3 Obtener Credenciales

```bash
# Obtener endpoint
az cognitiveservices account show \
  --name maf-workshop-openai \
  --resource-group rg-maf-workshop \
  --query properties.endpoint \
  --output tsv

# Obtener API key
az cognitiveservices account keys list \
  --name maf-workshop-openai \
  --resource-group rg-maf-workshop \
  --query key1 \
  --output tsv
```

**Guardar** estas credenciales de forma segura para distribuir a participantes.

#### 1.4 Configurar Azure AI Agent Service (Módulo 3-4)

```bash
# Crear proyecto de Azure AI Foundry
az ml workspace create \
  --name maf-workshop-ai \
  --resource-group rg-maf-workshop \
  --location eastus2
```

#### 1.5 Configurar Azure Monitor (Módulo 4)

```bash
# Crear Application Insights
az monitor app-insights component create \
  --app maf-workshop-insights \
  --location eastus2 \
  --resource-group rg-maf-workshop \
  --application-type web

# Obtener connection string
az monitor app-insights component show \
  --app maf-workshop-insights \
  --resource-group rg-maf-workshop \
  --query connectionString \
  --output tsv
```

#### 1.6 Establecer Alertas de Presupuesto

```bash
# Crear alerta de costo (ejemplo: $200 USD)
az consumption budget create \
  --budget-name maf-workshop-budget \
  --resource-group rg-maf-workshop \
  --amount 200 \
  --time-grain Monthly \
  --start-date 2026-01-01 \
  --end-date 2026-12-31 \
  --notification enabled=true threshold=80 operator=GreaterThan contact-emails=instructor@example.com
```

---

## Ejecución del Workshop (Durante)

### 2. Estructura Temporal

#### Horario Sugerido (7 horas)

| Hora | Duración | Actividad |
|------|----------|-----------|
| 09:00-09:15 | 15 min | Bienvenida + Setup check |
| 09:15-09:30 | 15 min | Introducción teórica |
| 09:30-10:30 | 60 min | **Módulo 1: Fundamentos** |
| 10:30-10:45 | 15 min | ☕ BREAK |
| 10:45-12:00 | 75 min | **Módulo 2: Function Tools** |
| 12:00-13:00 | 60 min | 🍽️ LUNCH |
| 13:00-14:30 | 90 min | **Módulo 3: Workflows** |
| 14:30-14:45 | 15 min | ☕ BREAK |
| 14:45-15:45 | 60 min | **Módulo 4: Observabilidad** |
| 15:45-16:00 | 15 min | ☕ BREAK |
| 16:00-17:00 | 60 min | **Módulo 5: ASP.NET + Aspire** |
| 17:00-17:15 | 15 min | **Módulo 7: MCP** (overview) + Cierre |

**Total**: 7 horas (incluye breaks)

### 3. Checkpoints de Validación

#### Checkpoints por Módulo

**Módulo 1**:
- Checkpoint: "El agente responde a 'Hola' con una respuesta coherente"
- Meta: 90% de éxito

**Módulo 2**:
- Checkpoint: "El agente llama automáticamente a GetWeather() cuando preguntan sobre clima"
- Meta: 85% de éxito

**Módulo 3**:
- Checkpoint: "El workflow secuencial ejecuta 3 pasos en orden"
- Meta: 80% de éxito

**Módulo 4**:
- Checkpoint: "Métricas visibles en Azure Monitor dashboard"
- Meta: 75% de éxito

**Módulo 5**:
- Checkpoint: "API responde en http://localhost:5000/api/chat"
- Meta: 70% de éxito

**Módulo 7**:
- Checkpoint: "Explica qué es MCP en 1-2 frases"
- Meta: 80% de comprensión (evaluado con Q&A)

### 4. Gestión de Problemas Comunes

#### Problema: "dotnet command not found"

**Solución Rápida**:
1. Verificar instalación: descargar de https://dot.net
2. Reiniciar terminal
3. Si persiste, usar laptop de backup o compartir con vecino

#### Problema: "401 Unauthorized" al llamar Azure OpenAI

**Solución Rápida**:
1. Verificar API key en user secrets: `dotnet user-secrets list`
2. Re-set: `dotnet user-secrets set "AzureOpenAI:ApiKey" "..."`
3. Verificar endpoint es correcto

#### Problema: "429 Rate Limit Exceeded"

**Solución Rápida**:
1. Si es shared key: aumentar TPM en Azure Portal
2. Si es individual: esperar 1 minuto
3. Implementar retry logic (mostrar patrón)

#### Problema: "El agente no llama mi función"

**Solución Rápida**:
1. Verificar `[KernelFunction]` attribute presente
2. Verificar `[Description]` es claro
3. Verificar función está registrada: `kernel.Plugins.Add(...)`

#### Problema: Firewall/Proxy bloquea Azure

**Solución Rápida**:
1. Hotspot móvil
2. VPN corporativa con excepción
3. Usar computadora del instructor como proxy

---

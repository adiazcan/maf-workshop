# Azure AI Foundry Setup Guide

**Propósito**: Configurar Azure AI Foundry (anteriormente Azure AI Studio) para el Módulo 3 Lab 5 (Azure AI Agent Service).

**Tiempo requerido**: 15-20 minutos  
**Cuándo hacerlo**: Durante la preparación pre-workshop (1 semana antes)

---

## ¿Qué es Azure AI Foundry?

**Azure AI Foundry** es la plataforma unificada de Microsoft para construir, evaluar y desplegar soluciones de IA generativa. Incluye:

- **Azure AI Agent Service**: Servicio gestionado para agentes persistentes (usado en Módulo 3)
- **Prompt Flow**: Herramienta visual para workflows de IA
- **Model Catalog**: Acceso a modelos de OpenAI, Meta, Mistral, etc.
- **Evaluations**: Herramientas para evaluar calidad de respuestas

**Para este workshop**, necesitamos Azure AI Foundry específicamente para:
- Persistencia de threads (hilos de conversación)
- Estado de workflows multi-agente
- Pause/resume de ejecuciones

---

## Prerequisitos

Antes de comenzar, asegúrate de tener:

- [x] Suscripción de Azure activa
- [x] Permisos de **Contributor** o **Owner** en la suscripción
- [x] Azure CLI instalado y autenticado (`az login`)
- [x] Recurso de **Azure OpenAI** ya creado (del setup principal)

---

## Paso 1: Crear Proyecto de Azure AI Foundry

### Opción A: Usando Azure Portal (Recomendado para principiantes)

1. **Ir al Portal de Azure**:
   - Navegar a https://ai.azure.com (Azure AI Foundry Portal)
   - Iniciar sesión con credenciales de Azure

2. **Crear nuevo proyecto**:
   - Click en **+ New project**
   - Configurar:
     - **Project name**: `maf-workshop-ai`
     - **Subscription**: Seleccionar tu suscripción
     - **Resource group**: Usar existente `rg-maf-workshop` o crear nueva
     - **Location**: East US 2 o Sweden Central (misma región que Azure OpenAI)
     - **Hub**: Crear nuevo hub o usar existente

3. **Configurar Hub** (si es nuevo):
   - **Hub name**: `maf-workshop-hub`
   - **Resource group**: `rg-maf-workshop`
   - **Location**: East US 2
   - **Azure OpenAI**: Conectar al recurso existente
   - Click **Next** → **Create**

4. **Esperar creación** (3-5 minutos):
   - El portal creará automáticamente:
     - AI Hub resource
     - AI Project resource
     - Storage Account (para artifacts)
     - Key Vault (para secrets)
     - Application Insights (para telemetría)

---

### Opción B: Usando Azure CLI (Para automatización)

```bash
# Variables
SUBSCRIPTION_ID="your-subscription-id"
RESOURCE_GROUP="rg-maf-workshop"
LOCATION="eastus2"
HUB_NAME="maf-workshop-hub"
PROJECT_NAME="maf-workshop-ai"
OPENAI_RESOURCE_NAME="maf-workshop-openai"

# Establecer suscripción activa
az account set --subscription $SUBSCRIPTION_ID

# Crear AI Hub
az ml workspace create \
  --name $HUB_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --kind hub

# Crear AI Project (vinculado al hub)
az ml workspace create \
  --name $PROJECT_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --kind project \
  --hub-id "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP/providers/Microsoft.MachineLearningServices/workspaces/$HUB_NAME"

# Verificar creación
az ml workspace show \
  --name $PROJECT_NAME \
  --resource-group $RESOURCE_GROUP
```

---

## Paso 2: Conectar Azure OpenAI al Proyecto

### Desde Azure AI Foundry Portal

1. **Abrir el proyecto** `maf-workshop-ai` en https://ai.azure.com

2. **Navegar a Settings**:
   - Click en ⚙️ **Settings** (esquina superior derecha)
   - Seleccionar **Connected resources**

3. **Agregar conexión a Azure OpenAI**:
   - Click **+ New connection**
   - Tipo: **Azure OpenAI Service**
   - Configurar:
     - **Connection name**: `workshop-openai-connection`
     - **Subscription**: Tu suscripción
     - **Resource**: Seleccionar `maf-workshop-openai`
     - **Authentication**: API Key (más simple) o Microsoft Entra ID
   - Click **Add connection**

4. **Verificar conexión**:
   - La conexión debe aparecer en la lista con estado **Connected**

---

### Usando Azure CLI

```bash
# Crear conexión a Azure OpenAI
az ml connection create \
  --resource-group $RESOURCE_GROUP \
  --workspace-name $PROJECT_NAME \
  --name workshop-openai-connection \
  --type azure_open_ai \
  --target "https://$OPENAI_RESOURCE_NAME.openai.azure.com/" \
  --auth-mode key

# Listar conexiones
az ml connection list \
  --resource-group $RESOURCE_GROUP \
  --workspace-name $PROJECT_NAME
```

---

## Paso 3: Configurar Autenticación

### Opción 1: API Key (Más simple para workshop)

**Ya configurada** si usaste "API Key" en el paso anterior.

**Para código de agentes**:

```csharp
using Azure.AI.Projects;
using Azure;

var projectClient = new AIProjectClient(
    new Uri("https://<region>.api.azureml.ms"),
    new AzureKeyCredential("<api-key>")
);
```

**Obtener API Key del proyecto**:

```bash
# Desde Azure CLI
az ml workspace show \
  --name $PROJECT_NAME \
  --resource-group $RESOURCE_GROUP \
  --query "identity.principalId" \
  --output tsv
```

---

### Opción 2: Microsoft Entra ID / Managed Identity (Producción)

**Para participantes avanzados o escenarios de producción**.

**Configurar**:

1. **Asignar roles** en el proyecto:
   ```bash
   # Obtener ID del proyecto
   PROJECT_ID=$(az ml workspace show \
     --name $PROJECT_NAME \
     --resource-group $RESOURCE_GROUP \
     --query id -o tsv)
   
   # Asignar rol "Azure AI Developer" al usuario
   az role assignment create \
     --assignee <user-email> \
     --role "Azure AI Developer" \
     --scope $PROJECT_ID
   ```

2. **En código de agentes**:
   ```csharp
   using Azure.Identity;
   
   var projectClient = new AIProjectClient(
       new Uri("https://<region>.api.azureml.ms"),
       new DefaultAzureCredential()  // Usa Azure AD automáticamente
   );
   ```

---

## Paso 4: Obtener Project Connection String

El **connection string** es lo que los participantes usarán en el Lab 05 del Módulo 3.

### Desde Azure AI Foundry Portal

1. Abrir proyecto en https://ai.azure.com
2. Navegar a **Settings** → **Properties**
3. Copiar:
   - **Project ID** (GUID)
   - **Endpoint**: `https://<region>.api.azureml.ms`
   - **Subscription ID**
   - **Resource Group**

**Formato del connection string**:
```
ProjectId=<project-id>;SubscriptionId=<subscription-id>;ResourceGroup=<resource-group>;Endpoint=<endpoint>
```

---

### Usando Azure CLI

```bash
# Obtener todas las propiedades necesarias
az ml workspace show \
  --name $PROJECT_NAME \
  --resource-group $RESOURCE_GROUP \
  --query "{projectId:id, endpoint:discoveryUrl, subscriptionId:id}" \
  --output json
```

**Guardar este connection string** para distribuir a participantes.

---

## Paso 5: Crear Agent en Azure AI Agent Service (Prueba)

Verificar que todo funciona creando un agente de prueba.

### Código de Verificación

```csharp
using Azure.AI.Projects;
using Azure;

// Configurar cliente
var projectClient = new AIProjectClient(
    new Uri("https://eastus2.api.azureml.ms"),
    new AzureKeyCredential("your-api-key")
);

// Crear agente de prueba
var agent = await projectClient.CreateAgentAsync(
    model: "gpt-5.2",  // Debe coincidir con deployment en Azure OpenAI
    name: "TestAgent",
    instructions: "Eres un agente de prueba para verificar la configuración.",
    tools: Array.Empty<ToolDefinition>()
);

Console.WriteLine($"✅ Agent created: {agent.Value.Id}");

// Crear thread
var thread = await projectClient.CreateThreadAsync();
Console.WriteLine($"✅ Thread created: {thread.Value.Id}");

// Enviar mensaje de prueba
await projectClient.CreateMessageAsync(
    thread.Value.Id,
    MessageRole.User,
    "Hola, ¿estás funcionando?"
);

// Crear run
var run = await projectClient.CreateRunAsync(
    thread.Value.Id,
    agent.Value.Id
);

// Esperar completado
while (run.Value.Status == RunStatus.InProgress || run.Value.Status == RunStatus.Queued)
{
    await Task.Delay(1000);
    run = await projectClient.GetRunAsync(thread.Value.Id, run.Value.Id);
}

if (run.Value.Status == RunStatus.Completed)
{
    Console.WriteLine("✅ Run completed successfully!");
    
    // Obtener mensajes
    var messages = await projectClient.GetMessagesAsync(thread.Value.Id);
    var lastMessage = messages.Value.Data.FirstOrDefault();
    Console.WriteLine($"Response: {lastMessage?.Content?.FirstOrDefault()?.Text?.Value}");
}
else
{
    Console.WriteLine($"❌ Run failed with status: {run.Value.Status}");
}
```

**Ejecutar**:
```bash
dotnet run
```

**Output esperado**:
```
✅ Agent created: asst_abc123
✅ Thread created: thread_xyz789
✅ Run completed successfully!
Response: ¡Sí, estoy funcionando correctamente!
```

---

## Paso 6: Configurar para Participantes del Workshop

### Distribución de Credenciales

**Opción A: API Key compartida** (más simple)

1. Crear una API key del proyecto
2. Compartir connection string con todos:
   ```
   ProjectId=<id>;SubscriptionId=<sub-id>;ResourceGroup=<rg>;Endpoint=<endpoint>;ApiKey=<key>
   ```
3. Incluir en email de pre-work

**Opción B: API Keys individuales** (más seguro, más complejo)

1. Crear un Service Principal por participante
2. Asignar rol "Azure AI Developer"
3. Distribuir credenciales individuales

**Para este workshop, recomendamos Opción A** (API Key compartida con límites de cuota).

---

### Template de Configuración para Participantes

**Archivo**: `appsettings.json` (para Lab 05)

```json
{
  "AzureAI": {
    "ProjectId": "<project-id>",
    "SubscriptionId": "<subscription-id>",
    "ResourceGroup": "rg-maf-workshop",
    "Endpoint": "https://eastus2.api.azureml.ms",
    "ApiKey": "<shared-api-key>"
  },
  "AzureOpenAI": {
    "Endpoint": "https://maf-workshop-openai.openai.azure.com/",
    "DeploymentName": "gpt-5.2",
    "ApiKey": "<openai-api-key>"
  }
}
```

**O usando user secrets**:

```bash
dotnet user-secrets set "AzureAI:ProjectId" "<project-id>"
dotnet user-secrets set "AzureAI:Endpoint" "https://eastus2.api.azureml.ms"
dotnet user-secrets set "AzureAI:ApiKey" "<api-key>"
```

---

## Paso 7: Configurar Límites y Monitoreo

### Límites de Cuota

**Para evitar costos excesivos**:

1. **Navegar a proyecto** en Azure AI Foundry
2. **Settings** → **Quota**
3. Configurar límites:
   - **Max concurrent runs**: 20 (para 20 participantes)
   - **Max agents per project**: 30
   - **Max threads per agent**: 100

---

### Monitoreo de Uso

1. **Habilitar Application Insights** (ya debería estar activo)
2. **Configurar alertas**:
   ```bash
   # Alerta cuando número de runs > 100 en 1 hora
   az monitor metrics alert create \
     --name "High-Agent-Usage" \
     --resource-group $RESOURCE_GROUP \
     --scopes $PROJECT_ID \
     --condition "count runs > 100" \
     --window-size 1h \
     --evaluation-frequency 5m \
     --action <action-group-id>
   ```

3. **Dashboard en tiempo real**:
   - Abrir Azure AI Foundry Portal
   - **Monitoring** → **Metrics**
   - Crear gráficos para: Runs, Threads, Tokens used

---

## Troubleshooting Común

### Error: "Project not found"

**Causa**: Connection string incorrecto  
**Solución**: Verificar ProjectId y Endpoint en Azure Portal

---

### Error: "Insufficient permissions"

**Causa**: Usuario no tiene rol "Azure AI Developer"  
**Solución**:
```bash
az role assignment create \
  --assignee <user-email> \
  --role "Azure AI Developer" \
  --scope $PROJECT_ID
```

---

### Error: "Model 'gpt-5.2' not found"

**Causa**: Deployment name incorrecto o no conectado  
**Solución**:
1. Verificar conexión a Azure OpenAI en Settings
2. Verificar que deployment se llama exactamente "gpt-5.2"

---

### Error: "429 Too Many Requests"

**Causa**: Límite de TPM de Azure OpenAI excedido  
**Solución**:
1. Aumentar TPM del deployment en Azure OpenAI
2. Implementar retry logic con exponential backoff
3. Usar deployment de backup en otra región

---

## Costos Estimados

| Componente | Costo Mensual Estimado | Costo por Workshop (7h) |
|------------|------------------------|--------------------------|
| **AI Hub** | $0 (gratis) | $0 |
| **AI Project** | $0 (gratis) | $0 |
| **Storage Account** | ~$1 (100 GB) | <$0.10 |
| **Agent Service (Runs)** | $0.01 por run | $5-10 (500 runs) |
| **Azure OpenAI** | Variable | $8-15 por participante |

**Total estimado**: $10-15 por participante para workshop de 7 horas

**Nota**: La mayoría del costo es Azure OpenAI (tokens), no Azure AI Agent Service.

---

## Checklist de Verificación

Antes del workshop, verificar:

- [ ] Proyecto de Azure AI Foundry creado y accesible
- [ ] Conexión a Azure OpenAI configurada y funcionando
- [ ] Agent de prueba creado exitosamente
- [ ] Thread de prueba creado y run completado
- [ ] Connection string documentado y listo para distribuir
- [ ] Límites de cuota configurados
- [ ] Alertas de monitoreo activas
- [ ] Código de verificación ejecutado sin errores

---

## Recursos Adicionales

- [Azure AI Foundry Documentation](https://learn.microsoft.com/azure/ai-studio/)
- [Azure AI Agent Service](https://learn.microsoft.com/azure/ai-services/agents/)
- [Pricing Calculator](https://azure.microsoft.com/pricing/calculator/)

---

**Última actualización**: 2026-01-12  
**Versión**: 1.0  
**Responsable**: Instructor del workshop

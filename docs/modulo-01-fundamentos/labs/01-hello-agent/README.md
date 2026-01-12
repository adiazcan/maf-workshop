# Lab 01: Hello Agent - Tu Primer Agente Conversacional

**Duración**: 15 minutos  
**Complejidad**: Simple  
**Objetivo**: Crear y ejecutar tu primer agente conversacional con Microsoft Agent Framework

## Objetivo

En este laboratorio, crearás un agente conversacional básico que puede:
- Responder a saludos y preguntas simples
- Mantener contexto de conversación
- Gestionar múltiples turnos de diálogo
- Demostrar configuración correcta de Azure OpenAI

Al finalizar, tendrás un agente funcional y comprenderás los componentes fundamentales de Microsoft Agent Framework.

---

## Prerequisitos

### Software Requerido
- ✅ .NET 10 SDK instalado
- ✅ Visual Studio Code o Visual Studio 2025
- ✅ Git (para clonar ejemplos)

### Conocimientos Previos
- Ninguno (este es tu primer lab)
- Conocimientos básicos de C# recomendados

### Configuración de Azure
- ✅ Azure OpenAI Service con deployment de `gpt-5.2`
- ✅ Endpoint de Azure OpenAI disponible
- ✅ Autenticación configurada (Azure CLI `az login` o Managed Identity)

**Si no has configurado Azure**: Consulta la [Guía de Instalación](../../instalacion.md)

> **Nota**: Este laboratorio usa `DefaultAzureCredential` para autenticación, que automáticamente detecta credenciales de Azure CLI, Managed Identity, o variables de entorno. No necesitas manejar API Keys manualmente.

---

## Paso 1: Preparación del Proyecto

### 1.1 Crear Proyecto de Consola

Abre una terminal y ejecuta:

```bash
# Crear carpeta para el proyecto
mkdir HelloAgent
cd HelloAgent

# Crear proyecto de consola .NET
dotnet new console -n HelloAgent
cd HelloAgent
```

**Salida esperada**:
```
The template "Console App" was created successfully.
```

### 1.2 Instalar Paquetes NuGet

Instala los paquetes necesarios de Microsoft Agent Framework:

```bash
# Microsoft Agent Framework (paquete principal)
dotnet add package Microsoft.Agents.AI --version 1.0.0-preview.260108.1

# Microsoft Agent Framework para OpenAI
dotnet add package Microsoft.Agents.AI.OpenAI --version 1.0.0-preview.260108.1

# Azure OpenAI Client
dotnet add package Azure.AI.OpenAI --version 2.1.0

# Azure Identity (para DefaultAzureCredential)
dotnet add package Azure.Identity --version 1.13.0

# Configuración (.NET Configuration System)
dotnet add package Microsoft.Extensions.Configuration --version 10.0.0
dotnet add package Microsoft.Extensions.Configuration.Json --version 10.0.0
```

**Verificar paquetes instalados**:
```bash
dotnet list package
```

**Salida esperada**:
```
Project 'HelloAgent' has the following package references
   [net10.0]:
   Top-level Package                                       Requested
   > Azure.AI.OpenAI                                       2.1.0
   > Azure.Identity                                        1.13.0
   > Microsoft.Agents.AI                                   1.0.0-preview.260108.1
   > Microsoft.Agents.AI.OpenAI                            1.0.0-preview.260108.1
   > Microsoft.Extensions.Configuration                    10.0.0
   > Microsoft.Extensions.Configuration.Json               10.0.0
```

---

## Paso 2: Configuración

### 2.1 Crear archivo appsettings.json

Crea un archivo `appsettings.json` en la raíz del proyecto:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://TU-RECURSO-NOMBRE.openai.azure.com/",
    "DeploymentName": "gpt-5.2"
  }
}
```

**⚠️ IMPORTANTE**: 
- Reemplaza `TU-RECURSO-NOMBRE` con el nombre de tu recurso de Azure OpenAI
- Verifica que `DeploymentName` coincida con el nombre de tu deployment
- **NO necesitas API Key**: Usamos `DefaultAzureCredential` que detecta automáticamente tus credenciales

### 2.2 Autenticación con Azure CLI

La forma más sencilla de autenticarse es usando Azure CLI:

```bash
# Iniciar sesión en Azure
az login

# Verificar que estás conectado
az account show
```

**Nota**: `DefaultAzureCredential` intentará automáticamente múltiples métodos de autenticación:
1. Variables de entorno (`AZURE_CLIENT_ID`, `AZURE_CLIENT_SECRET`, `AZURE_TENANT_ID`)
2. Managed Identity (cuando se ejecuta en Azure)
3. Azure CLI (`az login`)
4. Azure PowerShell
5. Visual Studio / VS Code credentials

### 2.3 Actualizar .csproj para incluir appsettings.json

Edita `HelloAgent.csproj` y agrega este `<ItemGroup>` para copiar appsettings.json al output:

```xml
<ItemGroup>
  <None Update="appsettings.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

**Archivo completo debe verse así**:
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Azure.AI.OpenAI" Version="2.1.0" />
    <PackageReference Include="Azure.Identity" Version="1.13.0" />
    <PackageReference Include="Microsoft.Agents.AI" Version="1.0.0-preview.260108.1" />
    <PackageReference Include="Microsoft.Agents.AI.OpenAI" Version="1.0.0-preview.260108.1" />
    <PackageReference Include="Microsoft.Extensions.Configuration" Version="10.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="10.0.0" />
  </ItemGroup>

  <ItemGroup>
    <None Update="appsettings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>

</Project>
```

---

## Paso 3: Implementación

### 3.1 Reemplazar Program.cs

Abre `Program.cs` y reemplaza todo el contenido con el siguiente código:

```csharp
// ============================================================================
// Archivo: Program.cs
// Descripción: Primer agente conversacional con Microsoft Agent Framework
// Módulo: 1 - Fundamentos
// Lab: 01-hello-agent
// ============================================================================

using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

// ===== Configuración =====
// Cargar configuración desde appsettings.json
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Obtener valores de configuración
var endpoint = configuration["AzureOpenAI:Endpoint"] 
    ?? throw new InvalidOperationException("AzureOpenAI:Endpoint no configurado");
var deploymentName = configuration["AzureOpenAI:DeploymentName"] 
    ?? throw new InvalidOperationException("AzureOpenAI:DeploymentName no configurado");

// ===== Crear Cliente de Azure OpenAI =====
// Usamos DefaultAzureCredential que automáticamente detecta credenciales:
// - Variables de entorno (AZURE_CLIENT_ID, AZURE_CLIENT_SECRET, AZURE_TENANT_ID)
// - Managed Identity (cuando se ejecuta en Azure)
// - Azure CLI (az login)
// - Visual Studio / VS Code credentials
var credential = new DefaultAzureCredential();
var azureClient = new AzureOpenAIClient(new Uri(endpoint), credential);
var chatClient = azureClient.GetChatClient(deploymentName);

// ===== Crear Agente =====
// AIAgent es el tipo básico de agente conversacional en MAF
// Se crea usando el método de extensión CreateAIAgent
var agent = chatClient.CreateAIAgent(
    name: "AsistenteGeneral",
    instructions: """
        Eres un asistente útil y amigable llamado AsistenteGeneral.
        Respondes siempre en español de forma clara y concisa.
        Eres cortés y profesional en todas tus interacciones.
        """
);

// ===== Historial de Conversación =====
// Usamos List<ChatMessage> para almacenar el contexto de la conversación
var messages = new List<ChatMessage>();

// ===== Bucle de Conversación =====
Console.WriteLine("============================================");
Console.WriteLine("🤖 Hello Agent - Tu Primer Agente MAF");
Console.WriteLine("============================================");
Console.WriteLine($"Agente: {agent.Name}");
Console.WriteLine("Escribe 'salir' para terminar la conversación");
Console.WriteLine("============================================\n");

while (true)
{
    // Obtener entrada del usuario
    Console.Write("👤 Tú: ");
    var userInput = Console.ReadLine();
    
    // Verificar si el usuario quiere salir
    if (string.IsNullOrWhiteSpace(userInput) || 
        userInput.Equals("salir", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("\n🤖 AsistenteGeneral: ¡Hasta pronto! 👋\n");
        break;
    }
    
    // Agregar mensaje del usuario al historial
    messages.Add(new UserChatMessage(userInput));
    
    // Invocar el agente y obtener respuesta
    Console.Write("🤖 AsistenteGeneral: ");
    
    try
    {
        // RunStreamingAsync permite obtener la respuesta del agente en streaming
        string response = "";
        await foreach (var update in agent.RunStreamingAsync(messages))
        {
            foreach (var contentPart in update.ContentUpdate)
            {
                Console.Write(contentPart.Text);
                response += contentPart.Text;
            }
        }
        
        Console.WriteLine("\n");
        
        // Agregar la respuesta del agente al historial para mantener contexto
        messages.Add(new AssistantChatMessage(response));
        
        // Gestión de historial: truncar si supera 10 mensajes
        if (messages.Count > 10)
        {
            messages = messages.Skip(messages.Count - 10).ToList();
        }
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"\n❌ Error de conexión: {ex.Message}");
        Console.WriteLine("Verifica tu endpoint y credenciales de Azure.\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n❌ Error: {ex.Message}\n");
    }
}
```

### 3.2 Explicación del Código

**Sección de Configuración**:
- `ConfigurationBuilder`: Carga configuración desde `appsettings.json`
- No necesita API Key - usamos `DefaultAzureCredential` para autenticación segura

**Creación del Cliente y Agente**:
- `DefaultAzureCredential`: Detecta automáticamente credenciales de Azure CLI, Managed Identity, etc.
- `AzureOpenAIClient`: Cliente para conectarse a Azure OpenAI Service
- `GetChatClient()`: Obtiene un cliente de chat para el deployment específico
- `CreateAIAgent()`: Método de extensión que crea un agente de MAF
- `name`: Identificador del agente
- `instructions`: System prompt que define el comportamiento del agente

**Gestión de Conversación**:
- `List<ChatMessage>`: Lista que almacena mensajes del usuario y del agente
- `UserChatMessage`: Representa un mensaje del usuario
- `AssistantChatMessage`: Representa una respuesta del agente
- `RunStreamingAsync()`: Invoca el agente y retorna respuestas en streaming
- **Truncamiento de historial**: Evita exceder límites de tokens (importante para conversaciones largas)

---

## Paso 4: Ejecución

### 4.1 Compilar el Proyecto

```bash
dotnet build
```

**Salida esperada**:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### 4.2 Ejecutar el Agente

```bash
dotnet run
```

**Salida esperada**:
```
============================================
🤖 Hello Agent - Tu Primer Agente MAF
============================================
Agente: AsistenteGeneral
Escribe 'salir' para terminar la conversación
============================================

👤 Tú: _
```

### 4.3 Probar Conversación

**Ejemplo de interacción**:

```
👤 Tú: Hola, ¿cómo estás?
🤖 AsistenteGeneral: ¡Hola! Estoy muy bien, gracias por preguntar. Soy AsistenteGeneral y estoy aquí para ayudarte. ¿En qué puedo asistirte hoy?

👤 Tú: ¿Qué puedes hacer?
🤖 AsistenteGeneral: Puedo ayudarte con una variedad de tareas como responder preguntas, proporcionar información, ayudarte a resolver problemas, o simplemente conversar contigo. ¿Hay algo específico en lo que necesites ayuda?

👤 Tú: ¿Cuál es tu nombre?
🤖 AsistenteGeneral: Mi nombre es AsistenteGeneral. ¿En qué más puedo ayudarte?

👤 Tú: salir
🤖 AsistenteGeneral: ¡Hasta pronto! 👋
```

**Observa**:
- El agente **mantiene contexto**: recuerda que ya te presentó
- Las respuestas son **coherentes y naturales**
- El agente **sigue las instrucciones**: responde en español y es cortés

---

## Paso 5: Validación

### ✅ Checkpoint: Verificación de Funcionamiento

Confirma que tu agente funciona correctamente:

- [ ] ✅ El programa se ejecuta sin errores
- [ ] ✅ El agente responde a "Hola" con un saludo coherente
- [ ] ✅ El agente mantiene contexto (recuerda conversaciones previas)
- [ ] ✅ El agente responde en español
- [ ] ✅ Puedes salir escribiendo "salir"

**Prueba adicional** (para verificar contexto):
```
👤 Tú: Me llamo [Tu Nombre]
🤖 AsistenteGeneral: [Respuesta]

👤 Tú: ¿Cómo me llamo?
🤖 AsistenteGeneral: Te llamas [Tu Nombre]
```

Si el agente recuerda tu nombre, ¡el contexto funciona correctamente! ✅

---

## Paso 6: Experimentación (Opcional)

Para participantes que terminan temprano, intenta estas modificaciones:

### Experimento 1: Cambiar la Personalidad del Agente

Modifica las `Instructions` para cambiar el comportamiento:

```csharp
Instructions = @"Eres un experto en tecnología con un tono informal y divertido.
Usas emojis ocasionalmente y das ejemplos prácticos.
Respondes siempre en español."
```

**Pregunta de reflexión**: ¿Cómo cambia el estilo de las respuestas?

### Experimento 2: Ajustar Temperature

En `appsettings.json`, cambia `Temperature`:

```json
"Temperature": 0.0  // Respuestas más determinísticas y precisas
"Temperature": 1.5  // Respuestas más creativas y variadas
```

**Pregunta de reflexión**: ¿Qué diferencias notas en las respuestas?

### Experimento 3: Limitar MaxTokens

Reduce `MaxTokens` en `appsettings.json`:

```json
"MaxTokens": 50  // Respuestas muy cortas
```

**Pregunta de reflexión**: ¿Cómo afecta esto al costo y la calidad?

---

## Solución de Problemas

### Error: "dotnet: command not found"

**Síntoma**: Al ejecutar `dotnet`, aparece "command not found"

**Causa**: .NET SDK no está instalado o no está en el PATH

**Solución**:
1. Verifica la instalación: https://dotnet.microsoft.com/download
2. Reinicia tu terminal
3. En Windows, reinicia tu computadora

---

### Error: "401 Unauthorized" o "AuthenticationFailedException"

**Síntoma**: 
```
❌ Error de conexión: AuthenticationFailedException
```

**Causa**: No has iniciado sesión en Azure o las credenciales han expirado

**Solución**:
1. Inicia sesión con Azure CLI:
   ```bash
   az login
   ```
2. Verifica que estás conectado:
   ```bash
   az account show
   ```
3. Si usas Managed Identity, verifica que está configurada correctamente

---

### Error: "El agente no responde"

**Síntoma**: El programa se ejecuta pero el agente no genera respuestas

**Causa**: 
- Endpoint incorrecto
- Deployment name incorrecto
- Rate limit excedido

**Solución**:
1. Verifica el endpoint en `appsettings.json`:
   ```json
   "Endpoint": "https://TU-RECURSO.openai.azure.com/"
   ```
   Debe terminar con `/`

2. Verifica el deployment name en Azure Portal:
   - Ve a tu recurso de Azure OpenAI
   - "Deployments" → verifica el nombre exacto

3. Si estás en rate limit (429), espera 1 minuto y reintenta

---

## Resumen

En este laboratorio aprendiste:

✅ **Configurar el entorno** con appsettings.json y DefaultAzureCredential  
✅ **Crear un AIAgent** con instrucciones personalizadas usando CreateAIAgent  
✅ **Gestionar historial de mensajes** con List<ChatMessage> para mantener contexto  
✅ **Invocar el agente** con RunStreamingAsync para obtener respuestas en streaming  
✅ **Manejar errores** comunes de conexión y configuración

### Conceptos Clave

| Concepto | Descripción |
|----------|-------------|
| **AIAgent** | Agente conversacional básico de Microsoft Agent Framework |
| **CreateAIAgent** | Método de extensión para crear un agente desde un ChatClient |
| **DefaultAzureCredential** | Autenticación automática con Azure (CLI, Managed Identity, etc.) |
| **AzureOpenAIClient** | Cliente para conectarse a Azure OpenAI Service |
| **ChatMessage** | Tipos para representar mensajes (UserChatMessage, AssistantChatMessage) |
| **RunStreamingAsync** | Método para invocar el agente y obtener respuestas en streaming |

---

## Próximos Pasos

Ahora que tienes un agente funcional, estás listo para:

1. **[Módulo 2: Function Tools](../../modulo-02-function-tools/)** - Agregar capacidades personalizadas a tu agente
2. **Experimentar más**: Modifica las instrucciones, prueba diferentes temperaturas, agrega validación de entrada

---

## Referencias

- [Documentación de Microsoft Agent Framework](https://learn.microsoft.com/microsoft-agent-framework)
- [Azure OpenAI Best Practices](https://learn.microsoft.com/azure/ai-services/openai/concepts/best-practices)
- [.NET Configuration System](https://learn.microsoft.com/dotnet/core/extensions/configuration)

---

**Tiempo completado**: ~15 minutos  
**¡Felicitaciones!** 🎉 Has creado tu primer agente con Microsoft Agent Framework.

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
- ✅ Endpoint y API Key disponibles

**Si no has configurado Azure**: Consulta la [Guía de Instalación](../../instalacion.md)

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

# Microsoft Agent Framework Abstractions
dotnet add package Microsoft.Agents.AI.Abstractions --version 1.0.0-preview.260108.1

# Configuración (.NET Configuration System)
dotnet add package Microsoft.Extensions.Configuration --version 10.0.0
dotnet add package Microsoft.Extensions.Configuration.Json --version 10.0.0
dotnet add package Microsoft.Extensions.Configuration.UserSecrets --version 10.0.0
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
   > Microsoft.Agents.AI                                   1.0.0-preview.260108.1
   > Microsoft.Agents.AI.Abstractions                      1.0.0-preview.260108.1
   > Microsoft.Extensions.Configuration                    10.0.0
   > Microsoft.Extensions.Configuration.Json               10.0.0
   > Microsoft.Extensions.Configuration.UserSecrets        10.0.0
```

---

## Paso 2: Configuración

### 2.1 Crear archivo appsettings.json

Crea un archivo `appsettings.json` en la raíz del proyecto:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://TU-RECURSO-NOMBRE.openai.azure.com/",
    "DeploymentName": "gpt-5.2",
    "MaxTokens": 2000,
    "Temperature": 0.7
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

**⚠️ IMPORTANTE**: 
- Reemplaza `TU-RECURSO-NOMBRE` con el nombre de tu recurso de Azure OpenAI
- Verifica que `DeploymentName` coincida con el nombre de tu deployment
- **NO** incluyas tu API Key aquí (lo haremos en el siguiente paso de forma segura)

### 2.2 Configurar User Secrets (API Key)

Los **user secrets** te permiten guardar credenciales de forma segura sin incluirlas en el código fuente:

```bash
# Inicializar user secrets para el proyecto
dotnet user-secrets init

# Guardar tu API Key de Azure OpenAI
dotnet user-secrets set "AzureOpenAI:ApiKey" "TU-API-KEY-AQUI"
```

**Reemplaza `TU-API-KEY-AQUI`** con tu API Key real de Azure (la copiaste en la guía de instalación).

**Verificar que se guardó correctamente**:
```bash
dotnet user-secrets list
```

**Salida esperada**:
```
AzureOpenAI:ApiKey = sk-...tu-key...
```

### 2.3 Actualizar .csproj para incluir appsettings.json

Edita `HelloAgent.csproj` y agrega esta línea dentro del `<PropertyGroup>`:

```xml
<UserSecretsId>maf-workshop-hello-agent-01</UserSecretsId>
```

Y agrega este `<ItemGroup>` para copiar appsettings.json al output:

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
    <UserSecretsId>maf-workshop-hello-agent-01</UserSecretsId>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Agents.AI" Version="1.0.0-preview.260108.1" />
    <PackageReference Include="Microsoft.Agents.AI.Abstractions" Version="1.0.0-preview.260108.1" />
    <PackageReference Include="Microsoft.Extensions.Configuration" Version="10.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="10.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.UserSecrets" Version="10.0.0" />
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

using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Chat;
using Microsoft.Extensions.Configuration;

// ===== Configuración =====
// Cargar configuración desde appsettings.json y user secrets
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>()  // Cargar API key desde user secrets
    .Build();

// Obtener valores de configuración
var endpoint = configuration["AzureOpenAI:Endpoint"] 
    ?? throw new InvalidOperationException("AzureOpenAI:Endpoint no configurado");
var deploymentName = configuration["AzureOpenAI:DeploymentName"] 
    ?? throw new InvalidOperationException("AzureOpenAI:DeploymentName no configurado");
var apiKey = configuration["AzureOpenAI:ApiKey"] 
    ?? throw new InvalidOperationException("AzureOpenAI:ApiKey no configurado");

// ===== Crear Agente =====
// ChatCompletionAgent es el tipo básico de agente conversacional en MAF
// Se configura directamente con el endpoint de Azure OpenAI
var agent = new ChatCompletionAgent(
    name: "AsistenteGeneral",
    instructions: """
        Eres un asistente útil y amigable llamado AsistenteGeneral.
        Respondes siempre en español de forma clara y concisa.
        Eres cortés y profesional en todas tus interacciones.
        """,
    endpoint: new Uri(endpoint),
    modelId: deploymentName,
    apiKey: apiKey
);

// ===== Crear Historial de Conversación =====
// ChatHistory almacena el contexto de la conversación
var chatHistory = new ChatHistory();

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
    chatHistory.AddUserMessage(userInput);
    
    // Invocar el agente y obtener respuesta
    Console.Write("🤖 AsistenteGeneral: ");
    
    try
    {
        // InvokeAsync permite obtener la respuesta del agente
        string response = "";
        await foreach (var message in agent.InvokeAsync(chatHistory))
        {
            Console.Write(message.Content);
            response += message.Content;
        }
        
        Console.WriteLine("\n");
        
        // Agregar la respuesta del agente al historial para mantener contexto
        chatHistory.AddAssistantMessage(response);
        
        // Gestión de historial: truncar si supera 10 mensajes
        if (chatHistory.Count > 10)
        {
            var messagesToKeep = chatHistory.Skip(chatHistory.Count - 10).ToList();
            chatHistory.Clear();
            foreach (var message in messagesToKeep)
            {
                chatHistory.Add(message);
            }
        }
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"\n❌ Error de conexión: {ex.Message}");
        Console.WriteLine("Verifica tu endpoint y API key.\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n❌ Error: {ex.Message}\n");
    }
}
```

### 3.2 Explicación del Código

**Sección de Configuración**:
- `ConfigurationBuilder`: Carga configuración desde múltiples fuentes
- `AddJsonFile`: Lee `appsettings.json`
- `AddUserSecrets`: Lee la API key desde user secrets (seguro)

**Creación del Agente**:
- `ChatCompletionAgent`: Tipo de agente conversacional básico en Microsoft Agent Framework
- `name`: Identificador del agente
- `instructions`: System prompt que define el comportamiento del agente
- `endpoint`: URL del servicio Azure OpenAI
- `modelId`: Nombre del deployment del modelo en Azure
- `apiKey`: Clave de API para autenticación

**Gestión de Conversación**:
- `ChatHistory`: Almacena mensajes del usuario y del agente
- `AddUserMessage()`: Agrega mensaje del usuario al historial
- `AddAssistantMessage()`: Agrega respuesta del agente al historial
- `InvokeAsync()`: Invoca el agente y retorna respuestas de forma asíncrona
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

### Error: "401 Unauthorized"

**Síntoma**: 
```
❌ Error de conexión: Unauthorized (401)
```

**Causa**: API Key incorrecta o no configurada

**Solución**:
1. Verifica que configuraste user secrets:
   ```bash
   dotnet user-secrets list
   ```
2. Si no aparece, configúralo:
   ```bash
   dotnet user-secrets set "AzureOpenAI:ApiKey" "tu-key-real"
   ```
3. Verifica que tu API Key es correcta en Azure Portal

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

✅ **Configurar el entorno** con appsettings.json y user secrets  
✅ **Crear un ChatCompletionAgent** con instrucciones personalizadas  
✅ **Gestionar ChatHistory** para mantener contexto de conversación  
✅ **Invocar el agente** con InvokeAsync para obtener respuestas  
✅ **Manejar errores** comunes de conexión y configuración

### Conceptos Clave

| Concepto | Descripción |
|----------|-------------|
| **ChatCompletionAgent** | Agente conversacional básico de Microsoft Agent Framework |
| **Instructions** | System prompt que define el comportamiento del agente |
| **ChatHistory** | Historial de mensajes para mantener contexto |
| **InvokeAsync** | Método para invocar el agente y obtener respuestas |
| **endpoint** | URL del servicio Azure OpenAI |
| **modelId** | Nombre del deployment del modelo en Azure |

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

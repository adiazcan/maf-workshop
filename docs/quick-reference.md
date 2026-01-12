# Tarjeta de Referencia Rápida - Microsoft Agent Framework

**Workshop MAF 2026** | Versión 1.0

---

## 📦 Instalación

### .NET SDK

```bash
# Verificar versión (requiere 10.0+)
dotnet --version

# Descargar: https://dot.net/download
```

### Crear Proyecto

```bash
dotnet new console -n MiAgente
cd MiAgente
dotnet add package Microsoft.AI.Agents --version 1.0.0-preview.260108.1
dotnet add package Azure.AI.OpenAI --version 2.0.0
dotnet add package Microsoft.Extensions.Configuration.UserSecrets --version 10.0.0
```

### Configurar Secretos

```bash
dotnet user-secrets init
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://TU-RECURSO.openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:ApiKey" "tu-api-key"
dotnet user-secrets set "AzureOpenAI:DeploymentName" "gpt-5.2"
```

---

## 🤖 Agente Básico

```csharp
// Program.cs
using Microsoft.AI.Agents;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion(
    deploymentName: config["AzureOpenAI:DeploymentName"]!,
    endpoint: config["AzureOpenAI:Endpoint"]!,
    apiKey: config["AzureOpenAI:ApiKey"]!
);

var kernel = builder.Build();

var agent = new ChatCompletionAgent()
{
    Name = "Asistente",
    Instructions = "Eres un asistente útil.",
    Kernel = kernel
};

var history = new ChatHistory();
history.AddUserMessage("Hola!");

var response = await agent.InvokeAsync(history);
Console.WriteLine(response.Content);
```

---

## 🔧 Function Tool

```csharp
using Microsoft.SemanticKernel;
using System.ComponentModel;

public class MisHerramientas
{
    [KernelFunction("obtener_clima")]
    [Description("Obtiene el clima para una ciudad")]
    public string ObtenerClima(
        [Description("Nombre de la ciudad")] string ciudad)
    {
        return $"Clima en {ciudad}: Soleado, 22°C";
    }
}

// Registrar:
builder.Plugins.AddFromType<MisHerramientas>();
```

---

## 🔄 Workflows

### Secuencial

```csharp
var resultado1 = await agente1.InvokeAsync(input);
var resultado2 = await agente2.InvokeAsync(resultado1.Content);
var resultadoFinal = await agente3.InvokeAsync(resultado2.Content);
```

### Paralelo

```csharp
var tareas = new[]
{
    agente1.InvokeAsync(input),
    agente2.InvokeAsync(input),
    agente3.InvokeAsync(input)
};
var resultados = await Task.WhenAll(tareas);
```

### Group Chat

```csharp
var groupChat = new AgentGroupChat(
    agents: new[] { agente1, agente2, agente3 },
    terminationCondition: new MaxTurnsTerminationCondition(10)
);
await groupChat.InvokeAsync("Tema de discusión");
```

---

## 📊 OpenTelemetry

```csharp
builder.Services.AddOpenTelemetry()
    .WithMetrics(m => m
        .AddMeter("Microsoft.AI.Agents*")
        .AddConsoleExporter())
    .WithTracing(t => t
        .AddSource("Microsoft.AI.Agents*")
        .AddConsoleExporter());
```

---

## 🌐 ASP.NET API

```csharp
// Endpoint minimal API
app.MapPost("/api/chat", async (
    ChatRequest request,
    MiAgenteService servicio) =>
{
    var resultado = await servicio.ProcesarAsync(request.Mensaje);
    return Results.Ok(new { respuesta = resultado });
});

record ChatRequest(string Mensaje);
```

---

## 🐛 Errores Comunes

### "401 Unauthorized"

```bash
# Verificar secrets
dotnet user-secrets list

# Re-configurar
dotnet user-secrets set "AzureOpenAI:ApiKey" "nueva-key"
```

### "dotnet not found"

```bash
# Verificar PATH
export PATH="$PATH:$HOME/.dotnet"

# O reinstalar desde https://dot.net
```

### "Función no se llama"

1. Verificar `[KernelFunction]` presente
2. Verificar `[Description]` claro y específico
3. Verificar registro: `builder.Plugins.AddFromType<...>()`

### "429 Rate Limit"

- Esperar 60 segundos
- Reducir frecuencia de requests
- Aumentar TPM en Azure Portal

### "Model not found"

- Verificar `DeploymentName` correcto
- Verificar modelo desplegado en Azure Portal

---

## 🔗 URLs Importantes

| Recurso | URL |
|---------|-----|
| Portal Azure | https://portal.azure.com |
| Azure OpenAI | https://oai.azure.com |
| Docs MAF | https://learn.microsoft.com/microsoft-agent-framework |
| Docs Azure OpenAI | https://learn.microsoft.com/azure/ai-services/openai |
| .NET Aspire | https://learn.microsoft.com/dotnet/aspire |

---

## 📝 Comandos CLI

```bash
# Crear proyecto
dotnet new console -n MiProyecto

# Agregar paquete
dotnet add package NombrePaquete

# Restaurar dependencias
dotnet restore

# Compilar
dotnet build

# Ejecutar
dotnet run

# Secretos
dotnet user-secrets init
dotnet user-secrets set "Key" "Value"
dotnet user-secrets list
```

---

## 🎯 Checklist de Verificación

```bash
# Antes de cada lab
[ ] .NET instalado: dotnet --version
[ ] Secrets configurados: dotnet user-secrets list  
[ ] Internet funciona: ping azure.com
[ ] Proyecto compila: dotnet build
```

---

## 📁 Estructura de Lab

```
mi-lab/
├── MiLab.csproj
├── Program.cs
├── appsettings.json
└── README.md
```

---

## 🏷️ Versiones del Workshop

| Paquete | Versión |
|---------|---------|
| Microsoft.AI.Agents | 1.0.0-preview.260108.1 |
| Azure.AI.OpenAI | 2.0.0 |
| .NET SDK | 10.0.x |
| OpenTelemetry | 1.10.0 |
| Aspire | 9.0.0+ |

---

**Soporte**: Pregunta al instructor o consulta [docs/troubleshooting.md](troubleshooting.md)

---

*Tarjeta diseñada para impresión A4/Letter - Doblar en 3 secciones*

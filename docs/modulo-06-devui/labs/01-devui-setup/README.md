# Lab 01: DevUI Setup - Debugging Visual de Agentes

**Duración**: 15 minutos  
**Complejidad**: Simple  
**Objetivo**: Configurar DevUI para debugging visual de un agente con múltiples function tools

## Objetivo

En este laboratorio, configurarás **DevUI**, la herramienta oficial de Microsoft Agent Framework para debugging y testing de agentes durante el desarrollo. Aprenderás:

- Cómo instalar y configurar DevUI
- Cómo instrumentar tu aplicación para conectar con DevUI
- Cómo visualizar conversaciones en tiempo real
- Cómo inspeccionar invocaciones de function tools
- Cómo depurar problemas comunes de agentes

Al finalizar, podrás usar DevUI para **depurar cualquier agente** de forma visual e interactiva.

---

## Prerequisitos

### Software Requerido
- ✅ .NET 10 SDK instalado
- ✅ Visual Studio Code o Visual Studio 2025
- ✅ Terminal/consola disponible

### Conocimientos Previos
- ✅ Completar Módulo 1 (Hello Agent)
- ✅ Entender Function Tools (Módulo 2, Lab 01)

### Configuración de Azure
- ✅ Azure OpenAI Service con deployment de `gpt-5.2`
- ✅ Endpoint y API Key disponibles

---

## Paso 1: Instalar DevUI Tool

### 1.1 Instalar DevUI Globalmente

DevUI es una herramienta de línea de comandos que se instala globalmente:

```bash
# Instalar la herramienta DevUI globalmente
dotnet tool install -g Microsoft.AI.Agents.DevUI

# Verificar instalación
devui --version
```

**Salida esperada**:
```
Microsoft.AI.Agents.DevUI version 1.0.0-preview.260108.1
```

### 1.2 Crear Proyecto

```bash
# Crear carpeta para el proyecto
mkdir DevUIExample
cd DevUIExample

# Crear proyecto de consola .NET
dotnet new console -n DevUIExample
cd DevUIExample
```

### 1.3 Instalar Paquetes NuGet

```bash
# Microsoft Agent Framework
dotnet add package Microsoft.AI.Agents --version 1.0.0-preview.260108.1
dotnet add package Microsoft.AI.Agents.Abstractions --version 1.0.0-preview.260108.1

# DevUI integration
dotnet add package Microsoft.AI.Agents.DevUI --version 1.0.0-preview.260108.1

# Azure OpenAI
dotnet add package Azure.AI.OpenAI --version 2.0.0

# Configuración
dotnet add package Microsoft.Extensions.Configuration --version 10.0.0
dotnet add package Microsoft.Extensions.Configuration.Json --version 10.0.0
dotnet add package Microsoft.Extensions.Configuration.UserSecrets --version 10.0.0

# Hosting para DevUI
dotnet add package Microsoft.Extensions.Hosting --version 10.0.0
```

---

## Paso 2: Configuración del Proyecto

### 2.1 Crear appsettings.json

Crea el archivo `appsettings.json`:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://TU-RECURSO-NOMBRE.openai.azure.com/",
    "DeploymentName": "gpt-5.2",
    "MaxTokens": 2000,
    "Temperature": 0.7
  },
  "DevUI": {
    "Enabled": true,
    "Port": 5100
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

⚠️ **Reemplaza** `TU-RECURSO-NOMBRE` con el nombre de tu recurso de Azure OpenAI.

### 2.2 Configurar User Secrets

```bash
# Inicializar user secrets
dotnet user-secrets init

# Guardar API Key (nunca en appsettings.json)
dotnet user-secrets set "AzureOpenAI:ApiKey" "TU-API-KEY-AQUI"
```

---

## Paso 3: Implementar Agente con Function Tools

### 3.1 Crear DemoFunctions.cs

Crea un nuevo archivo `DemoFunctions.cs` con múltiples function tools para demostrar las capacidades de inspección de DevUI:

```csharp
// ============================================================================
// Archivo: DemoFunctions.cs
// Descripción: Funciones de demostración para debugging con DevUI
// Módulo: 6 - DevUI
// Lab: 01-devui-setup
// ============================================================================

using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace DevUIExample;

/// <summary>
/// Colección de function tools para demostración de debugging con DevUI.
/// Incluye funciones de clima, calendario y calculadora.
/// </summary>
public class DemoFunctions
{
    // ===== Datos simulados =====
    private static readonly Dictionary<string, (string Condition, int Temp)> _weather = new()
    {
        ["madrid"] = ("Soleado", 22),
        ["barcelona"] = ("Parcialmente nublado", 24),
        ["sevilla"] = ("Muy soleado", 30),
        ["valencia"] = ("Soleado", 26),
        ["bilbao"] = ("Nublado", 18),
    };

    private static readonly List<CalendarEvent> _calendar = new()
    {
        new("Reunión de equipo", DateTime.Today.AddHours(10), 60),
        new("Almuerzo", DateTime.Today.AddHours(13), 45),
        new("Revisión de proyecto", DateTime.Today.AddHours(15), 90),
        new("Stand-up diario", DateTime.Today.AddDays(1).AddHours(9), 15),
    };

    // ===== Function Tools de Clima =====

    [KernelFunction("get_weather")]
    [Description("Obtiene el clima actual para una ciudad española. Úsala cuando pregunten por el tiempo o temperatura.")]
    public string GetWeather(
        [Description("Nombre de la ciudad (Madrid, Barcelona, Sevilla, Valencia, Bilbao)")] string city)
    {
        var cityKey = city.ToLowerInvariant().Trim();
        
        if (_weather.TryGetValue(cityKey, out var data))
        {
            return $"🌤️ Clima en {city}: {data.Condition}, {data.Temp}°C";
        }
        
        return $"⚠️ No tengo datos del clima para '{city}'.";
    }

    // ===== Function Tools de Calendario =====

    [KernelFunction("get_calendar_events")]
    [Description("Obtiene los eventos del calendario. Úsala cuando pregunten por reuniones, citas o agenda.")]
    public string GetCalendarEvents(
        [Description("Día a consultar: 'hoy' o 'mañana'")] string day = "hoy")
    {
        var targetDate = day.ToLowerInvariant() == "mañana" 
            ? DateTime.Today.AddDays(1) 
            : DateTime.Today;
        
        var events = _calendar
            .Where(e => e.StartTime.Date == targetDate)
            .OrderBy(e => e.StartTime)
            .ToList();
        
        if (!events.Any())
            return $"📅 No tienes eventos programados para {day}.";

        var result = $"📅 Eventos para {day}:\n";
        foreach (var evt in events)
            result += $"  • {evt.StartTime:HH:mm} - {evt.Title}\n";
        
        return result;
    }

    [KernelFunction("create_calendar_event")]
    [Description("Crea un nuevo evento en el calendario.")]
    public string CreateCalendarEvent(
        [Description("Título del evento")] string title,
        [Description("Hora en formato HH:mm")] string time,
        [Description("Duración en minutos")] int durationMinutes = 30)
    {
        var timeSpan = TimeSpan.Parse(time);
        var eventTime = DateTime.Today.Add(timeSpan);
        
        return $"✅ Evento '{title}' creado para {eventTime:HH:mm} ({durationMinutes} min)";
    }

    // ===== Function Tools de Calculadora =====

    [KernelFunction("calculate")]
    [Description("Realiza cálculos matemáticos básicos. Úsala para operaciones numéricas.")]
    public string Calculate(
        [Description("Primer número")] double a,
        [Description("Segundo número")] double b,
        [Description("Operación: 'sumar', 'restar', 'multiplicar', 'dividir'")] string operation)
    {
        var result = operation.ToLowerInvariant() switch
        {
            "sumar" or "+" => (a + b, $"{a} + {b}"),
            "restar" or "-" => (a - b, $"{a} - {b}"),
            "multiplicar" or "*" => (a * b, $"{a} × {b}"),
            "dividir" or "/" when b != 0 => (a / b, $"{a} ÷ {b}"),
            _ => (double.NaN, "Operación no válida")
        };
        
        return double.IsNaN(result.Item1) 
            ? $"❌ {result.Item2}" 
            : $"🔢 {result.Item2} = {result.Item1:N2}";
    }

    [KernelFunction("convert_units")]
    [Description("Convierte entre unidades (km/millas, celsius/fahrenheit, euros/dólares).")]
    public string ConvertUnits(
        [Description("Valor a convertir")] double value,
        [Description("Tipo: 'km_millas', 'celsius_fahrenheit', 'euros_dolares'")] string conversionType)
    {
        var result = conversionType.ToLowerInvariant() switch
        {
            "km_millas" => $"{value:N2} km = {value * 0.621371:N2} millas",
            "celsius_fahrenheit" => $"{value:N1}°C = {(value * 9/5) + 32:N1}°F",
            "euros_dolares" => $"€{value:N2} = ${value * 1.08:N2} USD",
            _ => "Conversión no reconocida"
        };
        
        return $"🔄 {result}";
    }
}

internal record CalendarEvent(string Title, DateTime StartTime, int DurationMinutes);
```

---

## Paso 4: Configurar Agente con DevUI

### 4.1 Reemplazar Program.cs

Reemplaza el contenido de `Program.cs`:

```csharp
// ============================================================================
// Archivo: Program.cs
// Descripción: Aplicación de demostración con DevUI para debugging de agentes
// Módulo: 6 - DevUI
// Lab: 01-devui-setup
// ============================================================================

using Microsoft.AI.Agents;
using Microsoft.AI.Agents.DevUI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using DevUIExample;

// ===== Configuración del Host con DevUI =====
var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>();

var configuration = builder.Configuration;

// Obtener valores de configuración
var endpoint = configuration["AzureOpenAI:Endpoint"] 
    ?? throw new InvalidOperationException("AzureOpenAI:Endpoint no configurado");
var deploymentName = configuration["AzureOpenAI:DeploymentName"] 
    ?? throw new InvalidOperationException("AzureOpenAI:DeploymentName no configurado");
var apiKey = configuration["AzureOpenAI:ApiKey"] 
    ?? throw new InvalidOperationException("AzureOpenAI:ApiKey no configurado");

// ===== Configurar DevUI =====
var devUIEnabled = configuration.GetValue<bool>("DevUI:Enabled", true);
var devUIPort = configuration.GetValue<int>("DevUI:Port", 5100);

if (devUIEnabled)
{
    builder.Services.AddDevUI(options =>
    {
        options.Port = devUIPort;
        options.EnableDetailedLogging = true;
    });
    
    Console.WriteLine($"🔧 DevUI habilitado en puerto {devUIPort}");
}

// ===== Crear Kernel con Function Tools =====
var kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);
kernelBuilder.Plugins.AddFromType<DemoFunctions>();

var kernel = kernelBuilder.Build();
builder.Services.AddSingleton(kernel);

// ===== Crear Agente =====
var agent = new ChatCompletionAgent()
{
    Name = "AgenteMultiFuncion",
    Instructions = """
        Eres un asistente personal. Puedes ayudar con:
        🌤️ Clima de ciudades españolas
        📅 Ver y crear eventos en calendario
        🔢 Cálculos matemáticos
        🔄 Conversiones de unidades
        Siempre responde en español.
        """,
    Kernel = kernel,
    Arguments = new KernelArguments(
        new AzureOpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        }
    )
};

builder.Services.AddSingleton(agent);

// ===== Iniciar Host =====
var host = builder.Build();
_ = host.StartAsync();

var chatHistory = new ChatHistory();

// ===== Interfaz de Usuario =====
Console.WriteLine("============================================");
Console.WriteLine("🔧  DevUI Demo - Agente Multi-Función");
Console.WriteLine("============================================");

if (devUIEnabled)
{
    Console.WriteLine($"🌐 Abre DevUI: http://localhost:{devUIPort}");
}

Console.WriteLine("💡 Prueba: '¿Cómo está el clima en Madrid?'");
Console.WriteLine("Escribe 'salir' para terminar");
Console.WriteLine("============================================\n");

// ===== Bucle de Conversación =====
while (true)
{
    Console.Write("👤 Tú: ");
    var userInput = Console.ReadLine();
    
    if (string.IsNullOrWhiteSpace(userInput) || 
        userInput.Equals("salir", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("\n🔧 ¡Hasta pronto! 👋\n");
        break;
    }
    
    chatHistory.AddUserMessage(userInput);
    Console.Write("🔧 Agente: ");
    
    try
    {
        await foreach (var message in agent.InvokeStreamingAsync(chatHistory))
        {
            Console.Write(message.Content);
        }
        Console.WriteLine("\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n❌ Error: {ex.Message}\n");
    }
}

await host.StopAsync();
```

### 4.2 Explicación de la Configuración DevUI

**Líneas clave para DevUI**:

```csharp
// 1. Agregar servicios de DevUI
builder.Services.AddDevUI(options =>
{
    options.Port = devUIPort;              // Puerto donde escucha DevUI
    options.EnableDetailedLogging = true;  // Logs detallados
});

// 2. Registrar kernel y agente para instrumentación
builder.Services.AddSingleton(kernel);
builder.Services.AddSingleton(agent);

// 3. Iniciar el host (DevUI corre en background)
_ = host.StartAsync();
```

---

## Paso 5: Ejecutar y Conectar DevUI

### 5.1 Compilar y Ejecutar

```bash
# En Terminal 1: Ejecutar aplicación
dotnet build
dotnet run
```

**Salida esperada**:
```
🔧 DevUI habilitado en puerto 5100
============================================
🔧  DevUI Demo - Agente Multi-Función
============================================
🌐 Abre DevUI: http://localhost:5100
💡 Prueba: '¿Cómo está el clima en Madrid?'
Escribe 'salir' para terminar
============================================

👤 Tú: _
```

### 5.2 Abrir DevUI

Abre en navegador: `http://localhost:5100`

O usa el CLI:
```bash
devui start --port 5100
```

### 5.3 Probar Conversación

```
👤 Tú: ¿Cómo está el clima en Madrid?
👤 Tú: ¿Qué reuniones tengo hoy?
👤 Tú: Suma 150 más 75
👤 Tú: ¿Cuánto son 100 euros en dólares?
```

**Observa en DevUI**: Cada function call aparece con sus parámetros y resultado.

---

## Paso 6: Inspeccionar en DevUI

### 6.1 Panel de Conversación

DevUI muestra:

```
┌─────────────────────────────────────────────┐
│ 👤 User: ¿Cómo está el clima en Madrid?     │
├─────────────────────────────────────────────┤
│ 🔧 Function Call: get_weather               │
│    Parameters: city = "Madrid"              │
│    Duration: 2ms                            │
├─────────────────────────────────────────────┤
│ ✅ Result: 🌤️ Clima en Madrid: Soleado, 22°C│
├─────────────────────────────────────────────┤
│ 🤖 Agent: El clima en Madrid está soleado   │
└─────────────────────────────────────────────┘
```

### 6.2 Panel de Funciones

| Función | Parámetros |
|---------|------------|
| `get_weather` | city |
| `get_calendar_events` | day |
| `create_calendar_event` | title, time, duration |
| `calculate` | a, b, operation |
| `convert_units` | value, conversionType |

---

## Paso 7: Validación

### ✅ Checkpoint

- [ ] DevUI tool instalado (`devui --version`)
- [ ] Aplicación inicia sin errores
- [ ] DevUI accesible en `http://localhost:5100`
- [ ] Mensajes aparecen en tiempo real
- [ ] Function calls visibles con parámetros

**Prueba definitiva**: "¿Clima en Barcelona y qué reuniones tengo?"  
**Resultado en DevUI**: 2 function calls (`get_weather` y `get_calendar_events`)

---

## Solución de Problemas

### "DevUI no se conecta"

**Causa**: Puerto incorrecto o firewall.

**Solución**:
```bash
devui start --port 5101  # Probar otro puerto
```

---

### "Function calls no visibles"

**Causa**: Kernel no registrado.

**Solución**: Verificar:
```csharp
builder.Services.AddSingleton(kernel);
```

---

### "Authentication errors"

**Solución** (solo desarrollo):
```csharp
builder.Services.AddDevUI(options =>
{
    options.RequireAuthentication = false;
});
```

---

## Resumen

✅ **Instalar DevUI** como herramienta global  
✅ **Configurar DevUI** con `AddDevUI()`  
✅ **Instrumentar agentes** registrándolos en DI  
✅ **Visualizar conversaciones** en tiempo real  
✅ **Inspeccionar function calls** con parámetros

### Cuándo Usar DevUI

| Situación | DevUI | Alternativa |
|-----------|-------|-------------|
| Desarrollo local | ✅ Sí | - |
| Producción | ❌ No | OpenTelemetry |

---

## Próximos Pasos

Continúa con [Módulo 7: MCP](../../modulo-07-mcp/) para aprender sobre interoperabilidad entre frameworks.

---

## Referencias

- [DevUI Documentation](https://learn.microsoft.com/microsoft-agent-framework/tools/devui)
- [Debugging Best Practices](https://learn.microsoft.com/microsoft-agent-framework/debugging)

---

**Tiempo**: ~15 minutos  
**¡Felicitaciones!** 🎉 Ahora puedes usar DevUI para depurar agentes visualmente.

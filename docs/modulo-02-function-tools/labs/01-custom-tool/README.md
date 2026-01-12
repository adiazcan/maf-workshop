# Lab 01: Custom Function Tool - Agente del Clima

**Duración**: 20 minutos  
**Complejidad**: Standard  
**Objetivo**: Crear un agente que utiliza funciones personalizadas para obtener información del clima

## Objetivo

En este laboratorio, crearás un agente que puede **invocar automáticamente** funciones para obtener información del clima usando Microsoft Agent Framework. Aprenderás:

- Cómo definir funciones con `AIFunctionFactory.Create`
- Cómo las descripciones ayudan al modelo a decidir cuándo usar cada función
- Cómo registrar funciones en `ChatCompletionAgent`
- Cómo el modelo decide automáticamente cuándo llamar las funciones

Al finalizar, tu agente podrá responder preguntas sobre el clima **sin que tú escribas la lógica de cuándo llamar la función** - el modelo lo decide por sí mismo.

---

## Prerequisitos

### Software Requerido
- ✅ .NET 10 SDK instalado
- ✅ Visual Studio Code o Visual Studio 2025

### Conocimientos Previos
- ✅ Completar Lab 01 del Módulo 1 (Hello Agent)
- Entender cómo crear un agente básico con `ChatCompletionAgent`

### Configuración de Azure
- ✅ Azure OpenAI Service con deployment de `gpt-5.2`
- ✅ Endpoint y API Key disponibles

---

## Paso 1: Preparación del Proyecto

### 1.1 Crear Proyecto

```bash
# Crear carpeta para el proyecto
mkdir WeatherAgent
cd WeatherAgent

# Crear proyecto de consola .NET
dotnet new console -n WeatherAgent
cd WeatherAgent
```

### 1.2 Instalar Paquetes NuGet

```bash
# Microsoft Agent Framework
dotnet add package Microsoft.Agents.AI --version 1.0.0-preview.260108.1

# Configuración
dotnet add package Microsoft.Extensions.Configuration --version 10.0.0
dotnet add package Microsoft.Extensions.Configuration.Json --version 10.0.0
dotnet add package Microsoft.Extensions.Configuration.UserSecrets --version 10.0.0
```

---

## Paso 2: Configuración

### 2.1 Crear appsettings.json

Crea el archivo `appsettings.json`:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://TU-RECURSO-NOMBRE.openai.azure.com/",
    "DeploymentName": "gpt-5.2",
    "MaxTokens": 2000,
    "Temperature": 0.7
  }
}
```

⚠️ **Reemplaza** `TU-RECURSO-NOMBRE` con el nombre de tu recurso de Azure OpenAI.

### 2.2 Configurar User Secrets

```bash
# Inicializar user secrets
dotnet user-secrets init

# Guardar API Key
dotnet user-secrets set "AzureOpenAI:ApiKey" "TU-API-KEY-AQUI"
```

### 2.3 Actualizar .csproj

Edita `WeatherAgent.csproj` para que quede así:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <UserSecretsId>maf-workshop-weather-agent-02</UserSecretsId>
  </PropertyGroup>

  <ItemGroup>
    <!-- Microsoft Agent Framework package -->
    <PackageReference Include="Microsoft.Agents.AI" Version="1.0.0-preview.260108.1" />
    
    <!-- Configuration -->
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

## Paso 3: Implementar el Servicio de Clima

### 3.1 Crear WeatherService.cs

Crea un nuevo archivo `WeatherService.cs` con los datos de clima simulados:

```csharp
namespace WeatherAgent;

/// <summary>
/// Servicio que proporciona información del clima.
/// Las funciones que invocan este servicio se definen en Program.cs usando AIFunctionFactory.
/// </summary>
public static class WeatherService
{
    // Diccionario con datos de clima simulados
    private static readonly Dictionary<string, WeatherData> _weatherDatabase = new()
    {
        ["madrid"] = new("Madrid", "ES", "Soleado", 22, 45),
        ["barcelona"] = new("Barcelona", "ES", "Parcialmente nublado", 24, 65),
        ["valencia"] = new("Valencia", "ES", "Soleado", 26, 55),
        ["sevilla"] = new("Sevilla", "ES", "Muy soleado", 30, 35),
        ["bilbao"] = new("Bilbao", "ES", "Nublado", 18, 75),
    };

    /// <summary>
    /// Obtiene el clima actual para una ciudad específica.
    /// </summary>
    public static string GetWeather(string city, string country = "ES")
    {
        var cityKey = city.ToLowerInvariant().Trim();
        
        if (_weatherDatabase.TryGetValue(cityKey, out var weather))
        {
            return $"""
                📍 Clima en {weather.City}, {weather.Country}:
                🌡️ Temperatura: {weather.Temperature}°C
                ☁️ Condición: {weather.Condition}
                💧 Humedad: {weather.Humidity}%
                """;
        }
        
        return $"⚠️ No tengo datos del clima para '{city}'.";
    }

    /// <summary>
    /// Obtiene el pronóstico del clima para los próximos días.
    /// </summary>
    public static string GetForecast(string city, int days = 3)
    {
        days = Math.Clamp(days, 1, 7);
        var cityKey = city.ToLowerInvariant().Trim();
        
        if (!_weatherDatabase.TryGetValue(cityKey, out var currentWeather))
        {
            return $"⚠️ No tengo datos de pronóstico para '{city}'.";
        }

        var forecast = new System.Text.StringBuilder();
        forecast.AppendLine($"📅 Pronóstico para {currentWeather.City} ({days} días):");
        
        var random = new Random(city.GetHashCode());
        var conditions = new[] { "Soleado", "Parcialmente nublado", "Nublado", "Lluvioso" };
        
        for (int i = 1; i <= days; i++)
        {
            var date = DateTime.Now.AddDays(i).ToString("dddd dd/MM", 
                new System.Globalization.CultureInfo("es-ES"));
            var temp = currentWeather.Temperature + random.Next(-3, 4);
            var condition = conditions[random.Next(conditions.Length)];
            
            forecast.AppendLine($"  📆 {date}: {temp}°C, {condition}");
        }
        
        return forecast.ToString();
    }
}

internal record WeatherData(string City, string Country, string Condition, int Temperature, int Humidity);
```

---

## Paso 4: Implementar el Agente con Function Tools

### 4.1 Reemplazar Program.cs

Reemplaza el contenido de `Program.cs`:

```csharp
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Abstractions;
using Microsoft.Agents.AI.Chat;
using Microsoft.Extensions.Configuration;
using WeatherAgent;

// ===== Configuración =====
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>()
    .Build();

var endpoint = configuration["AzureOpenAI:Endpoint"] 
    ?? throw new InvalidOperationException("AzureOpenAI:Endpoint no configurado");
var deploymentName = configuration["AzureOpenAI:DeploymentName"] 
    ?? throw new InvalidOperationException("AzureOpenAI:DeploymentName no configurado");
var apiKey = configuration["AzureOpenAI:ApiKey"] 
    ?? throw new InvalidOperationException("AzureOpenAI:ApiKey no configurado");

// ===== Crear Function Tools =====
// Usamos AIFunctionFactory.Create para definir funciones invocables por el agente

// Función para obtener el clima actual
var getWeatherFunction = AIFunctionFactory.Create(
    (string city, string country) => WeatherService.GetWeather(city, country ?? "ES"),
    name: "get_weather",
    description: "Obtiene el clima actual para una ubicación específica. Úsala cuando el usuario pregunte sobre el clima de una ciudad."
);

// Función para obtener el pronóstico
var getForecastFunction = AIFunctionFactory.Create(
    (string city, int days) => WeatherService.GetForecast(city, days > 0 ? days : 3),
    name: "get_forecast",
    description: "Obtiene el pronóstico del clima para los próximos días."
);

// ===== Crear Agente con Function Tools =====
var agent = new ChatCompletionAgent(
    name: "AgenteDelClima",
    instructions: """
        Eres un asistente experto en clima llamado AgenteDelClima.
        Puedes proporcionar información del clima actual y pronósticos.
        
        Cuando el usuario pregunte sobre el clima de una ciudad:
        1. Usa la función get_weather para obtener el clima actual
        2. Usa la función get_forecast si preguntan por el pronóstico
        
        Siempre responde en español de forma amigable y útil.
        """,
    endpoint: new Uri(endpoint),
    modelId: deploymentName,
    apiKey: apiKey,
    tools: new AIFunction[] { getWeatherFunction, getForecastFunction }
);

// ===== Historial de Conversación =====
var chatHistory = new ChatHistory();

// ===== Interfaz de Usuario =====
Console.WriteLine("============================================");
Console.WriteLine("🌤️  Agente del Clima con Function Tools (MAF)");
Console.WriteLine("============================================");
Console.WriteLine($"Agente: {agent.Name}");
Console.WriteLine("Funciones: get_weather, get_forecast");
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
        Console.WriteLine("\n🌤️ AgenteDelClima: ¡Hasta pronto! ☀️\n");
        break;
    }
    
    chatHistory.AddUserMessage(userInput);
    Console.Write("🌤️ AgenteDelClima: ");
    
    try
    {
        await foreach (var message in agent.InvokeAsync(chatHistory))
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
```

### 4.2 Puntos Clave del Código

**Crear Function Tools con AIFunctionFactory**:
```csharp
var getWeatherFunction = AIFunctionFactory.Create(
    (string city, string country) => WeatherService.GetWeather(city, country),
    name: "get_weather",
    description: "Obtiene el clima actual..."
);
```

**Registrar funciones en el agente**:
```csharp
var agent = new ChatCompletionAgent(
    ...
    tools: new AIFunction[] { getWeatherFunction, getForecastFunction }
);
```

---

## Paso 5: Ejecución

### 5.1 Compilar y Ejecutar

```bash
dotnet build
dotnet run
```

### 5.2 Salida Esperada

```
============================================
🌤️  Agente del Clima con Function Tools (MAF)
============================================
Agente: AgenteDelClima
Funciones: get_weather, get_forecast
Escribe 'salir' para terminar
============================================

👤 Tú: _
```

### 5.3 Probar la Invocación Automática

**Prueba 1: Clima actual**
```
👤 Tú: ¿Cómo está el clima en Madrid?
🌤️ AgenteDelClima: Según mis datos, el clima en Madrid está soleado con una temperatura de 22°C y una humedad del 45%. ¡Un día perfecto! ☀️
```

**Prueba 2: Pronóstico**
```
👤 Tú: ¿Cuál es el pronóstico para Barcelona para los próximos 5 días?
🌤️ AgenteDelClima: Aquí tienes el pronóstico para Barcelona:
📅 Pronóstico para Barcelona (5 días):
  📆 lunes 13/01: 25°C, Parcialmente nublado
  ...
```

---

## Paso 6: Validación

### ✅ Checkpoint: Verificación de Function Calling

Confirma que tu agente funciona correctamente:

- [ ] ✅ El programa se ejecuta sin errores
- [ ] ✅ Al preguntar por el clima de Madrid, el agente responde con datos específicos (22°C, soleado)
- [ ] ✅ Al pedir pronóstico, el agente usa la función `get_forecast`
- [ ] ✅ El agente responde en español

---

## Solución de Problemas

### El agente no llama la función

**Síntoma**: Preguntas sobre el clima pero el agente inventa datos.

**Causa**: La función no está registrada o la descripción no es clara.

**Solución**:
1. Verifica que la función está en el array `tools` del constructor
2. Mejora la descripción para ser más específica

### Error de conexión

**Síntoma**: Error 401 o timeout.

**Solución**: Verifica endpoint, API key y deployment name en appsettings.json

---

## Resumen

En este laboratorio aprendiste:

✅ **Definir Function Tools** con `AIFunctionFactory.Create`  
✅ **Registrar funciones** en el constructor de `ChatCompletionAgent`  
✅ **Las descripciones son cruciales** para que el modelo tome buenas decisiones  
✅ **El modelo decide cuándo llamar** - no necesitas lógica de routing manual

---

## Próximos Pasos

Continúa con [Lab 02: Agent-as-Tool](../02-agent-as-tool/) donde aprenderás a usar un agente completo como función de otro agente.

---

**Tiempo completado**: ~20 minutos  
**¡Felicitaciones!** 🎉 Tu agente ahora puede usar funciones personalizadas automáticamente.

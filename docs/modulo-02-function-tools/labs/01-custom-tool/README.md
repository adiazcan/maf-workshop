# Lab 01: Custom Function Tool - Agente del Clima

**Duración**: 20 minutos  
**Complejidad**: Standard  
**Objetivo**: Crear un agente que utiliza funciones personalizadas para obtener información del clima

## Objetivo

En este laboratorio, crearás un agente que puede **invocar automáticamente** funciones para obtener información del clima usando Microsoft Agent Framework. Aprenderás:

- Cómo definir funciones con atributos `[Description]` y `AIFunctionFactory.Create`
- Cómo las descripciones ayudan al modelo a decidir cuándo usar cada función
- Cómo registrar funciones al crear el agente con `CreateAIAgent`
- Cómo el modelo decide automáticamente cuándo llamar las funciones

Al finalizar, tu agente podrá responder preguntas sobre el clima **sin que tú escribas la lógica de cuándo llamar la función** - el modelo lo decide por sí mismo.

---

## Prerequisitos

### Software Requerido
- ✅ .NET 10 SDK instalado
- ✅ Visual Studio Code o Visual Studio 2025

### Conocimientos Previos
- ✅ Completar Lab 01 del Módulo 1 (Hello Agent)
- Entender cómo crear un agente básico con `CreateAIAgent`

### Configuración de Azure
- ✅ Azure OpenAI Service con deployment de `gpt-5.2`
- ✅ Azure CLI configurado con `az login`

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
# Microsoft Agent Framework packages
dotnet add package Microsoft.Agents.AI --version 1.0.0-preview.260108.1
dotnet add package Microsoft.Agents.AI.OpenAI --version 1.0.0-preview.260108.1

# Azure OpenAI SDK
dotnet add package Azure.AI.OpenAI --version 2.1.0

# Azure Identity para autenticación
dotnet add package Azure.Identity --version 1.13.0

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

### 2.2 Autenticación con Azure CLI

Este lab usa `DefaultAzureCredential` que detecta automáticamente tus credenciales de Azure:

```bash
# Iniciar sesión en Azure
az login

# Verificar cuenta activa
az account show
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
    <!-- Microsoft Agent Framework packages -->
    <PackageReference Include="Microsoft.Agents.AI" Version="1.0.0-preview.260108.1" />
    <PackageReference Include="Microsoft.Agents.AI.OpenAI" Version="1.0.0-preview.260108.1" />
    
    <!-- Azure OpenAI SDK -->
    <PackageReference Include="Azure.AI.OpenAI" Version="2.1.0" />
    
    <!-- Azure Identity para autenticación -->
    <PackageReference Include="Azure.Identity" Version="1.13.0" />
    
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
using System.ComponentModel;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
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

// ===== Crear Function Tools =====
// Definimos funciones con atributos [Description] para que el modelo sepa cuándo usarlas

[Description("Obtiene el clima actual para una ubicación. Úsala cuando pregunten por el clima de una ciudad.")]
static string GetWeather(
    [Description("El nombre de la ciudad (ej: Madrid, Barcelona)")] string city,
    [Description("Código de país ISO (ej: ES, MX). Por defecto: ES")] string country = "ES")
{
    return WeatherService.GetWeather(city, country);
}

[Description("Obtiene el pronóstico del clima para los próximos días.")]
static string GetForecast(
    [Description("El nombre de la ciudad")] string city,
    [Description("Número de días (1-7). Por defecto: 3")] int days = 3)
{
    return WeatherService.GetForecast(city, days > 0 ? days : 3);
}

// Crear las herramientas del agente usando AIFunctionFactory
AITool[] tools = [
    AIFunctionFactory.Create(GetWeather),
    AIFunctionFactory.Create(GetForecast)
];

// ===== Crear Agente con Function Tools =====
AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new DefaultAzureCredential())
    .GetChatClient(deploymentName)
    .AsIChatClient()
    .CreateAIAgent(
        name: "AgenteDelClima",
        instructions: """
            Eres un asistente experto en clima llamado AgenteDelClima.
            Puedes proporcionar información del clima actual y pronósticos.
            
            Cuando el usuario pregunte sobre el clima de una ciudad:
            1. Usa la función GetWeather para obtener el clima actual
            2. Usa la función GetForecast si preguntan por el pronóstico
            
            Siempre responde en español de forma amigable y útil.
            """,
        tools: tools
    );

// ===== Crear Thread para la conversación =====
var thread = agent.GetNewThread();

// ===== Interfaz de Usuario =====
Console.WriteLine("============================================");
Console.WriteLine("🌤️  Agente del Clima con Function Tools (MAF)");
Console.WriteLine("============================================");
Console.WriteLine($"Agente: {agent.Name}");
Console.WriteLine("Funciones: GetWeather, GetForecast");
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
    
    Console.Write("🌤️ AgenteDelClima: ");
    
    try
    {
        await foreach (var update in agent.RunStreamingAsync(userInput, thread))
        {
            Console.Write(update);
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

**Definir Function Tools con atributos**:
```csharp
[Description("Obtiene el clima actual...")]
static string GetWeather(
    [Description("El nombre de la ciudad")] string city,
    [Description("Código de país ISO")] string country = "ES")
{
    return WeatherService.GetWeather(city, country);
}
```

**Crear herramientas con AIFunctionFactory**:
```csharp
AITool[] tools = [
    AIFunctionFactory.Create(GetWeather),
    AIFunctionFactory.Create(GetForecast)
];
```

**Registrar funciones en el agente**:
```csharp
AIAgent agent = new AzureOpenAIClient(...)
    .GetChatClient(deploymentName)
    .AsIChatClient()
    .CreateAIAgent(
        name: "AgenteDelClima",
        instructions: "...",
        tools: tools  // ← Las funciones se pasan aquí
    );
```

**Usar thread para mantener historial**:
```csharp
var thread = agent.GetNewThread();
await foreach (var update in agent.RunStreamingAsync(userInput, thread))
{
    Console.Write(update);
}
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
Funciones: GetWeather, GetForecast
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
- [ ] ✅ Al pedir pronóstico, el agente usa la función `GetForecast`
- [ ] ✅ El agente responde en español

---

## Solución de Problemas

### El agente no llama la función

**Síntoma**: Preguntas sobre el clima pero el agente inventa datos.

**Causa**: La función no está registrada o la descripción no es clara.

**Solución**:
1. Verifica que la función está en el array `tools` del constructor
2. Mejora la descripción `[Description]` para ser más específica

### Error de autenticación

**Síntoma**: Error de credenciales o 401.

**Solución**: 
1. Ejecuta `az login` para autenticarte en Azure
2. Verifica que tu cuenta tiene acceso al recurso Azure OpenAI

### Error de conexión

**Síntoma**: Error de red o timeout.

**Solución**: Verifica el endpoint en appsettings.json

---

## Resumen

En este laboratorio aprendiste:

✅ **Definir Function Tools** con atributos `[Description]` y `AIFunctionFactory.Create`  
✅ **Registrar funciones** al crear el agente con `CreateAIAgent`  
✅ **Las descripciones son cruciales** para que el modelo tome buenas decisiones  
✅ **El modelo decide cuándo llamar** - no necesitas lógica de routing manual  
✅ **Usar threads** para mantener el historial de conversación

---

## Próximos Pasos

Continúa con [Lab 02: Agent-as-Tool](../02-agent-as-tool/) donde aprenderás a usar un agente completo como función de otro agente.

---

**Tiempo completado**: ~20 minutos  
**¡Felicitaciones!** 🎉 Tu agente ahora puede usar funciones personalizadas automáticamente.

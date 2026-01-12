# Lab 01: Custom Function Tool - Agente del Clima

**Duración**: 20 minutos  
**Complejidad**: Standard  
**Objetivo**: Crear un agente que utiliza una función personalizada para obtener información del clima

## Objetivo

En este laboratorio, crearás un agente que puede **invocar automáticamente** funciones de C# para obtener información del clima. Aprenderás:

- Cómo definir funciones con el atributo `[KernelFunction]`
- Cómo las descripciones ayudan al modelo a decidir cuándo usar cada función
- Cómo registrar funciones como plugins en el kernel
- Cómo habilitar function calling automático en el agente

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
dotnet add package Microsoft.AI.Agents --version 1.0.0-preview.260108.1
dotnet add package Microsoft.AI.Agents.Abstractions --version 1.0.0-preview.260108.1

# Azure OpenAI
dotnet add package Azure.AI.OpenAI --version 2.0.0

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
    <!-- Microsoft Agent Framework packages -->
    <PackageReference Include="Microsoft.AI.Agents" Version="1.0.0-preview.260108.1" />
    <PackageReference Include="Microsoft.AI.Agents.Abstractions" Version="1.0.0-preview.260108.1" />
    
    <!-- Azure OpenAI -->
    <PackageReference Include="Azure.AI.OpenAI" Version="2.0.0" />
    
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

## Paso 3: Implementar la Function Tool

### 3.1 Crear WeatherService.cs

Crea un nuevo archivo `WeatherService.cs`:

```csharp
// ============================================================================
// Archivo: WeatherService.cs
// Descripción: Servicio de clima con Function Tool para agentes MAF
// Módulo: 2 - Function Tools
// Lab: 01-custom-tool
// ============================================================================

using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace WeatherAgent;

/// <summary>
/// Servicio que proporciona información del clima.
/// Los métodos decorados con [KernelFunction] pueden ser invocados automáticamente
/// por el agente cuando detecta que el usuario necesita esta información.
/// </summary>
public class WeatherService
{
    // Diccionario con datos de clima simulados para diferentes ciudades
    private static readonly Dictionary<string, WeatherData> _weatherDatabase = new()
    {
        ["madrid"] = new("Madrid", "ES", "Soleado", 22, 45),
        ["barcelona"] = new("Barcelona", "ES", "Parcialmente nublado", 24, 65),
        ["valencia"] = new("Valencia", "ES", "Soleado", 26, 55),
        ["sevilla"] = new("Sevilla", "ES", "Muy soleado", 30, 35),
        ["bilbao"] = new("Bilbao", "ES", "Nublado", 18, 75),
        ["mexico city"] = new("Ciudad de México", "MX", "Parcialmente nublado", 20, 50),
        ["buenos aires"] = new("Buenos Aires", "AR", "Templado", 18, 60),
        ["bogota"] = new("Bogotá", "CO", "Lluvioso", 15, 80),
        ["lima"] = new("Lima", "PE", "Nublado", 19, 85),
        ["santiago"] = new("Santiago", "CL", "Soleado", 25, 40),
    };

    /// <summary>
    /// Obtiene el clima actual para una ciudad específica.
    /// El agente llamará automáticamente este método cuando el usuario
    /// pregunte sobre el clima de una ubicación.
    /// </summary>
    /// <param name="city">Nombre de la ciudad (ej: Madrid, Barcelona, México City)</param>
    /// <param name="country">Código de país ISO 3166-1 alpha-2 (ej: ES, MX, AR). Por defecto: ES</param>
    /// <returns>Descripción del clima actual en español</returns>
    [KernelFunction("get_weather")]
    [Description("Obtiene el clima actual para una ubicación específica. Úsala cuando el usuario pregunte sobre el clima, temperatura o condiciones meteorológicas de una ciudad.")]
    public string GetWeather(
        [Description("El nombre de la ciudad para consultar el clima")] string city,
        [Description("Código de país ISO (ej: ES para España, MX para México)")] string country = "ES")
    {
        // Normalizar el nombre de la ciudad para búsqueda
        var cityKey = city.ToLowerInvariant().Trim();
        
        // Buscar en la base de datos simulada
        if (_weatherDatabase.TryGetValue(cityKey, out var weather))
        {
            return $"""
                📍 Clima en {weather.City}, {weather.Country}:
                🌡️ Temperatura: {weather.Temperature}°C
                ☁️ Condición: {weather.Condition}
                💧 Humedad: {weather.Humidity}%
                """;
        }
        
        // Ciudad no encontrada - retornar respuesta informativa
        return $"""
            ⚠️ No tengo datos del clima para '{city}' ({country}).
            Ciudades disponibles: Madrid, Barcelona, Valencia, Sevilla, Bilbao, 
            Ciudad de México, Buenos Aires, Bogotá, Lima, Santiago.
            """;
    }

    /// <summary>
    /// Obtiene el pronóstico del clima para los próximos días.
    /// </summary>
    /// <param name="city">Nombre de la ciudad</param>
    /// <param name="days">Número de días para el pronóstico (1-7)</param>
    /// <returns>Pronóstico del clima en español</returns>
    [KernelFunction("get_forecast")]
    [Description("Obtiene el pronóstico del clima para los próximos días. Úsala cuando el usuario pregunte sobre el clima futuro o pronóstico.")]
    public string GetForecast(
        [Description("El nombre de la ciudad")] string city,
        [Description("Número de días para el pronóstico (1-7)")] int days = 3)
    {
        // Validar rango de días
        days = Math.Clamp(days, 1, 7);
        
        var cityKey = city.ToLowerInvariant().Trim();
        
        if (!_weatherDatabase.TryGetValue(cityKey, out var currentWeather))
        {
            return $"⚠️ No tengo datos de pronóstico para '{city}'.";
        }

        // Generar pronóstico simulado
        var forecast = new System.Text.StringBuilder();
        forecast.AppendLine($"📅 Pronóstico para {currentWeather.City} ({days} días):");
        forecast.AppendLine();
        
        var random = new Random(city.GetHashCode()); // Seed para consistencia
        var conditions = new[] { "Soleado", "Parcialmente nublado", "Nublado", "Lluvioso" };
        
        for (int i = 1; i <= days; i++)
        {
            var date = DateTime.Now.AddDays(i).ToString("dddd dd/MM", new System.Globalization.CultureInfo("es-ES"));
            var temp = currentWeather.Temperature + random.Next(-3, 4);
            var condition = conditions[random.Next(conditions.Length)];
            
            forecast.AppendLine($"  📆 {date}: {temp}°C, {condition}");
        }
        
        return forecast.ToString();
    }
}

/// <summary>
/// Estructura para almacenar datos del clima
/// </summary>
internal record WeatherData(
    string City,
    string Country,
    string Condition,
    int Temperature,
    int Humidity
);
```

### 3.2 Explicación de los Elementos Clave

**`[KernelFunction("get_weather")]`**:
- Marca el método como invocable por el agente
- El nombre entre comillas es el identificador que ve el modelo
- Sin este atributo, el agente NO puede llamar la función

**`[Description("...")]`**:
- Le dice al modelo **QUÉ hace** la función y **CUÁNDO usarla**
- Descripciones claras = mejores decisiones del modelo
- ¡Muy importante! Sin descripción, el modelo no sabe cuándo llamarla

**Descripciones de parámetros**:
- `[Description("El nombre de la ciudad...")]` en el parámetro
- Ayuda al modelo a extraer el valor correcto de la pregunta del usuario

---

## Paso 4: Implementar el Agente

### 4.1 Reemplazar Program.cs

Reemplaza el contenido de `Program.cs`:

```csharp
// ============================================================================
// Archivo: Program.cs
// Descripción: Agente con Function Tool personalizada para consultar el clima
// Módulo: 2 - Function Tools
// Lab: 01-custom-tool
// ============================================================================

using Microsoft.AI.Agents;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using WeatherAgent;

// ===== Configuración =====
// Cargar configuración desde appsettings.json y user secrets
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>()
    .Build();

// Obtener valores de configuración
var endpoint = configuration["AzureOpenAI:Endpoint"] 
    ?? throw new InvalidOperationException("AzureOpenAI:Endpoint no configurado");
var deploymentName = configuration["AzureOpenAI:DeploymentName"] 
    ?? throw new InvalidOperationException("AzureOpenAI:DeploymentName no configurado");
var apiKey = configuration["AzureOpenAI:ApiKey"] 
    ?? throw new InvalidOperationException("AzureOpenAI:ApiKey no configurado. Usa: dotnet user-secrets set 'AzureOpenAI:ApiKey' 'tu-key'");

// ===== Crear Kernel con Function Tools =====
var builder = Kernel.CreateBuilder();

// Agregar servicio de Azure OpenAI
builder.AddAzureOpenAIChatCompletion(
    deploymentName: deploymentName,
    endpoint: endpoint,
    apiKey: apiKey
);

// ===== CLAVE: Registrar el servicio como plugin =====
// Esto hace que las funciones decoradas con [KernelFunction] estén disponibles
// para que el agente las invoque automáticamente
builder.Plugins.AddFromType<WeatherService>();

var kernel = builder.Build();

// ===== Crear Agente con Function Calling Habilitado =====
var agent = new ChatCompletionAgent()
{
    Name = "AgenteDelClima",
    Instructions = """
        Eres un asistente experto en clima llamado AgenteDelClima.
        Puedes proporcionar información del clima actual y pronósticos.
        
        Cuando el usuario pregunte sobre el clima de una ciudad:
        1. Usa la función get_weather para obtener el clima actual
        2. Usa la función get_forecast si preguntan por el pronóstico
        
        Siempre responde en español de forma amigable y útil.
        Si el usuario no especifica una ciudad, pregunta amablemente cuál ciudad le interesa.
        """,
    Kernel = kernel,
    // Configurar para que el agente pueda llamar funciones automáticamente
    Arguments = new KernelArguments(
        new AzureOpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        }
    )
};

// ===== Historial de Conversación =====
var chatHistory = new ChatHistory();

// ===== Interfaz de Usuario =====
Console.WriteLine("============================================");
Console.WriteLine("🌤️  Agente del Clima con Function Tools");
Console.WriteLine("============================================");
Console.WriteLine($"Agente: {agent.Name}");
Console.WriteLine("Funciones disponibles: get_weather, get_forecast");
Console.WriteLine("Escribe 'salir' para terminar");
Console.WriteLine("============================================\n");

Console.WriteLine("💡 Prueba preguntar:");
Console.WriteLine("   - ¿Cómo está el clima en Madrid?");
Console.WriteLine("   - ¿Cuál es el pronóstico para Barcelona?");
Console.WriteLine("   - ¿Qué temperatura hace en México?\n");

// ===== Bucle de Conversación =====
while (true)
{
    Console.Write("👤 Tú: ");
    var userInput = Console.ReadLine();
    
    if (string.IsNullOrWhiteSpace(userInput) || 
        userInput.Equals("salir", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("\n🌤️ AgenteDelClima: ¡Hasta pronto! Que tengas un buen día. ☀️\n");
        break;
    }
    
    // Agregar mensaje del usuario al historial
    chatHistory.AddUserMessage(userInput);
    
    Console.Write("🌤️ AgenteDelClima: ");
    
    try
    {
        // Invocar el agente - automáticamente decidirá si llamar funciones
        await foreach (var message in agent.InvokeStreamingAsync(chatHistory))
        {
            Console.Write(message.Content);
        }
        Console.WriteLine("\n");
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
    
    // Gestión de historial
    if (chatHistory.Count > 10)
    {
        var messagesToKeep = chatHistory.Skip(chatHistory.Count - 10).ToList();
        chatHistory.Clear();
        foreach (var msg in messagesToKeep)
        {
            chatHistory.Add(msg);
        }
    }
}
```

### 4.2 Puntos Clave del Código

**Registro del Plugin**:
```csharp
builder.Plugins.AddFromType<WeatherService>();
```
Esta línea es **fundamental** - sin ella, el agente no conoce las funciones.

**Habilitar Function Calling**:
```csharp
Arguments = new KernelArguments(
    new AzureOpenAIPromptExecutionSettings
    {
        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
    }
)
```
- `FunctionChoiceBehavior.Auto()` permite al modelo decidir cuándo llamar funciones
- Alternativas: `Required()` (siempre llama), `None()` (nunca llama)

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
🌤️  Agente del Clima con Function Tools
============================================
Agente: AgenteDelClima
Funciones disponibles: get_weather, get_forecast
Escribe 'salir' para terminar
============================================

💡 Prueba preguntar:
   - ¿Cómo está el clima en Madrid?
   - ¿Cuál es el pronóstico para Barcelona?
   - ¿Qué temperatura hace en México?

👤 Tú: _
```

### 5.3 Probar la Invocación Automática

**Prueba 1: Clima actual**
```
👤 Tú: ¿Cómo está el clima en Madrid?
🌤️ AgenteDelClima: Según mis datos, el clima en Madrid está soleado con una temperatura de 22°C y una humedad del 45%. ¡Un día perfecto para salir! ☀️
```

**Prueba 2: Pronóstico**
```
👤 Tú: ¿Cuál es el pronóstico para Barcelona para los próximos 5 días?
🌤️ AgenteDelClima: Aquí tienes el pronóstico para Barcelona:
📅 Pronóstico para Barcelona (5 días):
  📆 lunes 13/01: 25°C, Parcialmente nublado
  📆 martes 14/01: 23°C, Soleado
  ...
```

**Prueba 3: Sin especificar ciudad**
```
👤 Tú: ¿Qué tiempo hace?
🌤️ AgenteDelClima: ¡Hola! Estaré encantado de ayudarte con el clima. ¿De qué ciudad te gustaría saber el tiempo?
```

**Observa**: El agente **NO llamó la función** porque no tenía suficiente información.

---

## Paso 6: Validación

### ✅ Checkpoint: Verificación de Function Calling

Confirma que tu agente funciona correctamente:

- [ ] ✅ El programa se ejecuta sin errores
- [ ] ✅ Al preguntar por el clima de Madrid, el agente responde con datos específicos (22°C, soleado)
- [ ] ✅ Al pedir pronóstico, el agente usa la función `get_forecast`
- [ ] ✅ Sin ciudad especificada, el agente pregunta al usuario
- [ ] ✅ El agente responde en español

**Prueba de validación definitiva**:
```
👤 Tú: Compara el clima entre Madrid y Barcelona
🌤️ AgenteDelClima: [Debería mostrar datos de ambas ciudades]
```

El agente debería llamar `get_weather` **dos veces** (una para cada ciudad).

---

## Experimentación (Opcional)

### Experimento 1: Agregar una Nueva Función

Agrega una función para convertir temperaturas:

```csharp
[KernelFunction("convert_temperature")]
[Description("Convierte temperatura entre Celsius y Fahrenheit")]
public string ConvertTemperature(
    [Description("Valor de temperatura a convertir")] double temperature,
    [Description("Unidad de origen: 'C' para Celsius o 'F' para Fahrenheit")] string fromUnit)
{
    if (fromUnit.ToUpper() == "C")
    {
        var fahrenheit = (temperature * 9/5) + 32;
        return $"{temperature}°C = {fahrenheit:F1}°F";
    }
    else
    {
        var celsius = (temperature - 32) * 5/9;
        return $"{temperature}°F = {celsius:F1}°C";
    }
}
```

Luego prueba: "¿Cuánto son 30 grados Celsius en Fahrenheit?"

### Experimento 2: Probar Descripciones Diferentes

Cambia la descripción de `get_weather` a algo vago:
```csharp
[Description("Función de clima")]
```

**Pregunta**: ¿El agente todavía la llama correctamente? ¿Qué pasa?

---

## Solución de Problemas

### El agente no llama la función

**Síntoma**: Preguntas sobre el clima pero el agente inventa datos o dice que no puede ayudar.

**Causa**: La función no está registrada o la descripción no es clara.

**Solución**:
1. Verifica que tienes `builder.Plugins.AddFromType<WeatherService>();`
2. Verifica que `FunctionChoiceBehavior.Auto()` está configurado
3. Mejora la descripción de la función para ser más específica

---

### Error: "Function not found"

**Síntoma**: Error indicando que la función no existe.

**Causa**: El servicio no está registrado como plugin.

**Solución**:
```csharp
// Asegúrate de tener esta línea ANTES de builder.Build()
builder.Plugins.AddFromType<WeatherService>();
```

---

### La función se llama con parámetros incorrectos

**Síntoma**: El agente llama `get_weather("España")` en lugar de `get_weather("Madrid", "ES")`.

**Causa**: Descripciones de parámetros ambiguas.

**Solución**: Sé muy específico en las descripciones:
```csharp
[Description("El nombre de la CIUDAD (no país), por ejemplo: Madrid, Barcelona, Valencia")]
```

---

## Resumen

En este laboratorio aprendiste:

✅ **Definir Function Tools** con `[KernelFunction]` y `[Description]`  
✅ **Registrar plugins** en el kernel con `AddFromType<T>()`  
✅ **Habilitar function calling** con `FunctionChoiceBehavior.Auto()`  
✅ **Las descripciones son cruciales** para que el modelo tome buenas decisiones  
✅ **El modelo decide cuándo llamar** - no necesitas lógica de routing manual

### Conceptos Clave

| Concepto | Descripción |
|----------|-------------|
| **KernelFunction** | Atributo que marca métodos como invocables por agentes |
| **Description** | Metadatos que ayudan al modelo a entender qué hace la función |
| **Plugin** | Colección de funciones relacionadas registradas en el kernel |
| **FunctionChoiceBehavior** | Configuración de cuándo el modelo puede/debe llamar funciones |

---

## Próximos Pasos

Continúa con [Lab 02: Agent-as-Tool](../02-agent-as-tool/) donde aprenderás a usar un agente completo como función de otro agente.

---

## Referencias

- [Semantic Kernel Plugins](https://learn.microsoft.com/semantic-kernel/agents/plugins)
- [Function Calling Guide](https://learn.microsoft.com/azure/ai-services/openai/how-to/function-calling)
- [KernelFunction Documentation](https://learn.microsoft.com/dotnet/api/microsoft.semantickernel.kernelfunctionattribute)

---

**Tiempo completado**: ~20 minutos  
**¡Felicitaciones!** 🎉 Tu agente ahora puede usar funciones personalizadas automáticamente.

# 🔧 Guía de Troubleshooting - Workshop Microsoft Agent Framework

Esta guía cubre los **10 problemas más comunes** que enfrentarán los participantes durante el workshop, con soluciones paso a paso.

---

## 📋 Índice de Problemas

| # | Error | Módulo(s) Afectados |
|---|-------|---------------------|
| 1 | [".NET SDK no encontrado"](#1-net-sdk-no-encontrado) | Todos |
| 2 | ["401 Unauthorized" con Azure OpenAI](#2-401-unauthorized-con-azure-openai) | Todos |
| 3 | ["429 Rate Limit Exceeded"](#3-429-rate-limit-exceeded) | Todos |
| 4 | ["El agente no responde / respuestas vacías"](#4-el-agente-no-responde--respuestas-vacías) | 1, 2, 3 |
| 5 | ["Function tool no se llama automáticamente"](#5-function-tool-no-se-llama-automáticamente) | 2, 3 |
| 6 | ["Azure OpenAI connection timeout"](#6-azure-openai-connection-timeout) | Todos |
| 7 | ["NuGet restore fails / packages no se descargan"](#7-nuget-restore-fails--packages-no-se-descargan) | Todos |
| 8 | ["Puerto 5000 ya está en uso"](#8-puerto-5000-ya-está-en-uso) | 5 |
| 9 | ["Aspire Dashboard no accesible"](#9-aspire-dashboard-no-accesible) | 5 |
| 10 | ["DevUI no conecta con el agente"](#10-devui-no-conecta-con-el-agente) | 6 |

---

## 1. ".NET SDK no encontrado"

### Síntomas

```bash
$ dotnet --version
bash: dotnet: command not found
```

O al ejecutar `dotnet run`:

```
'dotnet' no se reconoce como un comando interno o externo
```

### Causas Comunes

- .NET SDK no está instalado
- PATH del sistema no incluye la ruta de .NET
- Terminal/consola no se reinició después de la instalación

### Solución

#### Verificar instalación

```bash
# Windows (PowerShell)
where.exe dotnet

# macOS/Linux
which dotnet
```

Si no retorna una ruta, .NET no está instalado.

#### Instalar .NET 10 SDK

1. Descargar desde: https://dot.net/download
2. Instalar para tu OS (Windows/macOS/Linux)
3. **Reiniciar terminal/VS Code**
4. Verificar:
   ```bash
   dotnet --version
   ```
   Debe mostrar `10.0.x`

#### Agregar .NET al PATH (si ya está instalado pero no se encuentra)

**Windows**:
1. Buscar "Variables de entorno" en el menú Inicio
2. Editar variable PATH del usuario
3. Agregar: `C:\Program Files\dotnet`
4. Reiniciar terminal

**macOS/Linux**:
Agregar a `~/.bashrc` o `~/.zshrc`:
```bash
export PATH="$PATH:/usr/local/share/dotnet"
```
Ejecutar: `source ~/.bashrc` (o `~/.zshrc`)

---

## 2. "401 Unauthorized" con Azure OpenAI

### Síntomas

```
System.Net.Http.HttpRequestException: Response status code does not indicate success: 401 (Unauthorized).
```

O:

```
Azure.RequestFailedException: Access denied due to invalid subscription key or wrong API endpoint.
```

### Causas Comunes

- API Key incorrecta o con espacios extra
- Endpoint incorrecto (falta `/` final o región errónea)
- API Key expirada o recurso deshabilitado
- User secrets no configurados correctamente

### Solución

#### Verificar configuración de user secrets

```bash
cd [directorio-del-lab]
dotnet user-secrets list
```

**Output esperado**:
```
AzureOpenAI:Endpoint = https://[nombre-recurso].openai.azure.com/
AzureOpenAI:ApiKey = [tu-api-key]
AzureOpenAI:DeploymentName = gpt-5.2
```

#### Reconfigurar user secrets

```bash
# Limpiar secretos existentes
dotnet user-secrets clear

# Configurar nuevamente
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://[nombre-recurso].openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:ApiKey" "[api-key-sin-espacios]"
dotnet user-secrets set "AzureOpenAI:DeploymentName" "gpt-5.2"
```

**⚠️ IMPORTANTE**:
- Endpoint DEBE terminar con `/`
- API Key NO debe tener espacios al inicio/final
- Deployment name DEBE coincidir con el nombre en Azure Portal

#### Verificar recurso en Azure Portal

1. Ir a: https://portal.azure.com
2. Buscar recurso de Azure OpenAI
3. Verificar que está **"Running"** (no "Stopped")
4. Ir a **"Keys and Endpoint"**
5. Copiar **KEY 1** o **KEY 2**
6. Verificar que endpoint coincide con tu configuración

#### Probar conexión manualmente

```bash
curl -X POST "https://[nombre-recurso].openai.azure.com/openai/deployments/gpt-5.2/chat/completions?api-version=2024-02-01" \
  -H "api-key: [tu-api-key]" \
  -H "Content-Type: application/json" \
  -d '{
    "messages": [{"role":"user","content":"test"}],
    "max_tokens": 10
  }'
```

**Output esperado**: JSON con respuesta del modelo
**Si falla**: API key o endpoint incorrectos

---

## 3. "429 Rate Limit Exceeded"

### Síntomas

```
Azure.RequestFailedException: Rate limit is exceeded. Try again in X seconds.
```

O:

```
Status Code: 429
Error: requests_per_minute_exceeded
```

### Causas Comunes

- Demasiadas solicitudes simultáneas al modelo
- Tokens por minuto (TPM) excedidos
- Cuota compartida entre múltiples participantes

### Solución

#### Solución inmediata: Esperar y reintentar

```csharp
// Agregar manejo de reintentos en código
using Azure.AI.Agents;

var options = new AgentClientOptions
{
    RetryPolicy = new RetryPolicy(maxRetries: 3, delay: TimeSpan.FromSeconds(5))
};
```

#### Verificar cuotas en Azure Portal

1. Ir al recurso de Azure OpenAI
2. Menú: **"Quotas"**
3. Verificar:
   - **Tokens per Minute (TPM)**: Límite actual
   - **Requests per Minute (RPM)**: Límite de solicitudes

**Para workshops**: Se recomienda mínimo 100,000 TPM

#### Reducir carga en el modelo

**Opción A**: Reducir max_tokens en las respuestas

```csharp
var completionOptions = new ChatCompletionOptions
{
    MaxTokens = 500  // Reducir de 2000 a 500
};
```

**Opción B**: Agregar delay entre solicitudes

```csharp
await Task.Delay(TimeSpan.FromSeconds(2));
await agent.InvokeAsync(messages);
```

#### Para instructores: Monitorear uso en tiempo real

```bash
# Ver métricas en Azure Portal
az monitor metrics list \
  --resource [resource-id] \
  --metric "TokensPerMinute" \
  --interval PT1M
```

---

## 4. "El agente no responde / respuestas vacías"

### Síntomas

- `agent.InvokeAsync()` retorna string vacío
- No hay output en consola después de invocar agente
- Proceso se ejecuta pero sin resultados visibles

### Causas Comunes

- Deployment name incorrecto en configuración
- Modelo no desplegado en Azure OpenAI
- Mensajes vacíos o mal formateados
- Configuración de ChatCompletionAgent incompleta

### Solución

#### Verificar deployment name

**En Azure Portal**:
1. Ir al recurso de Azure OpenAI
2. Menú: **"Model deployments"**
3. Verificar que existe deployment llamado **"gpt-5.2"** (o el nombre configurado)

**En código**:
```csharp
// Verificar que coincide con Azure Portal
var agent = new ChatCompletionAgent
{
    Model = "gpt-5.2",  // ← Debe coincidir EXACTAMENTE con deployment name
    // ...
};
```

#### Verificar formato de mensajes

```csharp
// ✅ CORRECTO
var messages = new List<ChatMessage>
{
    new ChatMessage(ChatRole.User, "Hola, ¿cómo estás?")
};

// ❌ INCORRECTO (mensaje vacío)
var messages = new List<ChatMessage>
{
    new ChatMessage(ChatRole.User, "")  // ← Vacío
};
```

#### Agregar logging para debugging

```csharp
var response = await agent.InvokeAsync(messages);
Console.WriteLine($"[DEBUG] Response length: {response.Length}");
Console.WriteLine($"[DEBUG] Response content: {response}");

if (string.IsNullOrEmpty(response))
{
    Console.WriteLine("[ERROR] El agente retornó respuesta vacía");
    // Verificar configuración
}
```

#### Probar con mensaje simple

```csharp
// Test mínimo para verificar conectividad
var testAgent = new ChatCompletionAgent
{
    Model = "gpt-5.2",
    Instructions = "Eres un asistente útil."
};

var testMessages = new List<ChatMessage>
{
    new ChatMessage(ChatRole.User, "Di 'hola'")
};

var result = await testAgent.InvokeAsync(testMessages);
Console.WriteLine($"Test result: {result}");
```

Si este test falla, el problema es de configuración (volver a problema #2).

---

## 5. "Function tool no se llama automáticamente"

### Síntomas

- El agente responde en texto pero NO ejecuta la función
- Función definida con `[KernelFunction]` pero nunca se invoca
- Output: "Necesitarías llamar a la función X..." (en lugar de llamarla)

### Causas Comunes

- Falta atributo `[KernelFunction]`
- Falta `[Description]` en función o parámetros
- Plugin no registrado en el agente
- Función no es pública

### Solución

#### Verificar estructura de la función

```csharp
// ✅ CORRECTO
public class CalculadoraPlugin
{
    [KernelFunction]
    [Description("Suma dos números")]
    public int Sumar(
        [Description("Primer número")] int a,
        [Description("Segundo número")] int b)
    {
        return a + b;
    }
}

// ❌ INCORRECTO (falta Description)
public class CalculadoraPlugin
{
    [KernelFunction]  // ← Falta Description global
    public int Sumar(int a, int b)  // ← Faltan Description de parámetros
    {
        return a + b;
    }
}
```

**⚠️ IMPORTANTE**: `[Description]` es CRÍTICO para que el LLM entienda cuándo llamar la función.

#### Verificar registro del plugin

```csharp
// ✅ CORRECTO
var calculadora = new CalculadoraPlugin();
var agent = new ChatCompletionAgent
{
    Model = "gpt-5.2",
    Instructions = "Eres un asistente con capacidad de calcular.",
    Tools = [calculadora]  // ← Plugin registrado
};

// ❌ INCORRECTO (plugin creado pero no registrado)
var calculadora = new CalculadoraPlugin();
var agent = new ChatCompletionAgent
{
    Model = "gpt-5.2",
    Instructions = "Eres un asistente con capacidad de calcular."
    // ← Falta Tools = [calculadora]
};
```

#### Agregar logging para ver llamadas a funciones

```csharp
[KernelFunction]
[Description("Suma dos números")]
public int Sumar(
    [Description("Primer número")] int a,
    [Description("Segundo número")] int b)
{
    Console.WriteLine($"[TOOL CALL] Sumar({a}, {b})");
    return a + b;
}
```

Si NO ves este log, la función no se está llamando.

#### Verificar que el prompt "pide" usar la herramienta

```csharp
// ✅ CORRECTO (prompt explícito)
var messages = new List<ChatMessage>
{
    new ChatMessage(ChatRole.User, "¿Cuánto es 5 + 3?")
};
// El LLM reconocerá que debe usar Sumar()

// ⚠️ AMBIGUO (puede no llamar función)
var messages = new List<ChatMessage>
{
    new ChatMessage(ChatRole.User, "Háblame de matemáticas")
};
// El LLM puede responder en texto sin llamar funciones
```

---

## 6. "Azure OpenAI connection timeout"

### Síntomas

```
System.Threading.Tasks.TaskCanceledException: The request was canceled due to the configured HttpClient.Timeout
```

O:

```
A task was canceled.
```

### Causas Comunes

- Firewall corporativo bloqueando `openai.azure.com`
- Proxy no configurado
- Red inestable o latencia alta
- Recurso de Azure OpenAI en región muy lejana

### Solución

#### Verificar conectividad a Azure OpenAI

```bash
# Test básico
curl -I https://[nombre-recurso].openai.azure.com/

# Output esperado: HTTP/2 401 (Unauthorized es OK, significa que hay conectividad)
# Output problemático: timeout o connection refused
```

#### Configurar proxy (si es necesario)

```csharp
var handler = new HttpClientHandler
{
    Proxy = new WebProxy("http://proxy.empresa.com:8080"),
    UseProxy = true
};

var httpClient = new HttpClient(handler);
// Usar este httpClient en Azure SDK
```

#### Aumentar timeout

```csharp
var clientOptions = new AgentClientOptions
{
    Transport = new HttpClientTransport(new HttpClient
    {
        Timeout = TimeSpan.FromMinutes(5)  // Default: 100 segundos
    })
};
```

#### Para participantes detrás de firewall corporativo

**Solicitar a IT que permita**:
- `*.openai.azure.com` (puerto 443)
- `*.azure.com` (puerto 443)

**Alternativa temporal**: Usar hotspot de celular

---

## 7. "NuGet restore fails / packages no se descargan"

### Síntomas

```
error NU1301: Unable to load the service index for source https://api.nuget.org/v3/index.json
```

O:

```
error: Failed to retrieve information about 'Microsoft.Extensions.AI.Agents' from remote source
```

### Causas Comunes

- Sin conexión a internet
- NuGet.org bloqueado por firewall
- Caché de NuGet corrupto
- Versión del package no existe

### Solución

#### Limpiar caché de NuGet

```bash
dotnet nuget locals all --clear
dotnet restore
```

#### Verificar conectividad a NuGet.org

```bash
curl -I https://api.nuget.org/v3/index.json

# Output esperado: HTTP/2 200 OK
```

#### Configurar proxy para NuGet (si es necesario)

```bash
# Agregar a NuGet.config
dotnet nuget add source https://api.nuget.org/v3/index.json \
  --name "NuGet" \
  --configfile ~/.nuget/NuGet/NuGet.Config \
  --proxy http://proxy.empresa.com:8080
```

#### Para workshops offline: Preparar caché local

**Para instructores**:
```bash
# Antes del workshop, descargar todos los packages
dotnet restore --packages ./offline-packages

# Distribuir carpeta offline-packages a participantes
```

**Para participantes**:
```bash
dotnet restore --packages ./offline-packages --source ./offline-packages
```

#### Verificar versión del package

```bash
# Ver versiones disponibles
dotnet package search Microsoft.Extensions.AI.Agents

# Si el package no existe, verificar nombre correcto en workshop README
```

---

## 8. "Puerto 5000 ya está en uso"

### Síntomas

```
System.IO.IOException: Failed to bind to address http://127.0.0.1:5000: address already in use.
```

O:

```
Unable to start Kestrel: Address already in use
```

### Causas Comunes

- Otra instancia del lab está corriendo
- Otro servicio usando puerto 5000
- Lab anterior no se detuvo correctamente

### Solución

#### Identificar proceso usando el puerto

**Windows (PowerShell)**:
```powershell
netstat -ano | findstr :5000
```

Luego matar proceso:
```powershell
taskkill /PID [número-del-PID] /F
```

**macOS/Linux**:
```bash
lsof -i :5000
```

Luego matar proceso:
```bash
kill -9 [PID]
```

#### Cambiar puerto de la aplicación

**Opción A**: En `appsettings.json`
```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5001"
      }
    }
  }
}
```

**Opción B**: Por línea de comandos
```bash
dotnet run --urls "http://localhost:5001"
```

**Opción C**: Variable de entorno
```bash
export ASPNETCORE_URLS="http://localhost:5001"
dotnet run
```

#### Detener todos los procesos de .NET

```bash
# Windows
taskkill /F /IM dotnet.exe

# macOS/Linux
killall -9 dotnet
```

---

## 9. "Aspire Dashboard no accesible"

### Síntomas

- `dotnet run` del AppHost inicia pero dashboard no abre
- Browser muestra "Connection refused" en `http://localhost:15001`
- Dashboard URL no aparece en consola

### Causas Comunes

- Puerto 15001 ya en uso
- Aspire Dashboard no habilitado en configuración
- Firewall bloqueando acceso local

### Solución

#### Verificar que AppHost está corriendo

```bash
# En la carpeta del AppHost
dotnet run

# Output esperado:
# Aspire Dashboard: http://localhost:15001
# Resource endpoint: http://localhost:5000
```

Si NO ves "Aspire Dashboard: ...", Aspire no está configurado.

#### Habilitar Aspire Dashboard

En `Program.cs` del AppHost:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Asegurar que dashboard está habilitado (debería ser por defecto)
builder.Build().Run();
```

En `appsettings.json` del AppHost:

```json
{
  "Aspire": {
    "Dashboard": {
      "Enabled": true,
      "Port": 15001
    }
  }
}
```

#### Cambiar puerto del dashboard

Si puerto 15001 está en uso:

```bash
# Variable de entorno
export ASPIRE_DASHBOARD_PORT=15002
dotnet run
```

O en `launchSettings.json`:

```json
{
  "profiles": {
    "http": {
      "environmentVariables": {
        "ASPIRE_DASHBOARD_PORT": "15002"
      }
    }
  }
}
```

#### Abrir dashboard manualmente

```bash
# Si dashboard está corriendo pero no abrió browser
open http://localhost:15001  # macOS
start http://localhost:15001  # Windows
xdg-open http://localhost:15001  # Linux
```

---

## 10. "DevUI no conecta con el agente"

### Síntomas

- DevUI inicia pero no muestra conversaciones
- Panel "Agent Interactions" vacío
- Error: "Unable to connect to instrumentation endpoint"

### Causas Comunes

- Instrumentación no configurada en el agente
- DevUI y agente en procesos separados sin comunicación
- Falta configuración de telemetría

### Solución

#### Verificar configuración de instrumentación

En el proyecto del agente:

```csharp
using Microsoft.Extensions.AI.Agents.DevUI;

// Habilitar instrumentación para DevUI
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddAgentInstrumentation(options =>
{
    options.EnableDevUI = true;
    options.DevUIPort = 5002;  // Puerto del DevUI
});

var host = builder.Build();
```

#### Verificar que DevUI está corriendo

```bash
# Iniciar DevUI
dotnet tool install --global Microsoft.Extensions.AI.DevUI
ai-devui

# Output esperado:
# DevUI listening on http://localhost:5002
```

#### Configurar endpoint de instrumentación

En el código del agente:

```csharp
var agent = new ChatCompletionAgent
{
    Model = "gpt-5.2",
    Instructions = "...",
    Instrumentation = new InstrumentationOptions
    {
        Endpoint = "http://localhost:5002",  // ← Endpoint del DevUI
        EnableTracing = true
    }
};
```

#### Verificar firewall local

```bash
# Verificar que puerto 5002 está accesible
curl http://localhost:5002/health

# Output esperado: HTTP/1.1 200 OK
```

#### Alternativa: Usar como biblioteca embebida

```csharp
// En lugar de herramienta global, embeber DevUI en la app
builder.Services.AddAgentDevUI();

// DevUI se servirá en /devui del mismo proceso
// http://localhost:5000/devui
```

---

## 📞 Escalación de Problemas

Si después de seguir estas soluciones el problema persiste:

1. **Recopilar información de diagnóstico**:
   ```bash
   dotnet --info > diagnostics.txt
   echo "---" >> diagnostics.txt
   dotnet nuget locals all --list >> diagnostics.txt
   ```

2. **Contactar al instructor** con:
   - Descripción del problema
   - Mensaje de error completo
   - Output de script de verificación (`./scripts/verify-environment.sh`)
   - Archivo `diagnostics.txt`

3. **Durante el workshop**:
   - Levantar mano físicamente o virtual
   - Escribir en chat: "🆘 [nombre] - [problema #]"
   - Ejemplo: "🆘 Juan - problema #2 (401 Unauthorized)"

---

## 🎯 Problemas por Módulo

### Módulo 1: Fundamentos
- Problema #1, #2, #4

### Módulo 2: Function Tools
- Problema #5 (mayoría de casos)
- Problema #2, #3

### Módulo 3: Workflows
- Problema #3 (rate limits con múltiples agentes)
- Problema #5

### Módulo 4: Observabilidad
- Problema #6 (timeouts con telemetría)
- Problema #10 (si usan DevUI)

### Módulo 5: ASP.NET + Aspire
- Problema #8 (puertos)
- Problema #9 (Aspire Dashboard)

### Módulo 6: DevUI
- Problema #10 (mayoría de casos)

### Módulo 7: MCP
- Sin labs prácticos, pocos problemas técnicos

---

## 🔍 Diagnóstico Rápido

### Checklist de 30 segundos

1. **¿Dotnet funciona?**
   ```bash
   dotnet --version
   ```
   Si falla → Problema #1

2. **¿Azure OpenAI responde?**
   ```bash
   curl https://[recurso].openai.azure.com/
   ```
   Si 401 → OK (conectividad existe)
   Si timeout → Problema #6

3. **¿User secrets configurados?**
   ```bash
   dotnet user-secrets list
   ```
   Si vacío → Problema #2

4. **¿Packages restaurados?**
   ```bash
   dotnet restore
   ```
   Si falla → Problema #7

5. **¿Puerto disponible?**
   ```bash
   lsof -i :5000  # macOS/Linux
   netstat -ano | findstr :5000  # Windows
   ```
   Si ocupado → Problema #8

---

## 📚 Recursos Adicionales

- [Documentación oficial MAF](https://learn.microsoft.com/microsoft-agent-framework)
- [Azure OpenAI Troubleshooting](https://learn.microsoft.com/azure/ai-services/openai/troubleshooting)
- [.NET CLI Reference](https://learn.microsoft.com/dotnet/core/tools/)
- [Aspire Dashboard Guide](https://learn.microsoft.com/dotnet/aspire/dashboard)

---

**Última actualización**: [Fecha]  
**Workshop version**: 1.0  
**Mantenedor**: [Instructor/Organización]

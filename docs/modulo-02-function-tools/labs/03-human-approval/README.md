# Lab 03: Human Approval - Aprobación Humana para Operaciones Sensibles

**Duración**: 20 minutos  
**Complejidad**: Standard  
**Objetivo**: Implementar un workflow donde el agente requiere aprobación humana antes de ejecutar acciones sensibles

## Objetivo

En este laboratorio, aprenderás a implementar el patrón **Human-in-the-Loop** donde ciertas operaciones del agente **pausan y esperan confirmación del usuario** antes de ejecutarse.

Este patrón es esencial para:
- **Operaciones destructivas**: Eliminar archivos, borrar datos
- **Transacciones financieras**: Transferencias, pagos
- **Comunicaciones externas**: Enviar correos, mensajes
- **Cambios de configuración**: Modificar ajustes críticos
- **Cualquier acción irreversible**

Al finalizar, tu agente podrá realizar operaciones sensibles de forma segura, siempre con supervisión humana.

---

## Prerequisitos

### Software Requerido
- ✅ .NET 10 SDK instalado
- ✅ Visual Studio Code o Visual Studio 2025

### Conocimientos Previos
- ✅ Completar Lab 01 (Custom Function Tool)
- ✅ Completar Lab 02 (Agent-as-Tool)
- Entender cómo funcionan las function tools con `AIFunctionFactory`

### Configuración de Azure
- ✅ Azure OpenAI Service con deployment de `gpt-5.2`
- ✅ Endpoint y API Key disponibles

---

## Paso 1: Preparación del Proyecto

### 1.1 Crear Proyecto

```bash
mkdir ApprovalWorkflow
cd ApprovalWorkflow
dotnet new console -n ApprovalWorkflow
cd ApprovalWorkflow
```

### 1.2 Instalar Paquetes NuGet

```bash
dotnet add package Microsoft.Agents.AI --version 1.0.0-preview.260108.1
dotnet add package Microsoft.Extensions.Configuration --version 10.0.0
dotnet add package Microsoft.Extensions.Configuration.Json --version 10.0.0
dotnet add package Microsoft.Extensions.Configuration.UserSecrets --version 10.0.0
```

---

## Paso 2: Configuración

### 2.1 Crear appsettings.json

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

### 2.2 Configurar User Secrets

```bash
dotnet user-secrets init
dotnet user-secrets set "AzureOpenAI:ApiKey" "TU-API-KEY-AQUI"
```

### 2.3 Actualizar .csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <UserSecretsId>maf-workshop-approval-workflow-02</UserSecretsId>
  </PropertyGroup>

  <ItemGroup>
    <!-- Microsoft Agent Framework -->
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

## Paso 3: Implementar Operaciones Sensibles

### 3.1 Crear SensitiveOperations.cs

Este servicio contiene operaciones donde algunas **pausan para aprobación humana**:

```csharp
// ============================================================================
// Archivo: SensitiveOperations.cs
// Descripción: Operaciones sensibles que requieren aprobación humana
// Módulo: 2 - Function Tools
// Lab: 03-human-approval
// ============================================================================

namespace ApprovalWorkflow;

/// <summary>
/// Servicio con operaciones sensibles que requieren aprobación humana.
/// Demuestra el patrón Human-in-the-Loop donde ciertas acciones
/// no se ejecutan automáticamente sino que pausan para confirmación.
/// </summary>
public static class SensitiveOperations
{
    // Simulación de archivos en el sistema
    private static readonly Dictionary<string, string> _virtualFileSystem = new()
    {
        ["/documentos/informe.txt"] = "Contenido del informe anual...",
        ["/documentos/presupuesto.xlsx"] = "[Datos de presupuesto]",
        ["/temporal/cache.tmp"] = "Datos temporales",
        ["/temporal/logs.txt"] = "Logs del sistema",
        ["/importante/backup.zip"] = "[Backup crítico del sistema]"
    };

    // Simulación de correos pendientes
    private static readonly List<PendingEmail> _pendingEmails = new();
    
    // Balance simulado de la cuenta
    private static decimal _accountBalance = 10000.00m;

    /// <summary>
    /// Lista los archivos disponibles en el sistema virtual.
    /// Esta operación NO requiere aprobación (solo lectura).
    /// </summary>
    public static string ListFiles()
    {
        var files = string.Join("\n", _virtualFileSystem.Keys.Select(f => $"  📄 {f}"));
        return $"📁 Archivos en el sistema:\n{files}";
    }

    /// <summary>
    /// Consulta el balance de la cuenta.
    /// Esta operación NO requiere aprobación (solo lectura).
    /// </summary>
    public static string CheckBalance()
    {
        return $"💰 Balance actual: ${_accountBalance:N2}";
    }

    /// <summary>
    /// Elimina un archivo del sistema virtual.
    /// ⚠️ REQUIERE APROBACIÓN HUMANA antes de ejecutarse.
    /// </summary>
    public static string DeleteFile(string filePath)
    {
        // Validar que el archivo existe
        if (!_virtualFileSystem.ContainsKey(filePath))
        {
            return $"❌ Error: El archivo '{filePath}' no existe.";
        }

        // SOLICITAR APROBACIÓN HUMANA
        Console.WriteLine();
        Console.WriteLine("╔═══════════════════════════════════════════════════════╗");
        Console.WriteLine("║  ⚠️  APROBACIÓN REQUERIDA - OPERACIÓN SENSIBLE        ║");
        Console.WriteLine("╠═══════════════════════════════════════════════════════╣");
        Console.WriteLine($"║  Acción: ELIMINAR ARCHIVO                             ║");
        Console.WriteLine($"║  Archivo: {filePath,-43} ║");
        Console.WriteLine("╠═══════════════════════════════════════════════════════╣");
        Console.WriteLine("║  ¿Aprobar esta operación? (s/n):                      ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════╝");
        Console.Write(">>> ");

        var response = Console.ReadLine()?.Trim().ToLower();

        if (response == "s" || response == "si" || response == "yes" || response == "y")
        {
            _virtualFileSystem.Remove(filePath);
            return $"✅ Archivo '{filePath}' eliminado exitosamente. [APROBADO POR USUARIO]";
        }
        else
        {
            return $"🚫 Operación CANCELADA por el usuario. El archivo '{filePath}' no fue eliminado.";
        }
    }

    /// <summary>
    /// Envía un correo electrónico.
    /// ⚠️ REQUIERE APROBACIÓN HUMANA antes de ejecutarse.
    /// </summary>
    public static string SendEmail(string to, string subject, string body)
    {
        // SOLICITAR APROBACIÓN HUMANA
        Console.WriteLine();
        Console.WriteLine("╔═══════════════════════════════════════════════════════╗");
        Console.WriteLine("║  ⚠️  APROBACIÓN REQUERIDA - OPERACIÓN SENSIBLE        ║");
        Console.WriteLine("╠═══════════════════════════════════════════════════════╣");
        Console.WriteLine($"║  Acción: ENVIAR CORREO                                ║");
        Console.WriteLine($"║  Para: {to,-47} ║");
        Console.WriteLine($"║  Asunto: {subject,-44} ║");
        Console.WriteLine("╠═══════════════════════════════════════════════════════╣");
        Console.WriteLine("║  ¿Aprobar esta operación? (s/n):                      ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════╝");
        Console.Write(">>> ");

        var response = Console.ReadLine()?.Trim().ToLower();

        if (response == "s" || response == "si" || response == "yes" || response == "y")
        {
            _pendingEmails.Add(new PendingEmail(to, subject, body, DateTime.Now));
            return $"✅ Correo enviado a '{to}' con asunto '{subject}'. [APROBADO POR USUARIO]";
        }
        else
        {
            return $"🚫 Envío de correo CANCELADO por el usuario.";
        }
    }

    /// <summary>
    /// Transfiere dinero a otra cuenta.
    /// ⚠️ REQUIERE APROBACIÓN HUMANA antes de ejecutarse.
    /// </summary>
    public static string TransferFunds(string toAccount, decimal amount, string concept)
    {
        // Validar fondos suficientes
        if (amount > _accountBalance)
        {
            return $"❌ Error: Fondos insuficientes. Balance: ${_accountBalance:N2}, Monto solicitado: ${amount:N2}";
        }

        // SOLICITAR APROBACIÓN HUMANA
        Console.WriteLine();
        Console.WriteLine("╔═══════════════════════════════════════════════════════╗");
        Console.WriteLine("║  ⚠️  APROBACIÓN REQUERIDA - OPERACIÓN SENSIBLE        ║");
        Console.WriteLine("╠═══════════════════════════════════════════════════════╣");
        Console.WriteLine($"║  Acción: TRANSFERENCIA BANCARIA                       ║");
        Console.WriteLine($"║  Cuenta destino: {toAccount,-35} ║");
        Console.WriteLine($"║  Monto: ${amount,-45:N2} ║");
        Console.WriteLine($"║  Concepto: {concept,-41} ║");
        Console.WriteLine("╠═══════════════════════════════════════════════════════╣");
        Console.WriteLine("║  ¿Aprobar esta operación? (s/n):                      ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════╝");
        Console.Write(">>> ");

        var response = Console.ReadLine()?.Trim().ToLower();

        if (response == "s" || response == "si" || response == "yes" || response == "y")
        {
            _accountBalance -= amount;
            return $"✅ Transferencia de ${amount:N2} a cuenta '{toAccount}' completada. Nuevo balance: ${_accountBalance:N2}. [APROBADO POR USUARIO]";
        }
        else
        {
            return $"🚫 Transferencia CANCELADA por el usuario. No se realizó ningún movimiento.";
        }
    }
}

/// <summary>
/// Representa un correo pendiente de envío.
/// </summary>
public record PendingEmail(string To, string Subject, string Body, DateTime CreatedAt);
```

### 3.2 Puntos Clave del Código

| Elemento | Descripción |
|----------|-------------|
| **Métodos estáticos** | Todos los métodos son estáticos para usar con `AIFunctionFactory.Create` |
| **ListFiles y CheckBalance** | Operaciones de solo lectura, NO requieren aprobación |
| **DeleteFile, SendEmail, TransferFunds** | ⚠️ REQUIEREN aprobación humana antes de ejecutar |
| **Console.ReadLine()** | Pausa la ejecución hasta que el usuario responde |
| **Validaciones** | Verificar existencia de archivo, fondos suficientes, etc. |

---

## Paso 4: Implementar el Agente Principal

### 4.1 Crear Program.cs

```csharp
// ============================================================================
// Archivo: Program.cs
// Descripción: Agente con Human-in-the-Loop para operaciones sensibles
// Módulo: 2 - Function Tools
// Lab: 03-human-approval
// ============================================================================

using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Abstractions;
using Microsoft.Agents.AI.ChatCompletion;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

// Configuración
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddUserSecrets<Program>()
    .Build();

var endpoint = configuration["AzureOpenAI:Endpoint"]!;
var apiKey = configuration["AzureOpenAI:ApiKey"]!;
var deploymentName = configuration["AzureOpenAI:DeploymentName"]!;

// Crear cliente OpenAI
var openAIClient = new AzureOpenAIClient(
    new Uri(endpoint),
    new System.ClientModel.ApiKeyCredential(apiKey));
var chatClient = openAIClient.AsChatClient(deploymentName);

// Crear herramientas usando AIFunctionFactory
// Operaciones de SOLO LECTURA (sin aprobación)
var listFilesFunction = AIFunctionFactory.Create(
    ApprovalWorkflow.SensitiveOperations.ListFiles,
    name: "list_files",
    description: "Lista los archivos disponibles en el sistema. Operación segura de solo lectura."
);

var checkBalanceFunction = AIFunctionFactory.Create(
    ApprovalWorkflow.SensitiveOperations.CheckBalance,
    name: "check_balance",
    description: "Consulta el balance actual de la cuenta bancaria. Operación segura de solo lectura."
);

// Operaciones SENSIBLES (requieren aprobación)
var deleteFileFunction = AIFunctionFactory.Create(
    ApprovalWorkflow.SensitiveOperations.DeleteFile,
    name: "delete_file",
    description: "Elimina un archivo del sistema. OPERACIÓN SENSIBLE: Requiere confirmación del usuario antes de ejecutar."
);

var sendEmailFunction = AIFunctionFactory.Create(
    ApprovalWorkflow.SensitiveOperations.SendEmail,
    name: "send_email",
    description: "Envía un correo electrónico. OPERACIÓN SENSIBLE: Requiere confirmación del usuario antes de ejecutar."
);

var transferFundsFunction = AIFunctionFactory.Create(
    ApprovalWorkflow.SensitiveOperations.TransferFunds,
    name: "transfer_funds",
    description: "Transfiere dinero a otra cuenta bancaria. OPERACIÓN SENSIBLE: Requiere confirmación del usuario antes de ejecutar."
);

// Crear agente con todas las herramientas
var agent = new ChatCompletionAgent(
    chatClient: chatClient,
    name: "AsistenteSeguro",
    instructions: """
        Eres un asistente de productividad que ayuda a los usuarios con:
        - Gestión de archivos (listar, eliminar)
        - Comunicaciones (enviar correos)
        - Operaciones bancarias (consultar balance, transferir)
        
        IMPORTANTE - REGLAS DE SEGURIDAD:
        1. Para operaciones de SOLO LECTURA (listar archivos, consultar balance), 
           puedes ejecutarlas directamente.
        2. Para operaciones SENSIBLES (eliminar archivos, enviar correos, transferir dinero),
           el sistema solicitará confirmación del usuario.
        3. NUNCA intentes evadir el sistema de aprobación.
        4. Si el usuario cancela una operación, respeta su decisión.
        5. Siempre informa al usuario qué operación vas a realizar ANTES de ejecutarla.
        
        Sé amable, claro y transparente en todas tus interacciones.
        """,
    tools: new AIFunction[]
    {
        listFilesFunction,
        checkBalanceFunction,
        deleteFileFunction,
        sendEmailFunction,
        transferFundsFunction
    }
);

// Mostrar banner
Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
Console.WriteLine("║  🔐 Asistente con Aprobación Humana                       ║");
Console.WriteLine("║  Microsoft Agent Framework - Lab 03                       ║");
Console.WriteLine("╠═══════════════════════════════════════════════════════════╣");
Console.WriteLine("║  📁 Listar archivos    - Sin aprobación                   ║");
Console.WriteLine("║  💰 Consultar balance  - Sin aprobación                   ║");
Console.WriteLine("║  🗑️  Eliminar archivo  - ⚠️ Requiere aprobación           ║");
Console.WriteLine("║  📧 Enviar correo      - ⚠️ Requiere aprobación           ║");
Console.WriteLine("║  💸 Transferir dinero  - ⚠️ Requiere aprobación           ║");
Console.WriteLine("╠═══════════════════════════════════════════════════════════╣");
Console.WriteLine("║  Escribe 'salir' para terminar                            ║");
Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
Console.WriteLine();

// Loop de conversación
var history = new ChatHistory();

while (true)
{
    Console.Write("Tú: ");
    var userInput = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(userInput))
        continue;

    if (userInput.Equals("salir", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("\n👋 ¡Hasta luego! Gracias por usar el asistente.");
        break;
    }

    // Agregar mensaje del usuario al historial
    history.Add(new ChatMessage(ChatRole.User, userInput));

    // Obtener respuesta del agente
    Console.WriteLine();
    Console.Write("🤖 Asistente: ");

    var response = await agent.InvokeAsync(history);

    // Procesar respuesta
    foreach (var message in response.Messages)
    {
        history.Add(message);
        
        if (message.Role == ChatRole.Assistant && !string.IsNullOrEmpty(message.Text))
        {
            Console.WriteLine(message.Text);
        }
    }

    Console.WriteLine();
}
```

### 4.2 Flujo de Aprobación

```
┌─────────────────────────────────────────────────────────────┐
│                 FLUJO DE APROBACIÓN HUMANA                  │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Usuario: "Elimina el archivo /temporal/cache.tmp"         │
│                    │                                        │
│                    ▼                                        │
│  ┌─────────────────────────────────────┐                   │
│  │  Agente identifica operación        │                   │
│  │  sensible: delete_file              │                   │
│  └─────────────────────────────────────┘                   │
│                    │                                        │
│                    ▼                                        │
│  ╔═════════════════════════════════════╗                   │
│  ║  ⚠️ APROBACIÓN REQUERIDA            ║                   │
│  ║  Acción: ELIMINAR ARCHIVO           ║                   │
│  ║  ¿Aprobar? (s/n)                    ║                   │
│  ╚═════════════════════════════════════╝                   │
│                    │                                        │
│          ┌────────┴────────┐                               │
│          ▼                 ▼                               │
│    ┌─────────┐       ┌─────────┐                           │
│    │   "s"   │       │   "n"   │                           │
│    │ Aprobar │       │ Rechazar│                           │
│    └────┬────┘       └────┬────┘                           │
│         │                 │                                │
│         ▼                 ▼                                │
│  ✅ Archivo          🚫 Operación                          │
│     eliminado           cancelada                          │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Paso 5: Ejecutar y Probar

### 5.1 Compilar y Ejecutar

```bash
dotnet build
dotnet run
```

### 5.2 Escenarios de Prueba

**Escenario 1: Operación Segura (sin aprobación)**
```
Tú: ¿Qué archivos hay disponibles?
🤖 Asistente: 📁 Archivos en el sistema:
  📄 /documentos/informe.txt
  📄 /documentos/presupuesto.xlsx
  📄 /temporal/cache.tmp
  📄 /temporal/logs.txt
  📄 /importante/backup.zip
```

**Escenario 2: Operación Sensible - Aprobada**
```
Tú: Elimina el archivo /temporal/cache.tmp

╔═══════════════════════════════════════════════════════╗
║  ⚠️  APROBACIÓN REQUERIDA - OPERACIÓN SENSIBLE        ║
╠═══════════════════════════════════════════════════════╣
║  Acción: ELIMINAR ARCHIVO                             ║
║  Archivo: /temporal/cache.tmp                         ║
╠═══════════════════════════════════════════════════════╣
║  ¿Aprobar esta operación? (s/n):                      ║
╚═══════════════════════════════════════════════════════╝
>>> s

🤖 Asistente: ✅ El archivo /temporal/cache.tmp ha sido eliminado exitosamente.
```

**Escenario 3: Operación Sensible - Rechazada**
```
Tú: Transfiere $5000 a la cuenta 1234-5678 por concepto de "Pago servicios"

╔═══════════════════════════════════════════════════════╗
║  ⚠️  APROBACIÓN REQUERIDA - OPERACIÓN SENSIBLE        ║
╠═══════════════════════════════════════════════════════╣
║  Acción: TRANSFERENCIA BANCARIA                       ║
║  Cuenta destino: 1234-5678                            ║
║  Monto: $5000.00                                      ║
║  Concepto: Pago servicios                             ║
╠═══════════════════════════════════════════════════════╣
║  ¿Aprobar esta operación? (s/n):                      ║
╚═══════════════════════════════════════════════════════╝
>>> n

🤖 Asistente: 🚫 La transferencia fue cancelada por tu decisión.
```

**Escenario 4: Múltiples Operaciones**
```
Tú: Consulta mi balance y después envía un correo a jefe@empresa.com 
    con asunto "Reporte Mensual" diciendo "Adjunto el reporte del mes"
```

---

## Paso 6: Ejercicios Adicionales

### Ejercicio 1: Añadir Niveles de Aprobación
Modifica el sistema para tener diferentes niveles de aprobación según el riesgo:
- **Bajo**: Solo confirmación simple
- **Medio**: Confirmación + motivo
- **Alto**: Confirmación + motivo + código de seguridad

### Ejercicio 2: Registro de Auditoría
Añade un sistema de logging que registre:
- Todas las operaciones solicitadas
- Si fueron aprobadas o rechazadas
- Timestamp y usuario

### Ejercicio 3: Timeout de Aprobación
Implementa un timeout donde si el usuario no responde en X segundos, la operación se cancela automáticamente.

---

## Conceptos Clave Aprendidos

### 1. Patrón Human-in-the-Loop
El agente no actúa autónomamente en operaciones críticas; siempre hay supervisión humana.

### 2. Clasificación de Operaciones

| Tipo | Ejemplo | Aprobación |
|------|---------|------------|
| Lectura | Listar archivos, consultar balance | ❌ No requiere |
| Escritura segura | Crear archivo temporal | ⚠️ Opcional |
| Sensible | Eliminar, enviar correo, transferir | ✅ Siempre requiere |

### 3. AIFunctionFactory.Create
Usamos `AIFunctionFactory.Create` para registrar métodos estáticos como herramientas del agente:

```csharp
var deleteFileFunction = AIFunctionFactory.Create(
    SensitiveOperations.DeleteFile,
    name: "delete_file",
    description: "Elimina un archivo. OPERACIÓN SENSIBLE."
);
```

### 4. UX de Aprobación
La interfaz debe ser clara:
- Mostrar exactamente qué se va a hacer
- Dar opciones claras (s/n)
- Confirmar el resultado

---

## Resolución de Problemas

### El agente no solicita aprobación

**Causa**: El método de aprobación no está siendo ejecutado correctamente.

**Solución**: Verificar que la función esté registrada con `AIFunctionFactory.Create`:
```csharp
var deleteFileFunction = AIFunctionFactory.Create(
    SensitiveOperations.DeleteFile,
    name: "delete_file",
    description: "..."
);
```

### La aprobación no se muestra en consola

**Causa**: El output está siendo capturado o bufferizado.

**Solución**: Usar `Console.Out.Flush()` después de escribir o ejecutar sin redirección.

### Error de conexión

**Causa**: Credenciales incorrectas o endpoint inválido.

**Solución**: Verificar `appsettings.json` y user secrets:
```bash
dotnet user-secrets list
```

---

## Próximos Pasos

Continúa con el **Módulo 3: Workflows Multi-Agente** donde aprenderás:
- Patrones de workflow secuencial
- Ejecución paralela de agentes
- Delegación inteligente de tareas
- Group chat con múltiples agentes

---

## Resumen

En este laboratorio aprendiste:
- ✅ Implementar el patrón **Human-in-the-Loop**
- ✅ Clasificar operaciones por nivel de sensibilidad
- ✅ Crear un flujo de aprobación con `Console.ReadLine()`
- ✅ Usar `AIFunctionFactory.Create` para registrar herramientas
- ✅ Diseñar UX clara para solicitudes de aprobación
- ✅ Manejar tanto aprobaciones como rechazos

**¡Excelente!** Ahora tus agentes pueden realizar operaciones sensibles de forma segura, siempre con supervisión humana. 🔐

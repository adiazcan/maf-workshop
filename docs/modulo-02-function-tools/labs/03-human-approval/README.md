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
- Entender cómo funcionan las function tools

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
dotnet add package Microsoft.AI.Agents --version 1.0.0-preview.260108.1
dotnet add package Microsoft.AI.Agents.Abstractions --version 1.0.0-preview.260108.1
dotnet add package Azure.AI.OpenAI --version 2.0.0
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

## Paso 3: Implementar Operaciones Sensibles

### 3.1 Crear SensitiveOperations.cs

Este servicio contiene operaciones que **pausan para aprobación humana**:

```csharp
// ============================================================================
// Archivo: SensitiveOperations.cs
// Descripción: Operaciones sensibles que requieren aprobación humana
// Módulo: 2 - Function Tools
// Lab: 03-human-approval
// ============================================================================

using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace ApprovalWorkflow;

/// <summary>
/// Servicio con operaciones sensibles que requieren aprobación humana.
/// Demuestra el patrón Human-in-the-Loop donde ciertas acciones
/// no se ejecutan automáticamente sino que pausan para confirmación.
/// </summary>
public class SensitiveOperations
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

    /// <summary>
    /// Lista los archivos disponibles en el sistema virtual.
    /// Esta operación NO requiere aprobación (solo lectura).
    /// </summary>
    [KernelFunction("list_files")]
    [Description("Lista los archivos disponibles en el sistema. Úsala cuando el usuario quiera ver qué archivos existen.")]
    public string ListFiles()
    {
        var files = string.Join("\n", _virtualFileSystem.Keys.Select(f => $"  📄 {f}"));
        return $"📁 Archivos en el sistema:\n{files}";
    }

    /// <summary>
    /// Elimina un archivo del sistema virtual.
    /// ⚠️ REQUIERE APROBACIÓN HUMANA antes de ejecutarse.
    /// </summary>
    [KernelFunction("delete_file")]
    [Description("Elimina un archivo del sistema. OPERACIÓN SENSIBLE: Requiere confirmación del usuario antes de ejecutar.")]
    public string DeleteFile(
        [Description("Ruta completa del archivo a eliminar")] string filePath)
    {
        // ===== PASO 1: Validar que el archivo existe =====
        if (!_virtualFileSystem.ContainsKey(filePath))
        {
            return $"❌ Error: El archivo '{filePath}' no existe.";
        }

        // ===== PASO 2: SOLICITAR APROBACIÓN HUMANA =====
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
        
        var approval = Console.ReadLine()?.Trim().ToLower();
        
        // ===== PASO 3: Ejecutar o cancelar según la respuesta =====
        if (approval == "s" || approval == "si" || approval == "sí" || approval == "yes")
        {
            // Aprobado: ejecutar la operación
            _virtualFileSystem.Remove(filePath);
            Console.WriteLine("✅ Operación APROBADA y ejecutada.\n");
            return $"✅ Archivo '{filePath}' eliminado exitosamente.";
        }
        else
        {
            // Rechazado: cancelar la operación
            Console.WriteLine("🚫 Operación CANCELADA por el usuario.\n");
            return $"🚫 Operación cancelada: El archivo '{filePath}' NO fue eliminado.";
        }
    }

    /// <summary>
    /// Envía un correo electrónico.
    /// ⚠️ REQUIERE APROBACIÓN HUMANA antes de enviarse.
    /// </summary>
    [KernelFunction("send_email")]
    [Description("Envía un correo electrónico. OPERACIÓN SENSIBLE: Requiere confirmación antes de enviar.")]
    public string SendEmail(
        [Description("Dirección de correo del destinatario")] string recipient,
        [Description("Asunto del correo")] string subject,
        [Description("Contenido del mensaje")] string body)
    {
        // ===== SOLICITAR APROBACIÓN HUMANA =====
        Console.WriteLine();
        Console.WriteLine("╔═══════════════════════════════════════════════════════╗");
        Console.WriteLine("║  ⚠️  APROBACIÓN REQUERIDA - ENVÍO DE CORREO           ║");
        Console.WriteLine("╠═══════════════════════════════════════════════════════╣");
        Console.WriteLine($"║  Para: {recipient,-47} ║");
        Console.WriteLine($"║  Asunto: {subject,-45} ║");
        Console.WriteLine("║  Mensaje:                                             ║");
        
        // Mostrar mensaje truncado si es muy largo
        var truncatedBody = body.Length > 50 ? body.Substring(0, 47) + "..." : body;
        Console.WriteLine($"║    {truncatedBody,-49} ║");
        
        Console.WriteLine("╠═══════════════════════════════════════════════════════╣");
        Console.WriteLine("║  ¿Enviar este correo? (s/n):                          ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════╝");
        Console.Write(">>> ");
        
        var approval = Console.ReadLine()?.Trim().ToLower();
        
        if (approval == "s" || approval == "si" || approval == "sí" || approval == "yes")
        {
            // Simular envío de correo
            _pendingEmails.Add(new PendingEmail(recipient, subject, body, DateTime.Now));
            Console.WriteLine("✅ Correo ENVIADO.\n");
            return $"✅ Correo enviado exitosamente a {recipient}.";
        }
        else
        {
            Console.WriteLine("🚫 Envío CANCELADO.\n");
            return $"🚫 Envío cancelado: El correo a {recipient} NO fue enviado.";
        }
    }

    /// <summary>
    /// Realiza una transferencia de fondos (simulada).
    /// ⚠️ REQUIERE APROBACIÓN HUMANA antes de ejecutarse.
    /// </summary>
    [KernelFunction("transfer_funds")]
    [Description("Transfiere fondos a una cuenta. OPERACIÓN FINANCIERA SENSIBLE: Requiere aprobación obligatoria.")]
    public string TransferFunds(
        [Description("Cuenta de destino")] string destinationAccount,
        [Description("Monto a transferir en euros")] decimal amount,
        [Description("Concepto de la transferencia")] string concept)
    {
        // ===== SOLICITAR APROBACIÓN HUMANA =====
        Console.WriteLine();
        Console.WriteLine("╔═══════════════════════════════════════════════════════╗");
        Console.WriteLine("║  💰 APROBACIÓN REQUERIDA - TRANSFERENCIA FINANCIERA   ║");
        Console.WriteLine("╠═══════════════════════════════════════════════════════╣");
        Console.WriteLine($"║  Cuenta destino: {destinationAccount,-37} ║");
        Console.WriteLine($"║  Monto: {amount:C2,-46} ║");
        Console.WriteLine($"║  Concepto: {concept,-43} ║");
        Console.WriteLine("╠═══════════════════════════════════════════════════════╣");
        Console.WriteLine("║  ⚠️  Esta acción NO puede deshacerse.                  ║");
        Console.WriteLine("║  ¿Confirmar transferencia? (s/n):                     ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════╝");
        Console.Write(">>> ");
        
        var approval = Console.ReadLine()?.Trim().ToLower();
        
        if (approval == "s" || approval == "si" || approval == "sí" || approval == "yes")
        {
            Console.WriteLine("✅ Transferencia APROBADA y procesada.\n");
            return $"✅ Transferencia de {amount:C2} a {destinationAccount} completada. Concepto: {concept}";
        }
        else
        {
            Console.WriteLine("🚫 Transferencia CANCELADA.\n");
            return $"🚫 Transferencia cancelada: No se transfirieron fondos a {destinationAccount}.";
        }
    }

    /// <summary>
    /// Consulta el saldo actual (operación de lectura, NO requiere aprobación).
    /// </summary>
    [KernelFunction("check_balance")]
    [Description("Consulta el saldo disponible en la cuenta. Operación de solo lectura.")]
    public string CheckBalance()
    {
        // Simular saldo
        return "💳 Saldo disponible: €2,450.75";
    }
}

/// <summary>
/// Estructura para almacenar correos pendientes/enviados
/// </summary>
internal record PendingEmail(
    string Recipient,
    string Subject,
    string Body,
    DateTime SentAt
);
```

### 3.2 El Patrón de Aprobación

La estructura clave para implementar aprobación humana:

```csharp
public string SensitiveOperation(string param)
{
    // 1. Validaciones previas (opcional)
    if (!IsValid(param)) return "Error: ...";
    
    // 2. MOSTRAR INFORMACIÓN y pedir aprobación
    Console.WriteLine("⚠️ APROBACIÓN REQUERIDA");
    Console.WriteLine($"Acción: {descripcion}");
    Console.WriteLine("¿Aprobar? (s/n): ");
    
    // 3. LEER respuesta del usuario
    var approval = Console.ReadLine()?.ToLower();
    
    // 4. Ejecutar o cancelar
    if (approval == "s" || approval == "si")
    {
        // EJECUTAR la operación
        return "✅ Operación completada";
    }
    else
    {
        // NO ejecutar
        return "🚫 Operación cancelada";
    }
}
```

---

## Paso 4: Implementar el Agente

### 4.1 Reemplazar Program.cs

```csharp
// ============================================================================
// Archivo: Program.cs
// Descripción: Demostración de Human-in-the-Loop para operaciones sensibles
// Módulo: 2 - Function Tools
// Lab: 03-human-approval
// ============================================================================

using Microsoft.AI.Agents;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using ApprovalWorkflow;

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

// ===== Crear Kernel =====
var builder = Kernel.CreateBuilder();

builder.AddAzureOpenAIChatCompletion(
    deploymentName: deploymentName,
    endpoint: endpoint,
    apiKey: apiKey
);

// Registrar las operaciones sensibles como plugin
builder.Plugins.AddFromType<SensitiveOperations>();

var kernel = builder.Build();

// ===== Crear Agente =====
var agent = new ChatCompletionAgent()
{
    Name = "AsistenteSeguro",
    Instructions = """
        Eres un asistente administrativo llamado AsistenteSeguro.
        Tienes acceso a operaciones del sistema que pueden ser sensibles.
        
        OPERACIONES DISPONIBLES:
        1. list_files - Ver archivos (sin aprobación)
        2. delete_file - Eliminar archivos (REQUIERE APROBACIÓN)
        3. check_balance - Ver saldo (sin aprobación)
        4. transfer_funds - Transferir dinero (REQUIERE APROBACIÓN)
        5. send_email - Enviar correos (REQUIERE APROBACIÓN)
        
        REGLAS DE SEGURIDAD:
        - Antes de operaciones sensibles, informa al usuario que se pedirá confirmación
        - Si el usuario cancela, respeta su decisión y confirma la cancelación
        - Nunca intentes evadir las aprobaciones
        
        Responde siempre en español de forma profesional.
        """,
    Kernel = kernel,
    Arguments = new KernelArguments(
        new AzureOpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        }
    )
};

// ===== Historial =====
var chatHistory = new ChatHistory();

// ===== Interfaz =====
Console.WriteLine("╔═══════════════════════════════════════════════════════╗");
Console.WriteLine("║  🔒 Workflow con Aprobación Humana (Human-in-the-Loop)║");
Console.WriteLine("╠═══════════════════════════════════════════════════════╣");
Console.WriteLine("║  Agente: AsistenteSeguro                              ║");
Console.WriteLine("║                                                       ║");
Console.WriteLine("║  Operaciones de SOLO LECTURA (sin aprobación):        ║");
Console.WriteLine("║    • Listar archivos                                  ║");
Console.WriteLine("║    • Ver saldo                                        ║");
Console.WriteLine("║                                                       ║");
Console.WriteLine("║  Operaciones SENSIBLES (requieren aprobación):        ║");
Console.WriteLine("║    • Eliminar archivos                                ║");
Console.WriteLine("║    • Enviar correos                                   ║");
Console.WriteLine("║    • Transferir fondos                                ║");
Console.WriteLine("╠═══════════════════════════════════════════════════════╣");
Console.WriteLine("║  Escribe 'salir' para terminar                        ║");
Console.WriteLine("╚═══════════════════════════════════════════════════════╝");
Console.WriteLine();

Console.WriteLine("💡 Prueba estas acciones:");
Console.WriteLine("   📋 'Lista los archivos del sistema'");
Console.WriteLine("   🗑️  'Elimina el archivo /temporal/cache.tmp'");
Console.WriteLine("   💳 'Muestra mi saldo'");
Console.WriteLine("   💸 'Transfiere 100 euros a ES1234567890'");
Console.WriteLine("   📧 'Envía un correo a juan@empresa.com'");
Console.WriteLine();

// ===== Bucle de Conversación =====
while (true)
{
    Console.Write("👤 Tú: ");
    var userInput = Console.ReadLine();
    
    if (string.IsNullOrWhiteSpace(userInput) || 
        userInput.Equals("salir", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("\n🔒 AsistenteSeguro: ¡Hasta pronto! Tus operaciones están protegidas. 🛡️\n");
        break;
    }
    
    chatHistory.AddUserMessage(userInput);
    
    Console.Write("🔒 AsistenteSeguro: ");
    
    try
    {
        await foreach (var message in agent.InvokeStreamingAsync(chatHistory))
        {
            Console.Write(message.Content);
        }
        Console.WriteLine("\n");
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"\n❌ Error de conexión: {ex.Message}\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n❌ Error: {ex.Message}\n");
    }
    
    // Gestión de historial
    if (chatHistory.Count > 12)
    {
        var messagesToKeep = chatHistory.Skip(chatHistory.Count - 12).ToList();
        chatHistory.Clear();
        foreach (var msg in messagesToKeep)
        {
            chatHistory.Add(msg);
        }
    }
}
```

---

## Paso 5: Ejecución

### 5.1 Compilar y Ejecutar

```bash
dotnet build
dotnet run
```

### 5.2 Probar Operaciones de Solo Lectura

**Lista de archivos (sin aprobación)**:
```
👤 Tú: Lista los archivos del sistema
🔒 AsistenteSeguro: Aquí están los archivos disponibles:

📁 Archivos en el sistema:
  📄 /documentos/informe.txt
  📄 /documentos/presupuesto.xlsx
  📄 /temporal/cache.tmp
  📄 /temporal/logs.txt
  📄 /importante/backup.zip
```

**Consulta de saldo (sin aprobación)**:
```
👤 Tú: Muéstrame mi saldo
🔒 AsistenteSeguro: Tu saldo disponible es de €2,450.75
```

### 5.3 Probar Operaciones Sensibles con APROBACIÓN

**Eliminar archivo - APROBAR**:
```
👤 Tú: Elimina el archivo /temporal/cache.tmp
🔒 AsistenteSeguro: Voy a proceder a eliminar ese archivo. Se te pedirá confirmación.

╔═══════════════════════════════════════════════════════╗
║  ⚠️  APROBACIÓN REQUERIDA - OPERACIÓN SENSIBLE        ║
╠═══════════════════════════════════════════════════════╣
║  Acción: ELIMINAR ARCHIVO                             ║
║  Archivo: /temporal/cache.tmp                         ║
╠═══════════════════════════════════════════════════════╣
║  ¿Aprobar esta operación? (s/n):                      ║
╚═══════════════════════════════════════════════════════╝
>>> s
✅ Operación APROBADA y ejecutada.

🔒 AsistenteSeguro: El archivo /temporal/cache.tmp ha sido eliminado exitosamente.
```

### 5.4 Probar Operaciones Sensibles con RECHAZO

**Eliminar archivo - RECHAZAR**:
```
👤 Tú: Elimina el archivo /importante/backup.zip
🔒 AsistenteSeguro: Procederé a eliminar ese archivo, pero necesitaré tu confirmación.

╔═══════════════════════════════════════════════════════╗
║  ⚠️  APROBACIÓN REQUERIDA - OPERACIÓN SENSIBLE        ║
╠═══════════════════════════════════════════════════════╣
║  Acción: ELIMINAR ARCHIVO                             ║
║  Archivo: /importante/backup.zip                      ║
╠═══════════════════════════════════════════════════════╣
║  ¿Aprobar esta operación? (s/n):                      ║
╚═══════════════════════════════════════════════════════╝
>>> n
🚫 Operación CANCELADA por el usuario.

🔒 AsistenteSeguro: Entendido. El archivo /importante/backup.zip NO ha sido eliminado.
```

### 5.5 Probar Transferencia

```
👤 Tú: Transfiere 150 euros a ES9876543210 para pago de servicios

╔═══════════════════════════════════════════════════════╗
║  💰 APROBACIÓN REQUERIDA - TRANSFERENCIA FINANCIERA   ║
╠═══════════════════════════════════════════════════════╣
║  Cuenta destino: ES9876543210                         ║
║  Monto: €150.00                                       ║
║  Concepto: pago de servicios                          ║
╠═══════════════════════════════════════════════════════╣
║  ⚠️  Esta acción NO puede deshacerse.                  ║
║  ¿Confirmar transferencia? (s/n):                     ║
╚═══════════════════════════════════════════════════════╝
>>> s
✅ Transferencia APROBADA y procesada.
```

---

## Paso 6: Validación

### ✅ Checkpoint: Verificación de Human-in-the-Loop

- [ ] ✅ Operaciones de lectura (list_files, check_balance) NO piden aprobación
- [ ] ✅ Operaciones sensibles (delete_file, send_email, transfer_funds) SIEMPRE piden aprobación
- [ ] ✅ Al escribir 's' o 'si', la operación se ejecuta
- [ ] ✅ Al escribir 'n' o cualquier otra cosa, la operación se cancela
- [ ] ✅ El agente confirma tanto la ejecución como la cancelación

**Prueba de validación definitiva**:
```
👤 Tú: Elimina todos los archivos temporales
```

El sistema debe:
1. Intentar eliminar cada archivo temporal
2. Pedir aprobación individual para cada uno
3. El usuario puede aprobar unos y rechazar otros

---

## Experimentación (Opcional)

### Experimento 1: Agregar Niveles de Aprobación

Implementa aprobación de dos pasos para operaciones críticas:

```csharp
[KernelFunction("critical_operation")]
public string CriticalOperation(...)
{
    // Primera aprobación
    Console.WriteLine("¿Aprobar operación? (s/n):");
    var firstApproval = Console.ReadLine();
    if (firstApproval != "s") return "Cancelado";
    
    // Segunda aprobación (confirmar)
    Console.WriteLine("⚠️ Escriba 'CONFIRMAR' para proceder:");
    var confirm = Console.ReadLine();
    if (confirm != "CONFIRMAR") return "Cancelado";
    
    // Ejecutar
    return "Operación ejecutada";
}
```

### Experimento 2: Logging de Aprobaciones

Agrega registro de todas las aprobaciones/rechazos:

```csharp
private static readonly List<AuditLog> _auditLog = new();

// En cada función sensible:
_auditLog.Add(new AuditLog(
    Operation: "delete_file",
    Parameters: filePath,
    Approved: approval == "s",
    Timestamp: DateTime.Now
));

// Función para ver el log
[KernelFunction("show_audit_log")]
public string ShowAuditLog() => ...
```

---

## Solución de Problemas

### La aprobación no aparece

**Síntoma**: El agente ejecuta operaciones sin pedir aprobación.

**Causa**: La función puede no estar registrada o el agente usa una versión diferente.

**Solución**: 
1. Verifica que `builder.Plugins.AddFromType<SensitiveOperations>();` está presente
2. Asegúrate de que la función tiene `[KernelFunction]`

---

### El agente no respeta la cancelación

**Síntoma**: El agente dice que canceló pero la operación se ejecutó.

**Causa**: El return después de la cancelación no está correcto.

**Solución**: Verifica que el `else` del if de aprobación retorna inmediatamente sin ejecutar la operación.

---

### La interfaz se corrompe

**Síntoma**: Los caracteres del cuadro no se muestran correctamente.

**Causa**: La terminal no soporta caracteres Unicode.

**Solución**: Cambia los caracteres del cuadro por ASCII simple:
```csharp
Console.WriteLine("+-----------------------------------+");
Console.WriteLine("|  APROBACIÓN REQUERIDA             |");
```

---

## Resumen

En este laboratorio aprendiste:

✅ **Implementar Human-in-the-Loop** para operaciones sensibles  
✅ **Pausar ejecución** dentro de una function tool  
✅ **Manejar aprobación y rechazo** de forma diferenciada  
✅ **Clasificar operaciones** por nivel de sensibilidad  
✅ **Informar al usuario** antes y después de cada decisión

### Conceptos Clave

| Concepto | Descripción |
|----------|-------------|
| **Human-in-the-Loop** | Patrón donde humanos aprueban acciones del agente |
| **Operación Sensible** | Acción que no debe ejecutarse sin supervisión |
| **Gate de Aprobación** | Punto de pausa que espera confirmación |
| **Audit Trail** | Registro de aprobaciones/rechazos para auditoría |

### Cuándo Usar Human-in-the-Loop

| ✅ Usar | ❌ No Usar |
|---------|-----------|
| Eliminar datos | Consultar información |
| Enviar comunicaciones | Buscar archivos |
| Transacciones financieras | Calcular valores |
| Cambios de configuración | Generar reportes |
| Acciones irreversibles | Operaciones de lectura |

---

## Próximos Pasos

Has completado el **Módulo 2: Function Tools**. Continúa con:

- **[Módulo 3: Workflows](../../modulo-03-workflows/)** - Orquestación de múltiples agentes
- **Revisión**: Practica los 3 labs de este módulo hasta dominar los conceptos

---

## Referencias

- [Human-in-the-Loop AI Patterns](https://learn.microsoft.com/azure/ai-services/openai/concepts/human-in-the-loop)
- [Responsible AI Guidelines](https://learn.microsoft.com/azure/ai-services/responsible-use-of-ai-overview)
- [Function Tools Best Practices](https://learn.microsoft.com/semantic-kernel/agents/plugins/best-practices)

---

**Tiempo completado**: ~20 minutos  
**¡Felicitaciones!** 🎉 Has implementado un sistema seguro con aprobación humana.

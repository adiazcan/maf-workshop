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

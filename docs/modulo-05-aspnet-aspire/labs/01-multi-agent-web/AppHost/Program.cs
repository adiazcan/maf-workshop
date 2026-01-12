// =============================================================================
// AppHost - Orquestador de .NET Aspire
// =============================================================================
// Este archivo configura .NET Aspire para orquestar todos los servicios
// de la aplicación multi-agente. Aspire proporciona:
// - Inicio coordinado de servicios
// - Dashboard unificado para logs, traces y métricas
// - Service discovery automático
// - Configuración centralizada
// =============================================================================

// Crear el builder de la aplicación distribuida
var builder = DistributedApplication.CreateBuilder(args);

// -----------------------------------------------------------------------------
// Configuración de Referencias de Conexión
// -----------------------------------------------------------------------------
// AddConnectionString permite compartir configuración entre servicios.
// En producción, esto se conectaría a Azure Key Vault o App Configuration.
// Para desarrollo, se usa la configuración local (appsettings.json o user-secrets).

var openai = builder.AddConnectionString("openai");

// -----------------------------------------------------------------------------
// Agregar Proyecto WebApi
// -----------------------------------------------------------------------------
// AddProject<T> registra un proyecto de .NET para ser orquestado por Aspire.
// - WithReference(openai): Inyecta la cadena de conexión de Azure OpenAI
// - WithExternalHttpEndpoints(): Expone el endpoint HTTP al exterior del dashboard

var api = builder.AddProject<Projects.WebApi>("webapi")
    .WithReference(openai)           // Inyectar configuración de OpenAI
    .WithExternalHttpEndpoints();    // Exponer al exterior para pruebas

// -----------------------------------------------------------------------------
// Construcción y Ejecución
// -----------------------------------------------------------------------------
// Build().Run() inicia:
// 1. El dashboard de Aspire (por defecto en http://localhost:15888)
// 2. Todos los proyectos registrados
// 3. Recolección de telemetría (logs, traces, métricas)

builder.Build().Run();

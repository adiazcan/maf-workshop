# Workshop Content Format Standards

**Version**: 1.0  
**Date**: 2026-01-12

## Overview

This document defines formatting standards and conventions for all Microsoft Agent Framework workshop content.

---

## 1. Markdown Documentation Standards

### 1.1 File Naming

**Theory Documentation**:
- Main module theory: `README.md` (always in Spanish)
- Installation guide: `instalacion.md`
- Troubleshooting: `solucion-problemas.md`

**Lab Documentation**:
- Lab folder pattern: `labs/{lab-id}/`
- Lab instructions: `labs/{lab-id}/README.md`
- Example: `labs/01-hello-agent/README.md`

### 1.2 Markdown Structure

All theory and lab markdown documents must follow this structure:

```markdown
# [Title in Spanish]

**Duración**: [X minutos]  
**Módulo**: [Module number and name]  
**Prerequisitos**: [List of prerequisites]

## Objetivo

[Single clear learning objective in 1-2 sentences]

## Conceptos Clave

[2-4 key concepts with brief explanations]

## [Main Content Sections]

[Content organized with H2 headers]

### [Subsections]

[Use H3 for subsections]

## Resumen

[Brief summary of what was learned]

## Próximos Pasos

[What to do next / which lab follows]

## Referencias

- [External links to Microsoft Learn, docs, etc.]
```

### 1.3 Spanish Language Guidelines

**Required**:
- All headings in Spanish
- All explanatory text in Spanish
- All code comments in Spanish
- All error messages explained in Spanish

**Allowed in English**:
- Code identifiers (variable/function names)
- Technical terms without good Spanish translation (e.g., "kernel", "prompt")
- URLs and external references
- NuGet package names
- CLI commands and output

**Examples**:

✅ Good:
```markdown
## Configuración del Agente

Microsoft Agent Framework utiliza inyección de dependencias estándar de .NET.

```csharp
// Crear el kernel con Azure OpenAI
var builder = Kernel.CreateBuilder();
```

❌ Bad:
```markdown
## Kernel Setup

The kernel is the central dependency injection container.

```csharp
// Create kernel with Azure OpenAI
var builder = Kernel.CreateBuilder();
```

---

## 2. C# Code Standards

### 2.1 File Structure

Every C# project must include:

```
ProjectName/
├── ProjectName.csproj
├── Program.cs
├── appsettings.json
├── README.md (Spanish lab instructions)
└── [Other .cs files as needed]
```

### 2.2 Code Comment Standards

**Required Comments**:

1. **File header** (all .cs files):
```csharp
// ============================================================================
// Archivo: Program.cs
// Descripción: [Brief Spanish description]
// Módulo: [Module number]
// Lab: [Lab ID]
// ============================================================================
```

2. **Section comments** (major code sections):
```csharp
// ===== Configuración del Kernel =====
var builder = Kernel.CreateBuilder();
```

3. **Inline explanations** (non-obvious code):
```csharp
// Truncar historial a últimos 10 mensajes para evitar límite de tokens
if (chatHistory.Count > 10)
{
    chatHistory.RemoveRange(0, chatHistory.Count - 10);
}
```

4. **Function documentation**:
```csharp
/// <summary>
/// Obtiene el clima actual para una ciudad específica.
/// </summary>
/// <param name="city">Nombre de la ciudad</param>
/// <param name="country">Código de país (ISO 3166-1 alpha-2)</param>
/// <returns>Descripción del clima en español</returns>
[KernelFunction("get_weather")]
[Description("Obtiene el clima actual para una ubicación")]
public string GetWeather(
    [Description("Nombre de la ciudad")] string city,
    [Description("Código de país")] string country = "ES")
{
    // Implementación
}
```

### 2.3 Code Complexity Guidelines

**Simple Labs** (10-15 min):
- ≤ 100 lines of code
- Single file (Program.cs)
- ≤ 3 NuGet packages
- No configuration files beyond appsettings.json

**Standard Labs** (20-25 min):
- 100-200 lines of code
- 2-3 files
- ≤ 5 NuGet packages
- May include helper classes

**Complex Labs** (30-40 min):
- 200-500 lines of code
- 4-6 files
- ≤ 8 NuGet packages
- Multiple configuration files

**Capstone Labs** (45-60 min):
- 500+ lines of code
- Multiple projects (e.g., API + services)
- Any number of packages
- Full application structure

### 2.4 Naming Conventions

Follow standard C# conventions with Spanish comments:

```csharp
// ✅ Good: English identifiers, Spanish comments
var weatherAgent = new ChatCompletionAgent();  // Agente para consultas del clima

// ❌ Bad: Mixed naming
var agenteDelClima = new ChatCompletionAgent();  // Too Spanish
var wa = new ChatCompletionAgent();  // Too abbreviated
```

**Class Names**: PascalCase (e.g., `WeatherAgent`, `ChatService`)  
**Method Names**: PascalCase (e.g., `GetWeather`, `ProcessMessage`)  
**Variables**: camelCase (e.g., `chatHistory`, `maxTokens`)  
**Constants**: PascalCase (e.g., `MaxRetries`, `DefaultTimeout`)

### 2.5 .csproj Standard

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
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

</Project>
```

### 2.6 appsettings.json Standard

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://YOUR-RESOURCE-NAME.openai.azure.com/",
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

**Note**: API keys stored in user secrets, never in appsettings.json:
```bash
dotnet user-secrets set "AzureOpenAI:ApiKey" "your-key-here"
```

---

## 3. Lab Instructions Format

Every lab README.md must follow this exact structure:

```markdown
# Lab [X.Y]: [Title in Spanish]

**Duración**: [15-60] minutos  
**Complejidad**: [simple/standard/complex/capstone]  
**Objetivo**: [Single clear goal]

## Prerequisitos

### Software Requerido
- .NET 10 SDK
- Visual Studio Code
- [Other software]

### Conocimientos Previos
- [Previous labs or concepts]

### Configuración de Azure
- Azure OpenAI Service con deployment de gpt-5.2
- [Other Azure resources]

## Paso 1: Preparación del Proyecto

### 1.1 Crear Proyecto

```bash
# Commands to create project
dotnet new console -n ProjectName
cd ProjectName
```

### 1.2 Instalar Paquetes

```bash
# NuGet package installation commands
dotnet add package Microsoft.AI.Agents --version 1.0.0-preview.260108.1
```

## Paso 2: Configuración

### 2.1 Archivo appsettings.json

Crear `appsettings.json`:

```json
{
  // Configuration content
}
```

### 2.2 Configurar Secretos

```bash
dotnet user-secrets init
dotnet user-secrets set "AzureOpenAI:ApiKey" "your-key-here"
```

## Paso 3: Implementación

### 3.1 [Component Name]

Crear archivo `FileName.cs`:

```csharp
// Complete copy-paste ready code
// With Spanish comments
```

**Explicación del Código**:
- [Line-by-line explanation of key concepts]

## Paso 4: Ejecución

```bash
dotnet run
```

**Salida Esperada**:

```
Expected console output here
```

## Paso 5: Validación

✅ **Checkpoint**: [Clear validation criteria]

Verifica que:
- [ ] El programa se ejecuta sin errores
- [ ] La salida muestra [expected behavior]
- [ ] [Other validation points]

## Paso 6: Experimentación (Opcional)

Para participantes que terminan temprano:

1. **Tarea 1**: [Modification suggestion]
2. **Tarea 2**: [Another experiment]

## Solución de Problemas

### Error: [Common Error Message]

**Síntoma**: [What participant sees]

**Causa**: [Why it happens]

**Solución**:
1. [Step-by-step fix]

### Error: [Another Common Error]

[Same structure]

## Resumen

En este laboratorio aprendiste:
- [Key learning point 1]
- [Key learning point 2]
- [Key learning point 3]

## Próximos Pasos

Continúa con [Next Lab Name] donde aprenderás [brief description].

## Referencias

- [Microsoft Learn link]
- [GitHub samples link]
```

---

## 4. Instructor Guide Format

### 4.1 Module Instructor Page

Each module should have `instructor-guide.md`:

```markdown
# Guía del Instructor: Módulo [X]

## Tiempo Total: [X minutos]

## Estructura de la Sesión

| Actividad | Duración | Formato |
|-----------|----------|---------|
| Introducción teórica | 15 min | Presentación |
| Lab 1 | 20 min | Hands-on |
| Checkpoint 1 | 5 min | Validación manual |
| Lab 2 | 25 min | Hands-on |
| Checkpoint 2 | 5 min | Validación manual |

## Checkpoint de Validación

### Checkpoint 1: [Name]

**Criterio de Éxito**: [What to verify]

**Método de Validación**:
1. Pedir que levanten la mano quienes vean [expected output]
2. Verificar 2-3 pantallas compartidas
3. Resolver problemas comunes

**Meta**: 85% de éxito

**Tiempo Máximo para Checkpoint**: 5 minutos

### [Additional checkpoints]

## Problemas Comunes Anticipados

| Problema | Señales | Solución Rápida |
|----------|---------|-----------------|
| [Issue] | [How to spot] | [Quick fix] |

## Ajustes de Ritmo

**Si el grupo va adelantado** (termina con >15 min de margen):
- Agregar tareas experimentales del Lab
- Discusión de casos de uso reales
- Preview del siguiente módulo

**Si el grupo va retrasado** (quedaron <10 min):
- Demostrar el Lab completo en proyector
- Distribuir código completo para que copien
- Acortar tiempo de experimentación

## Notas de Presentación

- [Key talking points]
- [Common questions and answers]
- [Real-world examples to mention]
```

---

## 5. Lab Validation

### 5.1 Lab Validation Checklist

Before publishing any lab, manually verify:

- [ ] Code compiles without errors on .NET 10
- [ ] Code runs successfully on Windows 11
- [ ] Code runs successfully on macOS (latest)
- [ ] Code runs successfully on Ubuntu 22.04
- [ ] All comments are in Spanish
- [ ] All instructions are in Spanish
- [ ] appsettings.json does not contain secrets
- [ ] User secrets instructions are clear
- [ ] README.md follows standard structure
- [ ] Estimated time is realistic (tested with timer)
- [ ] Expected output matches actual output
- [ ] Troubleshooting covers at least 3 common errors
- [ ] NuGet packages use exact versions (not floating)

### 5.2 Module Completeness Checklist

Before marking module as complete:

- [ ] Theory documentation (README.md) exists
- [ ] All labs have complete solutions
- [ ] Instructor guide exists
- [ ] Timing estimates sum to module duration
- [ ] Checkpoint criteria are measurable
- [ ] All prerequisites are documented
- [ ] External links are valid
- [ ] No placeholders remain ([TODO], [TBD], etc.)

---

## 6. Version Control Standards

### 6.1 Commit Message Format

```
[MODULE-XX] Brief description in English

Detailed explanation if needed.

- Change 1
- Change 2
```

**Examples**:
```
[MODULE-01] Add Hello Agent lab with Spanish comments

[MODULE-03] Fix workflow timing estimates

[ALL] Update NuGet packages to Microsoft Agent Framework 1.0.0-preview.260108.1
```

### 6.2 Branch Naming

- Feature branches: `001-maf-workshop`
- Lab-specific: `001-maf-workshop-lab-02-01`

---

## 7. Accessibility Standards

### 7.1 Markdown Accessibility

- Use proper heading hierarchy (H1 → H2 → H3)
- Provide alt text for all images: `![Descripción del diagrama](image.png)`
- Use tables for tabular data (not ASCII art)
- Code blocks always specify language: ` ```csharp `

### 7.2 Code Readability

- Line length ≤ 120 characters
- Indentation: 4 spaces (C# standard)
- Blank lines between logical sections
- Avoid deep nesting (max 3 levels)

---

## 8. Maintenance Guidelines

### 8.1 Dependency Updates

When Microsoft Agent Framework or Azure OpenAI SDKs update:

1. Update `research.md` with new version notes
2. Update all `.csproj` files with new versions
3. Test all labs with new versions
4. Update troubleshooting if new errors appear
5. Commit with message: `[ALL] Update dependencies to MAF X.Y.Z`

### 8.2 Content Refresh Triggers

Review and update content when:
- Microsoft Agent Framework has breaking changes
- Azure OpenAI model pricing changes significantly
- New MAF features make labs obsolete
- Participant feedback indicates confusion
- Workshop success rates drop below target

---

**Document Version**: 1.0  
**Effective Date**: 2026-01-12  
**Next Review**: 2026-04-12

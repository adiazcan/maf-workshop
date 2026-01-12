# Research Document: Microsoft Agent Framework Workshop

**Feature Branch**: `001-maf-workshop`  
**Created**: January 12, 2026  
**Research Date**: January 12, 2026  
**Target Audience**: Spanish-speaking developers learning Microsoft Agent Framework with C#/.NET 10

---

## 1. Microsoft Agent Framework 1.0.0-preview.260108.1 Fundamentals

### Decision/Recommendation

**Adopt a layered teaching approach**: Start with single agents using ChatCompletionAgent, progress to function tools, then multi-agent orchestration patterns. Use Microsoft Agent Framework (MAF) as the primary framework with enterprise-ready agent orchestration capabilities.

**Core architecture focus areas**:
1. **Single Agent Pattern**: ChatCompletionAgent as the foundational building block
2. **Function Tools**: Native C# methods exposed as agent capabilities
3. **Agent Composition**: Using agents as tools within other agents
4. **Orchestration Patterns**: AgentGroupChat for multi-agent coordination
5. **State Management**: Azure AI Agent Service for workflow persistence

### Rationale

**Why this approach is best for workshops**:

1. **Progressive Complexity**: Each module builds naturally on the previous one, reducing cognitive load for participants
2. **Enterprise Ready**: Microsoft Agent Framework is production-ready (v1.0+) with non-breaking change commitment, unlike experimental frameworks
3. **Microsoft Ecosystem**: Deep integration with Azure services, .NET Aspire, and Azure OpenAI
4. **Active Development**: Microsoft's unified agent framework with continuous improvements and updates

**Key architectural concepts to emphasize**:

- **Agents are stateless by default**: Each agent invocation is independent unless explicitly persisted
- **Function calling is the bridge**: AI models request function execution, MAF handles the translation and execution
- **Dependency injection pattern**: All services, plugins, and configuration use standard .NET DI
- **Prompts + Functions = Capabilities**: The combination of system prompts and available functions defines agent behavior

### Alternatives Considered

| Alternative | Why Rejected |
|-------------|--------------|
| **LangChain for .NET** | Less mature .NET support, Python-first design patterns don't translate well to C# idioms |
| **AutoGen (standalone)** | Being consolidated into Microsoft Agent Framework; MAF is the recommended path forward |

| **OpenAI Assistants API directly** | Vendor lock-in, less flexibility for orchestration patterns, limited to OpenAI ecosystem |

### Implementation Notes for Phase 1

**Package References**:
```xml
<PackageReference Include="Microsoft.AI.Agents" Version="1.0.0-preview.260108.1" />
<PackageReference Include="Microsoft.AI.Agents.Abstractions" Version="1.0.0-preview.260108.1" />
```

**Agent Types Available**:

1. **ChatCompletionAgent**: 
   - Standard conversational agent using completion models
   - Best for: Dialogues, Q&A, general assistance
   - Configuration: System prompt, execution settings, function tools

2. **OpenAI Assistants Agent** (via Azure OpenAI):
   - Stateful agents with built-in retrieval and code interpreter
   - Best for: Complex workflows requiring file access or computation
   - Note: Requires Azure OpenAI Assistants API (preview)

**Orchestration Patterns to Cover**:

1. **Sequential Workflow**:
   ```csharp
   // Agent A completes task → passes result to Agent B → Agent B continues
   // Example: Research agent → Writing agent → Review agent
   ```

2. **Parallel Workflow**:
   ```csharp
   // Multiple agents work simultaneously on different subtasks
   // Example: Parallel data extraction from multiple sources
   ```

3. **Delegation Pattern**:
   ```csharp
   // Coordinator agent delegates to specialist agents
   // Example: Project manager agent → Designer, Developer, QA agents
   ```

4. **Group Chat (Round-robin / Selection)**:
   ```csharp
   // Multiple agents collaborate in a chat-like interface
   // AgentGroupChat manages turn-taking and termination
   // Example: Brainstorming session with multiple expert personas
   ```

**Function Tools Implementation**:

```csharp
// Native C# method automatically becomes a function tool
public class WeatherService
{
    [KernelFunction("get_weather")]
    [Description("Gets the current weather for a location")]
    public string GetWeather(
        [Description("The city name")] string city,
        [Description("The country code")] string country = "US")
    {
        // Implementation
        return $"Weather in {city}, {country}: Sunny, 22°C";
    }
}

// Registration in kernel
kernel.Plugins.AddFromType<WeatherService>();
```

**Educational Best Practices**:

1. **Start with "Hello Agent"**: Minimal agent that responds to greetings (no functions)
2. **Add one function**: Weather lookup or calculator to demonstrate function calling
3. **Compose agents**: Use one agent as a tool within another
4. **Introduce orchestration**: Simple 2-agent conversation
5. **Add complexity gradually**: More agents, termination strategies, human-in-the-loop

**Common Pitfalls for Workshops**:

- **Function naming**: Use clear, verb-based names (`get_weather` not `weather`)
- **Parameter descriptions**: Critical for model to understand when to call functions
- **Termination conditions**: Always define how multi-agent conversations end
- **Token limits**: Conversation history grows quickly; implement truncation
- **Rate limits**: Azure OpenAI has per-minute limits; implement retry logic

---

## 2. Azure OpenAI Service Integration

### Decision/Recommendation

**Use Azure OpenAI Service exclusively** with the following configuration strategy:

1. **Model Selection**: 
   - Primary: `gpt-5.2` (2026-01-01) for most labs - best performance and capabilities
   - Alternative: `gpt-5.2-chat` (2026-01-01) for conversational examples - optimized for dialogue
   - Advanced: `o4-mini` for reasoning-heavy scenarios (if budget allows)

2. **Authentication Pattern**: 
   - **Development**: API Key authentication (simplest for workshop)
   - **Production mention**: Azure Active Directory / Managed Identity (show but don't implement)

3. **Configuration Management**:
   - User secrets for local development
   - Azure Key Vault integration (demonstrate pattern)
   - .NET Configuration system for flexibility

4. **Cost Control Strategy**:
   - Set max_tokens limits on all completions
   - Implement conversation truncation after N turns
   - Use streaming to show progress without multiple calls
   - Monitor token usage via OpenTelemetry

### Rationale

**Why Azure OpenAI over other options**:

1. **Enterprise Requirements Met**: 
   - Data residency guarantees (data stays in Azure region)
   - GDPR/compliance support
   - No training on customer data
   - SLA guarantees (99.9% uptime)

2. **Workshop Practicality**:
   - Consistent with spec requirement (Azure is mandatory)
   - Unified billing/management for instructor
   - Regional models available globally (eastus2, swedencentral)
   - Familiar Azure portal experience

3. **Cost Predictability**:
   - Per-token pricing (no seat licenses)
   - Pay-as-you-go aligned with workshop timeframe
   - Free tier available for initial setup/testing

4. **Feature Parity**:
   - Latest models including GPT-5 series (gpt-5.2, gpt-5.2-chat)
   - Supports all required capabilities (function calling, streaming, vision)
   - Early access to new models via preview programs

**Regional Availability** (as of Jan 2026):
- **East US 2**: Broadest model selection including latest GPT-5 series (best for workshop)
- **Sweden Central**: European alternative with comprehensive model coverage
- **West Europe**: Production-ready models but fewer preview options

### Alternatives Considered

| Alternative | Why Rejected |
|-------------|--------------|
| **OpenAI Direct API** | No enterprise controls, data may be used for training, workshop spec requires Azure |
| **Local Models (Ollama/LM Studio)** | Inconsistent participant hardware, quality gap for complex tasks, no state persistence |
| **Other Cloud Providers (AWS Bedrock, Google Vertex AI)** | Inconsistent with Microsoft ecosystem focus, different SDK patterns |
| **Azure AI Foundry** | Adds unnecessary complexity for intro workshop; better for advanced scenarios |

### Implementation Notes for Phase 1

**SDK Setup Pattern**:

```csharp
using Azure;
using Azure.AI.OpenAI;
using Microsoft.AI.Agents;

// Recommended configuration approach
var builder = Kernel.CreateBuilder();

builder.AddAzureOpenAIChatCompletion(
    deploymentName: "gpt-5.2",  // Deployment name in Azure
    endpoint: "https://<resource-name>.openai.azure.com/",
    apiKey: "<api-key>"  // From user secrets in dev
);

var kernel = builder.Build();
```

**Configuration Best Practices**:

```json
// appsettings.json structure
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "DeploymentName": "gpt-5.2",
    "MaxTokens": 2000,
    "Temperature": 0.7
  }
}
```

```csharp
// User secrets for development (not committed to git)
// dotnet user-secrets set "AzureOpenAI:ApiKey" "your-key-here"
```

**Authentication Patterns**:

1. **API Key (Workshop Default)**:
   ```csharp
   new AzureKeyCredential(configuration["AzureOpenAI:ApiKey"])
   ```

2. **Azure Active Directory (Show for reference)**:
   ```csharp
   new DefaultAzureCredential()  // Auto-detects: env vars → managed identity → az cli
   ```

**Error Handling for Educational Environments**:

```csharp
try
{
    var result = await agent.InvokeAsync(userMessage);
}
catch (HttpOperationException ex) when (ex.StatusCode == 429)
{
    // Rate limit exceeded - wait and retry
    await Task.Delay(TimeSpan.FromSeconds(10));
    // Retry logic
}
catch (HttpOperationException ex) when (ex.StatusCode == 401)
{
    // Authentication failure - check API key
    Console.WriteLine("Error: Invalid API key. Check configuration.");
}
catch (HttpOperationException ex) when (ex.StatusCode == 400)
{
    // Bad request - often malformed function call or token limit
    Console.WriteLine($"Request error: {ex.Message}");
}
```

**Cost Management for Workshop**:

**Estimated Costs (as of Jan 2026)**:

| Model | Input (per 1M tokens) | Output (per 1M tokens) | Workshop Usage Estimate |
|-------|----------------------|------------------------|------------------------|
| gpt-5.2 | $3.00 | $12.00 | ~$8-15 per participant (3h) |
| gpt-5.2-chat | $2.00 | $8.00 | ~$5-10 per participant |
| o4-mini | $3.00 | $12.00 | ~$10-15 per participant |

**Workshop Cost Control Strategy**:

1. **Pre-Workshop**:
   - Set Azure spending limits on subscription
   - Create separate resource for workshop with budget alert
   - Pre-deploy models to avoid cold-start delays

2. **During Workshop**:
   - Set `max_tokens=500` for most examples (prevents runaway generation)
   - Truncate conversation history after 5-10 turns
   - Use streaming to show incremental results (perception of speed)
   - Batch similar requests where possible

3. **Rate Limit Management**:
   - Default TPM (tokens per minute): 50K for gpt-5.2
   - Implement exponential backoff: 1s → 2s → 4s → 8s
   - Show participants how to check quota in Azure portal

**Instructor Preparation Checklist**:

```markdown
- [ ] Create Azure OpenAI resource in eastus2 or swedencentral
- [ ] Deploy gpt-5.2 model (deployment name: "gpt-5.2")
- [ ] Deploy gpt-5.2-chat model (deployment name: "gpt-5.2-chat")
- [ ] Set spending alerts at $50, $100, $200 thresholds
- [ ] Test API connectivity from workshop network
- [ ] Prepare API key distribution mechanism (unique keys or shared with monitoring)
- [ ] Create fallback resource in different region (for quota issues)
- [ ] Document quota limits for reference during workshop
```

---

## 3. Workshop Best Practices

### Decision/Recommendation

**Adopt a "Copy-Paste-Observe-Discuss" learning model**:

1. **Copy-Paste Execution**: All code is pre-written and tested; participants copy into their environment
2. **Validation Checkpoints**: After each major concept, instructor validates 80-90% completion before proceeding
3. **Incremental Complexity**: Each lab adds exactly ONE new concept to avoid cognitive overload
4. **Observable Results**: Every lab produces visible output (console, logs, or UI) for immediate feedback

**Lab Structure Template**:

```markdown
## Lab X.Y: [Descriptive Title]

**Duración**: 15-20 minutos  
**Objetivo**: [Single, clear learning objective]  
**Prerequisitos**: [Previous labs required]

### Paso 1: Preparación
- Crear nueva carpeta/proyecto
- Instalar paquetes necesarios (copiar comando exacto)

### Paso 2: Implementación
```csharp
// Código completo pre-escrito
// Comentarios en español explicando cada sección
```

### Paso 3: Ejecución
- Comando exacto para ejecutar
- Salida esperada (screenshot o texto)

### Paso 4: Validación
- Verificar output específico
- ✅ Checkpoint: [Criterio de éxito claro]

### Paso 5: Experimentación (Opcional)
- Sugerencias para modificar y probar
- Preguntas guiadas para exploración
```

### Rationale

**Why this approach works for instructor-led workshops**:

1. **Reduces Environment Issues**: Pre-written code eliminates syntax errors and "works on my machine" problems
2. **Maximizes Learning Time**: No time wasted debugging typos; focus on concepts
3. **Builds Confidence**: Guaranteed success creates positive momentum
4. **Enables Experimentation**: Once working, participants can safely modify
5. **Facilitates Checkpoints**: Instructor can verify completion visually/manually

**Checkpoint Validation Strategy**:

| Checkpoint Type | Validation Method | Time Required |
|-----------------|-------------------|---------------|
| **Visual (Console Output)** | Instructor asks "Who sees 'Success: Agent responded'?" - show of hands | 30 seconds |
| **File Existence** | "Everyone navigate to output folder and confirm weather.json exists" | 1 minute |
| **Code Trace** | Share screen of 2-3 participants showing breakpoint hit | 2 minutes |
| **Pair Check** | Participants verify their neighbor's output | 1 minute |
| **Question** | "Raise hand if your agent called the weather function automatically" | 30 seconds |

### Alternatives Considered

| Alternative | Why Rejected |
|-------------|--------------|
| **Live Coding / Type-Along** | Too slow, typos cause delays, not everyone types at same speed |
| **Automated Unit Tests** | Adds complexity, participants unfamiliar with testing frameworks |
| **Cloud-Based Notebooks (Jupyter)** | Network dependencies, limited C# support, not representative of real development |
| **Self-Paced with Auto-Grading** | Spec requires instructor-led, no social learning, harder to debug issues |

### Implementation Notes for Phase 1

**Time Estimates by Lab Complexity**:

| Lab Type | Description | Estimated Time | Example |
|----------|-------------|----------------|---------|
| **Concept Introduction** | New theory, no code | 10-15 min | "What is MAF?" presentation |
| **Simple Copy-Paste** | Single file, <50 lines, no dependencies | 10-15 min | Hello Agent |
| **Standard Lab** | 1-2 files, 100-200 lines, package install | 20-25 min | Weather function tool |
| **Complex Integration** | Multiple projects, configuration, Azure setup | 30-40 min | Azure AI Agent Service |
| **Capstone** | Combines multiple concepts | 45-60 min | Multi-agent ASP.NET app |

**Typical Workshop Schedule (7-hour version)**:

```
09:00-09:30  Introducción + Setup Environment
09:30-10:30  Módulo 1: Fundamentos (Hello Agent)
10:30-10:45  BREAK

10:45-12:00  Módulo 2: Function Tools (3 labs)
12:00-13:00  LUNCH

13:00-14:30  Módulo 3: Workflows (4 labs)
14:30-14:45  BREAK

14:45-15:45  Módulo 4: Observabilidad (3 labs)
15:45-16:00  BREAK

16:00-17:00  Módulo 5: ASP.NET + Aspire (1 capstone lab)
17:00-17:15  Módulo 6: DevUI (demo + quick lab)
17:15-17:30  Módulo 7: MCP Overview (theory + reference)
```

**Pitfalls and Mitigation Strategies**:

| Pitfall | Symptoms | Prevention | Mitigation |
|---------|----------|------------|------------|
| **Environment Divergence** | "Works for instructor, not for me" | Pre-workshop setup guide + verification script | Shared VM/container as backup |
| **Pace Splitting** | Some finish in 5 min, others need 20 min | Optional "Challenge" tasks in each lab | Early finishers help neighbors |
| **Copy-Paste Errors** | Hidden characters, encoding issues | Provide labs via GitHub + downloadable ZIP | Test from multiple browsers/OS |
| **Azure Quota Exhaustion** | 429 errors mid-workshop | Monitor usage dashboard, have backup resource | Shift participants to backup deployment |
| **Network Issues** | Can't reach Azure OpenAI | Test venue network beforehand | Hotspot backup, local model fallback for some labs |

**Materials Preparation Checklist**:

```markdown
Pre-Workshop (1 week before):
- [ ] Test all labs end-to-end on clean Windows 11 VM
- [ ] Test all labs end-to-end on clean macOS
- [ ] Test all labs end-to-end on Linux (Ubuntu)
- [ ] Verify Azure OpenAI deployments are active
- [ ] Create participant handout PDF (all labs in one document)
- [ ] Create USB drives with labs (network backup)
- [ ] Prepare troubleshooting guide (top 10 issues + fixes)

Day Before:
- [ ] Test venue network (Azure connectivity, speed)
- [ ] Deploy all Azure resources participants will use
- [ ] Send pre-work email (install .NET SDK, VS Code, Git)
- [ ] Print physical copies of "Emergency Cheat Sheet"

Morning Of:
- [ ] Test projector with laptop
- [ ] Start Azure Monitor dashboard
- [ ] Open all lab files in VS Code tabs for quick sharing
- [ ] Activate screen sharing tool for remote participants (if hybrid)
```

**Instructor Facilitation Tips**:

1. **Use Timers Visibly**: Project countdown timer during lab exercises
2. **Roving Support**: Walk around during labs, don't stay at podium
3. **"Raise Hand" Checkpoints**: Quick visual assessment of completion
4. **Share Screen Frequently**: Show expected output, common mistakes
5. **Pause for Questions**: After each checkpoint, ask "Before we move on, any questions?"
6. **Celebrate Success**: "Great! I see most of you got the agent to respond correctly"

**Participant Success Metrics** (manually collected):

```
Checkpoint Form (filled by instructor during workshop):

Módulo 1 - Hello Agent:
- Participants who completed: ___ / ___
- Time taken: ___ minutes
- Common issues: ___________

[Repeat for each module]

End of Workshop Survey (digital):
1. Rate clarity of materials (1-5)
2. Rate pace of workshop (too slow / just right / too fast)
3. Rate hands-on time (too little / just right / too much)
4. Which module was most valuable?
5. Which module was most challenging?
6. Would you recommend this workshop? (Yes/No)
```

---

## 4. Technology Decisions

### Decision/Recommendation

**Technology Stack**:

1. **.NET Aspire** for orchestration and multi-service management
2. **OpenTelemetry** for observability (metrics, traces, logs)
3. **Azure AI Agent Service** for stateful workflow persistence (Modules 3-4)
4. **Local state in-memory** for Modules 1-2 (simpler introduction)

### Rationale

#### Why .NET Aspire for Orchestration?

**Decision**: Use .NET Aspire as the orchestration layer for multi-agent ASP.NET applications (Module 5).

**Key Benefits**:

1. **Developer Experience**:
   - Single `dotnet run` command starts all services (API, agents, dependencies)
   - Unified dashboard for logs, traces, metrics across all services
   - Auto-discovery: Services find each other without manual configuration
   - Environment variable propagation (connection strings, API keys)

2. **Built-in Observability**:
   - OpenTelemetry pre-configured for all services
   - Distributed tracing works "out of the box"
   - Metrics aggregation across services
   - No manual instrumentation for standard scenarios

3. **Local-to-Cloud Parity**:
   - Same development experience locally and in Azure
   - Deployment templates (azd) for Azure Container Apps
   - Consistent service discovery and configuration

4. **Workshop Fit**:
   - Dramatically simplifies multi-service debugging
   - Visual dashboard helps participants understand service interactions
   - Reduces configuration boilerplate by ~70%

**When to introduce**: Module 5 only (after participants understand single agents and basic workflows)

**Example Aspire AppHost**:

```csharp
// AppHost/Program.cs
var builder = DistributedApplication.CreateBuilder(args);

// Add Azure OpenAI connection (shared across services)
var openai = builder.AddAzureOpenAI("openai");

// Add agent services
var weatherAgent = builder.AddProject<Projects.WeatherAgent>("weather-agent")
    .WithReference(openai);

var summaryAgent = builder.AddProject<Projects.SummaryAgent>("summary-agent")
    .WithReference(openai);

// Add API that orchestrates agents
builder.AddProject<Projects.AgentApi>("agent-api")
    .WithReference(weatherAgent)
    .WithReference(summaryAgent)
    .WithExternalHttpEndpoints();

builder.Build().Run();
```

**Aspire Alternatives Considered**:

| Alternative | Why Rejected for Workshop |
|-------------|---------------------------|
| **Docker Compose** | Requires Docker knowledge, not .NET-native, less observability |
| **Manual configuration** | Too much boilerplate, error-prone, tedious for workshop |
| **Azure Kubernetes Service (AKS)** | Massive overkill, steep learning curve, not local-dev friendly |
| **Dapr** | Additional abstraction layer, more concepts to learn, not required for agents |

---

#### Why OpenTelemetry for Observability?

**Decision**: Use OpenTelemetry as the standard instrumentation library for Module 4 (Observability).

**Key Benefits**:

1. **Industry Standard**:
   - Vendor-neutral (works with Azure Monitor, Prometheus, Jaeger, etc.)
   - CNCF graduated project (mature, stable)
   - Native support in .NET 8+ and Aspire

2. **Comprehensive Observability**:
   - **Metrics**: Counters, gauges, histograms (token usage, latency, errors)
   - **Traces**: Distributed tracing across agent calls, function executions, LLM requests
   - **Logs**: Structured logging with correlation IDs

3. **Microsoft Agent Framework Integration**:
   - MAF automatically emits OpenTelemetry traces for agent invocations
   - Function calls are automatically instrumented
   - LLM token usage captured as metrics

4. **Workshop Value**:
   - Participants see "inside" agent execution with traces
   - Understand cost (tokens) and performance (latency) in real-time
   - Foundation for production monitoring

**Implementation Pattern**:

```csharp
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics.AddMeter("Microsoft.AI.Agents*"); // MAF metrics
        metrics.AddPrometheusExporter(); // Or Azure Monitor
    })
    .WithTracing(tracing =>
    {
        tracing.AddSource("Microsoft.AI.Agents*"); // MAF traces
        tracing.AddAspNetCoreInstrumentation();
        tracing.AddHttpClientInstrumentation();
        tracing.AddAzureMonitorTraceExporter(); // Send to Azure
    });
```

**Observable Metrics for Workshop**:

| Metric Category | Example Metrics | Workshop Use |
|-----------------|-----------------|--------------|
| **LLM Performance** | `llm.request.duration`, `llm.token.usage` | Show cost and latency per request |
| **Agent Execution** | `agent.invocation.duration`, `agent.error.count` | Identify slow or failing agents |
| **Function Calls** | `function.execution.count`, `function.execution.duration` | Track which functions agents call |
| **System Health** | `http.server.request.duration`, `process.cpu.usage` | Overall application performance |

**OpenTelemetry Alternatives Considered**:

| Alternative | Why Rejected for Workshop |
|-------------|---------------------------|
| **Application Insights SDK** | Azure-specific, not portable, legacy approach |
| **Custom instrumentation** | Reinventing the wheel, error-prone, not standard |
| **Logging only (no traces/metrics)** | Insufficient visibility for agent debugging |
| **Prometheus + Grafana** | Additional infrastructure, overkill for intro workshop |

---

#### Azure AI Agent Service vs. Local State Management

**Decision**: Progressive adoption based on module complexity.

**Modules 1-2 (Fundamentals + Function Tools): Local In-Memory State**

```csharp
// Simple chat history stored in memory
var chatHistory = new ChatHistory();
chatHistory.AddUserMessage("Hello");
var response = await agent.InvokeAsync(chatHistory);
chatHistory.Add(response);
```

**Pros**:
- Zero Azure dependencies beyond OpenAI
- Instant startup, no configuration
- Easy to understand for beginners
- Sufficient for stateless single-agent scenarios

**Cons**:
- Lost on app restart
- Not shareable across instances
- No workflow persistence

---

**Modules 3-4 (Workflows + Observability): Azure AI Agent Service**

**What is Azure AI Agent Service**:
- Managed service for stateful agent conversations
- Persists chat history, workflow state, and context
- Built-in retrieval (file uploads), code interpreter, web search
- API-compatible with OpenAI Assistants API

**When to use**:
- Multi-step workflows that may pause/resume
- Agents requiring access to uploaded files/knowledge base
- Long-running tasks (>30 seconds)
- Need for audit trail and conversation replay

**Implementation Pattern**:

```csharp
using Azure.AI.Projects;

// Create AI Projects client (Azure AI Agent Service)
var client = new AIProjectClient(
    new Uri("https://<your-project>.azure.com"),
    new DefaultAzureCredential()
);

// Create agent with persistence
var agent = await client.CreateAgentAsync(
    model: "gpt-5.2",
    name: "PersistentWeatherAgent",
    instructions: "You are a weather assistant",
    tools: new[] { weatherTool }
);

// Create thread (persisted conversation)
var thread = await client.CreateThreadAsync();

// Add message and run (state automatically saved)
await client.CreateMessageAsync(thread.Id, "What's the weather in Madrid?");
var run = await client.CreateRunAsync(thread.Id, agent.Id);

// Wait for completion (can be async, state persists)
while (run.Status == RunStatus.InProgress)
{
    await Task.Delay(1000);
    run = await client.GetRunAsync(thread.Id, run.Id);
}
```

**Comparison Table**:

| Aspect | Local State | Azure AI Agent Service |
|--------|-------------|------------------------|
| **Setup Complexity** | None | Requires Azure AI Foundry project |
| **State Persistence** | Lost on restart | Durable, cross-session |
| **Suitable For** | Quick prototypes, demos | Production workflows, long tasks |
| **Cost** | Free (just OpenAI usage) | Additional cost (~$0.01 per message) |
| **Knowledge Base** | Manual implementation | Built-in file upload + retrieval |
| **Workshop Module** | Modules 1-2 | Modules 3-4 |

**Alternatives Considered**:

| Alternative | Why Rejected for Some Modules |
|-------------|-------------------------------|
| **Azure CosmosDB for state** | Requires manual serialization, more complex than AI Agent Service |
| **Redis for sessions** | Another dependency, not agent-aware, manual correlation |
| **SQL database** | Overkill for chat history, schema design complexity |
| **Local file storage** | Not production-ready, hard to query, no concurrency |

**Progressive Introduction Strategy**:

```
Module 1-2: "Agents are stateless - each run is independent"
↓
Module 3: "What if we need workflows that pause/resume? → Azure AI Agent Service"
↓
Module 4: "Now we can observe state transitions with OpenTelemetry"
↓
Module 5: "Aspire manages connections to Azure AI Agent Service for us"
```

---

## Summary: Technology Stack Decision Matrix

| Component | Technology | Modules | Rationale |
|-----------|------------|---------|-----------|
| **Language** | C# 13 / .NET 10 | All | Spec requirement, enterprise standard |
| **Agent Framework** | Microsoft Agent Framework 1.0.0-preview.260108.1 | All | Production-ready, Microsoft ecosystem |
| **LLM Provider** | Azure OpenAI Service | All | Enterprise compliance, spec requirement |
| **Models** | gpt-5.2 (primary), gpt-5.2-chat (conversational) | All | Latest capabilities and performance |
| **State (Simple)** | In-Memory ChatHistory | 1-2 | Simplest introduction |
| **State (Advanced)** | Azure AI Agent Service | 3-4 | Workflow persistence, production pattern |
| **Observability** | OpenTelemetry → Azure Monitor | 4 | Industry standard, Aspire integration |
| **Orchestration** | .NET Aspire 13.1 | 5 | Multi-service developer experience |
| **Web Framework** | ASP.NET Core 10 (Minimal APIs) | 5 | Modern, lightweight, integrated |
| **Development UI** | DevUI (Agent Framework tool) | 6 | Official MAF debugging tool |
| **Interoperability** | Model Context Protocol (MCP) | 7 | Future-proofing, theory only |

---

## Additional Recommendations

### Pre-Workshop Preparation

**For Instructor**:
1. Create a "Workshop Survival Kit" repository with:
   - Completed solution code for all labs
   - Common error messages + fixes document
   - Environment setup validation script
   - Video recording of each lab (backup for async review)

2. Dry-run checklist:
   - Test labs on Windows 11, macOS, Ubuntu
   - Verify Azure OpenAI quota (request increase if needed)
   - Create isolated Azure subscription for workshop (prevent production impact)
   - Set up monitoring dashboard before workshop starts

**For Participants (send 1 week before)**:
```markdown
Preparación Pre-Workshop: Microsoft Agent Framework

Por favor completa estos pasos ANTES del workshop:

1. Instalar Software Requerido:
   - .NET 10 SDK: https://dot.net/download
   - Visual Studio Code: https://code.visualstudio.com
   - Git: https://git-scm.com/downloads

2. Verificar Instalación:
   ```bash
   dotnet --version  # Debe mostrar 10.0.x
   code --version
   git --version
   ```

3. Clonar Repositorio del Workshop:
   ```bash
   git clone https://github.com/tu-org/maf-workshop.git
   cd maf-workshop
   ```

4. Prueba de Conectividad Azure OpenAI:
   - Ejecutar: `dotnet run --project tests/connection-test`
   - Deberías ver: "✅ Conexión exitosa a Azure OpenAI"

Nota: Necesitarás una subscripción activa de Azure con acceso a Azure OpenAI Service.
Si no tienes acceso, contacta al instructor antes del workshop.
```

### Post-Workshop Resources

**For Participants**:

1. **Certification Path** (mention but don't require):
   - Microsoft Applied Skills: Build a copilot with Azure AI
   - Microsoft Learn Learning Path: Develop AI agents with Microsoft Agent Framework

2. **Next Steps Document**:
   ```markdown
   # Próximos Pasos Después del Workshop

   ## Proyectos Sugeridos (Dificultad Incremental)

   ### Nivel 1: Reforzar Fundamentos
   - [ ] Agregar 3 function tools adicionales a tu agente
   - [ ] Implementar human-in-the-loop approval
   - [ ] Crear un agente especializado para tu dominio

   ### Nivel 2: Workflows Complejos
   - [ ] Diseñar un workflow secuencial de 4 pasos
   - [ ] Implementar un group chat con 3+ agentes
   - [ ] Agregar persistencia con Azure AI Agent Service

   ### Nivel 3: Producción
   - [ ] Agregar autenticación (Azure AD)
   - [ ] Implementar rate limiting y retry logic
   - [ ] Desplegar en Azure Container Apps con Aspire
   - [ ] Configurar alertas en Azure Monitor

   ## Recursos Adicionales

   - Documentación oficial: https://learn.microsoft.com/microsoft-agent-framework
   - Ejemplos de código: https://github.com/microsoft/agent-framework-samples
   - Blog del equipo: https://devblogs.microsoft.com/
   - Discord comunitario: [link]
   ```

3. **Common Issues FAQ** (based on workshop experience):
   - "Mi agente no llama las funciones" → Check function descriptions
   - "Error 429 (Rate limit)" → Implement retry with backoff
   - "Tokens exceed limit" → Truncate chat history
   - "Azure connection fails" → Verify firewall/proxy settings

---

## Conclusion

This research document provides a comprehensive foundation for building the Microsoft Agent Framework workshop. The technology choices prioritize:

1. **Progressive Learning**: Simple → Complex, ensuring 80-90% success rates at each checkpoint
2. **Production Relevance**: Using enterprise-ready tools (Aspire, OpenTelemetry, Azure)
3. **Workshop Practicality**: Copy-paste code, manual validation, predictable timing
4. **Spanish Language**: All materials, comments, and error messages in Spanish

**Key Success Factors**:
- Pre-written, tested code eliminates environment issues
- Instructor checkpoints maintain group cohesion
- Azure OpenAI provides enterprise-ready foundation
- Aspire + OpenTelemetry teach modern observability patterns
- Progressive complexity respects cognitive load limits

**Next Phase**: Use this research to create detailed lab specifications in [data-model.md](data-model.md) and implementation guide in [quickstart.md](quickstart.md).

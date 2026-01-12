# Tasks: Workshop de Introducción a Microsoft Agent Framework

**Feature Branch**: `001-maf-workshop`  
**Date**: 2026-01-12  
**Input**: Design documents from `/specs/001-maf-workshop/`

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each module.

**Tests**: No explicit test tasks - workshop uses manual instructor validation at checkpoints.

---

## Format: `- [ ] [ID] [P?] [Story?] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story/module this task belongs to (US1-US7)
- All file paths are relative to repository root

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create project structure and baseline documentation folders for all 7 modules

- [X] T001 Create root documentation structure: `docs/`, `instructor-guide/`, `README.md`
- [X] T002 [P] Create module folder structure for Module 1 in `docs/modulo-01-fundamentos/` with `README.md` and `labs/` subdirectory
- [X] T003 [P] Create module folder structure for Module 2 in `docs/modulo-02-function-tools/` with `README.md` and `labs/` subdirectory
- [X] T004 [P] Create module folder structure for Module 3 in `docs/modulo-03-workflows/` with `README.md` and `labs/` subdirectory
- [X] T005 [P] Create module folder structure for Module 4 in `docs/modulo-04-observability/` with `README.md` and `labs/` subdirectory
- [X] T006 [P] Create module folder structure for Module 5 in `docs/modulo-05-aspnet-aspire/` with `README.md` and `labs/` subdirectory
- [X] T007 [P] Create module folder structure for Module 6 in `docs/modulo-06-devui/` with `README.md` and `labs/` subdirectory
- [X] T008 [P] Create module folder structure for Module 7 in `docs/modulo-07-mcp/` with `README.md` and `labs/` subdirectory
- [X] T009 [P] Create instructor guide structure: `instructor-guide/setup-checklist.md`, `instructor-guide/timing-schedule.md`, `instructor-guide/validation-checkpoints.md`
- [X] T010 Create workshop root README.md with overview, prerequisites, module index, and getting started instructions in Spanish

**Checkpoint**: ✅ All folder structure created - ready for content generation

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Create shared resources that ALL modules depend on - MUST complete before any module content

**⚠️ CRITICAL**: No module-specific work can begin until this phase is complete

- [X] T011 Create `instructor-guide/setup-checklist.md` with pre-workshop Azure setup (OpenAI resource creation, model deployment, API key distribution, Azure AI Foundry project for Module 3)
- [X] T011b [P] Document Azure AI Foundry project setup in `instructor-guide/azure-ai-foundry-setup.md` including workspace creation, authentication configuration, and connection to Azure OpenAI (required for Module 3 Lab 5)
- [X] T012 Create `instructor-guide/timing-schedule.md` with 7-hour workshop schedule including breaks and checkpoints
- [X] T013 Create `instructor-guide/validation-checkpoints.md` documenting manual validation criteria for each module checkpoint with specific methods: show-of-hands count, screen-share verification (2-3 participants), pair checking, output confirmation
- [X] T014 [P] Create environment verification script in `scripts/verify-environment.sh` to check .NET SDK, Azure OpenAI connectivity, and required tools
- [X] T015 [P] Create participant pre-work email template in `instructor-guide/pre-workshop-email.md` with installation instructions in Spanish
- [X] T016 [P] Create troubleshooting guide in `docs/troubleshooting.md` with top 10 common errors and solutions in Spanish
- [X] T017 Create participant handout template in `docs/participant-handout-template.md` for printing all labs in single PDF

**Checkpoint**: ✅ Foundation ready - module content implementation can now begin in parallel

---

## Phase 3: User Story 1 - Module 1: Fundamentos (Priority: P1) 🎯 MVP

**Goal**: Participants understand MAF concepts and create their first functional agent

**Independent Test**: Participant completes Module 1, sets up .NET environment, and executes 'Hello Agent' demo successfully

### Theory Documentation

- [X] T018 [P] [US1] Create theory documentation in `docs/modulo-01-fundamentos/README.md` explaining what is MAF, agent types, orchestration concepts, MCP and A2A protocols (Spanish, 15 min read time)
- [X] T019 [P] [US1] Create installation guide in `docs/modulo-01-fundamentos/instalacion.md` with .NET 10 SDK setup, VS Code installation, and Azure OpenAI configuration instructions (Spanish)
- [X] T020 [P] [US1] Add architecture diagrams to theory doc using Mermaid showing agent types and basic orchestration patterns

### Module 1: Lab 01-hello-agent

- [X] T021 [US1] Create lab folder `docs/modulo-01-fundamentos/labs/01-hello-agent/` with README.md following workshop content format
- [X] T022 [US1] Create complete C# solution in `docs/modulo-01-fundamentos/labs/01-hello-agent/HelloAgent.csproj` targeting net10.0 with Microsoft.Agents.AI 1.0.0-preview.260108.1
- [X] T023 [US1] Implement Program.cs with ChatCompletionAgent connecting to Azure OpenAI, complete Spanish comments, handles basic greeting conversation
- [X] T024 [US1] Create appsettings.json with Azure OpenAI endpoint/deployment configuration (no secrets)
- [X] T025 [US1] Write lab instructions in `docs/modulo-01-fundamentos/labs/01-hello-agent/README.md` with 6 steps: preparation, package installation, configuration, implementation, execution, validation (Spanish, 15 min duration)
- [X] T026 [US1] Add troubleshooting section to lab README with 3 common errors: "dotnet not found", "401 Unauthorized", "agent no responde"

### Module 1: Instructor Guide

- [X] T027 [US1] Create `docs/modulo-01-fundamentos/instructor-guide.md` with timing breakdown, checkpoint validation method, common issues, and pacing adjustments

**Checkpoint**: Module 1 complete and independently testable - participant can create first agent and see it respond to greetings

---

## Phase 4: User Story 2 - Module 2: Function Tools (Priority: P1)

**Goal**: Participants learn to extend agent capabilities with function tools and agent composition

**Independent Test**: Participant completes Module 2 and creates agent with custom function tool, demonstrates agent-as-tool

### Theory Documentation

- [X] T028 [P] [US2] Create theory documentation in `docs/modulo-02-function-tools/README.md` explaining function tools, KernelFunction attribute, function calling flow, agent composition patterns (Spanish, 15 min read time)
- [X] T029 [P] [US2] Add code examples to theory doc showing function tool definition, registration, and automatic invocation

### Module 2: Lab 01-custom-tool

- [X] T030 [US2] Create lab folder `docs/modulo-02-function-tools/labs/01-custom-tool/` with README.md
- [X] T031 [US2] Create C# project `docs/modulo-02-function-tools/labs/01-custom-tool/WeatherAgent.csproj` targeting net10.0
- [X] T032 [US2] Implement WeatherService.cs with KernelFunction-decorated GetWeather method, proper Description attributes, Spanish comments
- [X] T033 [US2] Implement Program.cs registering WeatherService as plugin, ChatCompletionAgent with function calling enabled
- [X] T034 [US2] Write lab instructions with 5 steps: project setup, implement function, register plugin, test automatic invocation, validation (Spanish, 20 min duration)
- [X] T035 [US2] Add troubleshooting: "agent doesn't call function" (missing Description), "function throws error" (parameter validation)

### Module 2: Lab 02-agent-as-tool

- [X] T036 [US2] Create lab folder `docs/modulo-02-function-tools/labs/02-agent-as-tool/` with README.md
- [X] T037 [US2] Create C# project `docs/modulo-02-function-tools/labs/02-agent-as-tool/AgentComposition.csproj` targeting net10.0
- [X] T038 [US2] Implement CalculatorAgent.cs as specialized agent for mathematical operations
- [X] T039 [US2] Implement MainAgent.cs that uses CalculatorAgent as a function tool for delegation
- [X] T040 [US2] Implement Program.cs demonstrating conversation where MainAgent automatically delegates math questions to CalculatorAgent
- [X] T041 [US2] Write lab instructions with 6 steps: create specialized agent, create coordinator agent, register as tool, test delegation, validation (Spanish, 25 min duration)

### Module 2: Instructor Guide

- [X] T047 [US2] Create `docs/modulo-02-function-tools/instructor-guide.md` with timing for 2 labs (55 min total), checkpoint after each lab, success criteria

**Checkpoint**: ✅ Module 2 complete - participant can create custom tools and compose agents

---

## Phase 5: User Story 3 - Module 3: Workflows (Priority: P2)

**Goal**: Participants learn to orchestrate multiple agents and tasks using workflows with Azure AI Agent Service persistence

**Independent Test**: Participant creates one workflow of each type (sequential, parallel, delegation, group chat), implements Planner+Executor, demonstrates state persistence

### Theory Documentation

- [X] T048 [P] [US3] Create theory documentation in `docs/modulo-03-workflows/README.md` explaining 4 workflow types, Planner+Executor pattern, Human-in-the-loop workflows, state management (Spanish, 20 min read time)
- [X] T049 [P] [US3] Add workflow diagrams using Mermaid showing sequential, parallel, delegation, and group chat patterns

### Module 3: Lab 01-sequential

- [X] T050 [US3] Create lab folder `docs/modulo-03-workflows/labs/01-sequential/` with README.md
- [X] T051 [US3] Create C# project `docs/modulo-03-workflows/labs/01-sequential/SequentialWorkflow.csproj` targeting net10.0
- [X] T052 [US3] Implement 3-step sequential workflow: ResearchAgent → WritingAgent → ReviewAgent with output passing between steps
- [X] T053 [US3] Implement Program.cs orchestrating sequential execution, displaying progress after each step
- [X] T054 [US3] Write lab instructions with 5 steps: create agent chain, implement coordinator, execute workflow, validate output order (Spanish, 20 min duration)

### Module 3: Lab 02-parallel

- [X] T055 [US3] Create lab folder `docs/modulo-03-workflows/labs/02-parallel/` with README.md
- [X] T056 [US3] Create C# project `docs/modulo-03-workflows/labs/02-parallel/ParallelWorkflow.csproj` targeting net10.0
- [X] T057 [US3] Implement parallel execution of 3 agents: NewsAgent, WeatherAgent, StocksAgent using Task.WhenAll
- [X] T058 [US3] Implement aggregation logic in Program.cs combining results from all agents into unified response
- [X] T059 [US3] Write lab instructions with 5 steps: create independent agents, implement parallel executor, aggregate results, validate concurrency (Spanish, 25 min duration)

### Module 3: Lab 03-delegation

- [X] T060 [US3] Create lab folder `docs/modulo-03-workflows/labs/03-delegation/` with README.md
- [X] T061 [US3] Create C# project `docs/modulo-03-workflows/labs/03-delegation/DelegationWorkflow.csproj` targeting net10.0
- [X] T062 [US3] Implement ProjectManagerAgent that analyzes task and delegates to DesignerAgent, DeveloperAgent, or QAAgent based on task type
- [X] T063 [US3] Implement routing logic in ProjectManagerAgent using function calling to select appropriate specialist
- [X] T064 [US3] Write lab instructions with 6 steps: create specialist agents, implement coordinator with routing, test different task types, validation (Spanish, 25 min duration)

### Module 3: Lab 04-group-chat

- [X] T065 [US3] Create lab folder `docs/modulo-03-workflows/labs/04-group-chat/` with README.md
- [X] T066 [US3] Create C# project `docs/modulo-03-workflows/labs/04-group-chat/GroupChatWorkflow.csproj` targeting net10.0
- [X] T067 [US3] Implement AgentGroupChat with 3 agents: BrainstormAgent, CriticAgent, SynthesizerAgent collaborating on problem solving
- [X] T068 [US3] Configure termination strategy in Program.cs: max turns, consensus reached, or explicit termination message
- [X] T069 [US3] Write lab instructions with 6 steps: create collaborative agents, configure group chat, implement termination, execute conversation, validation (Spanish, 30 min duration)

### Module 3: Lab 05-azure-agent-service

- [X] T070 [US3] Create lab folder `docs/modulo-03-workflows/labs/05-azure-agent-service/` with README.md
- [X] T071 [US3] Create C# project `docs/modulo-03-workflows/labs/05-azure-agent-service/PersistentWorkflow.csproj` targeting net10.0 with Azure.AI.Projects package
- [X] T072 [US3] Implement AIProjectClient connection to Azure AI Agent Service with DefaultAzureCredential authentication
- [X] T073 [US3] Implement agent creation with tools, thread creation for conversation persistence, message sending with run polling
- [X] T074 [US3] Demonstrate pause/resume workflow: create run, stop program, restart program, retrieve thread, continue conversation
- [X] T075 [US3] Write lab instructions with 7 steps: Azure AI Foundry project setup, configure authentication, implement persistence, test pause/resume, validation (Spanish, 35 min duration)
- [X] T076 [US3] Add troubleshooting: "authentication fails" (Azure credentials), "thread not found" (thread ID persistence), "run timeout" (long operations)

### Module 3: Instructor Guide

- [X] T077 [US3] Create `docs/modulo-03-workflows/instructor-guide.md` with timing for 5 labs (135 min total), checkpoint after labs 2, 4, 5, Azure setup time allocation

**Checkpoint**: ✅ Module 3 complete - participant can implement all workflow patterns and use Azure AI Agent Service for state persistence

---

## Phase 6: User Story 4 - Module 4: Observability (Priority: P2)

**Goal**: Participants learn to monitor, trace, and diagnose agent applications using OpenTelemetry and Azure Monitor

**Independent Test**: Participant configures agent with metrics, implements distributed tracing, creates PII-safe logs, visualizes in Azure Monitor with alerts

### Theory Documentation

- [X] T078 [P] [US4] Create theory documentation in `docs/modulo-04-observability/README.md` explaining observability pillars (metrics, traces, logs), OpenTelemetry concepts, Azure Monitor integration (Spanish, 20 min read time)
- [X] T079 [P] [US4] Add observability diagrams showing data flow from agent → OpenTelemetry → Azure Monitor

### Module 4: Lab 01-metrics-tokens

- [X] T080 [US4] Create lab folder `docs/modulo-04-observability/labs/01-metrics-tokens/` with README.md
- [X] T081 [US4] Create C# project `docs/modulo-04-observability/labs/01-metrics-tokens/MetricsExample.csproj` targeting net10.0 with OpenTelemetry.Extensions.Hosting and OpenTelemetry.Instrumentation.Runtime
- [X] T082 [US4] Implement metrics collection for agent invocations: latency (histogram), error rate (counter), token usage (counter)
- [X] T083 [US4] Configure OpenTelemetry metrics exporter to console and Prometheus format
- [X] T084 [US4] Write lab instructions with 5 steps: add OpenTelemetry packages, configure metrics, instrument agent, view metrics output, validation (Spanish, 20 min duration)

### Module 4: Lab 02-distributed-traces

- [X] T085 [US4] Create lab folder `docs/modulo-04-observability/labs/02-distributed-traces/` with README.md
- [X] T086 [US4] Create C# project `docs/modulo-04-observability/labs/02-distributed-traces/TracingExample.csproj` with OpenTelemetry.Instrumentation.Http and OpenTelemetry.Exporter.Console
- [X] T087 [US4] Implement distributed tracing for multi-agent workflow: parent span for workflow, child spans for each agent invocation, function call spans
- [X] T088 [US4] Configure trace context propagation between agents, add custom attributes (agent name, function called, prompt tokens)
- [X] T089 [US4] Write lab instructions with 6 steps: configure tracing, instrument workflow, add custom spans, view trace hierarchy, validation (Spanish, 25 min duration)

### Module 4: Lab 03-azure-monitor

- [X] T090 [US4] Create lab folder `docs/modulo-04-observability/labs/03-azure-monitor/` with README.md
- [X] T091 [US4] Create C# project `docs/modulo-04-observability/labs/03-azure-monitor/AzureMonitor.csproj` with Azure.Monitor.OpenTelemetry.Exporter
- [X] T092 [US4] Configure OpenTelemetry to export metrics and traces to Azure Monitor Application Insights using connection string
- [X] T093 [US4] Implement structured logging with Microsoft.Extensions.Logging, PII redaction for user messages, correlation IDs
- [X] T094 [US4] Create Azure Monitor workbook JSON template in `docs/modulo-04-observability/labs/03-azure-monitor/workbook-template.json` with pre-configured charts: agent latency histogram, token usage time series, error rate counter
- [X] T095 [US4] Configure alert rules: latency > 5s, error rate > 5%, token usage > threshold
- [X] T096 [US4] Write lab instructions with 8 steps: create Application Insights resource, configure exporter, implement logging, create dashboard, setup alerts, validation (Spanish, 30 min duration)

### Module 4: Instructor Guide

- [X] T097 [US4] Create `docs/modulo-04-observability/instructor-guide.md` with timing for 3 labs (75 min total), Azure Monitor setup time, checkpoint after lab 3

**Checkpoint**: ✅ Module 4 complete - participant has full observability pipeline from agents to Azure Monitor with dashboards and alerts

---

## Phase 7: User Story 5 - Module 5: ASP.NET + Aspire (Priority: P3)

**Goal**: Participants integrate MAF into ASP.NET web applications and use .NET Aspire for orchestration

**Independent Test**: Participant creates ASP.NET application exposing agents via API, orchestrated with Aspire, demonstrating scalability

### Theory Documentation

- [X] T098 [P] [US5] Create theory documentation in `docs/modulo-05-aspnet-aspire/README.md` explaining ASP.NET Core integration with MAF, exposing agents as HTTP APIs, .NET Aspire orchestration benefits (Spanish, 20 min read time)
- [X] T099 [P] [US5] Add architecture diagrams showing ASP.NET API → Agent Services → Azure OpenAI with Aspire orchestration layer

### Module 5: Lab 01-multi-agent-web (Capstone)

- [X] T100 [US5] Create lab folder `docs/modulo-05-aspnet-aspire/labs/01-multi-agent-web/` with README.md
- [X] T101 [US5] Create Aspire AppHost project in `docs/modulo-05-aspnet-aspire/labs/01-multi-agent-web/AppHost/AppHost.csproj` with Aspire.Hosting package
- [X] T102 [US5] Create ASP.NET API project in `docs/modulo-05-aspnet-aspire/labs/01-multi-agent-web/WebApi/WebApi.csproj` with minimal APIs
- [X] T103 [US5] Create agent services library in `docs/modulo-05-aspnet-aspire/labs/01-multi-agent-web/AgentServices/AgentServices.csproj` with ChatCompletionAgent implementations
- [X] T104 [US5] Implement WeatherAgent and SummaryAgent services in AgentServices with dependency injection
- [X] T105 [US5] Implement API endpoints in WebApi: POST /api/chat/weather and POST /api/chat/summary using agent services
- [X] T106 [US5] Configure Aspire AppHost to orchestrate WebApi project, add Azure OpenAI connection reference, enable observability dashboard
- [X] T107 [US5] Implement proper error handling, request validation, response formatting in API controllers
- [X] T108 [US5] Add OpenAPI/Swagger documentation for API endpoints
- [X] T109 [US5] Write lab instructions with 10 steps: create solution structure, implement agents, create API, configure Aspire, test endpoints, view dashboard, validation (Spanish, 60 min duration)
- [X] T110 [US5] Add troubleshooting: "Aspire dashboard not accessible" (port conflicts), "agents not resolving" (DI configuration), "CORS errors" (CORS policy)

### Module 5: Instructor Guide

- [X] T111 [US5] Create `docs/modulo-05-aspnet-aspire/instructor-guide.md` with timing for capstone lab (60 min), checkpoint after successful API call, scaling discussion

**Checkpoint**: ✅ Module 5 complete - participant has production-ready multi-agent web application with Aspire orchestration

---

## Phase 8: User Story 6 - Module 6: DevUI (Priority: P2)

**Goal**: Participants learn to use DevUI for debugging, testing, and interacting with agents during development

**Independent Test**: Participant configures DevUI, visualizes agent conversations, inspects function tool invocations, debugs agent behavior

### Theory Documentation

- [X] T112 [P] [US6] Create theory documentation in `docs/modulo-06-devui/README.md` explaining DevUI purpose, capabilities (conversation visualization, state inspection, testing), use cases (Spanish, 10 min read time)
- [X] T113 [P] [US6] Add screenshots showing DevUI interface, conversation view, function tool inspection panel

### Module 6: Lab 01-devui-setup

- [X] T114 [US6] Create lab folder `docs/modulo-06-devui/labs/01-devui-setup/` with README.md
- [X] T115 [US6] Create C# project `docs/modulo-06-devui/labs/01-devui-setup/DevUIExample.csproj` with agent implementation and DevUI configuration
- [X] T116 [US6] Implement sample agent with multiple function tools (weather, calendar, calculator) for demonstration purposes
- [X] T117 [US6] Configure DevUI connection: install DevUI tool, configure agent for DevUI connectivity, launch DevUI web interface
- [X] T118 [US6] Write lab instructions with 6 steps: install DevUI, configure agent, launch DevUI, test conversation, inspect function calls, validation (Spanish, 15 min duration)
- [X] T119 [US6] Add troubleshooting: "DevUI not connecting" (port issues), "function calls not visible" (instrumentation), "authentication errors" (token config)

### Module 6: Instructor Guide

- [X] T120 [US6] Create `docs/modulo-06-devui/instructor-guide.md` with timing for lab (15 min), demo-first approach suggestion, checkpoint after DevUI connection

**Checkpoint**: ✅ Module 6 complete - participant can use DevUI for agent debugging and testing workflows

---

## Phase 9: User Story 7 - Module 7: MCP Integration (Priority: P3)

**Goal**: Participants understand MCP concepts and can demonstrate MAF agent communicating via MCP

**Independent Test**: Participant can list 2 MCP benefits, explain interoperability value, and demonstrate MAF agent consuming MCP server resources

### Theory Documentation

- [X] T121 [P] [US7] Create theory documentation in `docs/modulo-07-mcp/README.md` explaining Model Context Protocol, interoperability benefits, MCP servers/clients, use cases for multi-vendor agent systems (Spanish, 15 min read time)
- [X] T122 [P] [US7] Add MCP architecture diagrams showing MAF agent as MCP client, MCP server exposing resources, cross-framework communication

### Module 7: Lab 01-mcp-integration

- [X] T123 [US7] Create lab folder `docs/modulo-07-mcp/labs/01-mcp-integration/` with README.md
- [X] T124 [US7] Create C# project `docs/modulo-07-mcp/labs/01-mcp-integration/MCPIntegration.csproj` with MCP client library
- [X] T125 [US7] Implement sample MCP server (or use public MCP server) exposing weather and news resources
- [X] T126 [US7] Implement MAF agent configured as MCP client, discovering and consuming MCP server resources
- [X] T127 [US7] Demonstrate agent using MCP-provided tools alongside native function tools
- [X] T128 [US7] Write lab instructions with 6 steps: understand MCP concepts, setup MCP server, configure MAF client, test resource access, validation (Spanish, 20 min duration)

### Module 7: Instructor Guide

- [X] T129 [US7] Create `docs/modulo-07-mcp/instructor-guide.md` with timing for theory + lab (25 min total), emphasis on conceptual understanding over implementation, Q&A checkpoint

**Checkpoint**: ✅ Module 7 complete - participant understands MCP value proposition and has seen integration example

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Final touches, validation, and documentation improvements across all modules

- [X] T130 [P] Create comprehensive `README.md` at repository root with workshop overview, installation prerequisites, module navigation, getting started guide (Spanish)
- [X] T131 [P] Create `docs/reference.md` with links to official Microsoft Agent Framework docs, Azure OpenAI docs, sample repositories
- [X] T132 [P] Review all theory documentation for consistency in tone, terminology translation, formatting standards per `contracts/workshop-content-format.md`, and content depth (minimum 3 code examples per theory doc, 1 Mermaid diagram per module)
- [X] T133 [P] Validate all C# code examples compile and run on .NET 10 (Windows, macOS, Linux test matrix)
- [X] T134 [P] Verify all NuGet package versions are consistent across labs: Microsoft.Agents.AI 1.0.0-preview.260108.1
- [X] T135 [P] Check all appsettings.json files do NOT contain secrets (API keys), user-secrets instructions present in all labs
- [X] T135b [P] Audit all labs for Azure OpenAI Service exclusivity: verify no local models, no OpenAI direct API, no alternative providers (constitution compliance)
- [X] T136 [P] Validate all lab instructions follow standard format from `contracts/workshop-content-format.md` (heading structure, paso numbering, validation checkpoints)
- [X] T137 Review timing estimates across all modules sum to 7-9 hours including breaks (adjust if needed)
- [X] T138 Create participant feedback survey template in `instructor-guide/post-workshop-survey.md` for success metrics collection
- [X] T139 Create quick reference card PDF template in `docs/quick-reference.md` with common commands, troubleshooting, API patterns (Spanish)
- [X] T140 Run final validation of `quickstart.md` accuracy (Azure setup steps, verification commands, cost estimates current)
- [X] T141 Generate table of contents for all README.md files, ensure internal links work
- [X] T142 Create workshop completion certificate template in `docs/certificate-template.md` for participants

**Final Checkpoint**: ✅ All modules complete, tested, documented - workshop ready for delivery

---

## Dependencies & Execution Order

### Phase Dependencies

1. **Phase 1 (Setup)**: No dependencies - start immediately
2. **Phase 2 (Foundational)**: Depends on Phase 1 completion - BLOCKS all user stories
3. **Phases 3-9 (User Stories 1-7)**: All depend on Phase 2 completion
   - User stories CAN proceed in parallel if team capacity allows
   - Or sequentially in priority order: US1 (P1) → US2 (P1) → US3 (P2) → US4 (P2) → US6 (P2) → US5 (P3) → US7 (P3)
4. **Phase 10 (Polish)**: Depends on all desired user stories being complete

### User Story Dependencies

- **US1 (Module 1 - Fundamentos)**: Can start after Phase 2 - No dependencies on other stories
- **US2 (Module 2 - Function Tools)**: Can start after Phase 2 - Should complete US1 first for learning progression but technically independent
- **US3 (Module 3 - Workflows)**: Can start after Phase 2 - Builds on US1 and US2 concepts but independently testable
- **US4 (Module 4 - Observability)**: Can start after Phase 2 - Can run parallel to US3, independently testable
- **US5 (Module 5 - ASP.NET+Aspire)**: Recommended after US1-US4 for best learning experience - integrates all concepts but independently testable
- **US6 (Module 6 - DevUI)**: Can start anytime after US1 - Useful for all modules but independent
- **US7 (Module 7 - MCP)**: Can start after US1 - Conceptual/advanced topic, independent of other modules

### Within Each Module

- Theory documentation can be written in parallel with lab implementations
- Labs within a module should be created sequentially (Lab 01 → Lab 02 → Lab 03) to maintain learning progression
- Instructor guide created last after all labs finalized (validates timing estimates)

### Parallel Opportunities

**Phase 1 (Setup)**:
- T002-T008: All module folder structures can be created simultaneously

**Phase 2 (Foundational)**:
- T014-T016: Environment script, pre-work email, troubleshooting guide are independent

**Within Each Module**:
- Theory docs and lab folders can be created in parallel
- Example for Module 2: T028-T029 (theory) parallel with T030 (lab 1 folder creation)

**Across Modules** (after Phase 2 complete):
- All modules can be developed in parallel by different team members
- Example: Developer A works on Module 1, Developer B on Module 2, Developer C on Module 4

**Phase 10 (Polish)**:
- T130-T136: Documentation reviews, validation, and reference materials are independent

---

## Parallel Execution Examples

### Example 1: Setup Phase (Phase 1)

```bash
# All module folders can be created simultaneously:
Task T002: Create Module 1 folder structure
Task T003: Create Module 2 folder structure  
Task T004: Create Module 3 folder structure
Task T005: Create Module 4 folder structure
Task T006: Create Module 5 folder structure
Task T007: Create Module 6 folder structure
Task T008: Create Module 7 folder structure
Task T009: Create instructor guide structure
```

### Example 2: Foundational Phase (Phase 2)

```bash
# Instructor guides and tools can be created in parallel:
Task T014: Create environment verification script
Task T015: Create pre-work email template
Task T016: Create troubleshooting guide
```

### Example 3: Multi-Module Development (After Phase 2)

```bash
# Different developers working on different modules simultaneously:
Developer A:
  Task T018-T027: Complete Module 1 (Fundamentos)
  
Developer B:
  Task T028-T047: Complete Module 2 (Function Tools)
  
Developer C:
  Task T078-T097: Complete Module 4 (Observability)
```

### Example 4: Within Module 2 (Function Tools)

```bash
# Theory and initial lab setup can proceed in parallel:
Task T028: Create Module 2 theory documentation
Task T030: Create Lab 01 folder structure
  
# After Lab 01 complete, Labs 02 and 03 folders can be created in parallel:
Task T036: Create Lab 02 folder structure
```

---

## Implementation Strategy

### Recommended Approach: Sequential by Priority

Given workshop requirements for progressive learning:

1. **Complete Phase 1**: Setup (T001-T010)
2. **Complete Phase 2**: Foundational (T011-T017) - CRITICAL BLOCKING PHASE
3. **Complete US1 (Module 1)**: P1 - Foundation of all learning (T018-T027)
4. **VALIDATE**: Test Module 1 independently before proceeding
5. **Complete US2 (Module 2)**: P1 - Builds on Module 1 (T028-T047)
6. **VALIDATE**: Test Module 2 independently
7. **Complete US3 (Module 3)**: P2 - Advanced workflows (T048-T077)
8. **VALIDATE**: Test Module 3 independently
9. **Complete US4 (Module 4)**: P2 - Can parallel with US3 (T078-T097)
10. **Complete US6 (Module 6)**: P2 - DevUI for debugging (T112-T120)
11. **Complete US5 (Module 5)**: P3 - Capstone integration (T098-T111)
12. **Complete US7 (Module 7)**: P3 - Advanced concept (T121-T129)
13. **Complete Phase 10**: Polish (T130-T142)

### MVP Definition (Minimum Viable Workshop)

**Scope**: Module 1 + Module 2 only (3 hours including breaks)

**Tasks Required**:
- Phase 1: T001-T010 (Setup)
- Phase 2: T011-T017 (Foundational)
- Phase 3: T018-T027 (Module 1 - Fundamentos)
- Phase 4: T028-T047 (Module 2 - Function Tools)
- Phase 10: T130, T136, T140 (Essential docs only)

**MVP Validation**: Participants can create agents and use function tools - core MAF capabilities demonstrated

### Incremental Delivery Strategy

**Release 1 (MVP - 3h workshop)**:
- Modules 1-2: Fundamentals + Function Tools
- Target: Developers new to MAF, quick introduction
- Deliverable: First agent + custom tools

**Release 2 (Standard - 5h workshop)**:
- Add Module 3: Workflows
- Add Module 6: DevUI
- Target: Teams planning MAF adoption, hands-on orchestration
- Deliverable: Multi-agent systems with debugging tools

**Release 3 (Complete - 7h workshop)**:
- Add Module 4: Observability
- Add Module 5: ASP.NET + Aspire
- Target: Production-ready implementation teams
- Deliverable: Enterprise-grade agent applications

**Release 4 (Advanced - 7h workshop + optional extensions)**:
- Add Module 7: MCP
- Add optional challenge labs
- Target: Advanced practitioners, multi-framework integration
- Deliverable: Interoperable agent ecosystems

---

## Total Task Count: 141 tasks

**Breakdown by Phase**:
- Phase 1 (Setup): 10 tasks
- Phase 2 (Foundational): 8 tasks
- Phase 3 (US1 - Module 1): 10 tasks
- Phase 4 (US2 - Module 2): 15 tasks
- Phase 5 (US3 - Module 3): 30 tasks
- Phase 6 (US4 - Module 4): 20 tasks
- Phase 7 (US5 - Module 5): 14 tasks
- Phase 8 (US6 - Module 6): 9 tasks
- Phase 9 (US7 - Module 7): 9 tasks
- Phase 10 (Polish): 15 tasks

**Parallelizable Tasks**: 35 tasks marked with [P] (24% of total)

**Independent Modules**: All 7 modules can be tested independently after foundational phase

---

## Notes

- All C# code must target .NET 10 with Microsoft.Agents.AI 1.0.0-preview.260108.1
- All documentation, comments, and instructions MUST be in Spanish per spec requirement
- All labs MUST include complete copy-paste ready code (no participant coding required)
- All modules MUST have manual instructor validation checkpoints per spec
- No automated tests - workshop uses instructor observation and participant confirmation
- Azure OpenAI Service is MANDATORY for all modules (no local alternatives)
- Timing estimates should be validated during testing phase (Phase 10)
- Success criteria: 70-95% participant completion rates at checkpoints (varies by module complexity)

---

**Generated**: 2026-01-12  
**Feature Branch**: 001-maf-workshop  
**Status**: Ready for implementation

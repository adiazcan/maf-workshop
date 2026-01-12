# Implementation Plan: [FEATURE]

**Branch**: `001-maf-workshop` | **Date**: 2026-01-12 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-maf-workshop/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

A comprehensive hands-on workshop teaching Microsoft Agent Framework (MAF) in Spanish. Covers 7 modules from basics to advanced patterns: Foundations, Function Tools, Workflows, Observability, Multi-agent with ASP.NET/Aspire, DevUI, and MCP. All code samples in C# with .NET 10, using Azure OpenAI exclusively. Pre-written labs for copy-paste execution, designed for 7-9 hour in-person instructor-led sessions with manual validation checkpoints. Workshop content delivered as markdown documentation plus complete runnable C# projects.

## Technical Context

**Language/Version**: C# with .NET 10  
**Primary Dependencies**: Microsoft Agent Framework 1.0.0-preview.260108.1, .NET Aspire 13.1, Azure OpenAI Service SDK, OpenTelemetry, ASP.NET Core  
**Storage**: Azure AI Agent Service (for workflow state/persistence), file-based markdown documentation  
**Testing**: Manual validation by instructor at checkpoints  
**Target Platform**: Cross-platform (.NET 10 - Windows, Linux, macOS), Azure cloud services (Azure OpenAI, Azure Monitor, Azure AI Agent Service)  
**Project Type**: Workshop/Educational content - Documentation (Markdown) + Code Samples (C# console apps and ASP.NET applications)  
**Performance Goals**: Workshop-appropriate - agent response times acceptable for learning (3-10s latency for LLM calls), no production performance requirements  
**Constraints**: Azure subscription required (mandatory), Spanish language for all content, pre-written copy-paste code only (no participant coding), 7-9 hour in-person workshop duration  
**Scale/Scope**: 7 modules covering foundations to advanced patterns, 10-20 hands-on labs, target 15-30 participants per session with instructor validation checkpoints

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Status**: ✅ PASS (constitution template not yet customized for this project)

**Analysis**: The current constitution.md is a template without specific principles defined. Once project-specific constitution is established, will re-evaluate against principles such as:

- **Testability**: Workshop labs must be executable and validatable (covered via integration tests for Azure connectivity + workshop-validation tests)
- **Documentation**: All content in markdown, Spanish language requirement satisfied
- **Simplicity**: Pre-written copy-paste code minimizes complexity for participants
- **Structure**: Clear modular organization by learning outcomes

**No violations identified** - educational/workshop projects have different constraints than production software.

---

**Post-Phase 1 Re-evaluation** (2026-01-12):

**Design Review**:
- ✅ **Validation**: Manual instructor checkpoints defined for each module to verify participant success
- ✅ **Documentation-First**: research.md, data-model.md, contracts/, quickstart.md all generated before implementation
- ✅ **Clear Structure**: 7 modules with progressive complexity, well-defined entities and relationships
- ✅ **Standards Compliance**: Content format standards documented in contracts/workshop-content-format.md
- ✅ **Instructor Support**: Comprehensive quickstart.md with pre-workshop, day-of, and post-workshop guidance

**Complexity Assessment**: Educational workshop content is inherently documentation-heavy but well-organized:
- Each module is self-contained
- Labs have complete pre-written solutions (spec requirement)
- Clear progression from fundamentals to advanced topics
- No unnecessary abstraction layers

**Conclusion**: Design passes constitution check. No violations requiring justification.

**Complexity Justification**: N/A - no constitution violations requiring justification.

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
# Workshop content structure (documentation-heavy educational project)
docs/
├── modulo-01-fundamentos/
│   ├── README.md                    # Theory: What is MAF, agent types, orchestration
│   ├── instalacion.md               # Setup: .NET 10, MAF, Azure OpenAI
│   └── labs/
│       └── 01-hello-agent/          # C# console app demo
├── modulo-02-function-tools/
│   ├── README.md                    # Theory: Function tools, composition
│   └── labs/
│       ├── 01-custom-tool/          # C# project with custom function
│       ├── 02-agent-as-tool/        # Composing agents
│       └── 03-human-approval/       # Human-in-the-loop example
├── modulo-03-workflows/
│   ├── README.md                    # Theory: Workflow types, Planner+Executor
│   └── labs/
│       ├── 01-sequential/           # Sequential workflow C# project
│       ├── 02-parallel/             # Parallel workflow C# project
│       ├── 03-delegation/           # Delegation pattern C# project
│       ├── 04-group-chat/           # Group chat C# project
│       └── 05-azure-agent-service/  # Azure AI Agent Service integration
├── modulo-04-observability/
│   ├── README.md                    # Theory: Metrics, traces, logs
│   └── labs/
│       ├── 01-metrics-tokens/       # OpenTelemetry metrics C# project
│       ├── 02-distributed-traces/   # Tracing example
│       └── 03-azure-monitor/        # Dashboard + alerts setup
├── modulo-05-aspnet-aspire/
│   ├── README.md                    # Theory: MAF + ASP.NET + Aspire
│   └── labs/
│       └── 01-multi-agent-web/      # Full ASP.NET + Aspire solution
│           ├── AppHost/             # Aspire orchestration project
│           ├── WebApi/              # ASP.NET API exposing agents
│           └── AgentServices/       # MAF agent services library
├── modulo-06-devui/
│   ├── README.md                    # Theory: DevUI capabilities, debugging
│   └── labs/
│       └── 01-devui-setup/          # DevUI configuration and usage guide
└── modulo-07-mcp/
    ├── README.md                    # Theory: Model Context Protocol
    └── labs/
        └── 01-mcp-integration/      # MCP + MAF integration example

instructor-guide/
├── setup-checklist.md                # Pre-workshop environment prep
├── timing-schedule.md                # Module timing + breaks
└── validation-checkpoints.md         # Checkpoints for participant success metrics
```

**Structure Decision**: Workshop/Educational structure chosen - content organized by learning modules with theory (markdown docs) paired with hands-on labs (complete C# projects). Each module is self-contained with README theory and `/labs/` subdirectory for runnable code.

## Complexity Tracking

**No complexity violations** - Workshop content follows educational best practices with clear structure and progressive learning path. No justification needed.

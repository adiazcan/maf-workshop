# MAF Workshop Constitution

## Core Principles

### I. Spanish-Language Content (NON-NEGOTIABLE)

**All workshop materials MUST be in Spanish:**
- Theory documentation, lab instructions, and comments in Spanish
- Code identifiers may remain in English (standard C# conventions)
- Technical terms without Spanish translation are acceptable ("kernel", "prompt")
- Error messages and troubleshooting explanations in Spanish

### II. Copy-Paste Ready Code (NON-NEGOTIABLE)

**All code examples MUST be executable without modification:**
- Complete, tested C# projects with all dependencies specified
- No placeholder code requiring participant completion
- Configuration via appsettings.json + user secrets (no hardcoded values)
- Every lab validated on Windows, macOS, and Linux before publication

### III. Manual Validation Acceptable

**Workshop uses instructor-led validation instead of automated tests:**
- Checkpoints defined with clear success criteria (e.g., "90% see expected output")
- Validation methods: show of hands, screen sharing, pair verification
- Instructor observes and confirms completion before proceeding
- No automated testing infrastructure required (educational context)

### IV. Azure OpenAI Exclusive

**All labs MUST use Azure OpenAI Service:**
- No local models, no OpenAI direct API, no alternative providers
- Consistent with enterprise requirements and spec mandate
- Azure subscription is prerequisite communicated in pre-work
- Cost estimates provided in instructor guide

### V. Progressive Complexity

**Content builds incrementally to respect cognitive load:**
- Each module adds exactly ONE major concept
- Simple labs (10-15 min) before complex labs (30-40 min)
- Success rates: 90% (Module 1) → 70% (Module 5) - realistic expectations
- Each module independently testable after foundational phase

## Content Standards

### Documentation Format

- All markdown follows standards in `contracts/workshop-content-format.md`
- Theory docs: minimum 3 code examples, 1 architecture diagram (Mermaid)
- Lab instructions: 6-step structure (preparation, installation, configuration, implementation, execution, validation)
- Troubleshooting: minimum 3 common errors with solutions

### Code Quality

- Target framework: net10.0 exclusively
- Microsoft.AI.Agents version: 1.0.0-preview.260108.1 (consistent across all labs)
- Spanish comments mandatory: file header, section markers, non-obvious logic

### Timing Accuracy

- Lab time estimates validated with timer during testing
- Module duration = theory + labs + checkpoints + 5-10 min buffer
- Total workshop: 7-9 hours including breaks
- Instructor guides document pacing adjustments

## Workshop-Specific Constraints

### Prerequisites

- Azure subscription with Azure OpenAI access (mandatory, no exceptions)
- .NET 10 SDK installed and verified
- VS Code or equivalent editor
- Pre-workshop environment verification script provided

### Delivery Format

- In-person instructor-led sessions only
- Instructor present for all checkpoints
- Manual validation at defined intervals
- Participants progress as cohort (not self-paced)

## Governance

**This constitution applies specifically to workshop content (educational materials), not production software.**

All lab implementations must:
- Verify compliance with Spanish language requirement
- Execute successfully without modification (copy-paste ready)
- Use Azure OpenAI exclusively (audit in Phase 10)
- Follow content format standards

Amendments require:
- Documentation of rationale
- Update to all affected modules
- Re-validation of timing estimates

**Version**: 1.0.0 | **Ratified**: 2026-01-12 | **Last Amended**: 2026-01-12

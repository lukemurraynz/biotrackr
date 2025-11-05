---
description: Unified workflow orchestrator for GitHub issues, SpecKit development, commits, and PRs
tools: ['github', 'runCommands', 'search', 'changes']
---

# Unified Development Workflow

Execute the complete feature development lifecycle or any individual component in flexible order: issue retrieval → specification → planning → implementation → commit → PR.

## Core Capabilities

**GitHub Issues**: Retrieve, filter, analyze, assign issues via `github` MCP server
**SpecKit**: Complete spec-to-implementation workflow (specify, clarify, plan, checklist, tasks, implement, analyze)
**Commits**: Generate conventional commits with DCO sign-off and AI tracking
**Pull Requests**: Create PRs with template compliance and change analysis

Invoke workflows in any order based on user needs and development phase.

## Workflow Dispatch

### GitHub Issue Operations

**When user asks about issues** (view, find, list, analyze):
1. Use `github` MCP server with fixed context: owner="willvelida", repo="biotrackr"
2. Filter by: state, labels, assignee, milestone, sort order
3. Output: Markdown table with summary and analysis
4. **Single issue view**: Ask "Assign yourself?" → "Start spec kit workflow?"
5. Reference: `.github/prompts/github.get-issue.prompt.md`

---

### SpecKit Workflows

**When user wants feature development**:

**`/speckit.specify`** - Create specification:
- Generate short name (2-4 words), check existing branches
- Run `.specify/scripts/powershell/create-new-feature.ps1 -Json`
- Create spec.md with requirements (max 3 [NEEDS CLARIFICATION])
- Generate requirements checklist
- Make informed guesses, document assumptions

**`/speckit.clarify`** - Resolve ambiguities:
- Run `.specify/scripts/powershell/check-prerequisites.ps1 -Json -PathsOnly`
- Sequential questioning (max 5 questions, one at a time)
- Provide recommended answers with reasoning
- Update spec immediately after each answer

**`/speckit.plan`** - Generate implementation plan:
- Run `.specify/scripts/powershell/setup-plan.ps1 -Json`
- Phase 0: Research (resolve unknowns)
- Phase 1: Design (data-model, contracts, quickstart)
- Update agent context via script

**`/speckit.checklist`** - Validate requirements quality:
- Run `.specify/scripts/powershell/check-prerequisites.ps1 -Json`
- Generate "unit tests for requirements" (NOT implementation tests)
- Ask: "Are [X] defined/specified?" not "Verify [X] works"
- ≥80% traceability to spec sections

**`/speckit.tasks`** - Generate task list:
- Organize by user story (from spec.md)
- Format: `- [ ] [TaskID] [P?] [Story?] Description with file path`
- Phase structure: Setup → Foundational → User Stories → Polish

**`/speckit.implement`** - Execute tasks:
- Run `.specify/scripts/powershell/check-prerequisites.ps1 -Json -RequireTasks -IncludeTasks`
- Check checklist status (ask to proceed if incomplete)
- Phase-by-phase execution, mark tasks [X] when complete

**`/speckit.analyze`** - Validate consistency:
- Run `.specify/scripts/powershell/check-prerequisites.ps1 -Json -RequireTasks -IncludeTasks`
- READ-ONLY cross-artifact analysis
- Detect: duplication, ambiguity, coverage gaps, constitution violations
- Max 50 findings, severity: CRITICAL/HIGH/MEDIUM/LOW

---

### Commit Message Generation

**When user wants to commit**:
1. Check repo root, active branch, staged changes
2. Extract issue number from branch: `{type}/gh-{issue}-{slug}`
3. If issue found, retrieve context via GitHub issue workflow
4. Ask user (one at a time):
   - AI model (1-9: None, GPT-4.1, GPT-4o, Claude Haiku 4.5, Claude Sonnet 4, Claude Sonnet 4.5, Gemini 2.5 Pro, GPT-5-Codex, o4-mini)
   - Contribution (1-4: code-generation, refactoring, documentation, test-generation)
5. Compose: `{type}({scope}): {description}` + bullets + trailers
6. **Always use `-s` flag for DCO**
7. Execute automatically: `git commit -s -m "..." --trailer "..." --trailer "..."`

<!-- <example-commit-command> -->
```bash
git commit -s -m "feat(weight-svc): add integration tests

- Implement E2E tests with Cosmos DB Emulator
- Add test isolation with container cleanup" \
  --trailer "agent: github-copilot" \
  --trailer "model: Claude Sonnet 4.5" \
  --trailer "contribution: test-generation"
```
<!-- </example-commit-command> -->

Reference: `.github/prompts/util.commit-message.prompt.md`

---

### Pull Request Creation

**When user wants to create PR**:
1. Execute Git commands (PowerShell): branch name, commits, changed files, conflicts
2. Extract branch info: type, issue number from `{type}/gh-{issue}-{slug}`
3. Read PR template: `.github/templates/pull-request-template.md`
4. Read changed files to identify modifications
5. Generate PR body with emoji categorization (🐛 fix, ✨ feat, 📚 docs, etc.)
6. **MANDATORY: ≤4000 characters total**
7. Show preview with character count
8. Confirm with user
9. Call `github` MCP server: `method: "create"`, owner="willvelida", repo="biotrackr"

Reference: `.github/prompts/github.raise-pr.prompt.md`

---

## Workflow Chaining

**Automatic suggestions** (offer, don't force):
- Issue assignment → Offer `/speckit.specify`
- Specify complete → Suggest `/speckit.clarify` (if ambiguities)
- Clarify complete → Suggest `/speckit.plan`
- Plan complete → Suggest `/speckit.checklist` and `/speckit.tasks`
- Tasks complete → Suggest `/speckit.analyze` then `/speckit.implement`
- Implement complete → Suggest commit
- Commit complete → Suggest PR

**User controls order**: Any workflow can be invoked independently, skipped, or repeated.

---

## Context Sharing

**Issue → SpecKit**: Issue title/body inform feature description, labels inform categorization
**SpecKit → Commit**: Branch name provides issue ref, feature context informs scope
**SpecKit → PR**: tasks.md informs changes, spec.md provides context
**Commit → PR**: Commits analyzed for summary, messages inform PR body

---

## Common Patterns

<!-- <patterns-usage> -->
**Complete lifecycle**:
```text
List issues → View issue #42 → Assign + start spec → Clarify → Plan → Tasks → Implement → Commit → PR
```

**Quick fix**:
```text
List bugs → View issue #55 → (Make changes) → Commit → PR
```

**Planning only**:
```text
Specify → Clarify → Plan → Checklist → Analyze → (Stop, no implementation)
```

**Direct implementation**:
```text
(Already on branch with changes) → Commit → PR
```
<!-- </patterns-usage> -->

---

## Reference Sources

<!-- <reference-sources> -->
**Component Prompts**:
- `.github/prompts/github.get-issue.prompt.md` - Issue retrieval
- `.github/prompts/github.raise-pr.prompt.md` - PR creation
- `.github/prompts/speckit.specify.prompt.md` - Feature specification
- `.github/prompts/speckit.clarify.prompt.md` - Ambiguity resolution
- `.github/prompts/speckit.plan.prompt.md` - Implementation planning
- `.github/prompts/speckit.checklist.prompt.md` - Requirements validation
- `.github/prompts/speckit.tasks.prompt.md` - Task generation
- `.github/prompts/speckit.implement.prompt.md` - Task execution
- `.github/prompts/speckit.analyze.prompt.md` - Consistency analysis
- `.github/prompts/util.commit-message.prompt.md` - Commit generation

**Templates**: `.github/templates/`, `.specify/templates/`
**Standards**: `docs/standards/commit-standards.md`, `.specify/memory/constitution.md`
**Scripts**: `.specify/scripts/powershell/*.ps1`
<!-- </reference-sources> -->
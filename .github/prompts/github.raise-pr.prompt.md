---
description: Automate pull request creation for Biotrackr using GitHub MCP server with PR template compliance
tools: ['github', 'run_in_terminal', 'read_file']
---

# GitHub Pull Request Creation Prompt

You are an expert at creating well-structured pull requests for the Biotrackr repository (willvelida/biotrackr). You use the GitHub MCP server to create PRs that follow the project's PR template and standards.

<!-- <table-of-contents> -->
## Table of Contents
- [Purpose](#purpose)
- [Repository Context](#repository-context)
- [Core Instructions](#core-instructions)
- [PR Template Requirements](#pr-template-requirements)
- [Branch Naming Conventions](#branch-naming-conventions)
- [Workflow](#workflow)
- [Usage Examples](#usage-examples)
- [Reference Sources](#reference-sources)
- [Best Practices](#best-practices)
<!-- </table-of-contents> -->

---

## Purpose

Create GitHub pull requests programmatically with:
- Automatic branch and commit analysis
- PR template compliance (4000 character limit)
- Related issue linking
- Change categorization and emoji formatting
- Base branch detection and validation

---

## Repository Context

<!-- <important-repository-context> -->
**Repository Information:**
- **Owner:** willvelida
- **Repository:** biotrackr
- **URL:** https://github.com/willvelida/biotrackr
- **Default Branch:** main
- **Access:** Public repository (requires authentication for PR creation)
- **PR Template:** `.github/templates/pull-request-template.md`
- **Branch Pattern:** `{type}/{slug}` or `{type}/gh-{issue}-{slug}`
<!-- </important-repository-context> -->

---

## Core Instructions

<!-- <important-core-instructions> -->
**You MUST follow these instructions when creating PRs:**

1. **Always use GitHub MCP server tools** for all PR operations:
   - Tool: `mcp_github_pull_request_write` with `method: "create"`
   - Required parameters: `owner`, `repo`, `title`, `body`, `head`, `base`

2. **Repository parameters are fixed:**
   - `owner`: "willvelida" 
   - `repo`: "biotrackr"
   - `base`: "main" (default) or specify target branch

3. **Execute Git commands using `run_in_terminal` tool:**
   - MUST use PowerShell syntax (default shell: pwsh.exe)
   - Get current branch: `git branch --show-current`
   - Get commit history: `git log main..HEAD --oneline`
   - Get changed files: `git diff --name-only main...HEAD`
   - Check merge conflicts: `git diff --check main...HEAD`
   - Handle command failures gracefully

4. **Read files using `read_file` tool:**
   - Read PR template: `.github/templates/pull-request-template.md`
   - Read changed files to understand modifications
   - Read commit standards: `docs/standards/commit-standards.md`
   - Analyze file content for accurate change descriptions

5. **Analyze current state before creating PR:**
   - Execute Git commands to gather branch and commit info
   - Extract type and optional issue number from branch name
   - Read changed files to identify key modifications
   - Identify functions/classes modified in each file

6. **Follow PR template structure exactly:**
   - Read template file to ensure compliance
   - Use emoji-based change categorization
   - Keep total description under 4000 characters (MANDATORY)
   - Include all template sections
   - Link related issues properly

7. **Handle edge cases gracefully:**
   - Warn if no commits exist on branch (check Git output)
   - Detect merge conflicts before creating PR (git diff --check)
   - Validate base branch exists
   - Check if PR already exists for branch
   - Handle Git command failures with clear error messages

8. **Interactive workflow:**
   - Show PR preview before creation
   - Confirm with user before submitting
   - Provide PR URL after successful creation
<!-- </important-core-instructions> -->

---

## PR Template Requirements

<!-- <schema-pr-template> -->
**Template Structure (from `docs/templates/pull-request-template.md`):**

```markdown
# 📝 Description
[Brief description of the changes in this PR]

## 🔗 Related Issues
[Link to any related issues or work items]

## 🚀 Changes
[List each change with emoji, type, and details]

**[emoji] [type](Short description)**  
What: [Brief description of what was changed]
Why: [Brief description of Why was this change made]  
📁 Files: `file1.ext` (`function1`, `function2`), `file2.ext` (config updates)

## 🙏 Additional Context
[Add any other context about the PR here]
```

**Change Type Emojis:**
- 🐛 `fix` - Bug fix
- ✨ `feat` - New feature
- 💥 `breaking` - Breaking change
- 📚 `docs` - Documentation
- 🔧 `config` - Configuration
- 🧹 `refactor` - Refactoring
- 🔒 `security` - Security fix
- ⚡ `perf` - Performance improvement

**Character Limit:** MANDATORY 4000 characters maximum (including all text, spaces, newlines, emojis, special characters)
<!-- </schema-pr-template> -->

---

## Branch Naming Conventions

<!-- <patterns-branch-naming> -->
**Format Patterns:**

| Pattern | Example | Issue Ref | Description |
|---------|---------|-----------|-------------|
| `{type}/{slug}` | `feat/add-cleanup-specialist-agent` | No | Simple feature branch |
| `{type}/gh-{issue}-{slug}` | `fix/gh-42-null-token-handling` | Yes (#42) | Branch linked to issue |

**Common Types:**
- `feat` - New features
- `fix` - Bug fixes
- `docs` - Documentation updates
- `test` - Test additions/changes
- `refactor` - Code refactoring
- `chore` - Maintenance tasks
- `ci` - CI/CD changes

**Issue Extraction:**
```bash
# Extract issue number from branch name
branch="feat/gh-42-cosmos-integration"
[[ $branch =~ gh-([0-9]+) ]] && issue_num=${BASH_REMATCH[1]}
# Result: issue_num=42
```
<!-- </patterns-branch-naming> -->

---

## Workflow

<!-- <important-workflow> -->
**Step-by-Step PR Creation Process:**

### 1. Pre-Creation Analysis

**Use `run_in_terminal` to execute Git commands:**

```powershell
# PowerShell (default shell)
git branch --show-current
git log main..HEAD --oneline
git diff --name-only main...HEAD
git diff --check main...HEAD
```

**Example tool invocation:**
```json
{
  "command": "git branch --show-current",
  "explanation": "Get current branch name for PR head reference",
  "isBackground": false
}
```

### 2. Extract Branch Information
- Parse branch name for type and issue number
- Determine change category (feat, fix, docs, etc.)
- Extract human-readable slug

### 3. Analyze Changes

**Use `read_file` to understand modifications:**
- Read each changed file to identify specific changes
- Extract function/class names that were modified
- Group changed files by directory/component
- Categorize changes by impact:
  - Core functionality
  - Tests
  - Documentation
  - Configuration
  - Infrastructure

**Example tool invocation:**
```json
{
  "filePath": "/path/to/changed/file.cs",
  "limit": 100,
  "offset": 1
}
```

### 4. Generate PR Body
- **Description Section:** 2-3 sentences summarizing the PR
- **Related Issues:** Link issues from branch name or commit messages
- **Changes Section:** List each major change with:
  - Appropriate emoji for change type
  - What changed (technical details)
  - Why it changed (business value/reason)
  - Files affected with specific functions/areas
- **Additional Context:** Optional section for:
  - Breaking changes
  - Migration notes
  - Testing instructions
  - Screenshots/demos

### 5. Generate PR Title
```text
Format: {type}({scope}): {brief description}
Examples:
  - feat(weight-svc): add Cosmos DB integration tests
  - fix(auth-svc): handle null token gracefully
  - docs(decision-records): document service lifetime patterns
```

### 6. Create PR
```json
{
  "method": "create",
  "owner": "willvelida",
  "repo": "biotrackr",
  "title": "{generated_title}",
  "body": "{generated_body}",
  "head": "{current_branch}",
  "base": "main"
}
```

### 7. Post-Creation
- Display PR URL
- Show PR number
- Confirm GitHub Actions workflows triggered
- Provide next steps (review, CI status)
<!-- </important-workflow> -->

---

## Tool Usage

<!-- <important-tool-usage> -->
**Execute Git Commands:**

Always use `run_in_terminal` for Git operations:

```json
// Get current branch
{
  "command": "git branch --show-current",
  "explanation": "Get current branch name",
  "isBackground": false
}

// Get commit history
{
  "command": "git log main..HEAD --oneline --format='%h %s'",
  "explanation": "Get commits since branching from main",
  "isBackground": false
}

// Get changed files
{
  "command": "git diff --name-only main...HEAD",
  "explanation": "List all files changed in this branch",
  "isBackground": false
}

// Check for merge conflicts
{
  "command": "git diff --check main...HEAD",
  "explanation": "Detect potential merge conflicts",
  "isBackground": false
}
```

**Read Files:**

Use `read_file` to analyze changed files and templates:

```json
// Read PR template
{
  "filePath": "c:\\Users\\velidawill\\Documents\\OpenSource\\biotrackr\\.github\\templates\\pull-request-template.md"
}

// Read changed file
{
  "filePath": "c:\\Users\\velidawill\\Documents\\OpenSource\\biotrackr\\src\\Biotrackr.Weight.Svc\\Services\\WeightService.cs",
  "limit": 200,
  "offset": 1
}

// Read commit standards
{
  "filePath": "c:\\Users\\velidawill\\Documents\\OpenSource\\biotrackr\\docs\\standards\\commit-standards.md"
}
```

**Error Handling:**

```powershell
# Handle Git command failures
$branch = git branch --show-current 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to get current branch: $branch"
    exit 1
}
```
<!-- </important-tool-usage> -->

---

## Usage Examples

<!-- <example-create-simple-pr> -->
**Example 1: Create PR for current branch**

User Request: "Create a pull request for my current changes"

Your Actions:
1. Execute `run_in_terminal`: `git branch --show-current` → `feat/add-cleanup-specialist-agent`
2. Execute `run_in_terminal`: `git log main..HEAD --oneline` → Analyze commit messages
3. Execute `run_in_terminal`: `git diff --name-only main...HEAD` → Get changed files
4. Execute `read_file` for each changed file → Identify specific modifications
5. Read PR template: `read_file` → `.github/templates/pull-request-template.md`
6. Extract branch info: type=`feat`, slug=`add-cleanup-specialist-agent`
7. Generate PR body following template
8. Call GitHub MCP server with PR details

Expected Output:
```markdown
**PR Title:** feat(agents): add cleanup specialist agent

**Preview:**

# 📝 Description
Adds a new cleanup specialist agent that identifies code maintenance opportunities and creates GitHub issues for technical debt.

## 🔗 Related Issues
N/A

## 🚀 Changes

**✨ feat(New agent implementation)**  
What: Created cleanup-specialist.agent.md with issue creation capabilities
Why: Automate technical debt identification and tracking
📁 Files: `.github/agents/cleanup-specialist.agent.md`

## 🙏 Additional Context
- Uses `mcp_github_issue_write` tool for issue creation
- Includes cleanup categories and prioritization logic
- Documentation includes examples and best practices

**Character Count:** 487/4000

Proceed with PR creation? (yes/no)
```
<!-- </example-create-simple-pr> -->

<!-- <example-create-pr-with-issue> -->
**Example 2: Create PR for branch with GitHub issue**

User Request: "Raise a PR for the current branch"

Branch Name: `fix/gh-42-null-token-handling`

Your Actions:
1. Detect issue number: #42
2. Retrieve issue details using `github-issue-retrieval.prompt.md`
3. Extract issue context (title, labels, description)
4. Generate PR with issue reference
5. Call GitHub MCP server

Expected Output:
```markdown
**PR Title:** fix(auth-svc): handle null token gracefully (fixes #42)

**Preview:**

# 📝 Description
Resolves null reference exceptions when tokens are null or empty by adding proper validation.

## 🔗 Related Issues
- Fixes #42

## 🚀 Changes

**🐛 fix(Null token handling)**  
What: Added null check before token validation in AuthService
Why: Prevents NullReferenceException during authentication flow
📁 Files: `src/Biotrackr.Auth.Svc/Services/AuthService.cs` (`ValidateToken`)

**✨ feat(Add unit tests)**  
What: Added test cases for null/empty token scenarios
Why: Ensure robust handling of edge cases
📁 Files: `src/Biotrackr.Auth.Svc.UnitTests/Services/AuthServiceTests.cs` (`ValidateToken_NullToken_ThrowsException`)

## 🙏 Additional Context
- Breaking change: Now throws `ArgumentNullException` instead of returning false
- All existing tests passing with updated behavior

**Character Count:** 821/4000

Proceed with PR creation? (yes/no)
```
<!-- </example-create-pr-with-issue> -->

<!-- <example-large-pr-summarization> -->
**Example 3: Summarize large PR to fit character limit**

User Request: "Create PR for comprehensive test coverage work"

Your Actions:
1. Detect extensive changes across multiple test projects
2. Summarize changes by category (unit tests, integration tests, workflows)
3. Use concise language to fit 4000 char limit
4. Group similar files together

Expected Output:
```markdown
**Warning:** Large changeset detected (42 files). Summarizing changes for brevity.

**PR Title:** test(weight-svc): add comprehensive test coverage

**Preview:**

# 📝 Description
Implements complete test suite for Weight Service including unit tests (97% coverage), contract tests, E2E tests with Cosmos DB Emulator, and GitHub Actions workflow integration.

## 🔗 Related Issues
- Closes #12
- Related to #45

## 🚀 Changes

**✨ feat(Unit test coverage)**  
What: Added unit tests for all service and repository classes
Why: Achieve 97% code coverage for critical business logic
📁 Files: `Biotrackr.Weight.Svc.UnitTests/` (8 test classes, 142 test methods)

**✨ feat(Integration tests)**  
What: Added contract and E2E test projects with Cosmos DB Emulator support
Why: Validate service integration and end-to-end workflows
📁 Files: `Biotrackr.Weight.Svc.IntegrationTests/` (Contract + E2E test suites)

**🔧 config(GitHub Actions workflow)**  
What: Created deploy-weight-service.yml with test job orchestration
Why: Automate test execution in CI/CD pipeline
📁 Files: `.github/workflows/deploy-weight-service.yml`

**📚 docs(Test documentation)**  
What: Added README and decision records for test architecture
Why: Document test patterns for future development
📁 Files: `docs/decision-records/`, test project READMEs

## 🙏 Additional Context
- All tests passing locally and in CI
- Cosmos DB Emulator setup documented
- Common issues documented in `.specify/memory/common-resolutions.md`

**Character Count:** 1421/4000

Proceed with PR creation? (yes/no)
```
<!-- </example-large-pr-summarization> -->

---

## Reference Sources

<!-- <reference-sources> -->
**GitHub MCP Server:**
- Official GitHub REST API: https://docs.github.com/en/rest/pulls
- MCP GitHub Server: https://github.com/modelcontextprotocol/servers
- GitHub Pull Requests API: https://docs.github.com/en/rest/pulls/pulls

**Repository Context:**
- Repository: willvelida/biotrackr
- PR Template: `.github/templates/pull-request-template.md`
- Commit Standards: `docs/standards/commit-standards.md`
- Issue Retrieval: `.github/prompts/github.get-issue.prompt.md`
<!-- </reference-sources> -->

---

## Best Practices

<!-- <important-best-practices> -->
**Follow these practices when creating PRs:**

1. **Character Budget Management:**
   - Reserve ~500 chars for template structure
   - Allocate ~1500 chars for changes section
   - Keep description under 300 chars
   - Leave buffer for additional context

2. **Change Categorization:**
   - Group related changes together
   - Use specific file paths and function names
   - Balance technical detail with readability
   - Prioritize most impactful changes first

3. **Issue Linking:**
   - Always check branch name for issue references
   - Use "Fixes #XX" for issues the PR resolves
   - Use "Related to #XX" for tangential issues
   - Retrieve issue context when available

4. **Error Handling:**
   - Check if PR already exists for branch
   - Validate base branch exists
   - Detect merge conflicts before creation
   - Warn if no commits exist on branch

5. **User Experience:**
   - Always show preview before creating
   - Display character count prominently
   - Provide clear next steps after creation
   - Include PR URL and number in response

6. **Workflow Integration:**
   - Mention if CI/CD workflows will trigger
   - Reference relevant GitHub Actions jobs
   - Link to workflow documentation if needed
<!-- </important-best-practices> -->

---

## Common Scenarios

<!-- <patterns-common-scenarios> -->
**Handle these typical requests:**

| User Intent | Actions Required | Output Format |
|-------------|------------------|---------------|
| "Create PR" | Analyze current branch, generate body, confirm | PR preview + confirmation |
| "Raise PR for feat/x" | Switch context to branch, analyze, generate | PR preview for specified branch |
| "Create PR fixing #42" | Link issue, retrieve context, generate with fix reference | PR with issue link |
| "Draft PR" | Same as create but mark as draft | Draft PR confirmation |
| "PR to develop" | Change base branch to "develop" | PR with custom base |

**Branch Analysis Patterns:**
- No commits: "No commits on branch. Stage and commit changes first."
- Merge conflicts: "Merge conflicts detected with main. Resolve conflicts before creating PR."
- PR exists: "PR already exists for this branch: #XX"
- Clean state: Proceed with PR creation
<!-- </patterns-common-scenarios> -->

---

## Validation Checklist

<!-- <important-validation-checklist> -->
**Before creating PR, verify:**

- [ ] Current branch is not `main` or `develop`
- [ ] Branch has at least one commit
- [ ] No merge conflicts with base branch
- [ ] PR body is under 4000 characters
- [ ] All template sections are present
- [ ] Change emojis match change types
- [ ] File paths are accurate
- [ ] Issue references are valid (if present)
- [ ] Title follows `{type}({scope}): {description}` format
- [ ] Description is clear and concise
<!-- </important-validation-checklist> -->

---
mode: agent
description: Generate git commit messages following Biotrackr conventional commit standards with DCO sign-off and AI contribution tracking.
tools: ["runCommands", "search", "changes"]
---

# Generate Git Commit Message

Produce a conventional commit message following Biotrackr standards for the current state of the repository. Prefer staged changes; fall back to all changes only when nothing is staged.

**Critical:** All commits MUST include DCO sign-off using the `-s` flag.

## Pre-Checks

- Stay inside the repo root for all git commands.
- Retrieve the active branch.
- Review the current state for any staged work.
- Review `docs/standards/commit-standards.md` for commit message conventions.

## Required Steps

1. Derive any GitHub issue number from the branch name formatted `{type}/gh-{issue}-{slug}` or `{type}/{slug}`; issue reference is optional.
2. If an issue number is detected, review the issue context by invoking `github-issue-retrieval.prompt.md` with the issue number.
3. Ask the user, one question at a time:

   - Current AI model (present choices 1-9: None, GPT-4.1, GPT-4o, Claude Haiku 4.5, Claude Sonnet 4, Claude Sonnet 4.5, Gemini 2.5 Pro, GPT-5-Codex, o4-mini).

   - Contribution type (choices 1-4: code-generation, refactoring, documentation, test-generation).

4. Compose the message:

- Subject (≤50 chars): `{type}({scope}): {description}` (issue reference optional in description).
- Blank line.
- 1-5 concise bullets explaining key outcomes/benefits (≤72 chars per line). Use fewer bullets for simple changes.
- Blank line.
- Mandatory trailers: `Signed-off-by` (auto-added by `-s`), `agent: github-copilot`, `model: <answer>`, `contribution: <answer>`.

5. Provide the final message and the `git commit -s` command. **Always include the `-s` flag for DCO compliance.** Use a single `-m` flag for the complete message (subject + body with bullet points), then separate `--trailer` flags only for each trailer.

### Commit Message Template

```text
{type}({scope}): {description}

- High-level benefit or change
- Supporting detail

Signed-off-by: Your Name <your.email@example.com>
agent: github-copilot
model: {model}
contribution: {contribution}
```

**Common Biotrackr Scopes:**
- Services: `weight-svc`, `activity-svc`, `sleep-svc`, `food-svc`, `auth-svc`
- APIs: `weight-api`, `activity-api`, `sleep-api`
- Infrastructure: `infra`, `bicep`, `cosmos`, `apim`, `workflows`
- Cross-cutting: `tests`, `ci-cd`, `docker`, `docs`

## Output Requirements

- Highlight if no GitHub issue number was detected (this is acceptable; issues are optional).
- **Always include the `-s` flag** for DCO compliance.
- Format the git command as: `git commit -s -m "subject + body" --trailer "agent: github-copilot" --trailer "model: {model}" --trailer "contribution: {contribution}"`.
- Execute the git commit command automatically. Be sure to stage all changes before executing the command, if there are no changes staged.

## Example Command Format

<!-- <example-commit-commands> -->
```bash
# Simple change example (no AI contribution)
git commit -s -m "fix(auth-svc): handle null token gracefully

- Add null check before token validation"

# Complex change example (with AI contribution)
git commit -s -m "docs(decision-records): document service lifetime patterns

- Added comprehensive guide for DI service lifetimes
- Documented HttpClient service registration patterns
- Included examples for Azure SDK client registration" \
  --trailer "agent: github-copilot" \
  --trailer "model: Claude Sonnet 4.5" \
  --trailer "contribution: documentation"

# With GitHub issue reference (optional)
git commit -s -m "feat(weight-svc): add Cosmos DB integration tests (fixes #42)

- Implement E2E tests with Cosmos DB Emulator
- Add test isolation with container cleanup
- Achieve 97% code coverage" \
  --trailer "agent: github-copilot" \
  --trailer "model: Claude Sonnet 4.5" \
  --trailer "contribution: test-generation"
```
<!-- </example-commit-commands> -->

## Reference Sources

<!-- <reference-sources> -->
- Commit Standards: `docs/standards/commit-standards.md`
- GitHub Issue Retrieval: `github-issue-retrieval.prompt.md`
- Developer Certificate of Origin: https://developercertificate.org/
<!-- </reference-sources> -->

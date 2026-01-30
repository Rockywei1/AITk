# Pull Request Code Review Process

This document describes a systematic approach to code reviewing pull requests with high precision and minimal false positives.

## Review Process

### Step 1: Eligibility Check

Before reviewing, verify the pull request:

- Is not closed or merged
- Is not a draft
- Is not an automated/trivial change that doesn't need review
- Has not already been reviewed by you

If any of these conditions are true, skip the review.

### Step 2: Gather Context

Collect relevant project guidelines:

- Find the root guidelines file (CLAUDE.md, CONTRIBUTING.md, etc.)
- Find any guidelines files in directories modified by the PR
- Read and understand the project's coding standards

### Step 3: Summarize the Change

Create a brief summary of what the PR is trying to accomplish:

- What feature/fix is being implemented?
- Which files are affected?
- What is the scope of the change?

### Step 4: Multi-Perspective Review

Review the change from multiple angles:

1. **Guidelines Compliance**: Audit changes against project guidelines
2. **Bug Detection**: Shallow scan for obvious bugs in the changed lines
3. **Historical Context**: Check git blame/history for relevant context
4. **Previous Feedback**: Review comments on previous PRs touching these files
5. **Code Comments**: Ensure changes comply with inline guidance

### Step 5: Confidence Scoring

For each issue found, assign a confidence score (0-100):

- **0**: Not confident - false positive or pre-existing issue
- **25**: Somewhat confident - may be false positive, couldn't verify
- **50**: Moderately confident - real but minor issue
- **75**: Highly confident - verified issue that will impact functionality
- **100**: Absolutely certain - confirmed critical issue

### Step 6: Filter Results

Only report issues with confidence score ≥ 80.

### Step 7: Format Output

If issues were found:

```
### Code review

Found N issues:

1. [Description] (Guideline: "[quote]")
   [Link to file and line]

2. [Description] (Bug: [explanation])
   [Link to file and line]
```

If no issues:

```
### Code review

No issues found. Checked for bugs and guideline compliance.
```

## False Positives to Avoid

- Pre-existing issues not introduced by this PR
- Issues that look like bugs but aren't
- Pedantic nitpicks
- Issues that linters/compilers would catch (CI handles these)
- General quality issues not explicitly required
- Intentional functionality changes
- Issues on unmodified lines

## Notes

- Do not attempt to build or typecheck - CI handles this
- Use project tooling to interact with the PR
- Create a todo list before starting
- Always cite and link to specific code locations
- Provide full file paths with line numbers for context

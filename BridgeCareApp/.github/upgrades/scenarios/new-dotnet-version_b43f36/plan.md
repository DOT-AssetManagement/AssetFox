# .NET 8 upgrade plan for `AssetFox.CalculateEvaluate`

## 1. Planning summary

This plan covers a focused upgrade of `AssetFox.CalculateEvaluate/AssetFox.CalculateEvaluate.csproj` from `net6.0` to `net8.0`.

The assessment indicates this is a low-risk migration with one direct project file change, two package-reference removals, and targeted validation through the existing `AssetFox.CalculateEvaluate.Testing` test project.

## 2. Scope

### In scope

- update `AssetFox.CalculateEvaluate/AssetFox.CalculateEvaluate.csproj` from `net6.0` to `net8.0`
- remove explicit package references that are no longer needed on newer .NET:
  - `System.Net.Http`
  - `System.Text.RegularExpressions`
- rebuild and validate the upgraded project against the existing `net8.0` test project
- investigate `Antlr4.Runtime.Standard` only if the initial upgrade introduces compile-time or runtime failures

### Out of scope

- broad solution-wide retargeting to `net8.0`
- package modernization outside the needs of this project upgrade
- ANTLR grammar regeneration unless required by a follow-up runtime or compatibility issue
- unrelated dependency cleanup in referenced projects

## 3. Inputs used for planning

### Assessment findings

- `Project.0002`: target framework needs to be changed
- `NuGet.0003`: package functionality is included with framework reference
- `Api.0002`: source incompatible for selected .NET version

### Current project state

`AssetFox.CalculateEvaluate/AssetFox.CalculateEvaluate.csproj` currently contains:

- `TargetFramework` = `net6.0`
- `Antlr4.Runtime.Standard` `4.8.0`
- `System.Net.Http` `4.3.4`
- `System.Text.RegularExpressions` `4.3.1`

### Dependency and compatibility context

Referenced projects are already compatible consumers for a `net8.0` library:

- `AssetFox.Choice` targets `netstandard2.0`
- `AssetFox.Comparison` targets `netstandard2.0`
- `AssetFox` targets `netstandard2.1`

Validation target already exists:

- `AssetFox.CalculateEvaluate.Testing/AssetFox.CalculateEvaluate.Testing.csproj` targets `net8.0`
- prior validation showed solution build success and `34/34` passing tests under `.NET 8`

## 4. Branch and workspace prerequisites

### Current branch state

- repository root: `C:\Code\Infrastructure Asset Management`
- current branch: `wj/updateDeprecatedRefs`
- proposed upgrade branch from workflow initialization: `upgrade-to-NET8`
- pending changes detected: `True`

### Planning conclusion for branch alignment

The current branch does **not** match the proposed dedicated upgrade branch.

Before execution begins, the execution agent should:

1. preserve or commit current pending changes
2. create or switch to the dedicated upgrade branch `upgrade-to-NET8`
3. confirm the upgrade work is isolated before editing the project file

If branch switching is intentionally deferred by the user, execution may continue on the current branch, but that should be treated as an explicit deviation from plan.

## 5. Recommended migration strategy

Use a **minimal-change, validate-immediately** strategy.

Rationale:

- only one production project is being retargeted
- direct project references are already compatible
- the associated test project already runs on `net8.0`
- analyzer concerns are limited and do not yet identify a concrete blocking source file

This means the first implementation pass should avoid opportunistic package upgrades and limit changes to the smallest set required for a successful `net8.0` build and test pass.

## 6. Execution sequence

### Phase 1 - Prepare execution context

1. confirm the working tree state and handle pending changes
2. switch to or create `upgrade-to-NET8`
3. verify that the .NET 8 SDK required by the solution is available if not already confirmed during execution startup

**Exit criteria:**

- upgrade work is isolated on the intended branch
- no ambiguous workspace state remains

### Phase 2 - Retarget the project

Modify `AssetFox.CalculateEvaluate/AssetFox.CalculateEvaluate.csproj` as follows:

1. change:
   - from: `<TargetFramework>net6.0</TargetFramework>`
   - to: `<TargetFramework>net8.0</TargetFramework>`
2. remove:
   - `<PackageReference Include="System.Net.Http" Version="4.3.4" />`
   - `<PackageReference Include="System.Text.RegularExpressions" Version="4.3.1" />`
3. keep `Antlr4.Runtime.Standard` `4.8.0` unchanged for the first pass

**Exit criteria:**

- project file reflects `net8.0`
- framework-included package references are removed
- no additional package churn has been introduced

### Phase 3 - Build validation

Run targeted validation in this order:

1. build `AssetFox.CalculateEvaluate/AssetFox.CalculateEvaluate.csproj`
2. if targeted build is clean, run a broader workspace or solution build if needed by the execution flow
3. inspect errors specifically for:
   - removed `System.*` package assumptions
   - API compatibility issues associated with analyzer rule `Api.0002`
   - ANTLR runtime or parsing-related compile failures

**Exit criteria:**

- project builds successfully on `net8.0`, or
- blocking errors are reduced to a small identified set for remediation

### Phase 4 - Test validation

Run the existing test project:

- `AssetFox.CalculateEvaluate.Testing`

Primary success signal:

- all tests pass on `net8.0`

If tests fail, classify failures into one of these buckets:

1. retargeting/build configuration issue
2. behavior change caused by BCL/runtime differences
3. ANTLR runtime compatibility issue

**Exit criteria:**

- test suite passes, or
- failures are categorized with a concrete remediation path

### Phase 5 - Conditional remediation only if validation fails

Only perform this phase if Phases 3 or 4 expose issues.

#### Option A - Source/API remediation

If the retarget introduces compile errors or runtime changes:

- fix only the affected source locations
- prefer localized API substitutions over broader refactors
- keep changes constrained to compatibility fixes

#### Option B - ANTLR package review

If failures point to `Antlr4.Runtime.Standard` behavior or compatibility:

1. evaluate updating `Antlr4.Runtime.Standard`
2. validate package compatibility before applying the update
3. rebuild and rerun tests after any package change
4. regenerate grammar artifacts only if required by the package change

This package review is a fallback path, not part of the first-pass implementation.

## 7. Validation checklist

Execution should record the following checkpoints:

- [ ] branch is correct for upgrade execution
- [ ] pending changes handled before edits
- [ ] `AssetFox.CalculateEvaluate.csproj` targets `net8.0`
- [ ] `System.Net.Http` reference removed
- [ ] `System.Text.RegularExpressions` reference removed
- [ ] project build succeeds
- [ ] solution or workspace build succeeds if run
- [ ] `AssetFox.CalculateEvaluate.Testing` passes on `.NET 8`
- [ ] no follow-up ANTLR action required, or follow-up work explicitly documented

## 8. Risks and mitigations

### Risk: analyzer-reported source incompatibility without exact location

**Mitigation:**
- retarget first, then use build output to identify the exact file and API surface needing change
- keep the first pass minimal so any resulting issue is easy to isolate

### Risk: removal of explicit `System.*` packages exposes hidden dependency assumptions

**Mitigation:**
- remove only the two framework-included package references flagged by assessment
- validate immediately with build plus tests

### Risk: old ANTLR runtime package behaves differently on `net8.0`

**Mitigation:**
- do not change ANTLR in the first pass
- treat ANTLR update as a controlled fallback only if build or tests show package-related issues

### Risk: branch/worktree contamination from existing pending changes

**Mitigation:**
- require branch alignment and pending-change handling before execution edits begin

## 9. Definition of done

The upgrade is complete when all of the following are true:

1. `AssetFox.CalculateEvaluate` targets `net8.0`
2. redundant `System.Net.Http` and `System.Text.RegularExpressions` references are removed
3. the project builds successfully
4. `AssetFox.CalculateEvaluate.Testing` passes on `.NET 8`
5. any required remediation is limited, documented, and included in the execution log

## 10. Recommended first execution task

Start with a single atomic change set:

1. update `AssetFox.CalculateEvaluate/AssetFox.CalculateEvaluate.csproj`
2. build
3. run `AssetFox.CalculateEvaluate.Testing`

If that succeeds, stop there. Do not add optional package upgrades in the same change set.

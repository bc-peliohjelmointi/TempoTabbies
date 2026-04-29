# High/Medium Issue Fix Instructions (Documentation Only)

This document gives explicit implementation instructions for all **High** and **Medium** findings in:
- `README-review-security.md`
- `README-review-functional.md`
- `README-review-good-practices.md`

---

## Execution order

1. Fix all **High** issues first.
2. Then fix **Medium** issues in this order:
   - reliability/data integrity
   - security hardening
   - maintainability/performance
3. After each issue, run a focused manual regression on the exact touched flow.

---

## HIGH-1: Path traversal in profile JSON save/load

**Source issue:** `README-review-security.md`  
**Files to update:** `TempoTabbies/Assets/Scripts/JSON_Stuff.cs`

### Required changes

1. Add one helper to validate and normalize profile names before file access.
2. Enforce an allowlist for names (letters, numbers, `_`, `-`; optionally space if desired).
3. Reject any name containing path separators or path traversal patterns.
4. Use `Path.Combine("JSON", safeName + ".json")` for all profile file paths.
5. Use the same sanitizer in **all** profile read/write methods (save + both load paths).
6. If invalid, do not continue file I/O; log a clear error and return safely.

### Done criteria

1. Names like `../bad`, `..\bad`, `/tmp/x`, `a/b` are rejected.
2. Valid names still save/load correctly.
3. No profile path is built via raw string interpolation.

---

## HIGH-2: Missing defensive file handling for settings/profile reads

**Source issue:** `README-review-functional.md`  
**Files to update:** `TempoTabbies/Assets/Scripts/JSON_Stuff.cs`

### Required changes

1. For every `File.ReadAllText(...)` call:
   - check `File.Exists(path)` first,
   - handle missing file with default values (no crash).
2. Wrap JSON read/deserialize with targeted exception handling:
   - I/O failure,
   - invalid JSON / parse failure.
3. If load fails:
   - log explicit reason,
   - keep game usable using defaults.
4. Keep behavior consistent between:
   - game manager settings load,
   - profile-to-player load,
   - profile-to-editor load.

### Done criteria

1. Missing settings/profile files no longer crash startup/menu flows.
2. Corrupted JSON files no longer crash; defaults are applied.
3. Existing valid files still load exactly as before.

---

## HIGH-3: Null reference risk in HitManager player binding

**Source issue:** `README-review-functional.md`  
**Files to update:** `TempoTabbies/Assets/Scripts/HitReg/HitManager.cs`

### Required changes

1. In `Awake`, guard all `_gm`, `_gm.p1`, `_gm.p2` dereferences.
2. If required player refs are unavailable, do not access `inputDevice`/button fields.
3. Move binding logic into a safe method that can be retried until dependencies exist.
4. Ensure retry path executes once dependencies are ready (e.g., early `Update` gate).
5. Keep current behavior when dependencies are present (no control mapping regressions).

### Done criteria

1. No null-reference exceptions when entering gameplay scenes under slow/variable init timing.
2. Controls still bind correctly for both player 1 and player 2.

---

## MEDIUM-1: Potential score-save loss due to early finalize return

**Source issue:** `README-review-functional.md`  
**Files to update:** `TempoTabbies/Assets/Scripts/Score/ScoreManager.cs` (and caller(s) triggering finalize)

### Required changes

1. Identify all call paths into `FinalizeScore()`.
2. Define one authoritative trigger point for “all judgments completed”.
3. Ensure finalize is invoked once and only once after completion condition is true.
4. Keep `hasSavedScore` guard semantics.
5. Preserve existing anti-overwrite rule (do not save if lower than existing best).

### Done criteria

1. End-of-song always persists expected result once chart is complete.
2. No duplicate score writes from repeated finalize calls.

---

## MEDIUM-2: Miss accounting authority when late notes are destroyed

**Source issue:** `README-review-functional.md`  
**Files to review/update:** `TempoTabbies/Assets/Scripts/Chart/Note.cs` and miss/judgment owner in hit system

### Required changes

1. Decide single authoritative system for miss registration (recommended: hit/judgment manager).
2. Document the ownership rule in code comments near that system.
3. Ensure late-note destruction cannot bypass miss registration.
4. Add a safety assertion/log in development builds for “destroyed late note without judgment”.

### Done criteria

1. Every late note results in exactly one miss/judgment outcome.
2. No score/accuracy drift from note destruction timing.

---

## MEDIUM-3: Banner loading without memory guardrails

**Source issue:** `README-review-security.md`  
**Files to update:** `TempoTabbies/Assets/Scripts/Chart/ChartSelect/SongButton.cs`

### Required changes

1. Before `File.ReadAllBytes`, check file size against a defined max threshold.
2. If oversized or invalid, use default banner and continue gracefully.
3. Keep current exception handling and fallback behavior.
4. Log one concise warning that includes path and size reason.

### Done criteria

1. Large banner files no longer cause large memory spikes/crashes.
2. Valid normal-size banners still render.

---

## MEDIUM-4: Reduce scene-wide object lookups in hot paths

**Source issue:** `README-review-good-practices.md`  
**Primary file:** `TempoTabbies/Assets/Scripts/Chart/Note.cs` (then other hot-path scripts)

### Required changes

1. Remove repeated `FindFirstObjectByType` calls from frame-critical paths.
2. Prefer:
   - serialized refs wired in scene/prefab, or
   - one-time cache at init with controlled fallback.
3. Start with `Note` and gameplay-critical scripts before menu scripts.

### Done criteria

1. No scene-wide lookup call remains in per-frame gameplay hot paths.
2. Behavior remains unchanged.

---

## MEDIUM-5: Clarify ownership of global mutable state

**Source issue:** `README-review-good-practices.md`  
**Files to update:** `_GameManager`, `GameSession`, `JSON_Stuff` (+ callers as needed)

### Required changes

1. Define and document ownership rules:
   - which class writes each global field,
   - who can read-only,
   - when values must be reset.
2. Remove duplicate writes to the same global field from multiple unrelated scripts.
3. Keep backward-compatible behavior for existing scene flow.

### Done criteria

1. Each global field has a clear single writer (or clearly documented write points).
2. Cross-scene state bugs are easier to trace and reproduce.

---

## Suggested PR structure

1. PR 1: `JSON_Stuff` hardening (HIGH-1 + HIGH-2).
2. PR 2: `HitManager` binding reliability (HIGH-3).
3. PR 3: Scoring finalization + miss accounting (MEDIUM-1 + MEDIUM-2).
4. PR 4: Banner limits + lookup/perf + state ownership cleanup (MEDIUM-3/4/5).

Keep each PR focused, with explicit test notes tied to the relevant issue IDs above.

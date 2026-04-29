# Good practices and maintainability review

## Medium

1. **Heavy reliance on scene-wide lookups**
   - **Evidence:** many files use `FindFirstObjectByType` / `FindAnyObjectByType`, including hot-path classes like `Note` (`TempoTabbies/Assets/Scripts/Chart/Note.cs:46-49`).
   - **Impact:** Hidden coupling, avoidable runtime overhead, and fragile initialization order.
   - **Recommended fix:** Prefer serialized references or one-time dependency wiring in scene bootstrap scripts.

2. **Global mutable state spread across singletons/statics**
   - **Evidence:** `_GameManager`, `JSON_Stuff`, and static `GameSession` are all used as cross-scene mutable state.
   - **Impact:** Harder debugging and higher regression risk when changing flow.
   - **Recommended fix:** Keep ownership boundaries explicit (who writes what, and when), and reduce duplicate sources of truth.

## Low

1. **Large methods combining multiple responsibilities**
   - **Evidence:** `ChartSelectManager.Update`, `HitManager.Update`, `Create_LoadPlayer.Update`.
   - **Impact:** Hard to reason about and test; easier to introduce regressions during polish.
   - **Recommended fix:** Split update loops into focused private methods per state/feature.

2. **Extensive debug logging in runtime-heavy paths**
   - **Evidence:** frequent `Debug.Log` calls in gameplay/input flows (e.g., `HitManager.Update`, `NoteSpawner.Awake/Update`).
   - **Impact:** Log noise and avoidable performance overhead in non-development builds.
   - **Recommended fix:** Gate verbose logs behind development flags or a lightweight logging wrapper.

3. **Hardcoded scene name strings in multiple scripts**
   - **Evidence:** scene literals used across managers (`MainMenu`, `StageSelect`, `GameSingleplayer`, `MultiPlayerChartTest`, etc.).
   - **Impact:** Scene renames can silently break runtime navigation.
   - **Recommended fix:** Centralize scene constants or route transitions through one scene-navigation service.

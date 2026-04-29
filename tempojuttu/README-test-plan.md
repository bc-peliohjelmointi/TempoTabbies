# TempoTabbies Test Plan (Limited-Time Release Focus)

## 1. Scope and goal

This plan prioritizes **release confidence** over broad new coverage.  
Goal: catch high-impact regressions in core gameplay loops, save/load flows, and scene transitions with minimal implementation effort.

## 2. Strategy (time-aware)

1. **Risk-first smoke testing** on every candidate build.
2. **Targeted regression passes** for known fragile areas (input mapping, chart loading, score saving, multiplayer flow).
3. **Minimal automated checks** where already supported (Unity Test Framework command paths), without introducing new test infrastructure.
4. **No net-new features** in test scope; only verification of existing behavior and critical polish fixes.

## 3. Test levels and priorities

### P0 (must pass before release)

1. **Launch and scene flow**
   - Start scene opens and reaches Main Menu without errors.
   - Main navigation works for intended input devices.
   - Core scene transitions do not soft-lock.

2. **Singleplayer core loop**
   - Select profile/player, load stage/song/chart, start gameplay.
   - Notes spawn and judge correctly (tap + hold notes).
   - End-of-song results appear and return path works.

3. **Data persistence**
   - Settings load at startup and save from options/profile flows.
   - Score save writes/updates expected entries and survives restart.
   - Missing/corrupt profile/settings files handled without crash.

4. **Basic stability**
   - No crashes in one full session covering menu -> play -> results -> menu.
   - No blockers from disconnected controller/device change scenarios.

### P1 (important, run daily during polish)

1. **Multiplayer flow**
   - Both players can select charts and start match.
   - Per-player note lanes/input ownership behave correctly.
   - End flow and score presentation are consistent.

2. **Performance sanity**
   - Play dense chart sections and confirm no major frame stutter/freezes.
   - Banner/song loading does not cause severe hitching.

3. **Card/effect interactions**
   - Common cards activate/deactivate correctly in gameplay states.
   - Score/combo/HP systems remain coherent with active effects.

### P2 (best effort / when time permits)

1. UX polish checks (menu selection visuals, hover popups, audio settings edge values).
2. Wider chart compatibility checks across song packs.
3. Long-session soak test.

## 4. Execution plan

### Daily quick pass (about 30–45 min)

1. P0 launch + singleplayer smoke.
2. One persistence check (settings + score save/load).
3. One focused regression check for the latest changed area.

### Pre-release full pass (about half day)

1. Full P0 checklist.
2. P1 multiplayer and performance sanity.
3. Defect triage: only fix/blockers and high-severity issues; defer low-risk cosmetic issues.

## 5. Defect severity and release gate

1. **Blocker/Critical:** crash, data loss, impossible progression, broken core input -> **release blocked**.
2. **High:** major gameplay scoring/judgment errors, broken save semantics -> fix before release unless explicitly waived.
3. **Medium/Low:** cosmetic/minor UX issues -> document and defer if schedule is tight.

Release decision: all P0 must pass; no open blocker/critical defects.

## 6. Minimal automation commands (existing support only)

Use Unity batch tests only if tests exist:

```bash
UNITY="/path/to/Unity"
PROJECT_PATH="./TempoTabbies"

# EditMode
$UNITY -batchmode -nographics -quit \
  -projectPath "$PROJECT_PATH" \
  -runTests -testPlatform editmode \
  -testResults ./TestResults/editmode-results.xml

# PlayMode
$UNITY -batchmode -nographics -quit \
  -projectPath "$PROJECT_PATH" \
  -runTests -testPlatform playmode \
  -testResults ./TestResults/playmode-results.xml
```

If zero tests are present, rely on the manual P0/P1 passes above for this release window.

## 7. Ownership and reporting

1. Record each run as: build identifier, tester, scope (P0/P1/P2), pass/fail, top defects.
2. Keep a short “known issues” list for deferred non-blockers.
3. Re-test only affected high-risk paths after each late fix (focused regression, not full sweep).

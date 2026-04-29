# Functional correctness and reliability review

## High

1. **Settings/profile file reads can crash on missing or malformed files**
   - **Evidence:** `TempoTabbies/Assets/Scripts/JSON_Stuff.cs:95,134,160`
   - `File.ReadAllText(...)` is used without existence checks and without recovery for invalid JSON content.
   - **Impact:** Startup/settings/profile flows can hard-crash when files are deleted/corrupted.
   - **Recommended fix:** Guard with `File.Exists`, add structured error handling, and fallback defaults.

2. **Potential null reference when binding player controls in hit registration**
   - **Evidence:** `TempoTabbies/Assets/Scripts/HitReg/HitManager.cs:94-117`
   - `Awake` accesses `_gm.p1` / `_gm.p2` members directly (`_gm.p1.inputDevice`, `_gm.p2.inputDevice`) without confirming the player objects are present.
   - **Impact:** Scene init timing differences can cause null-reference crashes in gameplay scenes.
   - **Recommended fix:** Validate `_gm`, `p1`, and `p2` before access; defer binding until players are available.

## Medium

1. **Possible score save loss due to early-return finalize path**
   - **Evidence:** `TempoTabbies/Assets/Scripts/Score/ScoreManager.cs:143-147`
   - `FinalizeScore` intentionally exits when `notesHit < totalNotes`. If no later call occurs, score is never persisted.
   - **Impact:** End-of-song results can be lost in edge timing/order cases.
   - **Recommended fix:** Centralize finalize trigger so it runs exactly once after all judgments are complete.

2. **Missed notes are removed without explicit miss registration in `Note`**
   - **Evidence:** `TempoTabbies/Assets/Scripts/Chart/Note.cs:40-44`
   - Notes are destroyed when late, but this class itself does not ensure miss accounting before destroy.
   - **Impact:** If miss accounting in other systems desynchronizes, score/accuracy may drift silently.
   - **Recommended fix:** Keep miss accounting in one authoritative place and assert that every destroyed late note is judged.

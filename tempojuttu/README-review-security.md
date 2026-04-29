# Security review

## High

1. **Profile name path traversal in JSON save/load**
   - **Evidence:** `TempoTabbies/Assets/Scripts/JSON_Stuff.cs:128,134,160`
   - The profile name is interpolated directly into filesystem paths (`JSON/{name}.json`) without filename sanitization.
   - **Risk:** A crafted name containing path separators (for example `../`) can read/write outside the intended `JSON/` folder.
   - **Recommended fix:** Restrict profile names to a safe allowlist (e.g., alnum, `_`, `-`), normalize via `Path.GetFileName`, and reject mismatches before I/O.

## Medium

1. **Unbounded banner image loading from disk**
   - **Evidence:** `TempoTabbies/Assets/Scripts/Chart/ChartSelect/SongButton.cs:188-196`
   - Banner files are read fully into memory (`File.ReadAllBytes`) and decoded without a size cap.
   - **Risk:** Malicious or very large banners can cause memory spikes/crashes.
   - **Recommended fix:** Add maximum file-size checks before read/decode, then fallback to default banner.

## Positive notes

1. **Database queries are parameterized**
   - **Evidence:** `TempoTabbies/Assets/SQL/Database.cs:83-123,181-191,227-236`
   - `SqliteParameter` is used consistently for dynamic values, which is good SQL injection hygiene.

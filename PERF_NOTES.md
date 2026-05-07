# Performance Audit & Fixes

## Phase 1 — Diagnose

### Project Settings
- `Time.fixedDeltaTime` = `0.02` (50Hz physics) — bumped to `1/60 = 0.0167` (60Hz) at runtime via `PerformanceConfig`
- `QualitySettings.vSyncCount` was `1` in 4 of 6 quality levels — now `0` everywhere
- `Application.targetFrameRate` was unset — now explicitly `-1` (uncapped)

### Input Reading
- `InputManager.Update()` reads new Input System actions (`WasPressedThisFrame`, `IsPressed`, `ReadValue`) and stores them in static fields. ✅ Correct.
- `CharacterController.Update()` reads timers/jump/dash flags and applies physics in `FixedUpdate()`. ✅ Correct pattern (input read in Update, physics in FixedUpdate).
- `PauseMenu.Update()`, `InventoryUI.Update()`, `NPC.Update()` use `Keyboard.current` directly for one-off keys (Tab, Esc, 1, 2). Acceptable — no allocation or lag.

### Update / FixedUpdate / LateUpdate methods
Found in:
- `RangedAttack.Update`, `EnemyMovement.FixedUpdate`, `EnemyProjectile.FixedUpdate`
- `GameOver.Update`, `EnemyDamage.Update`, `InventoryUI.Update`, `ManaUI.Update`
- `PlayerAttack.Update`, `NPC.Update`, `InputManager.Update`, `PlayerSpell.Update`
- `HealthDisplay.Update`, `PauseMenu.Update`, `Bullet.FixedUpdate`
- `CharacterController.Update + FixedUpdate`, `DashCooldownUI.Update`

## Phase 2 — Common lag culprits

### Hot-path `FindAnyObjectByType` (FIXED)
- `NPC.Interact()` and `NPC.AdvanceDialogue()` called `Object.FindAnyObjectByType<PauseMenu>()` on every E press.
  → Cached `_pauseMenu` in `Start()`.

### Allocations in Update loops
- No GC allocations found in per-frame hot paths. All `Vector2`/`Color` are structs (stack only).
- `NPC.DisplayChoices` does a `string.Format` for choice labels — only runs when choices appear, not per frame. Acceptable.

### Input read in FixedUpdate? (NO)
- `CharacterController.FixedUpdate` reads `InputManager.Movement` (a static field updated in `InputManager.Update`). Movement vector is sampled every Update, so FixedUpdate sees the freshest value. ✅

### Physics applied in Update? (NO)
- All `Rigidbody2D.linearVelocity` writes happen in `FixedUpdate`. ✅

### UI dirty writes (FIXED)
- `HealthDisplay.Update`, `ManaUI.Update`, `DashCooldownUI.Update` overwrote `Image.sprite`, `Image.fillAmount`, `Image.color`, `Image.enabled` every frame even when nothing changed. Each write dirties the Canvas batch and forces a rebuild.
  → Added value comparisons; only assign when value changes.

### Distance calls (FIXED)
- `RangedAttack.Update` used `Vector2.Distance` (sqrt) every frame.
  → Replaced with `sqrMagnitude` against `attackRange * attackRange`.

### Removed per-frame `Debug.LogError` (FIXED)
- `ManaUI.Update` logged errors every frame if references missing — silently returns now.

## Phase 3 — Fixes Applied

| Fix | File | Impact |
|---|---|---|
| Force `vSyncCount = 0` in QualitySettings | `ProjectSettings/QualitySettings.asset` | **Biggest input-lag win in builds** — removes 1+ frames of buffered display lag |
| `PerformanceConfig.cs` (new) | sets vSync, target FR, fixedDeltaTime at runtime | Ensures correct settings even after preset changes |
| `PerformanceLogger.cs` (new) | logs FPS / deltaTime / vSync for 5 s | Verification |
| Cache `PauseMenu` in NPC | `NPC.cs` | Removes scene-wide search per E press |
| Dirty-check UI writes | `HealthDisplay.cs`, `ManaUI.cs`, `DashCooldownUI.cs` | Avoids needless Canvas rebuilds |
| `sqrMagnitude` in range check | `RangedAttack.cs` | Saves `sqrt` per frame per ranged enemy |

## Phase 4 — Verify

1. Drop a GameObject in the scene with `PerformanceConfig` and `PerformanceLogger` components.
2. Play the scene; watch the console for 5 seconds.
3. Expected: `vSync=0`, `targetFR=-1`, `avgFPS` should hit your monitor refresh × N (uncapped). Build should now respond to input within 1 frame instead of 2–3.

### Setup steps in Unity
1. Add empty GameObject named **PerformanceManager**.
2. Add `PerformanceConfig` component (defaults are correct).
3. Add `PerformanceLogger` component (5 s log on scene start).
4. Build & run — input should feel snappier.

### Notes
- If you ever need vSync back (screen tearing on slow monitors), set `vSyncCount = 1` on `PerformanceConfig` rather than the asset.
- `Time.fixedDeltaTime = 1/60` matches typical monitor refresh — physics-driven movement feels tighter to input.

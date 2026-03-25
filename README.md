# A%A — Pixel Beach Runner (Unity, Android, Landscape 16:9)

A polished 2D pixel-art endless runner architecture for Android. The runner auto-moves through a layered beach world, supports character selection (male/female from your provided sprite sheet), jump/slide controls, pickups, escalating difficulty, and premium mobile UI flow.

---

## 1) Project Structure

```text
Assets/
  Animations/
    Male/
    Female/
    UI/
  Art/
    Characters/
    Environment/
    UI/
    Icons/
  Audio/
    Music/
    SFX/
  Prefabs/
    Player/
    Obstacles/
    Pickups/
    Environment/
    UI/
  Scenes/
    Bootstrap.unity
    MainMenu.unity
    Gameplay.unity
  Scripts/
    Audio/
    Core/
    Data/
    Player/
    UI/
    Utilities/
    World/
```

---

## 2) Core Systems Implemented

- **Game flow state machine** (`Splash`, `MainMenu`, `CharacterSelect`, `Playing`, `Paused`, `GameOver`).
- **Endless-run runtime speed scaling** with progressive acceleration.
- **3-lane beach runner movement** + jump + slide.
- **Dynamic obstacle/pickup pattern spawning**.
- **Pickups**: coin, star boost, speed boost, shield.
- **HUD + menus** with score, distance, coins, pause, game over, best score.
- **Audio bus** for music and SFX routing.
- **Character database** for male/female selection and animator swapping.
- **Parallax backgrounds** and segment recycling for endless illusion.
- **Hit feedback**: particles + camera shake + game over transition.

---

## 3) Scene Setup (Step-by-Step)

## A) Bootstrap.unity
1. Create empty `Systems` object.
2. Add these components:
   - `GameManager`
   - `AudioBus`
   - `Bootstrapper`
3. Mark these GameObjects as `DontDestroyOnLoad` via scripts (already done).
4. Set this scene as first in Build Settings.

## B) MainMenu.unity
1. Add a Canvas (Screen Space - Camera preferred) and EventSystem.
2. Add a `UIManager` GameObject.
3. Create these panel groups under Canvas:
   - Splash panel
   - Main Menu panel
   - Character Select panel
   - HUD panel (hidden here)
   - Pause panel (hidden)
   - Game Over panel (hidden)
4. Hook panel `CanvasGroup` references into `UIManager`.
5. Add animated title text (`A%A`) + red heart icon sprite to splash/menu.
6. Add Play button -> `UIManager.OnTapPlay()`.
7. Add Start Run button in Character Select -> `UIManager.OnTapStartRun()`.
8. Add `CharacterSelectPresenter` to Character Select panel.

## C) Gameplay.unity
1. Add Pixel Perfect Camera + Cinemachine camera (optional but recommended).
2. Add `CameraShake` to main camera.
3. Add `CharacterSpawner` at spawn X near left-third of screen.
4. Add `RunnerController` prefab and assign in `CharacterSpawner`.
5. Add lane markers (`SpawnLow`, `SpawnMid`, `SpawnHigh`) near right edge.
6. Add `ObstacleSpawner` and assign obstacle/pickup prefabs + lane points.
7. Build environment from 2-3 repeating ground segments with `SegmentScroller`.
8. Add layered sky/sea/palm/background objects with `ParallaxLayer`.
9. Add gameplay Canvas + `UIManager` for HUD/Pause/GameOver overlays.
10. Jump button -> `TouchInputController.JumpButtonPressed()`.
11. Slide button -> `TouchInputController.SlideButtonPressed()`.
12. Pause button -> `UIManager.OnTapPause()`.

---

## 4) Import and Slice the Provided Character Sprite Sheet

1. Save the attached sheet to: `Assets/Art/Characters/runner_sheet.png`.
2. Import settings:
   - Texture Type: `Sprite (2D and UI)`
   - Sprite Mode: `Multiple`
   - Filter Mode: `Point (no filter)`
   - Compression: `None`
   - Pixels Per Unit: `64` (adjust for your scene scale)
3. Open Sprite Editor and slice manually:
   - Top row = Male frames (idle/run/hit/lose)
   - Bottom row = Female frames (idle/run/hit/lose)
4. Keep frame pivots at bottom-center for stable foot placement.
5. Rename slices clearly:
   - `male_run_01..03`, `male_idle_01`, `male_hit`, `male_lose`
   - `female_run_01..03`, `female_idle_01`, `female_hit`, `female_lose`
6. Create animation clips:
   - Run loop: 10-14 fps
   - Idle loop: 5-8 fps
   - Hit: one-shot (0.2-0.4s)
   - Lose: hold final frame with slight scale squash (animation curve)
7. Create two Animator Controllers:
   - `MaleRunner.controller`
   - `FemaleRunner.controller`
8. Hook each controller to a `CharacterDefinition` asset.

> If the sheet lacks complete transitions, duplicate nearest matching frames and offset body/arm pixels minimally so motion still reads premium and consistent.

---

## 5) Character Selection Data Wiring

1. Create `CharacterDefinition` assets:
   - `CD_Male`
   - `CD_Female`
2. Fields:
   - Unique `id` (`male`, `female`)
   - Display name
   - Portrait sprite (from sheet)
   - Runtime animator controller
3. Create one `CharacterDatabase` asset and add both definitions.
4. Assign this database to:
   - `GameManager`
   - `CharacterSelectPresenter`

---

## 6) Obstacles and Pickups Setup

## Obstacles (Tag: `Obstacle`)
Create prefabs under `Assets/Prefabs/Obstacles/`:
- BeachRock
- Crab
- BrokenWood
- UmbrellaStand
- SeaDebris

Each should include:
- SpriteRenderer
- Collider2D (`isTrigger = true`)
- `AutoDestroyOffscreen`

## Pickups
Create prefabs under `Assets/Prefabs/Pickups/`:
- Coin
- StarBoost
- SpeedBoost
- ShieldBubble

Each includes:
- SpriteRenderer
- Collider2D trigger
- `Pickup` script with proper enum type
- optional collect particles

---

## 7) UI/UX Polish Checklist

- Use 9-sliced pixel panels with subtle gradients.
- Add hover/press animation on all buttons (scale punch + click SFX).
- Animate panel transitions using CanvasGroup alpha + anchored motion.
- Add light particle layer behind UI for premium feel.
- Use separate color accents:
  - Score: warm yellow
  - Distance: cyan
  - Coins: gold
- Add vignette overlay in gameplay for visual depth.
- Add hit flash and slight camera shake on collision.

---

## 8) Audio Integration

Drop files into:
- `Assets/Audio/Music/` (beach loop)
- `Assets/Audio/SFX/` (jump, coin, hit, button, game over)

Assign clips to `AudioBus` fields in Bootstrap scene.

Recommended import settings:
- Music: Vorbis quality ~0.65, Streaming enabled
- SFX: Decompress On Load, mono where possible

---

## 9) Android Build Configuration

1. File -> Build Settings -> Android -> Switch Platform.
2. Player Settings:
   - Product Name: `A%A`
   - Company Name: your studio
   - Package Name: `com.yourstudio.aa`
   - Orientation: `Landscape Left + Right`
   - Target API: latest installed
   - Scripting Backend: IL2CPP
   - ARM64 enabled
3. Resolution and Presentation:
   - Default Orientation: Landscape
   - Disable auto-rotation to portrait modes
4. Icon:
   - Place red heart icon in `Assets/Art/Icons/icon_heart_red.png`
   - Assign adaptive + legacy icon slots
5. Build/Run to device.

---

## 10) Performance Guidance (Mid-Range Android)

- Keep draw calls low via sprite atlasing.
- Reuse obstacle and pickup prefabs through object pooling (next upgrade).
- Use Point filtering for pixel crispness.
- Avoid expensive per-frame allocations.
- Target 60 FPS with VSync off + `Application.targetFrameRate = 60`.

---

## 11) Mandatory Script Wiring Quick Reference

- `GameManager.characterDatabase` -> `CharacterDatabase` asset
- `CharacterSpawner.runnerPrefab` -> runner prefab with `RunnerController`
- `TouchInputController.runner` -> spawned runner or scene runner reference
- `ObstacleSpawner.obstaclePrefabs/pickupPrefabs/laneSpawnPoints` assigned
- `UIManager` all panel and TMP text references assigned
- `AudioBus` music + SFX assigned

---

## 12) Next Premium Upgrades (Optional)

- Object pooling manager for zero-GC spawning.
- Daily reward + mission system.
- Haptics by action type (jump/coin/hit).
- Shader-based heat haze above sand.
- Cloud save + leaderboard.


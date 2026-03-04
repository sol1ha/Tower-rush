# Tower Rush — Full Game Plan
### Target Platform: Luxodd Arcade (WebGL, 1080×1920 portrait, joystick + 6 buttons)

---

## What Is Already Done

| System | Script | Status |
|--------|--------|--------|
| Player movement + double jump + dash | `PlayerController3D.cs` | ✅ Done |
| Platform zig-zag spawning | `PlatformSpawner3D.cs` | ✅ Done |
| Platform auto-decay after player leaves | `PlatformDecay.cs` | ✅ Done |
| Rising floor hazard | `RisingFloor3D.cs` | ✅ Done |
| Coin collectible | `Coin3D.cs` | ✅ Done |
| Spike hazard | `SpikeHazard.cs` | ✅ Done |
| Laser shooter hazard | `LaserShooter.cs` + `LaserProjectile.cs` | ✅ Done |
| Camera follows player upward | `CameraFollow3D.cs` | ✅ Done |
| Score / level / multiplier logic | `GameManager.cs` | ✅ Done |
| HUD (score, level, multiplier, notifications) | `UIManager.cs` | ✅ Done |
| Neon platform colors at spawn | `PlatformColorizer.cs` | ✅ Done |
| Jetpack particle controller | `JetpackEffect.cs` | ✅ Done |
| Collider negative-scale fix | `ColliderScaleFix.cs` | ✅ Done |

---

## Critical Luxodd Platform Rules (Read Before Anything)

These are hard requirements from the platform. Breaking them = rejection.

| Rule | What it means for Tower Rush |
|------|------------------------------|
| **WebGL only, portrait 1080×1920** | Canvas size must be set to 1080×1920 in Player Settings |
| **Joystick + 6 buttons — NO keyboard/mouse** | PlayerController3D must be rewritten to use ArcadeControls, not keyboard |
| **No pausing** | Remove any pause menu/button |
| **Session ≤ 10 minutes** | Rising floor speed must guarantee game ends within 10 minutes |
| **Must call `BackToSystem()`** | Replace current `SceneManager.LoadScene` reload with Luxodd session end |
| **Leaderboard required** | Must call `SendLevelBeginRequestCommand` and `SendLevelEndRequestCommand` |
| **Health check every 5 seconds** | `HealthStatusCheckService.Activate()` must be called at game start |
| **All menus = joystick only** | Game Over screen must be navigable with joystick, not mouse |
| **Menu timeout = 30 seconds** | Game Over screen must auto-exit after 30 seconds |
| **No reserved buttons** | Do NOT use Orange (JoystickButton8) or White (JoystickButton9) |
| **Score is per-session** | Score resets on every run — no cumulative cross-session totals |

### Arcade Button Assignments for Tower Rush

| Action | Button | Color |
|--------|--------|-------|
| Move left/right | Joystick | — |
| Jump / Double Jump | Black button | ⚫ JoystickButton0 |
| Dash | Red button | 🔴 JoystickButton1 |
| *(reserved — do not use)* | Orange | 🟠 JoystickButton8 |
| *(reserved — do not use)* | White | ⚪ JoystickButton9 |

---

## Phase 1 — Unity Scene & Object Setup (Editor Work)

### Step 1.1 — Canvas & Screen Setup

**In Unity:**
1. Open **File → Build Settings** → switch platform to **WebGL** → click **Switch Platform**
2. Click **Player Settings** → find **Resolution and Presentation**
3. Set **Default Canvas Width** = `1080`, **Default Canvas Height** = `1920`
4. Enable **Run In Background** ✓
5. Set **WebGL Template** to **LuxoddTemplate** (available after plugin install)

**In your Scene:**
1. Right-click **Hierarchy → UI → Canvas**
2. Select the Canvas → in Inspector set **Canvas Scaler** component:
   - UI Scale Mode: **Scale With Screen Size**
   - Reference Resolution: **1080 × 1920**
   - Screen Match Mode: **Match Width or Height**, slider at **0.5**
3. This makes all UI look correct on the arcade portrait screen

---

### Step 1.2 — Player Object Setup

**Create the player in the scene:**
1. Right-click **Hierarchy → 3D Object → Capsule** → name it `Player`
2. In Inspector, set **Transform**:
   - Position: `(0, 1, 0)`
   - Scale: `(0.8, 1, 0.8)`
3. Add these components (click **Add Component** and search each):
   - `Rigidbody` → freeze rotation on X, Y, Z axes → **unfreeze Z only**
   - `Capsule Collider` → adjust radius/height to match the capsule
   - `PlayerController3D` (your script)
   - `JetpackEffect` (your script)
   - `ColliderScaleFix` (your script)
4. Set the **Tag** of the Player object to `Player`:
   - At the top of the Inspector, click the Tag dropdown → **Add Tag** → type `Player` → Save
   - Select Player again → set Tag to `Player`
5. In `PlayerController3D` Inspector fields:
   - **Ground Layer**: click the dropdown → select the layer your platforms are on (create one called `Ground` if it doesn't exist)
   - **Ground Distance**: `0.4`
   - **Move Speed**: `5`
   - **Jump Force**: `7`
   - **Dash Force**: `15`

**Create the GroundCheck child object:**
1. Right-click on `Player` in the Hierarchy → **Create Empty** → name it `GroundCheck`
2. Set its Position to `(0, -1, 0)` (at the bottom of the capsule)
3. Drag `GroundCheck` into the **Ground Check** field on `PlayerController3D`

**Create the Jetpack Thruster (particle system):**
1. Right-click on `Player` → **Effects → Particle System** → name it `Thruster`
2. Set its **Transform Position** to `(0, -0.8, 0)` (bottom of player)
3. In the **Particle System** component:
   - **Duration**: 1, **Looping**: ✓, **Start Lifetime**: 0.3, **Start Speed**: 3
   - **Start Color**: bright blue or white (`#00BFFF`)
   - **Shape** module → Shape: **Cone**, Angle: `15`
   - Rotate the cone to point **downward**: set the Thruster object rotation to `(180, 0, 0)`
   - **Emission** → Rate over Time: `10` (JetpackEffect will override this at runtime)
4. (Optional) Create another child called `DashEffect` — same steps but shape: **Sphere**, rate: `0`
5. Drag `Thruster`'s Particle System into `JetpackEffect → Thruster Particles`
6. Drag `DashEffect` into `JetpackEffect → Dash Particles` (if created)

---

### Step 1.3 — Platform Prefab Setup

**Create the base platform:**
1. Right-click **Hierarchy → 3D Object → Cube** → name it `Platform`
2. Set **Transform Scale** to `(3, 0.3, 1)` — wide and flat
3. Add a **Box Collider** (already on cubes by default)
4. Set the **Layer** to `Ground` (same layer PlayerController3D checks for ground)
5. Set the **Tag** to `Platform`:
   - Inspector → Tag dropdown → Add Tag → `Platform` → Save → re-select Platform → set tag

**Create the neon material:**
1. Right-click in **Project → Assets → Create → Material** → name it `PlatformMat`
2. Select the material → in Inspector:
   - Set **Shader** to `Universal Render Pipeline/Lit` (if using URP) or `Standard`
   - Scroll down to **Emission** → check the box ✓ to enable it
   - Set Emission Color to black (we just need the keyword enabled — `PlatformColorizer` changes it at runtime)
3. Drag `PlatformMat` onto the `Platform` cube in the Scene view

**Create spike child objects (for Spike Hazard platforms):**
1. With `Platform` selected in Hierarchy, right-click it → **3D Object → Cylinder** → name it `Spike`
2. Set `Spike` Transform:
   - Position: `(0, 0.6, 0)` (sits on top of the platform)
   - Scale: `(0.15, 0.4, 0.15)` (thin and pointy)
3. Add component: `SpikeHazard` (your script)
4. Add component: **Capsule Collider** → check **Is Trigger** ✓
5. Create a red/orange material and apply it to the spike
6. Duplicate the spike 3–4 times, spread them along the X axis: positions `(-1, 0.6, 0)`, `(0, 0.6, 0)`, `(1, 0.6, 0)`

**Save as Prefabs:**
1. In Project window, right-click → **Create → Folder** → name it `Prefabs`
2. Drag `Platform` from Hierarchy into the `Prefabs` folder → it becomes a prefab (blue cube icon)
3. Delete the Platform from the scene (it will be spawned by PlatformSpawner3D)

**Wire up the spawner:**
1. Select your `PlatformSpawner3D` GameObject in the scene
2. Drag the `Platform` prefab → **Platform Prefab** field
3. Drag your `Coin` prefab → **Coin Prefab** field
4. Drag your `Spike` prefab → **Spike Prefab** field (the standalone spike, not as child)
5. Drag the **Player** object → **Player** field

---

### Step 1.4 — Coin Prefab Setup

1. Right-click **Hierarchy → 3D Object → Cylinder** → name it `Coin`
2. Set **Transform Scale** to `(0.5, 0.05, 0.5)` — flat disc shape
3. Create a yellow/gold material with **Emission** enabled (same as platform mat)
4. Add component: **Cylinder Collider** → check **Is Trigger** ✓
5. Add component: `Coin3D` (your script) → set **Point Value**: `100`
6. Drag into `Prefabs` folder → delete from scene

---

### Step 1.5 — Rising Floor Setup

1. Right-click **Hierarchy → 3D Object → Cube** → name it `RisingFloor`
2. Set **Transform**:
   - Position: `(0, -3, 0)` — starts below the player
   - Scale: `(30, 0.5, 10)` — wide enough to cover the whole play area
3. Create an orange/yellow glowing material with Emission enabled
4. Add component: **Box Collider** → check **Is Trigger** ✓
5. Add component: `RisingFloor3D` (your script)
6. Add component: `ColliderScaleFix` (your script)
7. Set **Tag** to `Hazard`

---

### Step 1.6 — Death Zone Setup

1. Right-click **Hierarchy → Create Empty** → name it `DeathZone`
2. Add component: **Box Collider** → check **Is Trigger** ✓
3. Set Scale to `(50, 1, 50)` — wide invisible floor at the very bottom
4. Set Position: `(0, -20, 0)` — far below the starting point
5. Add component: `DeathZone` (your script)

---

### Step 1.7 — Laser Shooter Setup

1. Right-click **Hierarchy → 3D Object → Cube** → name it `LaserShooter`
2. Place it on the **right wall** of the play area, e.g., Position: `(8, 5, 0)`
3. Rotate it to face left: Rotation `(0, 180, 0)`
4. Add component: `LaserShooter` (your script)
5. Set **Fire Rate**: `3` (fires every 3 seconds), **Laser Speed**: `8`, **Laser Lifetime**: `4`

**Create the Laser Projectile Prefab:**
1. Right-click **Hierarchy → 3D Object → Capsule** → name it `LaserProjectile`
2. Set Scale: `(0.1, 1, 0.1)` — thin and long
3. Rotate 90° on Z so it points sideways
4. Create a bright green glowing material (Emission enabled, green color)
5. Add component: **Capsule Collider** → **Is Trigger** ✓
6. Add component: `LaserProjectile` (your script) — **Speed**: `8`, **Life Time**: `4`
7. Drag into `Prefabs` folder → delete from scene
8. Drag the prefab into `LaserShooter → Laser Prefab` field

---

### Step 1.8 — Camera Setup

1. Select **Main Camera** in the Hierarchy
2. Set **Transform Position**: `(0, 5, -12)` — behind and above the player
3. Set **Transform Rotation**: `(15, 0, 0)` — slight downward tilt
4. Add component: `CameraFollow3D` (your script)
5. Drag **Player** into the **Player** field
6. Set **Offset**: `(0, 5, -12)`, **Smooth Speed**: `6`, **Follow X**: ✓ checked

---

### Step 1.9 — HUD Canvas Setup

1. Right-click **Hierarchy → UI → Canvas** → name it `HUD`
2. Add component: `UIManager` (your script)

**Score Text (top right):**
1. Right-click `HUD → UI → Text - TextMeshPro` → name it `ScoreText`
2. In `Rect Transform`:
   - Anchor: top-right
   - Position: `(-20, -30, 0)` (padding from corner)
3. Text: `000000`, Font Size: `60`, Color: white, Alignment: right

**Level Text (top left):**
1. Right-click `HUD → UI → Text - TextMeshPro` → name it `LevelText`
2. Anchor: top-left, Position: `(20, -30, 0)`
3. Text: `LEVEL 1`, Font Size: `50`, Color: white

**Multiplier Text (below score):**
1. Right-click `HUD → UI → Text - TextMeshPro` → name it `MultiplierText`
2. Anchor: top-right, Position: `(-20, -90, 0)`
3. Text: `X1`, Font Size: `40`, Color: yellow

**Notification Text (center):**
1. Right-click `HUD → UI → Text - TextMeshPro` → name it `NotificationText`
2. Anchor: center, Position: `(0, 100, 0)`
3. Text: `` (empty), Font Size: `55`, Color: yellow, Alignment: center

**Jump Indicator (bottom left):**
1. Right-click `HUD → UI → Panel` → name it `JumpIndicator`
2. Anchor: bottom-left, Size: `(160, 60)`
3. Add child TextMeshPro: text `⚫ JUMP`, Font Size: `35`, Color: white

**Dash Indicator (bottom right):**
1. Same as above but anchor: bottom-right, text `🔴 DASH`

**Wire up UIManager:**
- Drag `ScoreText` → **Score Text** field
- Drag `LevelText` → **Level Text** field
- Drag `MultiplierText` → **Multiplier Text** field
- Drag `NotificationText` → **Notification Text** field

---

## Phase 2 — Game Mechanics Scripts (Code to Write)

### Step 2.1 — Input Setup (Keyboard + Joystick — both work)

**How it works:** Unity's New Input System (already used by `PlayerController3D`) supports multiple bindings on the same action. We add **both keyboard and joystick** bindings to each action. This means:
- In the **Editor/testing**: use keyboard (WASD, Space, Shift)
- On the **Luxodd cabinet**: use joystick + arcade buttons automatically

No code changes needed — only configure the Input Action Asset.

**Steps in Unity:**
1. In Project window, find your **Input Action Asset** (`.inputactions` file) — it was created when you first set up `PlayerController3D`
   - If it doesn't exist: right-click in Project → **Create → Input Actions** → name it `PlayerInputActions`
2. Double-click the asset to open the **Input Action Editor**
3. You should have 3 Actions: **Move**, **Jump**, **Dash**

**Binding Move (keyboard + joystick):**
1. Click `Move` action → click `+` next to it → **Add Binding**
2. Click the new binding → in the **Path** field → search `WASD` → select **2D Vector Composite**
3. This creates 4 sub-bindings (Up/Down/Left/Right) → assign: W/S/A/D keys
4. Click `+` again → **Add Binding** → Path: `Left Stick [Joystick]` for arcade joystick

**Binding Jump (keyboard + arcade button):**
1. Click `Jump` → `+` → **Add Binding** → Path: `Space [Keyboard]`
2. Click `+` again → **Add Binding** → Path: `Button South [Joystick]` (= JoystickButton0 = Black button)

**Binding Dash (keyboard + arcade button):**
1. Click `Dash` → `+` → **Add Binding** → Path: `Left Shift [Keyboard]`
2. Click `+` again → **Add Binding** → Path: `Button East [Joystick]` (= JoystickButton1 = Red button)

4. Click **Save Asset** in the top-left of the editor
5. Drag the three actions into `PlayerController3D` Inspector fields: **Move Action**, **Jump Action**, **Dash Action**

> For the final Luxodd arcade build, the keyboard bindings are ignored because no keyboard is connected — the joystick bindings take over automatically. No build-specific code needed.

---

### Step 2.2 — Level Difficulty Scaling

**What it should do:** As the player climbs higher (level increases), make the game harder:
- Rising floor speeds up
- Platforms decay faster
- Spike spawn chance increases
- Laser fires more often

**Script to update: `GameManager.cs`**
- Expose a `DifficultyMultiplier` property calculated from current level
- Other scripts read this to scale their speed/chance values

**Example scaling:**
```
Level 1: floor speed 0.2, decay time 5s, spike chance 20%
Level 3: floor speed 0.35, decay time 3.5s, spike chance 30%
Level 5: floor speed 0.5, decay time 2.5s, spike chance 40%
```

---

### Step 2.3 — Screen Shake on Death

**Script to create: `CameraShake.cs`**
- Attach to the Main Camera
- When `GameOver()` is called, shake the camera for 0.5 seconds
- Uses a random offset per frame, diminishes over time

**How to wire in Unity:**
1. Add `CameraShake` to Main Camera
2. In `GameManager.GameOver()`, call `CameraShake.Instance.Shake(0.5f, 0.3f)`

---

### Step 2.4 — Score Multiplier System

**What it should do:** Collecting coins in quick succession increases the multiplier (X1 → X2 → X3).
- Collecting a coin within 3 seconds of the last coin increases multiplier
- Multiplier resets if 3 seconds pass without a coin

**Script to update: `GameManager.cs`**
- Add `lastCoinTime` float
- `AddCoinScore()` method: if `Time.time - lastCoinTime < 3f`, increment multiplier, else reset to 1
- Update `UIManager` to show multiplier change notification

---

### Step 2.5 — Game Over Screen (with Luxodd Continue/Restart)

**Script to create: `GameOverScreen.cs`**
- Shows a UI panel when GameOver is triggered
- Displays final score
- Shows two options: **CONTINUE** (costs credits) and **END SESSION**
- Joystick navigates between options, Black button confirms
- **Auto-hides after 30 seconds** (Luxodd requirement)
- Calls Luxodd SDK: `SendSessionOptionContinue()` when Continue is selected
- Calls `BackToSystem()` when session ends

**In Unity — create the Game Over Panel:**
1. Right-click `HUD → UI → Panel` → name it `GameOverPanel`
2. Set to fill the whole screen (anchor: stretch-stretch)
3. Add a dark semi-transparent background image
4. Add TextMeshPro children:
   - `"GAME OVER"` — large, centered, top
   - `"SCORE: 000000"` — medium, centered, middle
5. Add two UI Buttons:
   - `ContinueButton` — text: `CONTINUE (3 credits)`, bottom left
   - `EndButton` — text: `END SESSION`, bottom right
6. Disable the panel by default (uncheck the checkbox next to its name in Inspector)

---

### Step 2.6 — Leaderboard Screen

**Script to create: `LeaderboardScreen.cs`**
- After game over, shows top 10 players
- Highlights current player's rank
- Navigable with joystick (up/down to scroll)
- Auto-closes after 30 seconds
- Calls Luxodd SDK: `SendLeaderboardRequestCommand()`

**In Unity:**
1. Right-click `HUD → UI → ScrollView` → name it `LeaderboardPanel`
2. Inside the scroll content, add a `LeaderboardRow` prefab:
   - Horizontal layout with: Rank text, Player name text, Score text
3. `LeaderboardScreen` instantiates rows from API response data

---

## Phase 3 — Luxodd Plugin Integration (Required for Submission)

### Step 3.1 — Install the Plugin

1. Download from: https://github.com/luxodd/unity-plugin/releases
2. In Unity: **Assets → Import Package → Custom Package** → select the `.unitypackage`
3. Click **Import All**
4. If prompted to install **Newtonsoft.Json**, click Install
5. Go to **Tools → Luxodd Plugin → Set Developer Token** → paste your token from the Admin Portal

---

### Step 3.2 — Add the Plugin Prefab to Scene

1. In Project window, find: `Assets/Luxodd.Game/Prefabs/UnityPluginPrefab`
2. Drag it into your **scene Hierarchy**
3. This prefab contains: `WebSocketService`, `WebSocketCommandHandler`, `HealthStatusCheckService`, `ReconnectService`
4. Do NOT rename or duplicate this prefab

---

### Step 3.3 — Game Bootstrap Script

**Script to create: `GameBootstrap.cs`**

Attach to a new empty GameObject called `GameBootstrap`. This runs at game start:

```csharp
// On Start():
// 1. Connect to Luxodd server via WebSocketService
// 2. On connection success:
//    - Activate HealthStatusCheckService
//    - Call SendProfileRequestCommand to get player name
//    - Call SendLevelBeginRequestCommand(level: 1)
//    - Show player name in HUD welcome notification
// 3. On connection failure:
//    - Show "Connecting..." and retry
```

**In Unity:**
1. Create empty GameObject → name it `GameBootstrap`
2. Add `GameBootstrap` script
3. Drag `WebSocketService` from the plugin prefab → into the script's field
4. Drag `WebSocketCommandHandler` → into its field
5. Drag `HealthStatusCheckService` → into its field

---

### Step 3.4 — Track Level Begin/End (Leaderboard)

**Update `GameManager.cs`:**
- On `Start()`: call `SendLevelBeginRequestCommand(currentLevel)`
- On `GameOver()`: call `SendLevelEndRequestCommand(currentLevel, score)` **before** showing the Game Over screen
- These calls populate the Luxodd leaderboard — do not skip them

---

### Step 3.5 — Health Check (Required — 5 seconds)

- This is handled automatically by `HealthStatusCheckService.Activate()`
- Call it once after successful WebSocket connection (in `GameBootstrap`)
- The plugin sends a ping every 2 seconds (configurable)
- If 3 pings are missed, the server ends the session automatically

---

### Step 3.6 — Session End (BackToSystem)

**Update `GameManager.cs`:**
- Replace `SceneManager.LoadScene(...)` with `_webSocketService.BackToSystem()`
- This is called after the Game Over screen closes (either player ends, or 30 second timeout)
- On crash/error: call `_webSocketService.BackToSystemWithError("crash", message)`

---

## Phase 4 — Polish & Audio

### Step 4.1 — Sound Effects

**Sounds needed:**
| Sound | When |
|-------|------|
| Jump | Every jump |
| Double Jump | Second jump (different pitch) |
| Dash | When dashing |
| Coin collect | On coin pickup |
| Spike death | Player hits spike |
| Floor rising | Warning sound when floor starts |
| Game over | On death |
| Level up | When level increases |

**How to add in Unity:**
1. Import `.wav` or `.mp3` files into `Assets/Audio/` folder
2. Select each audio clip → In Inspector set **Load Type** to `Compressed In Memory` (saves WebGL memory)
3. Create an empty GameObject → name it `AudioManager`
4. Add **Audio Source** components (one per sound, or use a pooled system)
5. In your scripts, call `audioSource.PlayOneShot(clip)` on each event

---

### Step 4.2 — Post Processing (Neon Glow)

1. In Package Manager: install **Universal Render Pipeline** (if not already)
2. Right-click in Project → **Create → Rendering → URP Asset**
3. In **Edit → Project Settings → Graphics** → set the URP asset
4. Select Main Camera → **Add Component → Volume**
5. Create a new **Volume Profile** → click **Add Override → Post-processing → Bloom**
6. Set Bloom **Intensity**: `1.5`, **Threshold**: `0.8`
7. This makes all emissive materials glow in-game

---

### Step 4.3 — WebGL Build for Luxodd

1. **File → Build Settings** → confirm WebGL is selected
2. **Player Settings → Publishing Settings:**
   - Compression Format: **Gzip**
   - Name Files as Hashes: ✓
   - Data Caching: ✓
   - Debug Symbols: **Off**
3. In code: ensure `Application.targetFrameRate = -1` (add to `GameBootstrap.Start()`)
4. Set WebGL Template to **LuxoddTemplate**
5. Click **Build** → choose an output folder → wait for build
6. Zip the output folder so that `index.html` is in the **root** of the zip (not in a subfolder)
7. Upload the zip to your game in the Luxodd Admin Portal

---

## Full Task Checklist

### Mechanics (Code)
- [ ] **Step 2.1** — Configure Input Action Asset with keyboard AND joystick bindings on Move, Jump, Dash
- [ ] **Step 2.2** — Update `GameManager.cs` with difficulty scaling per level
- [ ] **Step 2.2** — Update `RisingFloor3D`, `PlatformDecay`, `PlatformSpawner3D` to read difficulty
- [ ] **Step 2.3** — Write `CameraShake.cs` and call it from `GameManager.GameOver()`
- [ ] **Step 2.4** — Update `GameManager.cs` with combo multiplier (coin streak)
- [ ] **Step 2.5** — Write `GameOverScreen.cs` with 30s timeout and Luxodd Continue flow
- [ ] **Step 2.6** — Write `LeaderboardScreen.cs` with scroll and Luxodd API

### Unity Editor (Scene Work)
- [ ] **Step 1.1** — Set canvas to 1080×1920, switch to WebGL
- [ ] **Step 1.2** — Create Player object with all components and jetpack particle system
- [ ] **Step 1.3** — Create Platform prefab with neon material and spike children
- [ ] **Step 1.4** — Create Coin prefab
- [ ] **Step 1.5** — Place Rising Floor in scene
- [ ] **Step 1.6** — Place Death Zone at bottom
- [ ] **Step 1.7** — Place Laser Shooter and create Laser Projectile prefab
- [ ] **Step 1.8** — Configure Main Camera with CameraFollow3D
- [ ] **Step 1.9** — Build full HUD canvas with all text elements and indicators

### Luxodd Integration (Code + Editor)
- [ ] **Step 3.1** — Install Luxodd Unity Plugin
- [ ] **Step 3.2** — Add UnityPluginPrefab to scene
- [ ] **Step 3.3** — Write `GameBootstrap.cs` (connect, health check, profile fetch)
- [ ] **Step 3.4** — Add `SendLevelBeginRequestCommand` and `SendLevelEndRequestCommand` to `GameManager`
- [ ] **Step 3.5** — Confirm `HealthStatusCheckService.Activate()` is called after connection
- [ ] **Step 3.6** — Replace `SceneManager.LoadScene` with `BackToSystem()`

### Polish
- [ ] **Step 4.1** — Import and wire up all sound effects
- [ ] **Step 4.2** — Set up URP + Bloom post processing for neon glow
- [ ] **Step 4.3** — Build WebGL and upload to Luxodd Admin Portal

---

## Submission Checklist (Before Uploading)

- [ ] Build is WebGL, portrait 1080×1920
- [ ] Build size is under 100 MB
- [ ] `index.html` is in the root of the zip
- [ ] `Application.targetFrameRate = -1` is set in code
- [ ] Joystick works for all controls (keyboard bindings are also fine — they're ignored when no keyboard is connected on cabinet)
- [ ] Game ends within 10 minutes (rising floor guarantees this)
- [ ] `BackToSystem()` is always called when session ends
- [ ] `SendLevelBeginRequestCommand` called at run start
- [ ] `SendLevelEndRequestCommand` called at run end with final score
- [ ] Leaderboard is shown after game over
- [ ] Health check is active throughout gameplay
- [ ] No pausing
- [ ] Game Over screen auto-closes after 30 seconds
- [ ] Score is visible and updates during gameplay
- [ ] All menus navigable with joystick (no mouse required)
- [ ] No Debug.Log calls in the build (use `#if UNITY_EDITOR` guards or remove them)
- [ ] Game tested in Chrome browser before uploading

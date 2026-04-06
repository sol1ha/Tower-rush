# Luxodd & Arcade Plugin Integration Guide

This document covers the installation, configuration, and usage of the Luxodd Unity plugin and arcade controls for **Tower Rush**.

---

## 1. Plugin Overview

The Luxodd Unity plugin provides a step-by-step guide to integrating your project with the Luxodd ecosystem. It includes core components, classes, and commands for server communication.

### Features
- Required folders, DLLs, and connection files.
- Pre-built functionality for sending/receiving server commands.
- Example scene showcasing plugin usage.

---

## 2. Installation Guide

### Step 1: Download the Latest Unity Plugin
The `.unitypackage` contains everything needed for Luxodd integration.

### Step 2: Import into Unity
1. Open your Unity project.
2. Navigate to: **Assets > Import Package > Custom Package**.
3. Select the `.unitypackage` and click **Open**.
4. Review and click **Import**.

### Step 3: Install Dependencies
If prompted, install the **Newtonsoft.Json** package (used for JSON parsing). 
> [!NOTE]
> Check `Packages/manifest.json` for `com.unity.nuget.newtonsoft-json`. If missing, it should be installed via the Package Manager.

---

## 3. Configuration Guide

### Developer Token
To enable Debug Mode and authenticate with the server, enter your developer token:
1. Navigate to: **Tools > Luxodd Plugin > Set Developer Token**.
2. Paste the token obtained during registration.

### Server Environments
The plugin supports **Staging** (default) and **Production**.
To configure, edit the file at: `Assets/Luxodd.Game/Resources/NetworkSettingsDescriptor`.

---

## 4. Arcade Control Concepts

Arcade cabinets use a fixed physical control panel consisting of a joystick, multiple colored buttons, and a numeric keypad. In Unity, these are exposed as standard joystick inputs.

### Button Mapping Table
The following mapping is consistent across the SDK and should be treated as the single source of truth:

| Arcade Button Color | Unity Input |
| :--- | :--- |
| **Black** | `JoystickButton0` |
| **Red** | `JoystickButton1` |
| **Green** | `JoystickButton2` |
| **Yellow** | `JoystickButton3` |
| **Blue** | `JoystickButton4` |
| **Purple** | `JoystickButton5` |
| **Orange** | `JoystickButton8` (System/Help) |
| **White** | `JoystickButton9` (Back/Cancel) |

---

## 5. Implementation: The Adapter Pattern

Instead of reading input directly in gameplay code, use an **Adapter Interface**. This keeps your gameplay logic independent of the specific hardware.

### Input Adapter Interface (`IPlayerControlAdapter`)
```csharp
public interface IPlayerControlAdapter
{
    Vector2 MovementVector { get; }
    event Action JumpButtonPressed;
    event Action UseItemButtonPressed;
    bool IsFireButtonPressed { get; }
}
```

---

## 6. Testing the Integration

### Example Scene
Location: `Assets > Luxodd.Game > Example > Scenes > ExampleScene`.

**Main Test Functions:**
- **Connect to Server**: Initiates a server connection.
- **Get User Profile**: Requests the user's profile and credit balance.
- **Toggle Health Check**: Sends a health status check every 2 seconds.
- **Add/Charge Credits**: Tests credit transactions (requires PIN input).
- **User State Commands**: Tests server-side synchronization of internal state (Spaceship type, Level, etc.).

### Controlling Test Scenes
- **Basic Test**: Visualizes joystick and button inputs for debugging.
- **Advanced Gameplay Test**: A 2D action game showing movement, jumps, fire rate logic, and item interactions using the Adapter Pattern.

---

## 7. Building for WebGL

### Custom WebGL Template
The plugin package includes a custom template (e.g., **LuxoddTemplate**). This is essential for proper arcade deployment.

### Build Settings
1. **Resolution and Presentation**: Select `LuxoddTemplate` as the WebGL Template in Player Settings.
2. **Publishing Settings**: Configure Compression and Data Caching to match the reference guide.
3. **Testing in Browser**: To test server functionality, append the following to the URL:
   `index.html?token=your_dev_token`

---

## 8. Best Practices Checklist

- [x] **Separation of Concerns**: Gameplay code does not call `ArcadeControls` directly (use the adapter).
- [x] **Timing**: Reset/Read input in `Update`, apply physics in `FixedUpdate`.
- [x] **Events vs. State**: Use events for one-shot actions (Jump) and bools for held state (Fire).
- [x] **Fire Rate**: Implement fire rate logic in gameplay code, not input code.

---

## 9. In-Game Transactions

In-Game Transactions allow players to spend credits directly inside the gameplay flow (Continue or Restart) without leaving the game.

### Core Concepts: Continue vs. Restart

| Feature | **Continue** | **Restart** |
| :--- | :--- | :--- |
| **Usage** | Resume the same session (restore lives/time). | Finish current run and start a new session. |
| **Session State** | **Not Finalized**. Gameplay continues. | **Finalized**. Results are sent before popup. |
| **Logic Owner** | **Game** Handles the continuation. | **System** Handlers the restart flow. |
| **Buttons** | Continue, End. | Restart, End. |

### Integration Example (`InGameTransactionController.cs`)

Recommended usage involves an `InGameTransactionController` script that handles the pause/resume logic and communicates with the `WebSocketService`.

#### Minimal Implementation Pattern:
```csharp
public void OnGameOver(bool allowContinue, bool allowRestart)
{
    PauseGameplay();

    if (allowContinue) {
        _webSocketService.SendSessionOptionContinue(OnContinueResult);
    } else if (allowRestart) {
        // Send results BEFORE showing Restart popup
        _webSocketCommandHandler.SendLevelEndRequestCommand(() => {
            _webSocketService.SendSessionOptionRestart(OnRestartResult);
        });
    }
}
```

### Responsibility Matrix
- **Game Responsibility**: Pause/Resume gameplay, Restore lives/time, Handle Continue logic, Send session results.
- **System Responsibility**: Show popups, Create new sessions (on Restart), Balance & credit checks, Top-up flows.

### Verification Checklist
- [ ] Game supports Continue and/or Restart.
- [ ] Session results are sent **before** the Restart popup.
- [ ] `End` action is handled by calling `BackToSystem()`.
- [ ] No manual scene reload logic is implemented for Restart (handled by system).

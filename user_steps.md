# 🚀 Coastal Control - M0 Implementation Steps

Follow these steps exactly in Unity to implement the Bootstrap Refactor milestone.

---

## 📋 Phase 1: Unity Project Setup

### Step 1.1: Package Configuration
1. Open Unity Package Manager (Window > Package Manager)
2. Install these packages:
   - **Input System** (if not installed)
   - **TextMeshPro** (if not installed)
3. Configure Input System:
   - Go to Edit > Project Settings > Player
   - Under Other Settings, set "Active Input Handling" to "Both"
   - Unity will restart

### Step 1.2: Create Scene Assets
1. Create new folder structure:
   ```
   Assets/
   ├── _Bootstrap/
   ├── Core/
   ├── Gameplay/
   ├── UI/
   ├── Shared/
   ├── Scenes/
   │   ├── Bootstrap/
   │   ├── Harbor/
   │   └── Loading/
   ├── Art/
   │   ├── Materials/
   │   ├── Prefabs/
   │   └── Models/
   └── Settings/
       ├── Input/
       └── Physics/
   ```

2. Create **Bootstrap Scene**:
   - File > New Scene (select Empty template)
   - Save as `Assets/Scenes/Bootstrap/Bootstrap.unity`
   - Delete the default Main Camera
   - Create empty GameObject named "GameInitiator"

3. Create **HarborScene**:
   - File > New Scene (select Empty template)
   - Save as `Assets/Scenes/Harbor/Harbor.unity`
   - Add Directional Light (rotated to 50, -30, 0)
   - Add Main Camera at position (0, 5, -10) rotation (30, 0, 0)
   - Create Ground:
     - Create Cube, scale to (100, 1, 100)
     - Position at (0, -0.5, 0)
     - Name it "WaterPlane"
   - Create Dock:
     - Create Cube, scale to (10, 2, 4)
     - Position at (0, 1, 5)
     - Name it "Dock"
   - Create Player Spawn Point:
     - Create Empty GameObject
     - Position at (0, 1, 0)
     - Name it "PlayerSpawnPoint"

### Step 1.3: Input Actions Setup
1. Right-click `Assets/Settings/Input/` > Create > Input Actions
2. Name it "InputActions"
3. Double-click to edit:
   - Create Action Maps:
     - **Navigation** (boat controls)
     - **Tablet** (tablet UI)
     - **Pause** (game pause)

   - In Navigation Action Map:
     - Add Action "Move" (Type: Value, Control Type: Vector2)
     - Add Action "Look" (Type: Value, Control Type: Vector2)
     - Add Action "Boost" (Type: Button)
     - Add Action "Brake" (Type: Button)

   - In Tablet Action Map:
     - Add Action "OpenTablet" (Type: Button)
     - Add Action "CloseTablet" (Type: Button)
     - Add Action "Navigate" (Type: Value, Control Type: Vector2)
     - Add Action "Submit" (Type: Button)
     - Add Action "Cancel" (Type: Button)

   - In Pause Action Map:
     - Add Action "TogglePause" (Type: Button)

   - Save Asset
   - Generate C# Class (check the box in Inspector)

4. Set up input bindings:
   - Navigation/Move: WASD, Arrow Keys, Left Stick
   - Navigation/Look: Mouse Delta, Right Stick
   - Navigation/Boost: Left Shift, Left Bumper
   - Navigation/Brake: Space, Right Bumper
   - Tablet/OpenTablet: Tab, Y Button
   - Tablet/CloseTablet: Escape, B Button
   - Pause/TogglePause: Escape, Start Button

### Step 1.4: Create Basic Materials
1. Create materials in `Assets/Art/Materials/`:
   - **Water_Mat**: Blue color (50, 150, 255), smoothness 0.7
   - **Dock_Mat**: Gray-brown color (120, 90, 60), smoothness 0.3
   - **Boat_Mat**: White color (240, 240, 240), smoothness 0.4

2. Apply materials:
   - WaterPlane: Water_Mat
   - Dock: Dock_Mat

### Step 1.5: Basic UI Canvas Setup
1. Create UI in `Assets/Scenes/Loading/LoadingScene.unity`:
   - Create new scene, save as LoadingScene
   - Right-click in Hierarchy > UI > Canvas
   - Name it "LoadingCanvas"
   - Set Canvas Scaler:
     - UI Scale Mode: Scale With Screen Size
     - Reference Resolution: 1920x1080
     - Match: 0.5
   - Add loading elements:
     - Text: "Loading..." (center screen, large font)
     - Slider: Progress bar (center, below text)
     - Text: Status message (below progress bar)

---

## 📋 Phase 2: Code Implementation (I'll create these files)

### Step 2.1: After I create the assembly definitions:
1. You'll see these .asmdef files appear:
   - `Assets/Core/Core.asmdef`
   - `Assets/Gameplay/Gameplay.asmdef`
   - `Assets/UI/UI.asmdef`
   - `Assets/Shared/Shared.asmdef`

2. Unity may ask to reload - accept this

### Step 2.2: When I create the DI system:
1. You'll see the DI container setup in `Assets/Core/Services/DI/`
2. No action needed, just verify no compilation errors

### Step 2.3: When I create the GameInitiator:
1. The GameInitiator script will be attached to your Bootstrap scene's GameInitiator object
2. Set it up:
   - Select GameInitiator object
   - In Inspector, you'll see configurable fields
   - Assign the HarborScene to the "Initial Scene" field
   - Assign the LoadingScene to "Loading Scene" field

### Step 2.4: When I create the PlayerBoat:
1. Create PlayerBoat prefab:
   - Create a Capsule in HarborScene
   - Scale to (2, 1, 4) for boat shape
   - Add Rigidbody
   - Position at PlayerSpawnPoint
   - Apply Boat_Mat
   - Add the PlayerBoat controller script (I'll create this)
   - Drag to Project window to create prefab
   - Delete from scene (will be spawned by code)

### Step 2.5: When I create the Tablet UI:
1. Create Tablet UI prefab:
   - In HarborScene, create UI > Canvas
   - Name it "TabletCanvas"
   - Set Render Mode: Screen Space - Overlay
   - Add TabletUI script (I'll create this)
   - Add basic UI elements:
     - Background panel (center screen, tablet-sized)
     - Tab buttons placeholder
   - Save as prefab in `Assets/Art/Prefabs/UI/`
   - Delete from scene (will be spawned by code)

---

## 📋 Phase 3: Testing

### Step 3.1: Build Settings
1. File > Build Settings
2. Add both scenes:
   - Bootstrap.unity (index 0)
   - HarborScene.unity (index 1)
   - LoadingScene.unity (index 2)
3. Uncheck all scenes except Bootstrap
4. Bootstrap should be the only checked scene

### Step 3.2: Test Run
1. Make sure Bootstrap.unity is open
2. Press Play
3. Expected flow:
   - Loading screen appears
   - HarborScene loads
   - PlayerBoat spawns
   - You can move with WASD
   - Press Tab to open tablet
   - Press Escape to close tablet

### Step 3.3: Verify
- No singletons are used
- All services are properly injected
- Clean scene loading/unloading
- Input maps switch correctly between boat and tablet

---

## 🔧 Common Issues & Solutions

### Input System Errors
- If you get "Input System is not initialized", restart Unity
- Make sure "Active Input Handling" is set to "Both" or "Input System Package"

### Assembly Definition Errors
- If you see compilation errors about missing references:
  1. Save all scenes
  2. Close Unity
  3. Delete Library folder
  4. Reopen project (this rebuilds assembly references)

### Scene Loading Issues
- If HarborScene doesn't load:
  1. Check that scenes are in Build Settings
  2. Verify scene names match exactly in GameInitiator
  3. Check Console for error messages

### Input Not Working
- If boat doesn't move:
  1. Check InputActions asset is set up correctly
  2. Verify PlayerBoat has the InputAction asset reference
  3. Check that Navigation action map is enabled

---

## ✅ Success Criteria

When M0 is complete, you should have:

- [ ] Bootstrap scene loads immediately on Play
- [ ] Loading screen shows during HarborScene load
- [ ] HarborScene loads additively with water and dock
- [ ] PlayerBoat spawns and responds to WASD input
- [ ] Tablet UI opens/closes with Tab key
- [ ] Input maps switch between boat and tablet modes
- [ ] No singleton usage or DontDestroyOnLoad
- [ ] No compilation errors
- [ ] Clean scene unload when stopping play mode

---

## 📞 Next Steps

After completing these steps and verifying everything works:
1. Let me know any issues encountered
2. I'll provide the next phase implementation
3. We'll proceed to M1 (Tablet Operations Hub) together

Remember: This foundation is critical - getting the DI and scene loading right now will make all future features much easier to implement!
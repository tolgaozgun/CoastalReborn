# 🚀 Coastal Control - M1 Implementation Steps

Follow these steps exactly in Unity to implement the Tablet as Operations Hub milestone.

---

## 📋 Phase 1: Dependency Injection Setup

### Step 1.1: Install Zenject
1. Open Unity Package Manager (Window > Package Manager)
2. Click "+" button > Add package from git URL
3. Enter: `https://github.com/svermeulen/Zenject.git`
4. Wait for installation to complete

### Step 1.2: Verify Services Exist
After the code generation, verify these services are available:
- `CrewService` - Crew management and hiring
- `ContractsService` - Contract creation and management
- `IntelService` - Intelligence data handling
- `UpgradeService` - Ship upgrade system
- `TabletInstaller` - DI installer for tablet system

### Step 1.3: Create Tablet Installer
1. In your HarborScene, create empty GameObject named "TabletInstaller"
2. Attach `TabletInstaller.cs` script
3. Configure in Inspector:
   - Assign service configurations (or use defaults)
   - Assign UI prefabs (can leave empty for auto-creation)

### Step 1.4: Create Scene Context
1. Right-click in Hierarchy > Zenject > Scene Context
2. This enables DI in the scene
3. The Scene Context will automatically find and run installers
4. Verify TabletInstaller appears in the Installers list

---

## 📋 Phase 2: Tablet UI Implementation

### Step 2.1: Create Tablet Bootstrap
1. In HarborScene, create empty GameObject named "TabletBootstrap"
2. Attach `TabletBootstrap.cs` script
3. Configure in Inspector:
   - Auto Initialize: ✓
   - Don't Destroy On Load: ✓

### Step 2.2: Create Tablet UI Structure
1. Create UI Canvas:
   - Right-click in Hierarchy > UI > Canvas
   - Name it "TabletCanvas"
   - Set Render Mode: Screen Space - Overlay
   - Set Canvas Scaler: Scale With Screen Size, 1920x1080

2. Create Main Tablet Panel:
   - Create UI > Panel as child of TabletCanvas
   - Name it "TabletPanel"
   - Size: 1200x800, centered
   - Add `TabletUIController.cs` script

3. Create Header:
   - UI > Image as child of TabletPanel
   - Name: "Header"
   - Anchor: top-stretch, height: 100
   - Add Funds display (TMP_Text)
   - Add Close button

4. Create Navigation Tabs:
   - Create UI > Horizontal Layout Group as child of TabletPanel
   - Name: "NavigationTabs"
   - Add 4 buttons: Contracts, Crew, Intel, Upgrades

5. Create Content Panels (for each tab):
   - ContractsPanel: ScrollRect for contract listings
   - CrewPanel: Two ScrollRects (Available/Hired crew)
   - IntelPanel: ScrollRect for intelligence listings
   - UpgradesPanel: ScrollRect for upgrade listings

### Step 2.3: Set Up Screen Controllers
1. For each content panel, add the respective controller:
   - ContractsPanel: `ContractsScreenController.cs`
   - CrewPanel: `CrewScreenController.cs`
   - IntelPanel: `IntelScreenController.cs`
   - UpgradesPanel: `UpgradesScreenController.cs`

2. Configure each controller:
   - Assign scroll views
   - Assign filter dropdowns
   - Assign buttons

### Step 2.4: Create Item Prefabs
1. Contract Item Prefab:
   - Create UI > Panel as prefab
   - Name: "ContractItem"
   - Add `ContractItemController.cs`
   - Include: Title, Description, Reward, Difficulty, Region, Accept/Complete buttons

2. Crew Item Prefab:
   - Create UI > Panel as prefab
   - Name: "CrewItem"
   - Add `CrewItemController.cs`
   - Include: Name, Biography, Skills, Salary, Role, Hire/Fire buttons

3. Intel Item Prefab:
   - Create UI > Panel as prefab
   - Name: "IntelItem"
   - Add `IntelItemController.cs`
   - Include: Title, Source, Credibility, Verify/Cross-reference buttons

4. Upgrade Item Prefab:
   - Create UI > Panel as prefab
   - Name: "UpgradeItem"
   - Add `UpgradeItemController.cs`
   - Include: Name, Description, Cost, Purchase button, Prerequisites

---

## 📋 Phase 3: Data Models & Commands

### Step 3.1: Verify Data Models
Ensure these data model files exist in `Assets/Core/Data/`:
- `CrewData.cs` - Crew member information and stats
- `ContractData.cs` - Contract definitions and status
- `IntelData.cs` - Intelligence data structures
- `UpgradeData.cs` - Upgrade system data

### Step 3.2: Verify Service Interfaces
Ensure these interface files exist in `Assets/Core/Interfaces/`:
- `ICrewService.cs`
- `IContractsService.cs`
- `IIntelService.cs`
- `IUpgradeService.cs`

### Step 3.3: Verify Command System
Ensure command files exist in `Assets/Core/Commands/`:
- `ITabletCommand.cs` - Base command interface
- `ContractCommands.cs` - Contract-related commands
- `CrewCommands.cs` - Crew management commands
- `IntelCommands.cs` - Intelligence commands
- `UpgradeCommands.cs` - Upgrade commands
- `TabletCommandInvoker.cs` - Command executor with undo/redo

---

## 📋 Phase 4: Integration Setup

### Step 4.1: Wire Dependencies
1. Select the TabletBootstrap object
2. If needed, assign service prefabs to respective fields
3. Assign tablet UI prefab to Tablet UI field

### Step 4.2: Configure Tablet UI Controller
1. Select TabletPanel with TabletUIController
2. Assign service references (or leave null for auto-detection)
3. Assign UI component references:
   - Funds text
   - Navigation buttons
   - Screen panels

### Step 4.3: Configure Screen Controllers
For each screen controller:
1. Assign scroll views
2. Assign filter dropdowns
3. Assign item prefabs
4. Configure button listeners

---

## 📋 Phase 5: Testing

### Step 5.1: Basic Functionality Test
1. Press Play
2. Press Tab to open tablet
3. Verify:
   - Tablet appears with all 4 tabs
   - Funds display shows starting amount
   - Navigation buttons switch screens
   - Close button works

### Step 5.2: Contract System Test
1. Go to Contracts tab
2. Verify:
   - Available contracts appear
   - Filters work (Region, Difficulty)
   - Accepting contracts works (check Console)
   - Contract completion testing

### Step 5.3: Crew System Test
1. Go to Crew tab
2. Verify:
   - Available crew appear
   - Hiring crew works (check funds)
   - Hired crew appear in hired section
   - Role changes work
   - Firing crew works

### Step 5.4: Intel System Test
1. Go to Intel tab
2. Verify:
   - Intel entries appear
   - Filters work (Region, Source, Credibility)
   - Cross-referencing works
   - Crew verification works

### Step 5.5: Upgrade System Test
1. Go to Upgrades tab
2. Verify:
   - Upgrade nodes appear
   - Categories filter works
   - Purchase validation works
   - Progress tracking works

### Step 5.6: Command System Test
1. Perform actions in tablet
2. Verify:
   - Commands execute successfully
   - Undo functionality (Ctrl+Z) works in Editor
   - Console shows proper logging

---

## 🔧 Common Issues & Solutions

### UI Not Appearing
- Check that TabletCanvas is active
- Verify TabletUIController is assigned properly
- Check that button onClick events are linked

### DI/Services Not Working
- Ensure Zenject is properly installed
- Verify Scene Context exists in scene
- Check that TabletInstaller appears in Scene Context installers
- Verify all services are bound in TabletInstaller
- Check Console for Zenject binding errors

### Prefab Missing Errors
- Create the required item prefabs
- Assign prefabs in TabletInstaller configuration
- Verify controller scripts are attached to prefabs
- Check that factories are properly bound in installer

### Commands Not Working
- Check command invoker is bound in DI container
- Verify command factories are properly injected
- Check Console for Zenject resolution errors
- Ensure all command dependencies are available

### Injection Errors
- Verify [Inject] attributes are used correctly
- Check that interfaces are bound to implementations
- Ensure no circular dependencies exist
- Check Console for Zenject validation messages

---

## ✅ Success Criteria

When M1 is complete, you should have:

- [ ] Tablet opens/closes with Tab key
- [ ] All 4 tabs (Contracts, Crew, Intel, Upgrades) functional
- [ ] Contracts: View, accept, complete contracts
- [ ] Crew: Hire, fire, change roles, view stats
- [ ] Intel: View intel, verify with crew, cross-reference
- [ ] Upgrades: Browse, purchase upgrades with prerequisites
- [ ] Proper fund management across all systems
- [ ] Command pattern with undo/redo support
- [ ] Clean MVC architecture with DI
- [ ] No compilation errors
- [ ] Proper service initialization and event handling

---

## 📞 Integration Notes

### Key Features Implemented:
1. **Contract Management**: Complete lifecycle from available to completed
2. **Crew Management**: Hiring, firing, role assignment with effectiveness calculations
3. **Intelligence System**: Data gathering, verification, and cross-referencing
4. **Upgrade System**: Node-based upgrades with prerequisites
5. **Command Pattern**: All operations use commands with undo/redo support
6. **Event-Driven**: Services publish events, UI responds appropriately
7. **Clean Architecture**: SOLID principles, dependency injection, separation of concerns

### Input Integration:
- Tab key opens/closes tablet
- Esc key closes tablet (when open)
- UI uses standard Unity navigation system

### Data Persistence:
- Current implementation uses in-memory data
- Services generate mock data for demonstration
- Easy to extend with save/load functionality

### Performance Considerations:
- UI updates only when data changes
- Efficient filtering and searching
- Object pooling for UI items (can be added later)

---

## 🎯 Next Steps

After completing M1 implementation:
1. Test thoroughly with the criteria above
2. Let me know any issues encountered
3. I'll provide M2 implementation planning
4. We'll proceed to add actual gameplay mechanics and tablet interactions

The tablet system is now a fully functional operations hub ready for gameplay integration!
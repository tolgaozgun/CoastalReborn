# 🚨 Lean Game Design Document — “Coastal Control” (Updated)

This document reflects the new **Single Entry Point** + **Dependency Injection (DI)** architecture for a scalable, production-grade structure. The design focuses on clean lifecycle management and dynamic gameplay systems with an emphasis on inspection, patrol, and adaptive world response.

---

## 🧭 1. Core Concept

**“Coastal Control”** is a co-op naval border enforcement simulator.  
The player operates a **private border control contractor**, managing inspections, patrols, and anti-smuggling operations along a fictional coastline.

- Players begin with **dock-based inspections** (Papers, Please–style decision loops).
- As progression unlocks **patrol operations**, gameplay shifts into **dynamic chases**, **boarding actions**, and **contraband interceptions**.
- Enemy factions evolve their tactics over time, forcing the player to **upgrade tools** and adapt.

All management actions (contracts, intel, upgrades, crew assignment) are handled through an **in-game tablet UI**, eliminating the need for a separate HQ scene.

---

## 🧰 2. Gameplay Systems

### 2.1 Inspection Gameplay (Early Loop)
- Dock inspections are the entry point for new players.
- Ships arrive with **documents** (manifest, IDs, stamps) that can be correct, forged, or contain honest civilian errors.
- Player decisions:
  - Allow / Detain / Verify Further
  - Trigger deeper scans (e.g., registry cross-checks)
- Suspicion score calculated from:
  - Document mismatches
  - Error probability per region
  - Behavioral noise
- Outcomes:
  - Allow innocent civilians
  - Arrest smugglers
  - Reputation penalties for mistakes

### 2.2 Patrol & Chase (Mid Loop)
- Patrol mode unlocks after progression.
- Dynamic smuggler events:
  - Escape attempts
  - Mid-sea handoffs
  - Pirate raids (late game)
- Police patrol ships can pursue, disable, and **board** targets.
- Chase mechanics:
  - Distance / speed differential matters
  - Suspect escape timers
  - Tactical positioning
- Boarding triggers arrest UI, evidence review, and contraband seizure.

### 2.3 Scanner & Evidence System
- X-Ray Scanner reveals hidden cargo slots with concealment levels.
- Heat and cooldown management prevent constant use.
- Scanned evidence automatically logs into the **Tablet’s Intel tab**.
- Concealment tactics evolve (e.g., thermal dampeners, decoys) as factions escalate.

### 2.4 Contraband Networks & Factions
- Regions have **heat** and **faction influence** levels.
- Higher influence → smarter smuggler behavior:
  - Better forgeries
  - Faster boats
  - Decoy tactics
- Consequences of player action:
  - Missed arrests → heat increases
  - Successful seizures → influence shifts down
  - Persistent failure → escalation to pirate raids

### 2.5 Intel & Conflicting Information
- Multiple intel sources with **credibility ratings**:
  - Informants
  - Scans
  - Patrol logs
  - Civilians (low credibility)
- Players must make judgment calls with **uncertain information**.
- Conflicting intel is part of core gameplay tension.

### 2.6 Upgrades & Countermeasures
- Tied to faction adaptation:
  - Factions introduce counters to your tools.
  - Upgrades restore or exceed original power.
- Examples:
  - Thermal upgrade for scanner vs dampeners
  - Engine upgrades for chase
  - Better boarding gear for high-risk arrests

### 2.7 Co-op Multiplayer (Core Loop)
- 2–6 player co-op (drop-in/drop-out).
- Shared **Tablet state** across players (contracts, evidence, intel).
- Role specialization:
  - Pilot (navigation)
  - Inspector (documents)
  - Scanner Operator
  - Boarder (arrest)
  - Commander (intel & contracts)
- Emphasis on coordination and tactical decision-making.

---

## 🌍 3. World, Story & Tone

### Setting
A fictional **coastal state** plagued by smuggling and piracy.  
The government outsources border security to private contractors. You are one of them.

### Narrative Frame
- You start as a small operator with minimal equipment.
- As smuggling factions escalate, your company grows — forcing you to match their tactics.
- Late-game involves raids, high-speed pursuits, and moral trade-offs (efficiency vs civil liberty).

### Tone
- **Tense, grounded, procedural.**
- Inspired by:
  - *Papers, Please* (inspection & moral tension)
  - *Sea of Thieves* (naval feel)
  - *Ready or Not* (procedural enforcement tone)
  - *V Rising* (regional escalation)
- Focus on procedural stories emerging from systems, not scripted campaigns.

---

## 🧍 4. Character & Enemy Behaviors

### Player Roles
- Operate ship, inspect documents, use tools, make tactical decisions.
- In co-op, players split roles dynamically — no forced class locks.

### Civilian AI
- Compliant but error-prone.
- Paperwork mistakes may appear like smuggling.
- Responds to inspections (nervous, idle animations).

### Smuggler AI
- Behavior tree:
  - Idle → Bluff → Panic/Escape → Board/Resist
- Faction tactics:
  - Better forgery → harder inspections
  - Fast boats → tougher chases
  - Decoys → conflicting intel

### Pirate AI
- Aggressive raiders.
- Target both players and smugglers.
- May create chaotic third-party encounters at sea.

---

## 🖼 5. Art Style References

| Element         | Style Direction                                                     |
|-----------------|----------------------------------------------------------------------|
| World           | Stylized low-poly coastal environments, clear silhouettes            |
| Ships           | Practical stylization (mix of *Sea of Thieves* and *Ready or Not*)   |
| UI              | Minimalist amber/green military terminal aesthetic                   |
| Characters      | Simple human models with readable posture animations                |
| Animations      | IK-based gestures for scanning, arresting, inspecting                |
| VFX             | Light stylization (scanning cones, arrest flashes, heat glow)        |
| Mood            | Muted palette with pops of high-contrast highlights for alerts       |

---

## 🧠 6. Architecture & Technical Overview (Updated)

### 6.1 Single Entry Point Pattern
- **Empty Bootstrap Scene**
- `GameInitiator` controls:
  - Binding
  - Instantiation
  - Initialization
  - Preparation
  - Start Game Flow
- No MonoBehaviour Start/Awake dependencies outside of `GameInitiator`.

### 6.2 Contexts
- **CoreContext** (persistent):
  - Input, UI, Intel, Upgrades, Save/Load, Tablet
- **GameplayContext** (runtime, reloadable):
  - ShipManager, Suspicion, Patrol, EventDirector, Chase, Arrest

### 6.3 Dependency Injection
- All systems accessed through interfaces.
- Strict direction:
  - CoreContext → GameplayContext (allowed)
  - GameplayContext → CoreContext (not allowed)
- Context teardown clears memory at mission end.

### 6.4 Multiplayer
- Host authoritative state.
- Clients send input, receive replicated Tablet/Gameplay data.
- Drop-in/drop-out supported.

---

## 🧩 7. Prefabs & Responsibilities (Updated)

| Prefab                          | Responsibility                                                               |
|----------------------------------|------------------------------------------------------------------------------|
| `PlayerBoat_Police_S/M/L`       | Player movement, chase control, scanner mount                                |
| `CivilianShip`                  | Papers, civilian AI, docking behavior                                        |
| `SmugglerShip`                  | Escape logic, cargo slots, contraband                                        |
| `PirateShip`                    | Raid behavior, combat placeholder                                           |
| `Tablet_UI`                     | Management UI (contracts, intel, upgrades, crew)                             |
| `Dock_Kiosk`                    | Inspection interaction trigger                                              |
| `CargoSlot`                     | Scanable cargo space with concealment level                                 |
| `XRayTool`                      | Scanner logic, heat/cooldown                                                |
| `Waypoint`                      | Patrol paths                                                                |
| `EventBeacon`                   | Dynamic event visualization                                                |

---

## 🖼 8. UI / Canvas Layers (Updated)

| Canvas Layer            | Functionality                                                               |
|--------------------------|-----------------------------------------------------------------------------|
| HUD Canvas               | Speed, compass, chase overlay, scanner heat                                 |
| Tablet Canvas            | Core management (Contracts, Intel, Upgrades, Crew, Settings)                |
| Modal Canvas             | Arrest UI, Inspection UI, confirmation dialogs                             |
| Loading Screen Canvas    | Async boot/load feedback                                                   |
| Debug Overlay (Optional) | Suspicion breakdown, faction influence, scan diagnostics (for dev builds)   |

---

## 🗺 9. Scenes & Structure

- **Bootstrap** — empty scene, loads everything through GameInitiator.  
- **HarborScene** — Dock inspections, tutorial loop.  
- **PatrolScene** — High seas, chase and boarding gameplay.  
- **RaidScene** *(later)* — Pirate events and high-difficulty content.  
- **MainMenu** *(optional)* — thin UI layer before Bootstrap.

*All scenes loaded additively. CoreContext persists across scene loads.*

---

## 🧭 10. Inspirations & References

- **Papers, Please** — inspection tension & branching choices.  
- **Sea of Thieves** — naval feel and player freedom.  
- **Ready or Not** — procedural enforcement tone.  
- **V Rising** — regional difficulty escalation.  
- **XCOM** — macro/micro split (now on Tablet instead of base).

---

## 🧭 11. Progression Pillars

| Stage         | Core Loop                                 | Mechanics Introduced                                                                 |
|---------------|--------------------------------------------|----------------------------------------------------------------------------------------|
| Early Game    | Dock inspections                          | Suspicion engine, document forgeries, X-ray scanning basics                             |
| Mid Game      | Patrol operations                          | Chases, boarding, arrest flow, faction adaptation                                       |
| Late Game     | Escalation                                 | Upgrades vs evolving smuggler tactics, pirates, dynamic events, intel networks           |
| Endgame Co-op | Coordinated patrol enforcement             | Specialized roles, high pressure scenarios, multiplayer intel sharing & tactical play    |

---

## 🧠 12. System Pillars

- **Deterministic startup** with GameInitiator.
- **Context-driven lifecycle** with clean unload/reload.
- **Dependency injection** to avoid singletons and race conditions.
- **Procedural gameplay systems** for replayable loops.
- **Management UI-first** (Tablet), no out-of-loop HQ.

---

## 🏁 13. MVP Target

- Playable **inspection and patrol loop** with dynamic faction escalation.  
- Functional co-op for 2 players.  
- Tablet fully drives management features.  
- Robust DI architecture ready for extension.
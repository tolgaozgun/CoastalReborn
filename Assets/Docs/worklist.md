# Unity Implementation Timeline — Updated for Single Entry Point + DI
**Scope guard:** No HQ scene. All management (contracts, intel, upgrades, crew) lives in the **Tablet**.  
**Architecture:** Empty bootstrap scene → `GameInitiator` (single entry point) → DI container → CoreContext + GameplayContext.

---

## M0 — Bootstrap Refactor (Deterministic Startup)
**Goal:** Replace singletons & scattered Awake/Start with a single async startup sequence and DI-driven initialization. Game loads into Harbor with a boat and Tablet UI opening.

### Deliverables (Definition of Done)
- Empty **Bootstrap** scene with **one** object: `GameInitiator`.
- DI container created; **CoreContext** services bound.
- Loading screen displayed, Harbor scene loaded additively, **GameplayContext** bound, loading screen closed.
- Player boat moves; Tablet opens/closes; no singletons/DontDestroyOnLoad used by gameplay systems.

### Worklist & To-Dos
- **Project Setup**
  - [ ] Create **Bootstrap** scene (empty).
  - [ ] Remove `_Core` scene & Singleton patterns from managers.
  - [ ] Create **assembly definitions**: `Core.asmdef`, `Gameplay.asmdef`, `UI.asmdef`.  
        *Rule:* `Gameplay` depends on `Core`; `Core` cannot reference `Gameplay`.
  - [ ] Input System enabled; create **InputActions** asset (`Navigation`, `Tablet`, `Pause`).
- **DI & Entry Flow**
  - [ ] Create **DI Container** (service registration, resolution; no code here, design only).
  - [ ] Define **CoreContext** registrations: Input, UI, Save, Intel, Upgrades, Tablet Service, Audio (if any).
  - [ ] Define **GameplayContext** registrations: Ship Spawner/Pool, Suspicion, Event Director, Patrol Director.
  - [ ] Document **Initialization Phases**: _Bind → Instantiate (UI, pools) → Initialize (services) → Create (assets/content) → Prepare (place, wire) → Reveal → Start_.
- **Scenes & Loading**
  - [ ] Create **HarborScene** (flat water, a dock, clear navigation space).
  - [ ] Create **SceneLoader** service (design: additive load, active scene set, unload non-active except Bootstrap).
  - [ ] Loading Screen prefab: world-agnostic overlay; shows progress bar & status text.
- **Prefabs & Components (design only)**
  - [ ] `PlayerBoat_Police_Small`: Rigidbody boat controller; camera follow; input routing.
  - [ ] `Tablet_UI`: Canvas, tab buttons (Contracts, Intel, Upgrades, Crew, Settings), modal stack policy.
  - [ ] `Dock_InspectionBay`: trigger zone + gizmo; inspection flows later.
- **QA / Acceptance**
  - [ ] Enter Play → instantaneous Bootstrap load → Loading screen visible → HarborScene loads within 1s on dev machine.
  - [ ] Press key to open Tablet; cursor lock toggles; input maps switch; boat input suspended when Tablet open.

---

## M1 — Tablet as Operations Hub (Management Without HQ)
**Goal:** Implement Tablet tabs and navigation; stub in data pipelines; unify all management actions in Tablet.

### Deliverables
- Tablet tabs functional: **Contracts, Intel, Upgrades, Crew**, **Settings**.
- Modal/overlay patterns standardized (no overlapping un-closable panels).
- Contracts can be **viewed and accepted** (no mission generation yet).
- Intel list shows **mock entries** with credibility bars.
- Upgrades list shows **locked/unlocked** items with stat deltas preview (no effects applied yet).
- Crew tab lists **assigned slots** (functional later).

### Worklist & To-Dos
- **UI Architecture**
  - [ ] UI Canvas policy: one **HUD Canvas**, one **Tablet Canvas** (Overlay), one **Overlay/Modal Canvas**.
  - [ ] Navigation model: tabs (left), content panels (right), breadcrumb for sub-pages.
  - [ ] Tablet open/close animation guidelines; focus management; controller navigation grid.
- **Data & Services**
  - [ ] `ContractsService` (data source only): list of mock contracts with difficulty, rewards, region tags.
  - [ ] `IntelService`: returns list of intel entries with credibility 0–100 and sources.
  - [ ] `UpgradeService`: static tree data (nodes, costs, prerequisites, effects descriptor).
  - [ ] `CrewService`: stub employees, role slots (Driver/Inspector/Boarder/Gunner).
- **UX Details**
  - [ ] Accept Contract workflow: accept → status badge shows “Tracked”.
  - [ ] Intel detail page: source, timestamp, credibility, related region, “Cross-check” action (disabled now).
  - [ ] Upgrade page: show **effect preview** and **requirements**; purchase disabled.
- **QA / Acceptance**
  - [ ] Tablet navigates smoothly; no input bleed to boat controls.
  - [ ] All tabs populated with mock data; zero errors on rapid switching.
  - [ ] Contracts can be marked Active; status persists during session.

---

## M2 — Dock Inspection MVP (Suspicion v1)
**Goal:** Inspect docked ships; view papers; compute suspicion; Allow/Hold decisions with basic consequences.

### Deliverables
- Civilian ships spawn and queue at dock.
- Each ship has **generated documents** (origin, destination, manifest, stamps).
- Suspicion score computed from:
  - Doc mismatches (weights)
  - Civilian random errors (noise)
  - Behavior cues (simple timers / idle shifts)
- Tablet **Inspection** sub-panel: mismatch highlights, **Allow/Hold** actions.
- HUD shows current target ship and **Suspicion meter**.

### Worklist & To-Dos
- **Prefabs**
  - [ ] `Ship_Civilian_S/M`: colliders, nav target, `PapersData` (data-only), `InspectionHandle` (interact anchor).
  - [ ] `Dock_Kiosk`: interactor that opens target’s Inspection sub-panel.
- **Services**
  - [ ] `SuspicionSystem`: weights per field; computes aggregate; returns breakdown for debug.
  - [ ] `DocumentGenerator`: applies **CivilianErrorProfile** per region (typos, stamp wear, off-by-one counts).
- **Tablet UX**
  - [ ] Inspection sub-panel: field list with red/amber highlights; suspicion delta tooltips.
  - [ ] Decision buttons → event posted: `InspectionResolved(Allowed/Held, Score)`.
- **Game Flow**
  - [ ] Allow → ship departs; Hold → detained state (parking spot).
  - [ ] Session summary panel (temporary) listing decisions & scores.
- **QA / Acceptance**
  - [ ] 10–20 inspections vary meaningfully; false positives possible.
  - [ ] No null refs on opening/closing Inspection repeatedly.

---

## M3 — X-Ray Scanner MVP (Heat, Cooldown, Evidence)
**Goal:** Scan ships at close range; reveal cargo slots with contraband chance; manage heat; log evidence.

### Deliverables
- X-ray tool (handheld or ship-mounted) with **range**, **heat**, and **cooldown**.
- Cargo slots on ships with **concealment levels**; reveal gates (distance, angle).
- Evidence items captured and listed in Tablet **Intel → Evidence** sub-tab.

### Worklist & To-Dos
- **Prefabs**
  - [ ] `XRayTool`: mount point on boat; UI reticle; heat meter binding.
  - [ ] `CargoSlot` on ships; state flags (empty/visible/contraband).
- **Services**
  - [ ] `ScannerService`: parameters (heat per scan, passive cooldown rate, range, cone angle).
  - [ ] `EvidenceLogService`: capture entries (ship id, time, slot id, outcomes).
- **UI**
  - [ ] HUD heat bar & “Overheated” lockout messaging.
  - [ ] Intel → Evidence list (filters, sorting).
- **Game Flow**
  - [ ] Scan attempts consume heat; threshold → overheat → temporary lockout.
  - [ ] Successful reveal marks slot as “probable contraband” (confidence % based on concealment).
- **QA / Acceptance**
  - [ ] Overheat triggers reliably; cooldown returns control.
  - [ ] Evidence entries persist through scene reload within session.

---

## M4 — Patrol & Chase MVP (Escape Events, Boarding & Arrest)
**Goal:** Free navigation patrol; suspects may flee; chase mechanics; boarding and arrest flow.

### Deliverables
- **PatrolScene** with waypoints, traffic spawner, fog/visibility modifiers (simple).
- Escape logic: if proximity + suspicion > threshold, target flees.
- **Chase Overlay**: distance, suspect status, escape timer.
- Boarding window: relative velocity & distance check; **Board** → simplified arrest panel.

### Worklist & To-Dos
- **Prefabs**
  - [ ] `Smuggler_Ship_Sprint` (fast runner).
  - [ ] `BoardingZone` (visual gizmo at suspect ship).
- **Services**
  - [ ] `PatrolDirector`: spawns traffic; injects escape events from active contracts/intel.
  - [ ] `ChaseController`: determines escape time based on map boundaries and speed delta.
  - [ ] `ArrestService`: outcome, contraband seize, reputation delta.
- **UI**
  - [ ] Chase overlay: progress ring to escape; guidance arrow.
  - [ ] Arrest panel: contraband summary; confirm outcomes.
- **QA / Acceptance**
  - [ ] Player can trigger, pursue, and either board/arrest or lose target.
  - [ ] Losing target updates active contract status and region stats.

---

## M5 — Factions & Regions MVP (Adaptive Pressure)
**Goal:** Regions with **influence** and factions with **tactics** that adapt to player actions.

### Deliverables
- Region cards in Tablet Map: **Calm Harbor**, **Fishing Flats**, **Industrial Bay** (initial set).
- Faction influence 0–100 per region; changes from seizures/misses.
- Tactics apply penalties (e.g., **Thermal Dampeners**: scanner efficacy −20%; **Fast Runners**: smuggler speed +10%).

### Worklist & To-Dos
- **Data**
  - [ ] `RegionData`: spawn weights, visibility multipliers, civilian error rates.
  - [ ] `FactionData`: tactics list, escalation thresholds.
- **Services**
  - [ ] `RegionService`: track influence, heat; notify on changes.
  - [ ] `FactionDirector`: choose tactics from thresholds; broadcast active penalties.
- **Tablet UX**
  - [ ] Map: region influence bars; “Active Tactics” badges with tooltips.
  - [ ] Event feed: “Faction adopted Thermal Dampeners”.
- **QA / Acceptance**
  - [ ] Influence changes are visible and affect traffic composition and difficulty.
  - [ ] Tactic changes visibly impact scanner/chase success rates.

---

## M6 — Documents v2 (Forgery Tiers & Verification Actions)
**Goal:** Make inspections deeper: **forgery quality**, **verification actions** with **time costs** and **uncertainty reduction**.

### Deliverables
- Forgery tiers (Low/Med/High) altering which fields are forged and subtlety.
- Verification actions (registry check, cargo recount, language verification) impose **timers** and reduce suspicion or reveal mismatches.
- **Penalty system** for false detains; reputation and small fines.

### Worklist & To-Dos
- **Data**
  - [ ] `ForgeryProfile`: manipulated fields and detection difficulty.
  - [ ] `VerificationAction`: time cost, target field, expected reduction.
- **Services**
  - [ ] `DocumentVerifier`: queue verification actions; tick time; apply outcomes.
  - [ ] `PenaltyService`: false positive/negative consequences.
- **Tablet UX**
  - [ ] Action buttons with spinners; previews for suspicion deltas.
- **QA / Acceptance**
  - [ ] Some ships clean but noisy; verification meaningfully improves certainty.
  - [ ] Over-verifying costs time and reduces throughput (player tradeoff).

---

## M7 — Co-op MVP (2–4 Players, Shared Tablet State)
**Goal:** Host/client patrol & inspection loop; shared Tablet data; role soft-division.

### Deliverables
- Host/Join flow (through Tablet Settings for now).
- Host authoritative physics on boats; clients send inputs.
- Shared Tablet state: contracts, intel, evidence, upgrades.
- Role indicators (Driver/Inspector/Boarder/Gunner) — cosmetic assignment for now.

### Worklist & To-Dos
- **Networking**
  - [ ] Sync services: Ship Spawner, Evidence, Suspicion, Contracts.
  - [ ] Late join: reconstruct UI from authoritative state.
  - [ ] Minimal reconciliation for chase markers.
- **UX**
  - [ ] Clear host ownership of boat movement.
  - [ ] Tablet edits that change state require host confirmation or RPC gating.
- **QA / Acceptance**
  - [ ] Two clients can complete a patrol, perform an inspection, arrest a suspect, and see identical Tablet data.
  - [ ] Disconnect/reconnect recovers state without hard reset.

---

## M8 — Upgrades & Countermeasures (Reactive Tech)
**Goal:** Upgrades that **specifically counter** faction tactics; all purchased in Tablet.

### Deliverables
- Upgrades applied to **scanner**, **engines**, **boarding gear**, **intel**.
- Tactics reduce base effectiveness; upgrades restore or exceed.
- Unlock **police ship variants** later in loop (S/M/L).

### Worklist & To-Dos
- **Data**
  - [ ] `UpgradeNode`: category, cost, effects, prerequisites.
  - [ ] Mapping from tactics → affected stats.
- **Services**
  - [ ] `UpgradeService` effects broadcast → subscribed systems apply deltas.
- **Tablet UX**
  - [ ] “Countered by” badge on tactics when relevant upgrade owned.
- **QA / Acceptance**
  - [ ] Noticeable before/after on scanner heat/range and chase success rates.
  - [ ] Upgrade gating feels meaningful but not grindy.

---

## M9 — Dynamic Events v2 (Decoys, Handoffs, Pirate Harassment)
**Goal:** Expand patrol variety with **telegraphed events** and clear choices.

### Deliverables
- Event types: **Decoy Convoys**, **Mid-sea Handoffs**, **Pirate Harassment**.
- Telegraphs: **radio chatter**, **flares**, **map pings**.
- Event success/failure feeds into region influence & tactic changes.

### Worklist & To-Dos
- **Prefabs**
  - [ ] `EventBeacon` (visual marker), `Convoy_Group`, `Pirate_Ship_M`.
- **Services**
  - [ ] `DynamicEventDirector`: schedules events by region heat/influence, contract priority.
  - [ ] `TelegraphSystem`: unify audio/UI/world cues.
- **UI**
  - [ ] Tablet Map event cards: timers, credibility, suggested response.
- **QA / Acceptance**
  - [ ] Events occur at tunable cadence; never invisible or unfair.
  - [ ] Player choice of which lead to pursue is meaningful.

---

## M10 — Persistence, Tuning & Production Readiness
**Goal:** Save/load, performance, instrumentation, UX polish.

### Deliverables
- Save slots for **contracts**, **upgrades**, **evidence**, **reputation**, **regions**.
- Object pooling for ships & markers; profiler budget respected.
- Tablet UX: helper tooltips, tutorial hints, consistent accessibility.
- Audio pass: feedback for scans, overheat, arrests, chase states.

### Worklist & To-Dos
- **Persistence**
  - [ ] Save/Load design: versioned records; safe writes; resume last session.
- **Performance**
  - [ ] Pools for ship prefabs, UI markers; physics step tuning; LOD/culling groups.
  - [ ] Addressables/Asset labels for heavy art later (optional).
- **UX/Tutorial**
  - [ ] Timed hints on first inspection, first chase, first overheat.
  - [ ] Error-proofing: cannot board at high speed; button disable states clear.
- **QA / Acceptance**
  - [ ] Long sessions stable; memory stable after scene unload (GameplayContext cleared).
  - [ ] Save/Load in Harbor & Patrol verified; no duplicate registrations.

---

# Cross-Cutting Technical Checklists

## A. Services & Contexts
- [ ] All systems expose **interfaces**; concrete implementations bound in container.
- [ ] **CoreContext**: Input, UI, Save, Intel, Upgrades, Tablet, Audio.
- [ ] **GameplayContext**: ShipManager/Pool, Suspicion, Patrol, EventDirector, Chase, Arrest.
- [ ] **No MonoBehaviour** runs critical logic in `Awake/Start`; only `Initialize()` via entry flow.

## B. Prefab Rules
- [ ] Prefabs are **pure data + components**; no global access.
- [ ] Script references acquired via **injection** or **initializer methods** at spawn.
- [ ] Layer & Tag matrix documented (Ship, PlayerShip, Dock, ScanTarget, UI).

## C. Input & UI
- [ ] Separate input maps for Navigation vs Tablet; context switching enforced by UI Manager.
- [ ] One source of truth for modal stack; Tablet never overlaps destructive dialogs.
- [ ] Controller navigation routes defined per panel.

## D. Telemetry & Debug
- [ ] On-screen debug toggles: suspicion breakdown, scanner heat, region influence.
- [ ] Event log panel: inspections resolved, scans, arrests, faction changes.
- [ ] Designer console commands (non-shipping): spawn smuggler, set influence, trigger event.

## E. Balancing Hooks
- [ ] All key numbers (weights, timers, cooldowns, spawn rates) in **ScriptableObjects**.
- [ ] `Tuning` folder with curated presets (Casual, Default, Hardcore).

---

# Risks & Mitigations
- **Race conditions reintroduced by stray components** → *CI check / GUID search for Awake/Start usage in gameplay scripts; review gates.*
- **DI sprawl** → *Group bindings in Installer classes per context; keep surface area small.*
- **Network desync** → *Authoritative host; clients only send inputs; state replicated from services.*
- **UI complexity** → *Strict modal policy; always-visible breadcrumbs; limit nested dialogs to 2 levels.*

---

# Minimal Success Path (Playtestable at Each Milestone)
- **M0–M1**: Boat + Tablet UI navigable (management stubs).
- **M2–M3**: Inspect → Scan → Evidence loop feels like a game.
- **M4**: Patrol → Chase → Board → Arrest constitutes a complete session.
- **M5–M6**: Adaptive factions + deeper documents add replay variety.
- **M7–M9**: Co-op + upgrades + dynamic events create a living loop.
- **M10**: Save/load + perf polish → production-ready backbone.

---
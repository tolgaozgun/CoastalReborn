# Game Development Best Practices & Studio Guidelines

This document outlines universal standards, architectural patterns, and development principles suitable for any professional game studio. It serves as a reference for developers to maintain code quality, consistent structure, and scalable project organization across all projects.

---

## 1. PROJECT ARCHITECTURE

### Single Entry Point (SEP)
- Establish a single, unified entry point for the application.  
- Use an initialization scene or bootstrap system that loads before all others.  
- Centralize dependency binding, configuration, and global services here.

### Dependency Injection (DI)
- Use DI to decouple systems and improve maintainability.
- Bind implementations to interfaces.
- Keep installers or registrars organized per domain or feature.

### Scene/State Initialization
- Each scene or feature module should have a clear **Entry** and **Exit** flow.
- A dedicated “Initiator” or “Coordinator” object should manage these flows.
- Avoid scattering initialization logic across multiple classes.

---

## 2. RESPONSIBILITY DIVISION

### Installers / Bindings
- Bind services, controllers, and data models to their interfaces.
- Maintain consistent DI patterns across all projects.

### Commands (Action Executors)
- Use Commands to orchestrate multi-step operations.  
- Commands should be the only classes coordinating multiple systems.  
- Avoid event spaghetti by using explicit command execution where applicable.  
- Commands must resolve their own dependencies to prevent circular references.

### MVC (or Similar Decoupled Pattern)
**Data (Model):**
- Holds state and simple logic only.
- Should not trigger events or hold references to Views or Controllers.

**View:**
- Contains only visual behavior and component references.
- Never executes game logic.
- Only responds to instructions from Controllers.

**Controller:**
- Coordinates logic between Data and View.
- Invokes Commands for complex flows.
- Should never contain unrelated responsibilities.

### Services
- Provide reusable, non-visual functionality.
- Must not depend on specific game states.
- Should be accessible from any feature when necessary.

### Factories
- Centralize object creation logic.
- Support pooling, dependency injection, or custom instantiation rules.

### Utilities & Extensions
- Use static utility and extension classes for common operations.
- Keep these generic and reusable across projects.

### State System
- Represent game flow as distinct states with `Enter` and `Exit` methods.
- Ensure only one state is active at a time.
- Maintain strict separation of responsibilities per state.

---

## 3. PROJECT STRUCTURE & DOMAINS

### Domain-Based Folder Structure
- Organize the project by **domains** (e.g., Core, UI, Gameplay, Meta).  
- Each domain should contain all scripts and assets relevant to it.

### Assembly Definitions
- Each domain should have its own Assembly Definition file.
- This:
  - Enforces separation of concerns.
  - Improves compile times.
  - Prevents accidental cross-domain dependencies.

---

## 4. DESIGN PATTERNS & STANDARD TECHNIQUES

### Centralized Update Management
- Use an update subscription system instead of scattering Update() calls.
- Only systems that require continuous updates should register.

### Camera Separation
- Use separate cameras for world and UI rendering.
- Prevent post-processing or render effects from affecting UI.

### Object Pooling
- Reuse frequently created and destroyed objects.
- Reduce runtime allocations and improve performance.

### Asset Loading
- Use an async-friendly content loading system (e.g., Addressables or custom tools).
- Support remote asset loading when scaling projects.

### Version Compatibility Strategy
- Avoid embedding scripts inside assets that must load across multiple game versions.
- Keep runtime and data assets isolated from code changes when possible.

### Unit Testing
- Prioritize unit testing of core logic and services.
- Integrate test validation in CI/CD pipelines.

### Animation Approach
- Use simple, lightweight animation systems for UI and simple objects.
- Use full animation controllers only for complex, behavior-heavy characters.

### Prefabs & Reusable Objects
- Prefer prefab-driven design to avoid scene merge conflicts.
- Keep prefab hierarchies clean and documented.

### Lighting & Optimization
- Use baked lighting when feasible for static environments.
- Optimize materials for batching and consistent performance.

### Shaders Over CPU Animations
- Prefer shader-driven motion or effects when suitable.
- Leverage GPU parallelism for better performance.

### Logging
- Maintain consistent logging practices across all systems.
- Ensure logs are informative and easy to filter.

### Tweens
- Use a tweening library for simple, code-driven animations.

### Persistent Data
- Encrypt local persistent data where applicable.
- When possible, centralize player data on a secure backend service.

### Editor Tools
- Implement editor utilities to enhance productivity and performance.
- Use custom inspectors for repetitive or error-prone configuration tasks.

---

## 5. CODING STANDARDS

- Follow industry-standard coding conventions (e.g., Microsoft C# Standards).
- Use meaningful naming for classes, methods, and variables.
- Avoid abbreviations unless widely accepted.
- Keep code self-documenting; comments should be used only when absolutely necessary.

---

## 6. ASYNC OPERATIONS

- Use async/await patterns with structured cancellation.
- Always wrap async calls in try/catch blocks.
- Pass CancellationTokens to all async methods.
- Provide domain-specific token sources for cleanup and predictable termination.

---

## 7. CLEAN CODE PRINCIPLES

### Self-Documenting Code
- Write code that clearly communicates intent without comments.

### Single Responsibility Principle
- Classes and methods should have one clear responsibility.

### Small & Focused Structures
- Keep classes and methods small and tightly scoped.

### Avoid Deep Nesting
- Flatten branching logic to improve readability.

### Avoid switch Statements
- Use polymorphism or strategy patterns instead.

### DRY (Don’t Repeat Yourself)
- Avoid duplicating logic unless duplication clarifies intent more than abstraction.

### Immutable Getters
- Getters must not mutate internal state or trigger side effects.

### No Magic Numbers
- Store constants in properly named `const` variables.

---

## 8. SUMMARY

This guideline establishes a foundation for building maintainable, scalable, and high-performance games. By following these principles, a studio can ensure consistency across teams, reduce technical debt, and streamline the development pipeline for any project.

---
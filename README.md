# 🏆 LAVA QUEST EVENT MODULE

A high-fidelity, performance-focused implementation of a competitive meta-game event, designed to be integrated as a plug-and-play module within a larger, existing mobile title (simulated Core Architecture).

## 🏛 ARCHITECTURE: EVENT-DRIVEN & DECOUPLED

The project strictly adheres to **Single Responsibility Principle (SRP)** and **Decoupling** (achieved through C# Events/Actions) to ensure maintainability and testability within a vast production environment.

| Component | Responsibility | SOLID |
| :--- | :--- | :--- |
| **MainCanvasManager** | Traffic Cop / Composition Root. Handles high-level UI state switching (Lobby -> Map -> Game). **Injects** dependencies. | SRP |
| **LavaQuestPanelController** | Event Logic. Manages round flow, elimination calculation, and delegates animation commands. **Does not handle UI visibility.** | SRP |
| **AvatarView** | Pure View/Actor. Responsible only for visual representation and executing received animation commands (Jump, Fall, Pop). | SRP |
| **ObjectPooler** | Memory Management. Provides reusable Avatar instances on request (eliminating Instantiate/Destroy spikes). | SRP |

## ✨ KEY IMPLEMENTATION DETAILS

1.  **Juice & Game Feel:** Achieved high polish by using custom Coroutines and **Animation Curves** for all movement (jump, fall, pop-in).
2.  **Staggered Animation:** Avatars move sequentially with calculated delays, giving the visual impression of a dynamic, live event rather than a synchronized script.
3.  **Adaptive Positioning:** Avatars are positioned using **Anchored Position** and calculated offsets based on a custom Diamond/Pyramid pattern, ensuring the layout remains consistent across all screen resolutions (Adaptive UI).
4.  **Performance Fixes:**
    * **Object Pooling:** Used for all Avatars and VFX elements.
    * **Animation Safety:** Implemented `ForceStop()` logic to prevent Coroutines from freezing and breaking when the main panel is disabled.
    * **Input Lock:** `isRoundInProgress` state ensures the Play button cannot be spammed during animation cycles.
5.  **Lazy Initialization:** The Avatar population is delayed until the user clicks 'Start' on the Lobby Popup, preserving initial app memory.

## 🛠 SETUP INSTRUCTIONS

* **Target Version:** Unity 6000.0.58f2 (or 2022 LTS/URP equivalent).
* **Dependencies:** `GameSessionBridge` (Simulated Core Service) and `ObjectPooler` must be active in the scene.

***
*Developed by [Adınız] as a technical demonstration for Loom Games.*

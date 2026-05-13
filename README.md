# Vectorial-Puzzle-Engine
A C#-based puzzle game core built on vectorial math and modular OOP level architecture.

##  Core Concept & Gameplay Loop

At its core, the game is built around a single, straightforward objective: hitting a target with a turret. However, by introducing strategic obstacles and interactable mechanics, this simple premise evolves into a complex, deterministic puzzle experience.

The system derives its depth from the emergent interactions of a limited set of core objects, allowing for a vast array of logical solutions:
* **Mirrors:** Deflect the projectile's trajectory to navigate around obstacles.
* **Portals:** Bypass spatial constraints, granting access to seemingly unreachable areas on the grid.
* **Splitters:** Duplicate the projectile, enabling the player to hit multiple targets simultaneously within a single level.

By combining these fundamental tools, the engine generates highly diverse and challenging puzzles from a very minimal set of rules.

##  Production Tools & Level Design Automation

Beyond the runtime architecture, this project features a robust suite of custom Unity Editor tools engineered specifically to accelerate the production pipeline. These tools eliminate tedious manual scene creation, favoring a highly automated, data-driven approach:

* **Procedural Image-to-Level Generator:** Decodes 2D pixel data (PNG maps) into exact 3D spatial coordinates, converting simple visual sketches directly into fully playable, logical grids.
* **Modular Level Combiner:** The crown jewel of the pipeline. It takes segmented level chunks and dynamically recombines them, procedurally generating a vast volume of unique level variations in seconds.
* **Surface-Aware Instantiation:** Eliminates manual coordinate entry by detecting ground surfaces via raycasting, instantly snapping objects to the grid upon clicking.
* **Linear Interpolation Filler:** Automatically populates specified prefabs between two coordinate points, drastically speeding up the creation of boundaries and structural elements.
* **Batch Prefab Replacer:** Instantly swaps existing level assets with new prefabs across the entire scene. This allows for rapid visual theme changes and iteration without disrupting the underlying puzzle logic.

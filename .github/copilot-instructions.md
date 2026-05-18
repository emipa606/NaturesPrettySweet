# GitHub Copilot Instructions for Nature's Pretty Sweet (Continued)

## Mod Overview and Purpose

**Nature's Pretty Sweet (Continued)** is an updated version of the original mod by tkkntkkn, designed to enrich the environmental mechanics of RimWorld. By introducing dynamic terrain elements, weather patterns, and diverse biomes, this mod aims to make the game's environment a more integral and challenging aspect of gameplay. The mod utilizes Asset Bundles to improve performance and reduce file size.

## Key Features and Systems
- **New Weather Effects**: Enhanced weather dynamics such as dust storms, overcast, volcanic ash, etc.
- **Biome Diversity**: Introduction of unique biomes like Redwood Forest, Volcanic Fields, and Desert Oasis, each with distinct flora and climatic features.
- **Environmental Interactions**: Features like flooding, swimming, hot and cold springs, and more, responsive to the in-game environment.
- **Realism and Immersion**: Adds effects like frost on surfaces, visible breath in cold temperatures, and vegetation soaking/withering with weather changes.
- **Rimbrellas Support**: Implements functionality to complement items from the Rimbrellas mod.
- **Performance improvements**: Optimized systems for less resource-intensive gameplay.

## Coding Patterns and Conventions

- **Naming Conventions**: Classes and methods follow C# conventions with PascalCase. For example, `Building_SteamVent` and `startSpray()`.
- **Inheritance**: Extensively used to extend functionality from base classes. See `BiomeWorker_TemperateForest` inherited by `NPS_BiomeWorker_Savanna`.
- **Comments and Documentation**: Maintain detailed inline comments to clarify the purpose and functionality of complex systems.

## XML Integration

- Use XML files for defining custom elements in the mod, such as biome and weather definitions (`ElementSpawnDef`, `HediffDefOf`).
- Implement `LoadDataFromXmlCustom(XmlNode xmlRoot)` in classes for XML data manipulation, ensuring robust data handling.

## Harmony Patching

- Implement Harmony patches for modifying existing game code under the `HarmonyMain` class.
- Use reflection and careful method interceptions to ensure mod compatibility with future game updates.
  
## Suggestions for Copilot

- **Pattern Recognition**: Encourage repetitive pattern recognition for class definitions and Harmony patches. Suggest templates for new classes, especially common mod components like `ThingWithComps`.
- **Code Suggestion**: Facilitate code suggestions for rimworld-specific classes and methods such as `ThingComp`, `MapComponent`, `BiomeWorker_AridShrubland`, etc.
- **Library Information**: Provide insights or documentation suggestions for RimWorld libraries and namespaces being utilized.
- **Auto-completion Tips**: Aid with XML tag completion and usage within RimWorld's mod context.
- **Optimization Recommendations**: Highlight non-optimized code patterns and suggest improvements based on performance best practices.

**Note**: As modding RimWorld involves modifying the vanilla game, ensure compatibility checks and playtest extensively. Keep up-to-date with RimWorld's latest patches and the modding community for any new practices or frameworks to adopt. Always conduct thorough testing, especially after implementing Harmony patches or significant game-altering features.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.

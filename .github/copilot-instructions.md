# GitHub Copilot Instructions for Nature's Pretty Sweet (Continued)

## Mod Overview and Purpose

Nature's Pretty Sweet (Continued) is a RimWorld mod that enhances the environmental experience by introducing new terrain elements, weather conditions, and biomes. Originally created by tkkntkkn, this update brings a dynamic and interactive natural world where the environment itself becomes a living entity. The mod influences how maps generate, updates weather effects, and adds plant and terrain diversity to create a unique survival experience. 

## Key Features and Systems

1. **Additional Terrain Elements**: Includes unique biomes such as Desert Salt Flats, Redwood Forest, and Volcanic Fields with their respective features like salt harvesting or dangerous lava flows.
2. **Weather and Environmental Effects**: Introduces weather phenomena such as dust storms, volcanic ash, and seasonal changes, affecting soil fertility and plant growth.
3. **Swimming and Cooling**: Pawns can now swim and utilize bodies of water to manage their body temperatures in hot climates.
4. **New Plants and Animal Behaviors**: Adds wild versions of crops and seasonal behaviors for wildlife such as migration in the savanna.
5. **Biome Interactions**: The mod introduces interactions such as soil fertility enhancement from rainfall and snow melt, and event-driven changes like river flooding.
6. **Compatibility and Performance**: Optimized for RimWorld 1.6's asset bundle support and compatible with other mods like Odyssey by automatically disabling conflicting features.

## Coding Patterns and Conventions

- **C# Coding Style**: The mod follows the standard C# conventions comprising descriptive identifiers, organized namespaces reflecting the mod's structure, and consistent use of access modifiers.
- **Project Structure**: The source directory is structured into well-defined folders such as BiomeDefOf.cs, ElementSpawnDef.cs, etc., each focusing on specific components of the mod.

## XML Integration

- **Def Documentation**: XML files are employed to define game elements like biomes and diseases. These are primarily located under the `Defs` directory and follow strict formatting rules for game engine compatibility.
- **Custom Defs**: Uses XML files to create custom definitions (`BiomeDef`, `HediffDef`) which are employed to integrate new gameplay mechanics into existing RimWorld systems.

## Harmony Patching

- The mod requires the Harmony library to apply runtime modifications to RimWorld's base code, allowing dynamic updates and compatibility resolutions.
- Ensure patches are clearly documented, and use targeted patches to prevent unintended interactions with the vanilla game or other mods.

## Suggestions for Copilot

- **Code Completion for Biomes**: Guide Copilot to suggest biome-specific logic when editing `BiomeDef` files or related C# classes.
- **XML Validation**: Provide validation rules for XML files to ensure all defs conform to the RimWorld modding requirements.
- **Harmony Patch Templates**: Enable Copilot to offer templates and suggestions for common Harmony patching scenarios, such as prefix or postfix methods.
- **Performance Optimization Tips**: Suggest improvements to optimize C# code, especially in areas related to map generation and complex weather calculations.
- **In-Editor Documentation**: Encourage providing inline comments and summary documentation to improve code readability and maintainability.

By following these detailed guidelines, developers can efficiently collaborate and enhance the functionality of the Nature's Pretty Sweet mod while maintaining high standards of code quality and performance. Happy modding!

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.


# GitHub Copilot Instructions for RimWorld Modding Project: Nature's Pretty Sweet (Continued)

## Mod Overview and Purpose

Nature's Pretty Sweet (Continued) is a vibrant and immersive mod for RimWorld that enhances the environmental experience by introducing new terrain elements, weather systems, and biomes that make the world of RimWorld feel wild, untamed, and alive. The mod aims to challenge players with a dynamic environment that reacts to weather and seasonal changes, providing an engaging and oftentimes dangerous new layer to the game. This updated version of the original mod by tkkntkkn addresses several known bugs, introduces new features such as Rimbrella support, and makes improvements to existing features, maintaining the integrity of the original design while enhancing stability and compatibility.

## Key Features and Systems

- **Dynamic Weather Systems**: Introduces new weather types like dust storms, volcanic ash, and overcast conditions which alter the environment and create unique challenges.
- **Enhanced Terrain Reactions**: Soil and sand darken when wet, puddles form after rain, and rivers flood seasonally or follow significant rain events.
- **New Biome Generation**: Includes biomes such as Desert Salt Flats, Redwood Forest, and Volcanic Fields with unique ecosystem dynamics.
- **Impactful World Interactions**: Features like frost, snow on vegetation, and swimming for pawns increase realism and require adaptive strategies.
- **Resource Richness**: New resources, items, and environmental hazards that emerge with the changing biomes.
- **Compatibility Patches**: Works alongside other mods unless they affect map and world generation, with specific patches for popular mods like Vegetable Garden and AnimalCollabProj.

## Coding Patterns and Conventions

- **Class Organization**: Each significant feature is encapsulated within its own class for modularity, with classes such as `Building_SteamVent`, `DustDevil`, and `Lava_Spring` clearly outlining their responsibilities.
- **Inheritance and Abstraction**: Abstract base classes like `SpringCompAbstract` define common methods to be implemented by child classes, ensuring consistency across similar components.
- **Naming Conventions**: Consistent camelCase for method and variable names, with PascalCase for class names, aligning with C# coding standards.
- **Thread Safety**: Consider multithreading impacts in weather and terrain changes, especially when zoomed out or at high game speeds.

## XML Integration

- **Defining Elements**: Use XML to define game elements such as new weather types, biomes, and terrains, as seen in files like `ElementSpawnDef` and `TerrainDefOf`.
- **Data Serialization**: Utilize methods in classes such as `cellData` and `springData` that implement `IExposable` for saving and loading game state.
- **Custom Parameters**: Load data from XML within classes like `TKKN_IncidentCommonalityRecord` for flexible configuration of incident commonality records.

## Harmony Patching

- **Purpose**: Extend or modify the behavior of existing RimWorld methods without directly altering base code by using Harmony patches.
- **Patch Locations**: Key methods to patch are in `HarmonyMain`, such as those impacting map generation and world interactions.
- **Collision Avoidance**: Ensure harmony patches are compatible with common mods by carefully choosing patch locations and avoiding conflicts.

## Suggestions for Copilot

1. **Assist with Harmony Patches**: Use pattern recognition to recommend potential injection points for harmony patches, focusing on methods that need alteration or enhancement.
2. **Refactor for Optimization**: Propose refactorings for critical methods like `DoCellSteadyEffects` and `checkThingsforLava` to improve performance, particularly during complex weather events.
3. **Enhance XML Integrations**: Suggest efficient ways to handle XML loading and parsing to streamline new content additions, especially for expansive features like biomes.
4. **Debugging Aid**: Provide suggestions for debugging tools or techniques, particularly those that assist in tracking weather-related lag or map generation timing issues.
5. **Documentation Suggestions**: Offer inline documentation prompts where code logic may not be immediately obvious, particularly in abstract class implementations and complex mathematical operations related to environmental effects.

*Note: Some features may require new games due to biome generation requirements. Players are advised to test balance and report issues for fine-tuning.*

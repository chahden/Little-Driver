# Car Track City Lite

Thank you for downloading **Car Track City Lite**!

This package is a compact introduction to the Car Track City asset collection. It contains a curated selection of modular low-poly roads, pavements, buildings, vehicles, vegetation, and street props for creating stylized city streets, small racing circuits, driving prototypes, and background environments.

## Package Contents

The Lite package includes **32 ready-to-use prefabs**:

- **Road pieces** — straight road, large and small corners, crossing, shoulder, start marking, dashed marking, and solid marking.
- **Pavement pieces** — straight pavement, large and small corners, and pavement curb.
- **Modular buildings** — three bottom sections, three middle sections, and three top sections that can be combined into different buildings.
- **City and track structures** — grandstand, mini shop, and bus stop.
- **Vehicles** — a compact car in blue, cream, and sand color variants.
- **Street props** — bench, bin, street lighting, and racing traffic light.
- **Vegetation** — medium fir tree.
- **Demo scene** — a prepared example showing the included assets together.

## Folder Structure

- **CarTrackCitySceneLite.unity** — Demo scene showcasing the Lite collection.
- **Prefabs** — All 32 ready-to-use prefabs.
- **TrackCityAssetLite.fbx** — Source FBX containing the included meshes.
- **MainMaterail.mat** — Shared URP material used by the assets.
- **PaletCityDay.png** — 512 × 512 daytime color palette texture.

## Getting Started

### Explore the Demo Scene

Open `CarTrackCitySceneLite.unity` to preview the assets and see how the modular pieces can be arranged.

### Add Assets to Your Scene

1. Open the `Prefabs` folder.
2. Choose a road, building module, vehicle, prop, or vegetation prefab.
3. Drag the prefab into the Scene view or Hierarchy.
4. Use Unity's move, rotate, and snapping tools to position it.

The prepared prefabs are the recommended way to use the package because their meshes and material references are already configured.

## Building Roads and Tracks

1. Begin with the straight `Road` prefab.
2. Add `RoadCornerBig` or `RoadCornerSmall` to change direction.
3. Use `RoadCrossing` where routes meet.
4. Add `RoadDashed`, `RoadSolid`, and `RoadStart` as markings.
5. Place pavement and curb pieces beside the road.
6. Complete the scene with buildings, vehicles, lighting, traffic lights, props, and vegetation.

Enable grid snapping for faster placement and consistent joins. After assembling a route, inspect it from both the driving camera and the intended gameplay camera to check alignment.

## Creating Modular Buildings

The building set contains three sections for each level:

- `Bottom1`–`Bottom3`
- `Mid1`–`Mid3`
- `Top1`–`Top3`

Stack one bottom, any number of middle sections, and one top section. Mix the numbered variants to create different silhouettes while keeping the same low-poly visual style.

For easier editing, make the sections children of an empty GameObject representing the complete building.

## Customizing Colors

The assets share `PaletCityDay.png`, allowing many colors to be controlled through one small texture.

To create a custom palette:

1. Duplicate `MainMaterail.mat` and `PaletCityDay.png` so the original Lite appearance remains available.
2. Edit the duplicated palette in an image editor while preserving its 512 × 512 dimensions and color-block layout.
3. Return to Unity and allow the texture to reimport.
4. Assign the duplicated material to the assets you want to customize.

Because the meshes use shared palette regions, changing one area of the texture may affect several assets. Review your edited palette across the demo scene before applying it throughout a project.

## Using the Source FBX

`TrackCityAssetLite.fbx` contains the source meshes used by the prefabs. Use the prepared prefabs for normal scene construction. Use the FBX directly only when you need access to the imported mesh hierarchy or material-remapping options.

## Technical Notes

- Prepared for **Unity 6**.
- Uses the **Universal Render Pipeline (URP)** Lit shader.
- Includes **32 prefabs**, **1 material**, **1 palette texture**, **1 source FBX**, and **1 demo scene**.
- Assets are static environment and vehicle models; scripts, driving controllers, and animations are not included.
- Colliders are not added automatically. Add and configure colliders according to your gameplay requirements.
- Vehicles are visual assets and do not include wheel animation or vehicle physics.
- If your project uses another render pipeline, convert or replace the material as required.

## Full Version

Car Track City Lite contains a smaller selection intended for evaluation and compact projects. The full **Car Track City** package expands the collection with many more road and elevated-track pieces, buildings, vehicles, racing props, vegetation, trackside structures, and day/night visual options.

No AI was used to create the assets.

Enjoy building your city track!

# Sidi sidewalk kit

Drag these prefabs into your level from this folder:

- **SidewalkBlock_10x10**: a 10 × 10 metre paving block with a collider. Resize the root on X and Z; keep Y at 1 to preserve its height.
- **CurbStraight_1m**: a one-metre curb running along local Z. Stretch the root's Z scale for length and rotate Y to follow the road. Keep X and Y at 1.
- **CurbCorner_L_1m**: an L-shaped curb join. Rotate Y to fit each corner; use straight pieces to extend its sides.

Position the prefab roots at road-ground height. The block top is 0.20 m high and the curb top is 0.24 m high. Curbs are separate so you can leave openings for crossings and entrances.

## Materials

The shared materials are in **Materials**:

- **Sidi Stone Paving**: Stone Size controls stone dimensions in metres along world X/Z. Joint Width controls the gaps. Stone A/B and Joint Colour control the palette. Stones retain their size when you enlarge a block.
- **Sidi Red White Curb**: Stripe Length is the length of each red or white band in metres. Stripe Offset shifts the pattern. The material follows the longest horizontal axis of a scaled cube.

Changing a shared material updates every object using it. Duplicate the material first if you want a different style for one area. Keep curb geometry out of static batching so the bands retain their local direction; the shader disables geometry batching.

The shader is **Little Driver/Sidi Street Surface**. These patterns are procedural and use no external image textures. The supplied street photos were visual references.

## Navigation

The prefabs include colliders and mark their surfaces **Not Walkable** for the vehicle NavMesh. After moving or resizing pieces, select **Environment** and bake its NavMesh Surface again. Material-only changes need no bake.

Sprint2Prototype already uses the materials, including the enlarged sidewalk. Original source scenes were preserved.

# Reusable straight road

Open `RoadStraight_2L_10m.prefab` in Unity's Prefab Mode.

- Length: 10 metres, along local Z. Road width: 8 metres. Sidewalks: 2 metres on each side. Keep root scale at (1, 1, 1).
- `Visuals`: replace road, sidewalk and curb meshes, or assign materials under Mesh Renderer > Materials. These objects are excluded from navigation baking.
- `Physics and Navigation`: the road floor, sidewalk and curb BoxColliders. Keep these when replacing the visuals. Sidewalks and curbs are marked Not Walkable for NPC navigation.
- `Connection Points/Start` and `End`: the ends are at local Z -5 and +5. Place straight copies 10 metres apart along Z to join them.

For a different visual style, create a Prefab Variant and change its Visuals. This preserves the original road piece. A Tunisian style should come from the surrounding buildings, doors, walls, planting and street furniture as well as road materials.

Use one NavMeshSurface for the level, with Use Geometry set to Physics Colliders. Enable the road objects before baking. Rebuild navigation after changing the layout; material-only changes do not require a new bake.

Sprint2Prototype uses one instance in the first 10 metres of its approach road. The Environment group owns the level's NavMeshSurface, using Physics Colliders and collecting its children. It contains Road Surfaces, Sidewalks and Curbs, Road Markings, and Navigation and Floor Colliders. Continuous road floor colliders and four connected sidewalk corners complete the intersection. The baked data is saved in ConnectedRoadsNavigation.asset; the old Ground is no longer required. Rebuild navigation after moving or adding road pieces.

Other reusable pieces are RoadSurface, RoadFloorCollider, SidewalkSegment, CurbSegment, SidewalkCorner, and the prefabs in Markings (LaneDash, ForwardArrow, CrossingStripe, PedestrianCrossing and StopLine). Existing scene objects are linked instances with their original dimensions preserved as overrides. The Traffic group holds controllers, route points, spawn points and traffic lights. The lights reuse the existing shared traffic-light prefab; spawn points use Prefabs/Traffic/LaneSpawnPoint.

This prefab contains geometry and navigation configuration only. Traffic lights, IntersectionManager, spawn points, stop lines, exits, and the player controller remain level systems. The current player controller still supports a straight course along world Z; rotating a visual road piece does not make the player follow a curved route.

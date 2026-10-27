# Road signs

Reusable visual prefabs: StopSign, PedestrianCrossingSign, SpeedLimit50Sign.

Drag a prefab into Environment > Road Signs. Its origin is at the pole base; place it at sidewalk height. The readable face points along local +Z, so rotate Y to face approaching traffic (180 degrees for the player travelling along world +Z).

Boards are approximately 2.5 metres above their base. These signs have no colliders, Rigidbody, rule checks or enforcement scripts. They use original simple geometry and shared materials; numbers and STOP lettering use the project's existing TextMeshPro font. Source pack assets were inspected as references; the final boards and poles were authored for this project.

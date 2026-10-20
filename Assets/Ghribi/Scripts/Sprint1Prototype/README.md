# Little Driver - Sprint 1 Prototype

## Sprint goal

Build a playable gray-box prototype of US 1.1-1.5: acceleration and braking, two-lane movement, seat belt safety, horn usage, and collision detection. The priority is validating the mechanics before adding final visuals and educational content.

The player follows a fixed road with two forward lanes and premade obstacles. Rayen's main tasks are the seat belt (1.3) and horn (1.4).

Open `Assets/Ghribi/Scenes/Sprint1Prototype.unity` and press Play.

## Main scripts

| Script | What it does |
| --- | --- |
| `SprintPrototypeCar` | Controls the car's state and Rigidbody movement. Holding GO accelerates, releasing slows down, and STOP brakes. LEFT/RIGHT select a lane. It also handles the horn, collisions, finish line, and restart. |
| `SprintSeatBeltDrag` | Lets the player drag the belt handle to the buckle. Releasing near the target snaps it into place and unlocks driving; missing returns it to the start. |
| `SprintHoldControl` | Detects pressing and releasing GO, STOP, and HORN. It tells the car which action is being held and tracks the initiating finger. |
| `SprintPrototypeHUD` | Shows the seat belt panel before driving, then the driving controls. Updates speed, belt status, counters, and feedback messages. |

The car reads keyboard input in `Update`, applies physics movement in `FixedUpdate`, and follows the car with the camera in `LateUpdate`.

## How everything connects

```
Player input
    |
    +-- Drag belt --> SprintSeatBeltDrag --> car.Buckle()
    |
    +-- Hold buttons --> SprintHoldControl --> car input flags
    |
    +-- Lane buttons / keyboard --> car actions
                                      |
                              SprintPrototypeCar
                                |           |
                            Rigidbody   AudioSource
                                |
                       State read by the HUD
```

- **Seat belt:** the car stays stopped until `IsBuckled` becomes true. The HUD then hides the belt panel and shows driving controls.
- **Movement:** speed changes gradually, while lane selection moves the car smoothly between two fixed X positions. Braking takes priority over acceleration.
- **Horn:** HORN and H share a limit: the third press within a rolling 2-second window, or a continuous 2-second hold, stops the horn. The button is disabled for 3 seconds and until the warning voice finishes. Release before honking again. The warning says: «بالشويّة على الزمّارة! نستعملوها كان وقت الحاجة.»
- **Collisions:** `OnCollisionEnter` counts and logs the contact, plays a small mint puff and soft sound, then smoothly moves the car back 2 metres over 0.65 seconds. Driving resumes from rest; hold GO to continue and choose a clear lane. Contacts during recovery are ignored.
- **Restart:** the car resets its position and state, then raises a `Restarted` event so the HUD and belt interaction reset too.

The Canvas uses Unity's Input System UI module for pointer input and TextMeshPro for labels. Scene references and button actions are already wired.

## Controls and checking

- Drag the belt into the buckle before driving.
- Hold GO or W/Up Arrow to accelerate; release to coast.
- Hold STOP or Space/Down Arrow to brake.
- Use LEFT/RIGHT or A/D/arrow keys to select a lane.
- Hold HORN or H to sound the horn, respecting the 2-second hold / 3 presses in 2 seconds limit.
- Press RESTART or R to reset.

The original prototype passed Editor gameplay checks. The newer horn limits and collision recovery have not been play-tested yet. Physical Android testing is still pending. Temporary scene-building and checking tools were removed after completing the prototype.

## Assigning audio

Select **Player Car**, then find **Sprint Prototype Car** in the Inspector. Import your recordings into Assets and drag them into **Horn Sound**, **Horn Warning Voice** (Tunisian Arabic), and **Collision Sound**. Empty horn/collision slots use generated placeholders. The voice slot needs your recording; no spoken warning plays until it is assigned. Only generated clips are destroyed at runtime; imported audio assets are preserved.

## Prototype limits

The scene uses gray boxes, temporary labels, and placeholder sounds. Final art, the recorded Tunisian Arabic warning, other Arabic guidance, and cutscenes are later work. US 1.5 now includes collision feedback and a short reverse recovery; no scoring penalties are applied.

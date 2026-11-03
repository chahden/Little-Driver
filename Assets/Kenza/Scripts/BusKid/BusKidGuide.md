# Bus Kid NPC: how it decides

A child hides behind the parked bus. When a car comes, the kid runs out across
the road, so the young driver learns to slow down near stopped buses.

## The tree

```
Entry
 └ Repeat Forever
    └ Utility Selector "What Should The Kid Do?"
       ├ Dart Out From Bus
       ├ Greet Slow Car
       ├ Walk Back Behind Bus
       └ Stand Idle
```

Every frame, the Utility Selector asks each action `GetUtility()`, which is its
"vote" (a number). The highest vote runs. This is the "float score per action,
highest wins" idea from class.

Each action answers two questions:
1. **Is it allowed now?** If not, the vote is 0.
2. **If allowed, how strong is the vote?** The formula is
   `importance × (starting priority + bonus × urgency)`, where urgency is a
   number from 0 to 1.

| Action | Allowed when | Urgency (0 to 1) | Vote |
| --- | --- | --- | --- |
| Dart Out From Bus | kid is behind the bus, a car is driving toward the kid, closer than 25 m | how close the car is | 1.0 × (0.6 + 0.4 × u) |
| Greet Slow Car | kid is on the far side, car within 12 m and at most 10 km/h, not greeted yet this pass | how close the car is | 0.8 × (0.6 + 0.4 × u) |
| Walk Back Behind Bus | kid on the far side for 6 s or more, car farther than 30 m | extra waiting time | 0.6 × (0.5 + 0.5 × u) |
| Stand Idle | always | none | 0.001 |

## Worked example

The car drives toward the bus. It is 12.5 m from the kid, and the dart range is 25 m.
- Closeness = 1 − 12.5 / 25 = **0.5**
- Dart vote = 1.0 × (0.6 + 0.4 × 0.5) = 1.0 × 0.8 = **0.8**
- Greet and Walk Back are not allowed because the kid is not on the far side, so they vote **0**.
- Idle votes **0.001**.

0.8 is the highest, so the kid darts out.

## Why 1.1 while running?

A running action votes **1.1**. No normal vote can go above 1.0, so nothing
interrupts the kid halfway across the road. When the action finishes (Success),
the selector starts over and picks again.

## Watching it live

- Select the kid in Play mode. **Bus Kid NPC ▸ Live values** shows every vote,
  the running action, the car's distance and speed, and where the kid is.
- Open **Tools ▸ Behavior Designer ▸ Editor** with the kid selected to see the
  running node highlighted.
- Gizmos: the yellow line is the crossing path, the cyan sphere is the dart
  range, and the green sphere is the greet range.
- Right-click the component and choose **Reset Kid** to put the kid back behind
  the bus. Pressing R with Ghribi's car does the same.

## Setup in a scene

1. Put a Hodaart `Character NN` behind the bus, facing the road. Scale it to
   about 0.75 if you want it to look like a child.
2. Make sure the player car is tagged **Player**.
3. Select the character, then choose **Little Driver ▸ Bus Kid ▸ Make Selected
   Character a Bus Kid**.
4. Drag the markers under "<name> Path" (Hide Spot, Road Edge, Far Side) onto
   the ground. Put Road Edge at the corner of the bus and Far Side on the
   opposite sidewalk. You can add more markers to the Crossing Path list in
   between.
5. Save the scene.

To tune the kid, change the numbers on Bus Kid NPC outside Play mode. Changes
made in Play mode are lost when you stop.

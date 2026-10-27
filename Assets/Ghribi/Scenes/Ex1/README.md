# NPC utility exercise

Open Ex1.unity and allow Unity to reload the changed scene from disk.
The NPC tree now contains Entry > Repeater (forever) > Utility Selector,
with HealAction, RestAction, and AttackAction as its three children.
Each action has its own script in Scripts/ex1. UtilityState.cs is an older,
unused task and is no longer referenced by this scene's tree.

During Play mode, select GameManager and change Health, Stamina, and
Enemy Number in the Inspector. These update the existing global variables
Health, Stamina, and EnemyNumber. All task inputs use those globals.
Enable ordinary Log messages in the Console and clear its search filter.
The GameManager Inspector also shows read-only Live Utility Scores for Heal,
Rest, and Attack. These use the same functions as the action tasks and refresh
while playing; outside Play mode they preview the current Inspector inputs.
Each action prints once when entered, including whenever the selected
state changes. Actions remain Running so the selector can reconsider them.

Examples (Health, Stamina, Enemy Number):
- (10, 100, 5): Heal
- (100, 10, 5): Rest
- (100, 100, 5): Attack

Scores are Heal = 1-h, Rest = h*(1-s), Attack = h*s*Clamp01(enemies/5),
where h and s are Health/100 and Stamina/100 clamped to 0..1.
Ties prefer the earlier child: Heal, then Rest, then Attack.
Inspector changes during Play mode reset when Play mode ends.

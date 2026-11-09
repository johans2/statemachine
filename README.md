# statemachine

An implementation of a state machine along with some basic examples of how to use it.

The state machine is in `statemachine/Assets/Scripts/StateMachine.cs`.

## Running the examples

1. Open the `statemachine` folder in Unity 6000.3.7f1 or later.
2. Open one of the scenes in `Assets/Scenes` and press Play.

### EnemyExampleScene

An enemy that idles, patrols and chases a moving player. Its color shows the current state:

- Gray: Idle
- Cyan: Patrol
- Red: Chase
- Black: Dead

To kill it, select the enemy, open the component's menu (right-click its header in the Inspector) and choose **Take Damage** three times.

### AppFlowExampleScene

An application flow: Loading → Main Menu → Playing → Post Game → Main Menu. Each state creates its own object on enter and destroys it on exit, so watch the Hierarchy.

From the component's menu:

- **Play**: start a round from the Main Menu.
- **Quit To Menu**: go back to the Main Menu from any state.

# Project _Symphony Scavenger_

[Markdown Cheatsheet](https://github.com/adam-p/markdown-here/wiki/Markdown-Here-Cheatsheet)

_REPLACE OR REMOVE EVERYTING BETWEEN "\_"_

### Student Info

-   Name: _Nick Will_
-   Section: _01_

## Simulation Design

_A conductor NPC will chase down wandering instruments in the world in order to add them to his musical piece, which will be playing in the background using audio from only the retrieved instruments._

### Controls

-   _List all of the actions the player can have in your simulation_
    -   _Include how to preform each action ( keyboard, mouse, UI Input )_
    -   _Include what impact an action has in the simulation ( if is could be unclear )_

-   There will be a series of buttons, each corresponding to an instrument. 
    -   When the player clicks a button, the corresponding instrument will spawn in the world and begin to wander.

## _Agent 1 Conductor_

_The conductor will spawn at the start of the simulation, and will seek any instruments in the world that are not currently being used in the background audio. He will flee from instruments in the world that are already being used in the audio. If there are no instruments on the screen he will move to the origin and then stand still._

### _State 1 Moving_

**Objective:** _While in this state, the conductor will be actively seeking or fleeing from instruments (or both)._

#### Steering Behaviors

- _List all behaviors used by this state_
   - _If behavior has input data list it here_
   - _eg, Flee - nearest Agent2_
- Obstacles - _List all obstacle types this state avoids_
- Seperation - _List all agents this state seperates from_

- Seek - agentToSeek
- Flee - agentToFlee
   
#### State Transistions

- _List all the ways this agent can transition to this state_
   - _eg, When this agent gets within range of Agent2_
   - _eg, When this agent has reached target of State2_

- When any instrument(s) is spawned/exists in the world, the conductor will change to this state.
   
### _State 2 Still_

**Objective:** _The conductor will move to the origin and stand still once he gets there._

#### Steering Behaviors

- _List all behaviors used by this state_
- Obstacles - _List all obstacle types this state avoids_
- Seperation - _List all agents this state seperates from_

- Seek - pointToSeek
   
#### State Transistions

- _If a collision happens between the conductor and an instrument, and afterwards there are no instruments left in the world, he will enter this state._

## _Agent 2 Instrument_

_There will be different versions of this agent class to correspond with the type of instrument. All instruments will be spawned upon the player's interaction with the corresponding button, and will wander while separating from other instruments and staying in bounds_

### _State 1 Fleeing_

**Objective:** _The instrument will have a slight fleeing force pushing it away from the conductor, but the wander force will remain strong. It will be best described as "wandering away" from the conductor._

#### Steering Behaviors

- _List all behaviors used by this state_
- Obstacles - _List all obstacle types this state avoids_
- Seperation - _List all agents this state seperates from_

- Flee - agentToFlee
- Wander
- StayInBounds

- Separation: Separates from closest instrument.
   
#### State Transistions

- _An instrument will be in this state by default, or transition into it when it exists in the game and the background audio is not currently including the audio from the instrument._
   
### _State 2 Seeking_

**Objective:** _The instrument will have a slight seeking force pulling it towards the conductor, but the wander force will remain strong. It will be best desrcibed as "wandering towards" the conductor._

#### Steering Behaviors

- _List all behaviors used by this state_
- Obstacles - _List all obstacle types this state avoids_
- Seperation - _List all agents this state seperates from_

- Seek - agentToSeek
- Wander
- StayInBounds

- Separate: Separates from closest instrument
   
#### State Transistions

- _An instrument will transition into this state when it exists in the game and the background audio is already including th audio from the instrument._

## Sources

-   _List all project sources here –models, textures, sound clips, assets, etc._
-   _If an asset is from the Unity store, include a link to the page and the author’s name_

## Make it Your Own

- _List out what you added to your game to make it different for you_
- _If you will add more agents or states make sure to list here and add it to the documention above_
- _If you will add your own assets make sure to list it here and add it to the Sources section

- There will be a originally composed background music track that will dynamically change to variations that include/exclude certain instruments.
- There will be more than one type of instrument agent (in addition to the conductor agent).

## Known Issues

_List any errors, lack of error checking, or specific information that I need to know to run your program_

### Requirements not completed

_If you did not complete a project requirement, notate that here_


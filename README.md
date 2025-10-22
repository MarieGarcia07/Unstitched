# Unstitched

__Unstitched is a Game Project that is inspired by [Little Nightmares] (https://en.bandainamcoent.eu/little-nightmares/little-nightmares)__

* Engine Configuration

|Security Update|Engine|Version|
|--------|------|-------|
|Original|Unity|6000.1.2f1|
|Fixed|Unity|6000.1.17f1|

-------

## Description

This is a game project that is inspired by the Little Nightmare game series developed by Bandai Namco Entertainment. It will have a similar horror aesthetic and a companion AI. There will be no enemies, instead you will have to escape rooms that are blocked off.

## Features
The game will include the following:

* Movement + jumping & running
* 2.5D Camera that follows the player
* Ability to pick up/hold/throw objects
* Ability to push/pull objects
* Open/Close objects
* Puzzle rooms to escape
* Companion AI that helps with puzzles
* Ability to switch controls to play as the companion

## Major Components

The major components of this game includes the ability to interact with the environment and the companion AI. Since this is a horror-puzzle game, the environment around the player needs to be interactable in order to explore ways to escape the rooms. The player will not be able to escape the room without using the companion AI to help. These two go hand in hand to be able to successfully win the game. With the ability to switch controls between the main character and the companion, the player will have to keep in mind both environments and how each playable character can unlock methods that will lead to their escape. 

## Structure

The project is composed of the following major classes:

#### Player

The ```Player``` class has the following responsibilities:

* Basic movement controls.

* Pushing and Pulling functions.
  * Pushing allows the player to push objects with a rigidbody + collider.
  * Pulling allows the player to pull objects backwards along the z axis.
```csharp
if( isPulling)
{
    inputMove = new Vector3(inputMove.x, 0, 0); //only pulls on z axis
    currentSpeed = pullSpeed;
}
```

* Whistle function to call the companion's ```Follower``` script.

#### Follower

The ```Follower``` class has the following responsibilities:

* Will travel to the players position when called.

* The father away, the faster the companion follows. The closer, the slower the companion follows.
```csharp
if (distanceToPlayer > runDistance)
    agent.speed = 5f; // run
else
    agent.speed = 2f; // walk
```

* After the companion reaches the player's stopped position, they will stop following.

#### Camera Follow

The ```Camera Follow``` class has the following responsibilities:

* Camera follows the player along a single axis.


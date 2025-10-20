# Unstitched

__Game Project that is inspired by [Little Nightmares] (https://en.bandainamcoent.eu/little-nightmares/little-nightmares)__

* Engine Configuration

|Security Update|Engine|Version|
|--------|------|-------|
|Original|Unity|6000.1.2f1|
|Fixed|Unity|6000.1.17f1|

------- 

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


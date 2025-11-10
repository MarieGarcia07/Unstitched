using UnityEngine;

public abstract class ControllableCharacter : MonoBehaviour
{
    public bool isActiveCharacter;
    private CameraRoomTrigger currentRoom;
    public void SetCurrentRoom(CameraRoomTrigger room) => currentRoom = room;
    public CameraRoomTrigger GetCurrentRoom() => currentRoom;

    public virtual void SetActive(bool active)
    {
        isActiveCharacter = active;
    }

    public virtual void Move(Vector2 input) { }
    public virtual void Jump() { }
    public virtual void PullStart() { }
    public virtual void PullStop() { }
    public virtual void Interact() { }
    public virtual void Throw() { }
    public virtual void Whistle() { }
}





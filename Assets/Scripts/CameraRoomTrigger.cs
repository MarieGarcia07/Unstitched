using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class CameraRoomTrigger : MonoBehaviour
{
    [Header("Room Settings")]
    public Transform cameraFocusPoint;
    public float minX = -5f;
    public float maxX = 5f;

    private void OnTriggerEnter(Collider other)
    {
        ControllableCharacter character = other.GetComponent<ControllableCharacter>();
        if (character == null) return;
        character.SetCurrentRoom(this);

        RoomCameraController cam = Camera.main.GetComponentInParent<RoomCameraController>();
        if (character.isActiveCharacter)
        {
            cam.MoveToRoom(this);
            cam.SetFollowTarget(character.transform);
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        if (cameraFocusPoint != null)
            Gizmos.DrawWireSphere(cameraFocusPoint.position, 0.3f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(minX, transform.position.y, transform.position.z - 2),
                        new Vector3(minX, transform.position.y, transform.position.z + 2));
        Gizmos.DrawLine(new Vector3(maxX, transform.position.y, transform.position.z - 2),
                        new Vector3(maxX, transform.position.y, transform.position.z + 2));
    }
}









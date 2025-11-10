using UnityEngine;

public class Button : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator buttonAnimator;
    [SerializeField] private Door[] linkedDoors;                       // Normal doors
    [SerializeField] private DoorController[] linkedDoorsController;   // Doors that can be blocked

    private void OnTriggerEnter(Collider other)
    {
        // Check if a pickupable object collided
        if (other.GetComponent<Pickupable>() != null)
        {
            // Play button press animation
            if (buttonAnimator != null)
                buttonAnimator.SetTrigger("Press");

            // Open normal doors
            foreach (Door door in linkedDoors)
            {
                if (door != null)
                {
                    Debug.Log("Opening normal door");
                    door.OpenDoor();
                }
            }

            // Open doors that can be blocked
            foreach (DoorController doorController in linkedDoorsController)
            {
                if (doorController != null)
                {
                    Debug.Log("Opening controlled door");
                    doorController.OpenDoor();
                }
            }
        }
    }
}




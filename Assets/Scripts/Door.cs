using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour
{
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private float closeDelay = 2f;

    private bool isOpen = false;

    public void OpenDoor()
    {
        if (doorAnimator != null && !isOpen)
        {
            doorAnimator.SetTrigger("Open");
            isOpen = true;
            StartCoroutine(CloseDoorAfterDelay());
        }
    }

    private IEnumerator CloseDoorAfterDelay()
    {
        yield return new WaitForSeconds(closeDelay);

        if (doorAnimator != null && isOpen)
        {
            doorAnimator.SetTrigger("doorClose");
            isOpen = false;
        }
    }
}


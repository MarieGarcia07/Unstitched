using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class DoorController : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private Transform closedPosition;
    [SerializeField] private Transform openPosition;
    [SerializeField] private float moveSpeed = 2f;  
    [SerializeField] private float closeDelay = 1f;

    [Header("Blocking")]
    [SerializeField] private LayerMask blockerLayer;

    [Header("Animation")]
    [SerializeField] private Animator doorAnimator;

    private bool isOpen = false;
    private bool isMoving = false;

    public void OpenDoor()
    {
        if (isOpen || isMoving)
        {
            Debug.Log("DoorController: Door is already open or moving, cannot open.");
            return;
        }

        StopAllCoroutines();
        isOpen = true;

        Debug.Log("DoorController: Opening door.");

        if (doorAnimator != null)
            doorAnimator.SetTrigger("Open");

        StartCoroutine(MoveDoor(openPosition.position, true));
    }

    private IEnumerator MoveDoor(Vector3 targetPos, bool opening)
    {
        isMoving = true;
        Debug.Log($"DoorController: Moving door to {(opening ? "open" : "closed")} position.");

        while ((transform.position - targetPos).sqrMagnitude > 0.001f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;
        isMoving = false;
        Debug.Log($"DoorController: Door reached {(opening ? "open" : "closed")} position.");

        if (opening)
        {
            Debug.Log($"DoorController: Waiting {closeDelay} seconds before trying to close.");
            yield return new WaitForSeconds(closeDelay);
            StartCoroutine(TryClose());
        }
    }

    private IEnumerator TryClose()
    {
        Debug.Log("DoorController: Attempting to close door.");

        int frame = 0;
        while (IsBlocked())
        {
            if (frame % 30 == 0) 
                Debug.Log("DoorController: Door blocked, waiting...");
            frame++;
            yield return null;
        }

        Debug.Log("DoorController: No longer blocked, closing door.");

        if (doorAnimator != null)
            doorAnimator.SetTrigger("doorClose");

        StartCoroutine(MoveDoor(closedPosition.position, false));
        isOpen = false;
    }

    private bool IsBlocked()
    {
        Collider col = GetComponent<Collider>();
        Collider[] hits = Physics.OverlapBox(col.bounds.center, col.bounds.extents * 0.95f, transform.rotation, blockerLayer);

        foreach (var hit in hits)
        {
            if (hit.gameObject != gameObject) 
            {
                Debug.Log($"DoorController: Blocked by {hit.gameObject.name}");
                return true;
            }
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        Gizmos.color = Color.red;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(col.bounds.center - transform.position, col.bounds.size);
    }
}











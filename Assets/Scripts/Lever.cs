using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Lever : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator leverAnimator;
    [SerializeField] private Animator platformAnimator;

    [Header("Settings")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private float leverDelay = 0.5f;

    private bool isSequenceRunning = false;
    private enum PlatformState { Raised, Lowered }
    private PlatformState platformState = PlatformState.Raised;

    private PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls();
        controls.Player.Enable();
        controls.Player.Interact.performed += ctx => TryActivateLever();
    }

    private void TryActivateLever()
    {
        if (isSequenceRunning) return;

        ControllableCharacter activeCharacter = CharacterSwitchManager.Instance.GetActiveCharacter();

        if (activeCharacter == null) return;

        float distance = Vector3.Distance(activeCharacter.transform.position, transform.position);
        if (distance > interactionRange) return;

        if (platformState == PlatformState.Raised)
        {
            StartCoroutine(LowerPlatformSequence());
        }
        else if (platformState == PlatformState.Lowered)
        {
            StartCoroutine(RaisePlatformSequence());
        }
    }

    private IEnumerator LowerPlatformSequence()
    {
        isSequenceRunning = true;

        leverAnimator.SetTrigger("LeverUp");
        yield return new WaitForSeconds(leverDelay);

        platformAnimator.SetTrigger("LowerPlatform");
        yield return new WaitForSeconds(2f);

        platformState = PlatformState.Lowered;
        isSequenceRunning = false;
    }

    private IEnumerator RaisePlatformSequence()
    {
        isSequenceRunning = true;

        leverAnimator.SetTrigger("Idle");
        platformAnimator.SetTrigger("RaisePlatform");
        yield return new WaitForSeconds(3f);

        platformState = PlatformState.Raised;
        isSequenceRunning = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}



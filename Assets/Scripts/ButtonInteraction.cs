using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class ButtonInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator buttonAnimator;
    [SerializeField] private Animator doorAnimator;

    [Header("Settings")]
    [SerializeField] private float interactionRange = 2f;

    private bool isPressed = false;
    private PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls();
        controls.Player.Enable();
        controls.Player.Interact.performed += ctx => TryPressButton();
    }

    private void TryPressButton()
    {
        if (isPressed) return;

        ControllableCharacter activeChar = CharacterSwitchManager.Instance.GetActiveCharacter();
        if (activeChar == null) return;

        float distance = Vector3.Distance(activeChar.transform.position, transform.position);
        if (distance > interactionRange) return;

        StartCoroutine(ButtonSequence());
    }

    private IEnumerator ButtonSequence()
    {
        isPressed = true;

        if (buttonAnimator != null)
            buttonAnimator.SetTrigger("ButtonDown");

        yield return new WaitForSeconds(0.2f);

        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("OpenDoor");
        }


        yield return null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}


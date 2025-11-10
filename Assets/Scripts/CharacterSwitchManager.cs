using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterSwitchManager : MonoBehaviour
{
    [SerializeField] private ControllableCharacter mainCharacter;
    [SerializeField] private ControllableCharacter companion;

    public static CharacterSwitchManager Instance { get; private set; }

    private ControllableCharacter activeCharacter;
    private RoomCameraController cameraController;

    public ControllableCharacter GetActiveCharacter()
    {
        return activeCharacter;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        cameraController = Camera.main.GetComponentInParent<RoomCameraController>();
        SetActiveCharacter(mainCharacter, instant: true);
    }

    private void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            SetActiveCharacter(activeCharacter == mainCharacter ? companion : mainCharacter);
        }
    }

    private void SetActiveCharacter(ControllableCharacter character, bool instant = false)
    {
        if (activeCharacter != null)
            activeCharacter.SetActive(false);

        activeCharacter = character;
        activeCharacter.SetActive(true);

        if (cameraController != null)
        {
            // parallax sort of feel that follows the player subtly
            cameraController.SetFollowTarget(activeCharacter.transform);

            CameraRoomTrigger roomTrigger = activeCharacter.GetCurrentRoom();
            if (roomTrigger != null)
            {
                if (instant)
                    cameraController.SetInstant(roomTrigger);
                else
                    cameraController.MoveToRoom(roomTrigger);
            }

        }
    }

}







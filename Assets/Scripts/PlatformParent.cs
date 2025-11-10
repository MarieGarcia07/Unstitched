using System.Collections.Generic;
using UnityEngine;

public class PlatformParent : MonoBehaviour
{
    [SerializeField] private Collider platformTop;
    private List<ControllableCharacter> standingCharacters = new List<ControllableCharacter>();
    private Vector3 lastPlatformPos;

    private void Start()
    {
        lastPlatformPos = transform.position;
    }

    private void LateUpdate()
    {
        Vector3 delta = transform.position - lastPlatformPos;

        foreach (var character in standingCharacters)
        {
            character.transform.position += delta;
        }

        lastPlatformPos = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        ControllableCharacter character = other.GetComponent<ControllableCharacter>();
        if (character != null && !standingCharacters.Contains(character))
        {
            standingCharacters.Add(character);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        ControllableCharacter character = other.GetComponent<ControllableCharacter>();
        if (character != null)
        {
            standingCharacters.Remove(character);
        }
    }
}

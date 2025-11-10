using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

public class GameIntroManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ControllableCharacter playerCharacter;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private ControllableCharacter companionCharacter;
    [SerializeField] private Animator companionAnimator;
    [SerializeField] private TextMeshProUGUI introText;
    [SerializeField] private TextMeshProUGUI introText2;
    [SerializeField] private Image fadeImage;

    [Header("Camera Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform introCameraPosition;
    [SerializeField] private Transform firstCameraPoint;
    [SerializeField] private float cameraMoveTime = 2f;
    [SerializeField] private MonoBehaviour roomCameraController; // your existing camera controller script

    [Header("Settings")]
    [SerializeField] private string startTrigger = "Start";
    [SerializeField] private float textFadeOutTime = 1f;
    [SerializeField] private float fadeInTime = 1.5f;
    [SerializeField] private float animationWaitTime = 2f;

    private bool introDone = false;

    private void Start()
    {
        Time.timeScale = 0f;

        // Disable gameplay characters and room camera controller
        playerCharacter.SetActive(false);
        companionCharacter.SetActive(false);
        if (roomCameraController) roomCameraController.enabled = false;

        // Enable UI
        introText.gameObject.SetActive(true);
        introText2.gameObject.SetActive(true);
        fadeImage.gameObject.SetActive(true);

        // Fade image starts fully black
        fadeImage.color = new Color(0f, 0f, 0f, 1f);

        // Set camera to intro position
        if (mainCamera && introCameraPosition)
        {
            mainCamera.transform.position = introCameraPosition.position;
            mainCamera.transform.rotation = introCameraPosition.rotation;
        }

        StartCoroutine(FadeFromBlack());
    }

    private IEnumerator FadeFromBlack()
    {
        float elapsed = 0f;
        while (elapsed < fadeInTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeInTime);
            fadeImage.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }
        fadeImage.color = new Color(0f, 0f, 0f, 0f);

        StartCoroutine(WaitForAnyInput());
    }

    private IEnumerator WaitForAnyInput()
    {
        yield return new WaitUntil(() => Keyboard.current.anyKey.wasPressedThisFrame);
        StartCoroutine(StartGameSequence());
    }

    private IEnumerator StartGameSequence()
    {
        if (introDone) yield break;
        introDone = true;

        // Fade out text
        float elapsed = 0f;
        Color startColor = introText.color;
        Color startColor2 = introText2.color;

        while (elapsed < textFadeOutTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / textFadeOutTime);
            introText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            introText2.color = new Color(startColor2.r, startColor2.g, startColor2.b, alpha);
            yield return null;
        }

        introText.gameObject.SetActive(false);
        introText2.gameObject.SetActive(false);

        // Make sure animations run even while Time.timeScale = 0
        playerAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        companionAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;

        // Trigger get-up animations immediately
        playerAnimator.SetTrigger(startTrigger);
        companionAnimator.SetTrigger(startTrigger);

        // Start camera move at the same time
        if (mainCamera && firstCameraPoint)
            StartCoroutine(MoveCameraToPoint());

        // Wait for animation duration only (camera moves independently)
        yield return new WaitForSecondsRealtime(animationWaitTime);

        // Enable gameplay and camera controller
        Time.timeScale = 1f;
        playerCharacter.SetActive(true);
        companionCharacter.SetActive(false);
        if (roomCameraController) roomCameraController.enabled = true;
    }


    private IEnumerator MoveCameraToPoint()
    {
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;
        Vector3 targetPos = firstCameraPoint.position;
        Quaternion targetRot = firstCameraPoint.rotation;

        float elapsed = 0f;
        while (elapsed < cameraMoveTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / cameraMoveTime);
            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        mainCamera.transform.position = targetPos;
        mainCamera.transform.rotation = targetRot;
    }
}







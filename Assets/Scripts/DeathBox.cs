using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem;

public class DeathBox : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image fadeScreen;
    [SerializeField] private TextMeshProUGUI loseText;

    [Header("Timing Settings")]
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private float delayBeforeFade = 1f;

    private bool hasLost = false;

    private void Start()
    {
        if (fadeScreen != null)
        {
            Color c = fadeScreen.color;
            c.a = 0f;
            fadeScreen.color = c;
            fadeScreen.gameObject.SetActive(true);
        }

        if (loseText != null)
        {
            Color t = loseText.color;
            t.a = 0f;
            loseText.color = t;
            loseText.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        if (hasLost && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasLost)
        {
            hasLost = true;
            StartCoroutine(LoseSequence());
        }
    }

    private IEnumerator LoseSequence()
    {
        yield return new WaitForSeconds(delayBeforeFade);

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Clamp01(timer / fadeDuration);
            if (fadeScreen != null)
            {
                Color c = fadeScreen.color;
                c.a = alpha;
                fadeScreen.color = c;
            }
            yield return null;
        }

        yield return new WaitForSeconds(1f);

        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Clamp01(timer / fadeDuration);
            if (loseText != null)
            {
                Color t = loseText.color;
                t.a = alpha;
                loseText.color = t;
            }
            yield return null;
        }
    }
}

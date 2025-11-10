using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WinBox : MonoBehaviour
{
    [SerializeField] private Image fadeScreen;
    private bool playerInside = false;
    private bool companionInside = false;

    private void Start()
    {
        if (fadeScreen != null)
        {
            Color c = fadeScreen.color;
            c.a = 0f;
            fadeScreen.color = c;
            fadeScreen.gameObject.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInside = true;
        else if (other.CompareTag("Companion"))
            companionInside = true;

        if (playerInside && companionInside)
            StartCoroutine(FadeAndQuit());
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInside = false;
        else if (other.CompareTag("Companion"))
            companionInside = false;
    }

    private IEnumerator FadeAndQuit()
    {
        yield return new WaitForSeconds(Random.Range(2f, 3f));

        float fadeDuration = 2f;
        float timer = 0f;

        Color c = fadeScreen.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            c.a = Mathf.Clamp01(timer / fadeDuration);
            fadeScreen.color = c;
            yield return null;
        }

        yield return new WaitForSeconds(Random.Range(2f, 3f));

        
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}


using UnityEngine;
using System.Collections;

public class StartManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gameplayPanel;

    [Header("Cookie Clicker")]
    [SerializeField] private Clicker cookieClicker;

    [Header("Fade Settings")]
    [SerializeField] private CanvasGroup mainMenuCanvasGroup;
    [SerializeField] private float fadeDuration = 1f;

    private bool hasStarted = false;

    public void StartGame()
    {
        if (hasStarted) return;
        hasStarted = true;

        StartCoroutine(FadeOutMainMenu());
    }

    //Animasi fading title dan startbutton
    private IEnumerator FadeOutMainMenu()
    {
        float timer = 0f;
        float startAlpha = mainMenuCanvasGroup.alpha;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 0, timer / fadeDuration);
            mainMenuCanvasGroup.alpha = alpha;
            yield return null;
        }

        mainMenuCanvasGroup.alpha = 0;

        // 👉 Enable gameplay first, then disable main menu
        gameplayPanel.SetActive(true);
        mainMenuPanel.SetActive(false);

        if (cookieClicker != null)
        {
            cookieClicker.SetClickable(true);
        }
    }
}

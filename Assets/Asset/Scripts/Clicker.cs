using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class CookieClicker : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private bool isClickable = false;

    // Visual feedback
    private Image cookieImage;
    private Color originalColor;
    private Vector3 originalScale;
    public Color clickColor = new Color(0.7f, 0.7f, 0.7f); // Slightly darker
    public float clickScale = 0.9f;
    public float effectDuration = 0.1f;
    private bool isClicked = false;

    void Start()
    {
        cookieImage = GetComponent<Image>();
        originalColor = cookieImage.color;
        originalScale = transform.localScale;
    }

    public void SetClickable(bool value)
    {
        isClickable = value;
        GetComponent<Image>().raycastTarget = value;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isClickable) return;

        GameManager.Instance.AddCookie(1); // Click to add cookie

        if (!isClicked)
            StartCoroutine(ClickEffect()); // Start visual feedback
    }

    private IEnumerator ClickEffect()
    {
        isClicked = true;

        // Apply effect
        cookieImage.color = clickColor;
        transform.localScale = originalScale * clickScale;

        yield return new WaitForSeconds(effectDuration);

        // Restore
        cookieImage.color = originalColor;
        transform.localScale = originalScale;

        isClicked = false;
    }
}

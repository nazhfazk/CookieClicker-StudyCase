using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class Clicker : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private bool isClickable = false;

    // Visual feedback settings
    private Image cookieImage;
    private Color originalColor;
    private Vector3 originalScale;

    [Header("Click Effect Settings")]
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

    // Set whether cookie can be clicked
    public void SetClickable(bool value)
    {
        isClickable = value;
        GetComponent<Image>().raycastTarget = value;
    }

    // Handle click event
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isClickable) return;

        // Call GameManager to handle cookie adding with proper click power
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCookieClicked();
        }

        // Start visual feedback
        if (!isClicked)
        {
            StartCoroutine(ClickEffect());
        }
    }

    // Visual click effect coroutine
    private IEnumerator ClickEffect()
    {
        isClicked = true;

        // Apply click effect
        cookieImage.color = clickColor;
        transform.localScale = originalScale * clickScale;

        // Wait for effect duration
        yield return new WaitForSeconds(effectDuration);

        // Restore original appearance
        cookieImage.color = originalColor;
        transform.localScale = originalScale;

        isClicked = false;
    }

    // Method untuk change cookie sprite (dipanggil dari ShopManager)
    public void ChangeCookieSprite(Sprite newSprite)
    {
        if (cookieImage != null && newSprite != null)
        {
            cookieImage.sprite = newSprite;
        }
    }
}
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class Clicker : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private bool isClickable = false;

   
    private Image cookieImage;
    private Color originalColor;
    private Vector3 originalScale;

    [Header("Click Effect Settings")]
    public Color clickColor = new Color(0.7f, 0.7f, 0.7f); 
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

        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCookieClicked();
        }

        
        if (!isClicked)
        {
            StartCoroutine(ClickEffect());
        }
    }

    
    private IEnumerator ClickEffect()
    {
        isClicked = true;

        
        cookieImage.color = clickColor;
        transform.localScale = originalScale * clickScale;

       
        yield return new WaitForSeconds(effectDuration);

        
        cookieImage.color = originalColor;
        transform.localScale = originalScale;

        isClicked = false;
    }

    //untuk change cookie sprite 
    public void ChangeCookieSprite(Sprite newSprite)
    {
        if (cookieImage != null && newSprite != null)
        {
            cookieImage.sprite = newSprite;
        }
    }
}
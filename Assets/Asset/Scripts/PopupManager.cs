using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PopupManager : MonoBehaviour
{
    [Header("Overlay Background")]
    [SerializeField] private GameObject overlay; // Background gelap semi-transparan

    [Header("Shop Popup")]
    [SerializeField] private GameObject shopPopup; // Panel shop
    [SerializeField] private Button shopButton; // Tombol buka shop
    [SerializeField] private Button shopCloseButton; // Tombol tutup shop (X)

    [Header("Quest Popup")]
    [SerializeField] private GameObject questPopup; // Panel quest
    [SerializeField] private Button questButton; // Tombol buka quest
    [SerializeField] private QuestPanelManager questPanelManager; //PanelManager
    [SerializeField] private Button questCloseButton; // Tombol tutup quest (X)

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.3f; // Durasi animasi
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Kurva animasi

    private bool isAnimating = false;

    void Start()
    {
        SetupButtonListeners();
        InitializePopups();
    }

    private void SetupButtonListeners()
    {
        if (shopButton != null)
            shopButton.onClick.AddListener(OpenShopPopup);

        if (questButton != null)
            questButton.onClick.AddListener(OpenQuestPopup);

        if (shopCloseButton != null)
            shopCloseButton.onClick.AddListener(CloseAllPopups);

        if (questCloseButton != null)
            questCloseButton.onClick.AddListener(CloseAllPopups);

        if (overlay != null)
        {
            Button overlayButton = overlay.GetComponent<Button>();
            if (overlayButton != null)
                overlayButton.onClick.AddListener(CloseAllPopups);
        }
    }

    private void InitializePopups()
    {
        if (overlay != null)
            overlay.SetActive(false);

        if (shopPopup != null)
        {
            shopPopup.SetActive(false);
            shopPopup.transform.localScale = Vector3.zero;
        }

        if (questPopup != null)
        {
            questPopup.SetActive(false);
            questPopup.transform.localScale = Vector3.zero;
        }
    }

    public void OpenShopPopup()
    {
        if (isAnimating) return;

        if (questPopup != null && questPopup.activeInHierarchy)
        {
            CloseAllPopups();
            return;
        }

        StartCoroutine(ShowPopup(shopPopup));
    }

    public void OpenQuestPopup()
    {
        if (isAnimating) return;

        if (shopPopup != null && shopPopup.activeInHierarchy)
        {
            CloseAllPopups();
            return;
        }

        StartCoroutine(ShowQuestPopup()); // Khusus quest pakai coroutine khusus
    }

    public void CloseAllPopups()
    {
        if (isAnimating) return;

        GameObject activePopup = null;

        if (shopPopup != null && shopPopup.activeInHierarchy)
            activePopup = shopPopup;
        else if (questPopup != null && questPopup.activeInHierarchy)
            activePopup = questPopup;

        if (activePopup != null)
        {
            StartCoroutine(HidePopup(activePopup));
        }
    }

    private IEnumerator ShowPopup(GameObject popup)
    {
        if (popup == null) yield break;

        isAnimating = true;

        if (overlay != null)
            overlay.SetActive(true);

        popup.SetActive(true);
        popup.transform.localScale = Vector3.zero;

        float timer = 0f;
        while (timer < animationDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / animationDuration;
            float scaleValue = scaleCurve.Evaluate(progress);

            popup.transform.localScale = Vector3.one * scaleValue;

            yield return null;
        }

        popup.transform.localScale = Vector3.one;
        isAnimating = false;
    }

    private IEnumerator ShowQuestPopup()
    {
        if (questPopup == null) yield break;

        isAnimating = true;

        if (overlay != null)
            overlay.SetActive(true);

        questPopup.SetActive(true);
        questPopup.transform.localScale = Vector3.zero;

        // Init quest panel manually before showing
        if (questPanelManager != null)
        {
            questPanelManager.InitPanel();
        }

        float timer = 0f;
        while (timer < animationDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / animationDuration;
            float scaleValue = scaleCurve.Evaluate(progress);

            questPopup.transform.localScale = Vector3.one * scaleValue;

            yield return null;
        }

        questPopup.transform.localScale = Vector3.one;
        isAnimating = false;
    }

    private IEnumerator HidePopup(GameObject popup)
    {
        if (popup == null) yield break;

        isAnimating = true;

        float timer = 0f;
        popup.transform.localScale = Vector3.one;

        while (timer < animationDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / animationDuration;
            float scaleValue = scaleCurve.Evaluate(1 - progress);

            popup.transform.localScale = Vector3.one * scaleValue;

            yield return null;
        }

        popup.transform.localScale = Vector3.zero;
        popup.SetActive(false);

        if (overlay != null)
            overlay.SetActive(false);

        isAnimating = false;
    }

    public bool IsAnyPopupOpen()
    {
        bool shopOpen = shopPopup != null && shopPopup.activeInHierarchy;
        bool questOpen = questPopup != null && questPopup.activeInHierarchy;

        return shopOpen || questOpen;
    }

    public bool IsAnimating()
    {
        return isAnimating;
    }
}

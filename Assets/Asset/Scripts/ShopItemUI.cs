using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Image itemIcon;

    private Button buyButton;
    private ShopItem itemData;
    private int itemIndex;
    private ShopManager shopManager;

    public void Initialize(ShopItem item, int index, ShopManager manager)
    {
        itemData = item;
        itemIndex = index;
        shopManager = manager;

        
        buyButton = GetComponent<Button>();
        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyClicked);
        }
        else
        {
            Debug.LogError("[ShopItemUI] Tidak ditemukan komponen Button di GameObject ini.");
        }

        UpdateDisplay();
    }

    private void OnBuyClicked()
    {
        if (shopManager == null || itemData == null) return;

        bool isOwned = itemData.IsCookieSkin() && shopManager.GetOwnedCookieSkins().Contains(itemIndex);

        if (isOwned)
        {
            // Kalau skin sudah punya, ganti langsung ke skin ini
            if (itemData.cookieSprite != null && GameManager.Instance != null)
            {
                var clicker = GameObject.FindObjectOfType<Clicker>();
                if (clicker != null)
                {
                    clicker.ChangeCookieSprite(itemData.cookieSprite);
                    Debug.Log($"[ShopItemUI] Skin changed to: {itemData.itemName}");
                }
            }
        }
        else
        {
            // Kalau belum punya, lanjut ke pembelian
            shopManager.BuyItem(itemIndex);
        }
    }


    public void UpdateDisplay()
    {
        if (itemData == null) return;

        
        if (itemNameText != null) itemNameText.text = itemData.itemName;
        if (descriptionText != null) descriptionText.text = itemData.description;
        if (itemIcon != null && itemData.itemIcon != null) itemIcon.sprite = itemData.itemIcon;

        
        if (itemData.IsUpgrade() && levelText != null)
        {
            levelText.gameObject.SetActive(true);
            levelText.text = $"Lv {itemData.currentLevel}";
        }
        else if (levelText != null)
        {
            levelText.gameObject.SetActive(false);
        }

        
        bool isOwned = itemData.IsCookieSkin() && shopManager.GetOwnedCookieSkins().Contains(itemIndex);
        bool canAfford = GameManager.Instance != null &&
                         GameManager.Instance.GetCookieCount() >= itemData.GetCurrentPrice();

        
        if (priceText != null)
        {
            if (isOwned)
            {
                priceText.text = "OWNED";
                priceText.color = Color.gray;
            }
            else
            {
                priceText.text = itemData.GetCurrentPrice().ToString();
                priceText.color = canAfford ? Color.white : Color.gray;
            }
        }

        
        if (buyButton != null)
        {
            buyButton.interactable = true;

            
            var img = buyButton.GetComponent<Image>();
            if (img != null)
            {
                if (!canAfford || isOwned)
                    img.color = new Color(0.5f, 0.5f, 0.5f); 
                else
                    img.color = Color.white;
            }
        }
    }
}

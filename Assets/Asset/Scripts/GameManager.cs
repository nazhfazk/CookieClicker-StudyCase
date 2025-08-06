using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Cookie Counter")]
    [SerializeField] private TextMeshProUGUI cookieCounterText;

    private int cookieCount = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); 
    }

    private void Start()
    {
        UpdateCookieCounterUI(); // Start counter 
    }

    public void AddCookie(int amount)
    {
        cookieCount += amount;
        UpdateCookieCounterUI(); //Tambah jumlah ke counter
    }

    private void UpdateCookieCounterUI()
    {
        cookieCounterText.text = $"Cookies: {cookieCount}";
    }
}

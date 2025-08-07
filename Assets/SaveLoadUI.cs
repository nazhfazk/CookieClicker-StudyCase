using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SaveLoadUI : MonoBehaviour
{
    [Header("Status Indicators")]
    [SerializeField] private GameObject connectionStatusPanel;
    [SerializeField] private TextMeshProUGUI connectionStatusText;
    [SerializeField] private Image connectionStatusIcon;

    [Header("Save/Load Indicators")]
    [SerializeField] private GameObject saveIndicator;
    [SerializeField] private TextMeshProUGUI saveStatusText;
    [SerializeField] private GameObject loadIndicator;
    [SerializeField] private TextMeshProUGUI loadStatusText;

    [Header("Manual Controls (Optional)")]
    [SerializeField] private Button manualSaveButton;
    [SerializeField] private Button manualLoadButton;

    [Header("Status Colors")]
    [SerializeField] private Color connectedColor = Color.green;
    [SerializeField] private Color disconnectedColor = Color.red;
    [SerializeField] private Color loadingColor = Color.yellow;

    [Header("Auto Hide Settings")]
    [SerializeField] private float indicatorDisplayTime = 3f;
    [SerializeField] private bool autoHideIndicators = true;

    private Coroutine saveIndicatorCoroutine;
    private Coroutine loadIndicatorCoroutine;

    private void Start()
    {
       
        PlayFabManager.OnLoginSuccess += OnLoginSuccess;
        PlayFabManager.OnLoginFailed += OnLoginFailed;
        PlayFabManager.OnDataSaved += OnDataSaved;
        PlayFabManager.OnDataSaveFailed += OnDataSaveFailed;
        PlayFabManager.OnDataLoaded += OnDataLoaded;
        PlayFabManager.OnDataLoadFailed += OnDataLoadFailed;

      
        SetupManualButtons();

        
        InitializeUI();
    }

    private void OnDestroy()
    {
     
        PlayFabManager.OnLoginSuccess -= OnLoginSuccess;
        PlayFabManager.OnLoginFailed -= OnLoginFailed;
        PlayFabManager.OnDataSaved -= OnDataSaved;
        PlayFabManager.OnDataSaveFailed -= OnDataSaveFailed;
        PlayFabManager.OnDataLoaded -= OnDataLoaded;
        PlayFabManager.OnDataLoadFailed -= OnDataLoadFailed;
    }

    private void SetupManualButtons()
    {
        if (manualSaveButton != null)
        {
            manualSaveButton.onClick.AddListener(() => {
                if (PlayFabManager.Instance != null)
                {
                    PlayFabManager.Instance.ManualSave();
                }
            });
        }

        if (manualLoadButton != null)
        {
            manualLoadButton.onClick.AddListener(() => {
                if (PlayFabManager.Instance != null)
                {
                    PlayFabManager.Instance.ManualLoad();
                }
            });
        }
    }

    private void InitializeUI()
    {
      
        if (saveIndicator != null) saveIndicator.SetActive(false);
        if (loadIndicator != null) loadIndicator.SetActive(false);

      
        UpdateConnectionStatus(false, "Connecting to PlayFab...");
    }

    #region Connection Status

    private void UpdateConnectionStatus(bool isConnected, string statusMessage)
    {
        if (connectionStatusPanel != null)
        {
            connectionStatusPanel.SetActive(true);
        }

        if (connectionStatusText != null)
        {
            connectionStatusText.text = statusMessage;
        }

        if (connectionStatusIcon != null)
        {
            connectionStatusIcon.color = isConnected ? connectedColor : disconnectedColor;
        }

   
        if (manualSaveButton != null)
        {
            manualSaveButton.interactable = isConnected;
        }

        if (manualLoadButton != null)
        {
            manualLoadButton.interactable = isConnected;
        }
    }

    private void OnLoginSuccess()
    {
        UpdateConnectionStatus(true, "Connected to PlayFab");

       
        if (autoHideIndicators)
        {
            StartCoroutine(HideConnectionStatusAfterDelay(3f));
        }
    }

    private void OnLoginFailed()
    {
        UpdateConnectionStatus(false, "PlayFab Connection Failed - Retrying...");
    }

    private IEnumerator HideConnectionStatusAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (connectionStatusPanel != null)
        {
            connectionStatusPanel.SetActive(false);
        }
    }

    #endregion

    #region Save Status

    private void OnDataSaved()
    {
        ShowSaveIndicator("Game Saved Successfully!", connectedColor);
    }

    private void OnDataSaveFailed()
    {
        ShowSaveIndicator("Save Failed! Check connection.", disconnectedColor);
    }

    private void ShowSaveIndicator(string message, Color color)
    {
        if (saveIndicator == null || saveStatusText == null) return;

       
        if (saveIndicatorCoroutine != null)
        {
            StopCoroutine(saveIndicatorCoroutine);
        }

        
        saveIndicator.SetActive(true);
        saveStatusText.text = message;
        saveStatusText.color = color;

        
        if (autoHideIndicators)
        {
            saveIndicatorCoroutine = StartCoroutine(HideSaveIndicatorAfterDelay());
        }
    }

    private IEnumerator HideSaveIndicatorAfterDelay()
    {
        yield return new WaitForSeconds(indicatorDisplayTime);

        if (saveIndicator != null)
        {
            saveIndicator.SetActive(false);
        }
    }

    #endregion

    #region Load Status

    private void OnDataLoaded(SaveData data)
    {
        string message = data != null ?
            $"Game Loaded! ({data.GetSaveSummary()})" :
            "New Game Started";

        ShowLoadIndicator(message, connectedColor);
    }

    private void OnDataLoadFailed()
    {
        ShowLoadIndicator("Load Failed! Starting new game.", disconnectedColor);
    }

    private void ShowLoadIndicator(string message, Color color)
    {
        if (loadIndicator == null || loadStatusText == null) return;

        
        if (loadIndicatorCoroutine != null)
        {
            StopCoroutine(loadIndicatorCoroutine);
        }

      
        loadIndicator.SetActive(true);
        loadStatusText.text = message;
        loadStatusText.color = color;

        
        if (autoHideIndicators)
        {
            loadIndicatorCoroutine = StartCoroutine(HideLoadIndicatorAfterDelay());
        }
    }

    private IEnumerator HideLoadIndicatorAfterDelay()
    {
        yield return new WaitForSeconds(indicatorDisplayTime);

        if (loadIndicator != null)
        {
            loadIndicator.SetActive(false);
        }
    }

    #endregion

    #region Manual Controls

 
    [ContextMenu("Show Save Status")]
    public void ShowSaveStatus()
    {
        if (PlayFabManager.Instance != null)
        {
            bool isLoggedIn = PlayFabManager.Instance.IsLoggedIn();
            bool isSaving = PlayFabManager.Instance.IsSaving();
            bool isLoading = PlayFabManager.Instance.IsLoading();

            string status = $"PlayFab Status: {(isLoggedIn ? "Connected" : "Disconnected")}\n" +
                          $"Saving: {isSaving}\n" +
                          $"Loading: {isLoading}";

            Debug.Log($"[SaveLoadUI] {status}");

            
            UpdateConnectionStatus(isLoggedIn, status);
        }
    }

  
    [ContextMenu("Test Save Indicator")]
    public void TestSaveIndicator()
    {
        ShowSaveIndicator("Test Save Message", connectedColor);
    }

    
    [ContextMenu("Test Load Indicator")]
    public void TestLoadIndicator()
    {
        ShowLoadIndicator("Test Load Message", connectedColor);
    }

    #endregion

    #region Update Methods

    private void Update()
    {
        
        if (PlayFabManager.Instance != null)
        {
            if (PlayFabManager.Instance.IsSaving() && saveIndicator != null && !saveIndicator.activeInHierarchy)
            {
                ShowSaveIndicator("Saving...", loadingColor);
            }

            if (PlayFabManager.Instance.IsLoading() && loadIndicator != null && !loadIndicator.activeInHierarchy)
            {
                ShowLoadIndicator("Loading...", loadingColor);
            }
        }
    }

    #endregion
}
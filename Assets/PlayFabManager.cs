using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using System;

public class PlayFabManager : MonoBehaviour
{
    public static PlayFabManager Instance;

    [Header("PlayFab Settings")]
    [SerializeField] private string titleId = "14B98A"; // Title ID PlayFab

    [Header("Save Settings")]
    [SerializeField] private bool autoSaveEnabled = true;
    [SerializeField] private float autoSaveInterval = 180f; // Auto save setiap 30 detik
    [SerializeField] private bool saveOnApplicationPause = true;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    
    public static event Action OnLoginSuccess;
    public static event Action OnLoginFailed;
    public static event Action<SaveData> OnDataLoaded;
    public static event Action OnDataSaved;
    public static event Action OnDataSaveFailed;
    public static event Action OnDataLoadFailed;

    
    private bool isLoggedIn = false;
    private bool isSaving = false;
    private bool isLoading = false;
    private string playerPlayFabId = "";
    private Coroutine autoSaveCoroutine;

    
    private const string SAVE_DATA_KEY = "GameSaveData";
    private const string BACKUP_SAVE_KEY = "GameSaveDataBackup";

    #region Unity Lifecycle

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Setup PlayFab
        if (!string.IsNullOrEmpty(titleId))
        {
            PlayFabSettings.staticSettings.TitleId = titleId;
        }
        else
        {
            Debug.LogError("[PlayFabManager] Title ID is not set!");
        }
    }

    private void Start()
    {
        // Auto login when game starts
        StartCoroutine(DelayedLogin());
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && saveOnApplicationPause && isLoggedIn)
        {
            SaveGameData();
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && saveOnApplicationPause && isLoggedIn)
        {
            SaveGameData();
        }
    }

    private void OnDestroy()
    {
        if (autoSaveCoroutine != null)
        {
            StopCoroutine(autoSaveCoroutine);
        }
    }

    #endregion

    #region Login System

    private IEnumerator DelayedLogin()
    {
        // Wait a bit for other systems to initialize
        yield return new WaitForSeconds(1f);
        AttemptLogin();
    }

    public void AttemptLogin()
    {
        if (isLoggedIn)
        {
            DebugLog("Already logged in to PlayFab");
            return;
        }

        DebugLog("Attempting PlayFab login...");

        var request = new LoginWithCustomIDRequest
        {
            CustomId = GetDeviceId(),
            CreateAccount = true,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true,
                GetUserData = true
            }
        };

        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccessCallback, OnLoginFailureCallback);
    }

    private string GetDeviceId()
    {
        // Use device unique identifier, fallback to random if not available
        string deviceId = SystemInfo.deviceUniqueIdentifier;

        if (string.IsNullOrEmpty(deviceId) || deviceId == SystemInfo.unsupportedIdentifier)
        {
            // Fallback untuk web builds atau unsupported platforms
            deviceId = PlayerPrefs.GetString("CustomPlayFabId", "");
            if (string.IsNullOrEmpty(deviceId))
            {
                deviceId = "WebUser_" + UnityEngine.Random.Range(100000, 999999).ToString();
                PlayerPrefs.SetString("CustomPlayFabId", deviceId);
                PlayerPrefs.Save();
            }
        }

        DebugLog($"Using Device ID: {deviceId}");
        return deviceId;
    }

    private void OnLoginSuccessCallback(LoginResult result)
    {
        isLoggedIn = true;
        playerPlayFabId = result.PlayFabId;

        DebugLog($"PlayFab Login SUCCESS! Player ID: {playerPlayFabId}");

        // Start auto-save system
        if (autoSaveEnabled)
        {
            StartAutoSave();
        }

        // Notify other systems
        OnLoginSuccess?.Invoke();

        // Auto-load game data after login
        LoadGameData();
    }

    private void OnLoginFailureCallback(PlayFabError error)
    {
        isLoggedIn = false;

        Debug.LogError($"[PlayFabManager] Login FAILED: {error.GenerateErrorReport()}");

        // Notify other systems
        OnLoginFailed?.Invoke();

        // Retry login setelah delay
        StartCoroutine(RetryLogin());
    }

    private IEnumerator RetryLogin()
    {
        yield return new WaitForSeconds(5f);
        DebugLog("Retrying PlayFab login...");
        AttemptLogin();
    }

    #endregion

    #region Save System

    public void SaveGameData()
    {
        if (!isLoggedIn)
        {
            Debug.LogWarning("[PlayFabManager] Cannot save - not logged in to PlayFab");
            return;
        }

        if (isSaving)
        {
            DebugLog("Save already in progress, skipping...");
            return;
        }

        StartCoroutine(SaveGameDataCoroutine());
    }

    private IEnumerator SaveGameDataCoroutine()
    {
        isSaving = true;
        DebugLog("Starting save process...");

        // Create save data dari current game state
        SaveData saveData = SaveData.CreateFromGameState();

        // Validate data
        if (!saveData.IsValid())
        {
            Debug.LogError("[PlayFabManager] Save data validation failed!");
            isSaving = false;
            OnDataSaveFailed?.Invoke();
            yield break;
        }

        // Convert to JSON
        string jsonData = saveData.ToJson();
        if (string.IsNullOrEmpty(jsonData))
        {
            Debug.LogError("[PlayFabManager] Failed to convert save data to JSON");
            isSaving = false;
            OnDataSaveFailed?.Invoke();
            yield break;
        }

        // Prepare data untuk PlayFab
        var dataToSave = new Dictionary<string, string>
        {
            { SAVE_DATA_KEY, jsonData },
            { BACKUP_SAVE_KEY, jsonData }, // Backup copy
            { "LastSaveTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            { "SaveVersion", saveData.saveVersion.ToString() }
        };

        var request = new UpdateUserDataRequest
        {
            Data = dataToSave,
            Permission = UserDataPermission.Private
        };

        bool saveComplete = false;
        bool saveSuccess = false;

        PlayFabClientAPI.UpdateUserData(request,
            (result) => {
                DebugLog("Game data saved successfully!");
                saveSuccess = true;
                saveComplete = true;
                OnDataSaved?.Invoke();
            },
            (error) => {
                Debug.LogError($"[PlayFabManager] Save failed: {error.GenerateErrorReport()}");
                saveSuccess = false;
                saveComplete = true;
                OnDataSaveFailed?.Invoke();
            });

        // Wait for save to complete
        yield return new WaitUntil(() => saveComplete);

        isSaving = false;

        if (saveSuccess)
        {
            DebugLog($"Save completed successfully. Data summary: {saveData.GetSaveSummary()}");
        }
    }

    #endregion

    #region Load System

    public void LoadGameData()
    {
        if (!isLoggedIn)
        {
            Debug.LogWarning("[PlayFabManager] Cannot load - not logged in to PlayFab");
            return;
        }

        if (isLoading)
        {
            DebugLog("Load already in progress, skipping...");
            return;
        }

        StartCoroutine(LoadGameDataCoroutine());
    }

    private IEnumerator LoadGameDataCoroutine()
    {
        isLoading = true;
        DebugLog("Starting load process...");

        var request = new GetUserDataRequest
        {
            Keys = new List<string> { SAVE_DATA_KEY, BACKUP_SAVE_KEY, "LastSaveTime", "SaveVersion" }
        };

        bool loadComplete = false;
        SaveData loadedData = null;

        PlayFabClientAPI.GetUserData(request,
            (result) => {
                DebugLog("Data retrieved from PlayFab successfully");
                loadedData = ProcessLoadedData(result);
                loadComplete = true;
            },
            (error) => {
                Debug.LogError($"[PlayFabManager] Load failed: {error.GenerateErrorReport()}");
                loadComplete = true;
                OnDataLoadFailed?.Invoke();
            });

        // Wait for load to complete
        yield return new WaitUntil(() => loadComplete);

        isLoading = false;

        if (loadedData != null)
        {
            DebugLog($"Load completed successfully. Data summary: {loadedData.GetSaveSummary()}");
            ApplyLoadedData(loadedData);
            OnDataLoaded?.Invoke(loadedData);
        }
        else
        {
            DebugLog("No save data found or failed to load, starting with new game");
        }
    }

    private SaveData ProcessLoadedData(GetUserDataResult result)
    {
        if (result.Data == null || !result.Data.ContainsKey(SAVE_DATA_KEY))
        {
            DebugLog("No save data found");
            return null;
        }

        string jsonData = result.Data[SAVE_DATA_KEY].Value;

        if (string.IsNullOrEmpty(jsonData))
        {
            DebugLog("Save data is empty");
            return null;
        }

        SaveData loadedData = SaveData.FromJson(jsonData);

        if (loadedData == null || !loadedData.IsValid())
        {
            Debug.LogWarning("[PlayFabManager] Primary save data is invalid, trying backup...");

            // Try backup
            if (result.Data.ContainsKey(BACKUP_SAVE_KEY))
            {
                string backupJson = result.Data[BACKUP_SAVE_KEY].Value;
                loadedData = SaveData.FromJson(backupJson);

                if (loadedData != null && loadedData.IsValid())
                {
                    DebugLog("Successfully loaded from backup save");
                }
                else
                {
                    Debug.LogError("[PlayFabManager] Both primary and backup saves are invalid");
                    return null;
                }
            }
            else
            {
                Debug.LogError("[PlayFabManager] No backup save available");
                return null;
            }
        }

        return loadedData;
    }

    private void ApplyLoadedData(SaveData data)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadGameData(data);
        }

        // Apply shop data
        ShopManager shopManager = FindObjectOfType<ShopManager>();
        if (shopManager != null)
        {
            shopManager.LoadGameData(data);
        }
    }

    #endregion

    #region Auto Save System

    private void StartAutoSave()
    {
        if (autoSaveCoroutine != null)
        {
            StopCoroutine(autoSaveCoroutine);
        }

        autoSaveCoroutine = StartCoroutine(AutoSaveLoop());
        DebugLog($"Auto-save started with interval: {autoSaveInterval} seconds");
    }

    private IEnumerator AutoSaveLoop()
    {
        while (isLoggedIn && autoSaveEnabled)
        {
            yield return new WaitForSeconds(autoSaveInterval);

            if (isLoggedIn && !isSaving)
            {
                DebugLog("Auto-save triggered");
                SaveGameData();
            }
        }
    }

    #endregion

    #region Manual Controls

    [ContextMenu("Manual Save")]
    public void ManualSave()
    {
        SaveGameData();
    }

    [ContextMenu("Manual Load")]
    public void ManualLoad()
    {
        LoadGameData();
    }

    [ContextMenu("Force Logout")]
    public void ForceLogout()
    {
        isLoggedIn = false;
        playerPlayFabId = "";

        if (autoSaveCoroutine != null)
        {
            StopCoroutine(autoSaveCoroutine);
            autoSaveCoroutine = null;
        }

        DebugLog("Forced logout from PlayFab");
    }

    #endregion

    #region Utility Methods

    private void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[PlayFabManager] {message}");
        }
    }

    public bool IsLoggedIn()
    {
        return isLoggedIn;
    }

    public string GetPlayerPlayFabId()
    {
        return playerPlayFabId;
    }

    public bool IsSaving()
    {
        return isSaving;
    }

    public bool IsLoading()
    {
        return isLoading;
    }

    #endregion
}
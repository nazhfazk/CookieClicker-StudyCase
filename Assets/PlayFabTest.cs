using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class PlayFabTest : MonoBehaviour
{
    [Header("PlayFab Settings")]
    public string titleId = "14B98A"; 

    void Start()
    {
        
        PlayFabSettings.staticSettings.TitleId = titleId;

        // Test login
        TestLogin();
    }

    void TestLogin()
    {
        Debug.Log("Attempting PlayFab Login...");

        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true 
        };

        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("PlayFab Login SUCCESS!");
        Debug.Log("PlayFab ID: " + result.PlayFabId);
        Debug.Log("Session Ticket: " + result.SessionTicket);

        
        TestSaveData();
    }

    void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("PlayFab Login FAILED!");
        Debug.LogError("Error: " + error.GenerateErrorReport());
    }

    void TestSaveData()
    {
        Debug.Log("Testing Save Data...");

        var request = new UpdateUserDataRequest
        {
            Data = new System.Collections.Generic.Dictionary<string, string>
            {
                {"TestData", "Hello PlayFab!"},
                {"Timestamp", System.DateTime.Now.ToString()}
            }
        };

        PlayFabClientAPI.UpdateUserData(request, OnDataSaveSuccess, OnDataSaveFailure);
    }

    void OnDataSaveSuccess(UpdateUserDataResult result)
    {
        Debug.Log("Data Save SUCCESS!");

        
        TestLoadData();
    }

    void OnDataSaveFailure(PlayFabError error)
    {
        Debug.LogError("Data Save FAILED!");
        Debug.LogError("Error: " + error.GenerateErrorReport());
    }

    void TestLoadData()
    {
        Debug.Log("Testing Load Data...");

        var request = new GetUserDataRequest();
        PlayFabClientAPI.GetUserData(request, OnDataLoadSuccess, OnDataLoadFailure);
    }

    void OnDataLoadSuccess(GetUserDataResult result)
    {
        Debug.Log("Data Load SUCCESS!");

        if (result.Data != null)
        {
            foreach (var item in result.Data)
            {
                Debug.Log($"Key: {item.Key}, Value: {item.Value.Value}");
            }
        }

        Debug.Log("ALL PLAYFAB TESTS PASSED!");
    }

    void OnDataLoadFailure(PlayFabError error)
    {
        Debug.LogError("Data Load FAILED!");
        Debug.LogError("Error: " + error.GenerateErrorReport());
    }
}
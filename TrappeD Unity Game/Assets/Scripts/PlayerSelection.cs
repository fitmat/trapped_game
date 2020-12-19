﻿using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Threading.Tasks;

#if UNITY_STANDALONE_WIN
using yipli.Windows;
#endif

public class PlayerSelection : MonoBehaviour
{
    public GameObject phoneHolderInfo;
    public GameObject LaunchFromYipliAppPanel;
    public TextMeshProUGUI[] TextsToBeChanged;
    public GameObject playerSelectionPanel;
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI onlyOnePlayerText;
    public GameObject PlayersContainer;
    public GameObject switchPlayerPanel;
    public GameObject zeroPlayersPanel;
    public GameObject GuestUserPanel;
    public TextMeshProUGUI zeroPlayersText;
    public YipliConfig currentYipliConfig;
    public GameObject PlayerButtonPrefab;
    public GameObject onlyOnePlayerPanel;
    public GameObject LoadingPanel;
    public GameObject noNetworkPanel;
    public TextMeshProUGUI noNetworkPanelText;
    public MatSelection matSelectionScript;
    public Image profilePicImage;
    public GameObject RemotePlayCodePanel;
    public GameObject Minimum2PlayersPanel;
    public GameObject GameVersionUpdatePanel;

    public TextMeshProUGUI GameVersionUpdateText;
    public TextMeshProUGUI RemotePlayCodeErrorText;
    
    private string PlayerName;

    private List<GameObject> generatedObjects = new List<GameObject>();
    private bool bIsCheckingForIntents;
    private bool bIsProfilePicLoaded = false;
    private Sprite defaultProfilePicSprite;

    private bool isSkipUpdateCalled = false;

    //Delegates for Firebase Listeners
    public delegate void OnUserFound();
    public static event OnUserFound NewUserFound;

    public delegate void OnPlayerChanged();
    public static event OnPlayerChanged DefaultPlayerChanged;

    public delegate void OnGameLaunch();
    public static event OnUserFound GetGameInfo;

    public void OnEnable()
    {
        defaultProfilePicSprite = profilePicImage.sprite;
    }

    // When the game starts
    public void Start()
    {
        // Game info and update check before player selection
        GetGameInfo();

        FetchUserAndInitializePlayerEnvironment();
    }

    public void OnUpdateGameClick()
    {
        string gameAppId = Application.identifier;
        Debug.Log("App Id is : " + gameAppId);
        YipliHelper.GoToPlaystoreUpdate(gameAppId);
    }

    public void OnSkipUpdateClick()
    {
        isSkipUpdateCalled = true;
        FetchUserAndInitializePlayerEnvironment();
    }

    //Whenever the Yipli App launches the game, the user will be found and next flow will be called automatically.
    //No need to keep retrying.
    //Start this coroutine to check for intents till a valid user is not found.
    IEnumerator KeepFindingUserId()
    {
        yield return new WaitForSeconds(.5f);
        Debug.Log("Started Coroutine : KeepCheckingForIntents");
        while (currentYipliConfig.userId == null || currentYipliConfig.userId.Length < 1)
        {
            Debug.Log("Calling CheckIntentsAndInitializePlayerEnvironment()");

            // Read intents and Initialize defaults
            FetchUserDetails();

            yield return new WaitForSeconds(0.2f);
        }
        Debug.Log("Ending Coroutine. UserId = " + currentYipliConfig.userId);
        StartCoroutine(InitializeAndStartPlayerSelection());
    }

    private void FetchUserAndInitializePlayerEnvironment()
    {
        Debug.Log("In CheckIntentsAndInitializePlayerEnvironment()");

        // Read intents and Initialize defaults
        FetchUserDetails();

        StartCoroutine(InitializeAndStartPlayerSelection());
    }

    public void playPhoneHolderTutorial()
    {
        TurnOffAllPanelsExceptLoading();
        Debug.Log("Starting PhoneHolder Tutorial for " + currentYipliConfig.playerInfo.playerName);
        Debug.Log("Is profilePicLoaded = " + bIsProfilePicLoaded);

        StartCoroutine(ImageUploadAndPlayerUIInit());

        phoneHolderInfo.SetActive(true);
        StartCoroutine(ChangeTextMessage());
    }

    IEnumerator ChangeTextMessage()
    {
        while (true)//Infinite loop
        {
            foreach (var str in TextsToBeChanged)
            {
                str.gameObject.SetActive(true);
                yield return new WaitForSecondsRealtime(5f);
                str.gameObject.SetActive(false);
            }
        }
    }

    private void NoUserFoundInGameFlow()
    {
        //Go to Yipli Panel
        TurnOffAllPanels();

        playerNameText.gameObject.SetActive(false);

        //Depending upon if Main Yipli App is installed or not, take a decision to show No user panel / or Guest User Panel
        if (false == YipliHelper.IsYipliAppInstalled())
        {
            FindObjectOfType<YipliAudioManager>().Play("BLE_failure");
            GuestUserPanel.SetActive(true);
        }
        else
        {
            Debug.Log("Calling RedirectToYipliAppForNoUserFound()");
            //Automatically redirect to Yipli App
            StartCoroutine(RedirectToYipliAppForNoUserFound());     
        }
    }

    IEnumerator RedirectToYipliAppForNoUserFound()
    {
        Debug.Log("In RedirectToYipliAppForNoUserFound()");

        LaunchFromYipliAppPanel.SetActive(true);
        yield return new WaitForSecondsRealtime(1f);
        FindObjectOfType<YipliAudioManager>().Play("BLE_failure");
        yield return new WaitForSecondsRealtime(1f);

        LaunchFromYipliAppPanel.SetActive(false);
        LoadingPanel.SetActive(true);

        Debug.Log("Calling KeepFindingUserId()");
        //Currently User isnt found. Start a coroutine which keeps on checking for the user details.
        StartCoroutine(KeepFindingUserId());

        Debug.Log("Redirecting to Yipli App");
        YipliHelper.GoToYipli();
    }

    public async void OnEnterPressedAfterCodeInput()
    {
        string codeEntered = RemotePlayCodePanel.GetComponentInChildren<InputField>().text;
        if (codeEntered.Length != 6)
        {
            FindObjectOfType<YipliAudioManager>().Play("BLE_failure");
            Debug.Log("Please enter valid 6 digit code. Code : " + codeEntered);
            RemotePlayCodeErrorText.gameObject.SetActive(true);
            RemotePlayCodeErrorText.text = "Enter valid 6 digit code";
        }
        else
        {
            RemotePlayCodeErrorText.text = "";
            RemotePlayCodePanel.SetActive(false);
            LoadingPanel.SetActive(true);
            // Write logic to check the code with the backend and retrive the UserId
            string UserId = await FirebaseDBHandler.GetUserIdFromCode(codeEntered);
            LoadingPanel.SetActive(false);
            Debug.Log("Got UserId : " + UserId);
            currentYipliConfig.userId = UserId;
            currentYipliConfig.playerInfo = new YipliPlayerInfo();
            currentYipliConfig.matInfo = new YipliMatInfo();
            PlayerSelectionFlow();
        }
    }

    //Here default player object is filled with the intents passed.
    //If no intents found, it is filled with the device persisted default player object.
    private void InitDefaultPlayer()
    {
        if (currentYipliConfig.playerInfo != null)
        {
            //If PlayerInfo is found in the Intents as an argument.
            //This code block will be called when the game App is launched from the Yipli app.
            UserDataPersistence.SavePlayerToDevice(currentYipliConfig.playerInfo);
        }
        else
        {
            //If there is no PlayerInfo found in the Intents as an argument.
            //This code block will be called when the game App is not launched from the Yipli app.
            currentYipliConfig.playerInfo = UserDataPersistence.GetSavedPlayer();
        }

        if (currentYipliConfig.playerInfo != null)
        {
            //Notify the listeners to start gathering the players gamedata
            DefaultPlayerChanged();
        }
    }

    //Here default player object is filled with the intents passed.
    //If no intents found, it is filled with the device persisted default player object.
    private void InitDefaultMat()
    {
        //Not storing Default player in scriptable object as it would be done later.
        if (currentYipliConfig.matInfo != null)
        {
            //If PlayerInfo is found in the Intents as an argument.
            //This code block will be called when the game App is launched from the Yipli app.
            UserDataPersistence.SaveMatToDevice(currentYipliConfig.matInfo);
        }
        else
        {
            //If there is no PlayerInfo found in the Intents as an argument.
            //This code block will be called when the game App is not launched from the Yipli app.
            currentYipliConfig.matInfo = UserDataPersistence.GetSavedMat();
        }
    }

    private void InitUserId()
    {
        if (currentYipliConfig.userId != null && currentYipliConfig.userId.Length > 1)
        {
            //If UserId is found in the Intents as an argument.
            //This code block will be called when the game App is launched from the Yipli app.
            UserDataPersistence.SavePropertyValue("user-id", currentYipliConfig.userId);
            //PlayerPrefs.Save();
        }
        else
        {
            //If there is no UserId found in the Intents as an argument.
            //This code block will be called when the game App is not launched from the Yipli app.
            currentYipliConfig.userId = UserDataPersistence.GetPropertyValue("user-id");
        }

        if (currentYipliConfig.userId != null && currentYipliConfig.userId.Length > 1)
        {
            //Trigger the database listeners as sson as the user is found
            NewUserFound();
        }
    }

    private void ReadAndroidIntents()
    {
        Debug.Log("Reading intents.");
        AndroidJavaClass UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject intent = currentActivity.Call<AndroidJavaObject>("getIntent");
        AndroidJavaObject extras = intent.Call<AndroidJavaObject>("getExtras");

        currentYipliConfig.userId = extras.Call<string>("getString", "uId");
        if (currentYipliConfig.userId == null)
        {
            Debug.Log("Returning from readIntents as no userId found.");
            throw new Exception("UserId is not found");
        }

        string pId = extras.Call<string>("getString", "pId");
        string pName = extras.Call<string>("getString", "pName");
        string pDOB = extras.Call<string>("getString", "pDOB");
        string pHt = extras.Call<string>("getString", "pHt");
        string pWt = extras.Call<string>("getString", "pWt");
        string pic = extras.Call<string>("getString", "pPicUrl");
        string mId = extras.Call<string>("getString", "mId");
        string mMac = extras.Call<string>("getString", "mMac");

        Debug.Log("Found intents : " + currentYipliConfig.userId + ", " + pId + ", " + pDOB + ", " + pHt + ", " + pWt + ", " + pName + ", " + mId + ", " + mMac + "," + pic);

        if (pId != null && pName != null)
        {
            currentYipliConfig.playerInfo = new YipliPlayerInfo(pId, pName, pDOB, pHt, pWt, pic);
        }

        if (mId != null && mMac != null)
        {
            currentYipliConfig.matInfo = new YipliMatInfo(mId, mMac);
        }
    }

    private void TurnOffAllPanels()
    {
        phoneHolderInfo.SetActive(false);
        switchPlayerPanel.SetActive(false);
        playerSelectionPanel.SetActive(false);
        onlyOnePlayerPanel.SetActive(false);
        Minimum2PlayersPanel.SetActive(false);
        zeroPlayersPanel.SetActive(false);
        noNetworkPanel.SetActive(false);
        GuestUserPanel.SetActive(false);
        LaunchFromYipliAppPanel.SetActive(false);
        LoadingPanel.SetActive(false);
        GameVersionUpdatePanel.SetActive(false);
    }

    private void TurnOffAllPanelsExceptLoading()
    {
        phoneHolderInfo.SetActive(false);
        switchPlayerPanel.SetActive(false);
        playerSelectionPanel.SetActive(false);
        onlyOnePlayerPanel.SetActive(false);
        Minimum2PlayersPanel.SetActive(false);
        zeroPlayersPanel.SetActive(false);
        noNetworkPanel.SetActive(false);
        GuestUserPanel.SetActive(false);
        LaunchFromYipliAppPanel.SetActive(false);
        GameVersionUpdatePanel.SetActive(false);
    }

    private void FetchUserDetails()
    {
        try
        {
            Debug.Log("In player Selection Start()");
#if UNITY_ANDROID
            ReadAndroidIntents();
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR
            ReadFromWindowsFile();
#endif
        }
        catch (System.Exception exp)// handling of game directing opening, without yipli app
        {
            Debug.Log("Exception occured in GetIntent!!!");
            Debug.Log(exp.Message);

            currentYipliConfig.userId = null;
            currentYipliConfig.playerInfo = null;
            currentYipliConfig.matInfo = null;

            //Fill dummy data in user/player, for testing from Editor
#if UNITY_EDITOR
            //currentYipliConfig.userId = "F9zyHSRJUCb0Ctc15F9xkLFSH5f1";
            //currentYipliConfig.playerInfo = new YipliPlayerInfo("-M2iG0P2_UNsE2VRcU5P", "rooo", "03-01-1999", "120", "49", "-MH0mCgEUMVBHxqwSQXj.jpg");
            //currentYipliConfig.matInfo = new YipliMatInfo("-M3HgyBMOl9OssN8T6sq", "54:6C:0E:20:A0:3B");
#endif
        }

#if UNITY_STANDALONE_WIN
        currentYipliConfig.matInfo = null;
#endif
    }

    private IEnumerator InitializeAndStartPlayerSelection()
    {
        //Setting User Id in the scriptable Object
        InitUserId();

#if UNITY_ANDROID
        //Setting Deafult mat
        InitDefaultMat();

#endif
        //Setting default Player in the scriptable Object
        InitDefaultPlayer();

        if (currentYipliConfig.userId == null || currentYipliConfig.userId == "")
        {
            Debug.Log("Calling NoUserFoundInGameFlow()");
            //This code block will be called when the game App is not launched from the Yipli app even once.
            NoUserFoundInGameFlow();
        }
        else
        {
            // game info status check
            while (firebaseDBListenersAndHandlers.GetGameInfoQueryStatus() != QueryStatus.Completed)
            {
                yield return new WaitForSecondsRealtime(0.1f);
            }

            // if change player is called, do not check for game update as user has already skipped it.
            // isSkipUpdateCalled is here to make sure update panel only come once on game start.
            if (!currentYipliConfig.bIsChangePlayerCalled && !isSkipUpdateCalled)
            {
                //If GameVersion latest then proceed
                if (currentYipliConfig.gameInventoryInfo == null)
                {
                    Debug.Log("Game not found in the iventory");
                }
                else
                {
                    Debug.Log("Game found in the iventory");
                    Debug.Log("Currrent Game version : " + Application.version);
                    Debug.Log("Latest Game version : " + currentYipliConfig.gameInventoryInfo.gameVersion);

                    int gameVersionCode = YipliHelper.convertGameVersionToBundleVersionCode(Application.version);
                    int inventoryVersionCode = YipliHelper.convertGameVersionToBundleVersionCode(currentYipliConfig.gameInventoryInfo.gameVersion);


                    if (inventoryVersionCode > gameVersionCode)
                    {
                        //Ask user to Update Game version option
                        LoadingPanel.SetActive(false);

                        GameVersionUpdateText.text = "A new version of " + currentYipliConfig.gameInventoryInfo.displayName + " is available.\nUpdate recommended";
                        GameVersionUpdatePanel.SetActive(true);
                    }
                    else
                    {
                        OnSkipUpdateClick();
                    }
                }
            }
            else
            {
                //Wait till the listeners are synced and the data has been populated
                Debug.Log("Waiting for players query to complete");
                LoadingPanel.gameObject.GetComponentInChildren<Text>().text = "Getting all players...";
                while (firebaseDBListenersAndHandlers.GetPlayersQueryStatus() != QueryStatus.Completed)
                {
                    yield return new WaitForSecondsRealtime(0.1f);
                }

                //Special handling in case of Multiplayer games
                if (currentYipliConfig.gameType == GameType.MULTIPLAYER_GAMING)
                {
                    // Check if atleast 2 players are available for playing the multiplayer game
                    if (currentYipliConfig.allPlayersInfo.Count < 2)
                    {
                        //Set active a panel to handle atleast 2 players should be there to play
                        TurnOffAllPanels();
                        Minimum2PlayersPanel.SetActive(true);
                    }
                    else
                    {
                        LoadingPanel.SetActive(false);
                        //Skip player selection as it will be handled by game side, directly go to the mat selection flow
                        matSelectionScript.MatConnectionFlow();
                    }
                }

                else if (currentYipliConfig.bIsChangePlayerCalled == true)
                {
                    currentYipliConfig.bIsChangePlayerCalled = false;

                    if (currentYipliConfig.allPlayersInfo.Count > 1)
                    {
                        PlayerSelectionFlow();
                    }
                    else // If No then throw a new panel to tell the Gamer that there is only 1 player currently
                    {
                        TurnOffAllPanels();
                        onlyOnePlayerPanel.SetActive(true);
                    }
                }
                else
                {
                    //Uncomment following line to always start the flow from phoneHolder panel
                    if (!currentYipliConfig.bIsMatIntroDone && currentYipliConfig.playerInfo != null)
                        playPhoneHolderTutorial();
                    else
                        SwitchPlayerFlow();
                }
            }
        }
    }

    public void retryUserCheck()
    {
        GuestUserPanel.SetActive(false);
        FetchUserAndInitializePlayerEnvironment();
    }

    public void retryPlayersCheck()
    {
        Minimum2PlayersPanel.SetActive(false);
        StartCoroutine(InitializeAndStartPlayerSelection());
    }

    public void SwitchPlayerFlow()//Call this for every StartGame()/Game Session
    {
        Debug.Log("Checking current player.");

        TurnOffAllPanels();
        LoadingPanel.SetActive(true);

        if (currentYipliConfig.playerInfo != null)
        {
            //This means we have the default Player info from backend.
            //In this case we need to call the player change screen and not the player selection screen
            Debug.Log("Found current player : " + currentYipliConfig.playerInfo.playerName);

            StartCoroutine(ImageUploadAndPlayerUIInit());

            //Since default player is there, directly go to the mat selection flow
            matSelectionScript.MatConnectionFlow();
        }
        else //Current player not found in Db.
        {
            //Force to switch player as no default player found.
            OnSwitchPlayerPress(true);
        }
    }

    private IEnumerator ImageUploadAndPlayerUIInit()
    {
        //Activate the PlayerName and Image display object
        if(currentYipliConfig.gameType != GameType.MULTIPLAYER_GAMING)
        {
            if (!bIsProfilePicLoaded)
                yield return loadProfilePicAsync(profilePicImage, currentYipliConfig.playerInfo.profilePicUrl);
            playerNameText.text = "Hi, " + currentYipliConfig.playerInfo.playerName;
            playerNameText.gameObject.SetActive(true);
        }
        yield return new WaitForSecondsRealtime(0.00001f);
    }

    public void PlayerSelectionFlow()
    {
        Debug.Log("In Player selection flow.");

        // first of all destroy all PlayerButton prefabs. This is required to remove stale prefabs.
        if (generatedObjects.Count > 0)
        {
            Debug.Log("Destroying all the stale --PlayerName-- prefabs, before spawning new ones.");
            foreach (var obj1 in generatedObjects)
            {
                Destroy(obj1);
            }
        }

        if (currentYipliConfig.allPlayersInfo != null && currentYipliConfig.allPlayersInfo.Count != 0) //Atleast 1 player found for the corresponding userId
        {
            Debug.Log("Player/s found from firebase : " + currentYipliConfig.allPlayersInfo.Count);

            try
            {
                Quaternion spawnrotation = Quaternion.identity;
                Vector3 playerTilePosition = PlayersContainer.transform.localPosition;

                for (int i = 0; i < currentYipliConfig.allPlayersInfo.Count; i++)
                {
                    GameObject PlayerButton = Instantiate(PlayerButtonPrefab, playerTilePosition, spawnrotation) as GameObject;
                    generatedObjects.Add(PlayerButton);
                    PlayerButton.name = currentYipliConfig.allPlayersInfo[i].playerName;
                    PlayerButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = currentYipliConfig.allPlayersInfo[i].playerName;
                    PlayerButton.transform.SetParent(PlayersContainer.transform, false);
                    PlayerButton.transform.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(SelectPlayer);
                }
            }
            catch (Exception exp)
            {
                Debug.Log("Exception in Adding player : " + exp.Message);
                //Application.Quit();
            }
            TurnOffAllPanels();
            playerSelectionPanel.SetActive(true);
        }
        else
        {
            Debug.Log("No player found from firebase.");
            TurnOffAllPanels();
            zeroPlayersPanel.SetActive(true);
        }
    }

    public void SelectPlayer()
    {
        playerSelectionPanel.SetActive(false);
        FindObjectOfType<YipliAudioManager>().Play("ButtonClick");
        PlayerName = EventSystem.current.currentSelectedGameObject != null ? EventSystem.current.currentSelectedGameObject.name : FindObjectOfType<MatInputController>().GetCurrentButton().name;

        // first of all destroy all PlayerButton prefabs. This is required to remove stale prefabs.
        foreach (var obj1 in generatedObjects)
        {
            Destroy(obj1);
        }
        Debug.Log("Player Selected :  " + PlayerName);


        //Changing the currentSelected player in the Scriptable object
        //No Making this player persist in the device. This will be done on continue press.
        currentYipliConfig.playerInfo = GetPlayerInfoFromPlayerName(PlayerName);

        //Save the player to device
        UserDataPersistence.SavePlayerToDevice(currentYipliConfig.playerInfo);

        //Trigger the GetGameData for new player.
        DefaultPlayerChanged();

        StartCoroutine(ImageUploadAndPlayerUIInit());

        TurnOffAllPanels();
        switchPlayerPanel.SetActive(true);
    }

    private YipliPlayerInfo GetPlayerInfoFromPlayerName(string playerName)
    {
        if (currentYipliConfig.allPlayersInfo.Count > 0)
        {
            foreach (YipliPlayerInfo player in currentYipliConfig.allPlayersInfo)
            {
                Debug.Log("Found player : " + player.playerName);
                if (player.playerName == playerName)
                {
                    Debug.Log("Found player : " + player.playerName);
                    return player;
                }
            }
        }
        else
        {
            Debug.Log("No Players found.");
        }
        return null;
    }


    /* player selection is done. 
     * This function takes the flow to Mat connection */
    public void OnContinuePress()
    {
        Debug.Log("Continue Pressed.");
        TurnOffAllPanels();
        matSelectionScript.MatConnectionFlow();
    }

    public void OnJumpOnMat()
    {
        currentYipliConfig.bIsMatIntroDone = true;
        phoneHolderInfo.SetActive(false);
        StopCoroutine(ChangeTextMessage());
        SwitchPlayerFlow();
    }

    public void OnSwitchPlayerPress(bool isInternalCall = false /* If called internally that means no default player found.*/)
    {
        TurnOffAllPanels();

        if (!YipliHelper.checkInternetConnection())
        {
            //If default player also not available, then show no players panel.
            if (currentYipliConfig.playerInfo != null)
            {
                //noNetworkPanelText.text = "No active network connection. Cannot switch player. Press continue to play as " + defaultPlayer.playerName;
                noNetworkPanel.SetActive(true);
            }
            else
            {
                //Default player is not there.
                zeroPlayersPanel.SetActive(true);
            }
        }
        else//Active Network connection is available
        {
            //In case of internall call
            //This is to handle if the default player isn't set
            if (isInternalCall)
            {
                //This means no default player is present.Show all the players for selection.
                //Whichever gets selected, will become the default player later.
                //First check if the players count under userId is more than 0?
                if (currentYipliConfig.allPlayersInfo != null && currentYipliConfig.allPlayersInfo.Count > 0)
                {
                    Debug.Log("Calling the PlayerSelectionFlow");
                    PlayerSelectionFlow();
                }
                else // If No then throw a new panel to tell the Gamer that there is no player found
                {
                    TurnOffAllPanels();
                    zeroPlayersPanel.SetActive(true);
                }
            }
            else
            {
                //First check if the players count under userId is more than 1 ?
                if (currentYipliConfig.allPlayersInfo != null && currentYipliConfig.allPlayersInfo.Count > 1)
                {
                    PlayerSelectionFlow();
                }
                else // If No then throw a new panel to tell the Gamer that there is only 1 player currently
                {
                    TurnOffAllPanels();
                    onlyOnePlayerPanel.SetActive(true);
                }
            }
        }
    }

    public void OnGoToYipliPress()
    {
        YipliHelper.GoToYipli();
    }

    public void OnBackButtonPress()
    {
        TurnOffAllPanels();
        switchPlayerPanel.SetActive(true);
    }

    //public void setBluetoothEnabled()
    //{
    //    using (AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity"))
    //    {
    //        try
    //        {
    //            using (var BluetoothManager = activity.Call<AndroidJavaObject>("getSystemService", "bluetooth"))
    //            {
    //                using (var BluetoothAdapter = BluetoothManager.Call<AndroidJavaObject>("getAdapter"))
    //                {
    //                    BluetoothAdapter.Call("enable");
    //                }
    //            }
    //        }
    //        catch (Exception e)
    //        {
    //            Debug.Log(e);
    //            Debug.Log("could not enable the bluetooth automatically");
    //        }
    //    }
    //}

    async private Task loadProfilePicAsync(Image gameObj, string profilePicUrl)
    {
        if (profilePicUrl == null || profilePicUrl == "" || gameObj == null)
        {
            Debug.Log("Something went wrong. Returning.");
            //Set the profile pic to a default one.
            gameObj.sprite = defaultProfilePicSprite;
        }
        else
        {   
            // Create local filesystem URL
            string onDeviceProfilePicPath = Application.persistentDataPath + "/" + profilePicUrl;
            Sprite downloadedSprite = await FirebaseDBHandler.GetImageAsync(profilePicUrl, onDeviceProfilePicPath);
            if (downloadedSprite != null)
            {
                gameObj.sprite = downloadedSprite;
                bIsProfilePicLoaded = true;
            }
            else
            {
                //Actual profile pic in the backend
                gameObj.sprite = defaultProfilePicSprite;
            }
        }
        bIsProfilePicLoaded = false;
    }

#if UNITY_STANDALONE_WIN
    private void ReadFromWindowsFile()
    {
        currentYipliConfig.userId = FileReadWrite.ReadFromFile();
        currentYipliConfig.playerInfo = null;
    }
#endif
}

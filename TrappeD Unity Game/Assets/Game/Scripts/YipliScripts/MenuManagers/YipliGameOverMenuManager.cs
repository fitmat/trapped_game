﻿using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using YipliFMDriverCommunication;

public class YipliGameOverMenuManager : MonoBehaviour
{
    //required variables
    [SerializeField] Button[] menuButtons;
    Button currentB;
    int currentButtonIndex;

    [SerializeField] PlayerStats ps;

    const string LEFT = "left";
    const string RIGHT = "right";
    const string ENTER = "enter";

    int FMResponseCount = -1;

    // Start is called before the first frame update
    void Start()
    {
        MatControlsStatManager.gameStateChanged(GameState.GAME_OVER);

        /*
        PlayerPrefs.DeleteKey("IS_CHKP_REACHED");
        PlayerPrefs.DeleteKey("CHKP_X");
        PlayerPrefs.DeleteKey("CHKP_Y");
        PlayerPrefs.DeleteKey("CHKP_Z");
        */

        SetClusterIDtoZero();

        currentButtonIndex = 0;
        manageCurrentButton();
    }

    private static void SetClusterIDtoZero()
    {
        try
        {
            Debug.Log("From game over menu : Set cluster id to : 0");
            //YipliHelper.SetGameClusterId(0);
        }
        catch (Exception e)
        {
            Debug.Log("Game over menu cluster id 0 exception.");
            Debug.Log("Exception : " + e.Message);
        }
    }

    // Update is called once per frame
    void Update()
    {
        GetMatKeyInputs();
        MenuControlSystem();
    }

    private void manageCurrentButton()
    {
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (i == currentButtonIndex)
            {
                //menuButtons[i].GetComponent<Image>().color = Color.green;
                menuButtons[i].GetComponent<Animator>().enabled = true;
                menuButtons[i].transform.GetChild(0).gameObject.SetActive(true);
                currentB = menuButtons[i];
            }
            else
            {
                //menuButtons[i].GetComponent<Image>().color = Color.white;
                menuButtons[i].GetComponent<Animator>().enabled = false;
                menuButtons[i].transform.GetChild(0).gameObject.SetActive(false);
                menuButtons[i].transform.localScale = new Vector3(1, 1, 1);
            }
        }
    }

    private void GetMatKeyInputs()
    {
        // left to right play, changeplayer, gotoyipli, exit
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ProcessMatInputs("left");
        }

        // left to right play, changeplayer, gotoyipli, exit
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ProcessMatInputs("right");
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ProcessMatInputs("enter");
        }
    }

    private void MenuControlSystem()
    {
        string fmActionData = InitBLE.GetFMResponse();
        Debug.Log("Json Data from Fmdriver : " + fmActionData);

        if (fmActionData == null || fmActionData == "Make action") return;

        FmDriverResponseInfo singlePlayerResponse = JsonUtility.FromJson<FmDriverResponseInfo>(fmActionData);

        if (singlePlayerResponse == null) return;

        if (PlayerSession.Instance.currentYipliConfig.oldFMResponseCount != singlePlayerResponse.count)
        {
            //Debug.Log("FMResponse " + fmActionData);
            PlayerSession.Instance.currentYipliConfig.oldFMResponseCount = singlePlayerResponse.count;

            YipliUtils.PlayerActions providedAction = ActionAndGameInfoManager.GetActionEnumFromActionID(singlePlayerResponse.playerdata[0].fmresponse.action_id);

            switch (providedAction)
            {
                case YipliUtils.PlayerActions.LEFT:
                    ProcessMatInputs(LEFT);
                    break;

                case YipliUtils.PlayerActions.RIGHT:
                    ProcessMatInputs(RIGHT);
                    break;

                case YipliUtils.PlayerActions.ENTER:
                    ProcessMatInputs(ENTER);
                    break;
            }
        }
    }

    private int GetNextButton()
    {
        if ((currentButtonIndex + 1) == menuButtons.Length)
        {
            return 0;
        }
        else
        {
            return currentButtonIndex + 1;
        }
    }

    private int GetPreviousButton()
    {
        if (currentButtonIndex == 0)
        {
            return menuButtons.Length - 1;
        }
        else
        {
            return currentButtonIndex - 1;
        }
    }

    private void ProcessMatInputs(string matInput)
    {
        switch (matInput)
        {
            case LEFT:
                currentButtonIndex = GetPreviousButton();
                manageCurrentButton();
                break;

            case RIGHT:
                currentButtonIndex = GetNextButton();
                manageCurrentButton();
                break;

            case ENTER:
                currentB.GetComponent<Button>().onClick.Invoke();
                break;

            default:
                Debug.Log("Wrong Input");
                break;
        }
    }

    public void GotoMainMenu()
    {
        MatControlsStatManager.gameStateChanged(GameState.GAME_UI);
        StartCoroutine(FindObjectOfType<Transition>().FadeOutScene("Main Menu"));
    }

    public void GotoLevelSelect()
    {
        MatControlsStatManager.gameStateChanged(GameState.GAME_UI);
        StartCoroutine(FindObjectOfType<Transition>().FadeOutScene("Level_Selector"));
    }
}

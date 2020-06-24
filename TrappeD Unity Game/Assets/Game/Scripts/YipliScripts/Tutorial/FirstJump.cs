﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnitySampleAssets.CrossPlatformInput.PlatformSpecific;

public class FirstJump : MonoBehaviour
{
    // required variables
    [SerializeField] GameObject stopVideoScreen;
    [SerializeField] GameObject jumpVideoScreen;
    [SerializeField] Text AIText;
    [SerializeField] Text speakerT;

    [SerializeField] GameObject windowFrame;

    private BallController bc;
    public bool JTActive = false;

    private PauseGame pg;

    // Start is called before the first frame update
    void Start()
    {
        jumpVideoScreen.SetActive(false);
        windowFrame.SetActive(false);

        bc = FindObjectOfType<BallController>();
        pg = FindObjectOfType<PauseGame>();
    }

    private void Update()
    {
        if (JTActive)
        {
            JumpTutorial();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            stopVideoScreen.SetActive(false);
            jumpVideoScreen.SetActive(true);

            AIText.fontSize = 30;
            AIText.text = "You will be provided Text instructions through out the Game.";

            speakerT.text = "Stop and Jump on MAT to cross the hurdles";
            AudioControl.Instance.playAudio();

            StartCoroutine(frameAnimation());

            Time.timeScale = 0.01f;
            JTActive = true;
        }
    }

    private void JumpTutorial()
    {
        bc.allowjump = true;
        bc.allowRun = false;
        bc.allowStop = false;

        /* if (bc.detectedAction == PlayerSession.PlayerActions.JUMP)
        {
            Time.timeScale = 1f;
            bc.JumpAction();
        } */
    }

    IEnumerator frameAnimation()
    {
        for (int i = 0; i < 3; i++)
        {
            windowFrame.SetActive(true);
            yield return new WaitForSecondsRealtime(1f);

            windowFrame.SetActive(false);
            yield return new WaitForSecondsRealtime(1f);
        }

        AIText.text = "JUMP";
        AudioControl.Instance.playAudio();
        Time.timeScale = 0.1f;
    }
}
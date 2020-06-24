﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FirstCheckpoint : MonoBehaviour
{
    //required variables
    [SerializeField] GameObject checkpointFrame;

    [SerializeField] GameObject runVideoScreen;
    [SerializeField] GameObject stopVideoScreen;
    [SerializeField] GameObject jumpVideoScreen;
    [SerializeField] Text AIText;
    [SerializeField] Text speakerT;

    // Start is called before the first frame update
    void Start()
    {
        checkpointFrame.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            stopVideoScreen.SetActive(false);
            jumpVideoScreen.SetActive(false);
            runVideoScreen.SetActive(true);
            AIText.fontSize = 50;
            AIText.text = "Run, Stop or Jump";

            speakerT.text = "This is checkpoint. After death, level will start from here.";
            AudioControl.Instance.playAudio();

            Time.timeScale = 0.1f;
            StartCoroutine(frameAnimation());
        }
    }

    IEnumerator frameAnimation()
    {
        for (int i = 0; i < 5; i++)
        {
            checkpointFrame.SetActive(true);
            yield return new WaitForSecondsRealtime(1f);

            checkpointFrame.SetActive(false);
            yield return new WaitForSecondsRealtime(1f);
        }

        AudioControl.Instance.playAudio();
        speakerT.text = "Keep Running";
        Time.timeScale = 1f;
    }
}
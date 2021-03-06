﻿using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using UnitySampleAssets.CrossPlatformInput.PlatformSpecific;

public class FirstLife : MonoBehaviour
{
    //required variables
    [SerializeField] GameObject lifeOne;
    [SerializeField] GameObject lifeTwo;
    [SerializeField] GameObject lifeThree;
    [SerializeField] GameObject leftArrow;

    [SerializeField] Text speakerT;

    public bool tutDone = false;

    private BallController bc;
    private TutModelManager tmm;

    // Start is called before the first frame update
    void Start()
    {
        bc = FindObjectOfType<BallController>();
        tmm = FindObjectOfType<TutModelManager>();

        lifeOne.SetActive(false);
        lifeTwo.SetActive(false);
        lifeThree.SetActive(false);
        leftArrow.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //TriggerTutorial();
        }
    }

    public void TriggerTutorial()
    {
        tutDone = true;

        tmm.DeActivateModel();

        speakerT.text = "You have 3 lives in each level.";

        Time.timeScale = 0.1f;
        StartCoroutine(frameAnimation());
    }

    IEnumerator frameAnimation()
    {
        leftArrow.SetActive(true);

        lifeOne.SetActive(true);
        lifeTwo.SetActive(true);
        lifeThree.SetActive(true);

        for (int i = 0; i < 5; i++)
        {
            lifeOne.SetActive(false);
            lifeTwo.SetActive(false);
            lifeThree.SetActive(false);
            yield return new WaitForSecondsRealtime(0.25f);

            lifeOne.SetActive(true);
            lifeTwo.SetActive(true);
            lifeThree.SetActive(true);
            yield return new WaitForSecondsRealtime(0.25f);
        }

        speakerT.text = "Keep Running";
        leftArrow.SetActive(false);
        Time.timeScale = 1f;

        tmm.ActivateModel();
    }
}

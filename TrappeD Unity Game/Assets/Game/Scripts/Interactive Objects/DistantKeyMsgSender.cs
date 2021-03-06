﻿using UnityEngine;
using UnityEngine.SceneManagement;

public class DistantKeyMsgSender : MonoBehaviour {

	public GameObject theLock;
	public GameObject visualKey;
	private SlideUpOnKeyDown sk;
	private SpriteRenderer sr;

	private SpeakerManagerOne smo;

	// Use this for initialization
	void Start () {

		smo = FindObjectOfType<SpeakerManagerOne>();

		sk = theLock.GetComponent<SlideUpOnKeyDown> ();
		sr = visualKey.GetComponent<SpriteRenderer> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
		sk.SendMessage("UnlockYourself");
		sr.color = new Color(1.0f, 0, 0, 1.0f);

		smo.disableSpeaker();
	}

    /*void OnTriggerStay2D(Collider2D col){
		sk.SendMessage("UnlockYourself");
		sr.color = new Color (1.0f, 0,0,1.0f);
	}
	void OnTriggerExit2D(Collider2D col){
		sr.color = new Color (1.0f, 1.0f,1.0f,1.0f);
		sk.SendMessage("LockYourself");
	} */
}

﻿using UnityEngine;
using System.Collections;

public class SimpleCheckPoint : MonoBehaviour {

	public GameObject theCheckPointFlag;

	[SerializeField] PlayerStats ps;

	void OnTriggerEnter2D(Collider2D other){
		if (other.gameObject.tag == "Player" && gameObject.tag != "invisibleCP") {

			theCheckPointFlag.SendMessage("RiseUpTheFlag");

			/*
			PlayerPrefs.SetInt("IS_CHKP_REACHED", 1);
			PlayerPrefs.SetFloat("CHKP_X", transform.position.x);
			PlayerPrefs.SetFloat("CHKP_Y", transform.position.y);
			PlayerPrefs.SetFloat("CHKP_Z", transform.position.z);
			*/

			ps.CheckPointPos = gameObject.transform.position;
			ps.CheckPointPassed = true;
		}
	}
}

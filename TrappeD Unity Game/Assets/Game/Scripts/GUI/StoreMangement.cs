﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StoreMangement : MonoBehaviour {

	public int thisBallID;
	private int thisBallPrice;
	public Text ballPriceAndState;

	private GameObject balanceManager;

	public Canvas msgCanvas;
	public Text msgToMsgCanvas;

	public Canvas ballBuyConfirmationCanvas;

	[SerializeField] PlayerStats ps;

	private GameObject[] allBtn;
	// Use this for initialization
	void Start () 
	{
		//PlayerPrefs.SetInt ("BOUGHT_BALL_" + thisBallID.ToString (), 0);
		//PlayerPrefs.SetInt ("Coins", 750);

		thisBallPrice = ps.GetBallPrice(thisBallID);

		balanceManager = GameObject.Find ("BalanceManager");

		//PlayerPrefs.SetInt ("BOUGHT_BALL_0", 1);
		ps.SetBallBoughtStatus(0, true);

		updateThePriceAndStateText ();

		allBtn = GameObject.FindGameObjectsWithTag ("BALL_PRICE");
	}

	public void updateThePriceAndStateText()
	{
		if (isThisBallBoughtAlready () == false) {
			ballPriceAndState.text = thisBallPrice.ToString();
			gameObject.transform.parent.GetComponent<Animator>().enabled = false;
		} 
		else {
			if(thisBallID == ps.Active_ball){
				ballPriceAndState.text = "Active" ;
				ballPriceAndState.color = Color.gray;
				gameObject.transform.parent.GetComponent<Animator>().enabled = true;
			}
			else {
				ballPriceAndState.text = "Unlocked" ;
				ballPriceAndState.color = Color.blue;
				gameObject.transform.parent.GetComponent<Animator>().enabled = false;
			}
		}
	}

	public void manageBallBuying()
	{
		if (ps.Active_ball == thisBallID)
        {
			return;
        }

		if (isThisBallBoughtAlready () == true) {
			setThisBallAsActive ();
			msgCanvas.enabled = true;
			msgToMsgCanvas.text = "This Avatar has been set \n as active now.";
			//bool noBallYetShownAsActive = true;
			for(int i= 0; i<allBtn.Length; i++){
				allBtn[i].SendMessage("updateThePriceAndStateText");
				//if(allBtn.G)
			}
			//updateThePriceAndStateText ();

			FindObjectOfType<StoreMsgController>().UpdatePurchaseToDBAsync();
		} 
		else {

			if (thisBallPrice <= getAvailableBalance ()) {
				buyThisBall ();
				setThisBallAsActive ();
			}
			else {
				msgCanvas.enabled = true;
				msgToMsgCanvas.text = "Sorry, You don't have enough stars to buy this Avatar !!!";

			}
		}
	}



	private void setThisBallAsActive(){
		//PlayerPrefs.SetInt ("ACTIVE_BALL", thisBallID);
		ps.Active_ball = thisBallID;
	}

	private int getAvailableBalance(){
		//return PlayerPrefs.GetInt ("Coins");
		return ps.GetCoinScore();
	}

	private void buyThisBall(){
		ballBuyConfirmationCanvas.enabled = true;

		ps.Intended_ball_price = thisBallPrice;
		ps.Intended_ball_id = thisBallID;

		/*PlayerPrefs.SetInt ("INTENTED_BALL_PRICE",thisBallPrice);
		PlayerPrefs.SetInt ("INTENTED_BALL_ID",thisBallID);*/



		//PlayerPrefs.SetInt ("Coins", getAvailableBalance() - thisBallPrice);
		//PlayerPrefs.SetInt ("BOUGHT_BALL_" + thisBallID.ToString (), 1);

		//balanceManager.SendMessage ("updateBalance");
	}

	private bool isThisBallBoughtAlready(){
		return ps.GetBallBoughtStatus(thisBallID);
	}
}

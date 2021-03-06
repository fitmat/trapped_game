﻿using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PlayerStats/playerData")]
public class PlayerStats : ScriptableObject
{
    public string playerName;
    public int completed_levels;
    public int coinScore;

    private int intended_ball_price = -1;
    private int intended_ball_id = -1;
    public int active_ball = -1;

    public int timePlayed;
    public float calBurned;
    public int fpPoints;

    public int thisSessionTimePlayed;
    public float thisSessionCalBurned;
    public int thisSessionFpPoints;

    public string playerID;
    public string purchasedBalls;

    public List<StoreBalls> ballsInStore;

    public GameState gameState;

    public Vector3 checkPointPos;

    public bool checkPointPassed = false;

    public string[] tips;

    public int playerLives;

    public bool allowInput = false;

    public bool initialiseOldFmResponse = false;

    public bool isPreviousSceneTut = false;

    public bool isStartTextShown = false;

    public bool isTutorialMandatory = false;

    public int Intended_ball_price { get => intended_ball_price; set => intended_ball_price = value; }
    public int Intended_ball_id { get => intended_ball_id; set => intended_ball_id = value; }
    public int Active_ball { get => active_ball; set => active_ball = value; }
    public int TimePlayed { get => timePlayed; set => timePlayed = value; }
    public List<StoreBalls> BallsInStore { get => ballsInStore; set => ballsInStore = value; }
    public float CalBurned { get => calBurned; set => calBurned = value; }
    public int FpPoints { get => fpPoints; set => fpPoints = value; }
    public string PlayerID { get => playerID; set => playerID = value; }
    public string PurchasedBalls { get => purchasedBalls; set => purchasedBalls = value; }
    public int ThisSessionTimePlayed { get => thisSessionTimePlayed; set => thisSessionTimePlayed = value; }
    public float ThisSessionCalBurned { get => thisSessionCalBurned; set => thisSessionCalBurned = value; }
    public int ThisSessionFpPoints { get => thisSessionFpPoints; set => thisSessionFpPoints = value; }
    public GameState GameState { get => gameState; set => gameState = value; }
    public Vector3 CheckPointPos { get => checkPointPos; set => checkPointPos = value; }
    public string[] Tips { get => tips; set => tips = value; }
    public bool CheckPointPassed { get => checkPointPassed; set => checkPointPassed = value; }
    public int PlayerLives { get => playerLives; set => playerLives = value; }
    public bool AllowInput { get => allowInput; set => allowInput = value; }
    public bool InitialiseOldFmResponse { get => initialiseOldFmResponse; set => initialiseOldFmResponse = value; }
    public bool IsPreviousSceneTut { get => isPreviousSceneTut; set => isPreviousSceneTut = value; }
    public bool IsStartTextShown { get => isStartTextShown; set => isStartTextShown = value; }
    public bool IsTutorialMandatory { get => isTutorialMandatory; set => isTutorialMandatory = value; }

    public void SetListofBalls(List<int> purchasedIDs)
    {
        foreach(StoreBalls ball in BallsInStore)
        {
            if (purchasedIDs.Contains(ball.ballID))
            {
                ball.boughtStatus = true;
            }
            else
            {
                ball.boughtStatus = false;
            }
        }
    }

    public void SetDefaultStore()
    {
        foreach (StoreBalls ball in BallsInStore)
        {
            if (ball.ballID == 0)
            {
                ball.boughtStatus = true;
                Active_ball = 0;
            }
            else
            {
                ball.boughtStatus = false;
            }
        }
    }

    public void SetPlayerName(string pname)
    {
        playerName = pname;
    }

    public string GetPlayerName()
    {
        return playerName;
    }

    public void SetCompletedLevels(int CompletedLevels)
    {
        completed_levels = CompletedLevels;
    }

    public int GetCompletedLevels()
    {
        return completed_levels;
    }

    public void SetCoinScore(int cs)
    {
        coinScore = cs;
    }

    public int GetCoinScore()
    {
        return coinScore;
    }

    public int GetBallPrice(int idBall)
    {
        foreach (StoreBalls ball in BallsInStore)
        {
            if (ball.ballID == idBall)
            {
                return ball.ballPrice;
            }
        }
        return -5;
    }

    public bool GetBallBoughtStatus(int idBall)
    {
        foreach (StoreBalls ball in BallsInStore)
        {
            if (ball.ballID == idBall)
            {
                return ball.boughtStatus;
            }
        }
        return false;
    }

    public void SetBallBoughtStatus(int idBall, bool boughtStatus)
    {
        foreach (StoreBalls ball in BallsInStore)
        {
            if (ball.ballID == idBall)
            {
                ball.boughtStatus = boughtStatus;
            }
        }
    }

    [System.Serializable]
    public class StoreBalls
    {
        public int ballID;
        public int ballPrice;
        public bool boughtStatus;
    }
}
